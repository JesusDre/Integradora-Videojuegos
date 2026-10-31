"""The weapons and ammo added in #135: pistola, metralleta, ballesta, martillo chillón, caja de balas
and virotes.

Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/obj_armas_nuevas.py
Outputs Props/obj_<name>.fbx and the preview docs/img/props/armas_nuevas.png.
Same conventions as obj_props.py: the guns point along +Y (the barrel forward, like in first person),
so their first-person view is placed the same way as the escopeta's; the martillo lies along +X with
its handle from 0, like the machete, so it sits in the fist the same way.
"""

import os
import sys

sys.path.append(os.path.dirname(__file__))

import lowpoly_props as lp  # noqa: E402

lp.reset_scene()

gun_metal = lp.make_material("Pavon", "#2A2C30", roughness=0.75, metallic=0.25)
steel = lp.make_material("Acero", "#8A8C8E", roughness=0.4, metallic=0.85)
grip = lp.make_material("Cacha", "#3A2618", roughness=0.85)
wood = lp.make_material("Madera", "#5A3A22", roughness=0.8)
string = lp.make_material("Cuerda", "#D8CCB0", roughness=0.9)
toy_red = lp.make_material("JugueteRojo", "#C42A22", roughness=0.55)
toy_yellow = lp.make_material("JugueteAmarillo", "#F0C828", roughness=0.5)
holy_glow = lp.make_material("Bendito", "#FFE08A", roughness=0.4, emission_strength=2.5)
brass = lp.make_material("Laton", "#C8A040", roughness=0.35, metallic=0.9)
box_card = lp.make_material("Carton", "#8A6A42", roughness=0.95)
feather = lp.make_material("Pluma", "#9A1A14", roughness=0.9)

made = []


def done(b, filename):
    obj, _ = b.finish()
    lp.export_static(obj, filename)
    made.append(obj)
    return obj


# ---- Pistola (0.22 m): slide, barrel, grip leaning back.
b = lp.PropBuilder("Pistola")
b.box((0.0, 0.03, 0.03), (0.032, 0.20, 0.035), gun_metal)                      # slide
b.cylinder((0.0, 0.14, 0.032), 0.007, 0.03, steel, segments=8, rotation=(90, 0, 0))   # muzzle
b.box((0.0, -0.045, -0.03), (0.03, 0.045, 0.09), grip, rotation=(-14, 0, 0))   # grip
b.box((0.0, 0.01, -0.012), (0.01, 0.04, 0.02), gun_metal)                      # trigger guard
done(b, "obj_pistola.fbx")

# ---- Metralleta (0.55 m): receiver, barrel with shroud, straight magazine, front grip and short stock.
b = lp.PropBuilder("Metralleta")
b.box((0.0, 0.0, 0.0), (0.05, 0.30, 0.06), gun_metal)                         # receiver
b.cylinder((0.0, 0.22, 0.008), 0.016, 0.16, gun_metal, segments=10, rotation=(90, 0, 0))   # shroud
b.cylinder((0.0, 0.31, 0.008), 0.008, 0.04, steel, segments=8, rotation=(90, 0, 0))        # muzzle
b.box((0.0, 0.06, -0.09), (0.03, 0.04, 0.13), gun_metal)                       # magazine
b.box((0.0, -0.06, -0.06), (0.032, 0.04, 0.09), grip, rotation=(-12, 0, 0))    # grip
b.box((0.0, -0.21, -0.01), (0.03, 0.14, 0.03), gun_metal)                      # stock bar
b.box((0.0, -0.28, -0.03), (0.035, 0.02, 0.08), gun_metal)                     # butt plate
done(b, "obj_metralleta.fbx")

# ---- Ballesta (0.6 m): wooden stock, a bow across the front, the string and a loaded bolt.
b = lp.PropBuilder("Ballesta")
b.box((0.0, 0.0, 0.0), (0.04, 0.52, 0.045), wood)                              # stock
b.box((0.0, -0.18, -0.04), (0.035, 0.12, 0.07), wood, rotation=(-10, 0, 0))    # butt
for side in (-1, 1):
    b.box((side * 0.12, 0.24, 0.01), (0.24, 0.025, 0.02), steel, rotation=(0, 0, side * -18))  # bow arms
    b.box((side * 0.12, 0.17, 0.012), (0.004, 0.16, 0.004), string, rotation=(0, 0, side * -52))  # string
b.box((0.0, 0.10, 0.03), (0.008, 0.30, 0.008), wood)                           # bolt shaft
b.prism([(-0.012, 0.0), (0.012, 0.0), (0.0, 0.03)], -0.004, 0.004, steel,
        transform=lp.Matrix.Translation((0.0, 0.25, 0.03)))                     # bolt tip
done(b, "obj_ballesta.fbx")

# ---- Martillo chillón (0.5 m along +X): a squeaky toy hammer, red head with yellow caps, and a
# golden glow ring around the head (it is blessed). Handle from 0 to 0.32, like the machete's grip.
b = lp.PropBuilder("Martillo")
b.cylinder((0.18, 0.0, 0.0), 0.018, 0.36, toy_yellow, segments=10, rotation=(0, 90, 0))   # handle
b.cylinder((0.42, 0.0, 0.0), 0.06, 0.20, toy_red, segments=14, rotation=(90, 0, 0))       # head
for side in (-1, 1):
    b.cylinder((0.42, side * 0.105, 0.0), 0.062, 0.02, toy_yellow, segments=14, rotation=(90, 0, 0))  # caps
b.cylinder((0.42, 0.0, 0.0), 0.075, 0.012, holy_glow, segments=16, rotation=(0, 90, 0))   # blessed ring
done(b, "obj_martillo.fbx")

# ---- Caja de balas (normal ammo, 0.12 m) and virotes (crossbow bolts, a bundle of three).
b = lp.PropBuilder("CajaBalas")
b.box((0.0, 0.0, 0.03), (0.12, 0.08, 0.06), box_card)
for i in range(4):
    b.cylinder((-0.045 + i * 0.03, 0.0, 0.07), 0.007, 0.03, brass, segments=8)
done(b, "obj_caja_balas.fbx")

b = lp.PropBuilder("Virotes")
for i, x in enumerate((-0.015, 0.0, 0.015)):
    b.box((x, 0.0, 0.01 + i * 0.004), (0.006, 0.32, 0.006), wood)
    b.prism([(-0.008, 0.0), (0.008, 0.0), (0.0, 0.02)], -0.003, 0.003, steel,
            transform=lp.Matrix.Translation((x, 0.16, 0.01 + i * 0.004)))
    b.box((x, -0.15, 0.01 + i * 0.004), (0.016, 0.03, 0.003), feather)
done(b, "obj_virotes.fbx")

lp.render_preview(made, "armas_nuevas.png", resolution=(1800, 600))
