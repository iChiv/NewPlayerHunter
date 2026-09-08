using System;
using System.Text;
using NewPlayerHunter.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NewPlayerHunter.Editor
{
    public static class FormalGameSceneBuilder
    {
        private const string ScenePath = "Assets/Game/Scenes/Game.unity";
        private const string CatalogPath =
            "Assets/Game/Content/M1ContentCatalog.asset";
        private const string FontPath =
            "Assets/Game/Fonts/NewPlayerHunter Chinese Dynamic.asset";
        private const string PortraitAtlasPath =
            "Assets/Game/Art/Generated/PlayerPortraitAtlas.png";
        private const string MagazineAtlasPath =
            "Assets/Game/Art/Generated/MagazineCoverAtlas.png";
        private const string IllustrationAtlasPath =
            "Assets/Game/Art/Generated/MagazineIllustrationAtlas.png";

        private static readonly Color Background =
            new Color(0.035f, 0.055f, 0.075f, 1f);
        private static readonly Color Panel =
            new Color(0.065f, 0.095f, 0.125f, 0.98f);
        private static readonly Color PanelLight =
            new Color(0.09f, 0.13f, 0.17f, 1f);
        private static readonly Color Accent =
            new Color(0.25f, 0.9f, 0.53f, 1f);
        private static readonly Color Muted =
            new Color(0.62f, 0.7f, 0.76f, 1f);
        private static readonly Color Warning =
            new Color(1f, 0.72f, 0.25f, 1f);
        private static readonly Color MagazinePaper =
            new Color(0.91f, 0.88f, 0.77f, 1f);
        private static readonly Color MagazineInk =
            new Color(0.10f, 0.09f, 0.07f, 1f);
        private static readonly Color MagazineRed =
            new Color(0.65f, 0.12f, 0.09f, 1f);

        private static TMP_FontAsset _font;

        [MenuItem("Tools/New Player Hunter/Rebuild Game Scene")]
        public static void RebuildGameScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            foreach (var root in scene.GetRootGameObjects())
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            _font = EnsureChineseFontAsset();
            var catalog = EnsureContentCatalog();
            PrewarmChineseFontAsset(_font, catalog);
            CreateCamera();
            CreateGlobalLight();
            CreateEventSystem();

            var gameRoot = new GameObject("WeeklyGame", typeof(GameController));
            gameRoot.GetComponent<GameController>().ConfigureContentCatalog(catalog);
            BuildCanvas(gameRoot.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            Selection.activeGameObject = gameRoot;
            AssetDatabase.SaveAssets();
            Debug.Log(
                "[NewPlayerHunter] 正式 Game 场景已重建：中文邮件、读信解锁、分配工作台与多页电子期刊均为 Scene 静态对象。");
        }

        private static GameContentCatalog EnsureContentCatalog()
        {
            EnsureFolder("Assets/Game", "Content");
            var catalog = AssetDatabase.LoadAssetAtPath<GameContentCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<GameContentCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            catalog.PopulateM1Defaults();
            catalog.ConfigureArt(
                AssetDatabase.LoadAssetAtPath<Texture2D>(PortraitAtlasPath),
                AssetDatabase.LoadAssetAtPath<Texture2D>(MagazineAtlasPath),
                AssetDatabase.LoadAssetAtPath<Texture2D>(IllustrationAtlasPath));
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        private static TMP_FontAsset EnsureChineseFontAsset()
        {
            EnsureFolder("Assets/Game", "Fonts");
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (existing != null)
            {
                return existing;
            }

            TMP_FontAsset fontAsset = null;
            var families = new[] { "Microsoft YaHei UI", "Microsoft YaHei" };
            foreach (var family in families)
            {
                try
                {
                    fontAsset = TMP_FontAsset.CreateFontAsset(family, "Regular", 90);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(
                        $"[NewPlayerHunter] 无法从系统字体 {family} 创建 TMP 字体：{exception.Message}");
                }

                if (fontAsset != null)
                {
                    break;
                }
            }

            if (fontAsset == null)
            {
                var fallback = Resources.Load<TMP_FontAsset>(
                    "Fonts & Materials/LiberationSans SDF");
                return fallback == null ? TMP_Settings.defaultFontAsset : fallback;
            }

            fontAsset.name = "NewPlayerHunter Chinese Dynamic";
            fontAsset.atlasPopulationMode = AtlasPopulationMode.DynamicOS;
            AssetDatabase.CreateAsset(fontAsset, FontPath);
            if (fontAsset.material != null &&
                !AssetDatabase.Contains(fontAsset.material))
            {
                fontAsset.material.name = fontAsset.name + " Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            foreach (var atlas in fontAsset.atlasTextures)
            {
                if (atlas != null && !AssetDatabase.Contains(atlas))
                {
                    atlas.name = fontAsset.name + " Atlas";
                    AssetDatabase.AddObjectToAsset(atlas, fontAsset);
                }
            }

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            return fontAsset;
        }

        private static void PrewarmChineseFontAsset(
            TMP_FontAsset fontAsset,
            GameContentCatalog catalog)
        {
            if (fontAsset == null ||
                fontAsset.atlasPopulationMode != AtlasPopulationMode.DynamicOS)
            {
                return;
            }

            var characters = new StringBuilder(
                "新星猎手收件箱期刊球员分配第周现金应收声望重新开始信息筛选中心先读邮件再交叉判断订阅打开邮件才算阅读请先再安排本周工作招聘需求简历私人请托固定委托价付款截止位置必需可选结束本周上一页下一页已读未读门将中卫翼卫中场边锋前锋可靠性较高中等较低未经核实菜单继续游戏存档将被删除，再点一次确认语言：中文退出返回");
            foreach (var player in catalog.Players)
            {
                Append(characters, player.displayName);
                Append(characters, player.biography);
                Append(characters, player.publicClaim);
                Append(characters, player.publicEvidence);
                Append(characters, player.expiryMailSubject);
                Append(characters, player.expiryMailBody);
            }

            foreach (var demand in catalog.Demands)
            {
                Append(characters, demand.clubDisplayName);
                Append(characters, demand.clubStanding);
                Append(characters, demand.clubBestAchievement);
                Append(characters, demand.clubProfile);
                Append(characters, demand.title);
                Append(characters, demand.description);
                Append(characters, demand.paymentTerms);
                Append(characters, demand.expiryMailSubject);
                Append(characters, demand.expiryMailBody);
            }

            foreach (var mail in catalog.Mails)
            {
                Append(characters, mail.sender);
                Append(characters, mail.subject);
                Append(characters, mail.receivedTime);
                Append(characters, mail.preview);
                Append(characters, mail.body);
                Append(characters, mail.sourceNote);
                Append(characters, mail.privateOfferTerms);
                Append(characters, mail.privateTargetClubRequirement);
                Append(characters, mail.privateRiskNote);
            }

            foreach (var issue in catalog.MagazineIssues)
            {
                Append(characters, issue.publicationName);
                Append(characters, issue.issueTitle);
                characters.Append(issue.issueNumber);
                foreach (var page in issue.pages)
                {
                    Append(characters, page.kicker);
                    Append(characters, page.headline);
                    Append(characters, page.deck);
                    Append(characters, page.bodyLeft);
                    Append(characters, page.bodyRight);
                    Append(characters, page.pullQuote);
                    Append(characters, page.sidebarTitle);
                    Append(characters, page.sidebarBody);
                }
            }

            fontAsset.isMultiAtlasTexturesEnabled = true;
            if (!fontAsset.TryAddCharacters(
                    characters.ToString(),
                    out var missingCharacters) &&
                !string.IsNullOrEmpty(missingCharacters))
            {
                Debug.LogWarning(
                    "[NewPlayerHunter] 中文字体仍缺少字符：" + missingCharacters);
            }

            if (fontAsset.material != null &&
                !AssetDatabase.Contains(fontAsset.material))
            {
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            foreach (var atlas in fontAsset.atlasTextures)
            {
                if (atlas != null && !AssetDatabase.Contains(atlas))
                {
                    AssetDatabase.AddObjectToAsset(atlas, fontAsset);
                }
            }

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
        }

        private static void Append(StringBuilder builder, LocalizedText text)
        {
            if (text != null)
            {
                builder.Append(text.chineseSimplified);
            }
        }

        private static void EnsureFolder(string parent, string name)
        {
            var path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject(
                "Main Camera",
                typeof(Camera),
                typeof(AudioListener),
                typeof(UniversalAdditionalCameraData));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;
        }

        private static void CreateGlobalLight()
        {
            var lightObject = new GameObject("Global Light 2D", typeof(Light2D));
            var light = lightObject.GetComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;
        }

        private static void CreateEventSystem()
        {
            var eventObject = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule));
            eventObject.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static void BuildCanvas(Transform gameRoot)
        {
            var canvasObject = new GameObject(
                "GameCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(gameRoot, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var background = CreatePanel(
                "Background", canvasObject.transform, Background, Vector2.zero, Vector2.one);
            BuildHeader(background);
            var assignmentWorkspace = CreateRect(
                "AssignmentWorkspace", background, Vector2.zero, Vector2.one);
            assignmentWorkspace.gameObject.AddComponent<CanvasGroup>();
            BuildDemandPanel(assignmentWorkspace);
            BuildPlayersPanel(assignmentWorkspace);
            var informationWorkspace = BuildInformationWorkspace(background);
            informationWorkspace.gameObject.AddComponent<CanvasGroup>();
            BuildFooter(background);
            BuildDragGhost(canvasObject.transform);
            BuildWeekTransitionOverlay(canvasObject.transform);
            BuildMainMenuOverlay(canvasObject.transform);
            assignmentWorkspace.gameObject.SetActive(false);
            informationWorkspace.gameObject.SetActive(true);
        }

        private static void BuildWeekTransitionOverlay(Transform parent)
        {
            var overlay = CreatePanel(
                "WeekTransitionOverlay", parent, Color.black, Vector2.zero, Vector2.one);
            overlay.GetComponent<Image>().raycastTarget = true;
            var group = overlay.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            CreateText(
                "Text", overlay, Ui("status.weekSettling"), 48f, Color.white,
                TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
        }

        private static void BuildMainMenuOverlay(Transform parent)
        {
            var overlay = CreatePanel(
                "MainMenuOverlay", parent,
                new Color(0.02f, 0.03f, 0.045f, 0.92f), Vector2.zero, Vector2.one);
            overlay.GetComponent<Image>().raycastTarget = true;
            var group = overlay.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            var panel = CreatePanel(
                "Panel", overlay, Panel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            panel.sizeDelta = new Vector2(520f, 620f);
            var outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.25f, 0.32f, 0.38f, 1f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            CreateText(
                "Title", panel, Ui("menu.title"), 44f, Accent,
                TextAlignmentOptions.Center,
                new Vector2(0f, 0.84f), new Vector2(1f, 0.97f));
            CreateButton(
                "ContinueButton", panel, Ui("menu.continue"),
                new Vector2(0.14f, 0.68f), new Vector2(0.86f, 0.79f), Accent, Background);
            CreateButton(
                "NewGameButton", panel, Ui("menu.newGame"),
                new Vector2(0.14f, 0.53f), new Vector2(0.86f, 0.64f), PanelLight, Color.white);
            CreateButton(
                "LanguageButton", panel, Ui("menu.language"),
                new Vector2(0.14f, 0.38f), new Vector2(0.86f, 0.49f), PanelLight, Color.white);
            CreateButton(
                "QuitButton", panel, Ui("menu.quit"),
                new Vector2(0.14f, 0.23f), new Vector2(0.86f, 0.34f), PanelLight, Warning);
            var resumeButton = CreateButton(
                "ResumeButton", panel, Ui("menu.resume"),
                new Vector2(0.14f, 0.08f), new Vector2(0.86f, 0.19f), Accent, Background);
            resumeButton.gameObject.SetActive(false);
        }

        private static string Ui(string key)
        {
            return UiStrings.Get(key, GameLanguage.ChineseSimplified);
        }

        private static void BuildHeader(RectTransform parent)
        {
            var header = CreatePanel(
                "Header", parent, Panel,
                new Vector2(0.015f, 0.905f), new Vector2(0.985f, 0.985f));
            CreateButton(
                "InformationTabButton", header, Ui("header.informationTab"),
                new Vector2(0.02f, 0.16f), new Vector2(0.17f, 0.84f), Accent, Background);
            CreateButton(
                "AssignmentTabButton", header, Ui("header.assignmentTab"),
                new Vector2(0.18f, 0.16f), new Vector2(0.30f, 0.84f), PanelLight, Color.white);
            CreateText(
                "Week", header, Ui("header.weekPlaceholder"), 18f, Accent,
                TextAlignmentOptions.Center,
                new Vector2(0.305f, 0f), new Vector2(0.64f, 1f));
            CreateText(
                "Economy", header, Ui("header.economyPlaceholder"), 18f, Color.white,
                TextAlignmentOptions.MidlineRight,
                new Vector2(0.64f, 0f), new Vector2(0.89f, 1f));
            CreateButton(
                "MenuButton", header, Ui("header.menu"),
                new Vector2(0.9f, 0.18f), new Vector2(0.98f, 0.82f), PanelLight, Color.white);
        }

        private static void BuildDemandPanel(RectTransform parent)
        {
            var panel = CreatePanel(
                "DemandPanel", parent, Panel,
                new Vector2(0.015f, 0.205f), new Vector2(0.485f, 0.89f));
            CreateText(
                "DemandLabel", panel, Ui("demand.label"), 18f, Accent,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.04f, 0.9f), new Vector2(0.96f, 0.98f));
            var scrollObject = new GameObject(
                "DemandList", typeof(RectTransform), typeof(ScrollRect));
            scrollObject.transform.SetParent(panel, false);
            var scrollTransform = scrollObject.GetComponent<RectTransform>();
            Stretch(scrollTransform,
                new Vector2(0.04f, 0.60f), new Vector2(0.96f, 0.88f));
            var viewport = CreateRect(
                "Viewport", scrollTransform, Vector2.zero, Vector2.one);
            viewport.gameObject.AddComponent<RectMask2D>();
            var list = CreateRect(
                "DemandListItems", viewport, new Vector2(0f, 1f), new Vector2(1f, 1f));
            list.pivot = new Vector2(0.5f, 1f);
            list.sizeDelta = new Vector2(0f, 8 * 72f);
            list.anchoredPosition = Vector2.zero;
            var scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = list;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 26f;
            for (var index = 0; index < 8; index++)
            {
                BuildDemandListItem(list, index);
            }

            CreateText(
                "DemandTitle", panel, Ui("demand.titlePlaceholder"), 30f, Color.white,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.04f, 0.48f), new Vector2(0.96f, 0.58f));
            CreateText(
                "DemandBody", panel, Ui("demand.bodyPlaceholder"), 16f, Muted,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.04f, 0.32f), new Vector2(0.96f, 0.48f));
            var slots = CreateRect(
                "Slots", panel,
                new Vector2(0.04f, 0.13f), new Vector2(0.96f, 0.31f));
            for (var index = 0; index < 2; index++)
            {
                BuildDemandSlot(slots, index);
            }

            CreateText(
                "Selection", panel, Ui("demand.selectionPlaceholder"), 18f, Warning,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.04f, 0.03f), new Vector2(0.96f, 0.14f));
        }

        private static void BuildDemandListItem(RectTransform parent, int index)
        {
            const float height = 64f;
            const float gap = 8f;
            var item = CreateTopItem(
                $"DemandListItem{index + 1:00}", parent, PanelLight,
                height, -index * (height + gap));
            var image = item.GetComponent<Image>();
            var button = item.gameObject.AddComponent<Button>();
            ConfigureButtonColors(button, image, PanelLight);
            CreateText(
                "Title", item, Ui("demand.itemTitlePlaceholder"), 14f, Color.white,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.97f));
            CreateText(
                "Meta", item, Ui("demand.itemMetaPlaceholder"), 12f, Muted,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.45f));
        }

        private static void BuildDemandSlot(RectTransform parent, int index)
        {
            const float height = 60f;
            const float gap = 6f;
            var slot = CreateTopItem(
                $"DemandSlot{index + 1:00}", parent, PanelLight,
                height, -index * (height + gap));
            var outline = slot.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.25f, 0.32f, 0.38f, 1f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            slot.gameObject.AddComponent<DemandSlotDropTarget>();
            CreateText(
                "Requirement", slot, Ui("slot.requirementPlaceholder"), 15f, Accent,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.035f, 0.48f), new Vector2(0.965f, 0.98f));
            CreateText(
                "Assignment", slot, Ui("slot.hint"), 14f, Muted,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.035f, 0.02f), new Vector2(0.965f, 0.48f));
        }

        private static void BuildPlayersPanel(RectTransform parent)
        {
            var panel = CreatePanel(
                "PlayersPanel", parent, Panel,
                new Vector2(0.5f, 0.205f), new Vector2(0.985f, 0.89f));
            CreateText(
                "PlayersLabel", panel, Ui("players.labelPlaceholder"), 17f, Accent,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.035f, 0.91f), new Vector2(0.965f, 0.985f));
            var scrollObject = new GameObject(
                "PlayerScroll", typeof(RectTransform), typeof(ScrollRect));
            scrollObject.transform.SetParent(panel, false);
            var scrollTransform = scrollObject.GetComponent<RectTransform>();
            Stretch(scrollTransform,
                new Vector2(0.035f, 0.035f), new Vector2(0.965f, 0.9f));
            var viewport = CreateRect(
                "Viewport", scrollTransform, Vector2.zero, Vector2.one);
            viewport.gameObject.AddComponent<RectMask2D>();
            var cards = CreateRect(
                "PlayerCards", viewport, new Vector2(0f, 1f), new Vector2(1f, 1f));
            cards.pivot = new Vector2(0.5f, 1f);
            cards.sizeDelta = new Vector2(0f, 51 * 84f);
            cards.anchoredPosition = Vector2.zero;
            var scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = cards;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 26f;
            for (var index = 0; index < 51; index++)
            {
                BuildPlayerCard(cards, index);
            }
        }

        private static void BuildPlayerCard(RectTransform parent, int index)
        {
            const float height = 76f;
            const float gap = 8f;
            var card = CreateTopItem(
                $"PlayerCard{index + 1:00}", parent, PanelLight,
                height, -index * (height + gap));
            var group = card.gameObject.AddComponent<CanvasGroup>();
            group.interactable = true;
            group.blocksRaycasts = true;
            card.gameObject.AddComponent<PlayerCardDragHandler>();
            CreateRawImage("Portrait", card,
                new Vector2(0.015f, 0.08f), new Vector2(0.105f, 0.92f));
            CreateText(
                "Name", card, Ui("playerCard.namePlaceholder"), 19f, Color.white,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.12f, 0.4f), new Vector2(0.40f, 0.98f));
            CreateText(
                "Position", card, Ui("playerCard.positionPlaceholder"), 14f, Accent,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.12f, 0.02f), new Vector2(0.40f, 0.42f));
            CreateText(
                "Claim", card, Ui("playerCard.claimPlaceholder"), 15f, Muted,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.42f, 0.08f), new Vector2(0.975f, 0.92f));
        }

        private static RectTransform BuildInformationWorkspace(RectTransform parent)
        {
            var workspace = CreateRect(
                "InformationWorkspace", parent, Vector2.zero, Vector2.one);
            var toolbar = CreatePanel(
                "Toolbar", workspace, Panel,
                new Vector2(0.015f, 0.805f), new Vector2(0.985f, 0.89f));
            CreateText(
                "BrowserLabel", toolbar, Ui("info.browserLabel"), 18f, Accent,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.02f, 0f), new Vector2(0.40f, 1f));
            CreateButton(
                "MailFilterButton", toolbar, Ui("info.mailFilter"),
                new Vector2(0.41f, 0.16f), new Vector2(0.53f, 0.84f), Accent, Background);
            CreateButton(
                "SubscriptionFilterButton", toolbar, Ui("info.subscriptionFilter"),
                new Vector2(0.54f, 0.16f), new Vector2(0.68f, 0.84f), PanelLight, Color.white);
            CreateText(
                "Counter", toolbar, Ui("info.counterPlaceholder"), 16f, Muted,
                TextAlignmentOptions.MidlineRight,
                new Vector2(0.70f, 0f), new Vector2(0.98f, 1f));

            var mailBrowser = BuildMailBrowser(workspace);
            var magazineBrowser = BuildMagazineBrowser(workspace);
            mailBrowser.gameObject.SetActive(true);
            magazineBrowser.gameObject.SetActive(false);
            return workspace;
        }

        private static RectTransform BuildMailBrowser(RectTransform parent)
        {
            var browser = CreateRect(
                "MailBrowser", parent,
                new Vector2(0f, 0.195f), new Vector2(1f, 0.8f));
            var listPanel = CreatePanel(
                "MessageListPanel", browser, Panel,
                new Vector2(0.015f, 0f), new Vector2(0.42f, 0.965f));
            CreateText(
                "ListLabel", listPanel, Ui("mail.listLabel"), 16f, Accent,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.035f, 0.91f), new Vector2(0.965f, 0.985f));
            var scrollObject = new GameObject(
                "MessageScroll", typeof(RectTransform), typeof(ScrollRect));
            scrollObject.transform.SetParent(listPanel, false);
            var scrollRectTransform = scrollObject.GetComponent<RectTransform>();
            Stretch(scrollRectTransform,
                new Vector2(0.035f, 0.04f), new Vector2(0.965f, 0.9f));
            var viewport = CreateRect(
                "Viewport", scrollRectTransform, Vector2.zero, Vector2.one);
            viewport.gameObject.AddComponent<RectMask2D>();
            var list = CreateRect(
                "MessageList", viewport, new Vector2(0f, 1f), new Vector2(1f, 1f));
            list.pivot = new Vector2(0.5f, 1f);
            list.sizeDelta = new Vector2(0f, 256 * 108f);
            list.anchoredPosition = Vector2.zero;
            var scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = list;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f;
            for (var index = 0; index < 256; index++)
            {
                BuildMailListItem(list, index);
            }

            var detail = CreatePanel(
                "DetailPanel", browser, Panel,
                new Vector2(0.435f, 0f), new Vector2(0.985f, 0.965f));
            CreateText(
                "ReadingLabel", detail, Ui("mail.readingLabel"), 15f, Accent,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.035f, 0.92f), new Vector2(0.965f, 0.985f));
            CreateText(
                "Sender", detail, Ui("mail.emptySender"), 16f, Muted,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.035f, 0.845f), new Vector2(0.965f, 0.92f));
            CreateText(
                "Subject", detail, Ui("mail.emptySubject"), 27f, Color.white,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.035f, 0.72f), new Vector2(0.965f, 0.85f));
            CreateText(
                "Meta", detail, Ui("mail.emptyMeta"), 14f, Warning,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.035f, 0.665f), new Vector2(0.965f, 0.72f));
            CreateText(
                "Body", detail,
                Ui("mail.emptyBody"),
                18f, Color.white, TextAlignmentOptions.TopLeft,
                new Vector2(0.035f, 0.53f), new Vector2(0.965f, 0.66f));
            BuildDemandMailBlock(detail);
            BuildResumeMailBlock(detail);
            BuildPrivateOfferMailBlock(detail);
            return browser;
        }

        private static void BuildMailListItem(RectTransform parent, int index)
        {
            const float height = 100f;
            const float gap = 8f;
            var item = CreateTopItem(
                $"MessageItem{index + 1:00}", parent, PanelLight,
                height, -index * (height + gap));
            var image = item.GetComponent<Image>();
            var button = item.gameObject.AddComponent<Button>();
            ConfigureButtonColors(button, image, PanelLight);
            var avatar = CreatePanel(
                "Avatar", item, new Color(0.16f, 0.34f, 0.29f, 1f),
                new Vector2(0.02f, 0.15f), new Vector2(0.13f, 0.85f));
            CreateText(
                "Initials", avatar, Ui("mail.avatarPlaceholder"), 19f, Color.white,
                TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
            CreateText(
                "Sender", item, Ui("mail.itemSenderPlaceholder"), 13f, Accent,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.15f, 0.74f), new Vector2(0.49f, 0.98f));
            CreateText(
                "Timestamp", item, Ui("mail.itemTimestampPlaceholder"), 12f, Muted,
                TextAlignmentOptions.MidlineRight,
                new Vector2(0.49f, 0.74f), new Vector2(0.84f, 0.98f));
            CreatePanel(
                "ReadDot", item, Warning,
                new Vector2(0.855f, 0.79f), new Vector2(0.88f, 0.91f));
            CreateText(
                "ReadState", item, Ui("mail.unread"), 12f, Warning,
                TextAlignmentOptions.MidlineRight,
                new Vector2(0.885f, 0.74f), new Vector2(0.985f, 0.98f));
            CreateText(
                "Subject", item, Ui("mail.itemSubjectPlaceholder"), 15f, Color.white,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.15f, 0.40f), new Vector2(0.985f, 0.75f));
            CreateText(
                "Preview", item, Ui("mail.itemPreviewPlaceholder"), 12f, Muted,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.15f, 0.05f), new Vector2(0.985f, 0.41f));
        }

        private static void BuildDemandMailBlock(RectTransform parent)
        {
            var block = CreatePanel(
                "DemandBlock", parent, new Color(0.08f, 0.16f, 0.13f, 1f),
                new Vector2(0.035f, 0.055f), new Vector2(0.965f, 0.51f));
            CreateText("Title", block, Ui("mail.demandBlock.title"), 17f, Accent,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.025f, 0.84f), new Vector2(0.975f, 0.98f));
            CreateText("ClubProfile", block, Ui("mail.demandBlock.clubProfilePlaceholder"), 15f, Color.white,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.025f, 0.64f), new Vector2(0.975f, 0.84f));
            CreateText("Slots", block, Ui("mail.demandBlock.slotsPlaceholder"), 17f, Color.white,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.025f, 0.48f), new Vector2(0.975f, 0.64f));
            CreateText("Deadline", block, Ui("mail.demandBlock.deadlinePlaceholder"), 16f, Muted,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.025f, 0.31f), new Vector2(0.68f, 0.48f));
            CreateText("Price", block, Ui("mail.demandBlock.pricePlaceholder"), 21f, Warning,
                TextAlignmentOptions.MidlineRight,
                new Vector2(0.68f, 0.31f), new Vector2(0.975f, 0.48f));
            CreateText("Payment", block, Ui("mail.demandBlock.paymentPlaceholder"), 16f, Color.white,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.025f, 0.06f), new Vector2(0.975f, 0.30f));
            block.gameObject.SetActive(false);
        }

        private static void BuildResumeMailBlock(RectTransform parent)
        {
            var block = CreatePanel(
                "ResumeBlock", parent, new Color(0.10f, 0.13f, 0.18f, 1f),
                new Vector2(0.035f, 0.035f), new Vector2(0.965f, 0.51f));
            CreateText("Title", block, Ui("mail.resumeBlock.title"), 15f, Accent,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.025f, 0.92f), new Vector2(0.975f, 1.00f));
            CreateRawImage("Portrait", block,
                new Vector2(0.025f, 0.60f), new Vector2(0.18f, 0.90f));
            CreateText("Player", block, Ui("mail.resumeBlock.playerPlaceholder"), 22f, Color.white,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.20f, 0.76f), new Vector2(0.62f, 0.90f));
            CreateText("Position", block, Ui("mail.resumeBlock.positionPlaceholder"), 15f, Warning,
                TextAlignmentOptions.MidlineRight,
                new Vector2(0.62f, 0.76f), new Vector2(0.975f, 0.90f));
            CreateText("Salary", block, Ui("mail.resumeBlock.salaryPlaceholder"), 16f, Warning,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.20f, 0.61f), new Vector2(0.975f, 0.76f));
            CreateText("Biography", block, Ui("mail.resumeBlock.biographyPlaceholder"), 13f, Muted,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.20f, 0.45f), new Vector2(0.975f, 0.61f));
            CreateText("Career", block, Ui("mail.resumeBlock.careerPlaceholder"), 13f, Muted,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.025f, 0.30f), new Vector2(0.975f, 0.45f));
            CreateText("Claim", block, Ui("mail.resumeBlock.claimPlaceholder"), 13f, Color.white,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.025f, 0.17f), new Vector2(0.975f, 0.30f));
            CreateText("Evidence", block, Ui("mail.resumeBlock.evidencePlaceholder"), 13f, Color.white,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.025f, 0.07f), new Vector2(0.80f, 0.17f));
            CreateText("Source", block, Ui("mail.resumeBlock.sourcePlaceholder"), 13f, Accent,
                TextAlignmentOptions.BottomRight,
                new Vector2(0.80f, 0.07f), new Vector2(0.975f, 0.17f));
            CreateText("Availability", block, Ui("mail.resumeBlock.availabilityPlaceholder"), 13f, Warning,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.025f, 0.00f), new Vector2(0.975f, 0.07f));
            block.gameObject.SetActive(false);
        }

        private static void BuildPrivateOfferMailBlock(RectTransform parent)
        {
            var block = CreatePanel(
                "PrivateOfferBlock", parent, new Color(0.22f, 0.12f, 0.06f, 1f),
                new Vector2(0.035f, 0.055f), new Vector2(0.965f, 0.51f));
            CreateText("Title", block, Ui("mail.offerBlock.title"), 17f, Warning,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.025f, 0.80f), new Vector2(0.975f, 0.98f));
            CreateText("Offer", block, Ui("mail.offerBlock.offerPlaceholder"), 25f, Color.white,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.025f, 0.58f), new Vector2(0.975f, 0.80f));
            CreateText("Terms", block, Ui("mail.offerBlock.termsPlaceholder"), 17f, Color.white,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.025f, 0.40f), new Vector2(0.975f, 0.58f));
            CreateText("TargetClub", block, Ui("mail.offerBlock.targetClubPlaceholder"), 16f, Accent,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.025f, 0.25f), new Vector2(0.975f, 0.40f));
            CreateText("Risk", block, Ui("mail.offerBlock.riskPlaceholder"), 16f, Warning,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.025f, 0.05f), new Vector2(0.975f, 0.25f));
            block.gameObject.SetActive(false);
        }

        private static RectTransform BuildMagazineBrowser(RectTransform parent)
        {
            var browser = CreateRect(
                "MagazineBrowser", parent,
                new Vector2(0f, 0.195f), new Vector2(1f, 0.8f));
            var rail = CreatePanel(
                "IssueRail", browser, Panel,
                new Vector2(0.015f, 0f), new Vector2(0.23f, 0.965f));
            CreateText(
                "RailTitle", rail, Ui("magazine.railTitle"), 17f, Accent,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.05f, 0.90f), new Vector2(0.95f, 0.985f));
            CreateText(
                "RailTip", rail, Ui("magazine.railTip"), 14f, Muted,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.05f, 0.80f), new Vector2(0.95f, 0.90f));
            var issueList = CreateRect(
                "IssueList", rail,
                new Vector2(0.05f, 0.06f), new Vector2(0.95f, 0.78f));
            for (var index = 0; index < 36; index++)
            {
                BuildIssueListItem(issueList, index);
            }

            var pagePanel = CreatePanel(
                "PagePanel", browser, MagazinePaper,
                new Vector2(0.245f, 0f), new Vector2(0.985f, 0.965f));
            BuildMagazineCover(pagePanel);
            BuildMagazineFeature(pagePanel);
            BuildMagazineScoutReport(pagePanel);
            CreateButton(
                "PrevPageButton", pagePanel, Ui("magazine.prevPage"),
                new Vector2(0.025f, 0.02f), new Vector2(0.14f, 0.09f),
                MagazineInk, Color.white);
            CreateText(
                "PageIndicator", pagePanel, Ui("magazine.pageIndicatorPlaceholder"), 14f, MagazineInk,
                TextAlignmentOptions.Center,
                new Vector2(0.18f, 0.02f), new Vector2(0.82f, 0.09f));
            CreateButton(
                "NextPageButton", pagePanel, Ui("magazine.nextPage"),
                new Vector2(0.86f, 0.02f), new Vector2(0.975f, 0.09f),
                MagazineRed, Color.white);
            return browser;
        }

        private static void BuildIssueListItem(RectTransform parent, int index)
        {
            const float height = 66f;
            const float gap = 8f;
            var item = CreateTopItem(
                $"IssueItem{index + 1:00}", parent, PanelLight,
                height, -index * (height + gap));
            var image = item.GetComponent<Image>();
            var button = item.gameObject.AddComponent<Button>();
            ConfigureButtonColors(button, image, PanelLight);
            CreateText(
                "Publication", item, Ui("magazine.itemPublicationPlaceholder"), 16f, Warning,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.05f, 0.50f), new Vector2(0.95f, 0.92f));
            CreateText(
                "Issue", item, Ui("magazine.itemIssuePlaceholder"), 14f, Color.white,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.52f));
        }

        private static void BuildMagazineCover(RectTransform parent)
        {
            var cover = CreateRect(
                "CoverLayout", parent,
                new Vector2(0.025f, 0.11f), new Vector2(0.975f, 0.975f));
            CreateRawImage("CoverImage", cover,
                new Vector2(0.54f, 0.08f), new Vector2(0.97f, 0.74f));
            CreatePanel(
                "CoverBand", cover, MagazineRed,
                new Vector2(0f, 0.76f), new Vector2(1f, 1f));
            CreateText(
                "Publication", cover, Ui("magazine.coverPublicationPlaceholder"), 46f, Color.white,
                TextAlignmentOptions.BottomLeft,
                new Vector2(0.04f, 0.78f), new Vector2(0.80f, 0.98f));
            CreateText(
                "IssueNumber", cover, Ui("magazine.coverIssuePlaceholder"), 18f, Color.white,
                TextAlignmentOptions.BottomRight,
                new Vector2(0.80f, 0.80f), new Vector2(0.96f, 0.96f));
            CreateText(
                "Headline", cover, Ui("magazine.coverHeadlinePlaceholder"), 50f, MagazineInk,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.04f, 0.40f), new Vector2(0.52f, 0.72f));
            CreateText(
                "Deck", cover, Ui("magazine.coverDeckPlaceholder"), 24f, MagazineRed,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.04f, 0.20f), new Vector2(0.52f, 0.40f));
            CreateText(
                "CoverNote", cover, Ui("magazine.coverNote"), 15f, MagazineInk,
                TextAlignmentOptions.BottomLeft,
                new Vector2(0.04f, 0.04f), new Vector2(0.52f, 0.18f));
        }

        private static void BuildMagazineFeature(RectTransform parent)
        {
            var layout = CreateRect(
                "FeatureLayout", parent,
                new Vector2(0.025f, 0.11f), new Vector2(0.975f, 0.975f));
            CreateText("Kicker", layout, Ui("magazine.featureKicker"), 15f, MagazineRed,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.03f, 0.91f), new Vector2(0.97f, 0.99f));
            CreateText("Headline", layout, Ui("magazine.featureHeadline"), 37f, MagazineInk,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.03f, 0.72f), new Vector2(0.97f, 0.91f));
            CreateText("Deck", layout, Ui("magazine.featureDeck"), 18f, MagazineRed,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.03f, 0.62f), new Vector2(0.97f, 0.73f));
            CreateText("BodyLeft", layout, Ui("magazine.featureBodyLeft"), 15f, MagazineInk,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.03f, 0.18f), new Vector2(0.43f, 0.60f));
            CreateText("BodyRight", layout, Ui("magazine.featureBodyRight"), 15f, MagazineInk,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.46f, 0.18f), new Vector2(0.72f, 0.60f));
            CreateText("PullQuote", layout, Ui("magazine.featurePullQuote"), 22f, MagazineRed,
                TextAlignmentOptions.Center,
                new Vector2(0.74f, 0.38f), new Vector2(0.97f, 0.60f));
            CreateText("SidebarTitle", layout, Ui("magazine.featureSidebarTitle"), 16f, MagazineInk,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.74f, 0.30f), new Vector2(0.97f, 0.38f));
            CreateText("SidebarBody", layout, Ui("magazine.featureSidebarBody"), 14f, MagazineInk,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.74f, 0.18f), new Vector2(0.97f, 0.31f));
            CreateMagazineFigure("Illustration", layout);
            CreateMagazineFigure("Illustration2", layout);
        }

        private static void BuildMagazineScoutReport(RectTransform parent)
        {
            var layout = CreateRect(
                "ScoutReportLayout", parent,
                new Vector2(0.025f, 0.11f), new Vector2(0.975f, 0.975f));
            CreatePanel(
                "ReportBand", layout, MagazineInk,
                new Vector2(0f, 0.82f), new Vector2(1f, 1f));
            CreateText("Kicker", layout, Ui("magazine.scoutKicker"), 15f, Color.white,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.03f, 0.92f), new Vector2(0.97f, 0.99f));
            CreateText("Headline", layout, Ui("magazine.scoutHeadline"), 34f, Color.white,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.03f, 0.83f), new Vector2(0.97f, 0.93f));
            CreateText("Deck", layout, Ui("magazine.scoutDeck"), 19f, MagazineRed,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.03f, 0.70f), new Vector2(0.97f, 0.81f));
            CreateText("BodyLeft", layout, Ui("magazine.scoutBodyLeft"), 17f, MagazineInk,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.03f, 0.30f), new Vector2(0.47f, 0.68f));
            CreateText("BodyRight", layout, Ui("magazine.scoutBodyRight"), 17f, MagazineInk,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.52f, 0.30f), new Vector2(0.97f, 0.68f));
            CreateText("PullQuote", layout, Ui("magazine.scoutPullQuote"), 21f, MagazineRed,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.03f, 0.17f), new Vector2(0.70f, 0.30f));
            CreateText("SidebarTitle", layout, Ui("magazine.scoutSidebarTitle"), 15f, MagazineInk,
                TextAlignmentOptions.MidlineRight,
                new Vector2(0.72f, 0.22f), new Vector2(0.97f, 0.30f));
            CreateText("SidebarBody", layout, Ui("magazine.scoutSidebarBody"), 13f, MagazineInk,
                TextAlignmentOptions.TopRight,
                new Vector2(0.72f, 0.12f), new Vector2(0.97f, 0.23f));
            CreateMagazineFigure("Illustration", layout);
        }

        private static void BuildFooter(RectTransform parent)
        {
            var footer = CreatePanel(
                "Footer", parent, Panel,
                new Vector2(0.015f, 0.02f), new Vector2(0.985f, 0.185f));
            CreateText(
                "EventLog", footer, Ui("footer.eventLogPlaceholder"), 16f, Muted,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.025f, 0.16f), new Vector2(0.64f, 0.87f));
            CreateText(
                "Status", footer, Ui("footer.statusPlaceholder"), 17f, Color.white,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0.66f, 0.55f), new Vector2(0.965f, 0.9f));
            var endWeekButton = CreateButton(
                "EndWeekButton", footer, Ui("footer.endWeek"),
                new Vector2(0.74f, 0.12f), new Vector2(0.965f, 0.51f), Accent, Background);
            endWeekButton.gameObject.SetActive(false);
        }

        private static void BuildDragGhost(Transform parent)
        {
            var ghost = new GameObject(
                "DragGhost",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            ghost.transform.SetParent(parent, false);
            var rect = ghost.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(360f, 72f);
            ghost.GetComponent<Image>().color =
                new Color(0.08f, 0.13f, 0.18f, 0.94f);
            ghost.GetComponent<Image>().raycastTarget = false;
            ghost.GetComponent<CanvasGroup>().blocksRaycasts = false;
            var outline = ghost.AddComponent<Outline>();
            outline.effectColor = new Color(0.27f, 0.91f, 0.55f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);
            CreateText(
                "Label", rect, Ui("button.defaultLabel"), 24f, Color.white,
                TextAlignmentOptions.MidlineLeft, Vector2.zero, Vector2.one);
            ghost.SetActive(false);
        }

        private static RectTransform CreatePanel(
            string name,
            Transform parent,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            var panel = new GameObject(
                name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = color;
            var rect = panel.GetComponent<RectTransform>();
            Stretch(rect, anchorMin, anchorMax);
            return rect;
        }

        private static RectTransform CreateRect(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            var rectObject = new GameObject(name, typeof(RectTransform));
            rectObject.transform.SetParent(parent, false);
            var rect = rectObject.GetComponent<RectTransform>();
            Stretch(rect, anchorMin, anchorMax);
            return rect;
        }

        private static RectTransform CreateTopItem(
            string name,
            Transform parent,
            Color color,
            float height,
            float y)
        {
            var item = new GameObject(
                name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            item.transform.SetParent(parent, false);
            item.GetComponent<Image>().color = color;
            var rect = item.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, height);
            rect.anchoredPosition = new Vector2(0f, y);
            return rect;
        }

        private static TextMeshProUGUI CreateText(
            string name,
            Transform parent,
            string content,
            float fontSize,
            Color color,
            TextAlignmentOptions alignment,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            var textObject = new GameObject(
                name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            Stretch(
                rect, anchorMin, anchorMax,
                new Vector2(4f, 4f), new Vector2(-4f, -4f));
            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.font = _font;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.text = content;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Ellipsis;
            return text;
        }

        private static RectTransform CreateMagazineFigure(string name, Transform parent)
        {
            var container = CreateRect(
                name, parent, new Vector2(0.03f, 0.02f), new Vector2(0.43f, 0.3f));
            var caption = CreateText(
                "Caption", container, Ui("magazine.captionPlaceholder"), 13f,
                new Color(0.42f, 0.38f, 0.30f, 1f),
                TextAlignmentOptions.BottomLeft,
                new Vector2(0f, 0f), new Vector2(1f, 0f));
            caption.rectTransform.offsetMax = new Vector2(-4f, 24f);
            var frame = CreatePanel("Frame", container, MagazineInk, Vector2.zero, Vector2.one);
            frame.offsetMin = new Vector2(0f, 28f);
            var imageObject = new GameObject(
                "Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            imageObject.transform.SetParent(frame, false);
            Stretch(
                imageObject.GetComponent<RectTransform>(),
                Vector2.zero, Vector2.one,
                new Vector2(3f, 3f), new Vector2(-3f, -3f));
            imageObject.GetComponent<RawImage>().raycastTarget = false;
            container.gameObject.SetActive(false);
            return container;
        }

        private static RawImage CreateRawImage(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            // 容器锚定图片区域；RawImage 子节点在容器内按 1:1 适配并居中，
            // 不会因为 AspectRatioFitter 相对父级放大而溢出到文字区。
            var container = CreateRect(name, parent, anchorMin, anchorMax);
            var imageObject = new GameObject(
                "Image",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage),
                typeof(AspectRatioFitter));
            imageObject.transform.SetParent(container, false);
            var rect = imageObject.GetComponent<RectTransform>();
            Stretch(rect, Vector2.zero, Vector2.one);
            var image = imageObject.GetComponent<RawImage>();
            image.raycastTarget = false;
            var aspect = imageObject.GetComponent<AspectRatioFitter>();
            aspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            aspect.aspectRatio = 1f;
            return image;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color color,
            Color labelColor)
        {
            var buttonObject = new GameObject(
                name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            Stretch(rect, anchorMin, anchorMax);
            var image = buttonObject.GetComponent<Image>();
            image.color = color;
            var button = buttonObject.GetComponent<Button>();
            ConfigureButtonColors(button, image, color);
            CreateText(
                "Label", rect, label, 19f, labelColor,
                TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
            return button;
        }

        private static void ConfigureButtonColors(Button button, Image image, Color color)
        {
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.86f);
            colors.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.7f);
            button.colors = colors;
        }

        private static void Stretch(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2? offsetMin = null,
            Vector2? offsetMax = null)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin ?? Vector2.zero;
            rect.offsetMax = offsetMax ?? Vector2.zero;
        }
    }
}
