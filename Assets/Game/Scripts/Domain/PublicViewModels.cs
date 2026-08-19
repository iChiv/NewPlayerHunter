using System;
using System.Collections.Generic;
using System.Linq;

namespace NewPlayerHunter.Domain
{
    public sealed class PublicClaimViewModel
    {
        public PublicClaimViewModel(string sourceId, int publishedWeek, string text)
        {
            SourceId = DomainGuard.StableId(sourceId, nameof(sourceId));
            PublishedWeek = publishedWeek;
            Text = DomainGuard.RequiredText(text, nameof(text));
        }

        public string SourceId { get; }

        public int PublishedWeek { get; }

        public string Text { get; }
    }

    public sealed class PublicEvidenceViewModel
    {
        public PublicEvidenceViewModel(
            string sourceId,
            int publishedWeek,
            EvidenceReliability reliability,
            string biasLabel,
            string summary)
        {
            SourceId = DomainGuard.StableId(sourceId, nameof(sourceId));
            PublishedWeek = publishedWeek;
            Reliability = reliability;
            BiasLabel = biasLabel ?? string.Empty;
            Summary = DomainGuard.RequiredText(summary, nameof(summary));
        }

        public string SourceId { get; }

        public int PublishedWeek { get; }

        public EvidenceReliability Reliability { get; }

        public string BiasLabel { get; }

        public string Summary { get; }
    }

    public sealed class PlayerPublicViewModel
    {
        public PlayerPublicViewModel(
            string playerId,
            string displayName,
            string biography,
            IEnumerable<PlayerPosition> claimedPositions,
            IEnumerable<PublicClaimViewModel> claims,
            IEnumerable<PublicEvidenceViewModel> evidence)
        {
            PlayerId = DomainGuard.StableId(playerId, nameof(playerId));
            DisplayName = DomainGuard.RequiredText(displayName, nameof(displayName));
            Biography = biography ?? string.Empty;
            ClaimedPositions = DomainGuard.ReadOnlyList(claimedPositions, nameof(claimedPositions));
            Claims = DomainGuard.ReadOnlyList(claims, nameof(claims));
            Evidence = DomainGuard.ReadOnlyList(evidence, nameof(evidence));
        }

        public string PlayerId { get; }

        public string DisplayName { get; }

        public string Biography { get; }

        public IReadOnlyList<PlayerPosition> ClaimedPositions { get; }

        public IReadOnlyList<PublicClaimViewModel> Claims { get; }

        public IReadOnlyList<PublicEvidenceViewModel> Evidence { get; }
    }

    public static class PlayerPublicViewModelFactory
    {
        public static PlayerPublicViewModel Create(
            PlayerPublicProfile profile,
            IEnumerable<PlayerClaim> claims,
            IEnumerable<EvidenceItem> evidence)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            if (evidence == null)
            {
                throw new ArgumentNullException(nameof(evidence));
            }

            var visibleClaims = claims
                .Where(claim => string.Equals(claim.PlayerId, profile.Id, StringComparison.Ordinal))
                .OrderBy(claim => claim.PublishedWeek)
                .Select(claim => new PublicClaimViewModel(
                    claim.SourceId,
                    claim.PublishedWeek,
                    claim.Text));
            var visibleEvidence = evidence
                .Where(item => string.Equals(item.PlayerId, profile.Id, StringComparison.Ordinal))
                .OrderBy(item => item.PublishedWeek)
                .Select(item => new PublicEvidenceViewModel(
                    item.SourceId,
                    item.PublishedWeek,
                    item.Reliability,
                    item.BiasLabel,
                    item.Summary));

            return new PlayerPublicViewModel(
                profile.Id,
                profile.DisplayName,
                profile.Biography,
                profile.ClaimedPositions,
                visibleClaims,
                visibleEvidence);
        }
    }
}
