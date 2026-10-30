"""Crafting and combat props (GDD §18, issue #19): machete, escopeta, botellas, veladora, sal, cruz de
plata, chancla, copal and venda. Low-poly, flat colors, real-world size (1 unit = 1 m).

Run from the repo root:
    blender -b --factory-startup --python Tools/Blender/obj_props.py
Outputs one FBX per prop in Assets/_Project/Art/Models/Props/ and docs/img/props/props.png.
"""

import math
import os
import sys

sys.path.append(os.path.dirname(__file__))

import lowpoly_props as lp  # noqa: E402

lp.reset_scene()

steel = lp.make_material("Acero", "#8A8C8E", roughness=0.4, metallic=0.85)
dark_steel = lp.make_material("AceroOscuro", "#3A3C40", roughness=0.45, metallic=0.8)
wood = lp.make_material("Madera", "#5A3A22", roughness=0.8)
# Matte: the flashlight is a few centimetres from the gun in first person and a shiny metal turns white.
gun_metal = lp.make_material("Pavon", "#2A2C30", roughness=0.75, metallic=0.25)
handle = lp.make_material("Mango", "#2E1F14", roughness=0.85)
glass = lp.make_material("Vidrio", "#5E7A5C", roughness=0.15)
glass_holy = lp.make_material("VidrioAguaBendita", "#9CC8E8", roughness=0.1, emission_strength=0.4)
glass_amber = lp.make_material("VidrioMezcal", "#B07A2A", roughness=0.15)
cork = lp.make_material("Corcho", "#9C7A50", roughness=0.9)
rag = lp.make_material("Trapo", "#B8A88A", roughness=1.0)
wax = lp.make_material("Cera", "#EFE6D2", roughness=0.7)
red_glass = lp.make_material("VidrioRojo", "#9A1A14", roughness=0.2, emission_strength=0.3)
flame = lp.make_material("Flama", "#FFB040", roughness=0.5, emission_strength=6.0)
sack = lp.make_material("Costal", "#CFC8B8", roughness=1.0)
string = lp.make_material("Cordel", "#6B5236", roughness=0.9)
silver = lp.make_material("Plata", "#D8DCE0", roughness=0.25, metallic=0.95)
rubber = lp.make_material("Hule", "#1E3A6E", roughness=0.8)
strap = lp.make_material("Correa", "#E0E0E0", roughness=0.8)
clay = lp.make_material("Barro", "#8A4A2A", roughness=0.9)
ember = lp.make_material("Brasas", "#FF4A10", roughness=0.6, emission_strength=4.0)
resin = lp.make_material("Copal", "#E8D9A0", roughness=0.5)
cloth = lp.make_material("Venda", "#F2EEE6", roughness=0.95)

made = []


def done(builder, filename):
    obj, _ = builder.finish()
    lp.export_static(obj, filename)
    made.append(obj)
    return obj


# ---- Machete (0.60 m), lying flat: the blade along +X.
b = lp.PropBuilder("Machete")
b.prism([(0.0, 0.0), (0.42, -0.005), (0.46, 0.02), (0.44, 0.045), (0.0, 0.04)], 0.0, 0.004, steel,
        transform=lp.Matrix.Translation((0.14, -0.02, 0.0)))
b.box((0.07, 0.0, 0.0), (0.14, 0.03, 0.024), handle)
b.box((0.142, 0.0, 0.0), (0.012, 0.05, 0.03), dark_steel)
done(b, "obj_machete.fbx")

# ---- Escopeta de dos cañones (1.0 m), along +Y (the barrels point forward, like in first person).
b = lp.PropBuilder("Escopeta")
for side in (-1, 1):
    b.cylinder((side * 0.014, 0.32, 0.0), 0.013, 0.62, gun_metal, segments=10, rotation=(90, 0, 0))
b.box((0.0, 0.0, -0.005), (0.05, 0.12, 0.05), gun_metal)                       # action
b.box((0.0, 0.18, -0.025), (0.045, 0.32, 0.03), wood)                            # fore-end
b.box((0.0, -0.22, -0.035), (0.042, 0.32, 0.07), wood, rotation=(-8, 0, 0))      # stock
b.box((0.0, -0.04, -0.045), (0.012, 0.05, 0.03), gun_metal)                     # trigger guard
done(b, "obj_escopeta.fbx")


# ---- Botellas (0.25 m): empty (material), holy water and molotov.
def bottle(name, body, with_rag, filename):
    # The glass color tells them apart from far away: green empty, pale blue holy water, amber molotov.
    b = lp.PropBuilder(name)
    b.lathe([(0.0, 0.035), (0.15, 0.036), (0.18, 0.02), (0.22, 0.013), (0.24, 0.013)], body, segments=12)
    if with_rag:
        b.box((0.0, 0.0, 0.255), (0.03, 0.03, 0.04), rag, rotation=(0, 15, 0))
        b.box((0.012, 0.0, 0.22), (0.012, 0.03, 0.06), rag, rotation=(0, -20, 0))
    else:
        b.cylinder((0.0, 0.0, 0.25), 0.011, 0.02, cork, segments=8)
    if body is glass_holy:
        b.box((0.0, -0.036, 0.09), (0.006, 0.004, 0.05), silver)   # a small cross on the glass
        b.box((0.0, -0.036, 0.1), (0.03, 0.004, 0.006), silver)
    return done(b, filename)


bottle("Botella", glass, False, "obj_botella.fbx")
bottle("BotellaAguaBendita", glass_holy, False, "obj_botella_agua.fbx")
bottle("BotellaMolotov", glass_amber, True, "obj_botella_molotov.fbx")

# ---- Veladora (0.20 m): tall red glass with white wax and a flame.
b = lp.PropBuilder("Veladora")
b.lathe([(0.0, 0.03), (0.18, 0.032)], red_glass, segments=12)
b.lathe([(0.0, 0.027), (0.17, 0.028)], wax, segments=12)
b.cylinder((0.0, 0.0, 0.18), 0.002, 0.02, handle, segments=4)
b.lathe([(0.185, 0.006), (0.2, 0.004), (0.215, 0.0)], flame, segments=6)
done(b, "obj_veladora.fbx")

# ---- Sal (0.25 m): a small cloth sack tied at the top.
b = lp.PropBuilder("Sal")
b.lathe([(0.0, 0.07), (0.05, 0.095), (0.15, 0.085), (0.2, 0.04), (0.21, 0.03)], sack, segments=10)
b.lathe([(0.2, 0.034), (0.215, 0.034)], string, segments=10)
b.lathe([(0.21, 0.025), (0.25, 0.04)], sack, segments=10)
done(b, "obj_sal.fbx")

# ---- Cruz de plata (0.36 m): a handheld silver cross with a wooden grip.
b = lp.PropBuilder("CruzPlata")
b.box((0.0, 0.0, 0.2), (0.035, 0.02, 0.26), silver)
b.box((0.0, 0.0, 0.25), (0.16, 0.02, 0.035), silver)
b.cylinder((0.0, 0.0, 0.035), 0.015, 0.09, wood, segments=8)
b.sphere((0.0, 0.0, 0.33), 0.014, silver, segments=8, rings=4)
done(b, "obj_cruz.fbx")

# ---- Chancla (0.27 m): a rubber flip-flop, lying flat.
b = lp.PropBuilder("Chancla")
sole = [(0.0, -0.13), (0.035, -0.125), (0.045, -0.06), (0.04, 0.02), (0.05, 0.09), (0.03, 0.135),
        (-0.03, 0.135), (-0.05, 0.09), (-0.04, 0.02), (-0.045, -0.06), (-0.035, -0.125)]
b.prism(sole, 0.0, 0.018, rubber)
b.box((0.0, 0.07, 0.03), (0.008, 0.008, 0.03), strap)
b.box((0.025, 0.03, 0.028), (0.008, 0.08, 0.008), strap, rotation=(0, 0, -30))
b.box((-0.025, 0.03, 0.028), (0.008, 0.08, 0.008), strap, rotation=(0, 0, 30))
done(b, "obj_chancla.fbx")

# ---- Copal (0.18 m): a clay sahumerio with a handle, embers and resin on top.
b = lp.PropBuilder("Copal")
b.lathe([(0.0, 0.04), (0.04, 0.05), (0.08, 0.07), (0.11, 0.075)], clay, segments=12)
b.lathe([(0.1, 0.065), (0.112, 0.065)], ember, segments=12)
b.cylinder((0.11, 0.0, 0.08), 0.012, 0.1, clay, segments=6, rotation=(0, 90, 0))
for x, y in ((0.015, 0.01), (-0.02, 0.015), (0.0, -0.02)):
    b.sphere((x, y, 0.12), 0.014, resin, segments=6, rings=4)
done(b, "obj_copal.fbx")

# ---- Venda (0.10 m): a rolled bandage with the end hanging.
b = lp.PropBuilder("Venda")
b.cylinder((0.0, 0.0, 0.035), 0.035, 0.07, cloth, segments=12, rotation=(90, 0, 0))
b.box((0.0, -0.045, 0.012), (0.06, 0.03, 0.004), cloth)
done(b, "obj_venda.fbx")

lp.render_preview(made, "props.png", resolution=(1800, 500))
