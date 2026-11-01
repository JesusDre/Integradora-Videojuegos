"""The possessed doll (issue #172): an enemy of Xochimilco that only moves while the Padre isn't looking.
It's bigger than the offering doll (0.9 m, a small child's height) so it reads as a threat, with its arms
reaching forward, a cracked porcelain face, black sockets (the red eyes are added in Unity, so they can
glow) and a torn dress. No skeleton: it never animates, it just isn't where it was a moment ago.

Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/ent_muneca.py
Outputs Characters/ent_muneca_poseida.fbx and docs/img/props/muneca_poseida.png.
Axes as in the other models: X right, -Y is the face (Unity's forward after import), Z up; feet at 0.
"""

import math
import os
import sys

sys.path.append(os.path.dirname(__file__))

import lowpoly_props as lp  # noqa: E402

lp.reset_scene()
OUT = os.path.join(lp.REPO_ROOT, "Assets", "_Project", "Art", "Models", "Characters")

skin = lp.make_material("Poseida_Porcelana", "#D2C4AC", roughness=0.55)
grime = lp.make_material("Poseida_Mugre", "#4A3E30", roughness=1.0)
dress = lp.make_material("Poseida_Vestido", "#7E6E66", roughness=0.95)
lace = lp.make_material("Poseida_Encaje", "#B8AE9C", roughness=0.9)
socket = lp.make_material("Poseida_Cuenca", "#030202", roughness=1.0)
blood = lp.make_material("Poseida_Sangre", "#4A0606", roughness=0.7, emission_strength=0.3)
hair = lp.make_material("Poseida_Pelo", "#120D0A", roughness=1.0)

b = lp.PropBuilder("MunecaPoseida")
# Dress: a bell from the knees to the chest, a lace hem, torn strips hanging.
b.lathe([(0.18, 0.24), (0.25, 0.25), (0.45, 0.17), (0.6, 0.12), (0.64, 0.09)], dress, segments=10)
b.lathe([(0.16, 0.245), (0.2, 0.255)], lace, segments=10)
for k in range(5):
    a = k * 2 * math.pi / 5 + 0.3
    b.box((math.cos(a) * 0.22, math.sin(a) * 0.22, 0.13), (0.06, 0.02, 0.12), dress, rotation=(0, 0, math.degrees(a)))
# Legs and dirty feet.
for side in (-1, 1):
    b.cylinder((side * 0.07, 0, 0.1), 0.035, 0.2, skin, segments=7)
    b.sphere((side * 0.07, -0.02, 0.02), 0.045, grime, segments=7, rings=5, scale=(1, 1.4, 0.6))
# Arms reaching forward, a little uneven (one higher than the other).
for side, up in ((-1, 6), (1, -4)):
    b.cylinder((side * 0.15, -0.16, 0.56 + up * 0.004), 0.03, 0.34, skin, segments=7, rotation=(90 + up, 0, side * 10))
    b.sphere((side * 0.17, -0.33, 0.56 + up * 0.008), 0.035, skin, segments=7, rings=5)
# Neck and head, tilted to one side.
b.cylinder((0, 0, 0.68), 0.04, 0.08, skin, segments=8)
head = (0.03, 0, 0.8)
b.sphere(head, 0.13, skin, segments=12, rings=9, scale=(1, 0.95, 1.05))
b.sphere((0.03, 0.03, 0.84), 0.138, hair, segments=12, rings=9, scale=(1.02, 1.0, 0.85))
for side in (-1, 1):
    b.box((0.03 + side * 0.12, 0.02, 0.7), (0.05, 0.05, 0.26), hair)                         # matted locks
    b.sphere((0.03 + side * 0.048, -0.11, 0.81), 0.032, socket, segments=8, rings=5)          # black sockets
    b.box((0.03 + side * 0.048, -0.122, 0.74), (0.01, 0.006, 0.1), blood)                     # tears of blood
# A wide crack across the face and a thin, too-wide mouth.
b.box((0.0, -0.126, 0.86), (0.07, 0.004, 0.006), grime, rotation=(0, 25, 0))
b.box((0.06, -0.124, 0.83), (0.006, 0.004, 0.06), grime)
b.box((0.03, -0.125, 0.75), (0.09, 0.004, 0.01), socket)

obj, _ = b.finish()
lp.PROPS_DIR = OUT
lp.export_static(obj, "ent_muneca_poseida.fbx")
lp.render_preview([obj], "muneca_poseida.png", resolution=(700, 700))
