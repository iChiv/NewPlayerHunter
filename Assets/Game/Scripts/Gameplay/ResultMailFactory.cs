using System;
using NewPlayerHunter.Domain;

namespace NewPlayerHunter.Gameplay
{
    public static class ResultMailFactory
    {
        private static readonly string[] HiddenInjuryHarshEvaluationsZh =
        {
            "随函附上的体检报告措辞沉重：队医在例行检查中发现了{0}本人都未必知情的隐患。俱乐部对此深感遗憾，但无法在现阶段承担相应风险，试训到此为止。",
            "队医在体检室门口拦住了教练组，随后的谈话很短。{0}的身体状况存在不容忽视的隐患，俱乐部本着对双方负责的态度，遗憾地终止本次试训。"
        };

        private static readonly string[] HiddenInjuryHarshEvaluationsEn =
        {
            "The enclosed medical report makes for grim reading: the club doctor found a problem even {0} didn't know about. The club is deeply sorry, but cannot take on that kind of risk right now. The trial ends here.",
            "The club doctor intercepted the coaching staff at the treatment-room door, and the conversation that followed was very short. {0}'s condition is a risk nobody can ignore, so — acting in everyone's best interest — the club regretfully terminates this trial."
        };

        private static readonly string[] HiddenInjuryMildEvaluationsZh =
        {
            "体检环节的结论出人意料。队医表示{0}的某项指标需要进一步观察，俱乐部不愿冒险，决定提前结束试训，并祝愿其早日康复。",
            "回函附带了一份措辞委婉的体检说明。{0}在训练中的表现尚可，但队医坚持“宁可错过，不可签错”，俱乐部采纳了这一建议。"
        };

        private static readonly string[] HiddenInjuryMildEvaluationsEn =
        {
            "The medical produced a surprise twist. The doctor says one of {0}'s numbers needs further observation, and the club would rather not gamble. The trial ends early, with sincere wishes for a speedy recovery.",
            "Attached is a delicately worded medical note. {0} did fine in training, but the doctor lives by 'better to miss one than mis-sign one', and the club has taken his advice."
        };

        private static readonly string[] PersonalityConflictEvaluationsZh =
        {
            "{0}与更衣室的磨合过程算不上顺利。教练组在回函草稿中删去了三处形容词，最终决定延长考察期，以观后效。",
            "纪律组与教练组就{0}的问题开了两次会，会议纪要里出现了“个性鲜明”这一外交辞令。俱乐部决定延长试训，暂不作正式承诺。",
            "{0}的球技获得了认可，但训练之外的表现引发了讨论。俱乐部决定延长考察期，并希望其在下次报到前熟读队规附录。"
        };

        private static readonly string[] PersonalityConflictEvaluationsEn =
        {
            "{0}'s settling-in with the dressing room has been anything but smooth. Three adjectives were crossed out of the reply draft before anyone dared send it. The observation period is extended; we shall see.",
            "The disciplinary committee and the coaches have now held two meetings about {0}, and the minutes use the diplomatic phrase 'a strong personality'. The trial is extended, with no formal commitment for now.",
            "{0}'s football won approval, but everything happening off the training pitch has sparked debate. The observation period is extended, with a polite suggestion to memorise the appendix of the club rulebook before reporting back."
        };

        private static readonly string[] FitnessDoubtEvaluationsZh =
        {
            "{0}展现了技术意识，但体能教练在报告边栏画了一个问号。俱乐部决定延长考察期，重点观察其身体状态能否支撑连续作战。",
            "教练组认可{0}的场上判断，但对其体能储备持保留意见。回函决定延长试训，体能表现将在下一阶段重新评估。"
        };

        private static readonly string[] FitnessDoubtEvaluationsEn =
        {
            "{0} showed real technical awareness, but the fitness coach drew a single question mark in the margin of his report. The observation period is extended, focused on whether the body can survive back-to-back matches.",
            "The coaching staff rate {0}'s reading of the game but reserve judgment on the engine. The trial continues; the stamina numbers will be re-evaluated in the next phase."
        };

        private static readonly string[] AcceptedSurpriseEvaluationsZh =
        {
            "{0}的表现超出了球探报告的所有预测，教练组在回函中使用了“意外之喜”一词。俱乐部欣然提供正式机会。",
            "原本只是例行考察，{0}却让教练组连夜修改了评估结论。俱乐部提供正式机会，并期待这份惊喜延续下去。",
            "回函起草人承认低估了{0}。试训表现远超预期，俱乐部已提供正式机会，球探部门正在重新学习如何撰写报告。"
        };

        private static readonly string[] AcceptedSurpriseEvaluationsEn =
        {
            "{0} outperformed every prediction in the scouting report, and the reply letter uses the phrase 'a lovely surprise'. The club is delighted to offer a proper deal.",
            "This was meant to be a routine look, but {0} made the coaching staff rewrite their conclusions overnight. A formal offer is on the table, and everyone hopes the surprise keeps going.",
            "The person drafting this reply admits he underestimated {0}. The trial was well beyond expectations, a formal offer has been made, and the scouting department is re-learning how to write reports."
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
            string playerDisplayName,
            GameLanguage language)
        {
            if (outcome == null)
            {
                throw new ArgumentNullException(nameof(outcome));
            }

            var isEnglish = language == GameLanguage.English;
            var club = string.IsNullOrEmpty(clubDisplayName)
                ? (isEnglish ? "The club" : "俱乐部")
                : clubDisplayName;
            var evaluation = BuildEvaluation(outcome, playerDisplayName, language);
            var body = isEnglish
                ? $"{club} has sent an official reply regarding \"{demandTitle}\".\n\n" +
                  evaluation + "\n\n" +
                  $"The assignment fee of €{outcome.RewardAmount:0.00} has been arranged per the payment terms and arrives with this letter."
                : $"{club} 就“{demandTitle}”发来正式回函。\n\n" +
                  evaluation + "\n\n" +
                  $"本次委托报酬 €{outcome.RewardAmount:0.00} 已按付款条款安排，随本回函一同到账。";

            return new MailContentEntry
            {
                id = BuildMailId(outcome),
                kind = MailContentKind.ClubFeedback,
                publishedWeek = outcome.OutcomeWeek,
                sender = Bilingual(club, club),
                subject = Bilingual(
                    $"试训反馈：{playerDisplayName}",
                    $"Trial feedback: {playerDisplayName}"),
                receivedTime = Bilingual("周一 07:30", "Mon 07:30"),
                preview = Bilingual(string.Empty, string.Empty),
                body = Bilingual(body, body),
                sourceNote = Bilingual(
                    "俱乐部正式回函 · 可直接采信",
                    "Official club reply · safe to trust"),
                relatedPlayerId = string.Empty,
                relatedDemandId = string.Empty,
                privateOfferAmount = 0,
                privateOfferTerms = Bilingual(string.Empty, string.Empty),
                privateRequiredClubId = string.Empty,
                privateTargetClubRequirement = Bilingual(string.Empty, string.Empty),
                privateRiskNote = Bilingual(string.Empty, string.Empty),
                expiredPlayerId = string.Empty,
                expiredDemandId = string.Empty
            };
        }

        private static string BuildEvaluation(
            PlacementOutcome outcome,
            string playerDisplayName,
            GameLanguage language)
        {
            var isEnglish = language == GameLanguage.English;
            switch (outcome.NarrativeKey)
            {
                case "placement.rejected.hidden_injury":
                    var harsh = outcome.MatchScore < 0.45d;
                    return PickVariant(
                        outcome.RandomRoll,
                        5d,
                        playerDisplayName,
                        harsh
                            ? (isEnglish ? HiddenInjuryHarshEvaluationsEn : HiddenInjuryHarshEvaluationsZh)
                            : (isEnglish ? HiddenInjuryMildEvaluationsEn : HiddenInjuryMildEvaluationsZh));
                case "placement.trial_extended.personality_conflict":
                    return PickVariant(
                        outcome.RandomRoll,
                        11d,
                        playerDisplayName,
                        isEnglish ? PersonalityConflictEvaluationsEn : PersonalityConflictEvaluationsZh);
                case "placement.trial_extended.fitness_doubt":
                    return PickVariant(
                        outcome.RandomRoll,
                        7d,
                        playerDisplayName,
                        isEnglish ? FitnessDoubtEvaluationsEn : FitnessDoubtEvaluationsZh);
                case "placement.accepted.surprise":
                    return PickVariant(
                        outcome.RandomRoll,
                        13d,
                        playerDisplayName,
                        isEnglish ? AcceptedSurpriseEvaluationsEn : AcceptedSurpriseEvaluationsZh);
            }

            var band = outcome.MatchScore >= 0.70d
                ? 2
                : outcome.MatchScore >= 0.45d
                    ? 1
                    : 0;

            if (isEnglish)
            {
                switch (outcome.ResultKind)
                {
                    case PlacementResultKind.Accepted when band == 2:
                        return $"{playerDisplayName} exceeded the coaching staff's expectations in the trial. The club has made a formal offer and gave special praise to his training attitude.";
                    case PlacementResultKind.Accepted:
                        return $"{playerDisplayName} met the recruitment standard, and the club has made a formal offer. The coaches call the performances solid, with obvious room to grow.";
                    case PlacementResultKind.TrialExtended when band >= 1:
                        return $"{playerDisplayName} showed potential worth a longer look, but the coaches have yet to agree. The observation period is extended, with no formal commitment for now.";
                    case PlacementResultKind.TrialExtended:
                        return $"{playerDisplayName} did not fully convince the coaching staff. The club will grant another observation period, though the wording of the reply is noticeably cautious.";
                    case PlacementResultKind.Rejected when band >= 1:
                        return $"Parts of {playerDisplayName}'s game were praised, but the whole package fell short this time. The club politely ends the trial early and promises to keep watching from afar.";
                    default:
                        return $"{playerDisplayName}'s performances were some distance from the club's requirements, and the trial ends early. The scouting report's appendix contains the phrase 'a memorable mistake'.";
                }
            }

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

        private static LocalizedText Bilingual(string chinese, string english)
        {
            return new LocalizedText
            {
                chineseSimplified = chinese,
                english = english
            };
        }
    }
}
