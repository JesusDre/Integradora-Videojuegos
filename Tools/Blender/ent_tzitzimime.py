"""La Tzitzimime: low-poly model, rig and animations.

Design: docs/personajes/tzitzimime.md. Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/ent_tzitzimime.py
Outputs Assets/_Project/Art/Models/Characters/ent_tzitzimime.fbx and the front/side sheet renders.
She is modeled at a human scale (Humanoid rig, like the others) and scaled up to 3.2 m at the end.
"""

import math
import os
import random
import sys

sys.path.append(os.path.dirname(__file__))
from mathutils import Vector  # noqa: E402

import lowpoly_humanoid as lp  # noqa: E402

lp.reset_scene()

# Palette from the character sheet.
night = lp.make_material("Tzitzimime_Cuerpo", "#0A0E1A", roughness=0.6)
bone_mat = lp.make_material("Tzitzimime_Hueso", "#CFC6B0", roughness=0.6)
stars = lp.make_material("Tzitzimime_Estrellas", "#DDE8FF", roughness=0.3, emission_strength=8.0)
obsidian = lp.make_material("Tzitzimime_Obsidiana", "#1A0A0C", roughness=0.15, metallic=0.3)
edge = lp.make_material("Tzitzimime_Filo", "#C0201A", roughness=0.3, emission_strength=3.0)
void = lp.make_material("Tzitzimime_Cuencas", "#050505", roughness=1.0)
eyes = lp.make_material("Tzitzimime_Ojos", "#FF1A0A", roughness=0.3, emission_strength=10.0)

# Long, thin limbs: a figure stretched beyond human proportions.
bones = lp.humanoid_bones(hips=0.95, chest_top=1.40, shoulder_z=1.44, head_top=1.74, shoulder_x=0.17,
                          elbow_x=0.50, wrist_x=0.82, hand_x=0.92, leg_x=0.09)
rig = lp.build_armature("Tzitzimime", bones)

b = lp.PartBuilder()
rng = random.Random(23)  # fixed seed: the stars on the body come out the same every run


def starry(a, b_, radius, bone, count):
    """Scatter small glowing stars on the surface of a limb (the points of light the copal reveals)."""
    a, b_ = Vector(a), Vector(b_)
    axis = (b_ - a).normalized()
    side = axis.orthogonal().normalized()
    up = axis.cross(side)
    for _ in range(count):
        t = rng.uniform(0.1, 0.9)
        ang = rng.uniform(0, 2 * math.pi)
        p = a.lerp(b_, t) + (side * math.cos(ang) + up * math.sin(ang)) * radius
        b.sphere(tuple(p), rng.uniform(0.006, 0.011), stars, bone, segments=5, rings=3)


# Torso: dark as the night sky, thin and closed in overlapping pieces (no hollow gaps), with the ribs as
# bone arcs on the front and the spine down the back.
b.lathe([(0.82, 0.15), (1.00, 0.13)], night, "Hips", segments=12, scale_y=0.7, cap_bottom=True, cap_top=True)
b.lathe([(0.97, 0.115), (1.20, 0.12)], night, "Spine", segments=12, scale_y=0.7, cap_bottom=True, cap_top=True)
# The chest widens into the shoulders so the arms join the body without a gap.
b.lathe([(1.17, 0.13), (1.32, 0.16), (1.41, 0.19), (1.47, 0.10)], night, "Chest", segments=12, scale_y=0.7, cap_bottom=True, cap_top=True)
for i, z in enumerate((1.20, 1.25, 1.30, 1.35)):
    r = 0.135 + 0.008 * i
    b.lathe([(z, r), (z + 0.018, r)], bone_mat, "Chest", segments=10, arc=(200, 340), scale_y=0.75)
for z in (0.98, 1.05, 1.12, 1.19, 1.26, 1.33):
    b.box((0, 0.088 if z > 1.15 else 0.084, z), (0.04, 0.02, 0.035), bone_mat, "Spine" if z < 1.17 else "Chest")
b.lathe([(0.90, 0.155), (0.95, 0.16)], bone_mat, "Hips", segments=12, scale_y=0.72)   # pelvis rim
starry((0, 0, 1.0), (0, 0, 1.4), 0.10, "Chest", 10)

# Skirt of night with a torn hem, also starred.
hem = b.lathe([(0.25, 0.30), (0.55, 0.24), (0.85, 0.16)], night, "Hips", segments=16, folds=8, fold_depth=0.12, scale_y=0.8)
for v in {v for f in hem for v in f.verts}:
    if v.co.z < 0.26:
        v.co.z += rng.uniform(0.0, 0.18)
b.lathe([(0.25, 0.29), (0.55, 0.23), (0.85, 0.15)], night, "Hips", segments=16, folds=8, fold_depth=0.12,
        scale_y=0.8, inward=True)   # lining: the skirt is never see-through from below
starry((0, 0, 0.4), (0, 0, 0.8), 0.24, "Hips", 10)

# Skull with deep sockets, red eyes and a jaw full of teeth.
b.tube((0, 0.01, 1.40), (0, 0.01, 1.55), 0.035, 0.032, bone_mat, "Neck", segments=8)
b.sphere((0, 0.005, 1.63), 1.0, bone_mat, "Head", segments=12, rings=8, scale=(0.085, 0.095, 0.10))
b.sphere((0, -0.035, 1.555), 1.0, bone_mat, "Head", segments=10, rings=6, scale=(0.06, 0.06, 0.04))
for x in (-0.035, 0.035):
    b.sphere((x, -0.072, 1.635), 0.026, void, "Head", segments=8, rings=5, scale=(1.0, 0.7, 0.9))
    b.sphere((x, -0.086, 1.633), 0.011, eyes, "Head", segments=6, rings=4)
b.sphere((0, -0.088, 1.598), 1.0, void, "Head", segments=6, rings=4, scale=(0.012, 0.008, 0.016))
b.box((0, -0.088, 1.553), (0.065, 0.012, 0.008), void, "Head")
for x in (-0.027, -0.009, 0.009, 0.027):
    b.box((x, -0.092, 1.553), (0.012, 0.01, 0.024), bone_mat, "Head")                   # teeth

# Headdress: a crown of obsidian rays like a starburst, each with a glowing red tip.
for i in range(11):
    a = math.radians(-100 + i * 20)          # fan across the top and sides of the head
    tilt = math.radians(-12)                  # leaning back a little
    root = Vector((0.07 * math.sin(a), 0.03, 1.66 + 0.07 * math.cos(a)))
    direction = Vector((math.sin(a), math.sin(tilt) * -1, math.cos(a))).normalized()
    length = 0.42 if i % 2 == 0 else 0.30
    tip = root + direction * length
    b.tube(tuple(root), tuple(root + direction * (length - 0.06)), 0.03, 0.012, obsidian, "Head", segments=5)
    b.tube(tuple(root + direction * (length - 0.07)), tuple(tip), 0.013, 0.0, edge, "Head", segments=5)

# Long thin arms: dark with bony forearms, and long obsidian claws.
for side, sign in (("Left", 1), ("Right", -1)):
    z = 1.44
    b.tube((sign * 0.17, 0, z), (sign * 0.52, 0, z), 0.05, 0.034, night, f"{side}UpperArm", segments=8)
    starry((sign * 0.17, 0, z), (sign * 0.50, 0, z), 0.04, f"{side}UpperArm", 4)
    b.tube((sign * 0.47, 0, z), (sign * 0.83, 0, z), 0.03, 0.022, bone_mat, f"{side}LowerArm", segments=8)
    b.tube((sign * 0.47, 0, z - 0.012), (sign * 0.83, 0, z - 0.012), 0.016, 0.014, bone_mat, f"{side}LowerArm", segments=6)
    b.tube((sign * 0.80, 0, z), (sign * 0.90, 0, z), 0.026, 0.03, bone_mat, f"{side}Hand", segments=6)
    for dz in (-0.027, -0.009, 0.009, 0.027):
        knuckle = (sign * 0.90, 0, z + dz)
        b.tube(knuckle, (sign * 0.97, -0.01, z + dz - 0.01), 0.008, 0.007, bone_mat, f"{side}Hand", segments=5)
        b.tube((sign * 0.965, -0.01, z + dz - 0.01), (sign * 1.13, -0.07, z + dz - 0.07), 0.009, 0.0,
               obsidian, f"{side}Hand", segments=5)                                       # claw
    b.tube((sign * 0.86, -0.025, z), (sign * 0.98, -0.09, z - 0.03), 0.009, 0.0, obsidian, f"{side}Hand", segments=5)

# Legs: dark, thin, bony feet with claws.
for side, sign in (("Left", 1), ("Right", -1)):
    x = sign * 0.09
    b.tube((x, 0, 0.95), (x, 0, 0.50), 0.055, 0.04, night, f"{side}UpperLeg", segments=8)
    b.tube((x, 0, 0.56), (x, 0, 0.10), 0.035, 0.025, bone_mat, f"{side}LowerLeg", segments=8)
    b.tube((x, 0.0, 0.10), (x, -0.13, 0.02), 0.03, 0.02, bone_mat, f"{side}Foot", segments=6)
    for dx in (-0.025, 0.0, 0.025):
        b.tube((x + dx, -0.12, 0.025), (x + dx * 1.4, -0.22, 0.0), 0.008, 0.0, obsidian, f"{side}Foot", segments=4)

body, triangles = b.finish("Tzitzimime", rig)

# Modeled at a human scale, then scaled to 3.2 m: a giant star being. Hip offsets in the animations
# below are multiplied by K to match.
HEIGHT = 3.20
K = HEIGHT / 1.85
lp.scale_character(rig, [body], K)
lp.render_sheet(HEIGHT, "ent_tzitzimime")

# ---------------------------------------------------------------- animations
anim = lp.Animator(rig)


def merge(*dicts):
    out = {}
    for d in dicts:
        for k, v in d.items():
            x, y, z = out.get(k, (0, 0, 0))
            out[k] = (x + v[0], y + v[1], z + v[2])
    return out


# CeilingCrawl (1.4 s): crawling on all fours like a spider. The code turns her upside down and sticks
# her to the ceiling (#45); here she crawls as if on a floor. Aims: -Y forward, +X her left, +Z up.
anim.new_clip("CeilingCrawl", 42, loop=True)
crouch = {"Spine": (55, 0, 0), "Chest": (20, 0, 0), "Neck": (-35, 0, 0), "Head": (-30, 0, 0)}
for f, s in ((1, 0), (11, 1), (22, 0), (32, -1), (42, 0)):
    anim.pose(f, merge(crouch, {"Spine": (0, 0, 6 * s)}), {"Hips": (0, 0, K * -0.35)}, aims={
        "LeftUpperArm": (0.45, -0.55 - 0.25 * s, -0.7), "RightUpperArm": (-0.45, -0.55 + 0.25 * s, -0.7),
        "LeftLowerArm": (0.2, -0.5 - 0.2 * s, -0.85), "RightLowerArm": (-0.2, -0.5 + 0.2 * s, -0.85),
        "LeftUpperLeg": (0.45, -0.75 + 0.3 * s, -0.5), "RightUpperLeg": (-0.45, -0.75 - 0.3 * s, -0.5),
        "LeftLowerLeg": (0.2, 0.45, -0.87), "RightLowerLeg": (-0.2, 0.45, -0.87),
    })

# CeilingAttack (1.2 s): from the crawl, rears up and slams both clawed arms down.
anim.new_clip("CeilingAttack", 36, loop=False)
crawl_legs = {"LeftUpperLeg": (0.45, -0.75, -0.5), "RightUpperLeg": (-0.45, -0.75, -0.5),
              "LeftLowerLeg": (0.2, 0.45, -0.87), "RightLowerLeg": (-0.2, 0.45, -0.87)}
anim.pose(1, crouch, {"Hips": (0, 0, K * -0.35)}, aims={**crawl_legs,
          "LeftUpperArm": (0.45, -0.55, -0.7), "RightUpperArm": (-0.45, -0.55, -0.7),
          "LeftLowerArm": (0.2, -0.5, -0.85), "RightLowerArm": (-0.2, -0.5, -0.85)})
anim.pose(12, merge(crouch, {"Spine": (-35, 0, 0), "Head": (10, 0, 0)}), {"Hips": (0, 0, K * -0.30)}, aims={**crawl_legs,
          "LeftUpperArm": (0.5, -0.2, 0.85), "RightUpperArm": (-0.5, -0.2, 0.85),
          "LeftLowerArm": (0.3, 0.2, 0.93), "RightLowerArm": (-0.3, 0.2, 0.93)})
anim.pose(18, merge(crouch, {"Spine": (20, 0, 0)}), {"Hips": (0, 0, K * -0.38)}, aims={**crawl_legs,
          "LeftUpperArm": (0.3, -0.8, -0.5), "RightUpperArm": (-0.3, -0.8, -0.5),
          "LeftLowerArm": (0.1, -0.6, -0.8), "RightLowerArm": (-0.1, -0.6, -0.8)})
anim.pose(36, crouch, {"Hips": (0, 0, K * -0.35)}, aims={**crawl_legs,
          "LeftUpperArm": (0.45, -0.55, -0.7), "RightUpperArm": (-0.45, -0.55, -0.7),
          "LeftLowerArm": (0.2, -0.5, -0.85), "RightLowerArm": (-0.2, -0.5, -0.85)})

# Reveal (1 s): the copal smoke burns her: she jerks and shields her face with her arms.
anim.new_clip("Reveal", 30, loop=False)
stand_open = {"LeftUpperArm": (0.85, -0.3, -0.4), "RightUpperArm": (-0.85, -0.3, -0.4),
              "LeftLowerArm": (0.8, -0.5, 0.0), "RightLowerArm": (-0.8, -0.5, 0.0)}
shield = {"LeftUpperArm": (0.4, -0.7, 0.2), "RightUpperArm": (-0.4, -0.7, 0.2),
          "LeftLowerArm": (-0.6, -0.3, 0.75), "RightLowerArm": (0.6, -0.3, 0.75)}
anim.pose(1, {}, aims=stand_open)
for f, jerk in ((5, 1), (9, -1), (13, 1), (18, 0)):
    anim.pose(f, {"Spine": (-10, 0, 8 * jerk), "Chest": (-8, 0, 0), "Head": (15, 0, -10 * jerk)}, aims=shield)
anim.pose(30, {}, aims=stand_open)

# FinalIdle (3 s): standing tall, arms spread wide, the crown swaying, breathing slow and heavy.
anim.new_clip("FinalIdle", 90, loop=True)
for f, s in ((1, 0), (45, 1), (90, 0)):
    anim.pose(f, {"Chest": (-6 * s, 0, 0), "Head": (-8 - 4 * s, 0, 5 * s)},
              aims={"LeftUpperArm": (0.9, -0.25, -0.35 + 0.15 * s), "RightUpperArm": (-0.9, -0.25, -0.35 + 0.15 * s),
                    "LeftLowerArm": (0.85, -0.45, 0.1 * s), "RightLowerArm": (-0.85, -0.45, 0.1 * s)})

# FinalAttack (1.2 s): winds up to one side and sweeps both claws across in front of her.
anim.new_clip("FinalAttack", 36, loop=False)
anim.pose(1, {}, aims=stand_open)
anim.pose(12, {"Spine": (-6, 0, 35), "Chest": (0, 0, 15)},
          aims={"LeftUpperArm": (0.6, 0.6, 0.2), "RightUpperArm": (-0.2, 0.9, 0.1),
                "LeftLowerArm": (0.3, 0.8, 0.3), "RightLowerArm": (-0.1, 0.9, 0.2)})
anim.pose(20, {"Spine": (12, 0, -35), "Chest": (6, 0, -15)},
          aims={"LeftUpperArm": (-0.2, -0.95, -0.1), "RightUpperArm": (-0.8, -0.5, -0.2),
                "LeftLowerArm": (-0.5, -0.8, -0.2), "RightLowerArm": (-0.9, -0.3, -0.2)})
anim.pose(36, {}, aims=stand_open)

# Defeat (2 s): she reaches up, then crumbles to her knees and sinks; the "breaking into lights"
# is a particle effect in code (#45).
anim.new_clip("Defeat", 60, loop=False)
anim.pose(1, {}, aims=stand_open)
anim.pose(18, {"Chest": (-20, 0, 0), "Head": (-30, 0, 0)},
          aims={"LeftUpperArm": (0.3, -0.1, 0.95), "RightUpperArm": (-0.3, -0.1, 0.95),
                "LeftLowerArm": (0.2, 0.0, 0.98), "RightLowerArm": (-0.2, 0.0, 0.98)})
anim.pose(40, {"Spine": (35, 0, 0), "Head": (35, 0, 0)}, {"Hips": (0, 0, K * -0.45)},
          aims={"LeftUpperLeg": (0.2, -0.95, -0.1), "RightUpperLeg": (-0.2, -0.95, -0.1),
                "LeftLowerLeg": (0.1, 0.1, -0.99), "RightLowerLeg": (-0.1, 0.1, -0.99),
                "LeftUpperArm": (0.3, -0.3, -0.9), "RightUpperArm": (-0.3, -0.3, -0.9)})
anim.pose(60, {"Spine": (60, 0, 0), "Head": (40, 0, 0)}, {"Hips": (0, 0, K * -0.85)},
          aims={"LeftUpperLeg": (0.2, -0.95, -0.1), "RightUpperLeg": (-0.2, -0.95, -0.1),
                "LeftLowerLeg": (0.1, 0.1, -0.99), "RightLowerLeg": (-0.1, 0.1, -0.99),
                "LeftUpperArm": (0.3, -0.3, -0.9), "RightUpperArm": (-0.3, -0.3, -0.9)})

lp.export_fbx(rig, [body], "ent_tzitzimime.fbx")
print(f"[tzitzimime] done, {triangles} triangles")
