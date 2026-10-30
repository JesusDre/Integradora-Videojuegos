"""Shared helpers to build low-poly humanoid characters in Blender from code.

Used by the per-character scripts (chr_cultista.py, ent_llorona.py...). Run those, not this file:
    blender -b --factory-startup --python Tools/Blender/chr_cultista.py

Conventions (see docs/personajes/README.md):
- 1 Blender unit = 1 m, Z up, the character faces -Y (Blender's front view) and stands on Z = 0.
- Rest pose is a T-pose with standard bone names, so Unity maps it as Humanoid automatically.
- Rigid skinning: every mesh part follows exactly one bone (weight 1.0). Robust for low-poly and loose clothes.
"""

import math
import os

import bmesh
import bpy
from mathutils import Matrix, Quaternion, Vector

REPO_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
MODELS_DIR = os.path.join(REPO_ROOT, "Assets", "_Project", "Art", "Models", "Characters")
SHEETS_DIR = os.path.join(REPO_ROOT, "docs", "personajes", "img")


# ---------------------------------------------------------------- scene

def reset_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0
    scene.render.fps = 30
    return scene


# ---------------------------------------------------------------- materials

def make_material(name, hex_color, roughness=0.8, metallic=0.0, emission_strength=0.0):
    """Flat-colored Principled BSDF material. Unity converts it to URP/Lit on import."""
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    color = hex_to_linear(hex_color)
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = color
    bsdf.inputs["Roughness"].default_value = roughness
    bsdf.inputs["Metallic"].default_value = metallic
    if emission_strength > 0:
        bsdf.inputs["Emission Color"].default_value = color
        bsdf.inputs["Emission Strength"].default_value = emission_strength
    mat.diffuse_color = color  # what the Workbench render and the viewport show
    return mat


def hex_to_linear(hex_color):
    hex_color = hex_color.lstrip("#")
    srgb = [int(hex_color[i:i + 2], 16) / 255.0 for i in (0, 2, 4)]
    # Blender stores linear colors; the hex codes in the character sheets are sRGB.
    linear = [c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4 for c in srgb]
    return (*linear, 1.0)


# ---------------------------------------------------------------- skeleton

# name: (head, tail, parent). Left side is +X because the character faces -Y.
def humanoid_bones(hips=0.95, chest_top=1.38, shoulder_z=1.42, head_top=1.75,
                   shoulder_x=0.20, elbow_x=0.48, wrist_x=0.74, hand_x=0.86, leg_x=0.10, knee_z=0.50, ankle_z=0.10):
    spine_z = hips + 0.10
    chest_z = hips + 0.25
    neck_z = chest_top + 0.12
    bones = {
        "Hips": ((0, 0, hips), (0, 0, spine_z), None),
        "Spine": ((0, 0, spine_z), (0, 0, chest_z), "Hips"),
        "Chest": ((0, 0, chest_z), (0, 0, chest_top), "Spine"),
        "Neck": ((0, 0, chest_top), (0, 0, neck_z), "Chest"),
        "Head": ((0, 0, neck_z), (0, 0, head_top), "Neck"),
    }
    for side, sign in (("Left", 1), ("Right", -1)):
        bones[f"{side}Shoulder"] = ((sign * 0.04, 0, chest_top), (sign * shoulder_x, 0, shoulder_z), "Chest")
        bones[f"{side}UpperArm"] = ((sign * shoulder_x, 0, shoulder_z), (sign * elbow_x, 0, shoulder_z), f"{side}Shoulder")
        bones[f"{side}LowerArm"] = ((sign * elbow_x, 0, shoulder_z), (sign * wrist_x, 0, shoulder_z), f"{side}UpperArm")
        bones[f"{side}Hand"] = ((sign * wrist_x, 0, shoulder_z), (sign * hand_x, 0, shoulder_z), f"{side}LowerArm")
        bones[f"{side}UpperLeg"] = ((sign * leg_x, 0, hips - 0.03), (sign * leg_x, 0, knee_z), "Hips")
        bones[f"{side}LowerLeg"] = ((sign * leg_x, 0, knee_z), (sign * leg_x, 0, ankle_z), f"{side}UpperLeg")
        bones[f"{side}Foot"] = ((sign * leg_x, 0, ankle_z), (sign * leg_x, -0.14, 0.02), f"{side}LowerLeg")
    return bones


def build_armature(name, bones):
    arm_data = bpy.data.armatures.new(name + "_Armature")
    arm_obj = bpy.data.objects.new(name + "_Rig", arm_data)
    bpy.context.collection.objects.link(arm_obj)
    bpy.context.view_layer.objects.active = arm_obj
    bpy.ops.object.mode_set(mode="EDIT")
    created = {}
    for bone_name, (head, tail, parent) in bones.items():
        eb = arm_data.edit_bones.new(bone_name)
        eb.head = Vector(head)
        eb.tail = Vector(tail)
        eb.roll = 0.0
        created[bone_name] = eb
    for bone_name, (_, _, parent) in bones.items():
        if parent:
            created[bone_name].parent = created[parent]
            created[bone_name].use_connect = False
    bpy.ops.object.mode_set(mode="OBJECT")
    return arm_obj


# ---------------------------------------------------------------- mesh parts

class PartBuilder:
    """Collects simple shapes into one bmesh, each tagged with a material and a bone."""

    def __init__(self):
        self.bm = bmesh.new()
        self.materials = []      # list of bpy materials, index = material slot
        self.face_bone = {}      # face index -> bone name (filled when finishing)
        self._pending = []       # (faces, bone)

    def _slot(self, material):
        if material not in self.materials:
            self.materials.append(material)
        return self.materials.index(material)

    def _tag(self, geom_faces, material, bone):
        slot = self._slot(material)
        for f in geom_faces:
            f.material_index = slot
            f.smooth = False
        self._pending.append((list(geom_faces), bone))
        return geom_faces

    def tube(self, start, end, r_start, r_end, material, bone, segments=10, cap=True):
        """Tapered cylinder from start to end (cone if a radius is 0)."""
        start, end = Vector(start), Vector(end)
        axis = end - start
        length = axis.length
        result = bmesh.ops.create_cone(self.bm, cap_ends=cap, cap_tris=False, segments=segments,
                                       radius1=r_start, radius2=r_end, depth=length)
        verts = result["verts"]
        # create_cone is centered on the origin along +Z; move it to start→end.
        rot = Vector((0, 0, 1)).rotation_difference(axis.normalized()).to_matrix().to_4x4()
        mat = Matrix.Translation((start + end) / 2) @ rot
        bmesh.ops.transform(self.bm, matrix=mat, verts=verts)
        faces = {f for v in verts for f in v.link_faces}
        return self._tag(faces, material, bone)

    def box(self, center, size, material, bone, rotation=None):
        result = bmesh.ops.create_cube(self.bm, size=1.0)
        verts = result["verts"]
        mat = Matrix.Translation(Vector(center))
        if rotation is not None:
            mat = mat @ rotation.to_matrix().to_4x4()
        mat = mat @ Matrix.Diagonal((*size, 1.0))
        bmesh.ops.transform(self.bm, matrix=mat, verts=verts)
        faces = {f for v in verts for f in v.link_faces}
        return self._tag(faces, material, bone)

    def sphere(self, center, radius, material, bone, segments=10, rings=6, scale=(1, 1, 1)):
        result = bmesh.ops.create_uvsphere(self.bm, u_segments=segments, v_segments=rings, radius=radius)
        verts = result["verts"]
        mat = Matrix.Translation(Vector(center)) @ Matrix.Diagonal((*scale, 1.0))
        bmesh.ops.transform(self.bm, matrix=mat, verts=verts)
        faces = {f for v in verts for f in v.link_faces}
        return self._tag(faces, material, bone)

    def torus(self, center, major, minor, material, bone, segments=14, ring=5):
        """Ring around the vertical axis (a belt)."""
        verts = []
        rings = []
        for i in range(segments):
            a = 2 * math.pi * i / segments
            ring_verts = []
            for j in range(ring):
                b = 2 * math.pi * j / ring
                r = major + minor * math.cos(b)
                ring_verts.append(self.bm.verts.new((center[0] + r * math.cos(a), center[1] + r * math.sin(a),
                                                     center[2] + minor * math.sin(b))))
            rings.append(ring_verts)
            verts += ring_verts
        faces = []
        for i in range(segments):
            for j in range(ring):
                a, b = rings[i][j], rings[(i + 1) % segments][j]
                c, d = rings[(i + 1) % segments][(j + 1) % ring], rings[i][(j + 1) % ring]
                faces.append(self.bm.faces.new((a, b, c, d)))
        return self._tag(faces, material, bone)

    def star(self, center, radius, points, material, bone, inner_ratio=0.45, depth=0.006, facing=(0, -1, 0)):
        """Flat star badge (the cult's eight-pointed star), extruded a little so it reads from the side."""
        outline = []
        for i in range(points * 2):
            r = radius if i % 2 == 0 else radius * inner_ratio
            a = math.pi / 2 + math.pi * i / points
            outline.append(Vector((r * math.cos(a), 0.0, r * math.sin(a))))
        front = [self.bm.verts.new(p) for p in outline]
        back = [self.bm.verts.new(p + Vector((0, depth, 0))) for p in outline]
        faces = [self.bm.faces.new(front[::-1]), self.bm.faces.new(back)]
        n = len(outline)
        for i in range(n):
            faces.append(self.bm.faces.new((front[i], front[(i + 1) % n], back[(i + 1) % n], back[i])))
        rot = Vector((0, -1, 0)).rotation_difference(Vector(facing).normalized()).to_matrix().to_4x4()
        bmesh.ops.transform(self.bm, matrix=Matrix.Translation(Vector(center)) @ rot, verts=front + back)
        return self._tag(faces, material, bone)

    def delete_faces(self, faces, predicate):
        """Remove faces of a part that match predicate(face center, face normal), e.g. to open a hood."""
        doomed = [f for f in faces if predicate(f.calc_center_median(), f.normal)]
        for entry in self._pending:
            entry[0][:] = [f for f in entry[0] if f not in doomed]
        bmesh.ops.delete(self.bm, geom=doomed, context="FACES")

    def finish(self, name, armature):
        """Create the mesh object, one vertex group per bone (weight 1), and bind it to the armature."""
        bmesh.ops.recalc_face_normals(self.bm, faces=self.bm.faces)
        vert_bone = {}
        for faces, bone in self._pending:
            for f in faces:
                if f.is_valid:
                    for v in f.verts:
                        vert_bone[v] = bone
        self.bm.verts.ensure_lookup_table()
        index_bone = {v.index: b for v, b in vert_bone.items() if v.is_valid}
        mesh = bpy.data.meshes.new(name + "_Mesh")
        self.bm.to_mesh(mesh)
        self.bm.free()
        obj = bpy.data.objects.new(name, mesh)
        bpy.context.collection.objects.link(obj)
        for mat in self.materials:
            mesh.materials.append(mat)
        groups = {}
        for vi, bone in index_bone.items():
            if bone not in groups:
                groups[bone] = obj.vertex_groups.new(name=bone)
            groups[bone].add([vi], 1.0, "REPLACE")
        obj.parent = armature
        mod = obj.modifiers.new("Armature", "ARMATURE")
        mod.object = armature
        triangles = sum(len(p.vertices) - 2 for p in mesh.polygons)
        print(f"[lowpoly] {name}: {len(mesh.vertices)} vertices, {triangles} triangles, {len(groups)} bones used")
        return obj, triangles


# ---------------------------------------------------------------- animation

class Animator:
    """Keyframes pose bones with rotations expressed around WORLD axes, which is easier to reason about
    than each bone's local axes: X = left/right axis (pitch forward/back), Y = front/back axis, Z = up."""

    def __init__(self, armature):
        self.arm = armature
        self.rest = {b.name: b.matrix_local.to_quaternion() for b in armature.data.bones}
        for pb in armature.pose.bones:
            pb.rotation_mode = "QUATERNION"
        if armature.animation_data is None:
            armature.animation_data_create()

    def new_clip(self, name, frames, loop):
        action = bpy.data.actions.new(name)
        action.use_fake_user = True  # keep it even when not assigned, so the FBX exports every clip
        action.use_frame_range = True
        action.frame_start, action.frame_end = 1, frames
        action.use_cyclic = loop
        self.arm.animation_data.action = action
        for pb in self.arm.pose.bones:
            pb.rotation_quaternion = Quaternion()
            pb.location = Vector()
        return action

    @staticmethod
    def world_rot(x=0.0, y=0.0, z=0.0):
        """Rotation in degrees around world axes, applied in Y → X → Z order."""
        qx = Quaternion((1, 0, 0), math.radians(x))
        qy = Quaternion((0, 1, 0), math.radians(y))
        qz = Quaternion((0, 0, 1), math.radians(z))
        return qz @ qx @ qy

    def pose(self, frame, rotations, locations=None, aims=None):
        """rotations: {bone: (x, y, z) degrees around world axes}, applied on top of the parent's rotation.
        aims: {bone: (x, y, z) world direction} makes the bone point that way (easier for arms and legs:
        "forearm points forward and down" instead of working out angles). Bones not listed follow their
        parent. locations: {bone: (x, y, z) meters in world axes}, only meaningful for "Hips"."""
        cumulative = {}
        aims = aims or {}
        # Parents first, so each bone knows how its parent is already rotated.
        for bone in self._hierarchy_order():
            parent_rot = cumulative[bone.parent.name] if bone.parent else Quaternion()
            if bone.name in aims:
                current = parent_rot @ (bone.tail_local - bone.head_local).normalized()
                world = current.rotation_difference(Vector(aims[bone.name]).normalized())
            else:
                x, y, z = rotations.get(bone.name, (0.0, 0.0, 0.0))
                world = self.world_rot(x, y, z)
            cumulative[bone.name] = world @ parent_rot
            r = self.rest[bone.name]
            pb = self.arm.pose.bones[bone.name]
            # Blender stores pose rotations in each bone's own rest space; convert from world axes.
            pb.rotation_quaternion = r.inverted() @ parent_rot.inverted() @ world @ parent_rot @ r
            pb.keyframe_insert("rotation_quaternion", frame=frame)
            loc = (locations or {}).get(bone.name)
            pb.location = (r.inverted() @ Vector(loc)) if loc else Vector()
            pb.keyframe_insert("location", frame=frame)

    def _hierarchy_order(self):
        ordered, pending = [], [b for b in self.arm.data.bones if b.parent is None]
        while pending:
            bone = pending.pop(0)
            ordered.append(bone)
            pending.extend(bone.children)
        return ordered


def arms_down(spread=12.0, forward=0.0, elbow=0.0):
    """Upper arms hanging from the T-pose. +Y rotation lowers the left arm, -Y the right one."""
    return {
        "LeftUpperArm": (forward, 90 - spread, 0),
        "RightUpperArm": (forward, -(90 - spread), 0),
        "LeftLowerArm": (-elbow, 0, 0),
        "RightLowerArm": (-elbow, 0, 0),
    }


# ---------------------------------------------------------------- export and sheet

def export_fbx(armature, mesh_objects, filename):
    os.makedirs(MODELS_DIR, exist_ok=True)
    path = os.path.join(MODELS_DIR, filename)
    bpy.ops.object.select_all(action="DESELECT")
    armature.select_set(True)
    for obj in mesh_objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = armature
    armature.animation_data.action = None
    bpy.ops.export_scene.fbx(
        filepath=path, use_selection=True, object_types={"ARMATURE", "MESH"},
        apply_scale_options="FBX_SCALE_ALL", axis_forward="-Z", axis_up="Y",
        add_leaf_bones=False, primary_bone_axis="Y", secondary_bone_axis="X",
        bake_anim=True, bake_anim_use_all_actions=True, bake_anim_use_nla_strips=False,
        bake_anim_force_startend_keying=True, bake_anim_simplify_factor=0.0,
        mesh_smooth_type="FACE", use_armature_deform_only=True,
    )
    print(f"[lowpoly] exported {path}")
    return path


def render_sheet(target_height, filename_base, resolution=(900, 1200)):
    """Front and side orthographic renders in T-pose: the concept image for the character issues."""
    os.makedirs(SHEETS_DIR, exist_ok=True)
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_WORKBENCH"
    scene.display.shading.light = "STUDIO"
    scene.display.shading.color_type = "MATERIAL"
    scene.display.shading.show_shadows = False
    scene.display.shading.show_cavity = True
    scene.display.shading.show_specular_highlight = False
    scene.render.resolution_x, scene.render.resolution_y = resolution
    scene.render.film_transparent = False
    world = bpy.data.worlds.new("SheetWorld")
    world.color = (0.55, 0.55, 0.58)
    scene.world = world
    cam_data = bpy.data.cameras.new("SheetCam")
    cam_data.type = "ORTHO"
    cam_data.ortho_scale = target_height * 1.25
    cam = bpy.data.objects.new("SheetCam", cam_data)
    bpy.context.collection.objects.link(cam)
    scene.camera = cam
    mid = target_height / 2
    outputs = []
    for view, location, rotation in (
        ("frente", (0, -6, mid), (math.radians(90), 0, 0)),
        ("perfil", (6, 0, mid), (math.radians(90), 0, math.radians(90))),
    ):
        cam.location = location
        cam.rotation_euler = rotation
        path = os.path.join(SHEETS_DIR, f"{filename_base}_{view}.png")
        scene.render.filepath = path
        bpy.ops.render.render(write_still=True)
        outputs.append(path)
        print(f"[lowpoly] rendered {path}")
    return outputs
