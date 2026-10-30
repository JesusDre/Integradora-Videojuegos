"""El Charro Negro (rider): low-poly model, rig and riding animations.

Design: docs/personajes/charro.md. Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/ent_charro.py
Outputs Assets/_Project/Art/Models/Characters/ent_charro.fbx and the front/side sheet renders.
He is modeled standing in a T-pose (Humanoid rest pose); the riding pose comes from the animations, and
in Unity he is mounted on the horse's "Saddle" bone (ent_caballo.fbx).
"""

import os
import sys

sys.path.append(os.path.dirname(__file__))
import lowpoly_humanoid as lp  # noqa: E402

lp.reset_scene()

# Palette from the character sheet.
suit = lp.make_material("Charro_Traje", "#0E0E10", roughness=0.6)
silver = lp.make_material("Charro_Plata", "#B9BCC0", roughness=0.3, metallic=0.9)
tie = lp.make_material("Charro_Mono", "#5A0B0B", roughness=0.5)
void = lp.make_material("Charro_Rostro", "#050505", roughness=1.0)
eyes = lp.make_material("Charro_Ojos", "#FF3010", roughness=0.3, emission_strength=6.0)
boots = lp.make_material("Charro_Botas", "#141012", roughness=0.45)

bones = lp.humanoid_bones(hips=0.97, chest_top=1.42, shoulder_z=1.46, head_top=1.76, shoulder_x=0.20,
                          elbow_x=0.48, wrist_x=0.74, hand_x=0.85)
rig = lp.build_armature("Charro", bones)

b = lp.PartBuilder()

# Tight charro trousers with a row of silver buttons down the outer seam, and riding boots.
b.lathe([(0.86, 0.16), (1.00, 0.165)], suit, "Hips", segments=14, scale_y=0.75)
b.box((0, -0.125, 0.96), (0.07, 0.01, 0.05), silver, "Hips")  # belt buckle
for side, sign in (("Left", 1), ("Right", -1)):
    x = sign * 0.10
    b.tube((x, 0, 0.95), (x, 0, 0.52), 0.075, 0.055, suit, f"{side}UpperLeg", segments=8)
    b.tube((x, 0, 0.52), (x, 0, 0.30), 0.052, 0.046, suit, f"{side}LowerLeg", segments=8)
    b.tube((x, 0, 0.32), (x, 0, 0.06), 0.058, 0.052, boots, f"{side}LowerLeg", segments=8)
    b.box((x, -0.05, 0.035), (0.09, 0.26, 0.07), boots, f"{side}Foot")
    for i in range(5):
        z = 0.90 - i * 0.09
        bone = f"{side}UpperLeg" if z > 0.52 else f"{side}LowerLeg"
        r = 0.075 - (0.90 - z) * 0.05
        b.sphere((x + sign * r, 0, z), 0.012, silver, bone, segments=6, rings=4)

# Short charro jacket with silver buttons and embroidery, white-less: all black, with a dark red bow tie.
b.lathe([(1.00, 0.165), (1.20, 0.18)], suit, "Spine", segments=14, scale_y=0.75)
b.lathe([(1.20, 0.18), (1.38, 0.205), (1.47, 0.11)], suit, "Chest", segments=14, scale_y=0.75, cap_top=True)
for x in (-0.075, 0.075):
    for i, z in enumerate((1.08, 1.17, 1.26, 1.34)):
        bone = "Spine" if z < 1.2 else "Chest"
        b.sphere((x, -0.13 - (0.01 if z > 1.2 else 0.0), z), 0.011, silver, bone, segments=6, rings=4)
b.box((0, -0.155, 1.30), (0.02, 0.005, 0.18), silver, "Chest")  # embroidered front seam
for x in (-0.03, 0.03):
    b.box((x, -0.095, 1.44), (0.05, 0.02, 0.035), tie, "Chest")
b.sphere((0, -0.105, 1.44), 0.013, tie, "Chest", segments=6, rings=4)

# Arms: black sleeves with silver cuffs and black gloves.
for side, sign in (("Left", 1), ("Right", -1)):
    b.tube((sign * 0.15, 0, 1.46), (sign * 0.48, 0, 1.46), 0.07, 0.058, suit, f"{side}UpperArm", segments=8)
    b.tube((sign * 0.48, 0, 1.46), (sign * 0.74, 0, 1.46), 0.056, 0.05, suit, f"{side}LowerArm", segments=8)
    b.tube((sign * 0.70, 0, 1.46), (sign * 0.74, 0, 1.46), 0.055, 0.055, silver, f"{side}LowerArm", segments=8, cap=False)
    b.box((sign * 0.80, 0, 1.46), (0.11, 0.04, 0.085), boots, f"{side}Hand")
    b.box((sign * 0.77, -0.035, 1.47), (0.05, 0.025, 0.025), boots, f"{side}Hand")

# No face under the hat: darkness and two red glowing eyes.
b.tube((0, 0, 1.44), (0, 0, 1.56), 0.045, 0.042, void, "Neck", segments=8)
b.sphere((0, 0, 1.62), 0.105, void, "Head", segments=10, rings=6)
for x in (-0.035, 0.035):
    b.sphere((x, -0.098, 1.635), 0.011, eyes, "Head", segments=6, rings=4)

# Wide-brimmed charro hat: a brim with the edge rolled up (closed loop profile, so it reads from above
# and below), a tall crown and a silver band.
b.lathe([(1.70, 0.10), (1.70, 0.30), (1.735, 0.355), (1.75, 0.35), (1.72, 0.30), (1.72, 0.10)], suit, "Head",
        segments=20)
b.lathe([(1.70, 0.115), (1.86, 0.11), (1.93, 0.085), (1.96, 0.04), (1.965, 0.0)], suit, "Head", segments=16)
b.lathe([(1.72, 0.118), (1.75, 0.117)], silver, "Head", segments=16)

body, triangles = b.finish("Charro", rig)
lp.render_sheet(1.97, "ent_charro")

# ---------------------------------------------------------------- animations
anim = lp.Animator(rig)


def merge(*dicts):
    out = {}
    for d in dicts:
        for k, v in d.items():
            x, y, z = out.get(k, (0, 0, 0))
            out[k] = (x + v[0], y + v[1], z + v[2])
    return out


# Riding pose: thighs forward and apart around the horse, shins hanging along its sides, both hands low
# in front holding the reins. Aims are world directions: -Y forward, +X his left, +Z up.
seat = {
    "LeftUpperLeg": (0.42, -0.62, -0.66), "RightUpperLeg": (-0.42, -0.62, -0.66),
    "LeftLowerLeg": (0.12, 0.08, -0.99), "RightLowerLeg": (-0.12, 0.08, -0.99),
}
reins = {
    "LeftUpperArm": (0.25, -0.35, -0.9), "RightUpperArm": (-0.25, -0.35, -0.9),
    "LeftLowerArm": (-0.1, -0.95, -0.28), "RightLowerArm": (0.1, -0.95, -0.28),
}

# RideIdle (2 s): sitting upright, slow breathing, head turning a little as if watching the player.
anim.new_clip("RideIdle", 60, loop=True)
for f, s in ((1, 0), (30, 1), (60, 0)):
    anim.pose(f, {"Chest": (-2 * s, 0, 0), "Head": (0, 0, 8 * s)}, aims={**seat, **reins})

# RideGallop (0.6 s): leaning forward, rocking and bouncing with the horse's stride.
anim.new_clip("RideGallop", 18, loop=True)
for f, s in ((1, 0), (5, 1), (10, 0), (14, -1), (18, 0)):
    anim.pose(f, {"Spine": (14 + 4 * s, 0, 0), "Chest": (4, 0, 0), "Head": (-10 - 3 * s, 0, 0)},
              {"Hips": (0, 0, 0.04 * abs(s))}, aims={**seat, **reins})

# Offer (1.5 s): reaches out with his right hand, palm up: the pact (an extra in the GDD, kept for the scene).
anim.new_clip("Offer", 45, loop=False)
anim.pose(1, {}, aims={**seat, **reins})
offer = {**seat, **reins, "RightUpperArm": (-0.35, -0.9, -0.1), "RightLowerArm": (-0.15, -0.98, 0.05)}
anim.pose(15, {"Spine": (8, 0, -12), "Head": (5, 0, -10)}, aims=offer)
anim.pose(35, {"Spine": (8, 0, -12), "Head": (5, 0, -10)}, aims=offer)
anim.pose(45, {}, aims={**seat, **reins})

# Dissolve (1.5 s): slumps forward and sinks into the saddle; the fade is done by the material (#39).
anim.new_clip("Dissolve", 45, loop=False)
anim.pose(1, {}, aims={**seat, **reins})
slump = {**seat, "LeftUpperArm": (0.2, -0.1, -0.97), "RightUpperArm": (-0.2, -0.1, -0.97)}
anim.pose(20, {"Spine": (35, 0, 0), "Chest": (15, 0, 0), "Head": (30, 0, 0)}, {"Hips": (0, 0, -0.2)}, aims=slump)
anim.pose(45, {"Spine": (50, 0, 0), "Chest": (20, 0, 0), "Head": (35, 0, 0)}, {"Hips": (0, 0, -0.6)}, aims=slump)

lp.export_fbx(rig, [body], "ent_charro.fbx")
print(f"[charro] done, {triangles} triangles")
