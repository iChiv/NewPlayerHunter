using System;
using System.Collections;
using System.Collections.Generic;
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
        private Button _informationTabButton;
        private Button _assignmentTabButton;
        private Button _mailFilterButton;
        private Button _subscriptionFilterButton;
        private Button _endWeekButton;
        private Button _previousPageButton;
        private Button _nextPageButton;
        private bool _carloFavorAccepted;
        private bool _carloFavorConsequenceApplied;

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

            BuildGameState();
            LoadProgressIfAvailable();
            BindSceneUi();
            RefreshUi();
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
                ? "已取消选择球员。"
                : $"已选择 {GetPlayerDisplayName(_selectedPlayerId)}，请点击或拖入招聘槽位。";
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
                LastStatus = "已清空该招聘槽位。";
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
            LastStatus = $"已把 {GetPlayerDisplayName(playerId)} 放入招聘槽位。";
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
            AddLog($"已载入第 {_state.CurrentWeek} 周存档。");
        }

        public string GetPlayerDisplayName(string playerId)
        {
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

            AddLog("2026年7月6日：季前训练开始。先阅读邮件，需求和简历才会进入分配工作台。");
            LastStatus = "新赛季从季前训练开始，共 52 周。请先读邮件，再根据期刊线索交叉判断。";
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
            _eventLogText = RequireSceneComponent<TextMeshProUGUI>(
                "GameCanvas/Background/Footer/EventLog");
            _statusText = RequireSceneComponent<TextMeshProUGUI>(
                "GameCanvas/Background/Footer/Status");
            _endWeekButton = RequireSceneComponent<Button>(
                "GameCanvas/Background/Footer/EndWeekButton");
            var resetButton = RequireSceneComponent<Button>(
                "GameCanvas/Background/Header/ResetButton");
            _weekTransitionOverlay = RequireSceneComponent<CanvasGroup>(
                "GameCanvas/WeekTransitionOverlay");
            _weekTransitionText = RequireSceneComponent<TextMeshProUGUI>(
                "GameCanvas/WeekTransitionOverlay/Text");
            _informationWorkspaceGroup =
                _informationWorkspace.GetComponent<CanvasGroup>();
            _assignmentWorkspaceGroup =
                _assignmentWorkspace.GetComponent<CanvasGroup>();

            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(() =>
            {
                Punch(resetButton.transform);
                RestartGame();
            });
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
            LastStatus = "信息中心已打开：邮件负责正式解锁，期刊负责交叉判断。";
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
            _weekTransitionText.text = "本周结算中…";
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
                ? $"赛季结束 · {_seasonCalendar.DateAfterFinalWeek:yyyy年M月d日}"
                : $"{FormatCurrentDate()} · {SeasonPhaseName(CurrentSeasonPhase)}";
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
            LastStatus = _currentDemand == null
                ? "当前没有已解锁且仍在有效期内的招聘；你仍可结束本周推进日期。"
                : "分配工作台只显示已读、未过期且从未提交给俱乐部的球员。";
            ApplyWorkspaceVisibility();
            RefreshUi();
        }

        private void SetInformationMode(InformationMode mode)
        {
            _informationMode = mode;
            if (mode == InformationMode.Mail)
            {
                LastStatus = "收件箱：打开邮件后，关联的招聘需求或球员简历才会进入工作台。";
            }
            else
            {
                EnsureSelectedIssue();
                LastStatus = "订阅期刊：翻页比较报道，但期刊不会替代正式简历邮件。";
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
                ? unlocksContent
                    ? $"已阅读《{Resolve(mail.subject)}》，关联档案已进入分配工作台。"
                    : $"已阅读《{Resolve(mail.subject)}》。"
                : $"重新打开《{Resolve(mail.subject)}》。";
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
            LastStatus = $"正在阅读《{Resolve(GetSelectedIssue().issueTitle)}》。";
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
                : _slotAssignments.Select(pair => new Assignment(
                    _currentDemand.Id,
                    pair.Key,
                    pair.Value,
                    _state.CurrentWeek)).ToList();

            if (_currentDemand != null)
            {
                var missingRequiredSlots = _currentDemand.Slots
                    .Where(slot => slot.IsRequired && !_slotAssignments.ContainsKey(slot.Id))
                    .ToList();
                if (missingRequiredSlots.Count > 0 && !_pendingEmptyConfirmation)
                {
                    _pendingEmptyConfirmation = true;
                    LastStatus =
                        $"还有 {missingRequiredSlots.Count} 个必需槽位为空。再次点击“结束本周”确认不完整提交或暂不推荐。";
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

                    AddLog(
                        $"即时收益：私人请托 +{FormatMoney(offerMail.privateOfferAmount)}；未披露推荐风险已记录。");
                }

                var arrangedNames = assignments
                    .Select(assignment => GetPlayerDisplayName(assignment.PlayerId))
                    .ToArray();
                AddLog(
                    $"{FormatCurrentDate()}：向“{_currentDemand.Title}”提交 {string.Join("、", arrangedNames)}。球员已从可用名单移除。");
                foreach (var outcome in commit.ScheduledOutcomes)
                {
                    AddLog(
                        $"{GetPlayerDisplayName(outcome.Assignment.PlayerId)} 的反馈预计在 {FormatWeekDate(outcome.OutcomeWeek)} 到达。");
                }
            }
            else if (_currentDemand != null)
            {
                AddLog(
                    $"{FormatCurrentDate()}：本周未向“{_currentDemand.Title}”推荐球员，需求仍会保留到截止日期。");
            }
            else
            {
                AddLog($"{FormatCurrentDate()}：本周没有有效招聘需求，事务所继续跟进赛事和市场消息。");
            }

            var advance = _engine.AdvanceOneWeek(_state);
            foreach (var outcome in advance.DeliveredOutcomes)
            {
                AddLog(DescribeOutcome(outcome));
                RegisterResultMail(outcome);
            }

            foreach (var payment in advance.PaidPayments)
            {
                AddLog($"已到账：{FormatMoney(payment.Amount)}（{payment.DemandId}）。");
            }

            if (_state.CurrentWeek == 4 && _carloFavorAccepted && !_carloFavorConsequenceApplied)
            {
                _state.ApplyReputationChange(-3);
                _carloFavorConsequenceApplied = true;
                AddLog("延迟后果：雨城竞技追查卡洛的推荐依据，声望 -3。恩佐叔叔提供的午餐发票未被视为球探报告。");
            }

            _slotAssignments.Clear();
            _selectedPlayerId = null;
            _selectedMailId = null;
            _pendingEmptyConfirmation = false;
            _activeWorkspace = WorkspaceMode.Information;
            _informationMode = InformationMode.Mail;

            if (IsGameComplete)
            {
                LastStatus =
                    $"一年赛季结束。现金 {FormatMoney(_state.Cash)}，应收 {FormatMoney(_state.OutstandingReceivables)}，声望 {_state.Reputation}。";
                AddLog("2027年7月5日：年度结算完成。所有董事会都已宣布下赛季会吸取教训。");
            }
            else
            {
                var nextPhase = CurrentSeasonPhase;
                if (nextPhase != startingPhase)
                {
                    AddLog($"{FormatCurrentDate()}：赛季进入“{SeasonPhaseName(nextPhase)}”阶段。");
                }

                LastStatus =
                    $"{FormatCurrentDate()}，{SeasonPhaseName(nextPhase)}。请查看新邮件和仍在有效期内的招聘。";
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
                ? $"赛季结束 · {_seasonCalendar.DateAfterFinalWeek:yyyy年M月d日}"
                : $"{FormatCurrentDate()} · {SeasonPhaseName(CurrentSeasonPhase)} · {_state.CurrentWeek}/{FinalPlayableWeek}周";
            _economyText.text =
                $"现金 {FormatMoney(_state.Cash)}    应收 {FormatMoney(_state.OutstandingReceivables)}    声望 {_state.Reputation}";
            _statusText.text = LastStatus;
            _eventLogText.text = string.Join(
                "\n",
                _eventLog.TakeLast(4).Select(entry => "• " + entry));
            _endWeekButton.interactable = !IsGameComplete;

            RefreshDemandPanel();
            RefreshSlots();
            RefreshPlayerCards();
            RefreshMailBrowser();
            RefreshMagazineBrowser();
            ApplyWorkspaceVisibility();
        }

        private void UpdateCurrentDemand()
        {
            _currentDemand = _weeklyDemands
                .Where(demand =>
                    _unlockedDemandIds.Contains(demand.Id) &&
                    demand.OpenedWeek <= CurrentWeek &&
                    demand.DeadlineWeek >= CurrentWeek &&
                    !_state.CommittedAssignments.Any(assignment =>
                        assignment.DemandId == demand.Id))
                .OrderBy(demand => demand.DeadlineWeek)
                .ThenBy(demand => demand.OpenedWeek)
                .FirstOrDefault();
        }

        private void RefreshDemandPanel()
        {
            if (IsGameComplete)
            {
                _demandTitleText.text = "年度工作总结";
                _demandBodyText.text =
                    $"现金：{FormatMoney(_state.Cash)}\n应收：{FormatMoney(_state.OutstandingReceivables)}\n声望：{_state.Reputation}";
                _selectionText.text = "本赛季已经结束。";
                return;
            }

            if (_currentDemand == null)
            {
                _demandTitleText.text = "当前没有有效招聘";
                _demandBodyText.text =
                    "可能原因：招聘邮件尚未阅读、需求已经提交，或截止日期已过。你仍可结束本周推进赛程。";
                _selectionText.text = "查看收件箱中的新招聘或过期通知。";
                return;
            }

            var entry = _demandContentById[_currentDemand.Id];
            _demandTitleText.text =
                $"{Resolve(entry.clubDisplayName)} · {_currentDemand.Title}";
            _demandBodyText.text =
                $"{Resolve(entry.clubStanding)} · {Resolve(entry.clubBestAchievement)}\n" +
                $"{Resolve(entry.description)}\n" +
                $"有效期 {entry.activeWeeks} 周 · 截止 {FormatWeekDate(entry.DeadlineWeek)} · " +
                $"委托价 {FormatMoney(_currentDemand.BaseReward)}\n{Resolve(entry.paymentTerms)}";
            _selectionText.text = string.IsNullOrEmpty(_selectedPlayerId)
                ? "未选择球员。已填槽位可在未选中球员时点击清空。"
                : $"已选择：{GetPlayerDisplayName(_selectedPlayerId)}";
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
                    $"{PositionName(slot.RequiredPosition)} · {(slot.IsRequired ? "必需" : "可选")}";
                var assignment = panel.Find("Assignment").GetComponent<TextMeshProUGUI>();
                assignment.text = isFilled
                    ? GetPlayerDisplayName(playerId) + " · 未选球员时点击可移除"
                    : "把球员拖到这里 / 选中球员后点击";
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
                    contentCatalog.PlayerPortraitAtlas, playerContent.portraitIndex, 4, 4);
                card.Find("Name").GetComponent<TextMeshProUGUI>().text = player.DisplayName;
                card.Find("Position").GetComponent<TextMeshProUGUI>().text =
                    string.Join(" / ", player.ClaimedPositions.Select(PositionName));
                card.Find("Claim").GetComponent<TextMeshProUGUI>().text =
                    player.Claims.Count == 0 ? player.Biography : player.Claims[0].Text;
                var isSelected = player.PlayerId == _selectedPlayerId;
                card.GetComponent<Image>().color = isSelected
                    ? new Color(0.16f, 0.36f, 0.25f, 1f)
                    : PanelLightColor;
            }

            var label = RequireSceneComponent<TextMeshProUGUI>(
                "GameCanvas/Background/AssignmentWorkspace/PlayersPanel/PlayersLabel");
            label.text =
                $"当前可用 {availablePlayers.Count} 人 · 已读 {_unlockedPlayerIds.Count} / {_players.Count} · 已安排或过期球员不会再次出现";
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
                    isRead ? "已读" : "未读";
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
                _informationCounterText.text =
                    $"{FormatCurrentDate()} · 已到达 {_visibleMails.Count} 封 · 已读 {_readMailIds.Count}";
            }

            var selected = _visibleMails.FirstOrDefault(mail => mail.id == _selectedMailId);
            if (selected == null)
            {
                ShowEmptyMailPane();
                return;
            }

            _mailSenderText.text = "发件人：" + Resolve(selected.sender);
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
            _mailSenderText.text = "收件箱";
            _mailSubjectText.text = "请选择并打开一封邮件";
            _mailMetaText.text = "只有实际阅读后，关联内容才会进入分配工作台";
            _mailBodyText.text = "招聘邮件会解锁需求；简历或私人请托邮件会解锁对应球员。期刊报道只作为判断证据。";
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
                "固定信息 · 招聘需求";
            _mailDemandBlock.Find("ClubProfile").GetComponent<TextMeshProUGUI>().text =
                $"{Resolve(demand.clubDisplayName)} · {Resolve(demand.clubStanding)}\n" +
                $"{Resolve(demand.clubBestAchievement)}";
            _mailDemandBlock.Find("Slots").GetComponent<TextMeshProUGUI>().text =
                "所需位置：" + string.Join("、", demand.slots.Select(slot =>
                    PositionName(slot.requiredPosition) + (slot.isRequired ? "（必需）" : "（可选）")));
            var remainingWeeks = Math.Max(0, demand.DeadlineWeek - CurrentWeek + 1);
            _mailDemandBlock.Find("Deadline").GetComponent<TextMeshProUGUI>().text =
                $"有效期 {demand.activeWeeks} 周 · 截止 {FormatWeekDate(demand.DeadlineWeek)} · 剩余 {remainingWeeks} 周";
            _mailDemandBlock.Find("Price").GetComponent<TextMeshProUGUI>().text =
                $"委托价：{FormatMoney(demand.baseReward)}";
            _mailDemandBlock.Find("Payment").GetComponent<TextMeshProUGUI>().text =
                "付款：" + Resolve(demand.paymentTerms);
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
                contentCatalog.PlayerPortraitAtlas, player.portraitIndex, 4, 4);
            _mailResumeBlock.Find("Title").GetComponent<TextMeshProUGUI>().text =
                "固定信息 · 球员简历";
            _mailResumeBlock.Find("Player").GetComponent<TextMeshProUGUI>().text =
                Resolve(player.displayName);
            _mailResumeBlock.Find("Position").GetComponent<TextMeshProUGUI>().text =
                "公开位置：" + PositionName(player.publicPosition);
            _mailResumeBlock.Find("Biography").GetComponent<TextMeshProUGUI>().text =
                Resolve(player.biography);
            _mailResumeBlock.Find("Salary").GetComponent<TextMeshProUGUI>().text =
                $"薪资期望：€{player.salaryMinWeekly}–€{player.salaryMaxWeekly} / 周";
            _mailResumeBlock.Find("Career").GetComponent<TextMeshProUGUI>().text =
                "经历：" + Resolve(player.careerHistory);
            _mailResumeBlock.Find("Claim").GetComponent<TextMeshProUGUI>().text =
                "自述：" + Resolve(player.publicClaim);
            _mailResumeBlock.Find("Evidence").GetComponent<TextMeshProUGUI>().text =
                "旁证：" + Resolve(player.publicEvidence);
            _mailResumeBlock.Find("Source").GetComponent<TextMeshProUGUI>().text =
                "可信度：" + ReliabilityName(player.evidenceReliability);
            var remainingWeeks = Math.Max(0, player.LastAvailableWeek - CurrentWeek + 1);
            _mailResumeBlock.Find("Availability").GetComponent<TextMeshProUGUI>().text =
                $"可安排至 {FormatWeekDate(player.LastAvailableWeek)} · 剩余 {remainingWeeks} 周";
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
                "固定信息 · 私人请托";
            _mailPrivateOfferBlock.Find("Offer").GetComponent<TextMeshProUGUI>().text =
                "即时酬谢：" + FormatMoney(mail.privateOfferAmount);
            _mailPrivateOfferBlock.Find("Terms").GetComponent<TextMeshProUGUI>().text =
                "要求：" + Resolve(mail.privateOfferTerms);
            _mailPrivateOfferBlock.Find("TargetClub").GetComponent<TextMeshProUGUI>().text =
                Resolve(mail.privateTargetClubRequirement);
            _mailPrivateOfferBlock.Find("Risk").GetComponent<TextMeshProUGUI>().text =
                "延迟风险：" + Resolve(mail.privateRiskNote);
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
                    issue.issueNumber + " · " + Resolve(issue.issueTitle);
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
                _informationCounterText.text =
                    $"已订阅 {_visibleIssues.Count} 期 · 期刊不会直接解锁球员";
            }

            var page = GetSelectedPage();
            var issueSelected = GetSelectedIssue();
            if (page == null || issueSelected == null)
            {
                _coverLayout.gameObject.SetActive(false);
                _featureLayout.gameObject.SetActive(false);
                _scoutReportLayout.gameObject.SetActive(false);
                _magazinePageIndicator.text = "暂无可读期刊";
                _previousPageButton.interactable = false;
                _nextPageButton.interactable = false;
                return;
            }

            _coverLayout.gameObject.SetActive(page.layout == MagazinePageLayout.Cover);
            _featureLayout.gameObject.SetActive(page.layout == MagazinePageLayout.Feature);
            _scoutReportLayout.gameObject.SetActive(page.layout == MagazinePageLayout.ScoutReport);
            _magazinePageIndicator.text =
                $"{Resolve(issueSelected.publicationName)} · {issueSelected.issueNumber}    第 {_magazinePageIndex + 1} / {issueSelected.pages.Count} 页";
            _previousPageButton.interactable = _magazinePageIndex > 0;
            _nextPageButton.interactable = _magazinePageIndex < issueSelected.pages.Count - 1;

            if (page.layout == MagazinePageLayout.Cover)
            {
                ApplyAtlasImage(_coverImage, contentCatalog.MagazineCoverAtlas,
                    issueSelected.coverIndex, 3, 2);
                SetText(_coverLayout, "Publication", Resolve(issueSelected.publicationName));
                SetText(_coverLayout, "IssueNumber", issueSelected.issueNumber);
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
            }
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

        private static string AvatarInitial(string sender)
        {
            if (string.IsNullOrWhiteSpace(sender))
            {
                return "邮";
            }

            var trimmed = sender.TrimStart('《', '【', '[', '(');
            return string.IsNullOrEmpty(trimmed) ? "邮" : trimmed[0].ToString();
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

        private static string BuildMailSummary(string body)
        {
            var normalized = string.IsNullOrWhiteSpace(body)
                ? "（邮件没有正文）"
                : string.Join(" ", body.Split(
                    new[] { ' ', '\r', '\n', '\t' },
                    StringSplitOptions.RemoveEmptyEntries));
            const int maximumCharacters = 34;
            if (normalized.Length > maximumCharacters)
            {
                normalized = normalized.Substring(0, maximumCharacters);
            }

            return normalized.TrimEnd('。', '！', '？', '.', '…') + "……";
        }

        private string Resolve(LocalizedText text)
        {
            return text == null
                ? string.Empty
                : text.Resolve(contentCatalog.DevelopmentLanguage);
        }

        private static string PositionName(PlayerPosition position)
        {
            switch (position)
            {
                case PlayerPosition.Goalkeeper:
                    return "门将";
                case PlayerPosition.Defender:
                    return "中卫";
                case PlayerPosition.WingBack:
                    return "翼卫";
                case PlayerPosition.Midfielder:
                    return "中场";
                case PlayerPosition.Winger:
                    return "边锋";
                default:
                    return "前锋";
            }
        }

        private static string MailKindName(MailContentKind kind)
        {
            switch (kind)
            {
                case MailContentKind.ClubRequest:
                    return "球队招聘";
                case MailContentKind.PlayerResume:
                    return "球员简历";
                case MailContentKind.PrivateRequest:
                    return "私人请托";
                case MailContentKind.ClubFeedback:
                    return "俱乐部回函";
                default:
                    return "普通邮件";
            }
        }

        private static string ReliabilityName(EvidenceReliability reliability)
        {
            switch (reliability)
            {
                case EvidenceReliability.High:
                    return "较高";
                case EvidenceReliability.Medium:
                    return "中等";
                case EvidenceReliability.Low:
                    return "较低";
                default:
                    return "未经核实";
            }
        }

        private string DescribeOutcome(PlacementOutcome outcome)
        {
            var playerName = GetPlayerDisplayName(outcome.Assignment.PlayerId);
            switch (outcome.ResultKind)
            {
                case PlacementResultKind.Accepted:
                    return $"反馈：{playerName} 打动了俱乐部并获得正式机会。";
                case PlacementResultKind.TrialExtended:
                    return $"反馈：{playerName} 获得继续考察，茶水间仍然意见不一。";
                default:
                    return $"反馈：{playerName} 的试训提前结束，俱乐部礼貌地换了话题。";
            }
        }

        private static string TranslateSubmissionErrors(
            IReadOnlyList<SubmissionError> errors)
        {
            if (errors.Any(error => error.Code == SubmissionErrorCode.SubmissionHasNoAssignments))
            {
                return "至少需要向一个招聘槽位安排一名球员。";
            }

            if (errors.Any(error => error.Code == SubmissionErrorCode.DuplicatePlayerInWeek))
            {
                return "同一名球员本周只能安排一次。";
            }

            if (errors.Any(error => error.Code == SubmissionErrorCode.PlayerAlreadyCommitted))
            {
                return "这名球员已经提交给其他俱乐部，不能再次安排。";
            }

            return "本周提交未通过，请检查招聘槽位和已读档案。";
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
                GetPlayerDisplayName(outcome.Assignment.PlayerId));
            _resultMails.Add(mail);
            AddLog($"收到 {Resolve(mail.sender)} 的正式回函，详情见收件箱。");
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
                    GetPlayerDisplayName(outcome.Assignment.PlayerId)));
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

        private static string FormatWeekDate(int week)
        {
            var date = SeasonStartDate.AddDays((Math.Max(1, week) - 1) * 7);
            return date.ToString("yyyy年M月d日");
        }

        private static string SeasonPhaseName(SeasonPhase phase)
        {
            switch (phase)
            {
                case SeasonPhase.Preseason:
                    return "季前训练";
                case SeasonPhase.SummerWindow:
                    return "夏季转会窗口";
                case SeasonPhase.LeagueOpening:
                    return "联赛开幕";
                case SeasonPhase.GroupStage:
                    return "洲际小组赛";
                case SeasonPhase.WinterSchedule:
                    return "冬季密集赛程";
                case SeasonPhase.WinterWindow:
                    return "冬季转会窗口";
                case SeasonPhase.KnockoutStage:
                    return "洲际淘汰赛";
                case SeasonPhase.RunIn:
                    return "争冠与保级冲刺";
                case SeasonPhase.Finals:
                    return "决赛阶段";
                default:
                    return "赛季总结";
            }
        }
    }
}
