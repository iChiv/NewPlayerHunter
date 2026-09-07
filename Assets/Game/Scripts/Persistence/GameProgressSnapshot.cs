using System;
using System.Collections.Generic;

namespace NewPlayerHunter.Persistence
{
    [Serializable]
    public sealed class AssignmentJson
    {
        public string demandId;
        public string slotId;
        public string playerId;
        public int week;
    }

    [Serializable]
    public sealed class OutcomeJson
    {
        public AssignmentJson assignment;
        public int outcomeWeek;
        public double matchScore;
        public double randomRoll;
        public int resultKind;
        public string rewardAmount;
        public int reputationDelta;
        public string narrativeKey;
    }

    [Serializable]
    public sealed class PaymentJson
    {
        public string id;
        public string demandId;
        public string playerId;
        public string amount;
        public int createdWeek;
        public int expectedWeek;
        public bool isPaid;
        public int paidWeek;
    }

    [Serializable]
    public sealed class WeekStateJson
    {
        public int currentWeek;
        public string cash;
        public int reputation;
        public List<AssignmentJson> committedAssignments = new List<AssignmentJson>();
        public List<OutcomeJson> pendingOutcomes = new List<OutcomeJson>();
        public List<OutcomeJson> deliveredOutcomes = new List<OutcomeJson>();
        public List<PaymentJson> payments = new List<PaymentJson>();
    }

    [Serializable]
    public sealed class GameProgressSnapshot
    {
        public int schemaVersion;
        public WeekStateJson weekState;
        public List<string> readMailIds = new List<string>();
        public List<string> unlockedPlayerIds = new List<string>();
        public List<string> unlockedDemandIds = new List<string>();
        public string selectedDemandId;
        public bool carloFavorAccepted;
        public bool carloFavorConsequenceApplied;
        public List<string> eventLog = new List<string>();
        public string lastStatus;
        public int randomDrawCount;
    }
}
