"""The chaneque (issue #173): a small mischievous forest spirit of Mexican legend, 0.75 m tall, with a
big head, pointed ears, a frayed palm hat, ragged clothes and long thin arms. Its yellow eyes are added
in Unity so they can glow. No skeleton: it moves with a procedural hop (ChanequeAI).

Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/ent_chaneque.py
Outputs Characters/ent_chaneque.fbx and docs/img/props/chaneque.png.
Axes as in the other models: X right, -Y is the face, Z up; feet at 0.
"""

import math
import os
import sys

sys.path.append(os.path.dirname(__file__))

import lowpoly_props as lp  # noqa: E402

lp.reset_scene()
OUT = os.path.join(lp.REPO_ROOT, "Assets", "_Project", "Art", "Models", "Characters")

skin = lp.make_material("Chaneque_Piel", "#6B5A3A", roughness=0.9)
skin_dark = lp.make_material("Chaneque_PielOscura", "#4A3D28", roughness=0.9)
cloth = lp.make_material("Chaneque_Manta", "#8C7F62", roughness=1.0)
sash = lp.make_material("Chaneque_Faja", "#6E1E14", roughness=0.9)
palm = lp.make_material("Chaneque_Palma", "#A88A4C", roughness=1.0)
socket = lp.make_material("Chaneque_Cuenca", "#060404", roughness=1.0)
teeth = lp.make_material("Chaneque_Dientes", "#D8CCAA", roughness=0.7)

b = lp.PropBuilder("Chaneque")
# Short bowed legs and big bare feet.
for side in (-1, 1):
    b.cylinder((side * 0.07, 0, 0.12), 0.035, 0.24, skin, segments=6, rotation=(0, side * -8, 0))
    b.box((side * 0.075, -0.04, 0.02), (0.08, 0.16, 0.04), skin_dark)
# Ragged shirt and trousers (a bell that ends in torn strips) with a red sash.
b.lathe([(0.2, 0.12), (0.3, 0.14), (0.42, 0.12), (0.5, 0.09)], cloth, segments=8)
b.lathe([(0.3, 0.145), (0.34, 0.145)], sash, segments=8)
for k in range(6):
    a = k * 2 * math.pi / 6
    b.box((math.cos(a) * 0.12, math.sin(a) * 0.12, 0.18), (0.05, 0.015, 0.07), cloth, rotation=(0, 0, math.degrees(a)))
# Long thin arms hanging low, with clawed hands.
for side in (-1, 1):
    # Shoulders close to the body, hands out to the sides (the arm leans outward going down).
    b.cylinder((side * 0.17, -0.02, 0.33), 0.025, 0.3, skin, segments=6, rotation=(0, -side * 18, 0))
    b.sphere((side * 0.22, -0.03, 0.17), 0.035, skin_dark, segments=6, rings=4)
    for f in (-1, 0, 1):
        b.cylinder((side * 0.22 + f * 0.015, -0.05, 0.13), 0.006, 0.05, skin_dark, segments=4, radius_top=0.002)
# A big head with pointed ears, a long nose and a wide grin full of teeth.
head = (0, 0, 0.62)
b.sphere(head, 0.13, skin, segments=11, rings=8, scale=(1.05, 1.0, 0.95))
for side in (-1, 1):
    b.cylinder((side * 0.19, 0.01, 0.69), 0.035, 0.18, skin, segments=5, radius_top=0.004, rotation=(0, side * 65, 0))  # pointed ear
    b.sphere((side * 0.05, -0.105, 0.65), 0.03, socket, segments=7, rings=5)           # sockets (eyes glow in Unity)
b.cylinder((0, -0.15, 0.6), 0.022, 0.08, skin_dark, segments=6, rotation=(80, 0, 0), radius_top=0.008)  # nose
b.box((0, -0.115, 0.55), (0.12, 0.02, 0.025), socket)                                    # grin
for i in range(6):
    b.box((-0.05 + i * 0.02, -0.127, 0.553), (0.008, 0.006, 0.014), teeth)
# A frayed palm hat, a bit too big.
b.cylinder((0, 0, 0.73), 0.24, 0.02, palm, segments=14)
b.cylinder((0, 0, 0.79), 0.11, 0.11, palm, segments=10, radius_top=0.08)
for k in range(10):
    a = k * 2 * math.pi / 10
    b.box((math.cos(a) * 0.25, math.sin(a) * 0.25, 0.725), (0.05, 0.012, 0.01), palm, rotation=(0, 0, math.degrees(a)))

obj, _ = b.finish()
lp.PROPS_DIR = OUT
lp.export_static(obj, "ent_chaneque.fbx")
lp.render_preview([obj], "chaneque.png", resolution=(700, 700))
