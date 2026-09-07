using System.Collections;
using System.Linq;
using NewPlayerHunter.Persistence;
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
        [SetUp]
        public void DeleteDefaultSaveBeforeSceneLoad()
        {
            new SaveGameService().DeleteProgress();
        }

        [TearDown]
        public void DeleteDefaultSaveAfterTest()
        {
            new SaveGameService().DeleteProgress();
        }

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

            Assert.That(controller.PlayerPoolCount, Is.EqualTo(51));
            Assert.That(controller.DemandPoolCount, Is.EqualTo(35));
            Assert.That(controller.MailPoolCount, Is.EqualTo(193));
            Assert.That(controller.MagazineIssueCount, Is.EqualTo(34));
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
            Assert.That(cards.childCount, Is.EqualTo(51));
            Assert.That(mailItems.childCount, Is.EqualTo(256));
            Assert.That(issueItems.childCount, Is.EqualTo(36));
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
                demandBlock.parent.Find("ResumeBlock/Portrait/Image").GetComponent<RawImage>().texture,
                Is.Not.Null);
            Assert.That(controller.ReadMailCount, Is.EqualTo(3));
            Assert.That(controller.UnlockedPlayerCount, Is.EqualTo(2));
            Assert.That(
                demandBlock.parent.Find("ResumeBlock/Salary").GetComponent<TextMeshProUGUI>().text,
                Does.Contain("/ 周"),
                "The resume block must show the player's weekly salary range.");
            Assert.That(
                demandBlock.parent.Find("ResumeBlock/Career").GetComponent<TextMeshProUGUI>().text,
                Does.Contain("经历"),
                "The resume block must show the player's career history.");
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
                magazineBrowser.Find("PagePanel/CoverLayout/CoverImage/Image").GetComponent<RawImage>().texture,
                Is.Not.Null);
            Assert.That(
                magazineBrowser.Find("PagePanel/CoverLayout/CoverImage/Image")
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
                cards.GetChild(0).Find("Portrait/Image").GetComponent<RawImage>().texture,
                Is.Not.Null);
            Assert.That(
                cards.GetChild(0).Find("Portrait/Image")
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

        [UnityTest]
        public IEnumerator GameScene_SavesAfterEndWeek_AndReloadRestoresProgress()
        {
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
            yield return null;

            var settings = new ES3Settings("nph-playmode-test-save.es3");
            var controller = Object.FindFirstObjectByType<GameController>();
            controller.ConfigureSaveGameService(new SaveGameService(settings));
            controller.RestartGame();
            yield return null;

            Assert.That(controller.CurrentWeek, Is.EqualTo(1));
            Assert.That(controller.HasSavedProgress, Is.False);

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
            Assert.That(controller.HasSavedProgress, Is.True);
            Assert.That(controller.CarloFavorAccepted, Is.True);
            var savedCash = controller.CurrentCash;
            var savedReadCount = controller.ReadMailCount;
            var savedStatus = controller.LastStatus;

            controller.ReloadProgressForTests();
            yield return null;

            Assert.That(controller.CurrentWeek, Is.EqualTo(2),
                "Reload must restore the saved week instead of starting over.");
            Assert.That(controller.CurrentCash, Is.EqualTo(savedCash));
            Assert.That(controller.ReadMailCount, Is.EqualTo(savedReadCount));
            Assert.That(controller.UnlockedPlayerCount, Is.EqualTo(1));
            Assert.That(controller.UnlockedDemandCount, Is.EqualTo(1));
            Assert.That(controller.CarloFavorAccepted, Is.True);
            Assert.That(controller.LastStatus, Is.EqualTo(savedStatus));
            Assert.That(controller.VisiblePlayerIds, Does.Not.Contain("player.cousin.carlo"),
                "Committed players stay out of the pool after a reload.");
            Assert.That(controller.CurrentDemandId, Is.Empty,
                "Week 2 demand remains locked because its mail was not read before saving.");

            controller.RestartGame();
            yield return null;
            Assert.That(controller.CurrentWeek, Is.EqualTo(1));
            Assert.That(controller.HasSavedProgress, Is.False,
                "Restarting must delete the save slot.");

            if (ES3.FileExists(settings))
            {
                ES3.DeleteFile(settings);
            }
        }

        [UnityTest]
        public IEnumerator GameScene_ResultMailArrivesWithClubEvaluation_AndSurvivesReload()
        {
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
            yield return null;

            var settings = new ES3Settings("nph-playmode-test-save.es3");
            var controller = Object.FindFirstObjectByType<GameController>();
            controller.ConfigureSaveGameService(new SaveGameService(settings));
            controller.RestartGame();
            yield return null;

            var overlay = controller.transform.Find("GameCanvas/WeekTransitionOverlay");
            Assert.That(overlay, Is.Not.Null,
                "The week transition overlay must be authored in the scene.");
            Assert.That(overlay.Find("Text"), Is.Not.Null);
            var overlayGroup = overlay.GetComponent<CanvasGroup>();
            Assert.That(overlayGroup.alpha, Is.EqualTo(0f));
            Assert.That(overlayGroup.blocksRaycasts, Is.False);

            controller.OpenMailForTests("mail.w1.rainy");
            controller.OpenMailForTests("mail.w1.carlo");
            controller.ShowAssignmentForTests();
            Assert.That(
                controller.AssignPlayerForTests(
                    controller.VisibleSlotIds[0], "player.cousin.carlo"),
                Is.True);

            var guard = 0;
            while (controller.ResultMailCount == 0 && guard < 3)
            {
                controller.EndWeekForTests();
                guard++;
            }

            yield return null;
            Assert.That(controller.ResultMailCount, Is.EqualTo(1),
                "A committed placement must produce a club feedback mail within two weeks.");

            var resultMailId = controller.VisibleMailIds
                .First(id => id.StartsWith("result.w", System.StringComparison.Ordinal));
            controller.OpenMailForTests(resultMailId);
            yield return null;
            Assert.That(controller.SelectedInformationSubject, Does.Contain("试训反馈"));
            Assert.That(controller.SelectedInformationSubject, Does.Contain("卡洛"));
            var bodyText = controller.transform
                .Find("GameCanvas/Background/InformationWorkspace/MailBrowser/DetailPanel/Body")
                .GetComponent<TextMeshProUGUI>().text;
            Assert.That(bodyText, Does.Contain("雨城竞技"),
                "The feedback mail must name the evaluating club.");
            Assert.That(bodyText, Does.Contain("€"),
                "The feedback mail must state the payment.");
            Assert.That(controller.ReadMailCount, Is.EqualTo(3));

            controller.SaveProgressForTests();
            controller.ReloadProgressForTests();
            yield return null;
            Assert.That(controller.ResultMailCount, Is.EqualTo(1),
                "Result mails must be regenerated from delivered outcomes after a reload.");
            Assert.That(controller.VisibleMailIds, Does.Contain(resultMailId));
            Assert.That(controller.ReadMailCount, Is.EqualTo(3),
                "Result mail read state must persist through the save.");

            if (ES3.FileExists(settings))
            {
                ES3.DeleteFile(settings);
            }
        }

        [UnityTest]
        public IEnumerator GameScene_DemandListSwitchesBetweenEligibleDemands()
        {
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
            yield return null;

            var controller = Object.FindFirstObjectByType<GameController>();
            for (var week = 1; week < 7; week++)
            {
                controller.EndWeekForTests();
            }

            yield return null;
            Assert.That(controller.CurrentWeek, Is.EqualTo(7));

            controller.OpenMailForTests("mail.w7.pub");
            controller.OpenMailForTests("mail.w7.relegation");
            yield return null;

            var demandItems = controller.transform.Find(
                "GameCanvas/Background/AssignmentWorkspace/DemandPanel/DemandList/Viewport/DemandListItems");
            Assert.That(demandItems, Is.Not.Null);
            Assert.That(demandItems.childCount, Is.EqualTo(8));
            var activeItems = demandItems.Cast<Transform>()
                .Where(item => item.gameObject.activeSelf)
                .ToArray();
            Assert.That(activeItems.Length, Is.EqualTo(2),
                "Both week 7 recruitment mails unlock one demand list entry each.");
            Assert.That(
                activeItems[0].Find("Title").GetComponent<TextMeshProUGUI>().text,
                Does.Contain("黑麦酒馆"));
            Assert.That(
                activeItems[1].Find("Title").GetComponent<TextMeshProUGUI>().text,
                Does.Contain("灯塔港"));
            Assert.That(
                activeItems[1].Find("Meta").GetComponent<TextMeshProUGUI>().text,
                Does.Contain("2600"));
            Assert.That(
                activeItems[1].Find("Meta").GetComponent<TextMeshProUGUI>().text,
                Does.Contain("1 槽"));
            var firstTitle = activeItems[0].Find("Title").GetComponent<TextMeshProUGUI>();
            controller.ShowAssignmentForTests();
            yield return null;
            firstTitle.ForceMeshUpdate();
            Assert.That(firstTitle.textInfo.characterCount, Is.GreaterThan(0),
                "Demand list title must fit its band and actually render glyphs.");
            var slotRequirement = controller.transform.Find(
                    "GameCanvas/Background/AssignmentWorkspace/DemandPanel/Slots/DemandSlot01/Requirement")
                .GetComponent<TextMeshProUGUI>();
            slotRequirement.ForceMeshUpdate();
            Assert.That(slotRequirement.textInfo.characterCount, Is.GreaterThan(0),
                "Demand slot requirement text must fit its band and actually render glyphs.");

            var demandTitle = controller.transform.Find(
                    "GameCanvas/Background/AssignmentWorkspace/DemandPanel/DemandTitle")
                .GetComponent<TextMeshProUGUI>();
            Assert.That(controller.CurrentDemandId, Is.EqualTo("demand.w7.pub"),
                "The earliest deadline becomes the default selected demand.");
            Assert.That(controller.SelectedDemandId, Is.EqualTo("demand.w7.pub"));
            Assert.That(demandTitle.text, Does.Contain("黑麦酒馆"));
            Assert.That(controller.VisibleSlotIds,
                Is.EqualTo(new[] { "slot.w7.pub.forward" }));
            var selectedColor = activeItems[0].GetComponent<Image>().color;
            Assert.That(activeItems[1].GetComponent<Image>().color,
                Is.Not.EqualTo(selectedColor));

            controller.SelectDemand("demand.w7.relegation");
            yield return null;
            Assert.That(controller.SelectedDemandId, Is.EqualTo("demand.w7.relegation"));
            Assert.That(controller.CurrentDemandId, Is.EqualTo("demand.w7.relegation"));
            Assert.That(demandTitle.text, Does.Contain("灯塔港"));
            Assert.That(controller.VisibleSlotIds,
                Is.EqualTo(new[] { "slot.w7.relegation.defender" }));
            Assert.That(activeItems[1].GetComponent<Image>().color,
                Is.EqualTo(selectedColor),
                "The selected demand list entry stays highlighted.");

            controller.OpenMailForTests("mail.w7.ferreira.antonio");
            Assert.That(controller.VisiblePlayerIds,
                Does.Contain("player.ferreira.antonio"));
            Assert.That(
                controller.AssignPlayerForTests(
                    "slot.w7.relegation.defender", "player.ferreira.antonio"),
                Is.True);
            Assert.That(controller.VisiblePlayerIds,
                Does.Not.Contain("player.ferreira.antonio"));

            controller.HandleSlotClicked("slot.w7.relegation.defender");
            yield return null;
            Assert.That(controller.VisiblePlayerIds,
                Does.Contain("player.ferreira.antonio"),
                "Removing an assignment still works after switching demands.");
            Assert.That(
                controller.AssignPlayerForTests(
                    "slot.w7.relegation.defender", "player.ferreira.antonio"),
                Is.True);

            controller.EndWeekForTests();
            yield return null;
            Assert.That(controller.CurrentWeek, Is.EqualTo(8),
                "A filled required slot commits without the empty confirmation.");
            Assert.That(controller.CurrentDemandId, Is.Empty,
                "The pub demand expired and the relegation demand was committed.");
            Assert.That(controller.SelectedDemandId, Is.Empty);
            Assert.That(
                demandItems.Cast<Transform>().Count(item => item.gameObject.activeSelf),
                Is.Zero,
                "The demand list hides completely when no demand is eligible.");
            Assert.That(demandTitle.text, Does.Contain("当前没有有效招聘"));
        }

        [UnityTest]
        public IEnumerator GameScene_SelectedDemandSurvivesSaveReload()
        {
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
            yield return null;

            var settings = new ES3Settings("nph-playmode-test-save.es3");
            var controller = Object.FindFirstObjectByType<GameController>();
            controller.ConfigureSaveGameService(new SaveGameService(settings));
            controller.RestartGame();
            yield return null;

            for (var week = 1; week < 7; week++)
            {
                controller.EndWeekForTests();
            }

            controller.OpenMailForTests("mail.w7.pub");
            controller.OpenMailForTests("mail.w7.relegation");
            controller.SelectDemand("demand.w7.relegation");
            controller.SaveProgressForTests();
            controller.ReloadProgressForTests();
            yield return null;

            Assert.That(controller.CurrentWeek, Is.EqualTo(7));
            Assert.That(controller.SelectedDemandId, Is.EqualTo("demand.w7.relegation"),
                "The selected demand must survive a save/reload cycle.");
            Assert.That(controller.CurrentDemandId, Is.EqualTo("demand.w7.relegation"));
            Assert.That(controller.VisibleSlotIds,
                Is.EqualTo(new[] { "slot.w7.relegation.defender" }));
            var demandTitle = controller.transform.Find(
                    "GameCanvas/Background/AssignmentWorkspace/DemandPanel/DemandTitle")
                .GetComponent<TextMeshProUGUI>();
            Assert.That(demandTitle.text, Does.Contain("灯塔港"));
            var demandItems = controller.transform.Find(
                "GameCanvas/Background/AssignmentWorkspace/DemandPanel/DemandList/Viewport/DemandListItems");
            Assert.That(
                demandItems.Cast<Transform>().Count(item => item.gameObject.activeSelf),
                Is.EqualTo(2));
            Assert.That(
                demandItems.GetChild(1).GetComponent<Image>().color,
                Is.Not.EqualTo(demandItems.GetChild(0).GetComponent<Image>().color),
                "Only the restored selection stays highlighted after reload.");

            if (ES3.FileExists(settings))
            {
                ES3.DeleteFile(settings);
            }
        }
    }
}
