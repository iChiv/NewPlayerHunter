using System;
using NewPlayerHunter.Domain;

namespace NewPlayerHunter.Gameplay
{
    public static class ResultMailFactory
    {
        private static readonly string[] HiddenInjuryHarshEvaluations =
        {
            "随函附上的体检报告措辞沉重：队医在例行检查中发现了{0}本人都未必知情的隐患。俱乐部对此深感遗憾，但无法在现阶段承担相应风险，试训到此为止。",
            "队医在体检室门口拦住了教练组，随后的谈话很短。{0}的身体状况存在不容忽视的隐患，俱乐部本着对双方负责的态度，遗憾地终止本次试训。"
        };

        private static readonly string[] HiddenInjuryMildEvaluations =
        {
            "体检环节的结论出人意料。队医表示{0}的某项指标需要进一步观察，俱乐部不愿冒险，决定提前结束试训，并祝愿其早日康复。",
            "回函附带了一份措辞委婉的体检说明。{0}在训练中的表现尚可，但队医坚持“宁可错过，不可签错”，俱乐部采纳了这一建议。"
        };

        private static readonly string[] PersonalityConflictEvaluations =
        {
            "{0}与更衣室的磨合过程算不上顺利。教练组在回函草稿中删去了三处形容词，最终决定延长考察期，以观后效。",
            "纪律组与教练组就{0}的问题开了两次会，会议纪要里出现了“个性鲜明”这一外交辞令。俱乐部决定延长试训，暂不作正式承诺。",
            "{0}的球技获得了认可，但训练之外的表现引发了讨论。俱乐部决定延长考察期，并希望其在下次报到前熟读队规附录。"
        };

        private static readonly string[] FitnessDoubtEvaluations =
        {
            "{0}展现了技术意识，但体能教练在报告边栏画了一个问号。俱乐部决定延长考察期，重点观察其身体状态能否支撑连续作战。",
            "教练组认可{0}的场上判断，但对其体能储备持保留意见。回函决定延长试训，体能表现将在下一阶段重新评估。"
        };

        private static readonly string[] AcceptedSurpriseEvaluations =
        {
            "{0}的表现超出了球探报告的所有预测，教练组在回函中使用了“意外之喜”一词。俱乐部欣然提供正式机会。",
            "原本只是例行考察，{0}却让教练组连夜修改了评估结论。俱乐部提供正式机会，并期待这份惊喜延续下去。",
            "回函起草人承认低估了{0}。试训表现远超预期，俱乐部已提供正式机会，球探部门正在重新学习如何撰写报告。"
        };

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
            switch (outcome.NarrativeKey)
            {
                case "placement.rejected.hidden_injury":
                    return PickVariant(
                        outcome.RandomRoll,
                        5d,
                        playerDisplayName,
                        outcome.MatchScore < 0.45d ? HiddenInjuryHarshEvaluations : HiddenInjuryMildEvaluations);
                case "placement.trial_extended.personality_conflict":
                    return PickVariant(
                        outcome.RandomRoll,
                        11d,
                        playerDisplayName,
                        PersonalityConflictEvaluations);
                case "placement.trial_extended.fitness_doubt":
                    return PickVariant(
                        outcome.RandomRoll,
                        7d,
                        playerDisplayName,
                        FitnessDoubtEvaluations);
                case "placement.accepted.surprise":
                    return PickVariant(
                        outcome.RandomRoll,
                        13d,
                        playerDisplayName,
                        AcceptedSurpriseEvaluations);
            }

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

        private static string PickVariant(
            double randomRoll,
            double prime,
            string playerDisplayName,
            string[] candidates)
        {
            var fraction = (randomRoll * prime) - Math.Floor(randomRoll * prime);
            var index = (int)(fraction * candidates.Length);
            if (index >= candidates.Length)
            {
                index = candidates.Length - 1;
            }

            return string.Format(candidates[index], playerDisplayName);
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
