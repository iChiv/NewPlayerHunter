using System;
using System.Collections.Generic;
using System.Linq;

namespace NewPlayerHunter.Domain
{
    public interface IRandomSource
    {
        int NextInt(int minimumInclusive, int maximumExclusive);

        double NextDouble();
    }

    public sealed class SeededRandomSource : IRandomSource
    {
        private readonly Random _random;

        public SeededRandomSource(int seed)
        {
            Seed = seed;
            _random = new Random(seed);
        }

        public int Seed { get; }

        public int DrawCount { get; private set; }

        public int NextInt(int minimumInclusive, int maximumExclusive)
        {
            DrawCount++;
            return _random.Next(minimumInclusive, maximumExclusive);
        }

        public double NextDouble()
        {
            DrawCount++;
            return _random.NextDouble();
        }

        // NextInt and NextDouble each consume exactly one underlying sample,
        // so replaying that many samples restores the sequence position.
        public void FastForward(int drawCount)
        {
            if (drawCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(drawCount));
            }

            for (var index = 0; index < drawCount; index++)
            {
                _random.NextDouble();
            }

            DrawCount += drawCount;
        }
    }

    public static class PlacementEvaluator
    {
        public static double CalculateMatchScore(PlayerTruth truth, DemandSlot slot, WeekRules rules)
        {
            if (truth == null)
            {
                throw new ArgumentNullException(nameof(truth));
            }

            if (slot == null)
            {
                throw new ArgumentNullException(nameof(slot));
            }

            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            var positionScore = truth.EffectivePositions.Contains(slot.RequiredPosition)
                ? 1d
                : rules.OutOfPositionScore;
            var abilityScore = RatioToTarget(truth.OverallAbility, slot.MinimumAbility);
            var fitnessScore = RatioToTarget(truth.Fitness, slot.PreferredFitness);
            var professionalismScore = RatioToTarget(
                truth.Professionalism,
                slot.PreferredProfessionalism);

            var score =
                (positionScore * 0.45d) +
                (abilityScore * 0.30d) +
                (fitnessScore * 0.15d) +
                (professionalismScore * 0.10d);

            return Math.Max(0d, Math.Min(1d, score));
        }

        public static decimal CalculateAssignmentReward(ClubDemand demand, double matchScore)
        {
            if (demand == null)
            {
                throw new ArgumentNullException(nameof(demand));
            }

            if (matchScore < 0d || matchScore > 1d)
            {
                throw new ArgumentOutOfRangeException(nameof(matchScore));
            }

            var slotShare = demand.BaseReward / demand.Slots.Count;
            return decimal.Round(
                slotShare * (decimal)matchScore,
                2,
                MidpointRounding.AwayFromZero);
        }

        private static double RatioToTarget(int actual, int target)
        {
            if (target <= 0)
            {
                return 1d;
            }

            return Math.Max(0d, Math.Min(1d, actual / (double)target));
        }
    }

    public sealed class WeekEngine
    {
        private readonly WeekRules _rules;
        private readonly IRandomSource _random;

        public WeekEngine(WeekRules rules, IRandomSource random)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public WeekEngine(int seed)
            : this(new WeekRules(), new SeededRandomSource(seed))
        {
        }

        public SubmissionValidationResult ValidateAssignments(
            WeekState state,
            IEnumerable<AssignmentSubmission> submissions)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (submissions == null)
            {
                throw new ArgumentNullException(nameof(submissions));
            }

            var submissionList = submissions.ToList();
            var errors = new List<SubmissionError>();
            if (submissionList.Count == 0)
            {
                errors.Add(new SubmissionError(
                    SubmissionErrorCode.NoSubmissions,
                    "At least one demand submission is required."));
                return new SubmissionValidationResult(errors);
            }

            var seenDemandIds = new HashSet<string>(StringComparer.Ordinal);
            var committedPlayerIds = new HashSet<string>(
                state.CommittedAssignments.Select(assignment => assignment.PlayerId),
                StringComparer.Ordinal);
            var assignedPlayerIds = new HashSet<string>(
                state.CommittedAssignments
                    .Where(assignment => assignment.Week == state.CurrentWeek)
                    .Select(assignment => assignment.PlayerId),
                StringComparer.Ordinal);
            var occupiedSlots = new HashSet<string>(
                state.CommittedAssignments
                    .Where(assignment => assignment.Week == state.CurrentWeek)
                    .Select(assignment => SlotKey(assignment.DemandId, assignment.SlotId)),
                StringComparer.Ordinal);

            foreach (var submission in submissionList)
            {
                if (submission == null)
                {
                    errors.Add(new SubmissionError(
                        SubmissionErrorCode.SubmissionHasNoAssignments,
                        "Submissions cannot contain null entries."));
                    continue;
                }

                if (!seenDemandIds.Add(submission.DemandId))
                {
                    errors.Add(new SubmissionError(
                        SubmissionErrorCode.DuplicateDemandSubmission,
                        $"Demand {submission.DemandId} was submitted more than once.",
                        submission.DemandId));
                }

                if (!state.TryGetDemand(submission.DemandId, out var demand))
                {
                    errors.Add(new SubmissionError(
                        SubmissionErrorCode.UnknownDemand,
                        $"Unknown demand {submission.DemandId}.",
                        submission.DemandId));
                    continue;
                }

                if (state.CurrentWeek < demand.OpenedWeek)
                {
                    errors.Add(new SubmissionError(
                        SubmissionErrorCode.DemandNotOpen,
                        $"Demand {demand.Id} does not open until week {demand.OpenedWeek}.",
                        demand.Id));
                }

                if (state.CurrentWeek > demand.DeadlineWeek)
                {
                    errors.Add(new SubmissionError(
                        SubmissionErrorCode.DemandExpired,
                        $"Demand {demand.Id} expired after week {demand.DeadlineWeek}.",
                        demand.Id));
                }

                if (submission.Assignments.Count == 0)
                {
                    errors.Add(new SubmissionError(
                        SubmissionErrorCode.SubmissionHasNoAssignments,
                        $"Demand {demand.Id} has no assignments.",
                        demand.Id));
                }

                var assignedSlotIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (var assignment in submission.Assignments)
                {
                    if (assignment == null)
                    {
                        errors.Add(new SubmissionError(
                            SubmissionErrorCode.SubmissionHasNoAssignments,
                            $"Demand {demand.Id} contains a null assignment.",
                            demand.Id));
                        continue;
                    }

                    if (!string.Equals(assignment.DemandId, submission.DemandId, StringComparison.Ordinal))
                    {
                        errors.Add(new SubmissionError(
                            SubmissionErrorCode.AssignmentDemandMismatch,
                            $"Assignment demand {assignment.DemandId} does not match submission {submission.DemandId}.",
                            submission.DemandId,
                            assignment.SlotId,
                            assignment.PlayerId));
                    }

                    if (assignment.Week != state.CurrentWeek)
                    {
                        errors.Add(new SubmissionError(
                            SubmissionErrorCode.AssignmentWeekMismatch,
                            $"Assignment week {assignment.Week} does not match current week {state.CurrentWeek}.",
                            submission.DemandId,
                            assignment.SlotId,
                            assignment.PlayerId));
                    }

                    if (!state.HasPlayer(assignment.PlayerId))
                    {
                        errors.Add(new SubmissionError(
                            SubmissionErrorCode.UnknownPlayer,
                            $"Unknown player {assignment.PlayerId}.",
                            submission.DemandId,
                            assignment.SlotId,
                            assignment.PlayerId));
                    }
                    else if (!_rules.AllowPlayerReassignmentAfterCommit &&
                             committedPlayerIds.Contains(assignment.PlayerId))
                    {
                        errors.Add(new SubmissionError(
                            SubmissionErrorCode.PlayerAlreadyCommitted,
                            $"Player {assignment.PlayerId} already has a committed placement.",
                            submission.DemandId,
                            assignment.SlotId,
                            assignment.PlayerId));
                    }
                    else if (!_rules.AllowMultipleAssignmentsPerPlayerPerWeek &&
                             !assignedPlayerIds.Add(assignment.PlayerId))
                    {
                        errors.Add(new SubmissionError(
                            SubmissionErrorCode.DuplicatePlayerInWeek,
                            $"Player {assignment.PlayerId} is already assigned in week {state.CurrentWeek}.",
                            submission.DemandId,
                            assignment.SlotId,
                            assignment.PlayerId));
                    }

                    if (!demand.TryGetSlot(assignment.SlotId, out _))
                    {
                        errors.Add(new SubmissionError(
                            SubmissionErrorCode.UnknownSlot,
                            $"Unknown slot {assignment.SlotId} on demand {demand.Id}.",
                            demand.Id,
                            assignment.SlotId,
                            assignment.PlayerId));
                        continue;
                    }

                    if (!assignedSlotIds.Add(assignment.SlotId))
                    {
                        errors.Add(new SubmissionError(
                            SubmissionErrorCode.DuplicateSlot,
                            $"Slot {assignment.SlotId} has more than one player.",
                            demand.Id,
                            assignment.SlotId,
                            assignment.PlayerId));
                    }

                    if (!occupiedSlots.Add(SlotKey(demand.Id, assignment.SlotId)))
                    {
                        errors.Add(new SubmissionError(
                            SubmissionErrorCode.SlotAlreadyAssigned,
                            $"Slot {assignment.SlotId} was already committed this week.",
                            demand.Id,
                            assignment.SlotId,
                            assignment.PlayerId));
                    }
                }

                var hasEmptyRequiredSlot = demand.Slots.Any(
                    slot => slot.IsRequired && !assignedSlotIds.Contains(slot.Id));
                if (hasEmptyRequiredSlot && !submission.ConfirmedEmptyRequiredSlots)
                {
                    errors.Add(new SubmissionError(
                        SubmissionErrorCode.UnconfirmedEmptyRequiredSlots,
                        $"Demand {demand.Id} has empty required slots that were not confirmed.",
                        demand.Id));
                }
            }

            return new SubmissionValidationResult(errors);
        }

        public CommitResult CommitAssignments(
            WeekState state,
            IEnumerable<AssignmentSubmission> submissions)
        {
            if (submissions == null)
            {
                throw new ArgumentNullException(nameof(submissions));
            }

            var submissionList = submissions.ToList();
            var validation = ValidateAssignments(state, submissionList);
            if (!validation.IsValid)
            {
                return new CommitResult(
                    validation,
                    Array.Empty<PlacementOutcome>(),
                    Array.Empty<PendingPayment>());
            }

            var outcomes = new List<PlacementOutcome>();
            var payments = new List<PendingPayment>();

            foreach (var assignment in submissionList.SelectMany(item => item.Assignments))
            {
                state.TryGetDemand(assignment.DemandId, out var demand);
                demand.TryGetSlot(assignment.SlotId, out var slot);
                var truth = state.GetTruth(assignment.PlayerId);
                var matchScore = PlacementEvaluator.CalculateMatchScore(truth, slot, _rules);
                var reward = PlacementEvaluator.CalculateAssignmentReward(demand, matchScore);
                var delay = _random.NextInt(
                    _rules.MinimumSettlementDelayWeeks,
                    _rules.MaximumSettlementDelayWeeks + 1);
                var outcomeWeek = state.CurrentWeek + delay;
                var randomRoll = _random.NextDouble();
                var resultKind = DetermineResultKind(matchScore, randomRoll);
                var reputationDelta = resultKind == PlacementResultKind.Accepted
                    ? 2
                    : resultKind == PlacementResultKind.Rejected
                        ? -1
                        : 0;
                var narrativeKey = DetermineNarrativeKey(resultKind, matchScore, randomRoll, truth);

                var outcome = new PlacementOutcome(
                    assignment,
                    outcomeWeek,
                    matchScore,
                    randomRoll,
                    resultKind,
                    reward,
                    reputationDelta,
                    narrativeKey);
                var payment = new PendingPayment(
                    PaymentId(assignment),
                    assignment.DemandId,
                    assignment.PlayerId,
                    reward,
                    state.CurrentWeek,
                    outcomeWeek);

                state.AddCommittedAssignment(assignment);
                state.AddPendingOutcome(outcome);
                state.AddPayment(payment);
                outcomes.Add(outcome);
                payments.Add(payment);
            }

            return new CommitResult(validation, outcomes, payments);
        }

        public AdvanceWeekResult AdvanceOneWeek(WeekState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            state.AdvanceWeek();
            var outcomes = state.DeliverDueOutcomes();
            var payments = state.PayDueReceivables();
            return new AdvanceWeekResult(state.CurrentWeek, outcomes, payments);
        }

        private static PlacementResultKind DetermineResultKind(double matchScore, double randomRoll)
        {
            var acceptanceChance = 0.15d + (matchScore * 0.75d);
            if (matchScore >= 0.70d && randomRoll < acceptanceChance)
            {
                return PlacementResultKind.Accepted;
            }

            if (matchScore >= 0.40d || randomRoll < acceptanceChance * 0.75d)
            {
                return PlacementResultKind.TrialExtended;
            }

            return PlacementResultKind.Rejected;
        }

        internal static string DetermineNarrativeKey(
            PlacementResultKind kind,
            double matchScore,
            double randomRoll,
            PlayerTruth truth)
        {
            if (kind == PlacementResultKind.Accepted)
            {
                return matchScore < 0.80d
                    ? "placement.accepted.surprise"
                    : "placement.accepted";
            }

            if (kind == PlacementResultKind.TrialExtended)
            {
                if (truth.Professionalism < 45 && Fraction(randomRoll * 13d) < 0.5d)
                {
                    return "placement.trial_extended.personality_conflict";
                }

                return truth.Fitness < 40
                    ? "placement.trial_extended.fitness_doubt"
                    : "placement.trial_extended";
            }

            return truth.Fitness < 40 && Fraction(randomRoll * 7d) < 0.5d
                ? "placement.rejected.hidden_injury"
                : "placement.rejected";
        }

        private static double Fraction(double value)
        {
            return value - Math.Floor(value);
        }

        private static string SlotKey(string demandId, string slotId)
        {
            return demandId + ":" + slotId;
        }

        private static string PaymentId(Assignment assignment)
        {
            return $"payment:{assignment.Week}:{assignment.DemandId}:{assignment.SlotId}:{assignment.PlayerId}";
        }
    }
}
