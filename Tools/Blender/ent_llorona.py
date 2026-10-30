"""La Llorona: low-poly model, rig and animations.

Design: docs/personajes/llorona.md. Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/ent_llorona.py
Outputs Assets/_Project/Art/Models/Characters/ent_llorona.fbx and the front/side sheet renders.
"""

import os
import sys

sys.path.append(os.path.dirname(__file__))
import lowpoly_humanoid as lp  # noqa: E402

lp.reset_scene()

# Palette from the character sheet.
dress = lp.make_material("Llorona_Vestido", "#D9DCD8", roughness=0.35)
dress_wet = lp.make_material("Llorona_VestidoBorde", "#7F8B86", roughness=0.3)
hair = lp.make_material("Llorona_Cabello", "#0B0B0D", roughness=0.25)
skin = lp.make_material("Llorona_Piel", "#AEB6BA", roughness=0.7)

# Longer arms than a person: that's what makes her unsettling. She is scaled up after modeling (see K).
bones = lp.humanoid_bones(hips=1.00, chest_top=1.44, shoulder_z=1.48, head_top=1.84,
                          shoulder_x=0.17, elbow_x=0.47, wrist_x=0.77, hand_x=0.92, leg_x=0.09)
rig = lp.build_armature("Llorona", bones)

b = lp.PartBuilder()
DEPTH = 0.8

# Long dress that touches the floor (no feet show: she seems to glide). The hem is a darker,
# wet band; the skirt is pleated so the cloth reads in the flashlight.
b.lathe([(0.0, 0.40), (0.08, 0.39)], dress_wet, "Hips", segments=22, folds=11, fold_depth=0.09, scale_y=DEPTH)
b.lathe([(0.08, 0.39), (0.35, 0.35), (0.70, 0.27), (0.95, 0.17), (1.03, 0.15)], dress, "Hips",
        segments=22, folds=11, fold_depth=0.09, scale_y=DEPTH)
b.lathe([(1.03, 0.15), (1.24, 0.155)], dress, "Spine", segments=14, scale_y=0.75)
b.lathe([(1.24, 0.155), (1.40, 0.165), (1.48, 0.09)], dress, "Chest", segments=14, scale_y=0.75, cap_top=True)

# Neck and head (the face is never seen: the hair covers it).
b.tube((0, 0.01, 1.45), (0, 0.01, 1.58), 0.038, 0.036, skin, "Neck", segments=8)
b.sphere((0, 0.0, 1.67), 1.0, skin, "Head", segments=12, rings=8, scale=(0.085, 0.095, 0.11))

# Hair: a cap on the head, long wet locks over the face and the sides that follow the head, and longer
# locks down the chest and back that follow the chest. Splitting it like this keeps the hair out of the
# dress when she bows her head; the chest locks start a little higher and wider, so the seam is hidden.
# Folds are shallow: deep folds would dip the strands into the body.
b.lathe([(1.58, 0.105), (1.66, 0.112), (1.74, 0.095), (1.79, 0.055), (1.81, 0.0)], hair, "Head",
        segments=14, scale_y=1.1)
b.lathe([(1.50, 0.13), (1.60, 0.122), (1.72, 0.102)], hair, "Head",
        segments=10, arc=(205, 335), folds=12, fold_depth=0.07, scale_y=1.15)       # over the face
b.lathe([(1.48, 0.135), (1.62, 0.122), (1.74, 0.102)], hair, "Head",
        segments=12, arc=(25, 155), folds=14, fold_depth=0.07, scale_y=1.15)        # back of the head
b.lathe([(1.50, 0.13), (1.62, 0.12), (1.72, 0.10)], hair, "Head",
        segments=4, arc=(150, 215), folds=6, fold_depth=0.06, scale_y=1.15)          # left side, above the shoulder
b.lathe([(1.50, 0.13), (1.62, 0.12), (1.72, 0.10)], hair, "Head",
        segments=4, arc=(325, 390), folds=6, fold_depth=0.06, scale_y=1.15)          # right side
b.lathe([(1.10, 0.19), (1.30, 0.17), (1.46, 0.155), (1.56, 0.145)], hair, "Chest",
        segments=10, arc=(215, 325), folds=12, fold_depth=0.06, scale_y=1.15)       # down the chest
b.lathe([(1.00, 0.20), (1.20, 0.18), (1.40, 0.16), (1.55, 0.148)], hair, "Chest",
        segments=12, arc=(30, 150), folds=14, fold_depth=0.06, scale_y=1.15)        # down the back

# Thin, long arms with torn short sleeves; long fingers.
for side, sign in (("Left", 1), ("Right", -1)):
    # The sleeve follows the upper arm, so it comes down with it.
    b.tube((sign * 0.17, 0, 1.48), (sign * 0.31, 0, 1.48), 0.05, 0.045, dress, f"{side}UpperArm", segments=8, cap=False)
    b.tube((sign * 0.17, 0, 1.48), (sign * 0.47, 0, 1.48), 0.034, 0.029, skin, f"{side}UpperArm", segments=8)
    b.tube((sign * 0.47, 0, 1.48), (sign * 0.77, 0, 1.48), 0.029, 0.022, skin, f"{side}LowerArm", segments=8)
    b.box((sign * 0.80, 0, 1.48), (0.07, 0.022, 0.055), skin, f"{side}Hand")
    for dz in (-0.021, -0.007, 0.007, 0.021):
        b.box((sign * 0.89, 0, 1.48 + dz), (0.12, 0.011, 0.011), skin, f"{side}Hand")
    b.box((sign * 0.80, -0.03, 1.48), (0.06, 0.012, 0.012), skin, f"{side}Hand")
    # Legs exist for the Humanoid rig but stay hidden inside the dress.
    b.tube((sign * 0.09, 0, 0.97), (sign * 0.09, 0, 0.52), 0.05, 0.04, dress, f"{side}UpperLeg", segments=6)
    b.tube((sign * 0.09, 0, 0.52), (sign * 0.09, 0, 0.10), 0.04, 0.03, dress, f"{side}LowerLeg", segments=6)
    b.box((sign * 0.09, -0.04, 0.05), (0.06, 0.16, 0.05), dress, f"{side}Foot")

body, triangles = b.finish("Llorona", rig)

# She is modeled at 1.85 m and then scaled to 2.20 m, so she towers over the Padre (1.80 m).
# Hip offsets in the animations below are multiplied by K to match.
HEIGHT = 2.20
K = HEIGHT / 1.85
lp.scale_character(rig, [body], K)
lp.render_sheet(HEIGHT, "ent_llorona")

# ---------------------------------------------------------------- animations
anim = lp.Animator(rig)


def merge(*dicts):
    out = {}
    for d in dicts:
        for k, v in d.items():
            x, y, z = out.get(k, (0, 0, 0))
            out[k] = (x + v[0], y + v[1], z + v[2])
    return out


# Hunched, head hanging: her base posture. Positive X leans the upper body forward.
hunch = {"Spine": (12, 0, 0), "Chest": (8, 0, 0), "Neck": (6, 0, 0), "Head": (8, 0, 0)}
hang = lp.arms_down(spread=6, forward=-6)

# Idle (3 s): standing still, sobbing: shoulders jerk up twice and the head shakes a little.
anim.new_clip("Idle", 90, loop=True)
for f, sob in ((1, 0), (12, 1), (20, 0), (30, 1), (40, 0), (90, 0)):
    shrug = 6 * sob
    anim.pose(f, merge(hang, hunch, {
        "LeftShoulder": (0, -shrug, 0), "RightShoulder": (0, shrug, 0),
        "Chest": (-3 * sob, 0, 0), "Head": (4 * sob, 0, 3 * sob),
    }))

# Stalk (2 s): slow glide, more hunched, the body sways side to side; the legs barely move.
anim.new_clip("Stalk", 60, loop=True)
for f, s in ((1, 0), (15, 1), (30, 0), (45, -1), (60, 0)):
    anim.pose(f, merge(lp.arms_down(spread=4, forward=-12), hunch, {
        "Spine": (6, 3 * s, 0), "Head": (5, -4 * s, 6 * s),
        "LeftUpperLeg": (-6 * s, 0, 0), "RightUpperLeg": (6 * s, 0, 0),
    }), {"Hips": (0, 0, K * 0.015 * abs(s))})

# Walk (1.2 s): coming for the player, leaning in with both arms reaching forward.
anim.new_clip("Walk", 36, loop=True)
reach = {"LeftUpperArm": (0.18, -0.82, -0.45), "RightUpperArm": (-0.18, -0.82, -0.45),
         "LeftLowerArm": (0.08, -0.95, -0.2), "RightLowerArm": (-0.08, -0.95, -0.2)}
for f, s in ((1, 0), (10, 1), (19, 0), (28, -1), (36, 0)):
    anim.pose(f, merge(hunch, {
        "Spine": (18, 0, 4 * s), "Head": (-10, 0, -4 * s),
        "LeftUpperLeg": (-12 * s, 0, 0), "RightUpperLeg": (12 * s, 0, 0),
        "LeftLowerLeg": (6, 0, 0), "RightLowerLeg": (6, 0, 0),
    }), {"Hips": (0, 0, K * -0.02 * abs(s))}, aims=reach)

# Grab (1 s): opens both arms wide in front, then closes them around the player and holds.
anim.new_clip("Grab", 30, loop=False)
lean = merge(hunch, {"Spine": (20, 0, 0), "Head": (-12, 0, 0)})
anim.pose(1, lean, aims=reach)
anim.pose(12, lean, aims={"LeftUpperArm": (0.6, -0.75, 0.15), "RightUpperArm": (-0.6, -0.75, 0.15),
                          "LeftLowerArm": (0.3, -0.92, 0.15), "RightLowerArm": (-0.3, -0.92, 0.15)})
anim.pose(20, merge(lean, {"Spine": (8, 0, 0)}),
          aims={"LeftUpperArm": (0.12, -0.98, 0.0), "RightUpperArm": (-0.12, -0.98, 0.0),
                "LeftLowerArm": (-0.4, -0.9, 0.0), "RightLowerArm": (0.4, -0.9, 0.0)})
anim.pose(30, merge(lean, {"Spine": (6, 0, 0)}),
          aims={"LeftUpperArm": (0.12, -0.98, -0.05), "RightUpperArm": (-0.12, -0.98, -0.05),
                "LeftLowerArm": (-0.42, -0.9, -0.05), "RightLowerArm": (0.42, -0.9, -0.05)})

# Vanish (1.5 s): folds in on herself and sinks into the ground. The fade-out is done by the
# material in code (#35); the animation only gives the shape.
anim.new_clip("Vanish", 45, loop=False)
anim.pose(1, merge(hang, hunch))
anim.pose(15, merge(lp.arms_down(spread=2, forward=-25, elbow=40), hunch, {"Spine": (25, 0, 0), "Head": (25, 0, 0)}),
          {"Hips": (0, 0, K * -0.15)})
anim.pose(45, merge(lp.arms_down(spread=2, forward=-30, elbow=60), hunch, {"Spine": (35, 0, 0), "Head": (30, 0, 0)}),
          {"Hips": (0, 0, K * -0.85)})

lp.export_fbx(rig, [body], "ent_llorona.fbx")
print(f"[llorona] done, {triangles} triangles")
