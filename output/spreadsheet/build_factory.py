"""Merge batch extraction + design + prose JSON into LateSeasonContentFactory.cs.

Usage: python output/spreadsheet/build_factory.py
Reads:  content_batch_01.json, content_design.json, prose_players.json,
        prose_demands.json, prose_magazines_a.json, prose_magazines_b.json
Writes: Assets/Game/Scripts/Gameplay/LateSeasonContentFactory.cs
"""
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
PROJ = ROOT.parent.parent
OUT_CS = PROJ / "Assets/Game/Scripts/Gameplay/LateSeasonContentFactory.cs"

POSITIONS = {"Goalkeeper", "Defender", "WingBack", "Midfielder", "Winger", "Forward"}
RELIABILITY = {"Unverified", "Low", "Medium", "High"}

ILLUSTRATION_INDEX = {
    "transfer_window": 0, "contract_money": 1, "injury": 2, "tactics_board": 3,
    "stadium_night": 4, "pub": 5, "training": 6, "scouting": 7, "media": 8,
    "dressing_room": 9, "boots_ball": 10, "referee": 11, "trophy": 12,
    "rain_match": 13, "gold_desert": 14, "youth": 15, "veteran": 16,
    "goalkeeper": 17, "deadline_fax": 18, "fans": 19, "winter_window": 20,
    "medical": 21, "agent_phone": 22, "data_chart": 23,
}


def load(name):
    return json.loads((ROOT / name).read_text(encoding="utf-8"))


def cs(value):
    s = "" if value is None else str(value)
    s = s.replace("\\", "\\\\").replace('"', '\\"').replace("\r\n", "\n").replace("\r", "\n")
    return '"' + s.replace("\n", "\\n") + '"'


def id_suffix(stable_id, parts=2):
    segs = stable_id.split(".")
    return ".".join(segs[-parts:])


def en_of(container, base, warnings, owner):
    """Read the '<base>En' sibling key; record a warning when absent/empty."""
    key = base + "En"
    raw = container.get(key)
    value = raw.strip() if isinstance(raw, str) else ""
    if not value:
        warnings.append(f"{owner}.{key}")
    return value


def pair(container, base, warnings, owner):
    """Format a (zh, en) string-literal pair for a generated call site."""
    zh = container.get(base) or ""
    return f"{cs(zh)}, {cs(en_of(container, base, warnings, owner))}"


def issue_number(raw):
    """Extract the numeric part, e.g. '第 07 期' -> '07'; keep raw when no digits."""
    m = re.search(r"\d+", raw or "")
    return m.group(0) if m else (raw or "")


def main():
    batch = load("content_batch_01.json")
    design = load("content_design.json")
    prose_players = load("prose_players.json")
    prose_demands = load("prose_demands.json")
    prose_mags = {**load("prose_magazines_a.json"), **load("prose_magazines_b.json")}

    players_src = {p["_row"]: p for p in batch["players"]}
    demands_src = {d["_row"]: d for d in batch["demands"]}

    player_calls, demand_calls, mail_calls, issue_calls = [], [], [], []
    all_mail_ids = set()
    problems = []
    warnings = []

    for dp in design["players"]:
        pid = dp["id"]
        src = players_src[dp["source_row"]]
        prose = prose_players.get(pid)
        if prose is None:
            problems.append(f"missing prose for {pid}")
            continue
        fix = prose.get("fix") or {}
        evidence = (fix.get("public_evidence_zh") or src.get("public_evidence_zh") or "").strip()
        reliability = (fix.get("evidence_reliability") or src.get("evidence_reliability") or "").strip()
        biography = (fix.get("biography_zh") or src.get("biography_zh") or "").strip()
        name = src["display_name_zh"].strip()
        name_en = en_of(prose, "name", warnings, pid)
        biography_en = en_of(prose, "biography", warnings, pid)
        claim_en = en_of(prose, "claim", warnings, pid)
        evidence_en = en_of(prose, "evidence", warnings, pid)
        career_en = en_of(prose, "careerHistory", warnings, pid)
        expiry_subject_en = en_of(prose, "expirySubject", warnings, pid)
        expiry_body_en = en_of(prose, "expiryBody", warnings, pid)
        pos = src["public_position"].strip()
        if pos not in POSITIONS or reliability not in RELIABILITY:
            problems.append(f"{pid}: bad enum")
            continue
        rm = prose["resumeMail"]
        rm_owner = f"{pid}.resumeMail"
        player_calls.append(
            f'                Player({cs(pid)}, {dp["portraitIndex"]}, {dp["availableFromWeek"]}, {dp["availabilityWeeks"]},\n'
            f'                    {cs(name)}, {cs(name_en)}, PlayerPosition.{pos}, {int(src["hidden_ability"])}, {int(src["hidden_fitness"])}, {int(src["hidden_professionalism"])},\n'
            f'                    {dp["salaryMin"]}, {dp["salaryMax"]}, {cs(prose["careerHistory"])}, {cs(career_en)},\n'
            f'                    {cs(biography)}, {cs(biography_en)}, {cs(src["public_claim_zh"].strip())}, {cs(claim_en)},\n'
            f'                    {cs(evidence)}, {cs(evidence_en)}, EvidenceReliability.{reliability},\n'
            f'                    {cs(prose["expirySubject"])}, {cs(expiry_subject_en)}, {cs(prose["expiryBody"])}, {cs(expiry_body_en)})')
        mail_id = f'mail.w{dp["availableFromWeek"]}.{id_suffix(pid)}'
        if mail_id in all_mail_ids:
            problems.append(f"duplicate mail id {mail_id}")
        all_mail_ids.add(mail_id)
        mail_calls.append(
            f'                Mail({cs(mail_id)}, MailContentKind.PlayerResume, {dp["availableFromWeek"]},\n'
            f'                    {pair(rm, "sender", warnings, rm_owner)}, {pair(rm, "subject", warnings, rm_owner)},\n'
            f'                    {pair(rm, "preview", warnings, rm_owner)}, {pair(rm, "body", warnings, rm_owner)},\n'
            f'                    {pair(rm, "sourceNote", warnings, rm_owner)}, playerId: {cs(pid)})')

    for dd in design["demands"]:
        did = dd["id"]
        src = demands_src[dd["source_row"]]
        prose = prose_demands.get(did)
        if prose is None:
            problems.append(f"missing prose for {did}")
            continue
        slots = ",\n".join(
            f'                    Slot({cs("slot." + id_suffix(did, 2) + "." + s["position"].lower())}, '
            f'PlayerPosition.{s["position"]}, {s["ability"]}, {s["fitness"]}, {s["professionalism"]})'
            for s in dd["slots"])
        demand_calls.append(
            f'                Demand({cs(did)}, {cs(dd["clubId"])},\n'
            f'                    {pair(prose, "clubName", warnings, did)}, {pair(prose, "clubStanding", warnings, did)},\n'
            f'                    {pair(prose, "clubBest", warnings, did)}, {pair(prose, "clubProfile", warnings, did)},\n'
            f'                    {cs(src["title_zh"].strip())}, {cs(en_of(prose, "title", warnings, did))},\n'
            f'                    {cs(src["description_zh"].strip())}, {cs(en_of(prose, "description", warnings, did))},\n'
            f'                    {dd["openedWeek"]}, {dd["activeWeeks"]}, {dd["baseReward"]},\n'
            f'                    {cs(src["payment_terms_zh"].strip())}, {cs(en_of(prose, "paymentTerms", warnings, did))},\n'
            f'                    {pair(prose, "expirySubject", warnings, did)}, {pair(prose, "expiryBody", warnings, did)},\n'
            f'{slots})')
        mail_id = f'mail.w{dd["openedWeek"]}.{id_suffix(did, 1)}'
        if mail_id in all_mail_ids:
            problems.append(f"duplicate mail id {mail_id}")
        all_mail_ids.add(mail_id)
        cm = prose["clubRequestMail"]
        cm_owner = f"{did}.clubRequestMail"
        mail_calls.append(
            f'                Mail({cs(mail_id)}, MailContentKind.ClubRequest, {dd["openedWeek"]},\n'
            f'                    {pair(cm, "sender", warnings, cm_owner)}, {pair(cm, "subject", warnings, cm_owner)},\n'
            f'                    {pair(cm, "preview", warnings, cm_owner)}, {pair(cm, "body", warnings, cm_owner)},\n'
            f'                    {pair(cm, "sourceNote", warnings, cm_owner)}, demandId: {cs(did)})')

    for di in design["issues"]:
        iid = di["id"]
        prose = prose_mags.get(iid)
        if prose is None:
            problems.append(f"missing prose for {iid}")
            continue
        pages = []
        for pi, pg in enumerate(prose["pages"]):
            pg_owner = f"{iid}.pages[{pi}]"
            illustration = pg.get("illustration") or ""
            illustration2 = pg.get("illustration2") or ""
            for key in (illustration, illustration2):
                if key and key not in ILLUSTRATION_INDEX:
                    problems.append(f"unknown illustration key {key} in {iid}")
            ill_index = ILLUSTRATION_INDEX.get(illustration, -1)
            ill_index2 = ILLUSTRATION_INDEX.get(illustration2, -1)
            pages.append(
                f'                    Page(MagazinePageLayout.{pg["layout"]},\n'
                f'                        {pair(pg, "kicker", warnings, pg_owner)}, {pair(pg, "headline", warnings, pg_owner)}, {pair(pg, "deck", warnings, pg_owner)},\n'
                f'                        {pair(pg, "bodyLeft", warnings, pg_owner)}, {pair(pg, "bodyRight", warnings, pg_owner)},\n'
                f'                        {pair(pg, "pullQuote", warnings, pg_owner)}, {pair(pg, "sidebarTitle", warnings, pg_owner)}, {pair(pg, "sidebarBody", warnings, pg_owner)},\n'
                f'                        {cs(pg.get("relatedPlayerId") or "")}, {ill_index}, {ill_index2})')
        publication_en = en_of(di, "publication", warnings, iid)
        issue_calls.append(
            f'                Issue({cs(iid)}, {di["coverIndex"]}, {di["week"]},\n'
            f'                    {cs(di["publication"])}, {cs(publication_en)},\n'
            f'                    {cs(prose["issueTitle"])}, {cs(en_of(prose, "issueTitle", warnings, iid))}, {cs(issue_number(di["issueNumber"]))},\n'
            + ",\n".join(pages) + ")")

    if problems:
        print("PROBLEMS:")
        for p in problems:
            print(" -", p)
        sys.exit(1)

    template = f"""// <auto-generated> 由 output/spreadsheet/build_factory.py 从内容批次 01 生成；请勿手改，改数据后重新生成。 </auto-generated>
using System.Collections.Generic;
using NewPlayerHunter.Domain;

namespace NewPlayerHunter.Gameplay
{{
    internal static class LateSeasonContentFactory
    {{
        public static List<PlayerContentEntry> BuildPlayers()
        {{
            return new List<PlayerContentEntry>
            {{
{",\n".join(player_calls)}
            }};
        }}

        public static List<DemandContentEntry> BuildDemands()
        {{
            return new List<DemandContentEntry>
            {{
{",\n".join(demand_calls)}
            }};
        }}

        public static List<MailContentEntry> BuildMails()
        {{
            return new List<MailContentEntry>
            {{
{",\n".join(mail_calls)}
            }};
        }}

        public static List<MagazineIssueContent> BuildMagazineIssues()
        {{
            return new List<MagazineIssueContent>
            {{
{",\n".join(issue_calls)}
            }};
        }}

        private static PlayerContentEntry Player(
            string id, int portraitIndex, int availableFromWeek, int availabilityWeeks,
            string nameZh, string nameEn, PlayerPosition position, int ability, int fitness, int professionalism,
            int salaryMinWeekly, int salaryMaxWeekly, string careerHistoryZh, string careerHistoryEn,
            string biographyZh, string biographyEn, string claimZh, string claimEn,
            string evidenceZh, string evidenceEn, EvidenceReliability reliability,
            string expirySubjectZh, string expirySubjectEn, string expiryBodyZh, string expiryBodyEn)
        {{
            return new PlayerContentEntry
            {{
                id = id,
                portraitIndex = portraitIndex,
                availableFromWeek = availableFromWeek,
                availabilityWeeks = availabilityWeeks,
                displayName = L(nameZh, nameEn),
                biography = L(biographyZh, biographyEn),
                publicPosition = position,
                hiddenAbility = ability,
                hiddenFitness = fitness,
                hiddenProfessionalism = professionalism,
                salaryMinWeekly = salaryMinWeekly,
                salaryMaxWeekly = salaryMaxWeekly,
                careerHistory = L(careerHistoryZh, careerHistoryEn),
                publicClaim = L(claimZh, claimEn),
                publicEvidence = L(evidenceZh, evidenceEn),
                evidenceReliability = reliability,
                expiryMailSubject = L(expirySubjectZh, expirySubjectEn),
                expiryMailBody = L(expiryBodyZh, expiryBodyEn)
            }};
        }}

        private static DemandContentEntry Demand(
            string id, string clubId,
            string clubNameZh, string clubNameEn, string standingZh, string standingEn,
            string bestAchievementZh, string bestAchievementEn, string clubProfileZh, string clubProfileEn,
            string titleZh, string titleEn, string descriptionZh, string descriptionEn,
            int openedWeek, int activeWeeks, int reward,
            string paymentTermsZh, string paymentTermsEn,
            string expirySubjectZh, string expirySubjectEn, string expiryBodyZh, string expiryBodyEn,
            params DemandSlotContentEntry[] slots)
        {{
            return new DemandContentEntry
            {{
                id = id,
                clubId = clubId,
                clubDisplayName = L(clubNameZh, clubNameEn),
                clubStanding = L(standingZh, standingEn),
                clubBestAchievement = L(bestAchievementZh, bestAchievementEn),
                clubProfile = L(clubProfileZh, clubProfileEn),
                title = L(titleZh, titleEn),
                description = L(descriptionZh, descriptionEn),
                openedWeek = openedWeek,
                activeWeeks = activeWeeks,
                baseReward = reward,
                paymentTerms = L(paymentTermsZh, paymentTermsEn),
                expiryMailSubject = L(expirySubjectZh, expirySubjectEn),
                expiryMailBody = L(expiryBodyZh, expiryBodyEn),
                slots = new List<DemandSlotContentEntry>(slots)
            }};
        }}

        private static DemandSlotContentEntry Slot(
            string id, PlayerPosition position, int ability, int fitness, int professionalism)
        {{
            return new DemandSlotContentEntry
            {{
                id = id,
                requiredPosition = position,
                minimumAbility = ability,
                preferredFitness = fitness,
                preferredProfessionalism = professionalism,
                isRequired = true
            }};
        }}

        private static MailContentEntry Mail(
            string id, MailContentKind kind, int week,
            string senderZh, string senderEn, string subjectZh, string subjectEn,
            string previewZh, string previewEn, string bodyZh, string bodyEn,
            string sourceZh, string sourceEn,
            string playerId = "", string demandId = "")
        {{
            return new MailContentEntry
            {{
                id = id,
                kind = kind,
                publishedWeek = week,
                sender = L(senderZh, senderEn),
                subject = L(subjectZh, subjectEn),
                preview = L(previewZh, previewEn),
                body = L(bodyZh, bodyEn),
                sourceNote = L(sourceZh, sourceEn),
                relatedPlayerId = playerId,
                relatedDemandId = demandId
            }};
        }}

        private static MagazineIssueContent Issue(
            string id, int coverIndex, int week,
            string publicationZh, string publicationEn, string titleZh, string titleEn,
            string issueNumber, params MagazinePageContent[] pages)
        {{
            return new MagazineIssueContent
            {{
                id = id,
                coverIndex = coverIndex,
                publishedWeek = week,
                publicationName = L(publicationZh, publicationEn),
                issueTitle = L(titleZh, titleEn),
                issueNumber = issueNumber,
                pages = new List<MagazinePageContent>(pages)
            }};
        }}

        private static MagazinePageContent Page(
            MagazinePageLayout layout,
            string kickerZh, string kickerEn, string headlineZh, string headlineEn, string deckZh, string deckEn,
            string leftZh, string leftEn, string rightZh, string rightEn,
            string quoteZh, string quoteEn, string sidebarTitleZh, string sidebarTitleEn,
            string sidebarBodyZh, string sidebarBodyEn,
            string relatedPlayerId, int illustrationIndex = -1, int illustrationIndex2 = -1)
        {{
            return new MagazinePageContent
            {{
                layout = layout,
                kicker = L(kickerZh, kickerEn),
                headline = L(headlineZh, headlineEn),
                deck = L(deckZh, deckEn),
                bodyLeft = L(leftZh, leftEn),
                bodyRight = L(rightZh, rightEn),
                pullQuote = L(quoteZh, quoteEn),
                sidebarTitle = L(sidebarTitleZh, sidebarTitleEn),
                sidebarBody = L(sidebarBodyZh, sidebarBodyEn),
                relatedPlayerId = relatedPlayerId,
                illustrationIndex = illustrationIndex,
                illustrationIndex2 = illustrationIndex2
            }};
        }}

        private static LocalizedText L(string zh, string en)
        {{
            return new LocalizedText
            {{
                chineseSimplified = zh ?? string.Empty,
                english = en ?? string.Empty
            }};
        }}
    }}
}}
"""
    OUT_CS.write_text(template, encoding="utf-8")
    print(f"players={len(player_calls)} demands={len(demand_calls)} mails={len(mail_calls)} issues={len(issue_calls)}")
    for w in warnings:
        print(f"WARN missing en: {w}")
    print(f"missing en fields: {len(warnings)}")
    print(f"written: {OUT_CS}")


if __name__ == "__main__":
    main()
