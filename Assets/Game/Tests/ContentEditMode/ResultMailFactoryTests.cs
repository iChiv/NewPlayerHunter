using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NewPlayerHunter.Domain;

namespace NewPlayerHunter.Gameplay.Tests
{
    public sealed class ResultMailFactoryTests
    {
        private const string PlayerName = "郝球员";

        [Test]
        public void VariantNarrativeKeys_ProduceDistinctDigitFreeEvaluationsWithPlayerName()
        {
            var cases = new[]
            {
                ("placement.rejected.hidden_injury", PlacementResultKind.Rejected, 0.30d, 2),
                ("placement.rejected.hidden_injury", PlacementResultKind.Rejected, 0.60d, 2),
                ("placement.trial_extended.personality_conflict", PlacementResultKind.TrialExtended, 0.55d, 3),
                ("placement.trial_extended.fitness_doubt", PlacementResultKind.TrialExtended, 0.50d, 2),
                ("placement.accepted.surprise", PlacementResultKind.Accepted, 0.72d, 3)
            };

            foreach (var (narrativeKey, resultKind, matchScore, expectedCount) in cases)
            {
                var texts = CollectEvaluations(narrativeKey, resultKind, matchScore);

                Assert.That(texts, Has.Count.EqualTo(expectedCount), narrativeKey);
                foreach (var text in texts)
                {
                    Assert.That(text, Is.Not.Null.And.Not.Empty, narrativeKey);
                    Assert.That(text, Does.Contain(PlayerName), narrativeKey);
                    Assert.That(text.Any(char.IsDigit), Is.False, $"{narrativeKey}: {text}");
                }
            }
        }

        [Test]
        public void HiddenInjuryEvaluation_BecomesHarsherWhenMatchScoreIsLow()
        {
            var harsh = CollectEvaluations("placement.rejected.hidden_injury", PlacementResultKind.Rejected, 0.30d);
            var mild = CollectEvaluations("placement.rejected.hidden_injury", PlacementResultKind.Rejected, 0.60d);

            Assert.That(harsh.Intersect(mild), Is.Empty);
            Assert.That(harsh.Any(text => text.Contains("终止")), Is.True);
        }

        [Test]
        public void SameOutcome_ProducesIdenticalMailText()
        {
            var outcome = CreateOutcome(
                "placement.trial_extended.personality_conflict",
                PlacementResultKind.TrialExtended,
                matchScore: 0.55d,
                randomRoll: 0.618d);

            var first = ResultMailFactory.Create(outcome, "试训委托", "雨城竞技", PlayerName);
            var second = ResultMailFactory.Create(outcome, "试训委托", "雨城竞技", PlayerName);

            Assert.That(second.id, Is.EqualTo(first.id));
            Assert.That(second.body.chineseSimplified, Is.EqualTo(first.body.chineseSimplified));
            Assert.That(second.subject.chineseSimplified, Is.EqualTo(first.subject.chineseSimplified));
        }

        [Test]
        public void BuildMailId_FollowsWeekDemandSlotPlayerFormat()
        {
            var outcome = CreateOutcome(
                "placement.accepted",
                PlacementResultKind.Accepted,
                matchScore: 0.90d,
                randomRoll: 0.25d);

            Assert.That(
                ResultMailFactory.BuildMailId(outcome),
                Is.EqualTo("result.w4.demand.sun.slot.wing.player.hao"));
        }

        [Test]
        public void LegacyNarrativeKeys_KeepOriginalBandEvaluation()
        {
            var rejected = EvaluationOf(CreateOutcome(
                "placement.rejected",
                PlacementResultKind.Rejected,
                matchScore: 0.30d,
                randomRoll: 0.90d));
            var accepted = EvaluationOf(CreateOutcome(
                "placement.accepted",
                PlacementResultKind.Accepted,
                matchScore: 0.85d,
                randomRoll: 0.25d));

            Assert.That(
                rejected,
                Is.EqualTo($"{PlayerName} 的表现与俱乐部要求差距明显，试训提前结束。球探报告附注里出现了“令人难忘的错误”这一措辞。"));
            Assert.That(
                accepted,
                Is.EqualTo($"{PlayerName} 在试训中的表现超出教练组预期，俱乐部已提供正式机会，并在回函中特别表扬了其训练态度。"));
        }

        private static HashSet<string> CollectEvaluations(
            string narrativeKey,
            PlacementResultKind resultKind,
            double matchScore)
        {
            var texts = new HashSet<string>(StringComparer.Ordinal);
            for (var sample = 0; sample < 1000; sample++)
            {
                var roll = (sample + 0.5d) / 1000d;
                texts.Add(EvaluationOf(CreateOutcome(narrativeKey, resultKind, matchScore, roll)));
            }

            return texts;
        }

        private static string EvaluationOf(PlacementOutcome outcome)
        {
            var mail = ResultMailFactory.Create(outcome, "试训委托", "雨城竞技", PlayerName);
            return mail.body.chineseSimplified.Split(new[] { "\n\n" }, StringSplitOptions.None)[1];
        }

        private static PlacementOutcome CreateOutcome(
            string narrativeKey,
            PlacementResultKind resultKind,
            double matchScore,
            double randomRoll)
        {
            return new PlacementOutcome(
                new Assignment("demand.sun", "slot.wing", "player.hao", week: 2),
                outcomeWeek: 4,
                matchScore,
                randomRoll,
                resultKind,
                rewardAmount: 640m,
                reputationDelta: 0,
                narrativeKey);
        }
    }
}
