"""Build art generation manifest for grok CLI batch runs.

Usage: python output/spreadsheet/make_art_manifest.py
Writes: output/art_raw/manifest.tsv  (relative_png_path<TAB>prompt per line)
"""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parent
ART = ROOT.parent / "art_raw"
DESIGN = json.loads((ROOT / "content_design.json").read_text(encoding="utf-8"))

PORTRAIT_STYLE = (
    "Original painterly oil-illustration portrait of a fictional football (soccer) player, "
    "waist-up, square 1:1 composition. Classic sports editorial oil painting style, soft visible "
    "brush strokes, muted color palette. Plain dark navy blue jersey with cream shoulder panels, "
    "no logos, no sponsor text, no badges, no numbers. Muted solid-color background. "
    "No text, no watermark, no real person likeness. Subject: "
)

COVER_STYLE = (
    "Original painterly oil-illustration magazine cover artwork, square 1:1 composition. "
    "Moody dramatic football (soccer) editorial scene, muted navy teal and burnt orange palette, "
    "soft brush strokes, cinematic lighting, metaphorical composition. "
    "Absolutely no text, no letters, no typography, no logos, no badges, no watermark, "
    "no real person likeness. Scene: "
)

COVER_BRIEFS = {
    "mag.season.07": "a footballer in a navy kit running onto a pitch carrying a fire extinguisher, transfer papers flying in the summer wind, dawn light over a small stadium",
    "mag.season.08": "a footballer silhouette standing atop a desert dune made of gold coins, a distant palace skyline, a second shadowy figure with a question mark above his head",
    "mag.season.09": "a massive muscular player charging forward like a freight train through mist, subtle speed lines, a giant stopwatch floating in the sky",
    "mag.season.10": "a goalkeeper calmly passing the ball with his feet on a chessboard-patterned pitch, in the far background an elegant aging star walking away under stadium lights",
    "mag.season.11": "a joyful teenage kid juggling a worn football in a narrow hillside alley, laundry lines overhead, an old captain armband hanging on a rusted fence nearby",
    "mag.season.12": "a ghostly striker materializing inside a foggy six-yard box, a lightning bolt splitting the night sky above the pitch",
    "mag.season.13": "a marathon runner dribbling a football on an endless road at sunrise, beside him a construction worker in boots leaning on a brick wall shaped like a defensive wall",
    "mag.season.14": "a cautious midfielder passing a ball wrapped in warning tape, while a short goalkeeper leaps impossibly high toward a cross above him",
    "mag.season.15": "a flashy young man juggling a football surrounded by glowing phone screens and a ring light, behind him a scrawny kid in oversized boots at a backyard barbecue pitch",
    "mag.season.16": "a futsal player with a blur of quick feet on a small indoor court, through a doorway a hunter quietly watching a goalmouth through binoculars",
    "mag.season.17": "a youthful forward with ancient knowing eyes holding an hourglass, sand falling upward, behind him a gaunt runner chasing his own shadow around a track",
    "mag.season.18": "a midfielder depicted as a precise clockwork mechanism at the center circle, gears and hands, next to him one player wearing three different colored position bibs at once",
    "mag.season.19": "a veteran goalkeeper sitting on a long bench cradling a trophy like a baby, a referee nearby performing a card trick with yellow cards",
    "mag.season.21": "a defender with boxing gloves guarding the goal box, a sly striker rolling dice at the penalty spot under floodlights",
    "mag.season.24": "a falcon-like winger cutting inside past defenders on a stormy evening, a grey-bearded captain pointing directions like a lighthouse keeper",
    "mag.season.26": "a footballer inside a giant medical scanner shaped like a stopwatch, snow falling on an empty training pitch, a family photo slipping from a kit bag",
    "mag.season.28": "a frosty transfer window opening like a shop door in winter, young players visible behind frosted glass with price tags, scouts outside in coats",
    "mag.season.29": "prospectors panning for gold nuggets shaped like footballs in a river, discarded jerseys from big clubs floating past",
    "mag.season.30": "a veteran midfielder holding a glowing knockout bracket like a treasure map, a calm striker lacing boots in a quiet tunnel before a storm final",
    "mag.season.31": "a jammed fax machine overflowing with transfer papers, a shop window slamming shut, clerks sweeping coffee-stained contracts into a bin",
    "mag.season.33": "a defender launching a long counterattack pass that turns into a glowing arrow across a dark pitch, a giant clock showing only three seconds",
    "mag.season.36": "a winger drawn as a soft shadow slipping between spotlight cones toward the far post, defenders looking the other way",
    "mag.season.39": "one exhausted player juggling three position bibs, standing on a shrinking coin stack, a tiny squad photo frame",
    "mag.season.42": "sprinters on an athletics track that curls into a league table, tiny numbers flying off like leaves",
    "mag.season.45": "a substitutes bench stretching into darkness under one spotlight, a lone goalkeeper gloves on knees waiting",
    "mag.season.48": "a giant silver trophy looming over a city skyline at dusk, tiny players at its base looking up, confetti beginning to fall",
    "mag.season.49": "a goalkeeper alone in a huge empty stadium facing a glowing penalty spot like an interrogation lamp",
    "mag.season.51": "an award ceremony stage set on a football pitch at night, confetti, a giant season calendar book closing, silhouettes of the year's characters in the crowd",
}


def main():
    lines = []
    for p in DESIGN["players"]:
        fname = f"portraits/{p['id'].replace('player.', '').replace('.', '_')}.png"
        lines.append((fname, PORTRAIT_STYLE + p["portraitBrief"]))
    for i in DESIGN["issues"]:
        fname = f"covers/{i['id'].replace('mag.', '').replace('.', '_')}.png"
        lines.append((fname, COVER_STYLE + COVER_BRIEFS[i["id"]]))
    out = ART / "manifest.tsv"
    with out.open("w", encoding="utf-8", newline="\n") as f:
        for fname, prompt in lines:
            f.write(f"{fname}\t{prompt} Save the image as a PNG file named exactly '{Path(fname).name}' in the current directory.\n")
    print(f"manifest lines: {len(lines)} -> {out}")


if __name__ == "__main__":
    main()
