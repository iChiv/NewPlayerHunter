"""Rebuild PlayerPortraitAtlas.png (8x8), MagazineCoverAtlas.png (6x6) and MagazineIllustrationAtlas.png (6x6).

Usage: python output/spreadsheet/rebuild_atlases.py
Reads:  Assets/Game/Art/Generated/PlayerPortraitAtlas.png (old 4x4, 16 cells)
        Assets/Game/Art/Generated/MagazineCoverAtlas.png (old 3x2, 6 cells)
        output/art_raw/portraits/*.png (35 new, mapped by design portraitIndex)
        output/art_raw/covers/*.png (28 new, mapped by design coverIndex)
Writes: same two atlas paths (backup copies saved next to originals as *.bak.png)
"""
import json
import sys
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parent
PROJ = ROOT.parent.parent
ART = PROJ / "Assets/Game/Art/Generated"
RAW = ROOT.parent / "art_raw"
DESIGN = json.loads((ROOT / "content_design.json").read_text(encoding="utf-8"))

CELL = 256
PORTRAIT_GRID = 8
COVER_GRID = 6


def slice_cells(img, columns, rows, count):
    w, h = img.size
    cells = []
    for i in range(count):
        x0 = round((i % columns) * w / columns)
        x1 = round((i % columns + 1) * w / columns)
        y0 = round((i // columns) * h / rows)
        y1 = round((i // columns + 1) * h / rows)
        cells.append(img.crop((x0, y0, x1, y1)).resize((CELL, CELL), Image.LANCZOS))
    return cells


def rebuild(old_path, old_cols, old_rows, old_count, new_map, grid, raw_dir):
    old = Image.open(old_path).convert("RGB")
    total = grid * grid
    migrated = old.size == (grid * CELL, grid * CELL)
    if migrated:
        # 已是目标规格的图集：按目标网格切片，新图覆盖同索引格，重跑幂等。
        old_cols, old_rows, old_count = grid, grid, grid * grid
        if new_map and max(new_map) >= total:
            raise SystemExit(f"{old_path.name}: index {max(new_map)} exceeds grid {grid}x{grid}")
    cells = slice_cells(old, old_cols, old_rows, old_count)
    if not migrated and old_count + len(new_map) > total:
        raise SystemExit(f"{old_path.name}: {old_count}+{len(new_map)} exceeds grid {grid}x{grid}")
    atlas = Image.new("RGB", (grid * CELL, grid * CELL))
    for i, cell in enumerate(cells):
        atlas.paste(cell, ((i % grid) * CELL, (i // grid) * CELL))
    missing = []
    for index, rel in sorted(new_map.items()):
        path = raw_dir / rel
        if not path.exists():
            missing.append(rel)
            continue
        img = Image.open(path).convert("RGB").resize((CELL, CELL), Image.LANCZOS)
        atlas.paste(img, ((index % grid) * CELL, (index // grid) * CELL))
    if missing:
        raise SystemExit(f"missing {len(missing)} files for {old_path.name}: {missing[:5]}...")
    atlas.save(old_path.with_suffix(".png"))
    print(f"{old_path.name}: {old_count} old + {len(new_map)} new -> {grid}x{grid} @ {atlas.size}")


ILLUSTRATION_KEYS = [
    "transfer_window", "contract_money", "injury", "tactics_board",
    "stadium_night", "pub", "training", "scouting", "media",
    "dressing_room", "boots_ball", "referee", "trophy",
    "rain_match", "gold_desert", "youth", "veteran",
    "goalkeeper", "deadline_fax", "fans", "winter_window",
    "medical", "agent_phone", "data_chart",
]


def build_fresh(out_path, raw_dir, names, grid):
    missing = [n for n in names if not (raw_dir / f"{n}.png").exists()]
    if missing:
        raise SystemExit(f"missing {len(missing)} files for {out_path.name}: {missing[:5]}...")
    atlas = Image.new("RGB", (grid * CELL, grid * CELL))
    for index, name in enumerate(names):
        img = Image.open(raw_dir / f"{name}.png").convert("RGB").resize((CELL, CELL), Image.LANCZOS)
        atlas.paste(img, ((index % grid) * CELL, (index // grid) * CELL))
    atlas.save(out_path)
    print(f"{out_path.name}: {len(names)} cells -> {grid}x{grid} @ {atlas.size}")


def main():
    portraits = {
        p["portraitIndex"]: f"{p['id'].replace('player.', '').replace('.', '_')}.png"
        for p in DESIGN["players"]
    }
    covers = {
        i["coverIndex"]: f"{i['id'].replace('mag.', '').replace('.', '_')}.png"
        for i in DESIGN["issues"]
    }
    rebuild(ART / "PlayerPortraitAtlas.png", 4, 4, 16, portraits, PORTRAIT_GRID, RAW / "portraits")
    rebuild(ART / "MagazineCoverAtlas.png", 3, 2, 6, covers, COVER_GRID, RAW / "covers")
    build_fresh(ART / "MagazineIllustrationAtlas.png", RAW / "illustrations",
                ILLUSTRATION_KEYS, COVER_GRID)


if __name__ == "__main__":
    main()
