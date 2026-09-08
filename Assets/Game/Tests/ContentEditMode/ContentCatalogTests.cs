using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace NewPlayerHunter.Gameplay.Tests
{
    public sealed class ContentCatalogTests
    {
        private GameContentCatalog _catalog;

        [SetUp]
        public void SetUp()
        {
            _catalog = ScriptableObject.CreateInstance<GameContentCatalog>();
            _catalog.PopulateM1Defaults();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_catalog);
        }

        [Test]
        public void M1Defaults_HaveConfigurablePoolsAndChineseDevelopmentLanguage()
        {
            Assert.That(_catalog.DevelopmentLanguage, Is.EqualTo(GameLanguage.ChineseSimplified));
            Assert.That(_catalog.Players, Has.Count.EqualTo(51));
            Assert.That(_catalog.Demands, Has.Count.EqualTo(35));
            Assert.That(_catalog.Mails, Has.Count.EqualTo(193));
            Assert.That(_catalog.MagazineIssues, Has.Count.EqualTo(34));
            Assert.That(
                _catalog.Mails.All(mail => !string.IsNullOrWhiteSpace(mail.receivedTime.chineseSimplified)),
                Is.True);
        }

        [Test]
        public void LocalizedText_EnglishFallsBackToChineseUntilTranslationExists()
        {
            var text = new LocalizedText
            {
                chineseSimplified = "中文开发文本",
                english = string.Empty
            };

            Assert.That(text.Resolve(GameLanguage.ChineseSimplified), Is.EqualTo("中文开发文本"));
            Assert.That(text.Resolve(GameLanguage.English), Is.EqualTo("中文开发文本"));
            text.english = "English translation";
            Assert.That(text.Resolve(GameLanguage.English), Is.EqualTo("English translation"));
        }

        [Test]
        public void EveryMailReference_UsesAnExistingStablePoolId()
        {
            var playerIds = _catalog.Players.Select(player => player.id).ToHashSet();
            var demandIds = _catalog.Demands.Select(demand => demand.id).ToHashSet();

            foreach (var mail in _catalog.Mails)
            {
                if (!string.IsNullOrEmpty(mail.relatedPlayerId))
                {
                    Assert.That(playerIds, Does.Contain(mail.relatedPlayerId), mail.id);
                }

                if (!string.IsNullOrEmpty(mail.relatedDemandId))
                {
                    Assert.That(demandIds, Does.Contain(mail.relatedDemandId), mail.id);
                }
            }
        }

        [Test]
        public void EveryPlayerAndDemand_HasARealMailThatCanUnlockIt()
        {
            foreach (var player in _catalog.Players)
            {
                Assert.That(
                    _catalog.Mails.Any(mail => mail.relatedPlayerId == player.id),
                    Is.True,
                    player.id);
            }

            foreach (var demand in _catalog.Demands)
            {
                Assert.That(
                    _catalog.Mails.Any(mail => mail.relatedDemandId == demand.id),
                    Is.True,
                    demand.id);
            }
        }

        [Test]
        public void MagazineIssues_AreMultiPageAndUseDistinctEditorialLayouts()
        {
            foreach (var issue in _catalog.MagazineIssues)
            {
                Assert.That(issue.pages.Count, Is.GreaterThanOrEqualTo(3), issue.id);
                Assert.That(
                    issue.pages.Select(page => page.layout).Distinct().Count(),
                    Is.GreaterThanOrEqualTo(2),
                    issue.id);
                Assert.That(issue.pages[0].layout, Is.EqualTo(MagazinePageLayout.Cover));
            }
        }

        [Test]
        public void MagazineContentPages_ReferenceValidIllustrationSlots()
        {
            foreach (var issue in _catalog.MagazineIssues)
            {
                foreach (var page in issue.pages)
                {
                    if (page.layout == MagazinePageLayout.Cover)
                    {
                        Assert.That(page.illustrationIndex, Is.EqualTo(-1),
                            $"{issue.id} cover must not use a content illustration.");
                        continue;
                    }

                    Assert.That(page.illustrationIndex, Is.InRange(0, 23),
                        $"{issue.id} {page.layout} page needs a valid illustration index.");
                }
            }
        }

        [Test]
        public void PlayerPool_AllowsMultiplePlayersWithTheSamePublicPosition()
        {
            Assert.That(
                _catalog.Players
                    .GroupBy(player => player.publicPosition)
                    .Any(group => group.Count() > 1),
                Is.True,
                "Player cards are generic list views; positions must not be unique slots.");
        }

        [Test]
        public void SixWeekExperience_HasDenseWeeklyMailAndOneMagazinePerWeek()
        {
            for (var week = 1; week <= 6; week++)
            {
                Assert.That(
                    _catalog.Mails.Count(mail => mail.publishedWeek == week),
                    Is.GreaterThanOrEqualTo(5),
                    $"week {week}");
                Assert.That(
                    _catalog.MagazineIssues.Count(issue => issue.publishedWeek == week),
                    Is.EqualTo(1),
                    $"week {week}");
            }
        }

        [Test]
        public void AnnualExperience_HasSeasonProgressAndConfigurableExpiryNotices()
        {
            Assert.That(
                _catalog.Mails.Count(mail => mail.id.StartsWith("mail.season.")),
                Is.EqualTo(13));
            Assert.That(
                _catalog.Mails.Any(mail => mail.publishedWeek == 52),
                Is.True);
            Assert.That(
                _catalog.Players.All(player =>
                    player.availableFromWeek >= 1 &&
                    player.availabilityWeeks >= 1 &&
                    !string.IsNullOrWhiteSpace(player.expiryMailBody.chineseSimplified)),
                Is.True);
            Assert.That(
                _catalog.Demands.All(demand =>
                    demand.activeWeeks >= 1 &&
                    demand.DeadlineWeek == demand.openedWeek + demand.activeWeeks - 1 &&
                    !string.IsNullOrWhiteSpace(demand.clubStanding.chineseSimplified) &&
                    !string.IsNullOrWhiteSpace(demand.clubBestAchievement.chineseSimplified)),
                Is.True);
            Assert.That(
                _catalog.Mails.Count(mail => !string.IsNullOrEmpty(mail.expiredPlayerId)),
                Is.EqualTo(51));
            Assert.That(
                _catalog.Mails.Count(mail => !string.IsNullOrEmpty(mail.expiredDemandId)),
                Is.EqualTo(35));
        }

        [Test]
        public void GeneratedArtIndices_AreUniqueAndFitTheirAtlases()
        {
            Assert.That(
                _catalog.Players.Select(player => player.portraitIndex).Distinct().Count(),
                Is.EqualTo(51));
            Assert.That(
                _catalog.Players.All(player => player.portraitIndex >= 0 && player.portraitIndex < 64),
                Is.True);
            Assert.That(
                _catalog.MagazineIssues.Select(issue => issue.coverIndex),
                Is.EquivalentTo(Enumerable.Range(0, 34).ToArray()));
        }

        [Test]
        public void LateSeasonExperience_SpreadsContentAcrossTheWholeSeason()
        {
            var unlockMails = _catalog.Mails
                .Where(mail => mail.kind == MailContentKind.PlayerResume ||
                               mail.kind == MailContentKind.ClubRequest)
                .ToList();
            Assert.That(
                unlockMails.Count(mail => mail.publishedWeek >= 7),
                Is.EqualTo(64));
            foreach (var week in new[]
                         { 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 21 })
            {
                Assert.That(
                    unlockMails.Count(mail => mail.publishedWeek == week),
                    Is.GreaterThanOrEqualTo(2),
                    $"week {week}");
            }

            var lateIssues = _catalog.MagazineIssues
                .Where(issue => issue.publishedWeek >= 7)
                .ToList();
            Assert.That(lateIssues, Has.Count.EqualTo(28));
            Assert.That(lateIssues.Any(issue => issue.publishedWeek == 51), Is.True);
            Assert.That(
                _catalog.Players.Count(player => player.availableFromWeek >= 7),
                Is.EqualTo(35));
            Assert.That(
                _catalog.Demands.Count(demand => demand.openedWeek >= 7),
                Is.EqualTo(29));
        }

        [Test]
        public void EveryPlayer_HasSalaryRangeAndCareerHistory()
        {
            foreach (var player in _catalog.Players)
            {
                Assert.That(player.salaryMinWeekly, Is.GreaterThan(0), player.id);
                Assert.That(
                    player.salaryMaxWeekly,
                    Is.GreaterThanOrEqualTo(player.salaryMinWeekly),
                    player.id);
                Assert.That(
                    string.IsNullOrWhiteSpace(player.careerHistory.chineseSimplified),
                    Is.False,
                    player.id);
            }
        }

        [Test]
        public void CarloPrivateRequest_HasFixedImmediateOfferAndDelayedRiskCopy()
        {
            var mail = _catalog.Mails.Single(item => item.id == "mail.w1.carlo");
            Assert.That(mail.kind, Is.EqualTo(MailContentKind.PrivateRequest));
            Assert.That(mail.privateOfferAmount, Is.EqualTo(350));
            Assert.That(mail.privateOfferTerms.chineseSimplified, Does.Contain("卡洛"));
            Assert.That(mail.privateRequiredClubId, Is.EqualTo("club.rainy"));
            Assert.That(mail.privateTargetClubRequirement.chineseSimplified, Does.Contain("雨城竞技"));
            Assert.That(mail.privateRiskNote.chineseSimplified, Does.Contain("声望"));
        }

        [Test]
        public void AllContent_EnglishTranslationsPresent()
        {
            var entries = _catalog.Players.Cast<object>()
                .Concat(_catalog.Demands)
                .Concat(_catalog.Mails)
                .Concat(_catalog.MagazineIssues)
                .Concat(_catalog.MagazineIssues.SelectMany(issue => issue.pages));
            var checkedCount = 0;
            foreach (var entry in entries)
            {
                var entryId = entry.GetType().GetField("id")?.GetValue(entry) as string
                    ?? entry.ToString();
                foreach (var field in entry.GetType().GetFields())
                {
                    if (field.FieldType != typeof(LocalizedText) ||
                        field.GetValue(entry) is not LocalizedText text ||
                        string.IsNullOrWhiteSpace(text.chineseSimplified))
                    {
                        continue;
                    }

                    checkedCount++;
                    Assert.That(
                        string.IsNullOrWhiteSpace(text.english),
                        Is.False,
                        $"{entryId}.{field.Name} has Chinese text but no English translation.");
                }
            }

            Assert.That(checkedCount, Is.GreaterThan(1000),
                "The reflection sweep must actually visit the catalog's localized fields.");
        }

        [Test]
        public void UiStrings_AllKeysHaveBothLanguages()
        {
            var entriesField = typeof(UiStrings).GetField(
                "Entries",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(entriesField, Is.Not.Null, "UiStrings.Entries field not found.");
            var entries = (IDictionary<string, string[]>)entriesField.GetValue(null);
            Assert.That(entries, Is.Not.Empty);
            foreach (var pair in entries)
            {
                Assert.That(pair.Value, Is.Not.Null.And.Length.EqualTo(2), pair.Key);
                Assert.That(
                    string.IsNullOrWhiteSpace(pair.Value[0]),
                    Is.False,
                    $"{pair.Key} is missing Chinese text.");
                Assert.That(
                    string.IsNullOrWhiteSpace(pair.Value[1]),
                    Is.False,
                    $"{pair.Key} is missing English text.");
            }
        }
    }
}
