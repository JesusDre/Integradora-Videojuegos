"""The three key items (GDD §10): muñeca ofrenda, moneda del pacto and fragmento de obsidiana.

Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/obj_objetos_clave.py
Outputs Assets/_Project/Art/Models/Props/obj_muneca.fbx, obj_moneda.fbx, obj_obsidiana.fbx and
docs/img/props/objetos_clave.png. Each one has a strong silhouette and its own color (white, gold,
black with red) so it reads from far away in the dark (issue #29).
"""

import math
import os
import random
import sys

sys.path.append(os.path.dirname(__file__))

import lowpoly_props as lp  # noqa: E402
from mathutils import Matrix  # noqa: E402

lp.reset_scene()
random.seed(29)  # same shard every time the script runs

# ---- Muñeca ofrenda (~0.40 m): a dirty porcelain doll like the ones hanging in Xochimilco.
dress = lp.make_material("Muneca_Vestido", "#CFC6B4", roughness=0.9)
skin = lp.make_material("Muneca_Porcelana", "#E6DACA", roughness=0.5)
hair = lp.make_material("Muneca_Pelo", "#16110E", roughness=0.9)
eye = lp.make_material("Muneca_Ojos", "#050505", roughness=0.3)
ribbon = lp.make_material("Muneca_Liston", "#8A1414", roughness=0.7)
b = lp.PropBuilder("Muneca")
b.lathe([(0.0, 0.11), (0.06, 0.105), (0.16, 0.075), (0.22, 0.05), (0.24, 0.04)], dress, segments=12)
b.lathe([(0.15, 0.078), (0.17, 0.078)], ribbon, segments=12)
b.cylinder((0, 0, 0.255), 0.022, 0.04, skin, segments=8)                           # neck
b.sphere((0, 0, 0.32), 0.065, skin, segments=12, rings=8)                         # head
b.sphere((0, 0.012, 0.335), 0.069, hair, segments=12, rings=8, scale=(1.0, 1.0, 0.85))  # hair cap (back)
for side in (-1, 1):
    b.box((side * 0.06, 0.01, 0.25), (0.03, 0.03, 0.17), hair, rotation=(0, side * 8, 0))   # long hair
    b.sphere((side * 0.024, -0.058, 0.325), 0.011, eye, segments=6, rings=4)               # eyes
    b.cylinder((side * 0.09, 0, 0.17), 0.016, 0.13, skin, segments=6, rotation=(0, side * 25, 0))  # arms
    b.cylinder((side * 0.035, 0, -0.03), 0.018, 0.08, skin, segments=6)                     # legs
muneca, _ = b.finish()
for v in muneca.data.vertices:
    v.co.z += 0.07  # legs start at the floor

# ---- Moneda del pacto (Ø 0.16 m): old gold with the cult's eight-pointed star, standing up.
gold = lp.make_material("Moneda_Oro", "#B08A3A", roughness=0.35, metallic=0.9, emission_strength=0.15)
star_red = lp.make_material("Moneda_Estrella", "#6A0E0E", roughness=0.5, emission_strength=0.5)
b = lp.PropBuilder("Moneda")
b.cylinder((0, 0, 0.08), 0.08, 0.018, gold, segments=24, rotation=(90, 0, 0))
b.cylinder((0, 0, 0.08), 0.084, 0.010, gold, segments=24, rotation=(90, 0, 0))   # raised rim
up = Matrix.Translation((0, 0, 0.08)) @ Matrix.Rotation(math.radians(90), 4, "X")
star = lp.star_outline(8, 0.055, inner_ratio=0.45, rotation=math.pi / 8)
b.prism(star, 0.009, 0.013, star_red, transform=up)     # front
b.prism(star, -0.013, -0.009, star_red, transform=up)   # back
moneda, _ = b.finish()

# ---- Fragmento de obsidiana (~0.32 m): a black volcanic shard with red glowing veins.
obsidian = lp.make_material("Obsidiana", "#0C0A10", roughness=0.08, metallic=0.2)
vein = lp.make_material("Obsidiana_Veta", "#FF2A10", roughness=0.4, emission_strength=4.0)
b = lp.PropBuilder("Obsidiana")
points = [(0, 0, 0.32), (0.015, -0.01, 0.30)]
for i in range(9):
    a = 2 * math.pi * i / 9 + random.uniform(-0.2, 0.2)
    r = random.uniform(0.05, 0.085)
    points.append((r * math.cos(a), r * math.sin(a) * 0.6, random.uniform(0.0, 0.05)))
    points.append((r * 0.7 * math.cos(a + 0.3), r * 0.7 * math.sin(a + 0.3) * 0.6, random.uniform(0.12, 0.2)))
b.hull(points, obsidian)
b.box((0.0, -0.03, 0.15), (0.012, 0.02, 0.22), vein, rotation=(0, 12, 0))
b.box((0.03, -0.022, 0.09), (0.01, 0.02, 0.12), vein, rotation=(0, -25, 0))
b.box((-0.025, 0.028, 0.12), (0.01, 0.02, 0.16), vein, rotation=(0, 18, 0))
obsidiana, _ = b.finish()

for obj, name in ((muneca, "obj_muneca.fbx"), (moneda, "obj_moneda.fbx"), (obsidiana, "obj_obsidiana.fbx")):
    lp.export_static(obj, name)
lp.render_preview([muneca, moneda, obsidiana], "objetos_clave.png", resolution=(1200, 600))
