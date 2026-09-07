"""Extract content-person submission xlsx -> JSON working copy + validation report.

Usage: python output/spreadsheet/extract_batch.py [xlsx_path]
Writes: output/spreadsheet/content_batch_01.json
Prints: validation report to stdout.
"""
import json
import re
import sys
from pathlib import Path

import openpyxl

ROOT = Path(__file__).resolve().parent
DEFAULT_XLSX = Path.home() / "Downloads" / "NewPlayerHunter_Content_Submission_Template.xlsx"
OUT_JSON = ROOT / "content_batch_01.json"

POSITIONS = {"Goalkeeper", "Defender", "WingBack", "Midfielder", "Winger", "Forward"}
RELIABILITY = {"Unverified", "Low", "Medium", "High"}

PLAYER_COLS = [
    "player_id", "submit_status", "display_name_zh", "display_name_en",
    "biography_zh", "biography_en", "public_position", "hidden_ability",
    "hidden_fitness", "hidden_professionalism", "public_claim_zh",
    "public_claim_en", "public_evidence_zh", "public_evidence_en",
    "evidence_reliability", "inspiration_notes_private", "portrait_brief",
    "asset_filename", "integrator_notes",
]
DEMAND_COLS = [
    "demand_id", "submit_status", "club_id", "title_zh", "title_en",
    "description_zh", "description_en", "opened_week", "deadline_week",
    "base_reward", "payment_terms_zh", "payment_terms_en",
    "narrative_notes", "integrator_notes",
]


def rows_of(ws, cols, start=6):
    out = []
    for i, row in enumerate(ws.iter_rows(min_row=start, values_only=True), start):
        if not any(v is not None and str(v).strip() for v in row):
            continue
        rec = {c: (None if v is None else (int(v) if isinstance(v, float) and float(v).is_integer() else v))
               for c, v in zip(cols, row)}
        rec["_row"] = i
        out.append(rec)
    return out


def validate(players, demands):
    issues = []
    names = {}
    for p in players:
        r = p["_row"]
        name = (p.get("display_name_zh") or "").strip()
        if not name:
            issues.append(f"球员 R{r}: 缺 display_name_zh")
        else:
            names.setdefault(name, []).append(r)
            if not (2 <= len(name) <= 14):
                issues.append(f"球员 R{r}: 姓名长度 {len(name)} 超出建议范围")
        pos = (p.get("public_position") or "").strip()
        if pos not in POSITIONS:
            issues.append(f"球员 R{r}: 非法位置 '{pos}'")
        rel = (p.get("evidence_reliability") or "").strip()
        if rel not in RELIABILITY:
            issues.append(f"球员 R{r}: 非法可靠性 '{rel}'")
        for f in ("hidden_ability", "hidden_fitness", "hidden_professionalism"):
            v = p.get(f)
            try:
                if v is None or not (0 <= int(v) <= 100):
                    raise ValueError
            except (TypeError, ValueError):
                issues.append(f"球员 R{r}: {f} 非 0-100 整数: {v!r}")
        for f, lo, hi in (("biography_zh", 1, 120), ("public_claim_zh", 1, 200),
                          ("public_evidence_zh", 0, 200)):
            v = (p.get(f) or "").strip()
            if not v and lo > 0:
                issues.append(f"球员 R{r}: 缺 {f}")
            elif v and not (lo <= len(v) <= hi):
                issues.append(f"球员 R{r}: {f} 长度 {len(v)} 超出 {lo}-{hi}")
    for name, rows in names.items():
        if len(rows) > 1:
            issues.append(f"球员重名 '{name}': 行 {rows}")

    titles = {}
    for d in demands:
        r = d["_row"]
        t = (d.get("title_zh") or "").strip()
        if not t:
            issues.append(f"招聘 R{r}: 缺 title_zh")
        else:
            titles.setdefault(t, []).append(r)
        desc = (d.get("description_zh") or "").strip()
        if not desc:
            issues.append(f"招聘 R{r}: 缺 description_zh")
    for t, rows in titles.items():
        if len(rows) > 1:
            issues.append(f"招聘标题重复 '{t}': 行 {rows}")
    return issues


def main():
    xlsx = Path(sys.argv[1]) if len(sys.argv) > 1 else DEFAULT_XLSX
    wb = openpyxl.load_workbook(xlsx, data_only=True)
    players = rows_of(wb["球员 Players"], PLAYER_COLS)
    demands = rows_of(wb["招聘 Demands"], DEMAND_COLS)
    payload = {"source": str(xlsx), "players": players, "demands": demands}
    OUT_JSON.write_text(json.dumps(payload, ensure_ascii=False, indent=2), encoding="utf-8")

    print(f"players={len(players)} demands={len(demands)}")
    print(f"written: {OUT_JSON}")
    issues = validate(players, demands)
    print(f"\n== 校验报告 ({len(issues)} 项) ==")
    for msg in issues:
        print(" -", msg)


if __name__ == "__main__":
    main()
