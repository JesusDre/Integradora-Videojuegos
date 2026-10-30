"""La Llorona: low-poly model, rig and animations.

Design: docs/personajes/llorona.md. Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/ent_llorona.py
Outputs Assets/_Project/Art/Models/Characters/ent_llorona.fbx and the front/side sheet renders.
"""

import math
import os
import random
import sys

sys.path.append(os.path.dirname(__file__))
import lowpoly_humanoid as lp  # noqa: E402

lp.reset_scene()

# Palette from the character sheet.
dress = lp.make_material("Llorona_Vestido", "#D9DCD8", roughness=0.35)
dress_wet = lp.make_material("Llorona_VestidoBorde", "#7F8B86", roughness=0.3)
hair = lp.make_material("Llorona_Cabello", "#0B0B0D", roughness=0.25)
skin = lp.make_material("Llorona_Piel", "#AEB6BA", roughness=0.7)
hollow = lp.make_material("Llorona_Cuencas", "#0A0606", roughness=1.0)
eyes = lp.make_material("Llorona_Ojos", "#FF1A0A", roughness=0.3, emission_strength=6.0)

# Thin body with dropped shoulders and longer arms than a person: that's what makes her unsettling.
# She is scaled up after modeling (see K).
bones = lp.humanoid_bones(hips=1.00, chest_top=1.42, shoulder_z=1.46, head_top=1.84,
                          shoulder_x=0.15, elbow_x=0.46, wrist_x=0.77, hand_x=0.92, leg_x=0.08)
rig = lp.build_armature("Llorona", bones)

b = lp.PartBuilder()
rng = random.Random(7)  # fixed seed: the torn hem and the hair strands come out the same every run

# Long dress that falls loose to the floor (no feet show: she seems to glide). The hem is a darker,
# wet band torn into uneven strips.
hem = b.lathe([(0.0, 0.345), (0.12, 0.335)], dress_wet, "Hips", segments=24, folds=12, fold_depth=0.1, scale_y=0.8)
for v in {v for f in hem for v in f.verts}:
    if v.co.z < 0.01:
        v.co.z += rng.uniform(0.0, 0.11)       # ragged edge
        v.co.x *= 1.0 + rng.uniform(0.0, 0.06)  # strips hang a little loose
b.lathe([(0.12, 0.335), (0.40, 0.30), (0.70, 0.23), (0.93, 0.15), (1.03, 0.125)], dress, "Hips",
        segments=24, folds=12, fold_depth=0.1, scale_y=0.8)
b.lathe([(1.03, 0.125), (1.22, 0.13)], dress, "Spine", segments=14, scale_y=0.7)
b.lathe([(1.22, 0.13), (1.36, 0.14), (1.45, 0.075)], dress, "Chest", segments=14, scale_y=0.7, cap_top=True)

# Neck and a gaunt face: sunken dark sockets with glowing red eyes and an open mouth. The hair hides
# it most of the time; it shows between the strands, from below, or when she throws her head back.
b.tube((0, 0.01, 1.43), (0, 0.01, 1.58), 0.032, 0.030, skin, "Neck", segments=8)
b.sphere((0, 0.0, 1.67), 1.0, skin, "Head", segments=12, rings=8, scale=(0.08, 0.092, 0.112))
for x in (-0.03, 0.03):
    b.sphere((x, -0.078, 1.69), 0.024, hollow, "Head", segments=8, rings=5, scale=(1.0, 0.6, 0.9))
    b.sphere((x, -0.088, 1.688), 0.009, eyes, "Head", segments=6, rings=4)
b.sphere((0, -0.084, 1.605), 1.0, hollow, "Head", segments=8, rings=5, scale=(0.02, 0.012, 0.034))

# Hair: a cap on the head and long wet strands of different lengths, clumped like wet hair. Each strand
# has a top part that follows the head and a lower part that follows the chest (they overlap at the
# seam), and it hangs a little away from the body so it never cuts into the dress. The strands in front
# of the face leave gaps, so sometimes the face and the red eyes show through.
cap = b.lathe([(1.58, 0.100), (1.66, 0.106), (1.74, 0.09), (1.79, 0.052), (1.81, 0.0)], hair, "Head",
               segments=14, scale_y=1.1)
b.delete_faces(cap, lambda c, n: c.y < -0.04 and c.z < 1.76)  # the cap only covers the top and back


def strand(angle, end_z, thickness, front):
    a = math.radians(angle)
    out = (math.cos(a), math.sin(a) * 1.15)
    sway = rng.uniform(-0.015, 0.015)
    root = (out[0] * 0.09, out[1] * 0.09, 1.76)
    seam_head = (out[0] * 0.128, out[1] * 0.128, 1.50)
    seam_body = (out[0] * 0.135 + sway, out[1] * 0.135, 1.53)
    spread = 0.17 if front else 0.19
    tip = (out[0] * spread + 2 * sway, out[1] * spread, end_z)
    b.tube(root, seam_head, thickness, thickness * 1.1, hair, "Head", segments=5)
    if end_z < 1.45:
        b.tube(seam_body, tip, thickness * 1.1, thickness * 0.35, hair, "Chest", segments=5)


# In front of the face, parted in the middle (270 is straight ahead). The part is about as wide as her
# eyes, so depending on the angle one or both red eyes peek through.
for angle in (216, 229, 241, 302, 314, 327):  # a bit uneven: one eye shows more than the other
    strand(angle, rng.uniform(0.95, 1.20), rng.uniform(0.018, 0.024), front=True)
# Down the back, to the hips.
for i in range(11):
    strand(22 + i * 13.6 + rng.uniform(-3, 3), rng.uniform(0.78, 1.05), rng.uniform(0.026, 0.035), front=False)
# The sides stop above the shoulders so the arms move freely.
for angle in (190, 205, 335, 350):
    strand(angle, 1.50, 0.026, front=False)

# Long thin arms: tapered bones with knobby elbows and wrists that cover the joints when they bend,
# a narrow palm and long bony fingers that curl down a little; torn short sleeves.
for side, sign in (("Left", 1), ("Right", -1)):
    z = 1.46
    b.tube((sign * 0.15, 0, z), (sign * 0.30, 0, z), 0.048, 0.043, dress, f"{side}UpperArm", segments=8, cap=False)
    b.tube((sign * 0.15, 0, z), (sign * 0.46, 0, z), 0.034, 0.026, skin, f"{side}UpperArm", segments=8)
    b.sphere((sign * 0.46, 0, z), 0.029, skin, f"{side}LowerArm", segments=8, rings=5)
    b.tube((sign * 0.46, 0, z), (sign * 0.77, 0, z), 0.026, 0.018, skin, f"{side}LowerArm", segments=8)
    b.sphere((sign * 0.77, 0, z), 0.021, skin, f"{side}Hand", segments=8, rings=5)
    b.tube((sign * 0.77, 0, z), (sign * 0.84, 0, z - 0.004), 0.02, 0.024, skin, f"{side}Hand", segments=6)
    for dz in (-0.018, -0.006, 0.006, 0.018):
        length = 0.07 if abs(dz) < 0.01 else 0.06
        knuckle = (sign * 0.84, 0, z + dz)
        joint = (sign * (0.84 + length), -0.006, z + dz - 0.008)
        tip = (sign * (0.84 + length * 1.75), -0.016, z + dz - 0.028)
        b.tube(knuckle, joint, 0.0065, 0.0055, skin, f"{side}Hand", segments=5)
        b.tube(joint, tip, 0.0055, 0.003, skin, f"{side}Hand", segments=5)
    b.tube((sign * 0.79, -0.015, z), (sign * 0.84, -0.04, z - 0.012), 0.007, 0.004, skin, f"{side}Hand", segments=5)
    # Legs exist for the Humanoid rig but stay hidden inside the dress.
    b.tube((sign * 0.08, 0, 0.97), (sign * 0.08, 0, 0.52), 0.045, 0.035, dress, f"{side}UpperLeg", segments=6)
    b.tube((sign * 0.08, 0, 0.52), (sign * 0.08, 0, 0.10), 0.035, 0.028, dress, f"{side}LowerLeg", segments=6)
    b.box((sign * 0.08, -0.04, 0.05), (0.055, 0.15, 0.05), dress, f"{side}Foot")

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

# Scream (1.2 s): she throws her head back and opens her arms; the hair falls away from the face and
# the red eyes show. The code triggers it near the player (#35), together with the scream sound.
anim.new_clip("Scream", 36, loop=False)
anim.pose(1, merge(hang, hunch))
anim.pose(8, merge(hunch, {"Spine": (-6, 0, 0), "Chest": (-12, 0, 0), "Neck": (-15, 0, 0), "Head": (-30, 0, 0)}),
          aims={"LeftUpperArm": (0.75, -0.3, -0.35), "RightUpperArm": (-0.75, -0.3, -0.35),
                "LeftLowerArm": (0.7, -0.55, -0.1), "RightLowerArm": (-0.7, -0.55, -0.1)})
anim.pose(26, merge(hunch, {"Spine": (-8, 0, 0), "Chest": (-14, 0, 0), "Neck": (-17, 0, 0), "Head": (-34, 0, 4)}),
          aims={"LeftUpperArm": (0.8, -0.25, -0.3), "RightUpperArm": (-0.8, -0.25, -0.3),
                "LeftLowerArm": (0.75, -0.5, -0.05), "RightLowerArm": (-0.75, -0.5, -0.05)})
anim.pose(36, merge(hang, hunch))

lp.export_fbx(rig, [body], "ent_llorona.fbx")
print(f"[llorona] done, {triangles} triangles")
