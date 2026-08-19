using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NewPlayerHunter.Domain
{
    public enum SubmissionErrorCode
    {
        NoSubmissions,
        SubmissionHasNoAssignments,
        UnknownDemand,
        DemandNotOpen,
        DemandExpired,
        AssignmentDemandMismatch,
        AssignmentWeekMismatch,
        UnknownPlayer,
        UnknownSlot,
        DuplicateDemandSubmission,
        DuplicateSlot,
        SlotAlreadyAssigned,
        DuplicatePlayerInWeek,
        PlayerAlreadyCommitted,
        UnconfirmedEmptyRequiredSlots
    }

    public sealed class SubmissionError
    {
        public SubmissionError(
            SubmissionErrorCode code,
            string message,
            string demandId = null,
            string slotId = null,
            string playerId = null)
        {
            Code = code;
            Message = DomainGuard.RequiredText(message, nameof(message));
            DemandId = demandId;
            SlotId = slotId;
            PlayerId = playerId;
        }

        public SubmissionErrorCode Code { get; }

        public string Message { get; }

        public string DemandId { get; }

        public string SlotId { get; }

        public string PlayerId { get; }
    }

    public sealed class SubmissionValidationResult
    {
        public SubmissionValidationResult(IEnumerable<SubmissionError> errors)
        {
            Errors = DomainGuard.ReadOnlyList(errors, nameof(errors));
        }

        public bool IsValid => Errors.Count == 0;

        public IReadOnlyList<SubmissionError> Errors { get; }
    }

    public enum PlacementResultKind
    {
        Rejected,
        TrialExtended,
        Accepted
    }

    public sealed class PlacementOutcome
    {
        public PlacementOutcome(
            Assignment assignment,
            int outcomeWeek,
            double matchScore,
            double randomRoll,
            PlacementResultKind resultKind,
            decimal rewardAmount,
            int reputationDelta,
            string narrativeKey)
        {
            Assignment = assignment ?? throw new ArgumentNullException(nameof(assignment));
            if (outcomeWeek <= assignment.Week)
            {
                throw new ArgumentOutOfRangeException(nameof(outcomeWeek));
            }

            if (matchScore < 0d || matchScore > 1d)
            {
                throw new ArgumentOutOfRangeException(nameof(matchScore));
            }

            if (randomRoll < 0d || randomRoll >= 1d)
            {
                throw new ArgumentOutOfRangeException(nameof(randomRoll));
            }

            if (rewardAmount < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(rewardAmount));
            }

            OutcomeWeek = outcomeWeek;
            MatchScore = matchScore;
            RandomRoll = randomRoll;
            ResultKind = resultKind;
            RewardAmount = rewardAmount;
            ReputationDelta = reputationDelta;
            NarrativeKey = DomainGuard.StableId(narrativeKey, nameof(narrativeKey));
        }

        public Assignment Assignment { get; }

        public int OutcomeWeek { get; }

        public double MatchScore { get; }

        public double RandomRoll { get; }

        public PlacementResultKind ResultKind { get; }

        public decimal RewardAmount { get; }

        public int ReputationDelta { get; }

        public string NarrativeKey { get; }
    }

    public sealed class PendingPayment
    {
        public PendingPayment(
            string id,
            string demandId,
            string playerId,
            decimal amount,
            int createdWeek,
            int expectedWeek)
        {
            if (amount < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (createdWeek < 1 || expectedWeek <= createdWeek)
            {
                throw new ArgumentOutOfRangeException(nameof(expectedWeek));
            }

            Id = DomainGuard.StableId(id, nameof(id));
            DemandId = DomainGuard.StableId(demandId, nameof(demandId));
            PlayerId = DomainGuard.StableId(playerId, nameof(playerId));
            Amount = amount;
            CreatedWeek = createdWeek;
            ExpectedWeek = expectedWeek;
        }

        public string Id { get; }

        public string DemandId { get; }

        public string PlayerId { get; }

        public decimal Amount { get; }

        public int CreatedWeek { get; }

        public int ExpectedWeek { get; }

        public bool IsPaid { get; private set; }

        public int? PaidWeek { get; private set; }

        internal void MarkPaid(int week)
        {
            if (IsPaid)
            {
                throw new InvalidOperationException($"Payment {Id} is already paid.");
            }

            if (week < ExpectedWeek)
            {
                throw new InvalidOperationException($"Payment {Id} is not due until week {ExpectedWeek}.");
            }

            IsPaid = true;
            PaidWeek = week;
        }

        internal void RestorePaid(int week)
        {
            if (week < ExpectedWeek)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(week),
                    $"Payment {Id} cannot have been paid before week {ExpectedWeek}.");
            }

            IsPaid = true;
            PaidWeek = week;
        }
    }

    public sealed class WeekRules
    {
        public WeekRules(
            bool allowMultipleAssignmentsPerPlayerPerWeek = false,
            bool allowPlayerReassignmentAfterCommit = false,
            int minimumSettlementDelayWeeks = 1,
            int maximumSettlementDelayWeeks = 2,
            double outOfPositionScore = 0.35d)
        {
            if (minimumSettlementDelayWeeks < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumSettlementDelayWeeks));
            }

            if (maximumSettlementDelayWeeks < minimumSettlementDelayWeeks)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumSettlementDelayWeeks));
            }

            if (outOfPositionScore < 0d || outOfPositionScore > 1d)
            {
                throw new ArgumentOutOfRangeException(nameof(outOfPositionScore));
            }

            AllowMultipleAssignmentsPerPlayerPerWeek = allowMultipleAssignmentsPerPlayerPerWeek;
            AllowPlayerReassignmentAfterCommit = allowPlayerReassignmentAfterCommit;
            MinimumSettlementDelayWeeks = minimumSettlementDelayWeeks;
            MaximumSettlementDelayWeeks = maximumSettlementDelayWeeks;
            OutOfPositionScore = outOfPositionScore;
        }

        public bool AllowMultipleAssignmentsPerPlayerPerWeek { get; }

        public bool AllowPlayerReassignmentAfterCommit { get; }

        public int MinimumSettlementDelayWeeks { get; }

        public int MaximumSettlementDelayWeeks { get; }

        public double OutOfPositionScore { get; }
    }

    public sealed class WeekState
    {
        private readonly Dictionary<string, PlayerPublicProfile> _players =
            new Dictionary<string, PlayerPublicProfile>(StringComparer.Ordinal);
        private readonly Dictionary<string, PlayerTruth> _truths =
            new Dictionary<string, PlayerTruth>(StringComparer.Ordinal);
        private readonly Dictionary<string, ClubDemand> _demands =
            new Dictionary<string, ClubDemand>(StringComparer.Ordinal);
        private readonly List<Assignment> _committedAssignments = new List<Assignment>();
        private readonly List<PlacementOutcome> _pendingOutcomes = new List<PlacementOutcome>();
        private readonly List<PlacementOutcome> _deliveredOutcomes = new List<PlacementOutcome>();
        private readonly List<PendingPayment> _payments = new List<PendingPayment>();

        public WeekState(int currentWeek = 1, decimal initialCash = 0m, int initialReputation = 0)
        {
            if (currentWeek < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(currentWeek));
            }

            if (initialCash < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCash));
            }

            CurrentWeek = currentWeek;
            Cash = initialCash;
            Reputation = initialReputation;
        }

        public int CurrentWeek { get; private set; }

        public decimal Cash { get; private set; }

        public int Reputation { get; private set; }

        public decimal OutstandingReceivables => _payments.Where(payment => !payment.IsPaid).Sum(payment => payment.Amount);

        public IReadOnlyList<PlayerPublicProfile> PublicPlayers =>
            new ReadOnlyCollection<PlayerPublicProfile>(_players.Values.ToList());

        public IReadOnlyList<ClubDemand> Demands =>
            new ReadOnlyCollection<ClubDemand>(_demands.Values.ToList());

        public IReadOnlyList<Assignment> CommittedAssignments =>
            new ReadOnlyCollection<Assignment>(_committedAssignments);

        public IReadOnlyList<PlacementOutcome> PendingOutcomes =>
            new ReadOnlyCollection<PlacementOutcome>(_pendingOutcomes);

        public IReadOnlyList<PlacementOutcome> DeliveredOutcomes =>
            new ReadOnlyCollection<PlacementOutcome>(_deliveredOutcomes);

        public IReadOnlyList<PendingPayment> Payments =>
            new ReadOnlyCollection<PendingPayment>(_payments);

        public void AddPlayer(PlayerPublicProfile publicProfile, PlayerTruth truth)
        {
            if (publicProfile == null)
            {
                throw new ArgumentNullException(nameof(publicProfile));
            }

            if (truth == null)
            {
                throw new ArgumentNullException(nameof(truth));
            }

            if (!string.Equals(publicProfile.Id, truth.PlayerId, StringComparison.Ordinal))
            {
                throw new ArgumentException("Public profile ID and truth player ID must match.", nameof(truth));
            }

            if (_players.ContainsKey(publicProfile.Id))
            {
                throw new ArgumentException($"Duplicate player ID: {publicProfile.Id}", nameof(publicProfile));
            }

            _players.Add(publicProfile.Id, publicProfile);
            _truths.Add(truth.PlayerId, truth);
        }

        public void AddDemand(ClubDemand demand)
        {
            if (demand == null)
            {
                throw new ArgumentNullException(nameof(demand));
            }

            if (_demands.ContainsKey(demand.Id))
            {
                throw new ArgumentException($"Duplicate demand ID: {demand.Id}", nameof(demand));
            }

            _demands.Add(demand.Id, demand);
        }

        public void ApplyImmediateIncome(decimal amount)
        {
            if (amount <= 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            Cash += amount;
        }

        public void ApplyReputationChange(int delta)
        {
            if (delta == 0)
            {
                return;
            }

            Reputation += delta;
        }

        internal bool TryGetDemand(string demandId, out ClubDemand demand)
        {
            return _demands.TryGetValue(demandId, out demand);
        }

        internal bool HasPlayer(string playerId)
        {
            return _players.ContainsKey(playerId);
        }

        internal PlayerTruth GetTruth(string playerId)
        {
            return _truths[playerId];
        }

        internal void AddCommittedAssignment(Assignment assignment)
        {
            _committedAssignments.Add(assignment);
        }

        internal void AddPendingOutcome(PlacementOutcome outcome)
        {
            _pendingOutcomes.Add(outcome);
        }

        internal void AddPayment(PendingPayment payment)
        {
            _payments.Add(payment);
        }

        internal void AdvanceWeek()
        {
            CurrentWeek++;
        }

        public WeekStateSnapshot CreateSnapshot()
        {
            return WeekStateSnapshot.FromState(this);
        }

        public void ApplySnapshot(WeekStateSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (snapshot.CurrentWeek < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(snapshot), "Snapshot week must be at least 1.");
            }

            if (snapshot.Cash < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(snapshot), "Snapshot cash cannot be negative.");
            }

            CurrentWeek = snapshot.CurrentWeek;
            Cash = snapshot.Cash;
            Reputation = snapshot.Reputation;

            _committedAssignments.Clear();
            _pendingOutcomes.Clear();
            _deliveredOutcomes.Clear();
            _payments.Clear();

            foreach (var assignment in snapshot.CommittedAssignments)
            {
                _committedAssignments.Add(assignment.ToAssignment());
            }

            foreach (var outcome in snapshot.PendingOutcomes)
            {
                _pendingOutcomes.Add(outcome.ToOutcome());
            }

            foreach (var outcome in snapshot.DeliveredOutcomes)
            {
                _deliveredOutcomes.Add(outcome.ToOutcome());
            }

            foreach (var payment in snapshot.Payments)
            {
                _payments.Add(payment.ToPayment());
            }
        }

        internal IReadOnlyList<PlacementOutcome> DeliverDueOutcomes()
        {
            var due = _pendingOutcomes
                .Where(outcome => outcome.OutcomeWeek <= CurrentWeek)
                .ToList();

            foreach (var outcome in due)
            {
                _pendingOutcomes.Remove(outcome);
                _deliveredOutcomes.Add(outcome);
                Reputation += outcome.ReputationDelta;
            }

            return new ReadOnlyCollection<PlacementOutcome>(due);
        }

        internal IReadOnlyList<PendingPayment> PayDueReceivables()
        {
            var due = _payments
                .Where(payment => !payment.IsPaid && payment.ExpectedWeek <= CurrentWeek)
                .ToList();

            foreach (var payment in due)
            {
                payment.MarkPaid(CurrentWeek);
                Cash += payment.Amount;
            }

            return new ReadOnlyCollection<PendingPayment>(due);
        }
    }

    public sealed class CommitResult
    {
        public CommitResult(
            SubmissionValidationResult validation,
            IEnumerable<PlacementOutcome> scheduledOutcomes,
            IEnumerable<PendingPayment> scheduledPayments)
        {
            Validation = validation ?? throw new ArgumentNullException(nameof(validation));
            ScheduledOutcomes = DomainGuard.ReadOnlyList(scheduledOutcomes, nameof(scheduledOutcomes));
            ScheduledPayments = DomainGuard.ReadOnlyList(scheduledPayments, nameof(scheduledPayments));
        }

        public SubmissionValidationResult Validation { get; }

        public IReadOnlyList<PlacementOutcome> ScheduledOutcomes { get; }

        public IReadOnlyList<PendingPayment> ScheduledPayments { get; }
    }

    public sealed class AdvanceWeekResult
    {
        public AdvanceWeekResult(
            int week,
            IEnumerable<PlacementOutcome> deliveredOutcomes,
            IEnumerable<PendingPayment> paidPayments)
        {
            Week = week;
            DeliveredOutcomes = DomainGuard.ReadOnlyList(deliveredOutcomes, nameof(deliveredOutcomes));
            PaidPayments = DomainGuard.ReadOnlyList(paidPayments, nameof(paidPayments));
        }

        public int Week { get; }

        public IReadOnlyList<PlacementOutcome> DeliveredOutcomes { get; }

        public IReadOnlyList<PendingPayment> PaidPayments { get; }
    }
}
