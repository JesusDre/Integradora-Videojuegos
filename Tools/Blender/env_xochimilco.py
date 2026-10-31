"""Xochimilco environment kit (issue #36): the pieces that replace the instance 1 blockout.

Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/env_xochimilco.py
Outputs one FBX per piece in Assets/_Project/Art/Models/Environment/Xochimilco/ and the preview
docs/img/props/kit_xochimilco.png. Sizes are real (1 unit = 1 m); every piece stands on Z = 0 at its pivot,
so in Unity it sits on the ground (or on the water line, for the boat).
"""

import math
import os
import random
import sys

sys.path.append(os.path.dirname(__file__))

import lowpoly_props as lp  # noqa: E402
from mathutils import Matrix  # noqa: E402

lp.reset_scene()
random.seed(36)
lp.PROPS_DIR = os.path.join(lp.REPO_ROOT, "Assets", "_Project", "Art", "Models", "Environment", "Xochimilco")

# Palette: night canals (GDD §22), plus the bright painted boats that make Xochimilco recognizable.
hull = lp.make_material("Trajinera_Casco", "#1F3A3A", roughness=0.8)
plank = lp.make_material("Tabla", "#5A4430", roughness=0.9)
plank_dark = lp.make_material("TablaOscura", "#3A2C20", roughness=0.9)
canopy = lp.make_material("Toldo", "#D8CFB8", roughness=0.95)
paint = [lp.make_material(n, c, roughness=0.7) for n, c in (
    ("Pintura_Roja", "#B8322A"), ("Pintura_Amarilla", "#E0B23A"), ("Pintura_Verde", "#2E8A4E"),
    ("Pintura_Azul", "#2E5EA8"), ("Pintura_Rosa", "#D0607E"))]
adobe = lp.make_material("Adobe", "#7A6248", roughness=1.0)
thatch = lp.make_material("Zacate", "#8A7442", roughness=1.0)
bark = lp.make_material("Corteza", "#3E3226", roughness=1.0)
leaves = lp.make_material("Follaje", "#2E4A30", roughness=1.0)
leaves_dark = lp.make_material("FollajeOscuro", "#22382A", roughness=1.0)
moss = lp.make_material("Heno", "#7C8A6A", roughness=1.0)
reed = lp.make_material("Tule", "#5E6E3A", roughness=1.0)
stone = lp.make_material("Piedra", "#5C5A56", roughness=0.95)
stone_dark = lp.make_material("PiedraOscura", "#3C3A38", roughness=0.95)
flower = lp.make_material("Cempasuchil", "#E8901C", roughness=0.8, emission_strength=0.3)

made = []


def done(b, filename, preview=True):
    obj, _ = b.finish()
    lp.export_static(obj, filename)
    if preview:
        made.append(obj)
    return obj


# ---- Trajinera (9 x 2.4 m): flat hull, benches, a canopy on posts and the painted arch at the bow (+Y).
b = lp.PropBuilder("Trajinera")
b.prism([(-1.2, -4.5), (1.2, -4.5), (1.25, 3.6), (0.6, 4.6), (-0.6, 4.6), (-1.25, 3.6)], -0.25, 0.25, hull)
b.box((0, 0, 0.27), (2.2, 8.6, 0.04), plank)                                     # deck
for y in (-2.8, -0.8, 1.2):
    b.box((0, y, 0.55), (2.0, 0.35, 0.06), plank_dark)                           # benches
    b.box((0, y, 0.4), (0.1, 0.3, 0.3), plank_dark)
b.box((0, 0.2, 0.75), (0.9, 2.2, 0.05), plank)                                   # long table
for x in (-1.05, 1.05):
    for y in (-3.8, 3.0):
        b.box((x, y, 1.35), (0.08, 0.08, 2.1), plank_dark)                       # canopy posts
b.box((0, -0.4, 2.45), (2.4, 7.2, 0.08), canopy)                                 # canopy
# The arch: a painted board with stripes, the signature of a trajinera.
for i, mat in enumerate(paint):
    b.box((0, 3.6, 2.6 + i * 0.12), (2.4 - i * 0.25, 0.08, 0.12), mat)
b.box((0, 3.62, 3.25), (0.9, 0.06, 0.25), paint[1])
done(b, "env_trajinera.fbx")

# ---- Choza del cuidador (12 x 11 m, the safe room): adobe walls, a thatched roof, door openings
# matching the scene (west door 2 m wide near the south wall, east door 2.5 m near the north wall).
b = lp.PropBuilder("Choza")
W, D, HGT = 12.0, 11.0, 3.0


def wall_x(y, x0, x1):
    b.box(((x0 + x1) / 2, y, HGT / 2), (x1 - x0, 0.3, HGT), adobe)


def wall_y(x, y0, y1):
    b.box((x, (y0 + y1) / 2, HGT / 2), (0.3, y1 - y0, HGT), adobe)


wall_x(-D / 2, -W / 2, W / 2)
wall_x(D / 2, -W / 2, W / 2)
wall_y(-W / 2, -D / 2, -3.0); wall_y(-W / 2, -1.0, D / 2)          # west, door from y -3 to -1
wall_y(W / 2, -D / 2, 1.0); wall_y(W / 2, 3.5, D / 2)              # east, door from y 1 to 3.5
for x, y0, y1 in ((-W / 2, -3.0, -1.0), (W / 2, 1.0, 3.5)):
    b.box((x, (y0 + y1) / 2, HGT - 0.25), (0.32, y1 - y0, 0.5), adobe)  # lintels over the doors
# Thatched hip roof: a low pyramid with an overhang.
over = 0.7
pts = [(-W / 2 - over, -D / 2 - over, HGT), (W / 2 + over, -D / 2 - over, HGT), (W / 2 + over, D / 2 + over, HGT),
       (-W / 2 - over, D / 2 + over, HGT), (0, 0, HGT + 2.2)]
b.hull(pts + [(p[0], p[1], p[2] + 0.25) for p in pts[:4]], thatch)
b.box((0, -D / 2 - 0.1, 2.2), (1.4, 0.1, 0.8), plank_dark)                  # small shuttered window
done(b, "env_choza.fbx")


# ---- Ahuejote (8 m): the slim tree that lines the chinampas.
def ahuejote(name, height, filename):
    b = lp.PropBuilder(name)
    b.cylinder((0, 0, height * 0.25), 0.18, height * 0.5, bark, segments=7, radius_top=0.12)
    rings = [(height * 0.15, 0.4), (height * 0.35, 0.95), (height * 0.6, 0.85), (height * 0.85, 0.45), (height, 0.05)]
    b.lathe(rings, leaves, segments=8)
    return done(b, filename)


ahuejote("Ahuejote", 8.0, "env_ahuejote.fbx")
ahuejote("AhuejoteBajo", 6.0, "env_ahuejote_bajo.fbx", )

# ---- Ahuehuete (12 m wide): the big old tree of the island, with hanging moss.
b = lp.PropBuilder("Ahuehuete")
b.cylinder((0, 0, 2.5), 1.1, 5.0, bark, segments=10, radius_top=0.7)
for a in range(5):
    ang = a * 2 * math.pi / 5
    b.cylinder((math.cos(ang) * 1.2, math.sin(ang) * 1.2, 0.4), 0.45, 0.8, bark, segments=6, rotation=(30 * math.sin(ang), 30 * math.cos(ang), 0))
for k in range(7):
    ang = k * 2 * math.pi / 7 + random.uniform(-0.2, 0.2)
    r = random.uniform(2.5, 4.5)
    c = (math.cos(ang) * r, math.sin(ang) * r, random.uniform(6.0, 8.0))
    b.sphere(c, random.uniform(2.2, 3.0), leaves_dark if k % 2 else leaves, segments=8, rings=5, scale=(1.0, 1.0, 0.6))
    b.box((c[0], c[1], c[2] - 2.0), (0.15, 0.15, 2.2), moss)                  # hanging moss (heno)
b.sphere((0, 0, 8.5), 3.2, leaves, segments=8, rings=5, scale=(1, 1, 0.6))
done(b, "env_ahuehuete.fbx")

# ---- Embarcadero (6 x 3 m): planks on posts, the deck at 0.1 m above the chinampa.
b = lp.PropBuilder("Embarcadero")
for i in range(12):
    b.box((-2.75 + i * 0.5, 0, 0.1), (0.46, 3.0, 0.06), plank if i % 2 else plank_dark)
for x in (-2.8, 0, 2.8):
    for y in (-1.4, 1.4):
        b.cylinder((x, y, -0.4), 0.09, 1.2, plank_dark, segments=6)
b.box((2.8, 1.4, 0.7), (0.1, 0.1, 1.0), plank_dark)                         # mooring post
done(b, "env_embarcadero.fbx")

# ---- Puente (4 x 3 m): a wooden footbridge with rails, slightly arched.
b = lp.PropBuilder("Puente")
for i in range(9):
    x = -1.8 + i * 0.45
    z = 0.12 + 0.18 * math.cos(x / 2.2 * math.pi / 2)
    b.box((x, 0, z), (0.42, 2.6, 0.06), plank)
for y in (-1.3, 1.3):
    for x in (-1.8, 0, 1.8):
        b.box((x, y, 0.6), (0.08, 0.08, 1.0), plank_dark)
    b.box((0, y, 1.05), (3.8, 0.07, 0.07), plank_dark)
done(b, "env_puente.fbx")

# ---- Poste de muñecas (2.2 m): a weathered post with a crossbar to hang the dolls from.
b = lp.PropBuilder("PosteMunecas")
b.cylinder((0, 0, 1.1), 0.07, 2.2, plank_dark, segments=6)
b.box((0, 0, 2.0), (0.9, 0.06, 0.06), plank_dark)
b.box((0.3, 0, 1.75), (0.02, 0.02, 0.45), moss)                             # rope ends
b.box((-0.32, 0, 1.8), (0.02, 0.02, 0.35), moss)
done(b, "env_poste_munecas.fbx")

# ---- Tule (reeds, 1.4 m): a clump to dress the shores.
b = lp.PropBuilder("Tule")
for k in range(14):
    ang, r = random.uniform(0, 2 * math.pi), random.uniform(0, 0.45)
    h = random.uniform(0.8, 1.5)
    tilt = (random.uniform(-12, 12), random.uniform(-12, 12), 0)
    b.cylinder((math.cos(ang) * r, math.sin(ang) * r, h / 2), 0.02, h, reed, segments=4, rotation=tilt, radius_top=0.004)
done(b, "env_tule.fbx")

# ---- Altar del ancla (3.4 m): stone platform and pedestal where the dolls are offered, with marigolds.
b = lp.PropBuilder("AltarAncla")
b.lathe([(0.0, 1.7), (0.12, 1.65), (0.14, 1.2), (0.22, 1.15)], stone, segments=12)
b.lathe([(0.22, 0.6), (0.85, 0.55), (0.9, 0.62)], stone_dark, segments=10)
for k in range(10):
    ang = k * 2 * math.pi / 10
    b.sphere((math.cos(ang) * 1.4, math.sin(ang) * 1.4, 0.2), 0.12, flower, segments=6, rings=4)
done(b, "env_altar_ancla.fbx")

lp.render_preview(made, "kit_xochimilco.png", resolution=(2000, 700))
