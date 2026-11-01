"""Scare screens (issue #137): turns a close-up render of an entity's face into a jump-scare image.

The renders come from Unity (a temporary camera in front of the head bone, lit red from below; see
DEFENSA "Susto al ser atrapado"). Run from the repo root:
    python3 Tools/Iconos/procesar_sustos.py <folder with the renders> [names...]
With names (e.g. Muneca), only those are processed, so the others stay exactly as they are.
Writes Assets/_Project/UI/Sustos/SUSTO_<name>.png. The treatment: a tight crop on the face, almost no
color except the reds, crushed blacks, a heavy vignette, a slight warp so it doesn't look clean, film
grain, and the eyes blown out with a red glow.
"""

import os
import sys

import numpy as np
from PIL import Image, ImageFilter

SRC = sys.argv[1] if len(sys.argv) > 1 else "."
OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "_Project", "UI", "Sustos")
rng = np.random.default_rng(137)

# Crop box (fraction of the render) around each face.
CROPS = {"Llorona": (0.14, 0.18, 0.86, 0.90), "Charro": (0.16, 0.14, 0.84, 0.82),
         "Muneca": (0.08, 0.06, 0.92, 0.90),   # the possessed doll (#172)
         "Chaneque": (0.1, 0.0, 0.9, 0.8)}      # the chaneque (#173)
# The color that survives the treatment: red for everyone, the chaneque's eyes are yellow.
KEEP = {"Chaneque": "yellow"}


def treat(name):
    img = Image.open(os.path.join(SRC, name + ".png")).convert("RGB")
    w, h = img.size
    x0, y0, x1, y1 = CROPS[name]
    img = img.crop((int(x0 * w), int(y0 * h), int(x1 * w), int(y1 * h))).resize((1024, 1024), Image.LANCZOS)
    a = np.asarray(img).astype(float) / 255.0

    # Keep the reds, drain everything else toward a cold gray.
    gray = a.mean(axis=2, keepdims=True)
    # Only what is truly red (eyes, mouth, the Charro's bow), not skin that the red light tinted.
    if KEEP.get(name) == "yellow":
        redness = np.clip((np.minimum(a[..., 0:1], a[..., 1:2]) - a[..., 2:3] * 1.6 - 0.25) * 3.0, 0, 1)
        tint = np.array([1.2, 1.0, 0.25])
    else:
        redness = np.clip((a[..., 0:1] - np.maximum(a[..., 1:2], a[..., 2:3]) * 1.15 - 0.12) * 3.0, 0, 1)
        tint = np.array([1.15, 0.45, 0.4])
    a = gray * np.array([0.82, 0.86, 0.9]) * (1 - redness) + a * tint * redness
    # Crushed blacks and harsh contrast.
    a = np.clip((a - 0.12) * 1.7, 0, 1) ** 1.25

    # A slight wave warp, so the image doesn't look like a clean render.
    yy, xx = np.mgrid[0:1024, 0:1024]
    dx = (np.sin(yy / 37.0) * 6 + np.sin(yy / 11.0) * 2).astype(int)
    a = a[yy, np.clip(xx + dx, 0, 1023)]

    # Heavy vignette and film grain.
    r = np.sqrt(((xx - 512) / 512.0) ** 2 + ((yy - 540) / 512.0) ** 2)
    a *= np.clip(1.25 - r ** 1.6, 0, 1)[..., None]
    a = np.clip(a + rng.normal(0, 0.05, a.shape[:2])[..., None], 0, 1)

    out = Image.fromarray((a * 255).astype(np.uint8))
    # Blown-out eyes: the brightest red spots get a red bloom.
    if KEEP.get(name) == "yellow":
        bright = Image.fromarray((np.clip((np.minimum(a[..., 0], a[..., 1]) - a[..., 2] * 1.6) * 3, 0, 1) * 255).astype(np.uint8))
        bloom = bright.filter(ImageFilter.GaussianBlur(18))
        glow = Image.merge("RGB", (bloom, bloom.point(lambda v: int(v * 0.8)), Image.new("L", bloom.size, 0)))
    else:
        bright = Image.fromarray((np.clip((a[..., 0] - a[..., 1] * 1.4) * 3, 0, 1) * 255).astype(np.uint8))
        bloom = bright.filter(ImageFilter.GaussianBlur(18))
        glow = Image.merge("RGB", (bloom, Image.new("L", bloom.size, 0), Image.new("L", bloom.size, 0)))
    out = Image.blend(out, Image.fromarray(np.clip(np.asarray(out).astype(int) + np.asarray(glow).astype(int), 0, 255).astype(np.uint8)), 0.8)
    os.makedirs(OUT, exist_ok=True)
    out.save(os.path.join(OUT, f"SUSTO_{name}.png"))
    print("SUSTO_" + name)


for n in (sys.argv[2:] or CROPS):
    treat(n)
