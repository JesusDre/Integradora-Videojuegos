"""Shared helpers to build static low-poly props in Blender from code (no skeleton).

Used by the prop scripts (obj_objetos_clave.py, obj_props.py). Run those, not this file:
    blender -b --factory-startup --python Tools/Blender/obj_objetos_clave.py

Conventions: 1 Blender unit = 1 m, Z up, the prop sits on Z = 0 (or is centered, for held items) and
faces -Y. Flat colors, no textures: the materials become URP/Lit when Unity imports the FBX.
"""

import math
import os

import bmesh
import bpy
from mathutils import Matrix, Vector

from lowpoly_humanoid import REPO_ROOT, hex_to_linear, make_material, reset_scene  # noqa: F401

PROPS_DIR = os.path.join(REPO_ROOT, "Assets", "_Project", "Art", "Models", "Props")
PREVIEW_DIR = os.path.join(REPO_ROOT, "docs", "img", "props")


class PropBuilder:
    """Collects simple shapes into one mesh; each shape gets a material slot."""

    def __init__(self, name):
        self.name = name
        self.bm = bmesh.new()
        self.materials = []

    def _paint(self, faces, material):
        if material not in self.materials:
            self.materials.append(material)
        slot = self.materials.index(material)
        for f in faces:
            f.material_index = slot
            f.smooth = False
        return faces

    def _new_faces(self, before):
        return [f for f in self.bm.faces if f not in before]

    def lathe(self, profile, material, segments=16, center=(0.0, 0.0), scale_y=1.0):
        """Surface of revolution around Z from (z, radius) pairs, capped at both ends."""
        before = set(self.bm.faces)
        rings = []
        for z, r in profile:
            ring = []
            for i in range(segments):
                a = 2 * math.pi * i / segments
                ring.append(self.bm.verts.new((center[0] + r * math.cos(a), center[1] + r * math.sin(a) * scale_y, z)))
            rings.append(ring)
        for lower, upper in zip(rings, rings[1:]):
            for i in range(segments):
                j = (i + 1) % segments
                self.bm.faces.new((lower[i], lower[j], upper[j], upper[i]))
        self.bm.faces.new(list(reversed(rings[0])))
        self.bm.faces.new(rings[-1])
        return self._paint(self._new_faces(before), material)

    def box(self, center, size, material, rotation=(0.0, 0.0, 0.0)):
        before = set(self.bm.faces)
        m = Matrix.Translation(center) @ _euler(rotation) @ Matrix.Diagonal((*size, 1.0))
        bmesh.ops.create_cube(self.bm, size=1.0, matrix=m)
        return self._paint(self._new_faces(before), material)

    def sphere(self, center, radius, material, segments=10, rings=6, scale=(1.0, 1.0, 1.0)):
        before = set(self.bm.faces)
        m = Matrix.Translation(center) @ Matrix.Diagonal((*scale, 1.0))
        bmesh.ops.create_uvsphere(self.bm, u_segments=segments, v_segments=rings, radius=radius, matrix=m)
        return self._paint(self._new_faces(before), material)

    def cylinder(self, center, radius, depth, material, segments=16, rotation=(0.0, 0.0, 0.0), radius_top=None):
        before = set(self.bm.faces)
        m = Matrix.Translation(center) @ _euler(rotation)
        bmesh.ops.create_cone(self.bm, cap_ends=True, segments=segments, radius1=radius,
                              radius2=radius if radius_top is None else radius_top, depth=depth, matrix=m)
        return self._paint(self._new_faces(before), material)

    def prism(self, outline, z0, z1, material, transform=None):
        """Extruded flat outline [(x, y), ...] from z0 to z1, then optionally transformed (a Matrix)."""
        before = set(self.bm.faces)
        bottom = [self.bm.verts.new((x, y, z0)) for x, y in outline]
        top = [self.bm.verts.new((x, y, z1)) for x, y in outline]
        n = len(outline)
        self.bm.faces.new(list(reversed(bottom)))
        self.bm.faces.new(top)
        for i in range(n):
            j = (i + 1) % n
            self.bm.faces.new((bottom[i], bottom[j], top[j], top[i]))
        if transform is not None:
            bmesh.ops.transform(self.bm, matrix=transform, verts=bottom + top)
        return self._paint(self._new_faces(before), material)

    def hull(self, points, material):
        """Convex hull of a point cloud: rocks, shards, crystals."""
        before = set(self.bm.faces)
        verts = [self.bm.verts.new(p) for p in points]
        result = bmesh.ops.convex_hull(self.bm, input=verts)
        # Points that ended up inside the hull are loose: remove them.
        loose = [v for v in result.get("geom_interior", []) if isinstance(v, bmesh.types.BMVert)]
        bmesh.ops.delete(self.bm, geom=loose, context="VERTS")
        return self._paint(self._new_faces(before), material)

    def finish(self):
        bmesh.ops.recalc_face_normals(self.bm, faces=self.bm.faces[:])
        mesh = bpy.data.meshes.new(self.name + "_Mesh")
        self.bm.to_mesh(mesh)
        self.bm.free()
        obj = bpy.data.objects.new(self.name, mesh)
        bpy.context.collection.objects.link(obj)
        for mat in self.materials:
            mesh.materials.append(mat)
        triangles = sum(len(p.vertices) - 2 for p in mesh.polygons)
        print(f"[props] {self.name}: {len(mesh.vertices)} vertices, {triangles} triangles")
        return obj, triangles


def star_outline(points, radius, inner_ratio=0.45, rotation=0.0):
    """Flat star outline (the cult's eight-pointed star when points = 8)."""
    out = []
    for i in range(points * 2):
        r = radius if i % 2 == 0 else radius * inner_ratio
        a = rotation + math.pi * i / points
        out.append((r * math.cos(a), r * math.sin(a)))
    return out


def export_static(obj, filename):
    """One FBX per prop, so each can be placed on its own in Unity."""
    os.makedirs(PROPS_DIR, exist_ok=True)
    path = os.path.join(PROPS_DIR, filename)
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.export_scene.fbx(
        filepath=path, use_selection=True, object_types={"MESH"},
        apply_scale_options="FBX_SCALE_ALL", axis_forward="-Z", axis_up="Y",
        mesh_smooth_type="FACE", bake_anim=False,
    )
    print(f"[props] exported {path}")
    return path


def render_preview(objects, filename, resolution=(1400, 600)):
    """All the props side by side, lit flat: the image for docs/DEFENSA.md."""
    os.makedirs(PREVIEW_DIR, exist_ok=True)
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_WORKBENCH"
    scene.display.shading.light = "STUDIO"
    scene.display.shading.color_type = "MATERIAL"
    scene.display.shading.show_cavity = True
    scene.render.resolution_x, scene.render.resolution_y = resolution
    world = bpy.data.worlds.new("PreviewWorld")
    world.color = (0.55, 0.55, 0.58)
    scene.world = world
    # Sizes (dimensions) are only computed after the scene updates.
    bpy.context.view_layer.update()
    # Spread the props along X with some space between them, standing on Z = 0.
    x = 0.0
    for obj in objects:
        width = obj.dimensions.x
        obj.location.x = x + width / 2
        x += width + 0.15
    for obj in objects:
        obj.location.x -= x / 2
    tallest = max(o.dimensions.z for o in objects)
    cam_data = bpy.data.cameras.new("PreviewCam")
    cam_data.type = "ORTHO"
    cam_data.ortho_scale = max(x * 1.1, tallest * 1.3 * resolution[0] / resolution[1])
    cam = bpy.data.objects.new("PreviewCam", cam_data)
    bpy.context.collection.objects.link(cam)
    scene.camera = cam
    cam.location = (0, -5, tallest / 2)
    cam.rotation_euler = (math.radians(90), 0, 0)
    path = os.path.join(PREVIEW_DIR, filename)
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)
    print(f"[props] preview {path}")


def _euler(rotation):
    rx, ry, rz = (math.radians(a) for a in rotation)
    return (Matrix.Rotation(rz, 4, "Z") @ Matrix.Rotation(ry, 4, "Y") @ Matrix.Rotation(rx, 4, "X"))


def vec(*a):
    return Vector(a)
