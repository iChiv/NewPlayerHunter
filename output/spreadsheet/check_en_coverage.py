"""Check English coverage for the content pipeline.

Scans prose_players.json, prose_demands.json, prose_magazines_a.json,
prose_magazines_b.json and content_design.json for missing '*En' sibling keys,
and scans SixWeekContentFactory.cs for untranslated Zh(...) / bad L(...) calls.

Exit 0 when nothing is missing, exit 1 otherwise.
"""
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
PROJ = ROOT.parent.parent
SIX_WEEK_CS = PROJ / "Assets/Game/Scripts/Gameplay/SixWeekContentFactory.cs"

PLAYER_FIELDS = ["name", "biography", "claim", "evidence", "careerHistory",
                 "expirySubject", "expiryBody"]
MAIL_FIELDS = ["sender", "subject", "preview", "body", "sourceNote"]
DEMAND_FIELDS = ["clubName", "clubStanding", "clubBest", "clubProfile",
                 "title", "description", "paymentTerms", "expirySubject", "expiryBody"]
PAGE_FIELDS = ["kicker", "headline", "deck", "bodyLeft", "bodyRight",
               "pullQuote", "sidebarTitle", "sidebarBody"]

CJK_RE = re.compile(r"[　-〿㐀-䶿一-鿿豈-﫿＀-￯]")
ZH_CALL_RE = re.compile(r'(?<![A-Za-z0-9_])Zh\(\s*"((?:[^"\\]|\\.)*)"')
L_CALL_RE = re.compile(
    r'(?<![A-Za-z0-9_])L\(\s*"(?:[^"\\]|\\.)*"\s*,\s*"((?:[^"\\]|\\.)*)"\s*\)')


def load(name):
    return json.loads((ROOT / name).read_text(encoding="utf-8"))


def missing_en(container, base):
    raw = container.get(base + "En")
    return not (isinstance(raw, str) and raw.strip())


def check_json():
    missing = []

    players = load("prose_players.json")
    for pid, prose in players.items():
        for base in PLAYER_FIELDS:
            if missing_en(prose, base):
                missing.append(f"{pid}.{base}En")
        rm = prose.get("resumeMail") or {}
        for base in MAIL_FIELDS:
            if missing_en(rm, base):
                missing.append(f"{pid}.resumeMail.{base}En")

    demands = load("prose_demands.json")
    for did, prose in demands.items():
        for base in DEMAND_FIELDS:
            if missing_en(prose, base):
                missing.append(f"{did}.{base}En")
        cm = prose.get("clubRequestMail") or {}
        for base in MAIL_FIELDS:
            if missing_en(cm, base):
                missing.append(f"{did}.clubRequestMail.{base}En")

    mags = {**load("prose_magazines_a.json"), **load("prose_magazines_b.json")}
    for iid, prose in mags.items():
        if missing_en(prose, "issueTitle"):
            missing.append(f"{iid}.issueTitleEn")
        for pi, pg in enumerate(prose.get("pages") or []):
            for base in PAGE_FIELDS:
                if missing_en(pg, base):
                    missing.append(f"{iid}.pages[{pi}].{base}En")

    design = load("content_design.json")
    for di in design.get("issues") or []:
        if missing_en(di, "publication"):
            missing.append(f"{di.get('id')}.publicationEn")

    return missing


def check_six_week():
    if not SIX_WEEK_CS.exists():
        return 0, 0
    text = SIX_WEEK_CS.read_text(encoding="utf-8")
    zh_cjk = sum(1 for m in ZH_CALL_RE.finditer(text) if CJK_RE.search(m.group(1)))
    l_bad = 0
    for m in L_CALL_RE.finditer(text):
        en = m.group(1)
        if not en.strip() or CJK_RE.search(en):
            l_bad += 1
    return zh_cjk, l_bad


def main():
    missing = check_json()
    zh_cjk, l_bad = check_six_week()

    for m in missing:
        print(f"missing en: {m}")
    if zh_cjk:
        print(f"SixWeekContentFactory.cs: Zh(...) single-arg calls containing CJK: {zh_cjk}")
    if l_bad:
        print(f"SixWeekContentFactory.cs: L(...) calls with empty/CJK english arg: {l_bad}")

    total = len(missing) + zh_cjk + l_bad
    print(f"summary: json missing en={len(missing)}, sixweek Zh+CJK={zh_cjk}, "
          f"sixweek L bad en={l_bad}, total={total}")
    sys.exit(1 if total else 0)


if __name__ == "__main__":
    main()
