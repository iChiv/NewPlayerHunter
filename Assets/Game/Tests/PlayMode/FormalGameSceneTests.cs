using System.Collections;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace NewPlayerHunter.Gameplay.Tests
{
    public sealed class FormalGameSceneTests
    {
        [UnityTest]
        public IEnumerator GameScene_ReadMailUnlocksAssignment_AndMagazineHasPages()
        {
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
            yield return null;

            var controller = Object.FindFirstObjectByType<GameController>();
            Assert.That(controller, Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<Camera>(), Is.Not.Null);
            Assert.That(GameObject.Find("Global Light 2D"), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<Canvas>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<EventSystem>(), Is.Not.Null);
            Assert.That(
                controller.transform.Find("GameCanvas/Background/Header/Title"),
                Is.Null,
                "The top-left header no longer reserves space for a game title.");

            Assert.That(controller.PlayerPoolCount, Is.EqualTo(16));
            Assert.That(controller.DemandPoolCount, Is.EqualTo(6));
            Assert.That(controller.MailPoolCount, Is.EqualTo(65));
            Assert.That(controller.MagazineIssueCount, Is.EqualTo(6));
            Assert.That(controller.UnlockedPlayerCount, Is.Zero);
            Assert.That(controller.UnlockedDemandCount, Is.Zero);
            Assert.That(controller.CurrentDemandId, Is.Empty);

            var cards = controller.transform.Find(
                "GameCanvas/Background/AssignmentWorkspace/PlayersPanel/PlayerScroll/Viewport/PlayerCards");
            var mailItems = controller.transform.Find(
                "GameCanvas/Background/InformationWorkspace/MailBrowser/MessageListPanel/MessageScroll/Viewport/MessageList");
            var demandBlock = controller.transform.Find(
                "GameCanvas/Background/InformationWorkspace/MailBrowser/DetailPanel/DemandBlock");
            var magazineBrowser = controller.transform.Find(
                "GameCanvas/Background/InformationWorkspace/MagazineBrowser");
            var issueItems = magazineBrowser.Find("IssueRail/IssueList");
            var privateOfferBlock = controller.transform.Find(
                "GameCanvas/Background/InformationWorkspace/MailBrowser/DetailPanel/PrivateOfferBlock");
            Assert.That(cards.childCount, Is.EqualTo(16));
            Assert.That(mailItems.childCount, Is.EqualTo(80));
            Assert.That(issueItems.childCount, Is.EqualTo(6));
            Assert.That(privateOfferBlock, Is.Not.Null);
            Assert.That(demandBlock.Find("ClubProfile"), Is.Not.Null);
            Assert.That(privateOfferBlock.Find("TargetClub"), Is.Not.Null);
            Assert.That(
                demandBlock.parent.Find("ResumeBlock/Availability"),
                Is.Not.Null);
            var firstMailItem = mailItems.GetChild(0);
            Assert.That(firstMailItem.Find("Avatar/Initials"), Is.Not.Null);
            Assert.That(firstMailItem.Find("Timestamp"), Is.Not.Null);
            Assert.That(firstMailItem.Find("ReadDot"), Is.Not.Null);
            Assert.That(
                firstMailItem.Find("Timestamp").GetComponent<TextMeshProUGUI>().text,
                Does.Contain("周一"));
            Assert.That(
                firstMailItem.Find("Preview").GetComponent<TextMeshProUGUI>().text,
                Does.EndWith("……"));
            Assert.That(demandBlock, Is.Not.Null);
            Assert.That(magazineBrowser, Is.Not.Null);
            Assert.That(magazineBrowser.Find("PagePanel/CoverLayout"), Is.Not.Null);
            Assert.That(magazineBrowser.Find("PagePanel/FeatureLayout"), Is.Not.Null);
            Assert.That(magazineBrowser.Find("PagePanel/ScoutReportLayout"), Is.Not.Null);

            controller.OpenMailForTests("mail.w1.rainy");
            yield return null;
            Assert.That(controller.ReadMailCount, Is.EqualTo(1));
            Assert.That(controller.UnlockedDemandCount, Is.EqualTo(1));
            Assert.That(controller.CurrentDemandId, Is.EqualTo("demand.week1.emergency"));
            Assert.That(demandBlock.gameObject.activeSelf, Is.True);

            controller.OpenMailForTests("mail.w1.carlo");
            controller.OpenMailForTests("mail.w1.rory");
            yield return null;
            Assert.That(
                demandBlock.parent.Find("ResumeBlock/Portrait").GetComponent<RawImage>().texture,
                Is.Not.Null);
            Assert.That(controller.ReadMailCount, Is.EqualTo(3));
            Assert.That(controller.UnlockedPlayerCount, Is.EqualTo(2));
            Assert.That(
                controller.VisiblePlayerIds,
                Is.EqualTo(new[] { "player.rocket.rory", "player.cousin.carlo" }),
                "Available players follow mail receipt order, not mail open order or fixed card position.");

            controller.ShowSubscriptionsForTests();
            yield return null;
            Assert.That(controller.ActiveInformationChannel, Is.EqualTo("Magazine"));
            Assert.That(controller.CurrentMagazinePageCount, Is.GreaterThan(1));
            Assert.That(controller.CurrentMagazinePageIndex, Is.Zero);
            Assert.That(controller.CurrentMagazinePageLayout, Is.EqualTo("Cover"));
            Assert.That(
                magazineBrowser.Find("PagePanel/CoverLayout/CoverImage").GetComponent<RawImage>().texture,
                Is.Not.Null);
            Assert.That(
                magazineBrowser.Find("PagePanel/CoverLayout/CoverImage")
                    .GetComponent<AspectRatioFitter>().aspectRatio,
                Is.EqualTo(1f));
            controller.NextMagazinePageForTests();
            yield return null;
            Assert.That(controller.CurrentMagazinePageIndex, Is.EqualTo(1));
            Assert.That(controller.CurrentMagazinePageLayout, Is.Not.EqualTo("Cover"));
            Assert.That(controller.UnlockedPlayerCount, Is.EqualTo(2),
                "Reading a magazine must not bypass the formal resume mail unlock.");

            controller.ShowAssignmentForTests();
            yield return null;
            Assert.That(controller.ActiveWorkspace, Is.EqualTo("Assignment"));
            Assert.That(controller.VisibleSlotIds.Count, Is.EqualTo(1));
            Assert.That(controller.VisiblePlayerIds.Count, Is.EqualTo(2));
            Assert.That(
                cards.GetChild(0).Find("Name").GetComponent<TextMeshProUGUI>().text,
                Is.EqualTo("罗里·奎克"));
            Assert.That(
                cards.GetChild(1).Find("Name").GetComponent<TextMeshProUGUI>().text,
                Is.EqualTo("卡洛·贝利尼"));
            Assert.That(
                cards.GetChild(0).Find("Portrait").GetComponent<RawImage>().texture,
                Is.Not.Null);
            Assert.That(
                cards.GetChild(0).Find("Portrait")
                    .GetComponent<AspectRatioFitter>().aspectRatio,
                Is.EqualTo(1f));
            Assert.That(
                controller.AssignPlayerForTests(
                    controller.VisibleSlotIds[0],
                    controller.VisiblePlayerIds[0]),
                Is.True);

            yield return null;
            Assert.That(
                controller.VisiblePlayerIds,
                Is.EqualTo(new[] { "player.cousin.carlo" }),
                "An assigned player must disappear from the available list.");
            Assert.That(
                cards.GetChild(0).Find("Name").GetComponent<TextMeshProUGUI>().text,
                Is.EqualTo("卡洛·贝利尼"),
                "Generic card views compact the remaining players from top to bottom.");

            controller.HandleSlotClicked(controller.VisibleSlotIds[0]);
            yield return null;
            Assert.That(
                controller.VisiblePlayerIds,
                Is.EqualTo(new[] { "player.rocket.rory", "player.cousin.carlo" }),
                "Removing an assignment restores the player in receipt order.");
            Assert.That(
                controller.AssignPlayerForTests(
                    controller.VisibleSlotIds[0],
                    controller.VisiblePlayerIds[0]),
                Is.True);
            controller.EndWeekForTests();
            yield return null;
            Assert.That(controller.CurrentWeek, Is.EqualTo(2));
            Assert.That(
                controller.VisiblePlayerIds,
                Does.Not.Contain("player.rocket.rory"),
                "A committed player must remain unavailable in every later week.");
            Assert.That(controller.CurrentDate, Is.EqualTo(new System.DateTime(2026, 7, 13)));
            Assert.That(controller.CurrentDemandId, Is.Empty,
                "Week 2 demand stays locked until its actual recruitment mail is opened.");
            Assert.That(controller.ActiveWorkspace, Is.EqualTo("Information"));
            Assert.That(controller.ActiveInformationChannel, Is.EqualTo("Mail"));

            var chineseTexts = controller.GetComponentsInChildren<TextMeshProUGUI>(true)
                .Where(text => text.text.Contains("邮件") || text.text.Contains("期刊"))
                .ToArray();
            Assert.That(chineseTexts, Is.Not.Empty);
        }

        [UnityTest]
        public IEnumerator GameScene_UnusedContentExpiresWithMail_AndSeasonStopsAfterWeekFiftyTwo()
        {
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
            yield return null;

            var controller = Object.FindFirstObjectByType<GameController>();
            controller.OpenMailForTests("mail.w1.rainy");
            controller.OpenMailForTests("mail.w1.carlo");
            Assert.That(controller.VisiblePlayerIds, Does.Contain("player.cousin.carlo"));

            controller.EndWeekForTests();
            controller.EndWeekForTests();
            yield return null;
            Assert.That(controller.CurrentWeek, Is.EqualTo(2));

            controller.EndWeekForTests();
            controller.EndWeekForTests();
            yield return null;
            Assert.That(controller.CurrentWeek, Is.EqualTo(3));
            Assert.That(controller.VisiblePlayerIds, Does.Not.Contain("player.cousin.carlo"));
            Assert.That(controller.CurrentDemandId, Is.Empty);

            controller.OpenMailForTests("mail.expiry.player.cousin.carlo");
            Assert.That(controller.SelectedInformationSubject, Does.Contain("卡洛"));
            controller.OpenMailForTests("mail.expiry.demand.week1.emergency");
            Assert.That(controller.SelectedInformationSubject, Does.Contain("雨城竞技"));

            while (!controller.IsGameComplete)
            {
                controller.EndWeekForTests();
            }

            yield return null;
            Assert.That(controller.CurrentWeek, Is.EqualTo(53));
            Assert.That(controller.IsGameComplete, Is.True);
            Assert.That(controller.CurrentDate, Is.EqualTo(new System.DateTime(2027, 6, 28)));
        }

        [UnityTest]
        public IEnumerator GameScene_CarloFavorPaysImmediately_AndDamagesReputationInWeekFour()
        {
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
            yield return null;

            var controller = Object.FindFirstObjectByType<GameController>();
            controller.OpenMailForTests("mail.w1.rainy");
            controller.OpenMailForTests("mail.w1.carlo");
            controller.ShowAssignmentForTests();
            Assert.That(
                controller.AssignPlayerForTests(
                    controller.VisibleSlotIds[0], "player.cousin.carlo"),
                Is.True);
            controller.EndWeekForTests();
            yield return null;

            Assert.That(controller.CurrentWeek, Is.EqualTo(2));
            Assert.That(controller.CarloFavorAccepted, Is.True);
            Assert.That(controller.CurrentCash, Is.GreaterThanOrEqualTo(850m));

            controller.OpenMailForTests("mail.w2.dockyard");
            controller.OpenMailForTests("mail.w2.milo");
            controller.OpenMailForTests("mail.w2.vic");
            controller.ShowAssignmentForTests();
            Assert.That(controller.AssignPlayerForTests("slot.week2.forward", "player.thermos.vic"), Is.True);
            Assert.That(controller.AssignPlayerForTests("slot.week2.midfield", "player.metronome.milo"), Is.True);
            controller.EndWeekForTests();
            yield return null;

            controller.OpenMailForTests("mail.w3.oldcastle");
            controller.OpenMailForTests("mail.w3.walter");
            controller.OpenMailForTests("mail.w3.luna");
            controller.ShowAssignmentForTests();
            Assert.That(controller.AssignPlayerForTests("slot.week3.defender", "player.wall.walter"), Is.True);
            Assert.That(controller.AssignPlayerForTests("slot.week3.winger", "player.zodiac.luna"), Is.True);
            controller.EndWeekForTests();
            yield return null;

            Assert.That(controller.CurrentWeek, Is.EqualTo(4));
            Assert.That(controller.CarloFavorConsequenceApplied, Is.True);
            var eventLog = controller.transform.Find("GameCanvas/Background/Footer/EventLog")
                .GetComponent<TextMeshProUGUI>().text;
            Assert.That(eventLog, Does.Contain("声望 -3"));
        }
    }
}
