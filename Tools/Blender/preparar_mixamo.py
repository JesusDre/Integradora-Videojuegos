"""Prepares a TRELLIS model (GLB) for Mixamo or for Unity.

TRELLIS gives a heavy mesh about 1 m tall, centered on its middle. This reduces it to a face budget,
scales it to its real height, puts the origin between the feet and exports an FBX with the texture
inside (Mixamo and Unity both read it). It also renders a front view to check the result.

Run from the repo root:
    blender -b --python Tools/Blender/preparar_mixamo.py -- <name> <in.glb> <out.fbx> <preview.png> [faces] [height]
Defaults: 6000 faces, 1.8 m (a person). Used for the cultist, Llorona, Charro and Tzitzimime.
"""

import math
import sys

import bpy
from mathutils import Vector

args = sys.argv[sys.argv.index("--") + 1:]
name, src, out, png = args[:4]
faces = int(args[4]) if len(args) > 4 else 6000
height = float(args[5]) if len(args) > 5 else 1.8

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=src)
ob = [o for o in bpy.data.objects if o.type == 'MESH'][0]
for o in list(bpy.data.objects):
    if o.type == 'EMPTY':
        bpy.data.objects.remove(o)
ob.parent = None
bpy.context.view_layer.objects.active = ob
ob.select_set(True)
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

# Fewer faces: hordes and several enemies are on screen at once.
dec = ob.modifiers.new('dec', 'DECIMATE')
dec.ratio = min(1.0, faces / len(ob.data.polygons))
bpy.ops.object.modifier_apply(modifier='dec')

# Real height, origin between the feet, centered.
co = [Vector(v.co) for v in ob.data.vertices]
lo = Vector([min(c[i] for c in co) for i in range(3)])
hi = Vector([max(c[i] for c in co) for i in range(3)])
s = height / (hi.z - lo.z)
for v in ob.data.vertices:
    v.co = Vector(((v.co.x - (lo.x + hi.x) / 2) * s, (v.co.y - (lo.y + hi.y) / 2) * s, (v.co.z - lo.z) * s))
ob.name = name
print('faces', len(ob.data.polygons))

bpy.ops.export_scene.fbx(filepath=out, use_selection=True, path_mode='COPY', embed_textures=True,
                         apply_scale_options='FBX_SCALE_ALL', axis_forward='-Z', axis_up='Y')

# Front view to check it faces forward, in A-pose, with its texture.
scn = bpy.context.scene
cam = bpy.data.objects.new('cam', bpy.data.cameras.new('cam'))
scn.collection.objects.link(cam)
cam.location = (0, -4, height / 2)
cam.rotation_euler = (math.radians(90), 0, 0)
cam.data.type = 'ORTHO'
cam.data.ortho_scale = height * 1.2
scn.camera = cam
sun = bpy.data.objects.new('sun', bpy.data.lights.new('sun', 'SUN'))
scn.collection.objects.link(sun)
sun.rotation_euler = (math.radians(60), 0, 0)
scn.world = bpy.data.worlds.new('w')
scn.world.color = (0.5, 0.5, 0.5)
scn.render.resolution_x = scn.render.resolution_y = 400
scn.render.filepath = png
bpy.ops.render.render(write_still=True)
