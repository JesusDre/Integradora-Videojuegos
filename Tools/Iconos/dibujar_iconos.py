"""Inventory icons (issue #132), drawn with PIL in a single style.

Run from the repo root:
    python3 Tools/Iconos/dibujar_iconos.py
Writes Assets/_Project/UI/Iconos/ICO_<name>.png (256 x 256, transparent) and the sheet
docs/img/ui/iconos.png. Several items share a 3D model (the salt bag is the salt material, the salt
rounds and the salt line), so the icons are drawn, not rendered: each one must be recognizable at a
glance in a 64 px slot. Style: a thick dark outline, a base color with a lighter side, and a soft glow
for the blessed and the key items.
"""

import math
import os

from PIL import Image, ImageDraw, ImageFilter

S = 256
OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "_Project", "UI", "Iconos")
SHEET = os.path.join(os.path.dirname(__file__), "..", "..", "docs", "img", "ui", "iconos.png")
INK = (24, 18, 14, 255)          # outline
W = 10                           # outline width

# Palette (close to the game's: warm ocher, dark wood, candle gold, blood red, cold silver).
GOLD, GOLD_L = (214, 160, 60), (246, 208, 120)
WOOD, WOOD_L = (110, 72, 42), (150, 104, 64)
RED, RED_L = (150, 36, 30), (196, 70, 56)
SILVER, SILVER_L = (150, 156, 164), (214, 220, 226)
GLASS, GLASS_L = (86, 128, 120), (150, 196, 186)
WATER = (120, 170, 220)
CLOTH, CLOTH_L = (206, 196, 172), (236, 228, 208)
WAX, WAX_L = (226, 210, 176), (246, 236, 212)
DARK, DARK_L = (52, 46, 44), (90, 82, 78)
GUN, GUN_L = (96, 92, 92), (150, 146, 144)     # guns: lighter than DARK so they read on the dark slot
SALT, SALT_L = (220, 220, 214), (250, 250, 246)
GREEN, GREEN_L = (76, 112, 64), (116, 154, 96)
BLUE = (70, 110, 180)


def canvas():
    return Image.new("RGBA", (S, S), (0, 0, 0, 0))


def poly(d, pts, fill, light=None, outline=True):
    """Polygon with a lighter left half (the light comes from the upper left)."""
    d.polygon(pts, fill=fill + (255,), outline=INK if outline else None, width=W if outline else 0)
    if light:
        xs = [p[0] for p in pts]
        cx = (min(xs) + max(xs)) / 2
        left = [(min(x, cx), y) for x, y in pts]
        d.polygon(left, fill=light + (255,))
        d.polygon(pts, outline=INK if outline else None, width=W if outline else 0)


def ell(d, box, fill, light=None, outline=True):
    d.ellipse(box, fill=fill + (255,), outline=INK if outline else None, width=W if outline else 0)
    if light:
        x0, y0, x1, y1 = box
        d.ellipse((x0 + (x1 - x0) * 0.18, y0 + (y1 - y0) * 0.15, x0 + (x1 - x0) * 0.5, y0 + (y1 - y0) * 0.45), fill=light + (255,))


def rect(d, box, fill, light=None, r=14):
    d.rounded_rectangle(box, r, fill=fill + (255,), outline=INK, width=W)
    if light:
        x0, y0, x1, y1 = box
        d.rounded_rectangle((x0 + W, y0 + W, (x0 + x1) / 2, y1 - W), max(r - 6, 2), fill=light + (255,))
        d.rounded_rectangle(box, r, outline=INK, width=W)


def glow(img, color, radius=26, strength=1.0):
    """A soft halo behind the drawing: blessed objects and key items shine."""
    alpha = img.split()[3].filter(ImageFilter.MaxFilter(15)).filter(ImageFilter.GaussianBlur(radius))
    halo = Image.new("RGBA", img.size, color + (0,))
    halo.putalpha(alpha.point(lambda a: int(a * 0.85 * strength)))
    return Image.alpha_composite(halo, img)


def bottle(d, body, light, label=None, liquid=None):
    poly(d, [(104, 40), (152, 40), (152, 86), (184, 120), (184, 220), (72, 220), (72, 120), (104, 86)], body, light)
    rect(d, (100, 22, 156, 48), WOOD, WOOD_L, r=6)
    if liquid:
        d.polygon([(80, 150), (176, 150), (176, 212), (80, 212)], fill=liquid + (220,))
    if label:
        rect(d, (84, 138, 172, 190), *label, r=8)


def cross(d, cx, cy, s, fill, light):
    poly(d, [(cx - s * 0.2, cy - s), (cx + s * 0.2, cy - s), (cx + s * 0.2, cy - s * 0.45), (cx + s * 0.65, cy - s * 0.45),
             (cx + s * 0.65, cy - s * 0.05), (cx + s * 0.2, cy - s * 0.05), (cx + s * 0.2, cy + s), (cx - s * 0.2, cy + s),
             (cx - s * 0.2, cy - s * 0.05), (cx - s * 0.65, cy - s * 0.05), (cx - s * 0.65, cy - s * 0.45), (cx - s * 0.2, cy - s * 0.45)],
         fill, light)


def flame(d, cx, base, h, w):
    pts = [(cx + math.sin(a) * w * (1 - (a / math.pi) ** 0.5) * 0.5 if False else cx, base)]
    outer = [(cx - w / 2, base), (cx - w / 2.4, base - h * 0.5), (cx, base - h), (cx + w / 2.4, base - h * 0.5), (cx + w / 2, base)]
    d.polygon(outer, fill=(240, 150, 40, 255), outline=INK, width=6)
    d.polygon([(cx - w / 4, base - 4), (cx, base - h * 0.62), (cx + w / 4, base - 4)], fill=(255, 226, 120, 255))


def sack(d, fill, light, tie=RED):
    poly(d, [(84, 70), (172, 70), (200, 140), (196, 218), (60, 218), (56, 140)], fill, light)
    d.line([(96, 76), (160, 76)], fill=tie + (255,), width=12)
    poly(d, [(98, 40), (158, 40), (150, 72), (106, 72)], fill, light)


def save(name, img, extra_glow=None):
    if extra_glow:
        img = glow(img, extra_glow)
    os.makedirs(OUT, exist_ok=True)
    img.save(os.path.join(OUT, f"ICO_{name}.png"))
    made.append((name, img))


made = []

# ---------------------------------------------------------------- consumables and faith objects
img = canvas(); d = ImageDraw.Draw(img)
bottle(d, GLASS, GLASS_L, liquid=WATER)
cross(d, 128, 172, 26, SILVER, SILVER_L)
save("AguaBendita", img, (150, 200, 255))

img = canvas(); d = ImageDraw.Draw(img)
bottle(d, (120, 84, 50), (160, 120, 76), liquid=(200, 110, 40))
d.polygon([(110, 22), (146, 22), (140, 6), (116, 6)], fill=CLOTH + (255,), outline=INK, width=6)
flame(d, 128, 14, 34, 30)
save("Molotov", img)

img = canvas(); d = ImageDraw.Draw(img)
rect(d, (54, 96, 202, 172), CLOTH, CLOTH_L, r=36)
d.rectangle((110, 92, 146, 176), fill=RED + (255,), outline=INK, width=6)
d.rectangle((92, 116, 164, 152), fill=RED + (255,))
d.rectangle((110, 116, 146, 152), fill=RED + (255,))
save("Venda", img)

img = canvas(); d = ImageDraw.Draw(img)
rect(d, (88, 92, 168, 224), WAX, WAX_L, r=10)
d.rectangle((82, 200, 174, 232), fill=RED + (255,), outline=INK, width=8)
d.line([(128, 92), (128, 70)], fill=INK, width=6)
flame(d, 128, 74, 54, 38)
save("Veladora", img, (255, 190, 90))

img = canvas(); d = ImageDraw.Draw(img)
sack(d, SALT, SALT_L)
for x in (92, 118, 148, 170):
    d.line([(x, 226), (x + 6, 244)], fill=SALT_L + (255,), width=8)
d.line([(70, 238), (200, 238)], fill=SALT_L + (255,), width=10)
save("LineaSal", img)

img = canvas(); d = ImageDraw.Draw(img)
ell(d, (70, 120, 186, 214), DARK, DARK_L)
d.rectangle((80, 150, 176, 176), fill=GOLD + (255,), outline=INK, width=6)
for i, (x, y) in enumerate(((104, 104), (130, 84), (152, 104))):
    d.ellipse((x - 18 - i * 2, y - 40 - i * 8, x + 18 + i * 2, y), fill=(200, 196, 190, 190))
save("Copal", img, (230, 220, 200))

img = canvas(); d = ImageDraw.Draw(img)
cross(d, 128, 130, 100, SILVER, SILVER_L)
ell(d, (114, 82, 142, 110), WOOD, WOOD_L)
save("CruzPlata", img, (200, 220, 255))

img = canvas(); d = ImageDraw.Draw(img)
poly(d, [(84, 40), (156, 40), (182, 110), (176, 210), (150, 232), (106, 232), (80, 210), (74, 110)], (60, 110, 160), (90, 140, 190))
d.line([(98, 96), (128, 132), (158, 96)], fill=INK, width=14)
cross(d, 128, 180, 22, SILVER, SILVER_L)
save("ChanclaBendita", img, (150, 200, 255))

# ---------------------------------------------------------------- ammo
img = canvas(); d = ImageDraw.Draw(img)
for i, x in enumerate((72, 128, 184)):
    rect(d, (x - 24, 70, x + 24, 200), RED, RED_L, r=10)
    d.rectangle((x - 26, 176, x + 26, 214), fill=GOLD + (255,), outline=INK, width=8)
    d.rectangle((x - 16, 96, x + 16, 112), fill=SALT_L + (255,))
save("BalasSal", img)

img = canvas(); d = ImageDraw.Draw(img)
for i, x in enumerate((80, 128, 176)):
    poly(d, [(x - 16, 110), (x - 16, 64), (x, 40), (x + 16, 64), (x + 16, 110)], SILVER, SILVER_L)
    rect(d, (x - 18, 108, x + 18, 214), GOLD, GOLD_L, r=6)
save("Balas", img)

# Each gun has its own ammo since #152: cartuchos for the escopeta (salt and normal), balas for the
# pistola (the ones above), and a magazine for the metralleta. The salt version of each has a white
# band of salt and a soft glow, like the other blessed things.
img = canvas(); d = ImageDraw.Draw(img)
for i, x in enumerate((72, 128, 184)):
    rect(d, (x - 24, 70, x + 24, 200), GREEN, GREEN_L, r=10)
    d.rectangle((x - 26, 176, x + 26, 214), fill=GOLD + (255,), outline=INK, width=8)
save("Cartuchos", img)

img = canvas(); d = ImageDraw.Draw(img)
for i, x in enumerate((80, 128, 176)):
    poly(d, [(x - 16, 110), (x - 16, 64), (x, 40), (x + 16, 64), (x + 16, 110)], SALT, SALT_L)
    rect(d, (x - 18, 108, x + 18, 214), GOLD, GOLD_L, r=6)
save("BalasPistolaSal", img, (230, 235, 255))


def magazine(d, band=None):
    poly(d, [(92, 70), (164, 70), (180, 230), (108, 230)], GUN, GUN_L)
    for x in (110, 146):
        poly(d, [(x - 14, 72), (x - 14, 40), (x, 22), (x + 14, 40), (x + 14, 72)], GOLD, GOLD_L)
    if band:
        d.polygon([(98, 130), (168, 130), (172, 152), (102, 152)], fill=band + (255,), outline=INK, width=6)


img = canvas(); d = ImageDraw.Draw(img)
magazine(d)
save("BalasMetralleta", img)

img = canvas(); d = ImageDraw.Draw(img)
magazine(d, SALT_L)
save("BalasMetralletaSal", img, (230, 235, 255))

img = canvas(); d = ImageDraw.Draw(img)
for i, (x0, y0) in enumerate(((40, 196), (64, 214), (88, 232))):
    d.line([(x0, y0), (x0 + 150, y0 - 150)], fill=INK, width=20)
    d.line([(x0, y0), (x0 + 150, y0 - 150)], fill=WOOD_L + (255,), width=10)
    tip = (x0 + 150, y0 - 150)
    d.polygon([(tip[0] + 22, tip[1] - 22), (tip[0] - 8, tip[1] - 12), (tip[0] + 12, tip[1] + 8)], fill=SILVER_L + (255,), outline=INK, width=5)
    d.polygon([(x0 - 6, y0 + 6), (x0 + 22, y0 - 4), (x0 + 6, y0 - 22)], fill=RED + (255,), outline=INK, width=5)
save("Virotes", img)

# ---------------------------------------------------------------- weapons (#134, #135)
img = canvas(); d = ImageDraw.Draw(img)
poly(d, [(60, 196), (176, 54), (214, 36), (206, 80), (90, 214)], SILVER, SILVER_L)
rect(d, (30, 196, 92, 232), WOOD, WOOD_L, r=10)
save("Machete", img)

img = canvas(); d = ImageDraw.Draw(img)
poly(d, [(20, 150), (176, 82), (236, 60), (242, 82), (186, 108), (60, 170)], GUN, GUN_L)
poly(d, [(20, 150), (60, 170), (66, 214), (24, 214)], WOOD, WOOD_L)
poly(d, [(110, 134), (150, 116), (156, 134), (118, 150)], WOOD, WOOD_L)
save("Escopeta", img)

img = canvas(); d = ImageDraw.Draw(img)
poly(d, [(40, 80), (210, 80), (210, 124), (110, 124), (96, 214), (54, 214), (66, 124), (40, 124)], GUN, GUN_L)
d.rectangle((60, 72, 74, 82), fill=INK)
save("Pistola", img)

img = canvas(); d = ImageDraw.Draw(img)
poly(d, [(26, 104), (196, 104), (232, 92), (236, 116), (196, 140), (150, 140), (150, 210), (118, 210), (110, 140), (26, 140)], GUN, GUN_L)
poly(d, [(64, 140), (98, 140), (92, 220), (64, 220)], GUN, GUN_L)
d.rectangle((200, 98, 226, 110), fill=SILVER_L + (255,))
save("Metralleta", img)

img = canvas(); d = ImageDraw.Draw(img)
d.arc((40, 40, 216, 150), 200, 340, fill=INK, width=22)
d.arc((40, 40, 216, 150), 200, 340, fill=WOOD_L + (255,), width=10)
d.line([(52, 106), (204, 106)], fill=CLOTH_L + (255,), width=4)
poly(d, [(118, 60), (138, 60), (138, 228), (118, 228)], WOOD, WOOD_L)
d.polygon([(128, 40), (114, 70), (142, 70)], fill=SILVER_L + (255,), outline=INK, width=6)
save("Ballesta", img)

img = canvas(); d = ImageDraw.Draw(img)
rect(d, (40, 56, 216, 134), RED, RED_L, r=34)
rect(d, (46, 66, 74, 124), (240, 220, 60), (255, 240, 140), r=10)
rect(d, (182, 66, 210, 124), (240, 220, 60), (255, 240, 140), r=10)
rect(d, (112, 128, 144, 236), (240, 220, 60), (255, 240, 140), r=10)
save("Martillo", img, (255, 220, 120))

# ---------------------------------------------------------------- crafting materials
img = canvas(); d = ImageDraw.Draw(img)
bottle(d, GLASS, GLASS_L)
save("Botella", img)

img = canvas(); d = ImageDraw.Draw(img)
for (x, y, r) in ((92, 168, 44), (158, 170, 46), (124, 124, 42)):
    ell(d, (x - r, y - r * 0.8, x + r, y + r * 0.8), (60, 30, 24))
    d.ellipse((x - r * 0.5, y - r * 0.3, x + r * 0.3, y + r * 0.3), fill=(240, 110, 40, 255))
save("Brasas", img, (255, 120, 40))

img = canvas(); d = ImageDraw.Draw(img)
for (x, y, a) in ((92, 150, -20), (148, 128, 15), (128, 190, 70)):
    c, s = math.cos(math.radians(a)), math.sin(math.radians(a))
    pts = [(x + c * dx - s * dy, y + s * dx + c * dy) for dx, dy in ((-50, -16), (50, -16), (50, 16), (-50, 16))]
    poly(d, pts, GOLD, GOLD_L)
save("Casquillos", img)

img = canvas(); d = ImageDraw.Draw(img)
poly(d, [(60, 120), (150, 90), (204, 130), (204, 190), (114, 222), (60, 180)], WAX, WAX_L)
d.line([(60, 120), (114, 152), (204, 130)], fill=INK, width=8)
d.line([(114, 152), (114, 222)], fill=INK, width=8)
save("Cera", img)

img = canvas(); d = ImageDraw.Draw(img)
poly(d, [(84, 40), (156, 40), (182, 110), (176, 210), (150, 232), (106, 232), (80, 210), (74, 110)], (120, 70, 40), (156, 100, 64))
d.line([(98, 96), (128, 132), (158, 96)], fill=INK, width=14)
save("Chancla", img)

img = canvas(); d = ImageDraw.Draw(img)
for y in (82, 128, 174):
    rect(d, (34, y - 22, 222, y + 22), WOOD, WOOD_L, r=8)
save("Madera", img)

img = canvas(); d = ImageDraw.Draw(img)
pts = [(60 + i * 12, 128 + math.sin(i * 0.8) * 46) for i in range(13)]
d.line(pts, fill=INK, width=22, joint="curve")
d.line(pts, fill=CLOTH + (255,), width=10, joint="curve")
save("Mecha", img)

img = canvas(); d = ImageDraw.Draw(img)
bottle(d, (200, 196, 170), (232, 228, 206), label=(GREEN, GREEN_L))
save("Mezcal", img)

img = canvas(); d = ImageDraw.Draw(img)
poly(d, [(50, 150), (90, 100), (210, 100), (206, 150), (166, 200), (46, 200)], SILVER, SILVER_L)
d.line([(50, 150), (166, 150), (206, 100)], fill=INK, width=8)
d.line([(166, 150), (166, 200)], fill=INK, width=8)
save("Plata", img, (210, 220, 240))

img = canvas(); d = ImageDraw.Draw(img)
poly(d, [(128, 40), (180, 140), (172, 196), (128, 222), (84, 196), (76, 140)], (200, 120, 30), (236, 170, 70))
save("Resina", img, (255, 170, 60))

img = canvas(); d = ImageDraw.Draw(img)
sack(d, SALT, SALT_L, tie=BLUE)
save("Sal", img)

img = canvas(); d = ImageDraw.Draw(img)
ell(d, (40, 70, 140, 190), CLOTH, CLOTH_L)
ell(d, (70, 100, 110, 160), (180, 170, 150))
poly(d, [(90, 70), (214, 92), (214, 212), (90, 190)], CLOTH, CLOTH_L)
save("Tela", img)

img = canvas(); d = ImageDraw.Draw(img)
poly(d, [(60, 70), (190, 60), (206, 120), (184, 214), (120, 200), (70, 218), (48, 150)], (132, 110, 84), (164, 140, 110))
d.line([(84, 110), (166, 98)], fill=INK, width=6)
d.line([(80, 170), (160, 160)], fill=INK, width=6)
save("Trapo", img)

# ---------------------------------------------------------------- key items
img = canvas(); d = ImageDraw.Draw(img)
poly(d, [(92, 120), (164, 120), (196, 230), (60, 230)], (154, 144, 124), (186, 176, 156))
ell(d, (86, 34, 170, 118), (205, 191, 168), (230, 220, 200))
ell(d, (100, 66, 118, 84), (8, 6, 6), outline=False)
ell(d, (138, 66, 156, 84), (8, 6, 6), outline=False)
d.line([(109, 84), (109, 110)], fill=(130, 16, 12, 255), width=5)
d.line([(147, 84), (147, 110)], fill=(130, 16, 12, 255), width=5)
save("Muneca", img, (220, 200, 170))

img = canvas(); d = ImageDraw.Draw(img)
ell(d, (40, 40, 216, 216), SILVER, SILVER_L)
ell(d, (70, 70, 186, 186), (128, 134, 142), outline=False)
d.polygon([(128, 84), (140, 118), (176, 118), (147, 140), (158, 176), (128, 154), (98, 176), (109, 140), (80, 118), (116, 118)], fill=(40, 34, 30, 255))
save("Moneda", img, (220, 225, 235))

img = canvas(); d = ImageDraw.Draw(img)
poly(d, [(128, 30), (196, 110), (162, 226), (94, 226), (60, 110)], (20, 16, 26), (60, 50, 74))
d.line([(128, 30), (116, 226)], fill=(90, 70, 110, 255), width=5)
save("Obsidiana", img, (160, 60, 200))

# The sheet: every icon on a slot-colored background, as it will look in the game.
cols = 8
rows = (len(made) + cols - 1) // cols
sheet = Image.new("RGBA", (cols * 140, rows * 140), (20, 17, 14, 255))
for i, (name, ic) in enumerate(made):
    x, y = (i % cols) * 140, (i // cols) * 140
    ImageDraw.Draw(sheet).rounded_rectangle((x + 6, y + 6, x + 134, y + 134), 14, fill=(36, 31, 26, 255))
    sheet.alpha_composite(ic.resize((116, 116), Image.LANCZOS), (x + 12, y + 12))
os.makedirs(os.path.dirname(SHEET), exist_ok=True)
sheet.save(SHEET)
print(len(made), "iconos")
