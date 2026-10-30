"""Horse of the Charro Negro: low-poly model, quadruped rig and animations.

Design: docs/personajes/charro.md. Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/ent_caballo.py
Outputs Assets/_Project/Art/Models/Characters/ent_caballo.fbx and the front/side sheet renders.
A horse is not a human, so its skeleton is "Generic" in Unity (not Humanoid); the rider (ent_charro.fbx)
is mounted on the "Saddle" bone.
"""

import math
import os
import random
import sys

sys.path.append(os.path.dirname(__file__))
from mathutils import Quaternion  # noqa: E402

import lowpoly_humanoid as lp  # noqa: E402

lp.reset_scene()

# Palette from the character sheet.
coat = lp.make_material("Caballo_Pelaje", "#09090B", roughness=0.4)
mane = lp.make_material("Caballo_Crin", "#151518", roughness=0.3)
hoof = lp.make_material("Caballo_Cascos", "#1A1A1C", roughness=0.6)
eyes = lp.make_material("Caballo_Ojos", "#FF3010", roughness=0.3, emission_strength=10.0)
blanket = lp.make_material("Caballo_Manta", "#4A0A0A", roughness=0.8)
bone_mat = lp.make_material("Caballo_Hueso", "#26221E", roughness=0.7)
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
    "Tail2": ((0, 0.98, 1.05), (0, 1.10, 0.30), "Tail1"),
    "Saddle": ((0, -0.12, 1.70), (0, -0.12, 1.83), "Spine2"),  # the rider is attached here (top of the seat)
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
rng = random.Random(11)  # fixed seed: mane and tail strands come out the same every run

# Body in one piece: rings from the chest to the rump, each following its part of the spine, so the
# withers, back and croup read as one animal (not three balls). Broad and heavy: a war horse.
# (center, half-width, half-height, bone)
b.loft([
    ((0, -0.86, 1.30), 0.11, 0.15, "Spine2"),
    ((0, -0.75, 1.29), 0.26, 0.33, "Spine2"),
    ((0, -0.55, 1.30), 0.32, 0.40, "Spine2"),   # chest and withers
    ((0, -0.30, 1.27), 0.33, 0.40, "Spine2"),
    ((0, -0.02, 1.24), 0.34, 0.39, "Spine1"),   # barrel
    ((0, 0.25, 1.25), 0.33, 0.38, "Spine1"),
    ((0, 0.50, 1.29), 0.33, 0.38, "Root"),      # croup
    ((0, 0.72, 1.28), 0.29, 0.33, "Root"),
    ((0, 0.87, 1.26), 0.17, 0.20, "Root"),
], coat, segments=16)
# Chest muscles.
for x in (-0.12, 0.12):
    b.sphere((x, -0.78, 1.16), 1.0, coat, "Spine2", segments=10, rings=6, scale=(0.13, 0.12, 0.18))

# Demonic touch: ribs, shoulder blades and hip bones showing through the skin.
for side in (1, -1):
    for i in range(5):
        y = -0.22 + i * 0.10
        bone = "Spine2" if y < -0.15 else "Spine1"
        b.sphere((side * 0.325, y, 1.17), 1.0, bone_mat, bone, segments=6, rings=4, scale=(0.025, 0.028, 0.17))
    b.sphere((side * 0.27, -0.52, 1.46), 1.0, bone_mat, "Spine2", segments=6, rings=4, scale=(0.04, 0.10, 0.06))  # shoulder blade
    b.sphere((side * 0.27, 0.58, 1.50), 0.05, bone_mat, "Root", segments=6, rings=4)                                 # hip bone

# Neck: thick and arched where it meets the chest, narrowing up to the head.
b.loft([
    ((0, -0.62, 1.42), 0.17, 0.27, "Spine2"),
    ((0, -0.76, 1.62), 0.14, 0.22, "Neck1"),
    ((0, -0.88, 1.80), 0.115, 0.17, "Neck2"),
    ((0, -0.98, 1.94), 0.09, 0.13, "Neck2"),
], coat, segments=12, cap_start=False)

# Head: broad forehead and jaw, tapering to the muzzle; nostrils, eyes, ears and a bridle.
b.loft([
    ((0, -0.95, 2.00), 0.08, 0.08, "Head"),
    ((0, -1.03, 1.93), 0.10, 0.14, "Head"),     # forehead and jaw
    ((0, -1.18, 1.82), 0.08, 0.10, "Head"),
    ((0, -1.32, 1.72), 0.07, 0.085, "Head"),
    ((0, -1.44, 1.64), 0.065, 0.08, "Head"),    # muzzle
    ((0, -1.49, 1.61), 0.04, 0.05, "Head"),
], coat, segments=12)
for x in (-0.03, 0.03):
    b.sphere((x, -1.47, 1.65), 0.014, hoof, "Head", segments=6, rings=4)            # nostrils
for x in (-0.095, 0.095):
    b.sphere((x, -1.06, 1.94), 0.024, eyes, "Head", segments=6, rings=4)
    b.tube((x * 0.6, -0.98, 2.04), (x * 0.75, -0.96, 2.17), 0.03, 0.0, coat, "Head", segments=5)  # ears
# Bridle straps around the nose and cheek, and the reins up to the saddle horn.
b.loft([((0, -1.36, 1.69), 0.078, 0.092, "Head"), ((0, -1.375, 1.68), 0.078, 0.092, "Head")], leather,
       segments=12, cap_start=False, cap_end=False)
for x in (-0.085, 0.085):
    b.tube((x, -1.36, 1.72), (x, -1.02, 1.98), 0.008, 0.008, leather, "Head", segments=4)
    b.sphere((x, -1.38, 1.66), 0.014, silver, "Head", segments=6, rings=4)          # bit rings
    b.tube((x, -1.38, 1.66), (x * 1.4, -1.0, 1.66), 0.006, 0.006, leather, "Head", segments=4)
    b.tube((x * 1.4, -1.0, 1.66), (x * 0.6, -0.38, 1.80), 0.006, 0.006, leather, "Neck1", segments=4)

# Long, thick mane falling on both sides of the neck (alternating), a crest along the top and a big
# forelock, so it reads from any side; long loose tail down to near the ground.
b.loft([((0, -0.60, 1.66), 0.04, 0.05, "Neck1"), ((0, -0.78, 1.86), 0.045, 0.05, "Neck1"),
        ((0, -0.92, 2.01), 0.04, 0.045, "Neck2")], mane, segments=6)
for i in range(12):
    t = i / 11
    y = -0.60 - 0.34 * t
    z = 1.66 + 0.36 * t
    bone = "Neck1" if t < 0.5 else "Neck2"
    side = 1 if i % 2 == 0 else -0.7
    length = rng.uniform(0.32, 0.46)
    b.tube((0, y + 0.05, z + 0.02), (side * 0.17, y + 0.10, z - length), 0.045, 0.01, mane, bone, segments=5)
b.tube((0, -0.98, 2.05), (0, -1.12, 1.90), 0.045, 0.01, mane, "Head", segments=5)
for k in range(5):
    dx = (k - 2) * 0.028
    b.tube((dx, 0.87, 1.35), (dx * 1.3, 0.99, 1.05), 0.04, 0.045, mane, "Tail1", segments=5)
    b.tube((dx * 1.3, 0.99, 1.05), (dx * 2.5 + rng.uniform(-0.02, 0.02), 1.12, rng.uniform(0.22, 0.34)),
           0.045, 0.01, mane, "Tail2", segments=5)

# Legs: big muscled upper legs, knobby knees, hocks and fetlocks, sturdy cannons and large hooves.
for side, sign in (("Left", 1), ("Right", -1)):
    x = sign * 0.17
    b.sphere((x, -0.56, 1.10), 1.0, coat, "Front{}Upper".format(side), segments=10, rings=6, scale=(0.13, 0.17, 0.22))
    b.tube((x, -0.55, 1.05), (x, -0.58, 0.60), 0.115, 0.068, coat, f"Front{side}Upper", segments=8)
    b.sphere((x, -0.58, 0.58), 0.065, coat, f"Front{side}Lower", segments=8, rings=5)              # knee
    b.tube((x, -0.58, 0.58), (x, -0.58, 0.16), 0.05, 0.045, coat, f"Front{side}Lower", segments=8)
    b.sphere((x, -0.585, 0.14), 0.055, coat, f"Front{side}Hoof", segments=8, rings=5)             # fetlock
    b.tube((x, -0.59, 0.14), (x, -0.61, 0.08), 0.046, 0.052, coat, f"Front{side}Hoof", segments=8) # pastern
    b.tube((x, -0.61, 0.09), (x, -0.62, 0.0), 0.06, 0.08, hoof, f"Front{side}Hoof", segments=8)
    x = sign * 0.18
    b.sphere((x, 0.58, 1.08), 1.0, coat, f"Back{side}Upper", segments=10, rings=6, scale=(0.15, 0.22, 0.27))  # haunch
    b.tube((x, 0.60, 1.05), (x, 0.50, 0.64), 0.15, 0.08, coat, f"Back{side}Upper", segments=8)
    b.sphere((x, 0.50, 0.62), 0.07, coat, f"Back{side}Lower", segments=8, rings=5)                # hock
    b.tube((x, 0.50, 0.62), (x, 0.61, 0.16), 0.058, 0.045, coat, f"Back{side}Lower", segments=8)
    b.sphere((x, 0.615, 0.14), 0.055, coat, f"Back{side}Hoof", segments=8, rings=5)
    b.tube((x, 0.615, 0.14), (x, 0.60, 0.08), 0.046, 0.052, coat, f"Back{side}Hoof", segments=8)
    b.tube((x, 0.60, 0.09), (x, 0.59, 0.0), 0.06, 0.08, hoof, f"Back{side}Hoof", segments=8)

# Charro saddle sized for the rider: blanket draped over the back, seat with a high cantle and a pommel
# with the horn, leather fenders under his thighs and stirrups exactly where his feet rest when he rides
# (measured from the rider's RideIdle pose and divided by the horse's scale).
for side in (1, -1):
    tilt = Quaternion((0, 1, 0), math.radians(-18 * side))
    b.box((side * 0.33, -0.10, 1.52), (0.02, 0.62, 0.32), blanket, "Spine2", rotation=tilt)
    b.box((side * 0.37, -0.10, 1.37), (0.025, 0.60, 0.012), silver, "Spine2", rotation=tilt)  # trim
b.box((0, -0.10, 1.665), (0.62, 0.64, 0.02), blanket, "Spine2")
b.box((0, -0.12, 1.695), (0.36, 0.46, 0.05), leather, "Saddle")
b.box((0, 0.12, 1.75), (0.32, 0.07, 0.12), leather, "Saddle")                      # cantle
b.box((0, -0.33, 1.73), (0.22, 0.06, 0.10), leather, "Saddle")                     # pommel
b.tube((0, -0.35, 1.76), (0, -0.38, 1.87), 0.024, 0.034, leather, "Saddle", segments=6)  # horn
for x in (-0.15, 0.15):
    for y in (-0.28, 0.06):
        b.sphere((x, y, 1.725), 0.016, silver, "Saddle", segments=6, rings=4)
STIRRUP = (0.383, -0.32, 1.126)
for side in (1, -1):
    b.box((side * 0.36, -0.20, 1.46), (0.02, 0.24, 0.32), leather, "Saddle",
          rotation=Quaternion((0, 1, 0), math.radians(-10 * side)))             # fender
    b.sphere((side * 0.375, -0.20, 1.50), 0.028, silver, "Saddle", segments=6, rings=4)  # concho
    b.tube((side * 0.31, -0.22, 1.69), (side * STIRRUP[0], STIRRUP[1], STIRRUP[2] + 0.04), 0.009, 0.009,
           leather, "Saddle", segments=4)
    b.box((side * STIRRUP[0], STIRRUP[1], STIRRUP[2]), (0.06, 0.13, 0.03), silver, "Saddle")

body, triangles = b.finish("Caballo", rig)

# Modeled at a normal horse size, then scaled up: withers at about 1.9 m, a war horse that dominates the
# player. Root offsets in the animations below are multiplied by K to match.
K = 1.15
lp.scale_character(rig, [body], K)
lp.render_sheet(2.55, "ent_caballo")

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
    anim.pose(f, pose, {"Root": (0, 0, K * 0.06 * abs(front))})

# Charge (1.2 s): rears up on its hind legs, then lunges forward.
anim.new_clip("Charge", 36, loop=False)
anim.pose(1, {})
rear = {"Root": (-32, 0, 0), "Neck1": (12, 0, 0), "Head": (10, 0, 0), "Tail1": (20, 0, 0)}
for side in ("Left", "Right"):
    rear[f"Front{side}Upper"] = (-55, 0, 0)
    rear[f"Front{side}Lower"] = (95, 0, 0)
    rear[f"Back{side}Upper"] = (25, 0, 0)
anim.pose(12, rear, {"Root": (0, 0, K * 0.10)})
anim.pose(18, rear, {"Root": (0, 0, K * 0.12)})
lunge = {"Root": (10, 0, 0), "Neck1": (-15, 0, 0), "Head": (10, 0, 0), "Tail1": (-35, 0, 0)}
for side in ("Left", "Right"):
    lunge[f"Front{side}Upper"] = (-40, 0, 0)
    lunge[f"Front{side}Lower"] = (10, 0, 0)
    lunge[f"Back{side}Upper"] = (35, 0, 0)
    lunge[f"Back{side}Lower"] = (-20, 0, 0)
anim.pose(26, lunge, {"Root": (0, 0, K * -0.03)})
anim.pose(36, {})

# Hit (0.5 s): flinches back and tosses its head when the cross strikes.
anim.new_clip("Hit", 15, loop=False)
anim.pose(1, {})
anim.pose(5, {"Root": (-8, 0, 6), "Neck1": (-18, 0, 10), "Head": (-15, 0, 0), "Tail1": (0, 0, 25)}, {"Root": (0, K * 0.10, 0)})
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
anim.pose(25, fall, {"Root": (0, 0, K * -0.55)})
anim.pose(45, fall, {"Root": (0, 0, K * -0.95)})

lp.export_fbx(rig, [body], "ent_caballo.fbx")
print(f"[caballo] done, {triangles} triangles")
