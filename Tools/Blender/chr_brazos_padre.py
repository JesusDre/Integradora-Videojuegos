"""The Padre's first-person arms (issue #28): black cassock sleeves, white cuffs and hands.

Built in camera space: the camera is at the origin looking along -Y (Blender's front view), so the
shoulders sit just behind and below it and the hands appear at the bottom of the screen. Unity puts the
model under the camera with no offset. Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/chr_brazos_padre.py
Outputs Assets/_Project/Art/Models/Characters/chr_brazos_padre.fbx with the clips Idle, Attack, Pray,
Throw and PickUp. The bone "RightGrip" marks the palm: held weapons are attached there in Unity.
"""

import math
import os
import sys

sys.path.append(os.path.dirname(__file__))

import lowpoly_humanoid as lp  # noqa: E402
from mathutils import Euler  # noqa: E402

lp.reset_scene()

sleeve = lp.make_material("Brazos_Sotana", "#16161A", roughness=0.95)
cuff = lp.make_material("Brazos_Puno", "#E8E4DA", roughness=0.8)
skin = lp.make_material("Brazos_Piel", "#C49A74", roughness=0.7)
rosary = lp.make_material("Brazos_Rosario", "#3A2616", roughness=0.6)

# Right side is -X (the view faces -Y). Points in meters from the camera: the hands rest in the lower
# third of the screen (about 18° below the center of the view).
SHOULDER = (-0.23, 0.06, -0.27)
ELBOW = (-0.20, -0.20, -0.27)
WRIST = (-0.15, -0.43, -0.15)
HAND_END = (-0.13, -0.54, -0.12)


def mirror(p):
    return (-p[0], p[1], p[2])


def lerp(a, c, t):
    return tuple(a[i] + (c[i] - a[i]) * t for i in range(3))


def add(p, d):
    return tuple(p[i] + d[i] for i in range(3))


bones = {"Arms": ((0, 0, 0), (0, -0.05, 0), None)}
for side, f in (("Right", lambda p: p), ("Left", mirror)):
    bones[f"{side}UpperArm"] = (f(SHOULDER), f(ELBOW), "Arms")
    bones[f"{side}LowerArm"] = (f(ELBOW), f(WRIST), f"{side}UpperArm")
    bones[f"{side}Hand"] = (f(WRIST), f(HAND_END), f"{side}LowerArm")
# The palm of the right hand, pointing forward: held weapons hang from it.
PALM = lerp(WRIST, HAND_END, 0.45)
bones["RightGrip"] = (PALM, add(PALM, (0, -0.1, 0)), "RightHand")
rig = lp.build_armature("BrazosPadre", bones)

b = lp.PartBuilder()
for side, f in (("Right", lambda p: p), ("Left", mirror)):
    cuff_start, cuff_end = lerp(ELBOW, WRIST, 0.86), lerp(ELBOW, WRIST, 0.97)
    b.tube(f(SHOULDER), f(ELBOW), 0.055, 0.05, sleeve, f"{side}UpperArm", segments=10)
    b.sphere(f(ELBOW), 0.05, sleeve, f"{side}UpperArm", segments=10, rings=6)
    b.tube(f(ELBOW), f(cuff_start), 0.05, 0.047, sleeve, f"{side}LowerArm", segments=10)
    b.tube(f(cuff_start), f(cuff_end), 0.05, 0.05, cuff, f"{side}LowerArm", segments=10)
    b.tube(f(cuff_end), f(WRIST), 0.026, 0.024, skin, f"{side}Hand", segments=8)
    # Hand: a single closed fist (one block) that grips the weapon; simple and readable at any distance.
    b.box(f(lerp(WRIST, HAND_END, 0.55)), (0.085, 0.1, 0.08), skin, f"{side}Hand")
# A wooden rosary wrapped around the left wrist: a detail the player sees all the time.
bead_center = mirror(lerp(ELBOW, WRIST, 0.93))
for i in range(8):
    a = 2 * math.pi * i / 8
    b.sphere(add(bead_center, (0.055 * math.cos(a), 0, 0.055 * math.sin(a))), 0.008, rosary, "LeftLowerArm", segments=6, rings=4)
mesh, triangles = b.finish("BrazosPadre", rig)

# ---- Animations (aims are world directions: -Y forward, +Z up, -X the Padre's right).
anim = lp.Animator(rig)
REST = {}  # rest pose: the arms as modeled (held forward)


def at(frame, aims=None, rotations=None):
    anim.pose(frame, rotations or {}, aims=aims or REST)


# Idle: a slow breath, the hands rise and fall a couple of centimeters.
anim.new_clip("Idle", 60, loop=True)
at(1)
at(30, rotations={"Arms": (2, 0, 0)})
at(60)

# Attack: the right arm raises up and to the right, then slashes down and across to the left.
anim.new_clip("Attack", 20, loop=False)
at(1)
at(6, aims={"RightUpperArm": (-0.35, -0.55, 0.75), "RightLowerArm": (0.1, -0.55, 0.85), "RightHand": (0.3, -0.5, 0.8)})
at(10, aims={"RightUpperArm": (0.25, -0.95, 0.1), "RightLowerArm": (0.7, -0.55, 0.45), "RightHand": (0.9, -0.2, 0.4)})
at(20)

# Pray: both hands come together in front of the chest, fingers up.
anim.new_clip("Pray", 30, loop=True)
pray = {"RightUpperArm": (0.45, -0.85, 0.05), "RightLowerArm": (0.75, -0.3, 0.6), "RightHand": (0.05, -0.2, 1.0),
        "LeftUpperArm": (-0.45, -0.85, 0.05), "LeftLowerArm": (-0.75, -0.3, 0.6), "LeftHand": (-0.05, -0.2, 1.0)}
at(1, aims=pray)
at(15, aims=pray, rotations={"Arms": (-2, 0, 0)})
at(30, aims=pray)

# Throw: the right arm cocks back over the shoulder, then whips forward and releases.
anim.new_clip("Throw", 22, loop=False)
at(1)
at(7, aims={"RightUpperArm": (-0.3, 0.6, 0.75), "RightLowerArm": (0.0, 0.85, 0.5), "RightHand": (0.0, 0.9, 0.4)})
at(12, aims={"RightUpperArm": (0.05, -0.95, 0.3), "RightLowerArm": (0.1, -0.85, 0.5), "RightHand": (0.1, -0.8, 0.6)})
at(22)

# PickUp: the right hand reaches forward and down, then comes back.
anim.new_clip("PickUp", 18, loop=False)
at(1)
at(8, aims={"RightUpperArm": (-0.05, -1.0, 0.0), "RightLowerArm": (0.05, -0.95, 0.2), "RightHand": (0.05, -0.9, -0.4)})
at(18)

lp.export_fbx(rig, [mesh], "chr_brazos_padre.fbx")
print(f"[brazos] {triangles} triangles")

# ---- Preview: what the player sees, rendered from the camera at the origin (70° vertical FOV, 16:9).
import bpy  # noqa: E402

scene = bpy.context.scene
scene.render.engine = "BLENDER_WORKBENCH"
scene.display.shading.light = "STUDIO"
scene.display.shading.color_type = "MATERIAL"
scene.render.resolution_x, scene.render.resolution_y = 960, 540
world = bpy.data.worlds.new("PreviewWorld")
world.color = (0.30, 0.32, 0.35)
scene.world = world
cam_data = bpy.data.cameras.new("FirstPerson")
cam_data.sensor_fit = "VERTICAL"
cam_data.angle_y = math.radians(70)
cam_data.clip_start = 0.05
cam = bpy.data.objects.new("FirstPerson", cam_data)
bpy.context.collection.objects.link(cam)
cam.rotation_euler = (math.radians(90), 0, math.radians(180))  # look along -Y, like the player
scene.camera = cam
# Frames go to a temporary folder; docs/personajes/img/brazos_hoja.png is those six frames side by side.
import tempfile  # noqa: E402

out_dir = os.path.join(tempfile.gettempdir(), "brazos_padre_preview")
os.makedirs(out_dir, exist_ok=True)
for clip, frame in (("Idle", 1), ("Attack", 6), ("Attack", 10), ("Pray", 1), ("Throw", 12), ("PickUp", 8)):
    rig.animation_data.action = bpy.data.actions[clip]
    scene.frame_set(frame)
    scene.render.filepath = os.path.join(out_dir, f"brazos_{clip.lower()}_{frame}.png")
    bpy.ops.render.render(write_still=True)
