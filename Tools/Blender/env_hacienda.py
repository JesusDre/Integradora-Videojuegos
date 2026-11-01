"""Hacienda decoration (issue #170): a stable, hay, pitchforks, troughs, a cart, a well, barrels,
firewood and fences, so the hacienda looks lived in. Low-poly with flat colors, like the other props.

Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/env_hacienda.py
Outputs Environment/Hacienda/env_<name>.fbx and the preview docs/img/props/hacienda.png.
Axes as in the other props: X right, Y forward (Unity's +Z), Z up; origin on the floor.
"""

import math
import os
import random
import sys

sys.path.append(os.path.dirname(__file__))

import lowpoly_props as lp  # noqa: E402

lp.reset_scene()
random.seed(170)
OUT = os.path.join(lp.REPO_ROOT, "Assets", "_Project", "Art", "Models", "Environment", "Hacienda")

hay = lp.make_material("Heno", "#B8964A", roughness=1.0)
hay_dark = lp.make_material("HenoOscuro", "#8E7134", roughness=1.0)
twine = lp.make_material("Mecate", "#5A4228", roughness=1.0)
wood = lp.make_material("MaderaVieja", "#5B4330", roughness=0.9)
wood_light = lp.make_material("MaderaClara", "#7A5C40", roughness=0.9)
wood_dark = lp.make_material("MaderaOscura", "#3A2A1E", roughness=0.9)
iron = lp.make_material("Hierro", "#3C3C3E", roughness=0.6, metallic=0.7)
stone = lp.make_material("Piedra", "#7A7268", roughness=1.0)
stone_dark = lp.make_material("PiedraOscura", "#5A534C", roughness=1.0)
water = lp.make_material("AguaTurbia", "#2E4038", roughness=0.1)
tile = lp.make_material("Teja", "#8A4A32", roughness=0.9)

made = []


def done(b, filename, preview=True):
    obj, _ = b.finish()
    lp.PROPS_DIR = OUT
    lp.export_static(obj, filename)
    if preview:
        made.append(obj)
    return obj


def bale(b, center, rot=0.0):
    """A hay bale (1.0 x 0.5 x 0.45 m) with two twine bands and a few loose tufts."""
    x, y, z = center
    b.box((x, y, z + 0.225), (1.0, 0.5, 0.45), hay, rotation=(0, 0, rot))
    for off in (-0.25, 0.25):
        dx, dy = math.cos(math.radians(rot)) * off, math.sin(math.radians(rot)) * off
        b.box((x + dx, y + dy, z + 0.225), (0.04, 0.52, 0.47), twine, rotation=(0, 0, rot))
    for _ in range(3):
        b.box((x + random.uniform(-0.4, 0.4), y + random.uniform(-0.2, 0.2), z + 0.46),
              (0.18, 0.04, 0.03), hay_dark, rotation=(0, 0, random.uniform(0, 180)))


# ---- Pacas de heno: one, and a stack of six (3 + 2 + 1).
b = lp.PropBuilder("PacaHeno")
bale(b, (0, 0, 0))
done(b, "env_paca_heno.fbx")

b = lp.PropBuilder("PacasApiladas")
for i in range(3):
    bale(b, (0, (i - 1) * 0.52, 0), 90)
for i in range(2):
    bale(b, (0, (i - 0.5) * 0.52, 0.45), 90)
bale(b, (0.05, 0, 0.9), 85)
done(b, "env_pacas_apiladas.fbx")

# ---- Trinche (pitchfork, 1.6 m): a wooden shaft and four iron tines. Lies along +Z (it gets leaned in Unity).
b = lp.PropBuilder("Trinche")
b.cylinder((0, 0, 0.65), 0.018, 1.3, wood_light, segments=6)
b.box((0, 0, 1.32), (0.26, 0.03, 0.03), iron)
for i in range(4):
    b.cylinder((-0.11 + i * 0.073, 0, 1.47), 0.008, 0.3, iron, segments=4, radius_top=0.003)
done(b, "env_trinche.fbx")

# ---- Bebedero (water trough, 2 m): planks around murky water.
b = lp.PropBuilder("Bebedero")
b.box((0, 0, 0.05), (2.0, 0.6, 0.1), wood_dark)
for side in (-1, 1):
    b.box((0, side * 0.28, 0.3), (2.0, 0.06, 0.5), wood)
    b.box((side * 0.97, 0, 0.3), (0.06, 0.6, 0.5), wood)
    b.box((side * 0.8, 0, -0.05), (0.12, 0.7, 0.12), wood_dark)
b.box((0, 0, 0.42), (1.88, 0.5, 0.02), water)
done(b, "env_bebedero.fbx")

# ---- Carreta (cart, 3 m with the tongue): a plank bed, two big spoked wheels, a tongue resting on the floor.
b = lp.PropBuilder("Carreta")
b.box((0, 0, 0.75), (1.4, 2.2, 0.08), wood)
for side in (-1, 1):
    b.box((side * 0.7, 0, 0.95), (0.06, 2.2, 0.35), wood_light)
    # wheel: a rim (thin cylinder) and spokes
    b.cylinder((side * 0.8, -0.1, 0.6), 0.6, 0.08, wood_dark, segments=16, rotation=(0, 90, 0))
    b.cylinder((side * 0.84, -0.1, 0.6), 0.5, 0.09, wood_light, segments=16, rotation=(0, 90, 0))
    b.cylinder((side * 0.85, -0.1, 0.6), 0.09, 0.16, iron, segments=8, rotation=(0, 90, 0))
b.box((0, -1.1, 0.95), (1.4, 0.06, 0.35), wood_light)
b.box((0, 1.6, 0.45), (0.1, 1.4, 0.08), wood, rotation=(-17, 0, 0))
b.cylinder((0, -0.1, 0.6), 0.04, 1.6, iron, segments=6, rotation=(0, 90, 0))
bale(b, (0.2, 0.3, 0.79), 10)
done(b, "env_carreta.fbx")

# ---- Barril (0.9 m): staves with iron hoops.
b = lp.PropBuilder("Barril")
b.lathe([(0.0, 0.27), (0.45, 0.32), (0.9, 0.27)], wood, segments=12)   # (height, radius)
for z in (0.12, 0.45, 0.78):
    b.cylinder((0, 0, z), 0.33 if z == 0.45 else 0.3, 0.04, iron, segments=12)
done(b, "env_barril.fbx")

# ---- Pozo (well, 1.8 m across): a stone ring, two posts, a crossbar, a little tiled roof and a bucket.
b = lp.PropBuilder("Pozo")
for k in range(10):
    a = k / 10 * 2 * math.pi
    b.box((math.cos(a) * 0.8, math.sin(a) * 0.8, 0.4), (0.55, 0.3, 0.8), stone if k % 2 else stone_dark,
          rotation=(0, 0, math.degrees(a) + 90))
b.cylinder((0, 0, 0.05), 0.65, 0.1, water, segments=12)
for side in (-1, 1):
    b.box((side * 0.85, 0, 1.3), (0.12, 0.12, 1.8), wood_dark)
b.cylinder((0, 0, 1.7), 0.05, 1.8, wood, segments=6, rotation=(0, 90, 0))
for side in (-1, 1):
    b.box((0, side * 0.35, 2.35), (2.0, 0.8, 0.06), tile, rotation=(side * 28, 0, 0))
b.cylinder((0.25, 0, 1.05), 0.12, 0.25, wood_light, segments=8, radius_top=0.14)
b.cylinder((0.25, 0, 1.45), 0.006, 0.5, twine, segments=3)
done(b, "env_pozo.fbx")

# ---- Leña (firewood pile, 1.6 m): logs stacked between two stakes.
b = lp.PropBuilder("Lena")
for row in range(4):
    for i in range(5 - row):
        x = (i - (4 - row) / 2) * 0.27
        b.cylinder((x, 0, 0.13 + row * 0.24), 0.12, 1.0, wood_light if (i + row) % 2 else wood, segments=7,
                   rotation=(90, 0, random.uniform(-4, 4)))
for side in (-1, 1):
    b.box((side * 0.8, 0, 0.55), (0.08, 0.08, 1.1), wood_dark)
done(b, "env_lena.fbx")

# ---- Cerca (fence, 4 m): two posts, three rails, one of them broken and sagging.
b = lp.PropBuilder("Cerca")
for x in (-2.0, 0.0, 2.0):
    b.box((x, 0, 0.6), (0.12, 0.12, 1.2), wood_dark)
for z in (0.35, 0.75):
    b.box((0, 0, z), (4.0, 0.06, 0.1), wood)
b.box((-1.0, 0, 1.0), (2.0, 0.06, 0.1), wood)
b.box((1.0, 0, 0.95), (2.0, 0.06, 0.1), wood, rotation=(0, 8, 0))
done(b, "env_cerca.fbx")

# ---- Caballeriza (stable, 10 x 4 m): a back wall, three stalls with half walls, posts, a sloped tiled
# roof, hay in the stalls and a feed box each. The front is open (the horses are long gone).
b = lp.PropBuilder("Caballeriza")
W, D, H = 10.0, 4.0, 3.0
b.box((0, D / 2, H / 2), (W, 0.2, H), wood_dark)                         # back wall
for side in (-1, 1):
    b.box((side * W / 2, D / 4, H / 2), (0.2, D / 2, H), wood_dark)       # side walls (back half)
    b.box((side * W / 2, -D / 4, 0.7), (0.2, D / 2, 1.4), wood)           # side walls (front, low)
for x in (-W / 6, W / 6):
    b.box((x, D / 4, 0.75), (0.12, D / 2, 1.5), wood)                     # stall dividers
for x in (-W / 2, -W / 6, W / 6, W / 2):
    b.box((x, -D / 2, H / 2), (0.2, 0.2, H), wood_dark)                  # front posts
b.box((0, -D / 2, H - 0.1), (W, 0.2, 0.2), wood_dark)                     # front beam
b.box((0, 0.1, H + 0.35), (W + 0.6, D + 1.2, 0.1), tile, rotation=(10, 0, 0))   # roof
for i, x in enumerate((-W / 3, 0, W / 3)):
    b.box((x, D / 2 - 0.35, 0.5), (1.2, 0.5, 0.1), wood_light)           # feed box
    b.box((x, D / 2 - 0.35, 0.65), (1.1, 0.4, 0.2), hay_dark)
    for _ in range(6):                                                    # straw on the floor
        b.box((x + random.uniform(-1.2, 1.2), random.uniform(-0.5, 1.4), 0.02),
              (random.uniform(0.4, 0.8), random.uniform(0.3, 0.6), 0.04), hay, rotation=(0, 0, random.uniform(0, 180)))
bale(b, (W / 3 + 0.2, -0.6, 0), 20)
done(b, "env_caballeriza.fbx")

lp.render_preview(made, "hacienda.png", resolution=(1800, 700))
