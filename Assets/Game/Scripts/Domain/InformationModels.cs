using System;

namespace NewPlayerHunter.Domain
{
    public enum InformationChannel
    {
        Mail,
        Subscription
    }

    public sealed class PublicInformationItem
    {
        public PublicInformationItem(
            string id,
            InformationChannel channel,
            string sender,
            string subject,
            string preview,
            string body,
            string sourceNote,
            int publishedWeek,
            string relatedPlayerId = null)
        {
            if (publishedWeek < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(publishedWeek));
            }

            Id = DomainGuard.StableId(id, nameof(id));
            Channel = channel;
            Sender = DomainGuard.RequiredText(sender, nameof(sender));
            Subject = DomainGuard.RequiredText(subject, nameof(subject));
            Preview = DomainGuard.RequiredText(preview, nameof(preview));
            Body = DomainGuard.RequiredText(body, nameof(body));
            SourceNote = DomainGuard.RequiredText(sourceNote, nameof(sourceNote));
            PublishedWeek = publishedWeek;
            RelatedPlayerId = string.IsNullOrWhiteSpace(relatedPlayerId)
                ? string.Empty
                : DomainGuard.StableId(relatedPlayerId, nameof(relatedPlayerId));
        }

        public string Id { get; }

        public InformationChannel Channel { get; }

        public string Sender { get; }

        public string Subject { get; }

        public string Preview { get; }

        public string Body { get; }

        public string SourceNote { get; }

        public int PublishedWeek { get; }

        public string RelatedPlayerId { get; }
    }
}
