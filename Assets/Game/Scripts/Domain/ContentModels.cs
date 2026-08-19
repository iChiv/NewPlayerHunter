using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NewPlayerHunter.Domain
{
    internal static class DomainGuard
    {
        public static string StableId(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Stable IDs cannot be empty.", parameterName);
            }

            return value.Trim();
        }

        public static string RequiredText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Text cannot be empty.", parameterName);
            }

            return value.Trim();
        }

        public static int Rating(int value, string parameterName)
        {
            if (value < 0 || value > 100)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Ratings must be between 0 and 100.");
            }

            return value;
        }

        public static ReadOnlyCollection<T> ReadOnlyList<T>(IEnumerable<T> values, string parameterName)
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return new ReadOnlyCollection<T>(values.ToList());
        }
    }

    public enum PlayerPosition
    {
        Goalkeeper,
        Defender,
        WingBack,
        Midfielder,
        Winger,
        Forward
    }

    public enum EvidenceReliability
    {
        Unverified,
        Low,
        Medium,
        High
    }

    public sealed class PlayerPublicProfile
    {
        public PlayerPublicProfile(
            string id,
            string displayName,
            string biography,
            IEnumerable<PlayerPosition> claimedPositions)
        {
            Id = DomainGuard.StableId(id, nameof(id));
            DisplayName = DomainGuard.RequiredText(displayName, nameof(displayName));
            Biography = biography ?? string.Empty;
            ClaimedPositions = DomainGuard.ReadOnlyList(
                (claimedPositions ?? throw new ArgumentNullException(nameof(claimedPositions)))
                    .Distinct(),
                nameof(claimedPositions));
        }

        public string Id { get; }

        public string DisplayName { get; }

        public string Biography { get; }

        public IReadOnlyList<PlayerPosition> ClaimedPositions { get; }
    }

    public sealed class PlayerTruth
    {
        public PlayerTruth(
            string playerId,
            int overallAbility,
            int fitness,
            int professionalism,
            IEnumerable<PlayerPosition> effectivePositions,
            string hiddenNotes)
        {
            PlayerId = DomainGuard.StableId(playerId, nameof(playerId));
            OverallAbility = DomainGuard.Rating(overallAbility, nameof(overallAbility));
            Fitness = DomainGuard.Rating(fitness, nameof(fitness));
            Professionalism = DomainGuard.Rating(professionalism, nameof(professionalism));
            EffectivePositions = DomainGuard.ReadOnlyList(
                (effectivePositions ?? throw new ArgumentNullException(nameof(effectivePositions)))
                    .Distinct(),
                nameof(effectivePositions));
            HiddenNotes = hiddenNotes ?? string.Empty;

            if (EffectivePositions.Count == 0)
            {
                throw new ArgumentException("A player truth requires at least one effective position.", nameof(effectivePositions));
            }
        }

        public string PlayerId { get; }

        public int OverallAbility { get; }

        public int Fitness { get; }

        public int Professionalism { get; }

        public IReadOnlyList<PlayerPosition> EffectivePositions { get; }

        public string HiddenNotes { get; }
    }

    public sealed class PlayerClaim
    {
        public PlayerClaim(string id, string playerId, string sourceId, int publishedWeek, string text)
        {
            if (publishedWeek < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(publishedWeek));
            }

            Id = DomainGuard.StableId(id, nameof(id));
            PlayerId = DomainGuard.StableId(playerId, nameof(playerId));
            SourceId = DomainGuard.StableId(sourceId, nameof(sourceId));
            PublishedWeek = publishedWeek;
            Text = DomainGuard.RequiredText(text, nameof(text));
        }

        public string Id { get; }

        public string PlayerId { get; }

        public string SourceId { get; }

        public int PublishedWeek { get; }

        public string Text { get; }
    }

    public sealed class EvidenceItem
    {
        public EvidenceItem(
            string id,
            string playerId,
            string sourceId,
            int publishedWeek,
            EvidenceReliability reliability,
            string biasLabel,
            string summary)
        {
            if (publishedWeek < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(publishedWeek));
            }

            Id = DomainGuard.StableId(id, nameof(id));
            PlayerId = DomainGuard.StableId(playerId, nameof(playerId));
            SourceId = DomainGuard.StableId(sourceId, nameof(sourceId));
            PublishedWeek = publishedWeek;
            Reliability = reliability;
            BiasLabel = biasLabel ?? string.Empty;
            Summary = DomainGuard.RequiredText(summary, nameof(summary));
        }

        public string Id { get; }

        public string PlayerId { get; }

        public string SourceId { get; }

        public int PublishedWeek { get; }

        public EvidenceReliability Reliability { get; }

        public string BiasLabel { get; }

        public string Summary { get; }
    }

    public sealed class DemandSlot
    {
        public DemandSlot(
            string id,
            PlayerPosition requiredPosition,
            int minimumAbility,
            int preferredFitness,
            int preferredProfessionalism,
            bool isRequired = true)
        {
            Id = DomainGuard.StableId(id, nameof(id));
            RequiredPosition = requiredPosition;
            MinimumAbility = DomainGuard.Rating(minimumAbility, nameof(minimumAbility));
            PreferredFitness = DomainGuard.Rating(preferredFitness, nameof(preferredFitness));
            PreferredProfessionalism = DomainGuard.Rating(
                preferredProfessionalism,
                nameof(preferredProfessionalism));
            IsRequired = isRequired;
        }

        public string Id { get; }

        public PlayerPosition RequiredPosition { get; }

        public int MinimumAbility { get; }

        public int PreferredFitness { get; }

        public int PreferredProfessionalism { get; }

        public bool IsRequired { get; }
    }

    public sealed class ClubDemand
    {
        private readonly Dictionary<string, DemandSlot> _slotsById;

        public ClubDemand(
            string id,
            string clubId,
            string title,
            int openedWeek,
            int activeWeeks,
            decimal baseReward,
            IEnumerable<DemandSlot> slots)
        {
            if (openedWeek < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(openedWeek));
            }

            if (activeWeeks < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(activeWeeks));
            }

            if (baseReward < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(baseReward));
            }

            Id = DomainGuard.StableId(id, nameof(id));
            ClubId = DomainGuard.StableId(clubId, nameof(clubId));
            Title = DomainGuard.RequiredText(title, nameof(title));
            OpenedWeek = openedWeek;
            ActiveWeeks = activeWeeks;
            BaseReward = baseReward;
            Slots = DomainGuard.ReadOnlyList(slots, nameof(slots));

            if (Slots.Count == 0)
            {
                throw new ArgumentException("A club demand requires at least one slot.", nameof(slots));
            }

            _slotsById = new Dictionary<string, DemandSlot>(StringComparer.Ordinal);
            foreach (var slot in Slots)
            {
                if (slot == null)
                {
                    throw new ArgumentException("Demand slots cannot contain null.", nameof(slots));
                }

                if (!_slotsById.TryAdd(slot.Id, slot))
                {
                    throw new ArgumentException($"Duplicate demand slot ID: {slot.Id}", nameof(slots));
                }
            }
        }

        public string Id { get; }

        public string ClubId { get; }

        public string Title { get; }

        public int OpenedWeek { get; }

        public int ActiveWeeks { get; }

        public int DeadlineWeek => OpenedWeek + ActiveWeeks - 1;

        public decimal BaseReward { get; }

        public IReadOnlyList<DemandSlot> Slots { get; }

        public bool TryGetSlot(string slotId, out DemandSlot slot)
        {
            return _slotsById.TryGetValue(slotId, out slot);
        }
    }

    public sealed class Assignment
    {
        public Assignment(string demandId, string slotId, string playerId, int week)
        {
            if (week < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(week));
            }

            DemandId = DomainGuard.StableId(demandId, nameof(demandId));
            SlotId = DomainGuard.StableId(slotId, nameof(slotId));
            PlayerId = DomainGuard.StableId(playerId, nameof(playerId));
            Week = week;
        }

        public string DemandId { get; }

        public string SlotId { get; }

        public string PlayerId { get; }

        public int Week { get; }
    }

    public sealed class AssignmentSubmission
    {
        public AssignmentSubmission(
            string demandId,
            IEnumerable<Assignment> assignments,
            bool confirmedEmptyRequiredSlots = false)
        {
            DemandId = DomainGuard.StableId(demandId, nameof(demandId));
            Assignments = DomainGuard.ReadOnlyList(assignments, nameof(assignments));
            ConfirmedEmptyRequiredSlots = confirmedEmptyRequiredSlots;
        }

        public string DemandId { get; }

        public IReadOnlyList<Assignment> Assignments { get; }

        public bool ConfirmedEmptyRequiredSlots { get; }
    }
}
