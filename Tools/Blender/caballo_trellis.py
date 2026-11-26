"""The Charro's horse with the TRELLIS body (Charro issue): same skeleton and animations as ent_caballo.py.

Mixamo only rigs two-legged characters, so the horse keeps OUR quadruped rig. This runs ent_caballo.py
(skeleton, clips, export) but swaps its low-poly body for the TRELLIS model just before binding:
  1. the TRELLIS mesh is reduced to ~2,000 faces and turned to face -Y like the rig;
  2. it is stretched to the old body's box, so legs, neck and tail fall on their bones;
  3. each vertex follows its two nearest bones (weights by distance), like an automatic skin.
The output keeps the same file and bone names (ent_caballo.fbx), so the prefab, the mounted Charro and
AC_Caballo keep working. Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/caballo_trellis.py
"""

import os
import sys

import bmesh
import bpy
from mathutils import Vector
from mathutils.geometry import intersect_point_line

HERE = os.path.dirname(os.path.abspath(__file__))
GLB = os.path.join(HERE, "..", "Modelos", "caballo_trellis.glb")
FACES = 2000


def trellis_body(old_body, rig):
    """Replaces the low-poly body by the TRELLIS one, fitted to the same box and skinned to the rig."""
    lo = Vector([min((old_body.matrix_world @ v.co)[i] for v in old_body.data.vertices) for i in range(3)])
    hi = Vector([max((old_body.matrix_world @ v.co)[i] for v in old_body.data.vertices) for i in range(3)])
    bpy.data.objects.remove(old_body)

    before = set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=GLB)
    new = [o for o in bpy.data.objects if o not in before]
    body = next(o for o in new if o.type == 'MESH')
    for o in new:
        if o is not body:
            bpy.data.objects.remove(o)
    body.parent = None
    bpy.ops.object.select_all(action='DESELECT')
    bpy.context.view_layer.objects.active = body
    body.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    dec = body.modifiers.new('dec', 'DECIMATE')
    dec.ratio = min(1.0, FACES / len(body.data.polygons))
    bpy.ops.object.modifier_apply(modifier='dec')

    # Face -Y like the rig: the head is the end of the body that reaches highest.
    vs = body.data.vertices
    y_mid = (min(v.co.y for v in vs) + max(v.co.y for v in vs)) / 2
    front_top = max((v.co.z for v in vs if v.co.y < y_mid), default=0)
    back_top = max((v.co.z for v in vs if v.co.y > y_mid), default=0)
    if back_top > front_top:
        for v in vs:
            v.co.x, v.co.y = -v.co.x, -v.co.y

    # Stretch to the old body's box.
    nlo = Vector([min(v.co[i] for v in vs) for i in range(3)])
    nhi = Vector([max(v.co[i] for v in vs) for i in range(3)])
    for v in vs:
        v.co = Vector([lo[i] + (v.co[i] - nlo[i]) / (nhi[i] - nlo[i]) * (hi[i] - lo[i]) for i in range(3)])
    body.name = "Caballo"

    # Skin: every vertex follows its two nearest deforming bones (the Saddle bone only carries the rider).
    segs = [(b.name, rig.matrix_world @ b.head_local, rig.matrix_world @ b.tail_local)
            for b in rig.data.bones if b.use_deform and b.name != "Saddle"]
    groups = {name: body.vertex_groups.new(name=name) for name, _, _ in segs}
    for v in vs:
        near = []
        for name, h, t in segs:
            p, f = intersect_point_line(v.co, h, t)
            p = h if f < 0 else t if f > 1 else p
            near.append(((v.co - p).length, name))
        near.sort()
        (d1, b1), (d2, b2) = near[0], near[1]
        w1 = 1.0 / max(d1, 1e-4) ** 4
        w2 = 1.0 / max(d2, 1e-4) ** 4
        groups[b1].add([v.index], w1 / (w1 + w2), 'REPLACE')
        groups[b2].add([v.index], w2 / (w1 + w2), 'REPLACE')

    body.parent = rig
    body.modifiers.new("Armature", "ARMATURE").object = rig
    triangles = sum(len(p.vertices) - 2 for p in body.data.polygons)
    print(f"[caballo_trellis] {len(vs)} vertices, {triangles} triangles")
    return body, triangles


# Run the original horse script with two changes: the TRELLIS body, and no concept sheet render.
source = open(os.path.join(HERE, "ent_caballo.py"), encoding="utf-8").read()
source = source.replace('body, triangles = b.finish("Caballo", rig)',
                        'body, triangles = b.finish("Caballo", rig)\nbody, triangles = trellis_body(body, rig)')
source = source.replace('lp.render_sheet(2.55, "ent_caballo")', '')
sys.argv = [os.path.join(HERE, "ent_caballo.py")]
exec(compile(source, sys.argv[0], "exec"), {"__name__": "__main__", "__file__": sys.argv[0],
                                            "trellis_body": trellis_body})
