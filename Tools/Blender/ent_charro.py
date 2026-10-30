"""El Charro Negro (rider): low-poly model, rig and riding animations.

Design: docs/personajes/charro.md. Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/ent_charro.py
Outputs Assets/_Project/Art/Models/Characters/ent_charro.fbx and the front/side sheet renders.
He is modeled standing in a T-pose (Humanoid rest pose); the riding pose comes from the animations, and
in Unity he is mounted on the horse's "Saddle" bone (ent_caballo.fbx).
"""

import math
import os
import sys

sys.path.append(os.path.dirname(__file__))
from mathutils import Quaternion  # noqa: E402

import lowpoly_humanoid as lp  # noqa: E402

lp.reset_scene()

# Palette from the character sheet.
suit = lp.make_material("Charro_Traje", "#0E0E10", roughness=0.6)
silver = lp.make_material("Charro_Plata", "#B9BCC0", roughness=0.3, metallic=0.9)
tie = lp.make_material("Charro_Mono", "#5A0B0B", roughness=0.5)
void = lp.make_material("Charro_Rostro", "#050505", roughness=1.0)
eyes = lp.make_material("Charro_Ojos", "#FF3010", roughness=0.3, emission_strength=6.0)
boots = lp.make_material("Charro_Botas", "#141012", roughness=0.45)
shirt = lp.make_material("Charro_Camisa", "#2A2A2E", roughness=0.7)
gloves = lp.make_material("Charro_Guantes", "#0A0A0C", roughness=0.5)
bone_mat = lp.make_material("Charro_Hueso", "#CFC6B0", roughness=0.6)

bones = lp.humanoid_bones(hips=0.97, chest_top=1.42, shoulder_z=1.46, head_top=1.76, shoulder_x=0.20,
                          elbow_x=0.48, wrist_x=0.74, hand_x=0.85)
rig = lp.build_armature("Charro", bones)

b = lp.PartBuilder()

# Tight charro trousers with a row of silver buttons down the outer seam, and riding boots.
# The torso is three closed pieces (hips, waist, chest) that overlap a little: when he leans while riding,
# the pieces turn separately, and open tubes would leave a gap where you could see inside (he would look
# hollow). Closed and overlapping, the joint always looks solid.
b.lathe([(0.86, 0.185), (1.03, 0.19)], suit, "Hips", segments=14, scale_y=0.75, cap_bottom=True, cap_top=True)
b.box((0, -0.143, 0.96), (0.08, 0.01, 0.055), silver, "Hips")  # belt buckle
for side, sign in (("Left", 1), ("Right", -1)):
    x = sign * 0.10
    b.tube((x, 0, 0.95), (x, 0, 0.52), 0.092, 0.066, suit, f"{side}UpperLeg", segments=8)
    b.sphere((x, 0, 0.52), 0.064, suit, f"{side}LowerLeg", segments=8, rings=5)                    # knee
    b.tube((x, 0, 0.52), (x, 0, 0.30), 0.062, 0.055, suit, f"{side}LowerLeg", segments=8)
    b.tube((x, 0, 0.32), (x, 0, 0.06), 0.068, 0.062, boots, f"{side}LowerLeg", segments=8)
    b.box((x, -0.05, 0.035), (0.10, 0.27, 0.075), boots, f"{side}Foot")
    for i in range(5):
        z = 0.90 - i * 0.09
        bone = f"{side}UpperLeg" if z > 0.52 else f"{side}LowerLeg"
        r = 0.092 - (0.90 - z) * 0.06
        b.sphere((x + sign * r, 0, z), 0.012, silver, bone, segments=6, rings=4)

# Short fitted charro jacket on a robust build: narrow waist, broad chest, lapels open in a V over a dark
# shirt, two rows of silver buttons and silver embroidery (grecas) on the back.
b.lathe([(0.97, 0.188), (1.10, 0.185), (1.23, 0.212)], suit, "Spine", segments=14, scale_y=0.74, cap_bottom=True, cap_top=True)
b.lathe([(1.17, 0.205), (1.32, 0.245), (1.40, 0.25), (1.47, 0.14)], suit, "Chest", segments=16, scale_y=0.74, cap_bottom=True, cap_top=True)
b.box((0, -0.181, 1.36), (0.085, 0.006, 0.14), shirt, "Chest")
for sign in (1, -1):
    lean = Quaternion((0, 1, 0), math.radians(-18 * sign))
    b.box((sign * 0.062, -0.186, 1.35), (0.04, 0.008, 0.17), suit, "Chest", rotation=lean)        # lapel
    b.box((sign * 0.082, -0.191, 1.35), (0.006, 0.004, 0.17), silver, "Chest", rotation=lean)     # its silver edge
for x in (-0.08, 0.08):
    for z in (1.08, 1.16, 1.24):
        b.sphere((x, -0.137 if z < 1.2 else -0.158, z), 0.012, silver, "Spine" if z < 1.2 else "Chest", segments=6, rings=4)
# Embroidered band across the back: two silver lines with diamonds between them.
for z in (1.30, 1.37):
    b.box((0, 0.182, z), (0.28, 0.006, 0.007), silver, "Chest")
for x in (-0.09, -0.03, 0.03, 0.09):
    b.box((x, 0.182, 1.335), (0.022, 0.006, 0.022), silver, "Chest", rotation=Quaternion((0, 1, 0), math.radians(45)))
for x in (-0.03, 0.03):
    b.box((x, -0.115, 1.445), (0.05, 0.02, 0.035), tie, "Chest")
b.sphere((0, -0.125, 1.445), 0.013, tie, "Chest", segments=6, rings=4)

# Arms: tapered sleeves with the elbow and wrist covered by rounded joints (no tube seams when they bend),
# silver embroidery at the cuffs, and black gloves with jointed fingers.
for side, sign in (("Left", 1), ("Right", -1)):
    z = 1.46
    # The sleeve starts at the shoulder joint (a tube starting inside the body would swing up through the
    # shoulder when the arm comes down); a round joint covers the shoulder instead.
    b.sphere((sign * 0.20, 0, z), 0.088, suit, f"{side}UpperArm", segments=10, rings=6)
    b.tube((sign * 0.20, 0, z), (sign * 0.48, 0, z), 0.084, 0.066, suit, f"{side}UpperArm", segments=10)
    b.sphere((sign * 0.48, 0, z), 0.068, suit, f"{side}LowerArm", segments=10, rings=6)
    b.tube((sign * 0.48, 0, z), (sign * 0.72, 0, z), 0.066, 0.056, suit, f"{side}LowerArm", segments=10)
    for dx in (0.66, 0.70):
        b.tube((sign * dx, 0, z), (sign * (dx + 0.012), 0, z), 0.06, 0.06, silver, f"{side}LowerArm", segments=10, cap=False)
    b.sphere((sign * 0.745, 0, z), 0.04, gloves, f"{side}Hand", segments=8, rings=5)
    b.tube((sign * 0.745, 0, z), (sign * 0.81, 0, z - 0.004), 0.038, 0.042, gloves, f"{side}Hand", segments=8)
    for dz in (-0.024, -0.008, 0.008, 0.024):
        joint = (sign * 0.86, -0.005, z + dz - 0.006)
        b.tube((sign * 0.81, 0, z + dz), joint, 0.011, 0.01, gloves, f"{side}Hand", segments=5)
        b.tube(joint, (sign * 0.895, -0.018, z + dz - 0.024), 0.01, 0.007, gloves, f"{side}Hand", segments=5)
    b.tube((sign * 0.77, -0.03, z), (sign * 0.82, -0.055, z - 0.015), 0.012, 0.008, gloves, f"{side}Hand", segments=5)

# Under the hat, a bare skull: the charro who sold his soul. Bone-colored cranium and cheekbones, deep
# black sockets with glowing red embers, a nose cavity and a jaw with a dark line of teeth.
b.tube((0, 0, 1.44), (0, 0, 1.56), 0.042, 0.04, bone_mat, "Neck", segments=8)
b.sphere((0, 0.005, 1.635), 1.0, bone_mat, "Head", segments=12, rings=8, scale=(0.085, 0.095, 0.095))
b.sphere((0, -0.03, 1.565), 1.0, bone_mat, "Head", segments=10, rings=6, scale=(0.06, 0.06, 0.04))     # jaw
for x in (-0.035, 0.035):
    b.sphere((x * 1.6, -0.06, 1.605), 0.022, bone_mat, "Head", segments=6, rings=4)                    # cheekbones
    b.sphere((x, -0.072, 1.635), 0.025, void, "Head", segments=8, rings=5, scale=(1.0, 0.7, 0.9))      # sockets
    b.sphere((x, -0.084, 1.633), 0.009, eyes, "Head", segments=6, rings=4)                             # embers
b.sphere((0, -0.088, 1.60), 1.0, void, "Head", segments=6, rings=4, scale=(0.012, 0.008, 0.016))      # nose
b.box((0, -0.083, 1.565), (0.06, 0.012, 0.008), void, "Head")                                          # teeth line
for x in (-0.024, -0.008, 0.008, 0.024):
    b.box((x, -0.085, 1.565), (0.004, 0.01, 0.02), void, "Head")

# Wide-brimmed charro hat sitting on the skull: the crown hugs the top of the head (the brim rests just
# above the brow), the brim has its edge rolled up (closed loop profile, so it reads from above and below),
# and a silver band. A dark ring inside the crown hides any gap against the skull.
b.lathe([(1.665, 0.10), (1.665, 0.30), (1.70, 0.355), (1.715, 0.35), (1.685, 0.30), (1.685, 0.10)], suit, "Head",
        segments=20)
b.lathe([(1.665, 0.098), (1.80, 0.10), (1.88, 0.085), (1.92, 0.045), (1.925, 0.0)], suit, "Head", segments=16)
b.lathe([(1.665, 0.094), (1.70, 0.09)], void, "Head", segments=16, inward=True)
b.lathe([(1.685, 0.101), (1.715, 0.1)], silver, "Head", segments=16)

body, triangles = b.finish("Charro", rig)
lp.render_sheet(1.93, "ent_charro")

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
    "LeftUpperLeg": (0.7, -0.5, -0.5), "RightUpperLeg": (-0.7, -0.5, -0.5),
    "LeftLowerLeg": (0.08, 0.1, -0.99), "RightLowerLeg": (-0.08, 0.1, -0.99),
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
    # A small lean only: the hands must stay where the reins end (see HANDS in ent_caballo.py).
    anim.pose(f, {"Spine": (6 + 3 * s, 0, 0), "Chest": (2, 0, 0), "Head": (-6 - 3 * s, 0, 0)},
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
