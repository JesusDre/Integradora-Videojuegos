"""The Padre's first-person arms cut from his full TRELLIS body (Padre model issue).

The game only shows the Padre's arms, held in front of the camera by OUR arms rig (chr_brazos_padre.py,
with the clips Idle, Attack, Pray, Throw and PickUp and the RightGrip bone that holds the weapons).
This script keeps that rig and its clips and swaps the low-poly sleeves for the real ones:
  1. it opens the Mixamo-rigged full body (Characters/Padre/padre.fbx);
  2. keeps only the vertices that mostly follow an arm bone (upper arm, forearm, hand, fingers);
  3. moves each of them from the A-pose arm to the first-person arm: every Mixamo arm bone has a frame
     (along the bone, and "outward" = the back of the hand) and so does its bone in our rig; a vertex
     keeps its place relative to its bones, blended by its Mixamo weights (like skinning);
  4. binds the result to our bones with the same weights (Arm -> UpperArm, ForeArm -> LowerArm,
     Hand and fingers -> Hand).
Output: chr_brazos_padre.fbx, same file and bone names, so the player prefab and AC_BrazosPadre keep
working. Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/brazos_padre_trellis.py
"""

import os
import sys

import bmesh
import bpy
from mathutils import Matrix, Vector

HERE = os.path.dirname(os.path.abspath(__file__))
RIGGED = os.path.join(HERE, "..", "..", "Assets", "_Project", "Art", "Models", "Characters", "Padre", "padre.fbx")
# Mixamo bone (without the "mixamorig:" prefix and the side) -> our bone (without the side).
MAP = {"Arm": "UpperArm", "ForeArm": "LowerArm", "Hand": "Hand"}


def frame(head, tail, outward):
    """A matrix whose Y runs along the bone and whose X points 'outward' (the back of the hand)."""
    y = (tail - head).normalized()
    x = (outward - y * outward.dot(y)).normalized()
    z = x.cross(y)
    m = Matrix((x, y, z)).transposed().to_4x4()
    m.translation = head
    return m


def our_bone(name):
    """Mixamo bone name -> (our bone, side), or None if it is not part of an arm."""
    if not name.startswith("mixamorig:"):
        return None
    n = name[len("mixamorig:"):]
    for side in ("Right", "Left"):
        if n.startswith(side):
            rest = n[len(side):]
            if rest.startswith("HandIndex") or rest.startswith("HandThumb") or rest.startswith("HandMiddle"):
                rest = "Hand"   # fingers move with the hand
            if rest in MAP:
                return MAP[rest], side
    return None


def trellis_arms(old_mesh, rig):
    bpy.data.objects.remove(old_mesh)
    before = set(bpy.data.objects)
    bpy.ops.import_scene.fbx(filepath=RIGGED)
    new = [o for o in bpy.data.objects if o not in before]
    mix = next(o for o in new if o.type == 'ARMATURE')
    body = next(o for o in new if o.type == 'MESH')

    # One transform per Mixamo arm bone: from its frame in the A-pose to its partner's frame in our rig.
    transforms = {}
    for b in mix.data.bones:
        hit = our_bone(b.name)
        if hit is None:
            continue
        ours, side = hit
        s = -1.0 if side == "Right" else 1.0   # the Padre's right is -X in both rigs
        src = frame(mix.matrix_world @ b.head_local, mix.matrix_world @ b.tail_local, Vector((s, 0, 0)))
        # Our bones belong to the whole chain: the fingers use the hand bone's frame.
        mb = mix.data.bones["mixamorig:" + side + {"UpperArm": "Arm", "LowerArm": "ForeArm", "Hand": "Hand"}[ours]]
        src = frame(mix.matrix_world @ mb.head_local, mix.matrix_world @ mb.tail_local, Vector((s, 0, 0)))
        tb = rig.data.bones[side + ours]
        dst = frame(rig.matrix_world @ tb.head_local, rig.matrix_world @ tb.tail_local, Vector((s, 0, 0.5)))
        transforms[b.name] = (dst @ src.inverted(), side + ours)

    names = {g.index: g.name for g in body.vertex_groups}
    mw = body.matrix_world
    bm = bmesh.new()
    bm.from_mesh(body.data)
    bm.verts.ensure_lookup_table()
    deform = bm.verts.layers.deform.active
    keep, new_weights = set(), {}
    for v in bm.verts:
        arm = {}
        total = 0.0
        for gi, w in v[deform].items():
            total += w
            if names[gi] in transforms:
                arm[names[gi]] = arm.get(names[gi], 0.0) + w
        arm_w = sum(arm.values())
        if total <= 0 or arm_w / total < 0.5:
            continue   # cassock, stole, chest...: not an arm
        world = mw @ v.co
        moved = Vector((0, 0, 0))
        ours = {}
        for name, w in arm.items():
            m, bone = transforms[name]
            moved += (m @ world) * (w / arm_w)
            ours[bone] = ours.get(bone, 0.0) + w / arm_w
        v.co = mw.inverted() @ moved
        keep.add(v.index)
        new_weights[v.index] = ours
    bmesh.ops.delete(bm, geom=[f for f in bm.faces if not all(v.index in keep for v in f.verts)], context='FACES')
    bmesh.ops.delete(bm, geom=[v for v in bm.verts if not v.link_faces], context='VERTS')

    # A clean object in world space, weighted to our bones.
    mesh = bpy.data.meshes.new("BrazosPadre_Mesh")
    old_index = bm.verts.layers.int.new("old")
    for v in bm.verts:
        pass
    bm.to_mesh(mesh)
    bm.free()
    arms = bpy.data.objects.new("BrazosPadre", mesh)
    bpy.context.collection.objects.link(arms)
    arms.matrix_world = mw
    for mat in body.data.materials:
        mesh.materials.append(mat)
    return arms, mix, body, new_weights


def bind(arms, mix, body, rig):
    """Weights by nearest-surviving original vertex: recomputed from the moved positions."""
    bpy.context.view_layer.objects.active = arms
    bpy.ops.object.select_all(action='DESELECT')
    arms.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    for o in (mix, body):
        bpy.data.objects.remove(o)
    # Each vertex to its two nearest arm bones of our rig (same idea as the horse).
    from mathutils.geometry import intersect_point_line
    segs = [(b.name, rig.matrix_world @ b.head_local, rig.matrix_world @ b.tail_local)
            for b in rig.data.bones if b.use_deform and b.name not in ("Arms", "RightGrip")]
    groups = {n: arms.vertex_groups.new(name=n) for n, _, _ in segs}
    for v in arms.data.vertices:
        side = "Right" if v.co.x < 0 else "Left"
        near = []
        for n, h, t in segs:
            if not n.startswith(side):
                continue
            p, f = intersect_point_line(v.co, h, t)
            p = h if f < 0 else t if f > 1 else p
            near.append(((v.co - p).length, n))
        near.sort()
        (d1, b1), (d2, b2) = near[0], near[1]
        w1, w2 = 1.0 / max(d1, 1e-4) ** 4, 1.0 / max(d2, 1e-4) ** 4
        groups[b1].add([v.index], w1 / (w1 + w2), 'REPLACE')
        groups[b2].add([v.index], w2 / (w1 + w2), 'REPLACE')
    arms.parent = rig
    arms.modifiers.new("Armature", "ARMATURE").object = rig
    tris = sum(len(p.vertices) - 2 for p in arms.data.polygons)
    print(f"[brazos_trellis] {len(arms.data.vertices)} vertices, {tris} triangles")
    return arms, tris


def close_right_hand(arms, rig, knuckles=0.42, curl_degrees=105.0):
    """The TRELLIS hand is open, so a weapon handle went through it. The right hand is closed into a
    fist: every vertex past the knuckles turns toward the palm around the knuckle line, more the farther
    it is (the fingertips curl the most), so the fingers wrap around the grip."""
    import math
    from mathutils import Matrix
    hand = rig.data.bones["RightHand"]
    wrist = rig.matrix_world @ hand.head_local
    tip = rig.matrix_world @ hand.tail_local
    along = (tip - wrist).normalized()
    length = (tip - wrist).length
    back = Vector((-1.0, 0, 0.5))                      # back of the right hand (outward, as in frame())
    back = (back - along * back.dot(along)).normalized()
    axis = along.cross(back).normalized()              # the knuckle line, across the hand
    pivot = wrist + along * (length * knuckles)
    # Pick the turning direction that sends the fingertips toward the palm (away from the back).
    test = Matrix.Rotation(math.radians(30), 3, axis) @ (tip - pivot)
    sign = 1.0 if test.dot(back) < (tip - pivot).dot(back) else -1.0
    for v in arms.data.vertices:
        if v.co.x > 0:
            continue   # left hand stays open
        rel = v.co - wrist
        t = rel.dot(along) / length
        if t <= knuckles or (rel - along * rel.dot(along)).length > length * 0.9:
            continue   # wrist and palm stay; the sleeve and forearm are farther from the hand axis
        k = min(1.0, (t - knuckles) / (1.0 - knuckles))
        rot = Matrix.Rotation(math.radians(curl_degrees * k) * sign, 3, axis)
        v.co = pivot + rot @ (v.co - pivot)


def make_arms(old_mesh, rig):
    arms, mix, body, _ = trellis_arms(old_mesh, rig)
    close_right_hand(arms, rig)
    return bind(arms, mix, body, rig)


# Run the original arms script with the TRELLIS sleeves in place of the low-poly ones.
source = open(os.path.join(HERE, "chr_brazos_padre.py"), encoding="utf-8").read()
source = source.replace('mesh, triangles = b.finish("BrazosPadre", rig)',
                        'mesh, triangles = b.finish("BrazosPadre", rig)\nmesh, triangles = make_arms(mesh, rig)')
sys.argv = [os.path.join(HERE, "chr_brazos_padre.py")]
exec(compile(source, sys.argv[0], "exec"), {"__name__": "__main__", "__file__": sys.argv[0], "make_arms": make_arms})
