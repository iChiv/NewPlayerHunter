using System;
using System.Globalization;
using System.Linq;
using NewPlayerHunter.Domain;

namespace NewPlayerHunter.Persistence
{
    public static class ProgressSnapshotMapper
    {
        public static WeekStateJson FromDomain(WeekStateSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            return new WeekStateJson
            {
                currentWeek = snapshot.CurrentWeek,
                cash = MoneyToString(snapshot.Cash),
                reputation = snapshot.Reputation,
                committedAssignments = snapshot.CommittedAssignments
                    .Select(FromDomain)
                    .ToList(),
                pendingOutcomes = snapshot.PendingOutcomes
                    .Select(FromDomain)
                    .ToList(),
                deliveredOutcomes = snapshot.DeliveredOutcomes
                    .Select(FromDomain)
                    .ToList(),
                payments = snapshot.Payments
                    .Select(FromDomain)
                    .ToList()
            };
        }

        public static WeekStateSnapshot ToDomain(WeekStateJson json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            return new WeekStateSnapshot
            {
                CurrentWeek = json.currentWeek,
                Cash = MoneyFromString(json.cash),
                Reputation = json.reputation,
                CommittedAssignments = (json.committedAssignments ?? Enumerable.Empty<AssignmentJson>())
                    .Select(ToDomain)
                    .ToList(),
                PendingOutcomes = (json.pendingOutcomes ?? Enumerable.Empty<OutcomeJson>())
                    .Select(ToDomain)
                    .ToList(),
                DeliveredOutcomes = (json.deliveredOutcomes ?? Enumerable.Empty<OutcomeJson>())
                    .Select(ToDomain)
                    .ToList(),
                Payments = (json.payments ?? Enumerable.Empty<PaymentJson>())
                    .Select(ToDomain)
                    .ToList()
            };
        }

        private static AssignmentJson FromDomain(AssignmentSnapshot snapshot)
        {
            return new AssignmentJson
            {
                demandId = snapshot.DemandId,
                slotId = snapshot.SlotId,
                playerId = snapshot.PlayerId,
                week = snapshot.Week
            };
        }

        private static AssignmentSnapshot ToDomain(AssignmentJson json)
        {
            return new AssignmentSnapshot
            {
                DemandId = json.demandId,
                SlotId = json.slotId,
                PlayerId = json.playerId,
                Week = json.week
            };
        }

        private static OutcomeJson FromDomain(OutcomeSnapshot snapshot)
        {
            return new OutcomeJson
            {
                assignment = FromDomain(snapshot.Assignment),
                outcomeWeek = snapshot.OutcomeWeek,
                matchScore = snapshot.MatchScore,
                randomRoll = snapshot.RandomRoll,
                resultKind = (int)snapshot.ResultKind,
                rewardAmount = MoneyToString(snapshot.RewardAmount),
                reputationDelta = snapshot.ReputationDelta,
                narrativeKey = snapshot.NarrativeKey
            };
        }

        private static OutcomeSnapshot ToDomain(OutcomeJson json)
        {
            return new OutcomeSnapshot
            {
                Assignment = ToDomain(json.assignment),
                OutcomeWeek = json.outcomeWeek,
                MatchScore = json.matchScore,
                RandomRoll = json.randomRoll,
                ResultKind = (PlacementResultKind)json.resultKind,
                RewardAmount = MoneyFromString(json.rewardAmount),
                ReputationDelta = json.reputationDelta,
                NarrativeKey = json.narrativeKey
            };
        }

        private static PaymentJson FromDomain(PaymentSnapshot snapshot)
        {
            return new PaymentJson
            {
                id = snapshot.Id,
                demandId = snapshot.DemandId,
                playerId = snapshot.PlayerId,
                amount = MoneyToString(snapshot.Amount),
                createdWeek = snapshot.CreatedWeek,
                expectedWeek = snapshot.ExpectedWeek,
                isPaid = snapshot.IsPaid,
                paidWeek = snapshot.PaidWeek
            };
        }

        private static PaymentSnapshot ToDomain(PaymentJson json)
        {
            return new PaymentSnapshot
            {
                Id = json.id,
                DemandId = json.demandId,
                PlayerId = json.playerId,
                Amount = MoneyFromString(json.amount),
                CreatedWeek = json.createdWeek,
                ExpectedWeek = json.expectedWeek,
                IsPaid = json.isPaid,
                PaidWeek = json.paidWeek
            };
        }

        private static string MoneyToString(decimal amount)
        {
            return amount.ToString(CultureInfo.InvariantCulture);
        }

        private static decimal MoneyFromString(string amount)
        {
            if (string.IsNullOrEmpty(amount))
            {
                return 0m;
            }

            return decimal.Parse(amount, CultureInfo.InvariantCulture);
        }
    }
}
