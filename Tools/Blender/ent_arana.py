"""The ceiling spider (issue #174): a big spider of the ex-convent's vaults, about 1 m across with its
legs, a hairy dark body, a bulbous abdomen with a pale skull-like mark, fangs, and eight legs bent like
a crouching hand. Its eyes are added in Unity so they can glow. No skeleton: CeilingSpider moves it.

Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/ent_arana.py
Outputs Characters/ent_arana.fbx and docs/img/props/arana.png.
Axes: X right, -Y is the front (fangs), Z up; it is modeled standing on a floor at 0. In the game it
hangs upside down from the ceiling (rotated 180° in Unity).
"""

import math
import os
import sys

sys.path.append(os.path.dirname(__file__))

import lowpoly_props as lp  # noqa: E402

lp.reset_scene()
OUT = os.path.join(lp.REPO_ROOT, "Assets", "_Project", "Art", "Models", "Characters")

body = lp.make_material("Arana_Cuerpo", "#1C1714", roughness=0.95)
body_light = lp.make_material("Arana_Pelo", "#3A2E24", roughness=1.0)
mark = lp.make_material("Arana_Marca", "#B8A890", roughness=0.8)
fang = lp.make_material("Arana_Colmillo", "#5A1010", roughness=0.6)
joint = lp.make_material("Arana_Articulacion", "#4A1E14", roughness=0.9)

b = lp.PropBuilder("Arana")
# Head (cephalothorax) in front, abdomen behind and a little higher.
b.sphere((0, -0.08, 0.22), 0.14, body, segments=10, rings=7, scale=(1.0, 1.1, 0.8))
b.sphere((0, 0.2, 0.28), 0.22, body, segments=12, rings=8, scale=(1.0, 1.25, 0.9))
# The pale mark on the abdomen: two "eye" holes and a jaw, like a little skull.
b.sphere((0, 0.2, 0.47), 0.09, mark, segments=8, rings=5, scale=(1.0, 1.1, 0.25))
for side in (-1, 1):
    b.sphere((side * 0.035, 0.18, 0.49), 0.025, body, segments=6, rings=4, scale=(1, 1, 0.4))
# Tufts of hair on the abdomen.
for k in range(10):
    a = k * 2 * math.pi / 10
    b.box((math.cos(a) * 0.2, 0.2 + math.sin(a) * 0.25, 0.3), (0.02, 0.06, 0.02), body_light, rotation=(30, 0, math.degrees(a)))
# Fangs.
for side in (-1, 1):
    b.cylinder((side * 0.04, -0.22, 0.16), 0.022, 0.1, fang, segments=6, radius_top=0.004, rotation=(-30, 0, 0))
# Eight legs: up from the body to a knee, then down to the floor, spread like a crouching hand.
for side in (-1, 1):
    for i, ang in enumerate((-50, -18, 15, 48)):
        a = math.radians(ang)
        base = (side * 0.1, -0.05 + i * 0.05, 0.22)
        knee = (side * (0.1 + 0.28 * math.cos(a)), base[1] + 0.28 * math.sin(a), 0.42)
        foot = (side * (0.1 + 0.55 * math.cos(a)), base[1] + 0.55 * math.sin(a), 0.0)
        for p0, p1, r0, r1 in ((base, knee, 0.03, 0.022), (knee, foot, 0.022, 0.008)):
            d = [p1[k] - p0[k] for k in range(3)]
            length = math.sqrt(sum(x * x for x in d))
            c = [(p0[k] + p1[k]) / 2 for k in range(3)]
            rot = lp.Vector((0, 0, 1)).rotation_difference(lp.Vector(d).normalized()).to_euler("XYZ")
            b.cylinder(tuple(c), r0, length, body, segments=5, radius_top=r1, rotation=tuple(math.degrees(x) for x in rot))
        b.sphere(knee, 0.03, joint, segments=6, rings=4)

obj, _ = b.finish()
lp.PROPS_DIR = OUT
lp.export_static(obj, "ent_arana.fbx")
lp.render_preview([obj], "arana.png", resolution=(700, 700))
