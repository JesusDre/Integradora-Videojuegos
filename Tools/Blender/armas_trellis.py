"""Weapons from TRELLIS, fitted to the old low-poly ones (weapons issue).

Each weapon in the Padre's hand (PF_Vista_*) was placed by hand around the old model. To keep that work,
the new model is put in exactly the same frame as the old one: same length axis, handle on the same end,
same "up", same length and center. Then Unity only swaps the mesh and material, nothing moves.
Per weapon: reduce the TRELLIS mesh, rotate it into the old frame, scale it to the old length, center it
on the old box, export with the same FBX settings as lowpoly_props.export_static (plus the texture).
Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/armas_trellis.py
"""

import os

import bpy
from mathutils import Matrix, Vector

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
PROPS = os.path.join(ROOT, "Assets", "_Project", "Art", "Models", "Props")
SOURCES = os.path.join(ROOT, "Tools", "Modelos")

# name: (old model, faces, rotation from the TRELLIS frame to the old one).
# TRELLIS weapons lie along Y with the handle/stock at +Y and their top at +Z.
TURN = Matrix.Rotation(3.14159265, 3, 'Z')   # old shotgun and crossbow: stock at -Y
WEAPONS = {
    # Old machete: along X (handle at -X), lying flat, spine toward -Y and edge toward +Y.
    "machete": ("obj_machete.fbx", 800, Matrix(((0, -1, 0), (0, 0, -1), (1, 0, 0)))),
    "escopeta": ("obj_escopeta.fbx", 1200, TURN),
    "ballesta": ("obj_ballesta.fbx", 1500, TURN),
}


def box(obj):
    vs = [obj.matrix_world @ v.co for v in obj.data.vertices]
    return (Vector([min(v[i] for v in vs) for i in range(3)]), Vector([max(v[i] for v in vs) for i in range(3)]))


def build(name, old_file, faces, rot):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=os.path.join(PROPS, old_file))
    old = next(o for o in bpy.data.objects if o.type == 'MESH')
    olo, ohi = box(old)
    bpy.data.objects.remove(old)

    bpy.ops.import_scene.gltf(filepath=os.path.join(SOURCES, name + "_trellis.glb"))
    obj = next(o for o in bpy.data.objects if o.type == 'MESH')
    for o in list(bpy.data.objects):
        if o is not obj:
            bpy.data.objects.remove(o)
    obj.parent = None
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    dec = obj.modifiers.new('dec', 'DECIMATE')
    dec.ratio = min(1.0, faces / len(obj.data.polygons))
    bpy.ops.object.modifier_apply(modifier='dec')

    for v in obj.data.vertices:
        v.co = rot @ v.co
    nlo, nhi = box(obj)
    axis = max(range(3), key=lambda i: ohi[i] - olo[i])
    s = (ohi[axis] - olo[axis]) / (nhi[axis] - nlo[axis])
    shift = (olo + ohi) / 2 - (nlo + nhi) / 2 * s
    for v in obj.data.vertices:
        v.co = v.co * s + shift
    obj.name = "obj_" + name

    out_dir = os.path.join(PROPS, "Armas")
    os.makedirs(out_dir, exist_ok=True)
    bpy.ops.export_scene.fbx(filepath=os.path.join(out_dir, name + ".fbx"), use_selection=True,
                             object_types={"MESH"}, apply_scale_options="FBX_SCALE_ALL", axis_forward="-Z",
                             axis_up="Y", mesh_smooth_type="FACE", bake_anim=False,
                             path_mode='COPY', embed_textures=True)
    print(f"[armas] {name}: {len(obj.data.polygons)} faces, scale {s:.2f}")


for weapon, args in WEAPONS.items():
    build(weapon, *args)
