"""Cultista (Hijos de la Estrella Negra): low-poly model, rig and animations.

Design: docs/personajes/cultista.md. Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/chr_cultista.py
Outputs Assets/_Project/Art/Models/Characters/chr_cultista.fbx and the front/side sheet renders.
"""

import os
import sys

sys.path.append(os.path.dirname(__file__))

import lowpoly_humanoid as lp  # noqa: E402

lp.reset_scene()

# Palette from the character sheet.
robe = lp.make_material("Cultista_Tunica", "#141418", roughness=0.95)
hood_inside = lp.make_material("Cultista_InteriorCapucha", "#050505", roughness=1.0)
rope = lp.make_material("Cultista_Cuerda", "#3B2A1E", roughness=0.9)
star = lp.make_material("Cultista_Estrella", "#7A1010", roughness=0.6)
mask = lp.make_material("Cultista_Mascara", "#D8D0BC", roughness=0.55)
eyes = lp.make_material("Cultista_Ojos", "#FF2A10", roughness=0.4, emission_strength=4.0)
skin = lp.make_material("Cultista_Piel", "#6E5A4E", roughness=0.8)
sandal = lp.make_material("Cultista_Sandalias", "#2E2219", roughness=0.9)
blade = lp.make_material("Cultista_Hoja", "#8A8C8E", roughness=0.45, metallic=0.8)
handle = lp.make_material("Cultista_Mango", "#3A2616", roughness=0.8)

bones = lp.humanoid_bones()
rig = lp.build_armature("Cultista", bones)

b = lp.PartBuilder()
DEPTH = 0.85  # bodies are a bit shallower front-to-back than wide

# Robe skirt: pleated cloth from the hem to the waist (follows the hips).
b.lathe([(0.10, 0.34), (0.35, 0.32), (0.65, 0.27), (0.90, 0.22), (1.00, 0.205)], robe, "Hips",
        segments=20, folds=8, fold_depth=0.07, scale_y=DEPTH)
# Torso under the cape.
b.lathe([(1.00, 0.20), (1.20, 0.21)], robe, "Spine", segments=16, scale_y=0.8)
b.lathe([(1.20, 0.21), (1.36, 0.215), (1.46, 0.12)], robe, "Chest", segments=16, scale_y=0.8, cap_top=True)
# Rope belt with two hanging ends tied at the front.
b.lathe([(0.98, 0.212), (1.00, 0.224), (1.02, 0.212)], rope, "Hips", segments=20, scale_y=DEPTH)
for x in (-0.03, 0.025):
    b.tube((x, -0.19, 0.99), (x * 1.6, -0.21, 0.72), 0.011, 0.009, rope, "Hips", segments=5)
b.star((0, -0.234, 1.33), 0.06, 8, star, "Chest", facing=(0, -1, 0.35))  # on the cape, visible from the front

# Cape (esclavina): short pleated capelet over the shoulders, under the hood.
b.lathe([(1.24, 0.30), (1.30, 0.29), (1.40, 0.24), (1.46, 0.15), (1.50, 0.10)], robe, "Chest",
        segments=20, folds=10, fold_depth=0.05, scale_y=DEPTH)

# Hood: rounded cloth around the head with a deep opening for the face. A dark lining inside
# hides the back of the hood when looking into the opening.
hood_profile = [(1.42, 0.17), (1.52, 0.165), (1.62, 0.16), (1.70, 0.14), (1.77, 0.10), (1.81, 0.05), (1.82, 0.0)]
opening = lambda c, n: c.y < -0.05 and 1.49 < c.z < 1.73  # noqa: E731
hood = b.lathe(hood_profile, robe, "Head", center=(0, 0.025), segments=16, scale_y=1.1)
b.delete_faces(hood, opening)
lining = b.lathe([(z, max(r - 0.012, 0.0)) for z, r in hood_profile], hood_inside, "Head",
                 center=(0, 0.025), segments=16, scale_y=1.1, inward=True)
b.delete_faces(lining, opening)
b.sphere((0, 0.03, 1.60), 0.10, hood_inside, "Head", segments=10, rings=6)

# Ritual mask: pale bone face with the cult's star painted on the forehead and glowing eyes.
b.sphere((0, -0.06, 1.60), 1.0, mask, "Head", segments=12, rings=8, scale=(0.08, 0.045, 0.105))
for x in (-0.03, 0.03):
    b.sphere((x, -0.101, 1.625), 0.017, hood_inside, "Head", segments=8, rings=4, scale=(1.0, 0.35, 0.8))
    b.sphere((x, -0.106, 1.625), 0.006, eyes, "Head", segments=6, rings=4)
b.star((0, -0.095, 1.672), 0.022, 8, star, "Head", depth=0.003, facing=(0, -1, 0.6))

# Sleeves (wider at the wrist) and hands with a thumb.
for side, sign in (("Left", 1), ("Right", -1)):
    b.tube((sign * 0.16, 0, 1.42), (sign * 0.48, 0, 1.42), 0.08, 0.072, robe, f"{side}UpperArm", segments=10)
    b.tube((sign * 0.48, 0, 1.42), (sign * 0.78, 0, 1.42), 0.072, 0.10, robe, f"{side}LowerArm", segments=10, cap=False)
    b.box((sign * 0.83, 0, 1.42), (0.10, 0.04, 0.085), skin, f"{side}Hand")
    b.box((sign * 0.80, -0.03, 1.435), (0.05, 0.025, 0.025), skin, f"{side}Hand")
    # Legs only show at the ankles; the rest stays under the robe.
    b.tube((sign * 0.10, 0, 0.92), (sign * 0.10, 0, 0.50), 0.075, 0.06, robe, f"{side}UpperLeg", segments=8)
    b.tube((sign * 0.10, 0, 0.50), (sign * 0.10, 0, 0.07), 0.06, 0.045, skin, f"{side}LowerLeg", segments=8)
    b.box((sign * 0.10, -0.05, 0.03), (0.095, 0.25, 0.05), sandal, f"{side}Foot")

# Machete in the right hand: wooden handle in the fist, small guard and a blade that widens to a
# curved tip, drawn in profile and extruded.
b.tube((-0.83, 0.05, 1.42), (-0.83, -0.08, 1.42), 0.016, 0.016, handle, "RightHand", segments=6)
b.box((-0.83, -0.085, 1.42), (0.02, 0.012, 0.05), handle, "RightHand")
b.prism([(-0.09, 1.44), (-0.40, 1.448), (-0.50, 1.44), (-0.53, 1.415), (-0.50, 1.385), (-0.09, 1.395)],
        -0.83, 0.006, blade, "RightHand")

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
