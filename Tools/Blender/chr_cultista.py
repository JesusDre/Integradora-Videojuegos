"""Cultista (Hijos de la Estrella Negra): low-poly model, rig and animations.

Design: docs/personajes/cultista.md. Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/chr_cultista.py
Outputs Assets/_Project/Art/Models/Characters/chr_cultista.fbx and the front/side sheet renders.
"""

import math
import os
import sys

sys.path.append(os.path.dirname(__file__))
from mathutils import Quaternion  # noqa: E402

import lowpoly_humanoid as lp  # noqa: E402

lp.reset_scene()

# Palette from the character sheet.
robe = lp.make_material("Cultista_Tunica", "#141418", roughness=0.95)
hood_inside = lp.make_material("Cultista_InteriorCapucha", "#050505", roughness=1.0)
rope = lp.make_material("Cultista_Cuerda", "#3B2A1E", roughness=0.9)
star = lp.make_material("Cultista_Estrella", "#7A1010", roughness=0.6)
eyes = lp.make_material("Cultista_Ojos", "#FF2A10", roughness=0.4, emission_strength=4.0)
skin = lp.make_material("Cultista_Piel", "#6E5A4E", roughness=0.8)
sandal = lp.make_material("Cultista_Sandalias", "#2E2219", roughness=0.9)
blade = lp.make_material("Cultista_Hoja", "#8A8C8E", roughness=0.45, metallic=0.8)
handle = lp.make_material("Cultista_Mango", "#3A2616", roughness=0.8)

bones = lp.humanoid_bones()
rig = lp.build_armature("Cultista", bones)

b = lp.PartBuilder()

# Robe: a tapered skirt that follows the hips and a torso that follows spine and chest.
b.tube((0, 0, 0.12), (0, 0, 1.00), 0.36, 0.21, robe, "Hips", segments=12, cap=False)
b.tube((0, 0, 1.00), (0, 0, 1.20), 0.21, 0.22, robe, "Spine", segments=12, cap=False)
b.tube((0, 0, 1.20), (0, 0, 1.44), 0.22, 0.12, robe, "Chest", segments=12)
b.torus((0, 0, 1.00), 0.215, 0.018, rope, "Hips")
b.star((0, -0.228, 1.30), 0.07, 8, star, "Chest")

# Hood: a pointed cone with the front opened, so the face is a dark void with two glowing eyes.
hood = b.tube((0, 0.01, 1.38), (0, 0.02, 1.68), 0.17, 0.15, robe, "Head", segments=12, cap=False)
b.delete_faces(hood, lambda c, n: n.y < -0.55 and 1.48 < c.z < 1.70)
b.tube((0, 0.02, 1.68), (0, 0.05, 1.84), 0.15, 0.0, robe, "Head", segments=12, cap=False)  # tip leans back a little
b.sphere((0, 0.03, 1.60), 0.105, hood_inside, "Head", segments=10, rings=6)
for x in (-0.035, 0.035):
    b.sphere((x, -0.07, 1.615), 0.012, eyes, "Head", segments=6, rings=4)

# Sleeves (wide at the wrist) and hands.
for side, sign in (("Left", 1), ("Right", -1)):
    b.tube((sign * 0.16, 0, 1.42), (sign * 0.48, 0, 1.42), 0.08, 0.07, robe, f"{side}UpperArm", segments=8)
    b.tube((sign * 0.48, 0, 1.42), (sign * 0.78, 0, 1.42), 0.07, 0.095, robe, f"{side}LowerArm", segments=8, cap=False)
    b.box((sign * 0.83, 0, 1.42), (0.10, 0.04, 0.085), skin, f"{side}Hand")
    # Legs only show at the ankles; the rest stays under the robe.
    b.tube((sign * 0.10, 0, 0.92), (sign * 0.10, 0, 0.50), 0.075, 0.06, robe, f"{side}UpperLeg", segments=8)
    b.tube((sign * 0.10, 0, 0.50), (sign * 0.10, 0, 0.07), 0.06, 0.045, skin, f"{side}LowerLeg", segments=8)
    b.box((sign * 0.10, -0.05, 0.03), (0.095, 0.25, 0.05), sandal, f"{side}Foot")

# Machete in the right hand: handle in the fist, blade pointing forward and a little down.
tilt = Quaternion((1, 0, 0), math.radians(-10))
b.tube((-0.83, 0.05, 1.42), (-0.83, -0.08, 1.42), 0.016, 0.016, handle, "RightHand", segments=6)
b.box((-0.83, -0.29, 1.40), (0.008, 0.42, 0.055), blade, "RightHand", rotation=tilt)

body, triangles = b.finish("Cultista", rig)

# Character sheet renders in T-pose (rest pose, before any action is assigned).
lp.render_sheet(1.80, "chr_cultista")

# ---------------------------------------------------------------- animations
anim = lp.Animator(rig)
down = lp.arms_down(spread=10, elbow=15)


def merge(*dicts):
    out = {}
    for d in dicts:
        for k, v in d.items():
            x, y, z = out.get(k, (0, 0, 0))
            out[k] = (x + v[0], y + v[1], z + v[2])
    return out


# Idle (2 s): slow breathing and a small head sway.
anim.new_clip("Idle", 60, loop=True)
for f, breath, head in ((1, 0, 0), (30, 2.5, 4), (60, 0, 0)):
    anim.pose(f, merge(down, {"Chest": (-breath, 0, 0), "Head": (breath, 0, head)}))

# Walk (1 s): short steps under the robe, arms almost still, slight hip bob.
# Positive X rotation swings a hanging limb backward, negative swings it forward.
anim.new_clip("Walk", 30, loop=True)
for f, s in ((1, 0), (8, 1), (15, 0), (22, -1), (30, 0)):
    anim.pose(f, merge(down, {
        "LeftUpperLeg": (-18 * s, 0, 0), "RightUpperLeg": (18 * s, 0, 0),
        "LeftLowerLeg": (8 + (14 if s < 0 else 0), 0, 0), "RightLowerLeg": (8 + (14 if s > 0 else 0), 0, 0),
        "LeftUpperArm": (8 * s, 0, 0), "RightUpperArm": (-8 * s, 0, 0),
        "Spine": (3, 0, 4 * s), "Head": (-2, 0, -3 * s),
    }), {"Hips": (0, 0, -0.02 * abs(s))})

# Run (0.66 s): longer strides (kept short enough not to break through the robe), torso leaning
# forward, arms pumping with bent elbows.
anim.new_clip("Run", 20, loop=True)
for f, s in ((1, 0), (6, 1), (11, 0), (16, -1), (20, 0)):
    anim.pose(f, merge(lp.arms_down(spread=15, elbow=35), {
        "LeftUpperLeg": (-20 * s, 0, 0), "RightUpperLeg": (20 * s, 0, 0),
        "LeftLowerLeg": (15 + (25 if s < 0 else 0), 0, 0), "RightLowerLeg": (15 + (25 if s > 0 else 0), 0, 0),
        "LeftUpperArm": (22 * s, 0, 0), "RightUpperArm": (-22 * s, 0, 0),
        "Spine": (12, 0, 6 * s), "Head": (-8, 0, 0),
    }), {"Hips": (0, 0, -0.04 * abs(s))})

# Attack (0.9 s): wind up with the machete above and behind the head, then a diagonal slash
# forward and down across the body. Aims are world directions: -Y is forward, +X is the
# character's left, +Z is up.
anim.new_clip("Attack", 27, loop=False)
anim.pose(1, down)
anim.pose(9, merge(down, {"Spine": (-8, 0, -15), "Chest": (-5, 0, -10)}),
          aims={"RightUpperArm": (-0.35, 0.15, 0.92), "RightLowerArm": (-0.1, 0.75, 0.35)})
anim.pose(14, merge(down, {"Spine": (15, 0, 20), "Chest": (8, 0, 10)}),
          aims={"RightUpperArm": (0.15, -0.95, -0.1), "RightLowerArm": (0.45, -0.6, -0.65)})
anim.pose(20, merge(down, {"Spine": (10, 0, 12), "Chest": (4, 0, 5)}),
          aims={"RightUpperArm": (0.1, -0.6, -0.8), "RightLowerArm": (0.3, -0.4, -0.85)})
anim.pose(27, down)

# Death (1.4 s): buckles and falls on its back.
anim.new_clip("Death", 42, loop=False)
anim.pose(1, down)
anim.pose(10, merge(down, {"Spine": (20, 0, 0), "Head": (20, 0, 0),
                           "LeftUpperLeg": (-15, 0, 0), "RightUpperLeg": (-15, 0, 0),
                           "LeftLowerLeg": (40, 0, 0), "RightLowerLeg": (40, 0, 0)}),
          {"Hips": (0, 0, -0.25)})
anim.pose(30, merge(lp.arms_down(spread=60), {"Hips": (-85, 0, 0), "Head": (-10, 0, 25),
                                              "LeftUpperLeg": (60, 0, 0), "RightUpperLeg": (70, 0, 0)}),
          {"Hips": (0, 0.15, -0.80)})
anim.pose(42, merge(lp.arms_down(spread=65), {"Hips": (-88, 0, 0), "Head": (-15, 0, 30),
                                              "LeftUpperLeg": (65, 0, 0), "RightUpperLeg": (75, 0, 0)}),
          {"Hips": (0, 0.18, -0.82)})

lp.export_fbx(rig, [body], "chr_cultista.fbx")
print(f"[cultista] done, {triangles} triangles")
