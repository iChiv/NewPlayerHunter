using System;
using System.Collections.Generic;
using NewPlayerHunter.Domain;
using UnityEngine;

namespace NewPlayerHunter.Gameplay
{
    public enum GameLanguage
    {
        ChineseSimplified,
        English
    }

    public enum MailContentKind
    {
        ClubRequest,
        PlayerResume,
        PrivateRequest,
        General,
        ClubFeedback
    }

    public enum MagazinePageLayout
    {
        Cover,
        Feature,
        ScoutReport
    }

    [Serializable]
    public sealed class LocalizedText
    {
        [TextArea] public string chineseSimplified;
        [TextArea] public string english;

        public string Resolve(GameLanguage language)
        {
            if (language == GameLanguage.English &&
                !string.IsNullOrWhiteSpace(english))
            {
                return english;
            }

            return chineseSimplified ?? string.Empty;
        }
    }

    [Serializable]
    public sealed class PlayerContentEntry
    {
        public string id;
        [Min(0)] public int portraitIndex;
        [Min(1)] public int availableFromWeek = 1;
        [Min(1)] public int availabilityWeeks = 4;
        public LocalizedText displayName;
        public LocalizedText biography;
        public PlayerPosition publicPosition;
        [Range(0, 100)] public int hiddenAbility;
        [Range(0, 100)] public int hiddenFitness;
        [Range(0, 100)] public int hiddenProfessionalism;
        [Min(0)] public int salaryMinWeekly;
        [Min(0)] public int salaryMaxWeekly;
        public LocalizedText careerHistory;
        public LocalizedText publicClaim;
        public LocalizedText publicEvidence;
        public EvidenceReliability evidenceReliability;
        public LocalizedText expiryMailSubject;
        public LocalizedText expiryMailBody;

        public int LastAvailableWeek =>
            availableFromWeek + availabilityWeeks - 1;
    }

    [Serializable]
    public sealed class DemandSlotContentEntry
    {
        public string id;
        public PlayerPosition requiredPosition;
        [Range(0, 100)] public int minimumAbility;
        [Range(0, 100)] public int preferredFitness;
        [Range(0, 100)] public int preferredProfessionalism;
        public bool isRequired = true;
    }

    [Serializable]
    public sealed class DemandContentEntry
    {
        public string id;
        public string clubId;
        public LocalizedText clubDisplayName;
        public LocalizedText clubStanding;
        public LocalizedText clubBestAchievement;
        public LocalizedText clubProfile;
        public LocalizedText title;
        public LocalizedText description;
        [Min(1)] public int openedWeek;
        [Min(1)] public int activeWeeks = 1;
        [Min(0)] public int baseReward;
        public LocalizedText paymentTerms;
        public LocalizedText expiryMailSubject;
        public LocalizedText expiryMailBody;
        public List<DemandSlotContentEntry> slots = new List<DemandSlotContentEntry>();

        public int DeadlineWeek => openedWeek + activeWeeks - 1;
    }

    [Serializable]
    public sealed class MailContentEntry
    {
        public string id;
        public MailContentKind kind;
        [Min(1)] public int publishedWeek;
        public LocalizedText sender;
        public LocalizedText subject;
        public LocalizedText receivedTime;
        public LocalizedText preview;
        public LocalizedText body;
        public LocalizedText sourceNote;
        public string relatedPlayerId;
        public string relatedDemandId;
        [Min(0)] public int privateOfferAmount;
        public LocalizedText privateOfferTerms;
        public string privateRequiredClubId;
        public LocalizedText privateTargetClubRequirement;
        public LocalizedText privateRiskNote;
        public string expiredPlayerId;
        public string expiredDemandId;
    }

    [Serializable]
    public sealed class MagazinePageContent
    {
        public MagazinePageLayout layout;
        public LocalizedText kicker;
        public LocalizedText headline;
        public LocalizedText deck;
        public LocalizedText bodyLeft;
        public LocalizedText bodyRight;
        public LocalizedText pullQuote;
        public LocalizedText sidebarTitle;
        public LocalizedText sidebarBody;
        public string relatedPlayerId;
    }

    [Serializable]
    public sealed class MagazineIssueContent
    {
        public string id;
        [Min(0)] public int coverIndex;
        [Min(1)] public int publishedWeek;
        public LocalizedText publicationName;
        public LocalizedText issueTitle;
        public string issueNumber;
        public List<MagazinePageContent> pages = new List<MagazinePageContent>();
    }

    [CreateAssetMenu(
        fileName = "GameContentCatalog",
        menuName = "New Player Hunter/Game Content Catalog")]
    public sealed class GameContentCatalog : ScriptableObject
    {
        [SerializeField] private GameLanguage developmentLanguage =
            GameLanguage.ChineseSimplified;
        [SerializeField] private List<PlayerContentEntry> players =
            new List<PlayerContentEntry>();
        [SerializeField] private List<DemandContentEntry> demands =
            new List<DemandContentEntry>();
        [SerializeField] private List<MailContentEntry> mails =
            new List<MailContentEntry>();
        [SerializeField] private List<MagazineIssueContent> magazineIssues =
            new List<MagazineIssueContent>();
        [SerializeField] private Texture2D playerPortraitAtlas;
        [SerializeField] private Texture2D magazineCoverAtlas;

        public GameLanguage DevelopmentLanguage => developmentLanguage;

        public IReadOnlyList<PlayerContentEntry> Players => players;

        public IReadOnlyList<DemandContentEntry> Demands => demands;

        public IReadOnlyList<MailContentEntry> Mails => mails;

        public IReadOnlyList<MagazineIssueContent> MagazineIssues => magazineIssues;

        public Texture2D PlayerPortraitAtlas => playerPortraitAtlas;

        public Texture2D MagazineCoverAtlas => magazineCoverAtlas;

        public bool IsConfigured =>
            players.Count > 0 &&
            demands.Count > 0 &&
            mails.Count > 0 &&
            magazineIssues.Count > 0;

        public void PopulateM1Defaults()
        {
            developmentLanguage = GameLanguage.ChineseSimplified;
            players = SixWeekContentFactory.BuildPlayers();
            players.AddRange(LateSeasonContentFactory.BuildPlayers());
            demands = SixWeekContentFactory.BuildDemands();
            demands.AddRange(LateSeasonContentFactory.BuildDemands());
            mails = SixWeekContentFactory.BuildMails(players, demands);
            mails.AddRange(LateSeasonContentFactory.BuildMails());
            magazineIssues = SixWeekContentFactory.BuildMagazineIssues();
            magazineIssues.AddRange(LateSeasonContentFactory.BuildMagazineIssues());
            EnsureMailTimestamps();
        }

        public void ConfigureArt(Texture2D portraits, Texture2D magazineCovers)
        {
            playerPortraitAtlas = portraits;
            magazineCoverAtlas = magazineCovers;
        }

        public bool EnsureMailTimestamps()
        {
            var changed = false;
            var ordinalByWeek = new Dictionary<int, int>();
            foreach (var mail in mails)
            {
                ordinalByWeek.TryGetValue(mail.publishedWeek, out var ordinal);
                ordinalByWeek[mail.publishedWeek] = ordinal + 1;
                if (mail.receivedTime != null &&
                    !string.IsNullOrWhiteSpace(mail.receivedTime.chineseSimplified))
                {
                    continue;
                }

                var hour = 8 + ordinal;
                var minute = mail.kind == MailContentKind.ClubRequest
                    ? 20
                    : mail.kind == MailContentKind.PrivateRequest
                        ? 40
                        : 5;
                mail.receivedTime = Zh($"周一 {hour:00}:{minute:00}");
                changed = true;
            }

            return changed;
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

