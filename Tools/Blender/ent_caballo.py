"""Horse of the Charro Negro: low-poly model, quadruped rig and animations.

Design: docs/personajes/charro.md. Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/ent_caballo.py
Outputs Assets/_Project/Art/Models/Characters/ent_caballo.fbx and the front/side sheet renders.
A horse is not a human, so its skeleton is "Generic" in Unity (not Humanoid); the rider (ent_charro.fbx)
is mounted on the "Saddle" bone.
"""

import os
import sys

sys.path.append(os.path.dirname(__file__))
import lowpoly_humanoid as lp  # noqa: E402

lp.reset_scene()

# Palette from the character sheet.
coat = lp.make_material("Caballo_Pelaje", "#09090B", roughness=0.4)
mane = lp.make_material("Caballo_Crin", "#151518", roughness=0.3)
hoof = lp.make_material("Caballo_Cascos", "#1A1A1C", roughness=0.6)
eyes = lp.make_material("Caballo_Ojos", "#FF3010", roughness=0.3, emission_strength=6.0)
leather = lp.make_material("Caballo_Silla", "#2A1A10", roughness=0.7)
silver = lp.make_material("Caballo_Plata", "#B9BCC0", roughness=0.3, metallic=0.9)

# Skeleton: name -> (head, tail, parent). The horse faces -Y; its left side is +X.
# Withers (top of the shoulders) at about 1.65 m and 2.4 m long, as in the sheet.
bones = {
    "Root": ((0, 0.55, 1.25), (0, 0.25, 1.27), None),         # pelvis / rump
    "Spine1": ((0, 0.25, 1.27), (0, -0.15, 1.30), "Root"),
    "Spine2": ((0, -0.15, 1.30), (0, -0.55, 1.33), "Spine1"),  # shoulders
    "Neck1": ((0, -0.60, 1.42), (0, -0.82, 1.70), "Spine2"),
    "Neck2": ((0, -0.82, 1.70), (0, -0.98, 1.92), "Neck1"),
    "Head": ((0, -0.98, 1.92), (0, -1.38, 1.68), "Neck2"),
    "Tail1": ((0, 0.82, 1.35), (0, 0.98, 1.05), "Root"),
    "Tail2": ((0, 0.98, 1.05), (0, 1.05, 0.60), "Tail1"),
    "Saddle": ((0, -0.12, 1.62), (0, -0.12, 1.75), "Spine2"),  # the rider is attached here
}
for side, sign in (("Left", 1), ("Right", -1)):
    x = sign * 0.16
    bones[f"Front{side}Upper"] = ((x, -0.55, 1.05), (x, -0.58, 0.58), "Spine2")
    bones[f"Front{side}Lower"] = ((x, -0.58, 0.58), (x, -0.58, 0.14), f"Front{side}Upper")
    bones[f"Front{side}Hoof"] = ((x, -0.58, 0.14), (x, -0.64, 0.02), f"Front{side}Lower")
    x = sign * 0.17
    bones[f"Back{side}Upper"] = ((x, 0.62, 1.10), (x, 0.50, 0.62), "Root")   # thigh, forward and down
    bones[f"Back{side}Lower"] = ((x, 0.50, 0.62), (x, 0.62, 0.14), f"Back{side}Upper")  # hock, back and down
    bones[f"Back{side}Hoof"] = ((x, 0.62, 0.14), (x, 0.58, 0.02), f"Back{side}Lower")
rig = lp.build_armature("Caballo", bones)

b = lp.PartBuilder()

# Body: rump, barrel and chest as three overlapping ovals, each following its part of the spine.
b.sphere((0, 0.50, 1.28), 1.0, coat, "Root", segments=14, rings=8, scale=(0.29, 0.38, 0.32))
b.sphere((0, 0.05, 1.25), 1.0, coat, "Spine1", segments=14, rings=8, scale=(0.30, 0.42, 0.34))
b.sphere((0, -0.45, 1.30), 1.0, coat, "Spine2", segments=14, rings=8, scale=(0.27, 0.30, 0.36))

# Neck, head, ears and glowing red eyes.
b.tube((0, -0.52, 1.38), (0, -0.85, 1.76), 0.20, 0.14, coat, "Neck1", segments=10)
b.tube((0, -0.80, 1.70), (0, -1.00, 1.94), 0.15, 0.12, coat, "Neck2", segments=10)
b.tube((0, -0.94, 1.96), (0, -1.43, 1.65), 0.12, 0.07, coat, "Head", segments=10)
b.sphere((0, -1.0, 1.92), 0.13, coat, "Head", segments=10, rings=6)
for x in (-0.06, 0.06):
    b.tube((x, -0.95, 2.02), (x * 1.2, -0.93, 2.15), 0.03, 0.0, coat, "Head", segments=5)
for x in (-0.095, 0.095):
    b.sphere((x, -1.08, 1.91), 0.018, eyes, "Head", segments=6, rings=4)

# Long mane along the top of the neck, and the tail.
for i in range(7):
    t = i / 6
    y = -0.58 - 0.40 * t
    z = 1.62 + 0.40 * t
    bone = "Neck1" if t < 0.55 else "Neck2"
    b.box((0, y + 0.06, z - 0.04), (0.035, 0.08, 0.24), mane, bone)
b.tube((0, 0.84, 1.36), (0, 0.98, 1.05), 0.06, 0.08, mane, "Tail1", segments=7)
b.tube((0, 0.98, 1.05), (0, 1.06, 0.60), 0.08, 0.02, mane, "Tail2", segments=7)

# Legs: front legs are straight; hind legs have the thigh and the hock angled like a real horse.
for side, sign in (("Left", 1), ("Right", -1)):
    x = sign * 0.16
    b.tube((x, -0.55, 1.10), (x, -0.58, 0.58), 0.08, 0.055, coat, f"Front{side}Upper", segments=8)
    b.tube((x, -0.58, 0.58), (x, -0.58, 0.14), 0.045, 0.038, coat, f"Front{side}Lower", segments=8)
    b.tube((x, -0.59, 0.14), (x, -0.60, 0.0), 0.045, 0.055, hoof, f"Front{side}Hoof", segments=8)
    x = sign * 0.17
    b.tube((x, 0.60, 1.15), (x, 0.50, 0.62), 0.12, 0.065, coat, f"Back{side}Upper", segments=8)
    b.tube((x, 0.50, 0.62), (x, 0.62, 0.14), 0.05, 0.038, coat, f"Back{side}Lower", segments=8)
    b.tube((x, 0.61, 0.14), (x, 0.60, 0.0), 0.045, 0.055, hoof, f"Back{side}Hoof", segments=8)

# Saddle with silver studs, horn and stirrups.
b.box((0, -0.12, 1.60), (0.34, 0.44, 0.07), leather, "Saddle")
b.box((0, 0.10, 1.66), (0.30, 0.06, 0.10), leather, "Saddle")
b.tube((0, -0.32, 1.62), (0, -0.35, 1.74), 0.022, 0.03, leather, "Saddle", segments=6)
for x in (-0.15, 0.15):
    for y in (-0.28, 0.04):
        b.sphere((x, y, 1.64), 0.015, silver, "Saddle", segments=6, rings=4)
for x in (-0.31, 0.31):
    b.tube((x, -0.10, 1.58), (x, -0.10, 1.08), 0.008, 0.008, leather, "Saddle", segments=4)
    b.box((x, -0.10, 1.05), (0.04, 0.10, 0.03), silver, "Saddle")

body, triangles = b.finish("Caballo", rig)
lp.render_sheet(2.20, "ent_caballo")

# ---------------------------------------------------------------- animations
# Rotations are around world axes (see lowpoly_humanoid.Animator): for a leg hanging down, negative X
# swings it forward and positive X swings it back.
anim = lp.Animator(rig)

# Idle (2 s): breathing, a head bob and a slow tail swish.
anim.new_clip("Idle", 60, loop=True)
for f, s in ((1, 0), (20, 1), (40, -1), (60, 0)):
    anim.pose(f, {"Spine1": (1.5 * abs(s), 0, 0), "Neck1": (3 * s, 0, 0), "Head": (-2 * s, 0, 0),
                  "Tail1": (0, 0, 12 * s), "Tail2": (0, 0, 8 * s)})

# Gallop (0.6 s): front and hind legs swing in opposite phase, the body rocks and bounces.
anim.new_clip("Gallop", 18, loop=True)
for f, front, back in ((1, 0, 1), (5, 1, 0), (10, 0, -1), (14, -1, 0), (18, 0, 1)):
    pose = {
        "Root": (4 * front, 0, 0), "Neck1": (-8 * front, 0, 0), "Head": (6 * front, 0, 0),
        "Tail1": (-25 - 10 * back, 0, 0),
    }
    for side in ("Left", "Right"):
        lag = 0.6 if side == "Right" else 1.0   # the right legs trail a little: reads as a gallop, not a hop
        pose[f"Front{side}Upper"] = (-35 * front * lag, 0, 0)
        pose[f"Front{side}Lower"] = ((70 if front * lag < -0.3 else 10), 0, 0)
        pose[f"Back{side}Upper"] = (30 * back * lag, 0, 0)
        pose[f"Back{side}Lower"] = ((-30 if back * lag > 0.3 else 5), 0, 0)
    anim.pose(f, pose, {"Root": (0, 0, 0.06 * abs(front))})

# Charge (1.2 s): rears up on its hind legs, then lunges forward.
anim.new_clip("Charge", 36, loop=False)
anim.pose(1, {})
rear = {"Root": (-32, 0, 0), "Neck1": (12, 0, 0), "Head": (10, 0, 0), "Tail1": (20, 0, 0)}
for side in ("Left", "Right"):
    rear[f"Front{side}Upper"] = (-55, 0, 0)
    rear[f"Front{side}Lower"] = (95, 0, 0)
    rear[f"Back{side}Upper"] = (25, 0, 0)
anim.pose(12, rear, {"Root": (0, 0, 0.10)})
anim.pose(18, rear, {"Root": (0, 0, 0.12)})
lunge = {"Root": (10, 0, 0), "Neck1": (-15, 0, 0), "Head": (10, 0, 0), "Tail1": (-35, 0, 0)}
for side in ("Left", "Right"):
    lunge[f"Front{side}Upper"] = (-40, 0, 0)
    lunge[f"Front{side}Lower"] = (10, 0, 0)
    lunge[f"Back{side}Upper"] = (35, 0, 0)
    lunge[f"Back{side}Lower"] = (-20, 0, 0)
anim.pose(26, lunge, {"Root": (0, 0, -0.03)})
anim.pose(36, {})

# Hit (0.5 s): flinches back and tosses its head when the cross strikes.
anim.new_clip("Hit", 15, loop=False)
anim.pose(1, {})
anim.pose(5, {"Root": (-8, 0, 6), "Neck1": (-18, 0, 10), "Head": (-15, 0, 0), "Tail1": (0, 0, 25)}, {"Root": (0, 0.10, 0)})
anim.pose(15, {})

# Dissolve (1.5 s): the legs give way and it sinks; the fade is done by the material in code (#39).
anim.new_clip("Dissolve", 45, loop=False)
anim.pose(1, {})
fall = {"Neck1": (35, 0, 0), "Head": (25, 0, 0), "Tail1": (40, 0, 0)}
for side in ("Left", "Right"):
    fall[f"Front{side}Upper"] = (-70, 0, 0)
    fall[f"Front{side}Lower"] = (130, 0, 0)
    fall[f"Back{side}Upper"] = (-50, 0, 0)
    fall[f"Back{side}Lower"] = (110, 0, 0)
anim.pose(25, fall, {"Root": (0, 0, -0.55)})
anim.pose(45, fall, {"Root": (0, 0, -0.95)})

lp.export_fbx(rig, [body], "ent_caballo.fbx")
print(f"[caballo] done, {triangles} triangles")
