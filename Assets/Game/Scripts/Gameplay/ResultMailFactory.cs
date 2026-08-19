using System;
using NewPlayerHunter.Domain;

namespace NewPlayerHunter.Gameplay
{
    public static class ResultMailFactory
    {
        public static string BuildMailId(PlacementOutcome outcome)
        {
            var assignment = outcome.Assignment;
            return
                $"result.w{outcome.OutcomeWeek}.{assignment.DemandId}.{assignment.SlotId}.{assignment.PlayerId}";
        }

        public static MailContentEntry Create(
            PlacementOutcome outcome,
            string demandTitle,
            string clubDisplayName,
            string playerDisplayName)
        {
            if (outcome == null)
            {
                throw new ArgumentNullException(nameof(outcome));
            }

            var club = string.IsNullOrEmpty(clubDisplayName) ? "俱乐部" : clubDisplayName;
            var evaluation = BuildEvaluation(outcome, playerDisplayName);
            var body =
                $"{club} 就“{demandTitle}”发来正式回函。\n\n" +
                evaluation + "\n\n" +
                $"本次委托报酬 €{outcome.RewardAmount:0.00} 已按付款条款安排，随本回函一同到账。";

            return new MailContentEntry
            {
                id = BuildMailId(outcome),
                kind = MailContentKind.ClubFeedback,
                publishedWeek = outcome.OutcomeWeek,
                sender = Zh(club),
                subject = Zh($"试训反馈：{playerDisplayName}"),
                receivedTime = Zh("周一 07:30"),
                preview = Zh(string.Empty),
                body = Zh(body),
                sourceNote = Zh("俱乐部正式回函 · 可直接采信"),
                relatedPlayerId = string.Empty,
                relatedDemandId = string.Empty,
                privateOfferAmount = 0,
                privateOfferTerms = Zh(string.Empty),
                privateRequiredClubId = string.Empty,
                privateTargetClubRequirement = Zh(string.Empty),
                privateRiskNote = Zh(string.Empty),
                expiredPlayerId = string.Empty,
                expiredDemandId = string.Empty
            };
        }

        private static string BuildEvaluation(PlacementOutcome outcome, string playerDisplayName)
        {
            var band = outcome.MatchScore >= 0.70d
                ? 2
                : outcome.MatchScore >= 0.45d
                    ? 1
                    : 0;

            switch (outcome.ResultKind)
            {
                case PlacementResultKind.Accepted when band == 2:
                    return $"{playerDisplayName} 在试训中的表现超出教练组预期，俱乐部已提供正式机会，并在回函中特别表扬了其训练态度。";
                case PlacementResultKind.Accepted:
                    return $"{playerDisplayName} 达到了本次录用标准，俱乐部提供正式机会。教练组评价其表现稳定，但仍有明显提升空间。";
                case PlacementResultKind.TrialExtended when band >= 1:
                    return $"{playerDisplayName} 展现了值得继续观察的潜力，但教练组意见尚未统一，俱乐部决定延长考察期，暂不作正式承诺。";
                case PlacementResultKind.TrialExtended:
                    return $"{playerDisplayName} 的表现未能完全说服教练组，俱乐部愿意再给一段观察期，但回函措辞明显谨慎。";
                case PlacementResultKind.Rejected when band >= 1:
                    return $"{playerDisplayName} 的个别环节得到认可，但整体未达到本次要求，俱乐部礼貌地提前结束试训，并表示未来保持关注。";
                default:
                    return $"{playerDisplayName} 的表现与俱乐部要求差距明显，试训提前结束。球探报告附注里出现了“令人难忘的错误”这一措辞。";
            }
        }

        private static LocalizedText Zh(string value)
        {
            return new LocalizedText
            {
                chineseSimplified = value,
                english = string.Empty
            };
        }
    }
}
