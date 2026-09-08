using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NewPlayerHunter.Domain;
using NewPlayerHunter.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace NewPlayerHunter.Gameplay
{
    public sealed class GameController : MonoBehaviour, IGameInteractionController
    {
        private enum WorkspaceMode
        {
            Information,
            Assignment
        }

        private enum InformationMode
        {
            Mail,
            Magazine
        }

        private const int FinalPlayableWeek = SeasonCalendar.MaximumPlayableWeeks;
        private const int GameSeed = 20260810;
        private const int PlayerAtlasColumns = 8;
        private const int PlayerAtlasRows = 8;
        private const int CoverAtlasColumns = 6;
        private const int CoverAtlasRows = 6;
        private const int IllustrationAtlasColumns = 6;
        private const int IllustrationAtlasRows = 6;
        private static readonly DateTime SeasonStartDate = new DateTime(2026, 7, 6);

        private static readonly Color PanelLightColor =
            new Color(0.09f, 0.13f, 0.17f, 1f);
        private static readonly Color AccentColor =
            new Color(0.25f, 0.9f, 0.53f, 1f);
        private static readonly Color MutedColor =
            new Color(0.62f, 0.7f, 0.76f, 1f);
        private static readonly Color ReadColor =
            new Color(0.08f, 0.19f, 0.15f, 1f);

        [SerializeField] private GameContentCatalog contentCatalog;

        private readonly List<PlayerPublicViewModel> _players =
            new List<PlayerPublicViewModel>();
        private readonly List<ClubDemand> _weeklyDemands =
            new List<ClubDemand>();
        private readonly List<ClubDemand> _eligibleDemands =
            new List<ClubDemand>();
        private readonly Dictionary<string, PlayerContentEntry> _playerContentById =
            new Dictionary<string, PlayerContentEntry>(StringComparer.Ordinal);
        private readonly Dictionary<string, DemandContentEntry> _demandContentById =
            new Dictionary<string, DemandContentEntry>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _slotAssignments =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly HashSet<string> _readMailIds =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _unlockedPlayerIds =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _unlockedDemandIds =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly List<MailContentEntry> _visibleMails =
            new List<MailContentEntry>();
        private readonly List<MailContentEntry> _resultMails =
            new List<MailContentEntry>();
        private readonly List<MagazineIssueContent> _visibleIssues =
            new List<MagazineIssueContent>();
        private readonly List<string> _eventLog = new List<string>();

        private WeekState _state;
        private WeekEngine _engine;
        private SeededRandomSource _random;
        private SeasonCalendar _seasonCalendar;
        private SaveGameService _saveGameService = new SaveGameService();
        private ClubDemand _currentDemand;
        private string _selectedDemandId;
        private string _selectedPlayerId;
        private string _selectedMailId;
        private string _selectedIssueId;
        private int _magazinePageIndex;
        private bool _pendingEmptyConfirmation;
        private bool _uiBound;
        private WorkspaceMode _activeWorkspace = WorkspaceMode.Information;
        private InformationMode _informationMode = InformationMode.Mail;

        private Canvas _canvas;
        private RectTransform _assignmentWorkspace;
        private RectTransform _informationWorkspace;
        private RectTransform _mailBrowser;
        private RectTransform _magazineBrowser;
        private RectTransform _slotsContainer;
        private RectTransform _demandListContainer;
        private RectTransform _playersContainer;
        private RectTransform _mailListContainer;
        private RectTransform _issueListContainer;
        private TextMeshProUGUI _weekText;
        private TextMeshProUGUI _economyText;
        private TextMeshProUGUI _demandTitleText;
        private TextMeshProUGUI _demandBodyText;
        private TextMeshProUGUI _selectionText;
        private TextMeshProUGUI _statusText;
        private TextMeshProUGUI _eventLogText;
        private TextMeshProUGUI _informationCounterText;
        private TextMeshProUGUI _mailSenderText;
        private TextMeshProUGUI _mailSubjectText;
        private TextMeshProUGUI _mailMetaText;
        private TextMeshProUGUI _mailBodyText;
        private RectTransform _mailDemandBlock;
        private RectTransform _mailResumeBlock;
        private RectTransform _mailPrivateOfferBlock;
        private TextMeshProUGUI _magazinePageIndicator;
        private RectTransform _coverLayout;
        private RectTransform _featureLayout;
        private RectTransform _scoutReportLayout;
        private CanvasGroup _weekTransitionOverlay;
        private TextMeshProUGUI _weekTransitionText;
        private bool _weekTransitionRunning;
        private CanvasGroup _informationWorkspaceGroup;
        private CanvasGroup _assignmentWorkspaceGroup;
        private Coroutine _workspaceTransition;
        private readonly Dictionary<Transform, Coroutine> _punchCoroutines =
            new Dictionary<Transform, Coroutine>();
        private RawImage _coverImage;
        private RectTransform _featureIllustration;
        private RectTransform _featureIllustration2;
        private RectTransform _scoutReportIllustration;
        private Button _informationTabButton;
        private Button _assignmentTabButton;
        private Button _mailFilterButton;
        private Button _subscriptionFilterButton;
        private Button _endWeekButton;
        private Button _previousPageButton;
        private Button _nextPageButton;
        private bool _carloFavorAccepted;
        private bool _carloFavorConsequenceApplied;
        private GameLanguage _language;
        private Button _resetButton;
        private Button _menuButton;
        private CanvasGroup _mainMenuOverlay;
        private TextMeshProUGUI _mainMenuTitleText;
        private Button _continueButton;
        private Button _newGameButton;
        private Button _languageButton;
        private Button _quitButton;
        private Button _resumeButton;
        private bool _mainMenuOpen;
        private bool _newGameConfirmPending;
        private bool _gameStarted;

        public int CurrentWeek => _state == null ? 0 : _state.CurrentWeek;

        public DateTime CurrentDate => _seasonCalendar == null
            ? default
            : _seasonCalendar.DateForWeek(Math.Min(CurrentWeek, FinalPlayableWeek));

        public SeasonPhase CurrentSeasonPhase => _seasonCalendar == null
            ? SeasonPhase.Preseason
            : _seasonCalendar.PhaseForWeek(Math.Min(CurrentWeek, FinalPlayableWeek));

        public decimal CurrentCash => _state == null ? 0m : _state.Cash;

        public bool IsGameComplete =>
            _state != null && _state.CurrentWeek > FinalPlayableWeek;

        public string LastStatus { get; private set; } = string.Empty;

        public IReadOnlyList<string> VisibleSlotIds => _currentDemand == null
            ? Array.Empty<string>()
            : _currentDemand.Slots.Select(slot => slot.Id).ToArray();

        public IReadOnlyList<string> VisiblePlayerIds => GetAvailablePlayersInReceivedOrder()
            .Select(player => player.PlayerId)
            .ToArray();

        public string CurrentDemandId =>
            _currentDemand == null ? string.Empty : _currentDemand.Id;

        public string SelectedDemandId =>
            _selectedDemandId == null ? string.Empty : _selectedDemandId;

        public IReadOnlyList<string> VisibleMailIds =>
            _visibleMails.Select(mail => mail.id).ToArray();

        public int ResultMailCount => _resultMails.Count(mail => mail.publishedWeek <= CurrentWeek);

        public int PlayerCardCount =>
            _playersContainer == null ? 0 : _playersContainer.childCount;

        public int SlotTargetCount => _slotsContainer == null
            ? 0
            : _slotsContainer.Cast<Transform>().Count(child => child.gameObject.activeSelf);

        public string ActiveWorkspace => _activeWorkspace.ToString();

        public string ActiveInformationChannel => _informationMode.ToString();

        public int VisibleInformationCount => _informationMode == InformationMode.Mail
            ? _visibleMails.Count
            : _visibleIssues.Count;

        public string SelectedInformationSubject
        {
            get
            {
                if (_informationMode == InformationMode.Mail)
                {
                    return Resolve(_visibleMails
                        .FirstOrDefault(mail => mail.id == _selectedMailId)?.subject);
                }

                return Resolve(_visibleIssues
                    .FirstOrDefault(issue => issue.id == _selectedIssueId)?.issueTitle);
            }
        }

        public int PlayerPoolCount => contentCatalog == null
            ? 0
            : contentCatalog.Players.Count;

        public int DemandPoolCount => contentCatalog == null
            ? 0
            : contentCatalog.Demands.Count;

        public int MailPoolCount => contentCatalog == null
            ? 0
            : contentCatalog.Mails.Count;

        public int MagazineIssueCount => contentCatalog == null
            ? 0
            : contentCatalog.MagazineIssues.Count;

        public int ReadMailCount => _readMailIds.Count;

        public int UnlockedPlayerCount => _unlockedPlayerIds.Count;

        public int UnlockedDemandCount => _unlockedDemandIds.Count;

        public bool CarloFavorAccepted => _carloFavorAccepted;

        public bool CarloFavorConsequenceApplied => _carloFavorConsequenceApplied;

        public int CurrentMagazinePageIndex => _magazinePageIndex;

        public int CurrentMagazinePageCount
        {
            get
            {
                var issue = GetSelectedIssue();
                return issue == null ? 0 : issue.pages.Count;
            }
        }

        public string CurrentMagazinePageLayout
        {
            get
            {
                var page = GetSelectedPage();
                return page == null ? string.Empty : page.layout.ToString();
            }
        }

        public RectTransform CanvasTransform =>
            _canvas == null ? null : _canvas.transform as RectTransform;

        public void ConfigureContentCatalog(GameContentCatalog catalog)
        {
            contentCatalog = catalog;
        }

        private void Awake()
        {
            if (contentCatalog == null || !contentCatalog.IsConfigured)
            {
                throw new InvalidOperationException(
                    "Game scene needs a configured GameContentCatalog asset.");
            }

            _language = contentCatalog.DevelopmentLanguage;
            if (_saveGameService.TryLoadString("ui.language", out var savedLanguage) &&
                Enum.TryParse(savedLanguage, out GameLanguage parsedLanguage))
            {
                _language = parsedLanguage;
            }

            BuildGameState();
            LoadProgressIfAvailable();
            BindSceneUi();
            RefreshUi();
            if (_mainMenuOverlay != null)
            {
                OpenMainMenu();
            }
        }

        public GameLanguage Language => _language;

        public bool IsMainMenuOpen => _mainMenuOpen && _mainMenuOverlay != null;

        public void SetLanguage(GameLanguage language)
        {
            _language = language;
            try
            {
                _saveGameService.SaveString("ui.language", language.ToString());
            }
            catch (Exception exception)
            {
                Debug.LogError($"[NewPlayerHunter] 保存语言偏好失败：{exception.Message}");
            }

            if (!_uiBound)
            {
                return;
            }

            ReapplyStaticUiTexts();
            if (IsMainMenuOpen)
            {
                RefreshMainMenuLabels();
            }

            RegenerateResultMails();
            RefreshUi();
        }

        public void OpenMainMenu()
        {
            if (_mainMenuOverlay == null)
            {
                return;
            }

            _mainMenuOpen = true;
            _newGameConfirmPending = false;
            _mainMenuOverlay.gameObject.SetActive(true);
            _mainMenuOverlay.alpha = 1f;
            _mainMenuOverlay.interactable = true;
            _mainMenuOverlay.blocksRaycasts = true;
            RefreshMainMenuLabels();
        }

        public void CloseMainMenu()
        {
            if (_mainMenuOverlay == null)
            {
                return;
            }

            _mainMenuOpen = false;
            _newGameConfirmPending = false;
            _gameStarted = true;
            _mainMenuOverlay.alpha = 0f;
            _mainMenuOverlay.interactable = false;
            _mainMenuOverlay.blocksRaycasts = false;
            _mainMenuOverlay.gameObject.SetActive(false);
        }

        public void CloseMainMenuForTests()
        {
            CloseMainMenu();
        }

        public void StartNewGameFromMenu()
        {
            if (HasSavedProgress && !_newGameConfirmPending)
            {
                _newGameConfirmPending = true;
                SetLabelText(
                    _newGameButton,
                    UiStrings.Get("menu.newGameConfirm", _language));
                return;
            }

            _newGameConfirmPending = false;
            RestartGame();
            CloseMainMenu();
        }

        public void QuitGame()
        {
            if (Application.isEditor)
            {
                Debug.Log("[NewPlayerHunter] Quit requested from the main menu (ignored in the editor).");
                return;
            }

            Application.Quit();
        }

        private void RefreshMainMenuLabels()
        {
            if (_mainMenuTitleText != null)
            {
                _mainMenuTitleText.text = UiStrings.Get("menu.title", _language);
            }

            SetLabelText(_continueButton, UiStrings.Get("menu.continue", _language));
            SetLabelText(
                _newGameButton,
                UiStrings.Get(
                    _newGameConfirmPending ? "menu.newGameConfirm" : "menu.newGame",
                    _language));
            SetLabelText(_languageButton, UiStrings.Get("menu.language", _language));
            SetLabelText(_quitButton, UiStrings.Get("menu.quit", _language));
            SetLabelText(_resumeButton, UiStrings.Get("menu.resume", _language));
            if (_continueButton != null)
            {
                _continueButton.interactable = HasSavedProgress;
            }

            if (_resumeButton != null)
            {
                _resumeButton.gameObject.SetActive(_gameStarted);
            }
        }

        private void ReapplyStaticUiTexts()
        {
            SetLabelText(
                _informationTabButton, UiStrings.Get("header.informationTab", _language));
            SetLabelText(
                _assignmentTabButton, UiStrings.Get("header.assignmentTab", _language));
            SetLabelText(_mailFilterButton, UiStrings.Get("info.mailFilter", _language));
            SetLabelText(
                _subscriptionFilterButton, UiStrings.Get("info.subscriptionFilter", _language));
            SetLabelText(_previousPageButton, UiStrings.Get("magazine.prevPage", _language));
            SetLabelText(_nextPageButton, UiStrings.Get("magazine.nextPage", _language));
            SetLabelText(_endWeekButton, UiStrings.Get("footer.endWeek", _language));
            SetLabelText(_resetButton, UiStrings.Get("header.reset", _language));
            SetLabelText(_menuButton, UiStrings.Get("header.menu", _language));
            SetPathLabel(
                "GameCanvas/Background/AssignmentWorkspace/DemandPanel/DemandLabel",
                "demand.label");
            SetPathLabel(
                "GameCanvas/Background/InformationWorkspace/Toolbar/BrowserLabel",
                "info.browserLabel");
            SetPathLabel(
                "GameCanvas/Background/InformationWorkspace/MailBrowser/MessageListPanel/ListLabel",
                "mail.listLabel");
            SetPathLabel(
                "GameCanvas/Background/InformationWorkspace/MailBrowser/DetailPanel/ReadingLabel",
                "mail.readingLabel");
            SetPathLabel(
                "GameCanvas/Background/InformationWorkspace/MailBrowser/DetailPanel/DemandBlock/Title",
                "mail.demandBlock.title");
            SetPathLabel(
                "GameCanvas/Background/InformationWorkspace/MailBrowser/DetailPanel/ResumeBlock/Title",
                "mail.resumeBlock.title");
            SetPathLabel(
                "GameCanvas/Background/InformationWorkspace/MailBrowser/DetailPanel/PrivateOfferBlock/Title",
                "mail.offerBlock.title");
            SetPathLabel(
                "GameCanvas/Background/InformationWorkspace/MagazineBrowser/IssueRail/RailTitle",
                "magazine.railTitle");
            SetPathLabel(
                "GameCanvas/Background/InformationWorkspace/MagazineBrowser/IssueRail/RailTip",
                "magazine.railTip");
            RefreshMainMenuLabels();
        }

        private void SetPathLabel(string path, string key)
        {
            var label = FindSceneComponent<TextMeshProUGUI>(path);
            if (label != null)
            {
                label.text = UiStrings.Get(key, _language);
            }
        }

        private static void SetLabelText(Button button, string text)
        {
            if (button == null)
            {
                return;
            }

            var label = button.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = text;
            }
        }

        public void SelectPlayer(string playerId)
        {
            if (!IsPlayerAvailable(playerId) || IsGameComplete)
            {
                return;
            }

            _selectedPlayerId = _selectedPlayerId == playerId ? null : playerId;
            _pendingEmptyConfirmation = false;
            LastStatus = string.IsNullOrEmpty(_selectedPlayerId)
                ? UiStrings.Get("status.playerDeselected", _language)
                : UiStrings.Format(
                    "status.playerSelected",
                    _language,
                    GetPlayerDisplayName(_selectedPlayerId));
            RefreshUi();
        }

        public void SelectDemand(string demandId)
        {
            if (IsGameComplete)
            {
                return;
            }

            UpdateCurrentDemand();
            var demand = _eligibleDemands.FirstOrDefault(item => item.Id == demandId);
            if (demand == null || demand.Id == _selectedDemandId)
            {
                return;
            }

            _selectedDemandId = demand.Id;
            _pendingEmptyConfirmation = false;
            var entry = _demandContentById[demand.Id];
            LastStatus = UiStrings.Format(
                "status.demandSelected",
                _language,
                Resolve(entry.clubDisplayName),
                Resolve(entry.title));
            RefreshUi();
        }

        public void HandleSlotClicked(string slotId)
        {
            if (_currentDemand == null || IsGameComplete)
            {
                return;
            }

            if (!string.IsNullOrEmpty(_selectedPlayerId))
            {
                AssignPlayerToSlot(slotId, _selectedPlayerId);
                return;
            }

            if (_slotAssignments.Remove(slotId))
            {
                _pendingEmptyConfirmation = false;
                LastStatus = UiStrings.Get("status.slotCleared", _language);
                RefreshUi();
            }
        }

        public bool AssignPlayerToSlot(string slotId, string playerId)
        {
            if (_currentDemand == null ||
                !_unlockedPlayerIds.Contains(playerId) ||
                !_currentDemand.Slots.Any(slot => slot.Id == slotId))
            {
                return false;
            }

            var duplicateSlot = _slotAssignments
                .FirstOrDefault(pair => pair.Value == playerId && pair.Key != slotId)
                .Key;
            if (!string.IsNullOrEmpty(duplicateSlot))
            {
                _slotAssignments.Remove(duplicateSlot);
            }

            _slotAssignments[slotId] = playerId;
            _selectedPlayerId = null;
            _pendingEmptyConfirmation = false;
            LastStatus = UiStrings.Format(
                "status.playerAssigned", _language, GetPlayerDisplayName(playerId));
            RefreshUi();
            return true;
        }

        public bool AssignPlayerForTests(string slotId, string playerId)
        {
            return AssignPlayerToSlot(slotId, playerId);
        }

        public void EndWeekForTests()
        {
            EndWeek();
        }

        public void OpenMailForTests(string mailId)
        {
            OpenMail(mailId);
        }

        public void ShowInformationForTests()
        {
            ShowInformationWorkspace();
        }

        public void ShowAssignmentForTests()
        {
            ShowAssignmentWorkspace();
        }

        public void ShowSubscriptionsForTests()
        {
            SetInformationMode(InformationMode.Magazine);
        }

        public void NextMagazinePageForTests()
        {
            ChangeMagazinePage(1);
        }

        public void RestartGame()
        {
            try
            {
                _saveGameService.DeleteProgress();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[NewPlayerHunter] 删除存档失败：{exception.Message}");
            }

            BuildGameState();
            BindPlayerCards();
            _activeWorkspace = WorkspaceMode.Information;
            _informationMode = InformationMode.Mail;
            ApplyWorkspaceVisibility();
            RefreshUi();
            Debug.Log("[NewPlayerHunter] 游戏已重置到第 1 周。");
        }

        public bool HasSavedProgress => _saveGameService.HasProgress;

        public void ConfigureSaveGameService(SaveGameService service)
        {
            _saveGameService = service ?? new SaveGameService();
        }

        public void SaveProgressForTests()
        {
            SaveProgress();
        }

        public void ReloadProgressForTests()
        {
            BuildGameState();
            LoadProgressIfAvailable();
            _activeWorkspace = WorkspaceMode.Information;
            _informationMode = InformationMode.Mail;
            ApplyWorkspaceVisibility();
            RefreshUi();
        }

        private void SaveProgress()
        {
            try
            {
                var snapshot = new GameProgressSnapshot
                {
                    weekState = ProgressSnapshotMapper.FromDomain(_state.CreateSnapshot()),
                    readMailIds = _readMailIds.ToList(),
                    unlockedPlayerIds = _unlockedPlayerIds.ToList(),
                    unlockedDemandIds = _unlockedDemandIds.ToList(),
                    selectedDemandId = _selectedDemandId,
                    carloFavorAccepted = _carloFavorAccepted,
                    carloFavorConsequenceApplied = _carloFavorConsequenceApplied,
                    eventLog = new List<string>(_eventLog),
                    lastStatus = LastStatus,
                    randomDrawCount = _random.DrawCount
                };
                _saveGameService.Save(snapshot);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[NewPlayerHunter] 自动保存失败：{exception.Message}");
            }
        }

        private void LoadProgressIfAvailable()
        {
            if (!_saveGameService.TryLoad(out var snapshot))
            {
                return;
            }

            _state.ApplySnapshot(ProgressSnapshotMapper.ToDomain(snapshot.weekState));
            _readMailIds.UnionWith(snapshot.readMailIds ?? new List<string>());
            _unlockedPlayerIds.UnionWith(snapshot.unlockedPlayerIds ?? new List<string>());
            _unlockedDemandIds.UnionWith(snapshot.unlockedDemandIds ?? new List<string>());
            _selectedDemandId = snapshot.selectedDemandId;
            _carloFavorAccepted = snapshot.carloFavorAccepted;
            _carloFavorConsequenceApplied = snapshot.carloFavorConsequenceApplied;
            if (snapshot.eventLog != null)
            {
                _eventLog.AddRange(snapshot.eventLog);
            }

            if (!string.IsNullOrEmpty(snapshot.lastStatus))
            {
                LastStatus = snapshot.lastStatus;
            }

            _random.FastForward(snapshot.randomDrawCount);
            RegenerateResultMails();
            AddLog(UiStrings.Format("log.saveLoaded", _language, _state.CurrentWeek));
        }

        public string GetPlayerDisplayName(string playerId)
        {
            if (_playerContentById.TryGetValue(playerId, out var entry))
            {
                return Resolve(entry.displayName);
            }

            return _players.FirstOrDefault(player => player.PlayerId == playerId)
                ?.DisplayName ?? playerId;
        }

        private void BuildGameState()
        {
            _state = new WeekState(currentWeek: 1, initialCash: 500m, initialReputation: 10);
            _random = new SeededRandomSource(GameSeed);
            _engine = new WeekEngine(new WeekRules(), _random);
            _seasonCalendar = new SeasonCalendar(SeasonStartDate);
            _players.Clear();
            _weeklyDemands.Clear();
            _playerContentById.Clear();
            _demandContentById.Clear();
            _slotAssignments.Clear();
            _readMailIds.Clear();
            _unlockedPlayerIds.Clear();
            _unlockedDemandIds.Clear();
            _eventLog.Clear();
            _visibleMails.Clear();
            _resultMails.Clear();
            _visibleIssues.Clear();
            _selectedPlayerId = null;
            _selectedDemandId = null;
            _selectedMailId = null;
            _selectedIssueId = null;
            _magazinePageIndex = 0;
            _pendingEmptyConfirmation = false;
            _carloFavorAccepted = false;
            _carloFavorConsequenceApplied = false;

            foreach (var entry in contentCatalog.Players)
            {
                var profile = new PlayerPublicProfile(
                    entry.id,
                    Resolve(entry.displayName),
                    Resolve(entry.biography),
                    new[] { entry.publicPosition });
                var truth = new PlayerTruth(
                    entry.id,
                    entry.hiddenAbility,
                    entry.hiddenFitness,
                    entry.hiddenProfessionalism,
                    new[] { entry.publicPosition },
                    "仅供模拟器使用，不得进入公开 UI。");
                var claims = new[]
                {
                    new PlayerClaim(
                        "claim." + entry.id,
                        entry.id,
                        "source.resume",
                        entry.availableFromWeek,
                        Resolve(entry.publicClaim))
                };
                var evidence = new[]
                {
                    new EvidenceItem(
                        "evidence." + entry.id,
                        entry.id,
                        "source.scouting",
                        entry.availableFromWeek,
                        entry.evidenceReliability,
                        "公开来源可能有偏差",
                        Resolve(entry.publicEvidence))
                };
                _state.AddPlayer(profile, truth);
                _players.Add(PlayerPublicViewModelFactory.Create(profile, claims, evidence));
                _playerContentById.Add(entry.id, entry);
            }

            foreach (var entry in contentCatalog.Demands)
            {
                var demand = new ClubDemand(
                    entry.id,
                    entry.clubId,
                    Resolve(entry.title),
                    entry.openedWeek,
                    entry.activeWeeks,
                    entry.baseReward,
                    entry.slots.Select(slot => new DemandSlot(
                        slot.id,
                        slot.requiredPosition,
                        slot.minimumAbility,
                        slot.preferredFitness,
                        slot.preferredProfessionalism,
                        slot.isRequired)));
                _state.AddDemand(demand);
                _weeklyDemands.Add(demand);
                _demandContentById.Add(entry.id, entry);
            }

            AddLog(UiStrings.Format("log.seasonStart", _language, FormatWeekDate(1)));
            LastStatus = UiStrings.Get("status.initial", _language);
        }

        private void BindSceneUi()
        {
            var eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                throw new InvalidOperationException("Game scene is missing its authored EventSystem.");
            }

            var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
            {
                throw new InvalidOperationException(
                    "Game scene EventSystem is missing InputSystemUIInputModule.");
            }

            inputModule.AssignDefaultActions();
            _canvas = RequireSceneComponent<Canvas>("GameCanvas");
            _weekText = RequireSceneComponent<TextMeshProUGUI>(
                "GameCanvas/Background/Header/Week");
            _economyText = RequireSceneComponent<TextMeshProUGUI>(
                "GameCanvas/Background/Header/Economy");
            _informationTabButton = RequireSceneComponent<Button>(
                "GameCanvas/Background/Header/InformationTabButton");
            _assignmentTabButton = RequireSceneComponent<Button>(
                "GameCanvas/Background/Header/AssignmentTabButton");
            _assignmentWorkspace = RequireSceneComponent<RectTransform>(
                "GameCanvas/Background/AssignmentWorkspace");
            _informationWorkspace = RequireSceneComponent<RectTransform>(
                "GameCanvas/Background/InformationWorkspace");
            _demandTitleText = RequireSceneComponent<TextMeshProUGUI>(
                "GameCanvas/Background/AssignmentWorkspace/DemandPanel/DemandTitle");
            _demandBodyText = RequireSceneComponent<TextMeshProUGUI>(
                "GameCanvas/Background/AssignmentWorkspace/DemandPanel/DemandBody");
            _selectionText = RequireSceneComponent<TextMeshProUGUI>(
                "GameCanvas/Background/AssignmentWorkspace/DemandPanel/Selection");
            _slotsContainer = RequireSceneComponent<RectTransform>(
                "GameCanvas/Background/AssignmentWorkspace/DemandPanel/Slots");
            _demandListContainer = RequireSceneComponent<RectTransform>(
                "GameCanvas/Background/AssignmentWorkspace/DemandPanel/DemandList/Viewport/DemandListItems");
            _playersContainer = RequireSceneComponent<RectTransform>(
                "GameCanvas/Background/AssignmentWorkspace/PlayersPanel/PlayerScroll/Viewport/PlayerCards");
            _mailFilterButton = RequireSceneComponent<Button>(
                "GameCanvas/Background/InformationWorkspace/Toolbar/MailFilterButton");
            _subscriptionFilterButton = RequireSceneComponent<Button>(
                "GameCanvas/Background/InformationWorkspace/Toolbar/SubscriptionFilterButton");
            _informationCounterText = RequireSceneComponent<TextMeshProUGUI>(
                "GameCanvas/Background/InformationWorkspace/Toolbar/Counter");
            _mailBrowser = RequireSceneComponent<RectTransform>(
                "GameCanvas/Background/InformationWorkspace/MailBrowser");
            _mailListContainer = RequireSceneComponent<RectTransform>(
                "GameCanvas/Background/InformationWorkspace/MailBrowser/MessageListPanel/MessageScroll/Viewport/MessageList");
            _mailSenderText = RequireSceneComponent<TextMeshProUGUI>(
                "GameCanvas/Background/InformationWorkspace/MailBrowser/DetailPanel/Sender");
            _mailSubjectText = RequireSceneComponent<TextMeshProUGUI>(
                "GameCanvas/Background/InformationWorkspace/MailBrowser/DetailPanel/Subject");
            _mailMetaText = RequireSceneComponent<TextMeshProUGUI>(
                "GameCanvas/Background/InformationWorkspace/MailBrowser/DetailPanel/Meta");
            _mailBodyText = RequireSceneComponent<TextMeshProUGUI>(
                "GameCanvas/Background/InformationWorkspace/MailBrowser/DetailPanel/Body");
            _mailDemandBlock = RequireSceneComponent<RectTransform>(
                "GameCanvas/Background/InformationWorkspace/MailBrowser/DetailPanel/DemandBlock");
            _mailResumeBlock = RequireSceneComponent<RectTransform>(
                "GameCanvas/Background/InformationWorkspace/MailBrowser/DetailPanel/ResumeBlock");
            _mailPrivateOfferBlock = RequireSceneComponent<RectTransform>(
                "GameCanvas/Background/InformationWorkspace/MailBrowser/DetailPanel/PrivateOfferBlock");
            _magazineBrowser = RequireSceneComponent<RectTransform>(
                "GameCanvas/Background/InformationWorkspace/MagazineBrowser");
            _issueListContainer = RequireSceneComponent<RectTransform>(
                "GameCanvas/Background/InformationWorkspace/MagazineBrowser/IssueRail/IssueList");
            _magazinePageIndicator = RequireSceneComponent<TextMeshProUGUI>(
                "GameCanvas/Background/InformationWorkspace/MagazineBrowser/PagePanel/PageIndicator");
            _previousPageButton = RequireSceneComponent<Button>(
                "GameCanvas/Background/InformationWorkspace/MagazineBrowser/PagePanel/PrevPageButton");
            _nextPageButton = RequireSceneComponent<Button>(
                "GameCanvas/Background/InformationWorkspace/MagazineBrowser/PagePanel/NextPageButton");
            _coverLayout = RequireSceneComponent<RectTransform>(
                "GameCanvas/Background/InformationWorkspace/MagazineBrowser/PagePanel/CoverLayout");
            _featureLayout = RequireSceneComponent<RectTransform>(
                "GameCanvas/Background/InformationWorkspace/MagazineBrowser/PagePanel/FeatureLayout");
            _scoutReportLayout = RequireSceneComponent<RectTransform>(
                "GameCanvas/Background/InformationWorkspace/MagazineBrowser/PagePanel/ScoutReportLayout");
            _coverImage = RequireSceneComponent<RawImage>(
                "GameCanvas/Background/InformationWorkspace/MagazineBrowser/PagePanel/CoverLayout/CoverImage/Image");
            _featureIllustration = RequireSceneComponent<RectTransform>(
                "GameCanvas/Background/InformationWorkspace/MagazineBrowser/PagePanel/FeatureLayout/Illustration");
            _featureIllustration2 = RequireSceneComponent<RectTransform>(
                "GameCanvas/Background/InformationWorkspace/MagazineBrowser/PagePanel/FeatureLayout/Illustration2");
            _scoutReportIllustration = RequireSceneComponent<RectTransform>(
                "GameCanvas/Background/InformationWorkspace/MagazineBrowser/PagePanel/ScoutReportLayout/Illustration");
            _eventLogText = RequireSceneComponent<TextMeshProUGUI>(
                "GameCanvas/Background/Footer/EventLog");
            _statusText = RequireSceneComponent<TextMeshProUGUI>(
                "GameCanvas/Background/Footer/Status");
            _endWeekButton = RequireSceneComponent<Button>(
                "GameCanvas/Background/Footer/EndWeekButton");
            _resetButton = FindSceneComponent<Button>(
                "GameCanvas/Background/Header/ResetButton");
            _menuButton = FindSceneComponent<Button>(
                "GameCanvas/Background/Header/MenuButton");
            _mainMenuOverlay = FindSceneComponent<CanvasGroup>(
                "GameCanvas/MainMenuOverlay");
            if (_mainMenuOverlay != null)
            {
                _mainMenuTitleText = FindSceneComponent<TextMeshProUGUI>(
                    "GameCanvas/MainMenuOverlay/Panel/Title");
                _continueButton = FindSceneComponent<Button>(
                    "GameCanvas/MainMenuOverlay/Panel/ContinueButton");
                _newGameButton = FindSceneComponent<Button>(
                    "GameCanvas/MainMenuOverlay/Panel/NewGameButton");
                _languageButton = FindSceneComponent<Button>(
                    "GameCanvas/MainMenuOverlay/Panel/LanguageButton");
                _quitButton = FindSceneComponent<Button>(
                    "GameCanvas/MainMenuOverlay/Panel/QuitButton");
                _resumeButton = FindSceneComponent<Button>(
                    "GameCanvas/MainMenuOverlay/Panel/ResumeButton");
            }

            _weekTransitionOverlay = RequireSceneComponent<CanvasGroup>(
                "GameCanvas/WeekTransitionOverlay");
            _weekTransitionText = RequireSceneComponent<TextMeshProUGUI>(
                "GameCanvas/WeekTransitionOverlay/Text");
            _informationWorkspaceGroup =
                _informationWorkspace.GetComponent<CanvasGroup>();
            _assignmentWorkspaceGroup =
                _assignmentWorkspace.GetComponent<CanvasGroup>();

            if (_resetButton != null)
            {
                _resetButton.onClick.RemoveAllListeners();
                _resetButton.onClick.AddListener(() =>
                {
                    Punch(_resetButton.transform);
                    RestartGame();
                });
            }

            if (_menuButton != null)
            {
                _menuButton.onClick.RemoveAllListeners();
                _menuButton.onClick.AddListener(() =>
                {
                    Punch(_menuButton.transform);
                    OpenMainMenu();
                });
            }

            if (_mainMenuOverlay != null)
            {
                if (_continueButton != null)
                {
                    _continueButton.onClick.RemoveAllListeners();
                    _continueButton.onClick.AddListener(CloseMainMenu);
                }

                if (_newGameButton != null)
                {
                    _newGameButton.onClick.RemoveAllListeners();
                    _newGameButton.onClick.AddListener(() =>
                    {
                        Punch(_newGameButton.transform);
                        StartNewGameFromMenu();
                    });
                }

                if (_languageButton != null)
                {
                    _languageButton.onClick.RemoveAllListeners();
                    _languageButton.onClick.AddListener(() =>
                    {
                        Punch(_languageButton.transform);
                        SetLanguage(_language == GameLanguage.ChineseSimplified
                            ? GameLanguage.English
                            : GameLanguage.ChineseSimplified);
                    });
                }

                if (_quitButton != null)
                {
                    _quitButton.onClick.RemoveAllListeners();
                    _quitButton.onClick.AddListener(() =>
                    {
                        Punch(_quitButton.transform);
                        QuitGame();
                    });
                }

                if (_resumeButton != null)
                {
                    _resumeButton.onClick.RemoveAllListeners();
                    _resumeButton.onClick.AddListener(CloseMainMenu);
                }
            }

            _informationTabButton.onClick.RemoveAllListeners();
            _informationTabButton.onClick.AddListener(() =>
            {
                Punch(_informationTabButton.transform);
                ShowInformationWorkspaceAnimated();
            });
            _assignmentTabButton.onClick.RemoveAllListeners();
            _assignmentTabButton.onClick.AddListener(() =>
            {
                Punch(_assignmentTabButton.transform);
                ShowAssignmentWorkspaceAnimated();
            });
            _mailFilterButton.onClick.RemoveAllListeners();
            _mailFilterButton.onClick.AddListener(() =>
            {
                Punch(_mailFilterButton.transform);
                SetInformationMode(InformationMode.Mail);
            });
            _subscriptionFilterButton.onClick.RemoveAllListeners();
            _subscriptionFilterButton.onClick.AddListener(() =>
            {
                Punch(_subscriptionFilterButton.transform);
                SetInformationMode(InformationMode.Magazine);
            });
            _previousPageButton.onClick.RemoveAllListeners();
            _previousPageButton.onClick.AddListener(() =>
            {
                Punch(_previousPageButton.transform);
                ChangeMagazinePage(-1);
            });
            _nextPageButton.onClick.RemoveAllListeners();
            _nextPageButton.onClick.AddListener(() =>
            {
                Punch(_nextPageButton.transform);
                ChangeMagazinePage(1);
            });
            _endWeekButton.onClick.RemoveAllListeners();
            _endWeekButton.onClick.AddListener(() =>
            {
                Punch(_endWeekButton.transform);
                BeginWeekTransition();
            });
            BindPlayerCards();
            _uiBound = true;
            ReapplyStaticUiTexts();
        }

        private void BindPlayerCards()
        {
            if (_playersContainer.childCount < _players.Count)
            {
                throw new InvalidOperationException(
                    $"Game scene needs {_players.Count} authored player cards.");
            }

            for (var index = 0; index < _playersContainer.childCount; index++)
            {
                var card = (RectTransform)_playersContainer.GetChild(index);
                card.gameObject.SetActive(false);
            }
        }

        private void ShowInformationWorkspace()
        {
            _activeWorkspace = WorkspaceMode.Information;
            LastStatus = UiStrings.Get("status.infoOpened", _language);
            ApplyWorkspaceVisibility();
            RefreshUi();
        }

        private void ShowInformationWorkspaceAnimated()
        {
            if (_activeWorkspace == WorkspaceMode.Information || _weekTransitionRunning)
            {
                return;
            }

            StartWorkspaceTransition(ShowInformationWorkspace);
        }

        private void ShowAssignmentWorkspaceAnimated()
        {
            if (_activeWorkspace == WorkspaceMode.Assignment || _weekTransitionRunning)
            {
                return;
            }

            StartWorkspaceTransition(ShowAssignmentWorkspace);
        }

        private void StartWorkspaceTransition(Action apply)
        {
            if (!_uiBound)
            {
                apply();
                return;
            }

            if (_workspaceTransition != null)
            {
                StopCoroutine(_workspaceTransition);
            }

            _workspaceTransition = StartCoroutine(WorkspaceTransitionRoutine(apply));
        }

        private IEnumerator WorkspaceTransitionRoutine(Action apply)
        {
            var outgoing = _activeWorkspace == WorkspaceMode.Information
                ? _informationWorkspaceGroup
                : _assignmentWorkspaceGroup;
            yield return FadeCanvasGroup(outgoing, outgoing == null ? 1f : outgoing.alpha, 0f, 0.22f);

            apply();
            if (outgoing != null)
            {
                outgoing.alpha = 1f;
            }

            var incoming = _activeWorkspace == WorkspaceMode.Information
                ? _informationWorkspaceGroup
                : _assignmentWorkspaceGroup;
            if (incoming != null)
            {
                incoming.alpha = 0f;
            }

            yield return FadeCanvasGroup(incoming, 0f, 1f, 0.25f);
            _workspaceTransition = null;
        }

        private void BeginWeekTransition()
        {
            if (_weekTransitionRunning || IsGameComplete)
            {
                return;
            }

            StartCoroutine(WeekTransitionRoutine());
        }

        private IEnumerator WeekTransitionRoutine()
        {
            _weekTransitionRunning = true;
            if (_weekTransitionOverlay == null)
            {
                EndWeek();
                _weekTransitionRunning = false;
                yield break;
            }

            _weekTransitionOverlay.blocksRaycasts = true;
            _weekTransitionText.text = UiStrings.Get("status.weekSettling", _language);
            yield return FadeCanvasGroup(
                _weekTransitionOverlay, _weekTransitionOverlay.alpha, 1f, 0.5f);

            var weekBefore = CurrentWeek;
            EndWeek();
            if (CurrentWeek == weekBefore && !IsGameComplete)
            {
                // 提交被拦下（空缺确认或校验失败），不展示新日期，直接淡回。
                yield return FadeCanvasGroup(_weekTransitionOverlay, 1f, 0f, 0.3f);
                _weekTransitionOverlay.blocksRaycasts = false;
                _weekTransitionRunning = false;
                yield break;
            }

            _weekTransitionText.text = IsGameComplete
                ? UiStrings.Format(
                    "status.seasonEndLine",
                    _language,
                    FormatDate(_seasonCalendar.DateAfterFinalWeek))
                : UiStrings.Format(
                    "status.weekLine",
                    _language,
                    FormatCurrentDate(),
                    SeasonPhaseName(CurrentSeasonPhase));
            yield return new WaitForSeconds(1.0f);
            yield return FadeCanvasGroup(_weekTransitionOverlay, 1f, 0f, 0.5f);
            _weekTransitionOverlay.blocksRaycasts = false;
            _weekTransitionRunning = false;
        }

        private IEnumerator FadeCanvasGroup(
            CanvasGroup group,
            float from,
            float to,
            float duration)
        {
            if (group == null)
            {
                yield break;
            }

            for (var elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                group.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            group.alpha = to;
        }

        private void Punch(Transform target)
        {
            if (target == null)
            {
                return;
            }

            if (_punchCoroutines.TryGetValue(target, out var running) && running != null)
            {
                StopCoroutine(running);
            }

            _punchCoroutines[target] = StartCoroutine(PunchRoutine(target));
        }

        private IEnumerator PunchRoutine(Transform target)
        {
            const float duration = 0.2f;
            for (var elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                var progress = elapsed / duration;
                var scale = progress < 0.5f
                    ? Mathf.Lerp(1f, 0.95f, progress * 2f)
                    : Mathf.Lerp(0.95f, 1f, (progress - 0.5f) * 2f);
                target.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            target.localScale = Vector3.one;
            _punchCoroutines.Remove(target);
        }

        private void ShowAssignmentWorkspace()
        {
            _activeWorkspace = WorkspaceMode.Assignment;
            LastStatus = UiStrings.Get(
                _currentDemand == null
                    ? "status.assignmentOpenedEmpty"
                    : "status.assignmentOpened",
                _language);
            ApplyWorkspaceVisibility();
            RefreshUi();
        }

        private void SetInformationMode(InformationMode mode)
        {
            _informationMode = mode;
            if (mode == InformationMode.Mail)
            {
                LastStatus = UiStrings.Get("status.mailMode", _language);
            }
            else
            {
                EnsureSelectedIssue();
                LastStatus = UiStrings.Get("status.magazineMode", _language);
            }

            RefreshUi();
        }

        private void OpenMail(string mailId)
        {
            var mail = contentCatalog.Mails.FirstOrDefault(item =>
                item.id == mailId && ShouldDisplayMail(item));
            if (mail == null)
            {
                mail = _resultMails.FirstOrDefault(item =>
                    item.id == mailId && item.publishedWeek <= CurrentWeek);
            }

            if (mail == null)
            {
                return;
            }

            _selectedMailId = mail.id;
            var firstRead = _readMailIds.Add(mail.id);
            if (!string.IsNullOrEmpty(mail.relatedPlayerId))
            {
                _unlockedPlayerIds.Add(mail.relatedPlayerId);
            }

            if (!string.IsNullOrEmpty(mail.relatedDemandId))
            {
                _unlockedDemandIds.Add(mail.relatedDemandId);
            }

            var unlocksContent =
                !string.IsNullOrEmpty(mail.relatedPlayerId) ||
                !string.IsNullOrEmpty(mail.relatedDemandId);
            LastStatus = firstRead
                ? UiStrings.Format(
                    unlocksContent ? "status.mailReadUnlock" : "status.mailRead",
                    _language,
                    Resolve(mail.subject))
                : UiStrings.Format("status.mailReopened", _language, Resolve(mail.subject));
            RefreshUi();
        }

        private void SelectIssue(string issueId)
        {
            if (!_visibleIssues.Any(issue => issue.id == issueId))
            {
                return;
            }

            _selectedIssueId = issueId;
            _magazinePageIndex = 0;
            LastStatus = UiStrings.Format(
                "status.issueSelected", _language, Resolve(GetSelectedIssue().issueTitle));
            RefreshUi();
        }

        private void ChangeMagazinePage(int delta)
        {
            var issue = GetSelectedIssue();
            if (issue == null || issue.pages.Count == 0)
            {
                return;
            }

            _magazinePageIndex = Mathf.Clamp(
                _magazinePageIndex + delta,
                0,
                issue.pages.Count - 1);
            RefreshUi();
        }

        private void EndWeek()
        {
            if (IsGameComplete)
            {
                return;
            }

            UpdateCurrentDemand();
            var startingPhase = CurrentSeasonPhase;
            var assignments = _currentDemand == null
                ? new List<Assignment>()
                : _slotAssignments
                    .Where(pair => _currentDemand.Slots.Any(slot => slot.Id == pair.Key))
                    .Select(pair => new Assignment(
                        _currentDemand.Id,
                        pair.Key,
                        pair.Value,
                        _state.CurrentWeek))
                    .ToList();

            if (_currentDemand != null)
            {
                var missingRequiredSlots = _currentDemand.Slots
                    .Where(slot => slot.IsRequired && !_slotAssignments.ContainsKey(slot.Id))
                    .ToList();
                if (missingRequiredSlots.Count > 0 && !_pendingEmptyConfirmation)
                {
                    _pendingEmptyConfirmation = true;
                    LastStatus = UiStrings.Format(
                        "status.emptySlotsWarning", _language, missingRequiredSlots.Count);
                    RefreshUi();
                    return;
                }
            }

            if (assignments.Count > 0)
            {
                var submission = new AssignmentSubmission(
                    _currentDemand.Id,
                    assignments,
                    confirmedEmptyRequiredSlots: _pendingEmptyConfirmation);
                var commit = _engine.CommitAssignments(_state, new[] { submission });
                if (!commit.Validation.IsValid)
                {
                    LastStatus = TranslateSubmissionErrors(commit.Validation.Errors);
                    RefreshUi();
                    return;
                }

                var offerMail = contentCatalog.Mails.FirstOrDefault(mail =>
                    mail.privateOfferAmount > 0 &&
                    assignments.Any(assignment => assignment.PlayerId == mail.relatedPlayerId) &&
                    (string.IsNullOrEmpty(mail.privateRequiredClubId) ||
                     string.Equals(
                         mail.privateRequiredClubId,
                         _currentDemand.ClubId,
                         StringComparison.Ordinal)));
                if (offerMail != null)
                {
                    _state.ApplyImmediateIncome(offerMail.privateOfferAmount);
                    if (offerMail.id == "mail.w1.carlo")
                    {
                        _carloFavorAccepted = true;
                    }

                    AddLog(UiStrings.Format(
                        "log.instantIncome",
                        _language,
                        FormatMoney(offerMail.privateOfferAmount)));
                }

                var arrangedNames = assignments
                    .Select(assignment => GetPlayerDisplayName(assignment.PlayerId))
                    .ToArray();
                AddLog(UiStrings.Format(
                    "log.submitted",
                    _language,
                    FormatCurrentDate(),
                    _currentDemand.Title,
                    string.Join(UiStrings.Get("list.and", _language), arrangedNames)));
                foreach (var outcome in commit.ScheduledOutcomes)
                {
                    AddLog(UiStrings.Format(
                        "log.feedbackEta",
                        _language,
                        GetPlayerDisplayName(outcome.Assignment.PlayerId),
                        FormatWeekDate(outcome.OutcomeWeek)));
                }
            }
            else if (_currentDemand != null)
            {
                AddLog(UiStrings.Format(
                    "log.noSubmission", _language, FormatCurrentDate(), _currentDemand.Title));
            }
            else
            {
                AddLog(UiStrings.Format("log.noDemand", _language, FormatCurrentDate()));
            }

            var advance = _engine.AdvanceOneWeek(_state);
            foreach (var outcome in advance.DeliveredOutcomes)
            {
                AddLog(DescribeOutcome(outcome));
                RegisterResultMail(outcome);
            }

            foreach (var payment in advance.PaidPayments)
            {
                AddLog(UiStrings.Format(
                    "log.paymentReceived", _language, FormatMoney(payment.Amount), payment.DemandId));
            }

            if (_state.CurrentWeek == 4 && _carloFavorAccepted && !_carloFavorConsequenceApplied)
            {
                _state.ApplyReputationChange(-3);
                _carloFavorConsequenceApplied = true;
                AddLog(UiStrings.Get("log.carloConsequence", _language));
            }

            _slotAssignments.Clear();
            _selectedPlayerId = null;
            _selectedMailId = null;
            _pendingEmptyConfirmation = false;
            _activeWorkspace = WorkspaceMode.Information;
            _informationMode = InformationMode.Mail;

            if (IsGameComplete)
            {
                LastStatus = UiStrings.Format(
                    "status.gameComplete",
                    _language,
                    FormatMoney(_state.Cash),
                    FormatMoney(_state.OutstandingReceivables),
                    _state.Reputation);
                AddLog(UiStrings.Format(
                    "log.seasonComplete",
                    _language,
                    FormatDate(_seasonCalendar.DateAfterFinalWeek)));
            }
            else
            {
                var nextPhase = CurrentSeasonPhase;
                if (nextPhase != startingPhase)
                {
                    AddLog(UiStrings.Format(
                        "log.phaseChange",
                        _language,
                        FormatCurrentDate(),
                        SeasonPhaseName(nextPhase)));
                }

                LastStatus = UiStrings.Format(
                    "status.newWeek",
                    _language,
                    FormatCurrentDate(),
                    SeasonPhaseName(nextPhase));
            }

            Debug.Log(
                $"[NewPlayerHunter] 已推进至第 {_state.CurrentWeek} 周。Cash={_state.Cash:0.00}, Receivables={_state.OutstandingReceivables:0.00}, Reputation={_state.Reputation}.");
            RefreshUi();
            SaveProgress();
        }

        private void RefreshUi()
        {
            if (!_uiBound)
            {
                return;
            }

            UpdateCurrentDemand();
            _weekText.text = IsGameComplete
                ? UiStrings.Format(
                    "status.seasonEndLine",
                    _language,
                    FormatDate(_seasonCalendar.DateAfterFinalWeek))
                : UiStrings.Format(
                    "header.weekLine",
                    _language,
                    FormatCurrentDate(),
                    SeasonPhaseName(CurrentSeasonPhase),
                    _state.CurrentWeek,
                    FinalPlayableWeek);
            _economyText.text = UiStrings.Format(
                "header.economy",
                _language,
                FormatMoney(_state.Cash),
                FormatMoney(_state.OutstandingReceivables),
                _state.Reputation);
            _statusText.text = LastStatus;
            _eventLogText.text = string.Join(
                "\n",
                _eventLog.TakeLast(4).Select(entry => "• " + entry));
            _endWeekButton.interactable = !IsGameComplete;

            RefreshDemandPanel();
            RefreshDemandList();
            RefreshSlots();
            RefreshPlayerCards();
            RefreshMailBrowser();
            RefreshMagazineBrowser();
            ApplyWorkspaceVisibility();
        }

        private void UpdateCurrentDemand()
        {
            _eligibleDemands.Clear();
            _eligibleDemands.AddRange(_weeklyDemands
                .Where(demand =>
                    _unlockedDemandIds.Contains(demand.Id) &&
                    demand.OpenedWeek <= CurrentWeek &&
                    demand.DeadlineWeek >= CurrentWeek &&
                    !_state.CommittedAssignments.Any(assignment =>
                        assignment.DemandId == demand.Id))
                .OrderBy(demand => demand.DeadlineWeek)
                .ThenBy(demand => demand.OpenedWeek));

            var selected = _eligibleDemands.FirstOrDefault(demand =>
                demand.Id == _selectedDemandId);
            if (selected == null)
            {
                selected = _eligibleDemands.FirstOrDefault();
                _selectedDemandId = selected == null ? null : selected.Id;
            }

            _currentDemand = selected;
        }

        private void RefreshDemandPanel()
        {
            if (IsGameComplete)
            {
                _demandTitleText.text = UiStrings.Get("demand.yearSummary", _language);
                _demandBodyText.text = UiStrings.Format(
                    "demand.yearSummaryBody",
                    _language,
                    FormatMoney(_state.Cash),
                    FormatMoney(_state.OutstandingReceivables),
                    _state.Reputation);
                _selectionText.text = UiStrings.Get("demand.seasonOver", _language);
                return;
            }

            if (_currentDemand == null)
            {
                _demandTitleText.text = UiStrings.Get("demand.noDemandTitle", _language);
                _demandBodyText.text = UiStrings.Get("demand.noDemandBody", _language);
                _selectionText.text = UiStrings.Get("demand.noDemandSelection", _language);
                return;
            }

            var entry = _demandContentById[_currentDemand.Id];
            _demandTitleText.text =
                $"{Resolve(entry.clubDisplayName)} · {Resolve(entry.title)}";
            _demandBodyText.text =
                $"{Resolve(entry.clubStanding)} · {Resolve(entry.clubBestAchievement)}\n" +
                $"{Resolve(entry.description)}\n" +
                UiStrings.Format(
                    "demand.bodyFormat",
                    _language,
                    entry.activeWeeks,
                    FormatWeekDate(entry.DeadlineWeek),
                    FormatMoney(_currentDemand.BaseReward),
                    Resolve(entry.paymentTerms));
            _selectionText.text = string.IsNullOrEmpty(_selectedPlayerId)
                ? UiStrings.Get("demand.noPlayerSelected", _language)
                : UiStrings.Format(
                    "demand.playerSelected", _language, GetPlayerDisplayName(_selectedPlayerId));
        }

        private void RefreshDemandList()
        {
            if (!IsGameComplete && _eligibleDemands.Count > _demandListContainer.childCount)
            {
                Debug.LogWarning(
                    $"[NewPlayerHunter] 有效委托超过 {_demandListContainer.childCount} 条预置容量，仅显示截止最近的前 {_demandListContainer.childCount} 条。");
            }

            for (var index = 0; index < _demandListContainer.childCount; index++)
            {
                var item = (RectTransform)_demandListContainer.GetChild(index);
                var isUsed = !IsGameComplete && index < _eligibleDemands.Count;
                item.gameObject.SetActive(isUsed);
                if (!isUsed)
                {
                    continue;
                }

                var demand = _eligibleDemands[index];
                var entry = _demandContentById[demand.Id];
                item.Find("Title").GetComponent<TextMeshProUGUI>().text =
                    $"{Resolve(entry.clubDisplayName)} · {Resolve(entry.title)}";
                item.Find("Meta").GetComponent<TextMeshProUGUI>().text = UiStrings.Format(
                    "demand.itemMeta",
                    _language,
                    FormatWeekDate(demand.DeadlineWeek),
                    FormatMoney(demand.BaseReward),
                    demand.Slots.Count);
                var button = item.GetComponent<Button>();
                var capturedId = demand.Id;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    Punch(item);
                    SelectDemand(capturedId);
                });
                item.GetComponent<Image>().color = demand.Id == _selectedDemandId
                    ? new Color(0.16f, 0.36f, 0.25f, 1f)
                    : PanelLightColor;
            }
        }

        private void RefreshSlots()
        {
            var count = _currentDemand == null ? 0 : _currentDemand.Slots.Count;
            if (_slotsContainer.childCount < count)
            {
                throw new InvalidOperationException(
                    $"Game scene needs {count} authored demand slots.");
            }

            for (var index = 0; index < _slotsContainer.childCount; index++)
            {
                var panel = (RectTransform)_slotsContainer.GetChild(index);
                var isUsed = index < count;
                panel.gameObject.SetActive(isUsed);
                if (!isUsed)
                {
                    continue;
                }

                var slot = _currentDemand.Slots[index];
                var isFilled = _slotAssignments.TryGetValue(slot.Id, out var playerId);
                panel.GetComponent<Image>().color = isFilled ? ReadColor : PanelLightColor;
                var outline = panel.GetComponent<Outline>();
                outline.effectColor = isFilled
                    ? AccentColor
                    : new Color(0.25f, 0.32f, 0.38f, 1f);
                panel.GetComponent<DemandSlotDropTarget>().Configure(this, slot.Id);
                panel.Find("Requirement").GetComponent<TextMeshProUGUI>().text =
                    $"{PositionName(slot.RequiredPosition)} · " +
                    UiStrings.Get(slot.IsRequired ? "slot.required" : "slot.optional", _language);
                var assignment = panel.Find("Assignment").GetComponent<TextMeshProUGUI>();
                assignment.text = isFilled
                    ? UiStrings.Format("slot.filled", _language, GetPlayerDisplayName(playerId))
                    : UiStrings.Get("slot.hint", _language);
                assignment.color = isFilled ? Color.white : MutedColor;
            }
        }

        private void RefreshPlayerCards()
        {
            var availablePlayers = GetAvailablePlayersInReceivedOrder();
            if (_playersContainer.childCount < availablePlayers.Count)
            {
                throw new InvalidOperationException(
                    $"Game scene needs {availablePlayers.Count} authored player card views.");
            }

            for (var index = 0; index < _playersContainer.childCount; index++)
            {
                var card = (RectTransform)_playersContainer.GetChild(index);
                var isUsed = index < availablePlayers.Count;
                card.gameObject.SetActive(isUsed);
                if (!isUsed)
                {
                    continue;
                }

                var player = availablePlayers[index];
                card.GetComponent<PlayerCardDragHandler>().Configure(this, player.PlayerId);
                var playerContent = _playerContentById[player.PlayerId];
                ApplyAtlasImage(card.Find("Portrait/Image").GetComponent<RawImage>(),
                    contentCatalog.PlayerPortraitAtlas, playerContent.portraitIndex,
                    PlayerAtlasColumns, PlayerAtlasRows);
                card.Find("Name").GetComponent<TextMeshProUGUI>().text =
                    Resolve(playerContent.displayName);
                card.Find("Position").GetComponent<TextMeshProUGUI>().text =
                    string.Join(" / ", player.ClaimedPositions.Select(PositionName));
                var claimText = Resolve(playerContent.publicClaim);
                card.Find("Claim").GetComponent<TextMeshProUGUI>().text =
                    string.IsNullOrWhiteSpace(claimText)
                        ? Resolve(playerContent.biography)
                        : claimText;
                var isSelected = player.PlayerId == _selectedPlayerId;
                card.GetComponent<Image>().color = isSelected
                    ? new Color(0.16f, 0.36f, 0.25f, 1f)
                    : PanelLightColor;
            }

            var label = RequireSceneComponent<TextMeshProUGUI>(
                "GameCanvas/Background/AssignmentWorkspace/PlayersPanel/PlayersLabel");
            label.text = UiStrings.Format(
                "players.label",
                _language,
                availablePlayers.Count,
                _unlockedPlayerIds.Count,
                _players.Count);
        }

        private bool IsPlayerAvailable(string playerId)
        {
            if (!_unlockedPlayerIds.Contains(playerId) ||
                _slotAssignments.ContainsValue(playerId) ||
                _state.CommittedAssignments.Any(assignment =>
                    assignment.PlayerId == playerId))
            {
                return false;
            }

            return _playerContentById.TryGetValue(playerId, out var content) &&
                   CurrentWeek >= content.availableFromWeek &&
                   CurrentWeek <= content.LastAvailableWeek;
        }

        private IReadOnlyList<PlayerPublicViewModel> GetAvailablePlayersInReceivedOrder()
        {
            var playerById = _players.ToDictionary(
                player => player.PlayerId,
                StringComparer.Ordinal);
            var orderedPlayerIds = contentCatalog.Mails
                .Where(mail =>
                    mail.publishedWeek <= CurrentWeek &&
                    !string.IsNullOrEmpty(mail.relatedPlayerId) &&
                    IsPlayerAvailable(mail.relatedPlayerId))
                .OrderBy(mail => mail.publishedWeek)
                .Select(mail => mail.relatedPlayerId)
                .Distinct(StringComparer.Ordinal);

            return orderedPlayerIds
                .Where(playerById.ContainsKey)
                .Select(playerId => playerById[playerId])
                .ToArray();
        }

        private bool ShouldDisplayMail(MailContentEntry mail)
        {
            if (mail == null || mail.publishedWeek > CurrentWeek)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(mail.expiredPlayerId))
            {
                return _playerContentById.TryGetValue(
                           mail.expiredPlayerId,
                           out var player) &&
                       CurrentWeek > player.LastAvailableWeek &&
                       !_state.CommittedAssignments.Any(assignment =>
                           assignment.PlayerId == mail.expiredPlayerId);
            }

            if (!string.IsNullOrEmpty(mail.expiredDemandId))
            {
                return _demandContentById.TryGetValue(
                           mail.expiredDemandId,
                           out var demand) &&
                       CurrentWeek > demand.DeadlineWeek &&
                       !_state.CommittedAssignments.Any(assignment =>
                           assignment.DemandId == mail.expiredDemandId);
            }

            return true;
        }

        private void RefreshMailBrowser()
        {
            _visibleMails.Clear();
            _visibleMails.AddRange(contentCatalog.Mails
                .Where(ShouldDisplayMail)
                .Concat(_resultMails.Where(mail => mail.publishedWeek <= CurrentWeek))
                .OrderByDescending(mail => _readMailIds.Contains(mail.id) ? 0 : 1)
                .ThenByDescending(mail => mail.publishedWeek));

            if (_visibleMails.Count > _mailListContainer.childCount)
            {
                Debug.LogWarning(
                    $"[NewPlayerHunter] 邮件超过 {_mailListContainer.childCount} 封预置容量，最旧的已读邮件暂不显示。");
                _visibleMails.RemoveRange(
                    _mailListContainer.childCount,
                    _visibleMails.Count - _mailListContainer.childCount);
            }

            for (var index = 0; index < _mailListContainer.childCount; index++)
            {
                var listItem = (RectTransform)_mailListContainer.GetChild(index);
                var isUsed = index < _visibleMails.Count;
                listItem.gameObject.SetActive(isUsed);
                if (!isUsed)
                {
                    continue;
                }

                var mail = _visibleMails[index];
                var isRead = _readMailIds.Contains(mail.id);
                var sender = Resolve(mail.sender);
                var avatar = listItem.Find("Avatar");
                avatar.Find("Initials").GetComponent<TextMeshProUGUI>().text =
                    AvatarInitial(sender);
                avatar.GetComponent<Image>().color = MailAvatarColor(mail.kind);
                listItem.Find("ReadDot").GetComponent<Image>().color =
                    isRead ? AccentColor : new Color(1f, 0.72f, 0.25f, 1f);
                listItem.Find("ReadState").GetComponent<TextMeshProUGUI>().text =
                    UiStrings.Get(isRead ? "mail.read" : "mail.unread", _language);
                listItem.Find("Sender").GetComponent<TextMeshProUGUI>().text =
                    sender;
                listItem.Find("Timestamp").GetComponent<TextMeshProUGUI>().text =
                    $"{FormatWeekDate(mail.publishedWeek)} {Resolve(mail.receivedTime)}";
                listItem.Find("Subject").GetComponent<TextMeshProUGUI>().text =
                    Resolve(mail.subject);
                listItem.Find("Preview").GetComponent<TextMeshProUGUI>().text =
                    BuildMailSummary(Resolve(mail.body));
                var button = listItem.GetComponent<Button>();
                var capturedId = mail.id;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    Punch(listItem);
                    OpenMail(capturedId);
                });
                listItem.GetComponent<Image>().color = mail.id == _selectedMailId
                    ? new Color(0.13f, 0.31f, 0.23f, 1f)
                    : isRead ? ReadColor : PanelLightColor;
            }

            if (_informationMode == InformationMode.Mail)
            {
                _informationCounterText.text = UiStrings.Format(
                    "mail.counter",
                    _language,
                    FormatCurrentDate(),
                    _visibleMails.Count,
                    _readMailIds.Count);
            }

            var selected = _visibleMails.FirstOrDefault(mail => mail.id == _selectedMailId);
            if (selected == null)
            {
                ShowEmptyMailPane();
                return;
            }

            _mailSenderText.text =
                UiStrings.Get("mail.senderPrefix", _language) + Resolve(selected.sender);
            _mailSubjectText.text = Resolve(selected.subject);
            _mailMetaText.text =
                $"{FormatWeekDate(selected.publishedWeek)} · {MailKindName(selected.kind)} · {Resolve(selected.sourceNote)}";
            _mailBodyText.text = Resolve(selected.body);
            RefreshMailDemandBlock(selected);
            RefreshMailResumeBlock(selected);
            RefreshMailPrivateOfferBlock(selected);
        }

        private void ShowEmptyMailPane()
        {
            _mailSenderText.text = UiStrings.Get("mail.emptySender", _language);
            _mailSubjectText.text = UiStrings.Get("mail.emptySubject", _language);
            _mailMetaText.text = UiStrings.Get("mail.emptyMeta", _language);
            _mailBodyText.text = UiStrings.Get("mail.emptyBody", _language);
            _mailDemandBlock.gameObject.SetActive(false);
            _mailResumeBlock.gameObject.SetActive(false);
            _mailPrivateOfferBlock.gameObject.SetActive(false);
        }

        private void RefreshMailDemandBlock(MailContentEntry mail)
        {
            DemandContentEntry demand = null;
            var hasDemand = !string.IsNullOrEmpty(mail.relatedDemandId) &&
                _demandContentById.TryGetValue(mail.relatedDemandId, out demand);
            _mailDemandBlock.gameObject.SetActive(hasDemand);
            if (!hasDemand)
            {
                return;
            }

            _mailDemandBlock.Find("Title").GetComponent<TextMeshProUGUI>().text =
                UiStrings.Get("mail.demandBlock.title", _language);
            _mailDemandBlock.Find("ClubProfile").GetComponent<TextMeshProUGUI>().text =
                $"{Resolve(demand.clubDisplayName)} · {Resolve(demand.clubStanding)}\n" +
                $"{Resolve(demand.clubBestAchievement)}";
            _mailDemandBlock.Find("Slots").GetComponent<TextMeshProUGUI>().text =
                UiStrings.Get("mail.demandBlock.slotsPrefix", _language) +
                string.Join(UiStrings.Get("list.and", _language), demand.slots.Select(slot =>
                    PositionName(slot.requiredPosition) +
                    UiStrings.Get(
                        slot.isRequired
                            ? "mail.demandBlock.requiredSuffix"
                            : "mail.demandBlock.optionalSuffix",
                        _language)));
            var remainingWeeks = Math.Max(0, demand.DeadlineWeek - CurrentWeek + 1);
            _mailDemandBlock.Find("Deadline").GetComponent<TextMeshProUGUI>().text =
                UiStrings.Format(
                    "mail.demandBlock.deadline",
                    _language,
                    demand.activeWeeks,
                    FormatWeekDate(demand.DeadlineWeek),
                    remainingWeeks);
            _mailDemandBlock.Find("Price").GetComponent<TextMeshProUGUI>().text =
                UiStrings.Format(
                    "mail.demandBlock.price", _language, FormatMoney(demand.baseReward));
            _mailDemandBlock.Find("Payment").GetComponent<TextMeshProUGUI>().text =
                UiStrings.Get("mail.demandBlock.payment", _language) +
                Resolve(demand.paymentTerms);
        }

        private void RefreshMailResumeBlock(MailContentEntry mail)
        {
            PlayerContentEntry player = null;
            var hasPlayer = !string.IsNullOrEmpty(mail.relatedPlayerId) &&
                _playerContentById.TryGetValue(mail.relatedPlayerId, out player);
            _mailResumeBlock.gameObject.SetActive(hasPlayer);
            if (!hasPlayer)
            {
                return;
            }

            ApplyAtlasImage(_mailResumeBlock.Find("Portrait/Image").GetComponent<RawImage>(),
                contentCatalog.PlayerPortraitAtlas, player.portraitIndex,
                PlayerAtlasColumns, PlayerAtlasRows);
            _mailResumeBlock.Find("Title").GetComponent<TextMeshProUGUI>().text =
                UiStrings.Get("mail.resumeBlock.title", _language);
            _mailResumeBlock.Find("Player").GetComponent<TextMeshProUGUI>().text =
                Resolve(player.displayName);
            _mailResumeBlock.Find("Position").GetComponent<TextMeshProUGUI>().text =
                UiStrings.Get("mail.resumeBlock.position", _language) +
                PositionName(player.publicPosition);
            _mailResumeBlock.Find("Biography").GetComponent<TextMeshProUGUI>().text =
                Resolve(player.biography);
            _mailResumeBlock.Find("Salary").GetComponent<TextMeshProUGUI>().text =
                UiStrings.Format(
                    "mail.resumeBlock.salary",
                    _language,
                    player.salaryMinWeekly,
                    player.salaryMaxWeekly);
            _mailResumeBlock.Find("Career").GetComponent<TextMeshProUGUI>().text =
                UiStrings.Get("mail.resumeBlock.career", _language) +
                Resolve(player.careerHistory);
            _mailResumeBlock.Find("Claim").GetComponent<TextMeshProUGUI>().text =
                UiStrings.Get("mail.resumeBlock.claim", _language) +
                Resolve(player.publicClaim);
            _mailResumeBlock.Find("Evidence").GetComponent<TextMeshProUGUI>().text =
                UiStrings.Get("mail.resumeBlock.evidence", _language) +
                Resolve(player.publicEvidence);
            _mailResumeBlock.Find("Source").GetComponent<TextMeshProUGUI>().text =
                UiStrings.Get("mail.resumeBlock.source", _language) +
                ReliabilityName(player.evidenceReliability);
            var remainingWeeks = Math.Max(0, player.LastAvailableWeek - CurrentWeek + 1);
            _mailResumeBlock.Find("Availability").GetComponent<TextMeshProUGUI>().text =
                UiStrings.Format(
                    "mail.resumeBlock.availability",
                    _language,
                    FormatWeekDate(player.LastAvailableWeek),
                    remainingWeeks);
        }

        private void RefreshMailPrivateOfferBlock(MailContentEntry mail)
        {
            var hasOffer = mail.privateOfferAmount > 0;
            _mailPrivateOfferBlock.gameObject.SetActive(hasOffer);
            if (!hasOffer)
            {
                return;
            }

            _mailPrivateOfferBlock.Find("Title").GetComponent<TextMeshProUGUI>().text =
                UiStrings.Get("mail.offerBlock.title", _language);
            _mailPrivateOfferBlock.Find("Offer").GetComponent<TextMeshProUGUI>().text =
                UiStrings.Get("mail.offerBlock.offer", _language) +
                FormatMoney(mail.privateOfferAmount);
            _mailPrivateOfferBlock.Find("Terms").GetComponent<TextMeshProUGUI>().text =
                UiStrings.Get("mail.offerBlock.terms", _language) +
                Resolve(mail.privateOfferTerms);
            _mailPrivateOfferBlock.Find("TargetClub").GetComponent<TextMeshProUGUI>().text =
                Resolve(mail.privateTargetClubRequirement);
            _mailPrivateOfferBlock.Find("Risk").GetComponent<TextMeshProUGUI>().text =
                UiStrings.Get("mail.offerBlock.risk", _language) +
                Resolve(mail.privateRiskNote);
        }

        private void RefreshMagazineBrowser()
        {
            _visibleIssues.Clear();
            _visibleIssues.AddRange(contentCatalog.MagazineIssues
                .Where(issue => issue.publishedWeek <= CurrentWeek)
                .OrderByDescending(issue => issue.publishedWeek));
            EnsureSelectedIssue();

            for (var index = 0; index < _issueListContainer.childCount; index++)
            {
                var item = (RectTransform)_issueListContainer.GetChild(index);
                var isUsed = index < _visibleIssues.Count;
                item.gameObject.SetActive(isUsed);
                if (!isUsed)
                {
                    continue;
                }

                var issue = _visibleIssues[index];
                item.Find("Publication").GetComponent<TextMeshProUGUI>().text =
                    Resolve(issue.publicationName);
                item.Find("Issue").GetComponent<TextMeshProUGUI>().text =
                    FormatIssueNumber(issue.issueNumber) + " · " + Resolve(issue.issueTitle);
                var button = item.GetComponent<Button>();
                var capturedId = issue.id;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SelectIssue(capturedId));
                item.GetComponent<Image>().color = issue.id == _selectedIssueId
                    ? new Color(0.22f, 0.19f, 0.09f, 1f)
                    : PanelLightColor;
            }

            if (_informationMode == InformationMode.Magazine)
            {
                _informationCounterText.text = UiStrings.Format(
                    "magazine.counter", _language, _visibleIssues.Count);
            }

            var page = GetSelectedPage();
            var issueSelected = GetSelectedIssue();
            if (page == null || issueSelected == null)
            {
                _coverLayout.gameObject.SetActive(false);
                _featureLayout.gameObject.SetActive(false);
                _scoutReportLayout.gameObject.SetActive(false);
                _magazinePageIndicator.text = UiStrings.Get("magazine.empty", _language);
                _previousPageButton.interactable = false;
                _nextPageButton.interactable = false;
                return;
            }

            _coverLayout.gameObject.SetActive(page.layout == MagazinePageLayout.Cover);
            _featureLayout.gameObject.SetActive(page.layout == MagazinePageLayout.Feature);
            _scoutReportLayout.gameObject.SetActive(page.layout == MagazinePageLayout.ScoutReport);
            _magazinePageIndicator.text = UiStrings.Format(
                "magazine.pageIndicator",
                _language,
                Resolve(issueSelected.publicationName),
                FormatIssueNumber(issueSelected.issueNumber),
                _magazinePageIndex + 1,
                issueSelected.pages.Count);
            _previousPageButton.interactable = _magazinePageIndex > 0;
            _nextPageButton.interactable = _magazinePageIndex < issueSelected.pages.Count - 1;

            if (page.layout == MagazinePageLayout.Cover)
            {
                ApplyAtlasImage(_coverImage, contentCatalog.MagazineCoverAtlas,
                    issueSelected.coverIndex, CoverAtlasColumns, CoverAtlasRows);
                SetText(_coverLayout, "Publication", Resolve(issueSelected.publicationName));
                SetText(_coverLayout, "IssueNumber", FormatIssueNumber(issueSelected.issueNumber));
                SetText(_coverLayout, "Headline", Resolve(page.headline));
                SetText(_coverLayout, "Deck", Resolve(page.deck));
                SetText(_coverLayout, "CoverNote", Resolve(page.bodyLeft));
            }
            else
            {
                var layout = page.layout == MagazinePageLayout.Feature
                    ? _featureLayout
                    : _scoutReportLayout;
                SetText(layout, "Kicker", Resolve(page.kicker));
                SetText(layout, "Headline", Resolve(page.headline));
                SetText(layout, "Deck", Resolve(page.deck));
                SetText(layout, "BodyLeft", Resolve(page.bodyLeft));
                SetText(layout, "BodyRight", Resolve(page.bodyRight));
                SetText(layout, "PullQuote", Resolve(page.pullQuote));
                SetText(layout, "SidebarTitle", Resolve(page.sidebarTitle));
                SetText(layout, "SidebarBody", Resolve(page.sidebarBody));
                if (page.layout == MagazinePageLayout.Feature)
                {
                    BindFeatureFigures(layout, page);
                }
                else
                {
                    BindScoutReportFigure(layout, page);
                }
            }
        }

        private void BindFeatureFigures(RectTransform layout, MagazinePageContent page)
        {
            var atlas = contentCatalog.MagazineIllustrationAtlas;
            _featureIllustration.gameObject.SetActive(false);
            _featureIllustration2.gameObject.SetActive(false);
            if (page.illustrationIndex < 0 || atlas == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            var left = layout.Find("BodyLeft").GetComponent<TextMeshProUGUI>();
            var right = layout.Find("BodyRight").GetComponent<TextMeshProUGUI>();
            var layoutRect = layout.rect;
            if (layoutRect.height <= 0f || layoutRect.width <= 0f)
            {
                return;
            }

            const float margin = 0.03f;
            const float bottom = 0.02f;
            var leftBottom = TextBottom(left, layoutRect.height);
            var rightBottom = TextBottom(right, layoutRect.height);
            if (page.illustrationIndex2 >= 0)
            {
                PlaceFigure(
                    _featureIllustration, left.rectTransform,
                    leftBottom - margin, bottom, page.illustrationIndex);
                PlaceFigure(
                    _featureIllustration2, right.rectTransform,
                    rightBottom - margin, bottom, page.illustrationIndex2);
                return;
            }

            var top = Mathf.Min(leftBottom, rightBottom) - margin;
            if (top - bottom < 0.12f)
            {
                return;
            }

            SetFigureRect(
                _featureIllustration,
                left.rectTransform.anchorMin.x, bottom,
                right.rectTransform.anchorMax.x, top);
            ShowFigure(_featureIllustration, page.illustrationIndex);
        }

        private void BindScoutReportFigure(RectTransform layout, MagazinePageContent page)
        {
            var atlas = contentCatalog.MagazineIllustrationAtlas;
            _scoutReportIllustration.gameObject.SetActive(false);
            if (page.illustrationIndex < 0 || atlas == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            var right = layout.Find("BodyRight").GetComponent<TextMeshProUGUI>();
            var layoutRect = layout.rect;
            if (layoutRect.height <= 0f || layoutRect.width <= 0f)
            {
                return;
            }

            var top = TextBottom(right, layoutRect.height) - 0.03f;
            const float bottom = 0.32f;
            if (top - bottom < 0.12f)
            {
                return;
            }

            SetFigureRect(
                _scoutReportIllustration,
                right.rectTransform.anchorMin.x, bottom,
                right.rectTransform.anchorMax.x, top);
            ShowFigure(_scoutReportIllustration, page.illustrationIndex);
        }

        private void PlaceFigure(
            RectTransform figure, RectTransform column, float top, float bottom, int index)
        {
            if (top - bottom < 0.12f)
            {
                return;
            }

            SetFigureRect(figure, column.anchorMin.x, bottom, column.anchorMax.x, top);
            ShowFigure(figure, index);
        }

        private static float TextBottom(TextMeshProUGUI text, float layoutHeight)
        {
            return text.rectTransform.anchorMax.y - text.preferredHeight / layoutHeight;
        }

        private static void SetFigureRect(
            RectTransform figure, float minX, float minY, float maxX, float maxY)
        {
            figure.anchorMin = new Vector2(minX, minY);
            figure.anchorMax = new Vector2(maxX, maxY);
            figure.offsetMin = Vector2.zero;
            figure.offsetMax = Vector2.zero;
        }

        private void ShowFigure(RectTransform figure, int index)
        {
            figure.gameObject.SetActive(true);
            ApplyAtlasImageCropped(
                figure.Find("Frame/Image").GetComponent<RawImage>(),
                contentCatalog.MagazineIllustrationAtlas,
                index, IllustrationAtlasColumns, IllustrationAtlasRows);
            figure.Find("Caption").GetComponent<TextMeshProUGUI>().text =
                UiStrings.Get("magazine.captionPrefix", _language) +
                UiStrings.IllustrationCaption(index, _language);
        }

        private static void ApplyAtlasImageCropped(
            RawImage image, Texture texture, int index, int columns, int rows)
        {
            image.texture = texture;
            if (texture == null)
            {
                image.color = new Color(0.18f, 0.22f, 0.25f, 1f);
                image.uvRect = new Rect(0f, 0f, 1f, 1f);
                return;
            }

            image.color = Color.white;
            var clamped = Mathf.Clamp(index, 0, (columns * rows) - 1);
            var column = clamped % columns;
            var rowFromTop = clamped / columns;
            var cellWidth = 1f / columns;
            var cellHeight = 1f / rows;
            var inset = cellWidth * 0.02f;
            var x = (column * cellWidth) + inset;
            var w = cellWidth - (inset * 2f);
            var yBottom = 1f - ((rowFromTop + 1) * cellHeight) + inset;
            var h = cellHeight - (inset * 2f);
            var rect = image.rectTransform.rect;
            var aspect = rect.height > 0.01f ? rect.width / rect.height : 1.5f;
            if (aspect > 1f)
            {
                var visible = h / aspect;
                yBottom += (h - visible) * 0.5f;
                h = visible;
            }
            else if (aspect < 1f)
            {
                var visible = w * aspect;
                x += (w - visible) * 0.5f;
                w = visible;
            }

            image.uvRect = new Rect(x, yBottom, w, h);
        }

        private void EnsureSelectedIssue()
        {
            if (_visibleIssues.Count == 0)
            {
                _selectedIssueId = null;
                _magazinePageIndex = 0;
                return;
            }

            if (_visibleIssues.All(issue => issue.id != _selectedIssueId))
            {
                _selectedIssueId = _visibleIssues[0].id;
                _magazinePageIndex = 0;
            }

            var issue = GetSelectedIssue();
            if (issue != null)
            {
                _magazinePageIndex = Mathf.Clamp(
                    _magazinePageIndex,
                    0,
                    Mathf.Max(0, issue.pages.Count - 1));
            }
        }

        private MagazineIssueContent GetSelectedIssue()
        {
            return _visibleIssues.FirstOrDefault(issue => issue.id == _selectedIssueId);
        }

        private MagazinePageContent GetSelectedPage()
        {
            var issue = GetSelectedIssue();
            return issue == null || issue.pages.Count == 0
                ? null
                : issue.pages[_magazinePageIndex];
        }

        private void ApplyWorkspaceVisibility()
        {
            if (_assignmentWorkspace == null || _informationWorkspace == null)
            {
                return;
            }

            var showInformation = _activeWorkspace == WorkspaceMode.Information;
            _informationWorkspace.gameObject.SetActive(showInformation);
            _assignmentWorkspace.gameObject.SetActive(!showInformation);
            _endWeekButton.gameObject.SetActive(!showInformation);
            if (showInformation)
            {
                var showMail = _informationMode == InformationMode.Mail;
                _mailBrowser.gameObject.SetActive(showMail);
                _magazineBrowser.gameObject.SetActive(!showMail);
                SetButtonVisual(_mailFilterButton, showMail);
                SetButtonVisual(_subscriptionFilterButton, !showMail);
            }

            SetButtonVisual(_informationTabButton, showInformation);
            SetButtonVisual(_assignmentTabButton, !showInformation);
        }

        private T FindSceneComponent<T>(string relativePath) where T : Component
        {
            var child = transform.Find(relativePath);
            return child == null ? null : child.GetComponent<T>();
        }

        private T RequireSceneComponent<T>(string relativePath) where T : Component
        {
            var child = transform.Find(relativePath);
            if (child == null)
            {
                throw new InvalidOperationException(
                    $"Game scene is missing authored object '{relativePath}'.");
            }

            var component = child.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException(
                    $"Game scene object '{relativePath}' is missing {typeof(T).Name}.");
            }

            return component;
        }

        private static void SetButtonVisual(Button button, bool active)
        {
            if (button == null)
            {
                return;
            }

            var color = active ? AccentColor : PanelLightColor;
            if (button.targetGraphic is Image image)
            {
                image.color = color;
            }

            var label = button.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            if (label != null)
            {
                label.color = active
                    ? new Color(0.035f, 0.055f, 0.075f, 1f)
                    : Color.white;
            }
        }

        private static void SetText(RectTransform layout, string childName, string value)
        {
            var child = layout.Find(childName);
            if (child != null)
            {
                child.GetComponent<TextMeshProUGUI>().text = value;
            }
        }

        private static void ApplyAtlasImage(
            RawImage image,
            Texture texture,
            int index,
            int columns,
            int rows)
        {
            image.texture = texture;
            image.color = texture == null ? new Color(0.18f, 0.22f, 0.25f, 1f) : Color.white;
            if (texture == null)
            {
                image.uvRect = new Rect(0f, 0f, 1f, 1f);
                return;
            }

            var clamped = Mathf.Clamp(index, 0, (columns * rows) - 1);
            var column = clamped % columns;
            var rowFromTop = clamped / columns;
            var cellWidth = 1f / columns;
            var cellHeight = 1f / rows;
            var inset = Mathf.Min(cellWidth, cellHeight) * 0.035f;
            image.uvRect = new Rect(
                (column * cellWidth) + inset,
                1f - ((rowFromTop + 1) * cellHeight) + inset,
                cellWidth - (inset * 2f),
                cellHeight - (inset * 2f));
        }

        private string AvatarInitial(string sender)
        {
            if (string.IsNullOrWhiteSpace(sender))
            {
                return UiStrings.Get("mail.avatarFallback", _language);
            }

            var trimmed = sender.TrimStart('《', '【', '[', '(');
            return string.IsNullOrEmpty(trimmed)
                ? UiStrings.Get("mail.avatarFallback", _language)
                : trimmed[0].ToString();
        }

        private static Color MailAvatarColor(MailContentKind kind)
        {
            switch (kind)
            {
                case MailContentKind.ClubRequest:
                    return new Color(0.12f, 0.42f, 0.31f, 1f);
                case MailContentKind.PlayerResume:
                    return new Color(0.18f, 0.34f, 0.52f, 1f);
                case MailContentKind.PrivateRequest:
                    return new Color(0.55f, 0.30f, 0.12f, 1f);
                case MailContentKind.ClubFeedback:
                    return new Color(0.58f, 0.47f, 0.16f, 1f);
                default:
                    return new Color(0.32f, 0.34f, 0.38f, 1f);
            }
        }

        private string BuildMailSummary(string body)
        {
            var normalized = string.IsNullOrWhiteSpace(body)
                ? UiStrings.Get("mail.noBody", _language)
                : string.Join(" ", body.Split(
                    new[] { ' ', '\r', '\n', '\t' },
                    StringSplitOptions.RemoveEmptyEntries));
            const int maximumCharacters = 34;
            if (normalized.Length > maximumCharacters)
            {
                normalized = normalized.Substring(0, maximumCharacters);
            }

            return normalized.TrimEnd('。', '！', '？', '.', '…') +
                   UiStrings.Get("mail.ellipsis", _language);
        }

        private string FormatIssueNumber(string issueNumber)
        {
            if (string.IsNullOrEmpty(issueNumber))
            {
                return string.Empty;
            }

            var digits = new string(issueNumber.Where(char.IsDigit).ToArray());
            return digits.Length == 0
                ? issueNumber
                : UiStrings.Format("magazine.issueNumber", _language, digits);
        }

        private string Resolve(LocalizedText text)
        {
            return text == null
                ? string.Empty
                : text.Resolve(_language);
        }

        private string PositionName(PlayerPosition position)
        {
            switch (position)
            {
                case PlayerPosition.Goalkeeper:
                    return UiStrings.Get("position.goalkeeper", _language);
                case PlayerPosition.Defender:
                    return UiStrings.Get("position.defender", _language);
                case PlayerPosition.WingBack:
                    return UiStrings.Get("position.wingBack", _language);
                case PlayerPosition.Midfielder:
                    return UiStrings.Get("position.midfielder", _language);
                case PlayerPosition.Winger:
                    return UiStrings.Get("position.winger", _language);
                default:
                    return UiStrings.Get("position.forward", _language);
            }
        }

        private string MailKindName(MailContentKind kind)
        {
            switch (kind)
            {
                case MailContentKind.ClubRequest:
                    return UiStrings.Get("mailKind.clubRequest", _language);
                case MailContentKind.PlayerResume:
                    return UiStrings.Get("mailKind.playerResume", _language);
                case MailContentKind.PrivateRequest:
                    return UiStrings.Get("mailKind.privateRequest", _language);
                case MailContentKind.ClubFeedback:
                    return UiStrings.Get("mailKind.clubFeedback", _language);
                default:
                    return UiStrings.Get("mailKind.general", _language);
            }
        }

        private string ReliabilityName(EvidenceReliability reliability)
        {
            switch (reliability)
            {
                case EvidenceReliability.High:
                    return UiStrings.Get("reliability.high", _language);
                case EvidenceReliability.Medium:
                    return UiStrings.Get("reliability.medium", _language);
                case EvidenceReliability.Low:
                    return UiStrings.Get("reliability.low", _language);
                default:
                    return UiStrings.Get("reliability.unverified", _language);
            }
        }

        private string DescribeOutcome(PlacementOutcome outcome)
        {
            var playerName = GetPlayerDisplayName(outcome.Assignment.PlayerId);
            switch (outcome.ResultKind)
            {
                case PlacementResultKind.Accepted:
                    return UiStrings.Format("log.outcomeAccepted", _language, playerName);
                case PlacementResultKind.TrialExtended:
                    return UiStrings.Format("log.outcomeExtended", _language, playerName);
                default:
                    return UiStrings.Format("log.outcomeRejected", _language, playerName);
            }
        }

        private string TranslateSubmissionErrors(
            IReadOnlyList<SubmissionError> errors)
        {
            if (errors.Any(error => error.Code == SubmissionErrorCode.SubmissionHasNoAssignments))
            {
                return UiStrings.Get("error.noAssignments", _language);
            }

            if (errors.Any(error => error.Code == SubmissionErrorCode.DuplicatePlayerInWeek))
            {
                return UiStrings.Get("error.duplicatePlayer", _language);
            }

            if (errors.Any(error => error.Code == SubmissionErrorCode.PlayerAlreadyCommitted))
            {
                return UiStrings.Get("error.playerCommitted", _language);
            }

            return UiStrings.Get("error.generic", _language);
        }

        private void AddLog(string message)
        {
            _eventLog.Add(message);
        }

        private void RegisterResultMail(PlacementOutcome outcome)
        {
            var mailId = ResultMailFactory.BuildMailId(outcome);
            if (_resultMails.Any(mail => mail.id == mailId))
            {
                return;
            }

            _demandContentById.TryGetValue(outcome.Assignment.DemandId, out var demandContent);
            var mail = ResultMailFactory.Create(
                outcome,
                demandContent == null ? outcome.Assignment.DemandId : Resolve(demandContent.title),
                demandContent == null ? string.Empty : Resolve(demandContent.clubDisplayName),
                GetPlayerDisplayName(outcome.Assignment.PlayerId),
                _language);
            _resultMails.Add(mail);
            AddLog(UiStrings.Format(
                "log.resultMailReceived", _language, Resolve(mail.sender)));
        }

        private void RegenerateResultMails()
        {
            _resultMails.Clear();
            foreach (var outcome in _state.DeliveredOutcomes)
            {
                var mailId = ResultMailFactory.BuildMailId(outcome);
                if (_resultMails.Any(mail => mail.id == mailId))
                {
                    continue;
                }

                _demandContentById.TryGetValue(outcome.Assignment.DemandId, out var demandContent);
                _resultMails.Add(ResultMailFactory.Create(
                    outcome,
                    demandContent == null ? outcome.Assignment.DemandId : Resolve(demandContent.title),
                    demandContent == null ? string.Empty : Resolve(demandContent.clubDisplayName),
                    GetPlayerDisplayName(outcome.Assignment.PlayerId),
                    _language));
            }
        }

        private static string FormatMoney(decimal amount)
        {
            return "€" + amount.ToString("0.00");
        }

        private string FormatCurrentDate()
        {
            return FormatWeekDate(Math.Min(CurrentWeek, FinalPlayableWeek));
        }

        private string FormatWeekDate(int week)
        {
            return FormatDate(SeasonStartDate.AddDays((Math.Max(1, week) - 1) * 7));
        }

        private string FormatDate(DateTime date)
        {
            var format = UiStrings.Get("date.full", _language);
            return _language == GameLanguage.English
                ? date.ToString(format, CultureInfo.InvariantCulture)
                : date.ToString(format);
        }

        private string SeasonPhaseName(SeasonPhase phase)
        {
            switch (phase)
            {
                case SeasonPhase.Preseason:
                    return UiStrings.Get("phase.preseason", _language);
                case SeasonPhase.SummerWindow:
                    return UiStrings.Get("phase.summerWindow", _language);
                case SeasonPhase.LeagueOpening:
                    return UiStrings.Get("phase.leagueOpening", _language);
                case SeasonPhase.GroupStage:
                    return UiStrings.Get("phase.groupStage", _language);
                case SeasonPhase.WinterSchedule:
                    return UiStrings.Get("phase.winterSchedule", _language);
                case SeasonPhase.WinterWindow:
                    return UiStrings.Get("phase.winterWindow", _language);
                case SeasonPhase.KnockoutStage:
                    return UiStrings.Get("phase.knockoutStage", _language);
                case SeasonPhase.RunIn:
                    return UiStrings.Get("phase.runIn", _language);
                case SeasonPhase.Finals:
                    return UiStrings.Get("phase.finals", _language);
                default:
                    return UiStrings.Get("phase.summary", _language);
            }
        }
    }
}
