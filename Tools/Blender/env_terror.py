"""Horror ambience pieces (issue #109): creepier dolls and vegetation for the fog.

Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/env_terror.py
Outputs:
- Props/obj_muneca.fbx (the key item, now the creepy doll; it stays pale so it's seen from far away);
- Environment/Comun/env_muneca_colgada.fbx, env_cabezas.fbx, env_muneca_rota.fbx (decoration);
- Environment/Comun/env_pasto.fbx, env_pasto_seco.fbx, env_arbusto.fbx, env_arbol_seco.fbx;
- docs/img/props/terror.png (preview).
"""

import math
import os
import random
import sys

sys.path.append(os.path.dirname(__file__))

import lowpoly_props as lp  # noqa: E402

lp.reset_scene()
random.seed(109)
PROPS = lp.PROPS_DIR
COMMON = os.path.join(lp.REPO_ROOT, "Assets", "_Project", "Art", "Models", "Environment", "Comun")

skin = lp.make_material("Muneca_Porcelana", "#CDBFA8", roughness=0.6)
grime = lp.make_material("Muneca_Mugre", "#5A4E3E", roughness=1.0)
dress = lp.make_material("Muneca_Vestido", "#9A907C", roughness=0.95)
socket = lp.make_material("Muneca_Cuenca", "#050404", roughness=1.0)
tears = lp.make_material("Muneca_Lagrimas", "#5A0A08", roughness=0.8, emission_strength=0.4)
hair = lp.make_material("Muneca_Pelo", "#15100D", roughness=1.0)
ribbon = lp.make_material("Muneca_Liston", "#6A1010", roughness=0.8)
rope = lp.make_material("Cuerda", "#5E4A32", roughness=1.0)
grass = lp.make_material("Pasto", "#34462A", roughness=1.0)
grass_tip = lp.make_material("PastoPunta", "#4E5E36", roughness=1.0)
dry = lp.make_material("PastoSeco", "#8A7646", roughness=1.0)
bush = lp.make_material("Arbusto", "#263826", roughness=1.0)
dead_wood = lp.make_material("MaderaMuerta", "#3A332C", roughness=1.0)

made = []


def tilt(p, center, degrees):
    """Rotate point p around the X axis through center (the head leaning to one side)."""
    a = math.radians(degrees)
    x, y, z = p[0] - center[0], p[1] - center[1], p[2] - center[2]
    return (center[0] + x * math.cos(a) - z * math.sin(a), center[1] + y, center[2] + x * math.sin(a) + z * math.cos(a))


def doll(b, base_z=0.0, head_tilt=22.0, one_arm=False, lean=0.0):
    """A creepy porcelain doll, feet at base_z: dirty torn dress, black eye sockets, red tear stains,
    cracks, a tilted head and matted hair."""
    z = base_z
    b.lathe([(z + 0.0, 0.11), (z + 0.03, 0.115), (z + 0.06, 0.1), (z + 0.16, 0.075), (z + 0.22, 0.05), (z + 0.24, 0.04)], dress, segments=9)
    b.lathe([(z + 0.0, 0.112), (z + 0.025, 0.116)], grime, segments=9)              # muddy hem
    b.lathe([(z + 0.15, 0.078), (z + 0.165, 0.078)], ribbon, segments=9)
    b.cylinder((0, 0, z + 0.255), 0.022, 0.04, skin, segments=7)
    neck = (0, 0, z + 0.27)
    head = tilt((0, 0, z + 0.32), neck, head_tilt)
    b.sphere(head, 0.065, skin, segments=10, rings=7)
    b.sphere(tilt((0, 0.014, z + 0.34), neck, head_tilt), 0.069, hair, segments=10, rings=7, scale=(1.0, 1.0, 0.8))
    for side in (-1, 1):
        # Hollow black sockets instead of eyes, and a red stain running down from each.
        b.sphere(tilt((side * 0.025, -0.056, z + 0.328), neck, head_tilt), 0.017, socket, segments=6, rings=4)
        b.box(tilt((side * 0.025, -0.062, z + 0.29), neck, head_tilt), (0.006, 0.004, 0.05), tears)
        b.box(tilt((side * 0.062, 0.012, z + 0.255), neck, head_tilt), (0.03, 0.03, 0.15), hair)       # matted locks
    b.box(tilt((0.012, -0.062, z + 0.36), neck, head_tilt), (0.03, 0.003, 0.004), grime)              # cracks
    b.box(tilt((0.03, -0.06, z + 0.345), neck, head_tilt), (0.004, 0.003, 0.03), grime)
    b.box(tilt((0.0, -0.064, z + 0.295), neck, head_tilt), (0.025, 0.004, 0.006), socket)             # a thin mouth
    for side in ((-1,) if one_arm else (-1, 1)):
        b.cylinder((side * 0.09, 0, z + 0.17), 0.016, 0.13, skin, segments=6, rotation=(0, side * (25 + lean), 0))
    for side in (-1, 1):
        b.cylinder((side * 0.035, 0, z - 0.03), 0.018, 0.08, skin, segments=6)
        b.sphere((side * 0.035, 0, z - 0.07), 0.02, grime, segments=6, rings=4)                        # dirty feet


def done(b, folder, filename, preview=True):
    obj, _ = b.finish()
    lp.PROPS_DIR = folder
    lp.export_static(obj, filename)
    if preview:
        made.append(obj)
    return obj


# ---- Key item: the creepy doll standing, feet at 0 (the pickup floats it a little).
b = lp.PropBuilder("Muneca")
doll(b, base_z=0.07)
done(b, PROPS, "obj_muneca.fbx")

# ---- Hanging doll: pivot at the top of the rope (z = 0), the doll dangles below, one arm torn off.
b = lp.PropBuilder("MunecaColgada")
b.cylinder((0, 0, -0.2), 0.006, 0.4, rope, segments=4)
doll(b, base_z=-0.82, head_tilt=35.0, one_arm=True, lean=15)
done(b, COMMON, "env_muneca_colgada.fbx")

# ---- A string of doll heads hanging from a wire (pivot at the top).
b = lp.PropBuilder("Cabezas")
b.cylinder((0, 0, -0.5), 0.005, 1.0, rope, segments=4)
for i in range(4):
    zc = -0.18 - i * 0.24
    off = (random.uniform(-0.04, 0.04), random.uniform(-0.03, 0.03))
    b.sphere((off[0], off[1], zc), 0.06, skin, segments=9, rings=6)
    b.sphere((off[0], off[1] + 0.013, zc + 0.015), 0.063, hair, segments=9, rings=6, scale=(1, 1, 0.75))
    for side in (-1, 1):
        b.sphere((off[0] + side * 0.022, off[1] - 0.052, zc + 0.008), 0.015, socket, segments=6, rings=4)
done(b, COMMON, "env_cabezas.fbx")

# ---- A broken doll lying in the mud, its head apart from the body.
b = lp.PropBuilder("MunecaRota")
b.lathe([(0.0, 0.1), (0.05, 0.09), (0.15, 0.06), (0.22, 0.04)], dress, segments=9)
for v in list(b.bm.verts):
    x, y, zz = v.co
    v.co = (x, -zz + 0.12, y * 0.5 + 0.05)          # lay the body down along -Y
b.cylinder((0.07, -0.02, 0.03), 0.016, 0.13, skin, segments=6, rotation=(0, 80, 20))
b.sphere((0.16, 0.05, 0.06), 0.065, skin, segments=10, rings=7)                       # the head, rolled away
b.sphere((0.17, 0.065, 0.075), 0.068, hair, segments=10, rings=7, scale=(1, 1, 0.8))
b.sphere((0.135, 0.002, 0.08), 0.017, socket, segments=6, rings=4)
b.box((0.135, -0.002, 0.045), (0.005, 0.004, 0.04), tears)
done(b, COMMON, "env_muneca_rota.fbx")


# ---- Grass clumps (about 0.5 m): thin tapered blades leaning outward.
def grass_clump(name, base, tip, blades, height, filename):
    b = lp.PropBuilder(name)
    for _ in range(blades):
        ang = random.uniform(0, 2 * math.pi)
        r = random.uniform(0.0, 0.18)
        h = random.uniform(height * 0.5, height)
        lean = random.uniform(8, 28)
        b.cylinder((math.cos(ang) * r, math.sin(ang) * r, h / 2), 0.018, h, base if random.random() < 0.6 else tip,
                   segments=3, radius_top=0.002, rotation=(math.sin(ang) * lean, -math.cos(ang) * lean, 0))
    return done(b, COMMON, filename)


grass_clump("Pasto", grass, grass_tip, 14, 0.6, "env_pasto.fbx")
grass_clump("PastoSeco", dry, dry, 12, 0.55, "env_pasto_seco.fbx")

# ---- Bush (1 m): a few squashed blobs.
b = lp.PropBuilder("Arbusto")
for _ in range(5):
    c = (random.uniform(-0.35, 0.35), random.uniform(-0.35, 0.35), random.uniform(0.3, 0.55))
    b.sphere(c, random.uniform(0.3, 0.45), bush, segments=7, rings=5, scale=(1, 1, 0.7))
done(b, COMMON, "env_arbusto.fbx")

# ---- Dead tree (6 m): a twisted trunk with bare crooked branches, perfect against the fog.
from mathutils import Vector  # noqa: E402


def branch(b, start, direction, length, r0, r1):
    """A tapered cylinder from start, pointing along direction (a branch that really leaves the trunk)."""
    d = Vector(direction).normalized()
    rot = Vector((0, 0, 1)).rotation_difference(d).to_euler("XYZ")
    center = Vector(start) + d * (length / 2)
    b.cylinder(tuple(center), r0, length, dead_wood, segments=5, radius_top=r1,
               rotation=tuple(math.degrees(x) for x in rot))
    return tuple(Vector(start) + d * length)


b = lp.PropBuilder("ArbolSeco")
top = branch(b, (0, 0, 0), (0.05, -0.04, 1), 3.4, 0.22, 0.13)
for k in range(6):
    ang = k * 2 * math.pi / 6 + random.uniform(-0.3, 0.3)
    t = random.uniform(0.6, 1.0)
    start = (top[0] * t, top[1] * t, top[2] * t)
    rise = random.uniform(0.5, 1.1)
    end = branch(b, start, (math.cos(ang), math.sin(ang), rise), random.uniform(1.3, 2.2), 0.08, 0.035)
    branch(b, end, (math.cos(ang + 0.6), math.sin(ang + 0.6), rise + 0.4), random.uniform(0.6, 1.0), 0.035, 0.008)
done(b, COMMON, "env_arbol_seco.fbx")

lp.render_preview(made, "terror.png", resolution=(2000, 600))
