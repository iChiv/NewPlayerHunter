using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace NewPlayerHunter.Domain.Tests
{
    public sealed class WeekEngineTests
    {
        [Test]
        public void MultiSlotSubmission_RequiresExplicitPartialConfirmation()
        {
            var state = CreateStateWithPlayers();
            var demand = CreateTwoSlotDemand("demand.multi");
            state.AddDemand(demand);
            var engine = new WeekEngine(seed: 17);
            var assignment = new Assignment(demand.Id, "slot.forward", "player.exact", state.CurrentWeek);

            var unconfirmed = engine.ValidateAssignments(
                state,
                new[]
                {
                    new AssignmentSubmission(demand.Id, new[] { assignment })
                });

            Assert.That(unconfirmed.IsValid, Is.False);
            Assert.That(
                unconfirmed.Errors.Select(error => error.Code),
                Does.Contain(SubmissionErrorCode.UnconfirmedEmptyRequiredSlots));

            var confirmed = engine.ValidateAssignments(
                state,
                new[]
                {
                    new AssignmentSubmission(
                        demand.Id,
                        new[] { assignment },
                        confirmedEmptyRequiredSlots: true)
                });

            Assert.That(confirmed.IsValid, Is.True);
        }

        [Test]
        public void DuplicatePlayerInSameWeek_IsRejectedByDefault_AndCanBeEnabledByRule()
        {
            var defaultState = CreateStateWithPlayers();
            var firstDemand = CreateSingleSlotDemand("demand.first", "slot.first", PlayerPosition.Forward);
            var secondDemand = CreateSingleSlotDemand("demand.second", "slot.second", PlayerPosition.Forward);
            defaultState.AddDemand(firstDemand);
            defaultState.AddDemand(secondDemand);
            var submissions = CreateDuplicatePlayerSubmissions(defaultState.CurrentWeek, firstDemand, secondDemand);

            var defaultValidation = new WeekEngine(seed: 3).ValidateAssignments(defaultState, submissions);

            Assert.That(defaultValidation.IsValid, Is.False);
            Assert.That(
                defaultValidation.Errors.Select(error => error.Code),
                Does.Contain(SubmissionErrorCode.DuplicatePlayerInWeek));

            var permissiveState = CreateStateWithPlayers();
            permissiveState.AddDemand(firstDemand);
            permissiveState.AddDemand(secondDemand);
            var permissiveEngine = new WeekEngine(
                new WeekRules(allowMultipleAssignmentsPerPlayerPerWeek: true),
                new SeededRandomSource(3));

            var permissiveValidation = permissiveEngine.ValidateAssignments(
                permissiveState,
                CreateDuplicatePlayerSubmissions(permissiveState.CurrentWeek, firstDemand, secondDemand));

            Assert.That(permissiveValidation.IsValid, Is.True);
        }

        [Test]
        public void CommittedPlayer_IsRejectedInLaterWeeks_UnlessRuleExplicitlyAllowsReturn()
        {
            var state = CreateStateWithPlayers();
            var firstDemand = CreateSingleSlotDemand(
                "demand.first-week",
                "slot.first-week",
                PlayerPosition.Forward);
            var laterDemand = CreateSingleSlotDemand(
                "demand.later-week",
                "slot.later-week",
                PlayerPosition.Forward);
            state.AddDemand(firstDemand);
            state.AddDemand(laterDemand);
            var engine = new WeekEngine(seed: 41);

            var firstCommit = engine.CommitAssignments(
                state,
                SingleSubmission(state, firstDemand, "player.exact"));
            Assert.That(firstCommit.Validation.IsValid, Is.True);
            engine.AdvanceOneWeek(state);

            var laterValidation = engine.ValidateAssignments(
                state,
                SingleSubmission(state, laterDemand, "player.exact"));
            Assert.That(laterValidation.IsValid, Is.False);
            Assert.That(
                laterValidation.Errors.Select(error => error.Code),
                Does.Contain(SubmissionErrorCode.PlayerAlreadyCommitted));

            var permissiveEngine = new WeekEngine(
                new WeekRules(allowPlayerReassignmentAfterCommit: true),
                new SeededRandomSource(41));
            Assert.That(
                permissiveEngine.ValidateAssignments(
                    state,
                    SingleSubmission(state, laterDemand, "player.exact")).IsValid,
                Is.True);
        }

        [Test]
        public void SeasonCalendar_AdvancesBySevenDays_AndEndsAfterFiftyTwoWeeks()
        {
            var calendar = new SeasonCalendar(new DateTime(2026, 7, 6));

            Assert.That(calendar.DateForWeek(1), Is.EqualTo(new DateTime(2026, 7, 6)));
            Assert.That(calendar.DateForWeek(52), Is.EqualTo(new DateTime(2027, 6, 28)));
            Assert.That(calendar.DateAfterFinalWeek, Is.EqualTo(new DateTime(2027, 7, 5)));
            Assert.That(calendar.PhaseForWeek(1), Is.EqualTo(SeasonPhase.Preseason));
            Assert.That(calendar.PhaseForWeek(29), Is.EqualTo(SeasonPhase.WinterWindow));
            Assert.That(calendar.PhaseForWeek(52), Is.EqualTo(SeasonPhase.SeasonReview));
            Assert.Throws<ArgumentOutOfRangeException>(() => calendar.DateForWeek(53));
        }

        [Test]
        public void PartialMatch_DiscountsRewardWithoutBlockingSubmission()
        {
            var exactState = CreateStateWithPlayers();
            var exactDemand = CreateSingleSlotDemand("demand.reward", "slot.reward", PlayerPosition.Forward);
            exactState.AddDemand(exactDemand);
            var exactCommit = new WeekEngine(seed: 11).CommitAssignments(
                exactState,
                SingleSubmission(exactState, exactDemand, "player.exact"));

            var partialState = CreateStateWithPlayers();
            var partialDemand = CreateSingleSlotDemand("demand.reward", "slot.reward", PlayerPosition.Forward);
            partialState.AddDemand(partialDemand);
            var partialCommit = new WeekEngine(seed: 11).CommitAssignments(
                partialState,
                SingleSubmission(partialState, partialDemand, "player.partial"));

            Assert.That(exactCommit.Validation.IsValid, Is.True);
            Assert.That(partialCommit.Validation.IsValid, Is.True);
            Assert.That(exactCommit.ScheduledPayments.Single().Amount, Is.EqualTo(1000m));
            Assert.That(partialCommit.ScheduledPayments.Single().Amount, Is.GreaterThan(0m));
            Assert.That(
                partialCommit.ScheduledPayments.Single().Amount,
                Is.LessThan(exactCommit.ScheduledPayments.Single().Amount));
        }

        [Test]
        public void Settlement_IsDelayedOneOrTwoWeeks_ThenMovesReceivableIntoCash()
        {
            var state = CreateStateWithPlayers(initialCash: 125m);
            var demand = CreateSingleSlotDemand("demand.delay", "slot.delay", PlayerPosition.Forward);
            state.AddDemand(demand);
            var engine = new WeekEngine(seed: 29);

            var commit = engine.CommitAssignments(state, SingleSubmission(state, demand, "player.exact"));
            var payment = commit.ScheduledPayments.Single();
            var startingCash = state.Cash;

            Assert.That(commit.Validation.IsValid, Is.True);
            Assert.That(payment.ExpectedWeek - payment.CreatedWeek, Is.InRange(1, 2));
            Assert.That(state.OutstandingReceivables, Is.EqualTo(payment.Amount));
            Assert.That(state.Cash, Is.EqualTo(startingCash));

            while (state.CurrentWeek + 1 < payment.ExpectedWeek)
            {
                var earlyWeek = engine.AdvanceOneWeek(state);
                Assert.That(earlyWeek.DeliveredOutcomes, Is.Empty);
                Assert.That(earlyWeek.PaidPayments, Is.Empty);
            }

            var dueWeek = engine.AdvanceOneWeek(state);

            Assert.That(dueWeek.Week, Is.EqualTo(payment.ExpectedWeek));
            Assert.That(dueWeek.DeliveredOutcomes, Has.Count.EqualTo(1));
            Assert.That(dueWeek.PaidPayments, Has.Count.EqualTo(1));
            Assert.That(payment.IsPaid, Is.True);
            Assert.That(state.OutstandingReceivables, Is.Zero);
            Assert.That(state.Cash, Is.EqualTo(startingCash + payment.Amount));
        }

        [Test]
        public void SameSeed_ReproducesDelayRollResultAndNarrative()
        {
            var firstState = CreateStateWithPlayers();
            var firstDemand = CreateSingleSlotDemand("demand.seed", "slot.seed", PlayerPosition.Forward);
            firstState.AddDemand(firstDemand);
            var first = new WeekEngine(seed: 20260810)
                .CommitAssignments(firstState, SingleSubmission(firstState, firstDemand, "player.exact"))
                .ScheduledOutcomes.Single();

            var secondState = CreateStateWithPlayers();
            var secondDemand = CreateSingleSlotDemand("demand.seed", "slot.seed", PlayerPosition.Forward);
            secondState.AddDemand(secondDemand);
            var second = new WeekEngine(seed: 20260810)
                .CommitAssignments(secondState, SingleSubmission(secondState, secondDemand, "player.exact"))
                .ScheduledOutcomes.Single();

            Assert.That(second.OutcomeWeek, Is.EqualTo(first.OutcomeWeek));
            Assert.That(second.RandomRoll, Is.EqualTo(first.RandomRoll));
            Assert.That(second.ResultKind, Is.EqualTo(first.ResultKind));
            Assert.That(second.NarrativeKey, Is.EqualTo(first.NarrativeKey));
            Assert.That(second.RewardAmount, Is.EqualTo(first.RewardAmount));
        }

        [Test]
        public void PublicViewModelBoundary_HasNoPlayerTruthInputOrOutput()
        {
            var profile = CreatePublicProfile("player.boundary", "Visible Nickname");
            var truth = new PlayerTruth(
                profile.Id,
                overallAbility: 97,
                fitness: 88,
                professionalism: 12,
                effectivePositions: new[] { PlayerPosition.Forward },
                hiddenNotes: "SECRET_DISCIPLINE_PROBLEM");
            var claims = new[]
            {
                new PlayerClaim(
                    "claim.boundary",
                    profile.Id,
                    "source.agent",
                    1,
                    "His agent says he never misses training.")
            };
            var evidence = new[]
            {
                new EvidenceItem(
                    "evidence.boundary",
                    profile.Id,
                    "source.paper",
                    1,
                    EvidenceReliability.Medium,
                    "Local favourite",
                    "A match report praises his movement.")
            };

            var viewModel = PlayerPublicViewModelFactory.Create(profile, claims, evidence);
            var publicTypes = typeof(PlayerPublicViewModel)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => property.PropertyType)
                .ToList();
            var factoryParameterTypes = typeof(PlayerPublicViewModelFactory)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .SelectMany(method => method.GetParameters())
                .Select(parameter => parameter.ParameterType)
                .ToList();

            Assert.That(publicTypes.Any(type => type == typeof(PlayerTruth)), Is.False);
            Assert.That(factoryParameterTypes.Any(type => type == typeof(PlayerTruth)), Is.False);
            Assert.That(viewModel.DisplayName, Is.EqualTo("Visible Nickname"));
            Assert.That(viewModel.Claims.Single().Text, Does.Not.Contain(truth.HiddenNotes));
            Assert.That(viewModel.Evidence.Single().Summary, Does.Not.Contain(truth.HiddenNotes));
        }

        [Test]
        public void FixedDataSixWeekSimulation_ResolvesAllScheduledOutcomesAndPayments()
        {
            var state = CreateStateWithPlayers(initialCash: 50m);
            var demand = CreateTwoSlotDemand("demand.six-week");
            state.AddDemand(demand);
            var engine = new WeekEngine(seed: 606);
            var submissions = new[]
            {
                new AssignmentSubmission(
                    demand.Id,
                    new[]
                    {
                        new Assignment(demand.Id, "slot.forward", "player.exact", state.CurrentWeek),
                        new Assignment(demand.Id, "slot.midfield", "player.midfielder", state.CurrentWeek)
                    })
            };

            var commit = engine.CommitAssignments(state, submissions);
            Assert.That(commit.Validation.IsValid, Is.True);
            Assert.That(commit.ScheduledOutcomes, Has.Count.EqualTo(2));

            for (var week = 0; week < 6; week++)
            {
                engine.AdvanceOneWeek(state);
            }

            Assert.That(state.CurrentWeek, Is.EqualTo(7));
            Assert.That(state.PendingOutcomes, Is.Empty);
            Assert.That(state.DeliveredOutcomes, Has.Count.EqualTo(2));
            Assert.That(state.Payments.All(payment => payment.IsPaid), Is.True);
            Assert.That(state.OutstandingReceivables, Is.Zero);
            Assert.That(state.Cash, Is.GreaterThan(50m));
        }

        [Test]
        public void ExternalNarrativeEvent_CanApplyImmediateIncomeAndDelayedReputation()
        {
            var state = new WeekState(initialCash: 500m, initialReputation: 10);

            state.ApplyImmediateIncome(350m);
            Assert.That(state.Cash, Is.EqualTo(850m));

            state.ApplyReputationChange(-3);
            Assert.That(state.Reputation, Is.EqualTo(7));
            Assert.Throws<System.ArgumentOutOfRangeException>(
                () => state.ApplyImmediateIncome(0m));
        }

        private static WeekState CreateStateWithPlayers(decimal initialCash = 0m)
        {
            var state = new WeekState(initialCash: initialCash, initialReputation: 10);
            AddPlayer(
                state,
                "player.exact",
                "Exact Eddie",
                PlayerPosition.Forward,
                overallAbility: 90,
                fitness: 90,
                professionalism: 90);
            AddPlayer(
                state,
                "player.partial",
                "Maybe Marco",
                PlayerPosition.Defender,
                overallAbility: 40,
                fitness: 45,
                professionalism: 55);
            AddPlayer(
                state,
                "player.midfielder",
                "Middle Mika",
                PlayerPosition.Midfielder,
                overallAbility: 82,
                fitness: 85,
                professionalism: 88);
            return state;
        }

        private static void AddPlayer(
            WeekState state,
            string id,
            string name,
            PlayerPosition position,
            int overallAbility,
            int fitness,
            int professionalism)
        {
            state.AddPlayer(
                CreatePublicProfile(id, name),
                new PlayerTruth(
                    id,
                    overallAbility,
                    fitness,
                    professionalism,
                    new[] { position },
                    "Hidden test-only truth"));
        }

        private static PlayerPublicProfile CreatePublicProfile(string id, string name)
        {
            return new PlayerPublicProfile(
                id,
                name,
                "A deliberately unreliable public biography.",
                new[] { PlayerPosition.Forward });
        }

        private static ClubDemand CreateSingleSlotDemand(
            string demandId,
            string slotId,
            PlayerPosition position)
        {
            return new ClubDemand(
                demandId,
                "club.test",
                "Test recruitment",
                openedWeek: 1,
                activeWeeks: 3,
                baseReward: 1000m,
                slots: new[]
                {
                    new DemandSlot(
                        slotId,
                        position,
                        minimumAbility: 80,
                        preferredFitness: 80,
                        preferredProfessionalism: 80)
                });
        }

        private static ClubDemand CreateTwoSlotDemand(string demandId)
        {
            return new ClubDemand(
                demandId,
                "club.multi",
                "Two-player trial",
                openedWeek: 1,
                activeWeeks: 3,
                baseReward: 2000m,
                slots: new[]
                {
                    new DemandSlot(
                        "slot.forward",
                        PlayerPosition.Forward,
                        minimumAbility: 75,
                        preferredFitness: 75,
                        preferredProfessionalism: 70),
                    new DemandSlot(
                        "slot.midfield",
                        PlayerPosition.Midfielder,
                        minimumAbility: 75,
                        preferredFitness: 70,
                        preferredProfessionalism: 75)
                });
        }

        private static IReadOnlyList<AssignmentSubmission> SingleSubmission(
            WeekState state,
            ClubDemand demand,
            string playerId)
        {
            return new[]
            {
                new AssignmentSubmission(
                    demand.Id,
                    new[]
                    {
                        new Assignment(demand.Id, demand.Slots[0].Id, playerId, state.CurrentWeek)
                    })
            };
        }

        private static IReadOnlyList<AssignmentSubmission> CreateDuplicatePlayerSubmissions(
            int week,
            ClubDemand firstDemand,
            ClubDemand secondDemand)
        {
            return new[]
            {
                new AssignmentSubmission(
                    firstDemand.Id,
                    new[]
                    {
                        new Assignment(firstDemand.Id, firstDemand.Slots[0].Id, "player.exact", week)
                    }),
                new AssignmentSubmission(
                    secondDemand.Id,
                    new[]
                    {
                        new Assignment(secondDemand.Id, secondDemand.Slots[0].Id, "player.exact", week)
                    })
            };
        }
    }
}
