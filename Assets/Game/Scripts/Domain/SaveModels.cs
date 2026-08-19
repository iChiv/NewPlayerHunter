using System;
using System.Collections.Generic;
using System.Linq;

namespace NewPlayerHunter.Domain
{
    public sealed class AssignmentSnapshot
    {
        public string DemandId;
        public string SlotId;
        public string PlayerId;
        public int Week;

        public static AssignmentSnapshot FromAssignment(Assignment assignment)
        {
            return new AssignmentSnapshot
            {
                DemandId = assignment.DemandId,
                SlotId = assignment.SlotId,
                PlayerId = assignment.PlayerId,
                Week = assignment.Week
            };
        }

        public Assignment ToAssignment()
        {
            return new Assignment(DemandId, SlotId, PlayerId, Week);
        }
    }

    public sealed class OutcomeSnapshot
    {
        public AssignmentSnapshot Assignment;
        public int OutcomeWeek;
        public double MatchScore;
        public double RandomRoll;
        public PlacementResultKind ResultKind;
        public decimal RewardAmount;
        public int ReputationDelta;
        public string NarrativeKey;

        public static OutcomeSnapshot FromOutcome(PlacementOutcome outcome)
        {
            return new OutcomeSnapshot
            {
                Assignment = AssignmentSnapshot.FromAssignment(outcome.Assignment),
                OutcomeWeek = outcome.OutcomeWeek,
                MatchScore = outcome.MatchScore,
                RandomRoll = outcome.RandomRoll,
                ResultKind = outcome.ResultKind,
                RewardAmount = outcome.RewardAmount,
                ReputationDelta = outcome.ReputationDelta,
                NarrativeKey = outcome.NarrativeKey
            };
        }

        public PlacementOutcome ToOutcome()
        {
            return new PlacementOutcome(
                Assignment.ToAssignment(),
                OutcomeWeek,
                MatchScore,
                RandomRoll,
                ResultKind,
                RewardAmount,
                ReputationDelta,
                NarrativeKey);
        }
    }

    public sealed class PaymentSnapshot
    {
        public string Id;
        public string DemandId;
        public string PlayerId;
        public decimal Amount;
        public int CreatedWeek;
        public int ExpectedWeek;
        public bool IsPaid;
        public int PaidWeek;

        public static PaymentSnapshot FromPayment(PendingPayment payment)
        {
            return new PaymentSnapshot
            {
                Id = payment.Id,
                DemandId = payment.DemandId,
                PlayerId = payment.PlayerId,
                Amount = payment.Amount,
                CreatedWeek = payment.CreatedWeek,
                ExpectedWeek = payment.ExpectedWeek,
                IsPaid = payment.IsPaid,
                PaidWeek = payment.PaidWeek ?? 0
            };
        }

        public PendingPayment ToPayment()
        {
            var payment = new PendingPayment(
                Id,
                DemandId,
                PlayerId,
                Amount,
                CreatedWeek,
                ExpectedWeek);
            if (IsPaid)
            {
                payment.RestorePaid(PaidWeek);
            }

            return payment;
        }
    }

    public sealed class WeekStateSnapshot
    {
        public int CurrentWeek;
        public decimal Cash;
        public int Reputation;
        public List<AssignmentSnapshot> CommittedAssignments = new List<AssignmentSnapshot>();
        public List<OutcomeSnapshot> PendingOutcomes = new List<OutcomeSnapshot>();
        public List<OutcomeSnapshot> DeliveredOutcomes = new List<OutcomeSnapshot>();
        public List<PaymentSnapshot> Payments = new List<PaymentSnapshot>();

        public static WeekStateSnapshot FromState(WeekState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return new WeekStateSnapshot
            {
                CurrentWeek = state.CurrentWeek,
                Cash = state.Cash,
                Reputation = state.Reputation,
                CommittedAssignments = state.CommittedAssignments
                    .Select(AssignmentSnapshot.FromAssignment)
                    .ToList(),
                PendingOutcomes = state.PendingOutcomes
                    .Select(OutcomeSnapshot.FromOutcome)
                    .ToList(),
                DeliveredOutcomes = state.DeliveredOutcomes
                    .Select(OutcomeSnapshot.FromOutcome)
                    .ToList(),
                Payments = state.Payments
                    .Select(PaymentSnapshot.FromPayment)
                    .ToList()
            };
        }
    }
}
