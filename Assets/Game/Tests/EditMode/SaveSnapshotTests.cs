using System;
using System.Linq;
using NUnit.Framework;

namespace NewPlayerHunter.Domain.Tests
{
    public sealed class SaveSnapshotTests
    {
        [Test]
        public void SnapshotRoundtrip_RestoresWeekEconomyAndSettlementQueues()
        {
            var state = CreateStateWithPlayers();
            var firstDemand = CreateSingleSlotDemand("demand.first", "slot.first");
            var secondDemand = CreateSingleSlotDemand("demand.second", "slot.second");
            state.AddDemand(firstDemand);
            state.AddDemand(secondDemand);
            var engine = new WeekEngine(seed: 77);

            var firstCommit = engine.CommitAssignments(
                state,
                SingleSubmission(state, firstDemand, "player.exact"));
            Assert.That(firstCommit.Validation.IsValid, Is.True);
            engine.AdvanceOneWeek(state);

            var secondCommit = engine.CommitAssignments(
                state,
                SingleSubmission(state, secondDemand, "player.wildcard"));
            Assert.That(secondCommit.Validation.IsValid, Is.True);

            var snapshot = state.CreateSnapshot();
            Assert.That(snapshot.PendingOutcomes, Is.Not.Empty,
                "The second commit must leave at least one pending outcome.");

            var restored = CreateStateWithPlayers();
            restored.AddDemand(CreateSingleSlotDemand("demand.first", "slot.first"));
            restored.AddDemand(CreateSingleSlotDemand("demand.second", "slot.second"));
            restored.ApplySnapshot(snapshot);

            AssertSnapshotsEqual(snapshot, restored.CreateSnapshot());
        }

        [Test]
        public void ApplySnapshot_RejectsNullOrInvalidSnapshots()
        {
            var state = CreateStateWithPlayers();

            Assert.Throws<ArgumentNullException>(() => state.ApplySnapshot(null));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                state.ApplySnapshot(new WeekStateSnapshot { CurrentWeek = 0, Cash = 10m }));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                state.ApplySnapshot(new WeekStateSnapshot { CurrentWeek = 1, Cash = -1m }));
        }

        [Test]
        public void SeededRandomSource_FastForwardRestoresSequencePosition()
        {
            var consumed = new SeededRandomSource(1234);
            consumed.NextInt(1, 3);
            consumed.NextDouble();
            consumed.NextDouble();
            Assert.That(consumed.DrawCount, Is.EqualTo(3));

            var replayed = new SeededRandomSource(1234);
            replayed.FastForward(consumed.DrawCount);
            Assert.That(replayed.DrawCount, Is.EqualTo(3));

            for (var index = 0; index < 5; index++)
            {
                Assert.That(replayed.NextDouble(), Is.EqualTo(consumed.NextDouble()));
            }

            Assert.Throws<ArgumentOutOfRangeException>(() => replayed.FastForward(-1));
        }

        private static void AssertSnapshotsEqual(WeekStateSnapshot expected, WeekStateSnapshot actual)
        {
            Assert.That(actual.CurrentWeek, Is.EqualTo(expected.CurrentWeek));
            Assert.That(actual.Cash, Is.EqualTo(expected.Cash));
            Assert.That(actual.Reputation, Is.EqualTo(expected.Reputation));
            Assert.That(
                actual.CommittedAssignments.Select(KeyOf),
                Is.EqualTo(expected.CommittedAssignments.Select(KeyOf)));
            Assert.That(
                actual.PendingOutcomes.Select(KeyOf),
                Is.EqualTo(expected.PendingOutcomes.Select(KeyOf)));
            Assert.That(
                actual.DeliveredOutcomes.Select(KeyOf),
                Is.EqualTo(expected.DeliveredOutcomes.Select(KeyOf)));
            Assert.That(
                actual.Payments.Select(KeyOf),
                Is.EqualTo(expected.Payments.Select(KeyOf)));
        }

        private static string KeyOf(AssignmentSnapshot assignment)
        {
            return $"{assignment.Week}:{assignment.DemandId}:{assignment.SlotId}:{assignment.PlayerId}";
        }

        private static string KeyOf(OutcomeSnapshot outcome)
        {
            return
                $"{KeyOf(outcome.Assignment)}->{outcome.OutcomeWeek}:{outcome.ResultKind}:{outcome.RewardAmount}:{outcome.ReputationDelta}:{outcome.MatchScore}:{outcome.RandomRoll}:{outcome.NarrativeKey}";
        }

        private static string KeyOf(PaymentSnapshot payment)
        {
            return
                $"{payment.Id}:{payment.Amount}:{payment.CreatedWeek}:{payment.ExpectedWeek}:{payment.IsPaid}:{payment.PaidWeek}";
        }

        private static WeekState CreateStateWithPlayers()
        {
            var state = new WeekState(currentWeek: 1, initialCash: 500m, initialReputation: 10);
            AddPlayer(state, "player.exact", 90, 90, 90);
            AddPlayer(state, "player.wildcard", 40, 50, 60);
            return state;
        }

        private static void AddPlayer(
            WeekState state,
            string id,
            int overallAbility,
            int fitness,
            int professionalism)
        {
            state.AddPlayer(
                new PlayerPublicProfile(
                    id,
                    "Test player",
                    "A deliberately unreliable public biography.",
                    new[] { PlayerPosition.Forward }),
                new PlayerTruth(
                    id,
                    overallAbility,
                    fitness,
                    professionalism,
                    new[] { PlayerPosition.Forward },
                    "Hidden test-only truth"));
        }

        private static ClubDemand CreateSingleSlotDemand(string demandId, string slotId)
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
                        PlayerPosition.Forward,
                        minimumAbility: 80,
                        preferredFitness: 80,
                        preferredProfessionalism: 80)
                });
        }

        private static AssignmentSubmission[] SingleSubmission(
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
    }
}
