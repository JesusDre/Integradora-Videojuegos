"""Draws the top-down plans of the three instances (issues #5, #30 and #38) as PNG.

    python3 Tools/Planos/dibujar_planos.py

Output: docs/img/niveles/instancia1_xochimilco.png, instancia2_hacienda.png, instancia3_convento.png.
The plans are drawn from code (not by hand) so a change in the level is a change of a few numbers here,
and the blockout in Unity (#16, #40, #46) can copy the same coordinates: 1 unit = 1 m, X to the east,
Y to the north (in Unity, this Y is Z).
Needs Pillow (python3 -m pip install pillow).
"""

import math
import os

from PIL import Image, ImageDraw, ImageFont

REPO_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
OUT_DIR = os.path.join(REPO_ROOT, "docs", "img", "niveles")

FONT = "/usr/share/fonts/google-noto/NotoSans-Regular.ttf"
FONT_BOLD = "/usr/share/fonts/google-noto/NotoSans-Bold.ttf"

PX_PER_M = 9          # 1 m = 9 px
MARGIN = 60
TITLE_H = 110
LEGEND_W = 560

# Palette shared by the three plans; each instance adds its own ground colors.
INK = (34, 30, 28)
PAPER = (244, 239, 228)
ROUTE = (196, 34, 30)
SCRIPTED = (40, 90, 170)
ENTITY = (120, 50, 150)
KEY = (214, 160, 20)
SAFE = (46, 120, 80)
DARK = (25, 25, 32)


def font(size, bold=False):
    return ImageFont.truetype(FONT_BOLD if bold else FONT, size)


class Plan:
    def __init__(self, title, subtitle, width_m, height_m, ground):
        self.w_m, self.h_m = width_m, height_m
        self.map_w = int(width_m * PX_PER_M)
        self.map_h = int(height_m * PX_PER_M)
        self.img = Image.new("RGB", (MARGIN * 2 + self.map_w + LEGEND_W, TITLE_H + MARGIN * 2 + self.map_h), PAPER)
        self.d = ImageDraw.Draw(self.img, "RGBA")
        self.legend = []      # (symbol drawer, text)
        self.steps = []       # route step texts, in order
        self.walked_m = 0.0   # length of the on-foot route, for the legend
        self.d.text((MARGIN, 24), title, font=font(40, True), fill=INK)
        self.d.text((MARGIN, 74), subtitle, font=font(20), fill=(90, 84, 78))
        self.rect(0, 0, width_m, height_m, ground, outline=INK, width=3)

    # ---- coordinates: meters (x east, y north) to pixels
    def px(self, x, y):
        return (MARGIN + x * PX_PER_M, TITLE_H + MARGIN + (self.h_m - y) * PX_PER_M)

    # ---- areas
    def rect(self, x0, y0, x1, y1, fill, outline=None, width=2):
        a, b = self.px(x0, y1), self.px(x1, y0)
        self.d.rectangle([a, b], fill=fill, outline=outline, width=width)

    def poly(self, pts, fill, outline=None, width=2):
        self.d.polygon([self.px(*p) for p in pts], fill=fill, outline=outline, width=width)

    def hatch(self, x0, y0, x1, y1, color=(0, 0, 0, 70), step=1.6):
        """Diagonal lines: zones without light (only the flashlight)."""
        a, b = self.px(x0, y1), self.px(x1, y0)
        s = step * PX_PER_M
        k = -(b[1] - a[1])
        while k < b[0] - a[0]:
            p0 = (a[0] + k, b[1]); p1 = (a[0] + k + (b[1] - a[1]), a[1])
            # clip the line to the rectangle
            t0 = max(0.0, (a[0] - p0[0]) / (p1[0] - p0[0]))
            t1 = min(1.0, (b[0] - p0[0]) / (p1[0] - p0[0]))
            if t1 > t0:
                q0 = (p0[0] + (p1[0] - p0[0]) * t0, p0[1] + (p1[1] - p0[1]) * t0)
                q1 = (p0[0] + (p1[0] - p0[0]) * t1, p0[1] + (p1[1] - p0[1]) * t1)
                self.d.line([q0, q1], fill=color, width=2)
            k += s
        self.d.rectangle([a, b], outline=(0, 0, 0, 120), width=1)

    def label(self, x, y, text, size=17, color=INK, bold=False, anchor="mm"):
        self.d.text(self.px(x, y), text, font=font(size, bold), fill=color, anchor=anchor)

    # ---- routes
    def route(self, pts, color=ROUTE, width=5, dashed=False):
        pp = [self.px(*p) for p in pts]
        if not dashed:
            self.walked_m += sum(math.dist(a, b) for a, b in zip(pts, pts[1:]))
        for a, b in zip(pp, pp[1:]):
            if dashed:
                self._dashed(a, b, color, width)
            else:
                self.d.line([a, b], fill=color, width=width)
            self._arrow(a, b, color)

    def _dashed(self, a, b, color, width, dash=14, gap=10):
        length = math.dist(a, b)
        ux, uy = (b[0] - a[0]) / length, (b[1] - a[1]) / length
        t = 0.0
        while t < length:
            e = min(t + dash, length)
            self.d.line([(a[0] + ux * t, a[1] + uy * t), (a[0] + ux * e, a[1] + uy * e)], fill=color, width=width)
            t += dash + gap

    def _arrow(self, a, b, color):
        # Arrowhead at the middle of each segment: the direction is readable along the whole route.
        mx, my = (a[0] + b[0]) / 2, (a[1] + b[1]) / 2
        ang = math.atan2(b[1] - a[1], b[0] - a[0])
        size = 16
        tip = (mx + math.cos(ang) * size / 2, my + math.sin(ang) * size / 2)
        left = (tip[0] - math.cos(ang - 0.5) * size, tip[1] - math.sin(ang - 0.5) * size)
        right = (tip[0] - math.cos(ang + 0.5) * size, tip[1] - math.sin(ang + 0.5) * size)
        self.d.polygon([tip, left, right], fill=color)

    def loop(self, pts, color=ENTITY, width=3):
        """Closed dotted path: where an entity roams."""
        pp = [self.px(*p) for p in pts] + [self.px(*pts[0])]
        for a, b in zip(pp, pp[1:]):
            self._dashed(a, b, color, width, dash=6, gap=8)

    # ---- markers (drawn in pixels around a point in meters)
    def step(self, x, y, n, text):
        cx, cy = self.px(x, y)
        self.d.ellipse([cx - 17, cy - 17, cx + 17, cy + 17], fill=INK, outline=PAPER, width=3)
        self.d.text((cx, cy), str(n), font=font(18, True), fill=PAPER, anchor="mm")
        self.steps.append(f"{n}. {text}")

    def start(self, x, y):
        self._badge(x, y, "I", (30, 140, 60), "circle")

    def exit(self, x, y):
        self._badge(x, y, "S", (30, 140, 60), "square")

    def key(self, x, y, n):
        self._badge(x, y, str(n), KEY, "diamond")

    def altar(self, x, y):
        self._badge(x, y, "+", SAFE, "square")

    def group(self, x, y, count, name):
        cx, cy = self.px(x, y)
        self.d.polygon([(cx, cy - 20), (cx - 19, cy + 14), (cx + 19, cy + 14)], fill=ROUTE, outline=INK)
        self.d.text((cx, cy + 3), str(count), font=font(16, True), fill="white", anchor="mm")
        self.d.text((cx + 22, cy - 2), name, font=font(15, True), fill=ROUTE, anchor="lm")

    def note(self, x, y):
        self._badge(x, y, "N", (250, 250, 245), "square", text_color=INK)

    def stash(self, x, y):
        self._badge(x, y, "A", (140, 100, 60), "circle")

    def anchor(self, x, y):
        cx, cy = self.px(x, y)
        for r, c in ((26, ENTITY), (18, PAPER), (11, ENTITY)):
            self.d.ellipse([cx - r, cy - r, cx + r, cy + r], fill=c)

    def light(self, x, y):
        cx, cy = self.px(x, y)
        self.d.ellipse([cx - 22, cy - 22, cx + 22, cy + 22], fill=(255, 170, 40, 60))
        self.d.ellipse([cx - 6, cy - 6, cx + 6, cy + 6], fill=(240, 140, 20), outline=INK)

    def special(self, x, y, letter, color):
        self._badge(x, y, letter, color, "circle")

    def _badge(self, x, y, text, color, shape, text_color="white"):
        cx, cy = self.px(x, y)
        r = 15
        if shape == "circle":
            self.d.ellipse([cx - r, cy - r, cx + r, cy + r], fill=color, outline=INK, width=2)
        elif shape == "square":
            self.d.rectangle([cx - r, cy - r, cx + r, cy + r], fill=color, outline=INK, width=2)
        else:
            self.d.polygon([(cx, cy - r - 4), (cx + r + 4, cy), (cx, cy + r + 4), (cx - r - 4, cy)], fill=color, outline=INK)
        self.d.text((cx, cy), text, font=font(17, True), fill=text_color, anchor="mm")

    # ---- legend, scale and north arrow
    def finish(self, path, symbols, notes):
        lx = MARGIN * 2 + self.map_w
        y = TITLE_H + MARGIN
        self.d.text((lx, y), "Recorrido", font=font(24, True), fill=INK)
        y += 40
        for s in self.steps:
            y = self._wrapped(lx, y, s, 17, LEGEND_W - 40) + 4
        y += 16
        self.d.text((lx, y), "Marcas", font=font(24, True), fill=INK)
        y += 40
        for draw, text in symbols:
            draw(lx + 18, y + 12)
            y = self._wrapped(lx + 48, y, text, 17, LEGEND_W - 80) + 8
        y += 12
        notes = list(notes) + [f"Recorrido a pie: unos {round(self.walked_m / 10) * 10} m, más los tramos por guion; "
                               "con exploración, encuentros y crafteo, unos 10 minutos."]
        for n in notes:
            y = self._wrapped(lx, y, n, 16, LEGEND_W - 40, color=(90, 84, 78)) + 6
        self._scale()
        self._north()
        os.makedirs(OUT_DIR, exist_ok=True)
        self.img.save(os.path.join(OUT_DIR, path))
        print("saved", path)

    def _wrapped(self, x, y, text, size, width, color=INK):
        f = font(size)
        words, line = text.split(), ""
        for w in words:
            test = (line + " " + w).strip()
            if self.d.textlength(test, font=f) > width:
                self.d.text((x, y), line, font=f, fill=color)
                y += size + 6
                line = w
            else:
                line = test
        if line:
            self.d.text((x, y), line, font=f, fill=color)
            y += size + 6
        return y

    def _scale(self):
        x0, y0 = MARGIN + 20, TITLE_H + MARGIN + self.map_h - 30
        seg = 10 * PX_PER_M
        self.d.rectangle([x0 - 10, y0 - 46, x0 + 2 * seg + 150, y0 + 22], fill=(255, 255, 255, 200))
        for i in range(2):
            self.d.rectangle([x0 + i * seg, y0, x0 + (i + 1) * seg, y0 + 10], fill=INK if i == 0 else PAPER, outline=INK)
        for i, t in enumerate(("0", "10", "20 m")):
            self.d.text((x0 + i * seg, y0 - 4), t, font=font(14), fill=INK, anchor="mb")
        # The Padre at the same scale: 1.8 m tall, about 0.5 m wide seen from above.
        fx = x0 + 2 * seg + 60
        h = 1.8 * PX_PER_M
        self.d.rectangle([fx, y0 + 10 - h, fx + 0.5 * PX_PER_M, y0 + 10], fill=ROUTE)
        self.d.text((fx + 12, y0 + 2), "Padre 1.8 m", font=font(14), fill=INK, anchor="lm")

    def _north(self):
        cx, cy = MARGIN + 40, TITLE_H + MARGIN + 50
        self.d.polygon([(cx, cy - 30), (cx - 12, cy + 10), (cx, cy + 2), (cx + 12, cy + 10)], fill=INK)
        self.d.text((cx, cy + 26), "N", font=font(18, True), fill=INK, anchor="mm")


def legend_common(p, extra=()):
    """Symbols every plan uses, plus the instance's own."""
    base = [
        (lambda x, y: p._badge_at(x, y, "I", (30, 140, 60), "circle"), "Inicio"),
        (lambda x, y: p._badge_at(x, y, "S", (30, 140, 60), "square"), "Salida"),
        (lambda x, y: p._badge_at(x, y, "1", KEY, "diamond"), "Objeto clave (los 3 se llevan al ancla o al pedestal)"),
        (lambda x, y: p._badge_at(x, y, "+", SAFE, "square"), "Altar de crafteo (cuarto seguro: autoguardado al entrar y salir)"),
        (lambda x, y: p._triangle_at(x, y), "Grupo de cultistas (número = cuántos)"),
        (lambda x, y: p._badge_at(x, y, "N", (250, 250, 245), "square", INK), "Nota (desbloquea una receta)"),
        (lambda x, y: p._badge_at(x, y, "A", (140, 100, 60), "circle"), "Alijo de sal, botellas o vendas (secreto)"),
        (lambda x, y: p._line_at(x, y, ROUTE, False), "Recorrido a pie, en orden"),
        (lambda x, y: p._line_at(x, y, SCRIPTED, True), "Recorrido por guion (el jugador no controla)"),
        (lambda x, y: p._line_at(x, y, ENTITY, True), "Zona por donde ronda la entidad"),
    ]
    return base + list(extra)


# Legend drawers work in pixels (the legend is outside the map).
def _badge_at(self, x, y, text, color, shape, text_color="white"):
    r = 13
    if shape == "circle":
        self.d.ellipse([x - r, y - r, x + r, y + r], fill=color, outline=INK, width=2)
    elif shape == "square":
        self.d.rectangle([x - r, y - r, x + r, y + r], fill=color, outline=INK, width=2)
    else:
        self.d.polygon([(x, y - r - 3), (x + r + 3, y), (x, y + r + 3), (x - r - 3, y)], fill=color, outline=INK)
    self.d.text((x, y), text, font=font(15, True), fill=text_color, anchor="mm")


def _triangle_at(self, x, y):
    self.d.polygon([(x, y - 15), (x - 14, y + 11), (x + 14, y + 11)], fill=ROUTE, outline=INK)
    self.d.text((x, y + 2), "3", font=font(13, True), fill="white", anchor="mm")


def _line_at(self, x, y, color, dashed):
    if dashed:
        self._dashed((x - 16, y), (x + 18, y), color, 4, dash=8, gap=6)
    else:
        self.d.line([(x - 16, y), (x + 18, y)], fill=color, width=5)


def _anchor_at(self, x, y):
    for r, c in ((15, ENTITY), (10, PAPER), (6, ENTITY)):
        self.d.ellipse([x - r, y - r, x + r, y + r], fill=c)


def _light_at(self, x, y):
    self.d.ellipse([x - 14, y - 14, x + 14, y + 14], fill=(255, 170, 40, 60))
    self.d.ellipse([x - 5, y - 5, x + 5, y + 5], fill=(240, 140, 20), outline=INK)


def _dark_at(self, x, y):
    self.d.rectangle([x - 14, y - 12, x + 14, y + 12], fill=(0, 0, 0, 40), outline=INK)
    for k in range(-14, 14, 6):
        self.d.line([(x + k, y + 12), (x + k + 12, y - 12)], fill=(0, 0, 0, 120), width=2)


Plan._badge_at = _badge_at
Plan._triangle_at = _triangle_at
Plan._line_at = _line_at
Plan._anchor_at = _anchor_at
Plan._light_at = _light_at
Plan._dark_at = _dark_at


# ============================================================ Instance 1: Xochimilco
def instancia1():
    water = (88, 140, 140)
    land = (150, 160, 110)
    p = Plan("Instancia 1 · Isla de las Muñecas (Xochimilco)",
             "Vista desde arriba · ~10 min · 1 cuadro = 10 m · paleta azul-verde, niebla blanca, luna fría",
             170, 120, water)
    # 10 m grid so distances can be read.
    for gx in range(10, 170, 10):
        p.d.line([p.px(gx, 0), p.px(gx, 120)], fill=(255, 255, 255, 25))
    for gy in range(10, 120, 10):
        p.d.line([p.px(0, gy), p.px(170, gy)], fill=(255, 255, 255, 25))

    # Chinampas (islands of land between canals).
    p.rect(5, 5, 45, 36, land, outline=INK)                   # A: embarcadero
    p.rect(55, 40, 100, 76, land, outline=INK)                # B: chinampa de las muñecas
    p.rect(100, 54, 124, 82, land, outline=INK)               # C: choza del cuidador
    p.poly([(126, 40), (166, 40), (166, 112), (128, 112), (126, 90)], land, outline=INK)  # D: la isla
    p.rect(108, 63, 120, 74, (120, 96, 70), outline=INK)       # choza = cuarto seguro
    p.label(114, 76.5, "Cuarto seguro", 15, SAFE, True)
    p.rect(100, 60, 104, 64, (140, 110, 80), outline=INK)      # puente de tablones B→C
    p.d.line([p.px(124, 70), p.px(127, 70)], fill=(140, 110, 80), width=24)  # puente C→D
    p.label(25, 33, "Embarcadero", 15, INK, True)
    p.label(78, 73, "Chinampa de las muñecas", 15, INK, True)
    p.label(146, 47, "La isla (ahuehuete y muñecas colgadas)", 14, INK, True)
    p.d.ellipse([*p.px(140, 92), *p.px(150, 82)], fill=(70, 90, 50), outline=INK)  # ahuehuete
    p.label(145, 87, "árbol", 13, PAPER)
    p.label(50, 100, "Canal principal (niebla)", 18, (230, 240, 240), True)
    # The Llorona's hunting ground in section 2: the canals around the island.
    p.loop([(123, 38), (169, 38), (169, 116), (124, 116), (122, 86)])
    p.label(150, 117.5, "La Llorona recorre estos canales (fase 1)", 14, ENTITY, True)
    p.hatch(55, 40, 74, 58)      # zona oscura bajo los árboles de la chinampa B
    p.light(114, 68)             # veladoras del cuarto seguro
    p.light(142, 107); p.light(150, 107); p.light(146, 101)   # 3 veladoras fijas del ancla

    # Route on foot, then the two scripted trajinera trips.
    p.route([(12, 10), (20, 22), (36, 28), (42, 32)])
    p.route([(42, 32), (52, 38), (60, 46)], color=SCRIPTED, dashed=True)
    p.route([(60, 46), (78, 52), (92, 68), (101, 62), (110, 66)])
    p.route([(118, 70), (130, 72), (140, 76), (142, 98), (146, 104)])
    p.route([(150, 110), (160, 116), (168, 100), (168, 30), (160, 6)], color=SCRIPTED, dashed=True)

    p.start(12, 10); p.step(18, 14, 1, "Llegada en trajinera al embarcadero. Primeras muñecas colgadas y una nota del culto.")
    p.key(35, 24, 1); p.step(30, 30, 2, "Sección 1: la primera muñeca ofrenda, a la vista junto al sendero.")
    p.group(28, 18, 2, "G1")
    p.step(50, 33, 3, "Cruce en trajinera (por guion): avanza sola; el Padre puede mirar y usar objetos rápidos.")
    p.group(76, 56, 3, "G2"); p.key(92, 72, 2)
    p.step(84, 62, 4, "Chinampa de las muñecas: grupo de 3 cultistas y la segunda muñeca.")
    p.note(62, 72); p.stash(58, 44)
    p.altar(114, 66)
    p.step(106, 58, 5, "Cuarto seguro (choza del cuidador): altar, crafteo y autoguardado.")
    p.group(134, 66, 3, "G3"); p.group(158, 92, 2, "G4")
    p.key(140, 102, 3); p.step(138, 82, 6, "Sección 2: la isla. La Llorona acecha desde los canales; tercera muñeca a la vista.")
    p.stash(162, 46); p.note(160, 70)
    p.anchor(146, 106); p.step(136, 108, 7, "Ancla de La Llorona: colocar las 3 muñecas, encender las 3 veladoras y rezar (3 oleadas de 3).")
    p.exit(160, 6); p.step(166, 20, 8, "Huida en trajinera (por guion) hasta la salida del canal; sin checkpoint.")

    p.finish("instancia1_xochimilco.png", legend_common(p, [
        (lambda x, y: p._anchor_at(x, y), "Ancla de la entidad (ritual final)"),
        (lambda x, y: p._light_at(x, y), "Luz (veladoras); el resto, solo luna y linterna"),
        (lambda x, y: p._dark_at(x, y), "Zona sin luz"),
    ]), [
        "Objetos clave: 2 muñecas en la sección 1 y 1 en la sección 2, siempre a la vista.",
        "Encuentros: 4 grupos de 2–3 cultistas (GDD §12). Ritual: 3 oleadas de 3.",
        "Recetas de esta instancia: agua bendita, balas de sal, línea de sal, veladora y molotov.",
    ])


# ============================================================ Instance 2: Hacienda
def instancia2():
    dirt = (196, 168, 118)
    p = Plan("Instancia 2 · Hacienda abandonada (Bajío)",
             "Vista desde arriba · ~10 min · 1 cuadro = 10 m · paleta ocre, negro y rojo apagado; atardecer que se apaga",
             180, 130, dirt)
    for gx in range(10, 180, 10):
        p.d.line([p.px(gx, 0), p.px(gx, 130)], fill=(0, 0, 0, 18))
    for gy in range(10, 130, 10):
        p.d.line([p.px(0, gy), p.px(180, gy)], fill=(0, 0, 0, 18))

    wall = (120, 92, 70)
    floor = (222, 206, 176)
    agave = (120, 150, 120)
    p.rect(150, 0, 180, 70, agave, outline=INK); p.label(165, 64, "Campo de agave", 14, INK, True)
    # Trojes (granaries) and service yard.
    p.rect(128, 12, 148, 50, floor, outline=wall, width=5); p.label(138, 47, "Trojes", 15, INK, True)
    # Casa grande around its central patio.
    p.rect(78, 18, 124, 64, (170, 130, 100), outline=INK, width=4)
    p.rect(90, 30, 112, 52, floor, outline=INK); p.label(101, 41, "Patio", 14, INK)
    p.label(101, 61, "Casa grande", 16, INK, True)
    p.label(86, 60, "despacho", 12, INK); p.label(118, 22, "cocina", 12, INK)
    # Tienda de raya: the safe room between the sections.
    p.rect(80, 70, 96, 82, (150, 110, 80), outline=INK, width=4); p.label(88, 85, "Cuarto seguro", 15, SAFE, True)
    p.label(88, 67, "tienda de raya", 12, INK)
    # Panteón and capilla in section 2.
    p.rect(14, 80, 58, 124, (170, 160, 140), outline=INK, width=3); p.label(36, 127, "Panteón", 16, INK, True)
    for tx in range(18, 56, 6):
        for ty in range(84, 120, 7):
            p.rect(tx, ty, tx + 2.5, ty + 4, (120, 110, 100))
    p.rect(62, 98, 84, 124, (200, 190, 170), outline=INK, width=5); p.label(73, 127, "Capilla", 16, INK, True)
    p.light(73, 118); p.light(68, 108); p.light(78, 108)
    # Holy water channels in front of the chapel, across the Charro's charge routes.
    for cx in (60, 73, 86):
        p.rect(cx - 5, 90, cx + 5, 92, (60, 120, 200), outline=INK)
    p.label(92, 91, "3 canaletas (línea de agua bendita)", 13, (40, 80, 160), True, "lm")
    # The Charro rides the open ground between the safe room, the panteón and the chapel.
    p.loop([(60, 70), (120, 72), (140, 100), (120, 124), (92, 124), (90, 94), (60, 94)])
    p.label(118, 104, "El Charro Negro cabalga aquí (fase 1)", 14, ENTITY, True)
    p.label(118, 99, "no puede entrar a la capilla", 13, ENTITY)
    # Road from the chapel to the main gate (the escape on foot).
    p.d.line([p.px(62, 96), p.px(40, 62), p.px(4, 44)], fill=(160, 130, 90), width=60)
    p.label(14, 60, "Camino al portón", 15, INK, True)
    p.rect(0, 38, 4, 52, wall, outline=INK)
    p.hatch(128, 12, 148, 50); p.hatch(14, 80, 58, 124)

    p.route([(172, 20), (150, 28), (140, 34), (126, 40), (112, 40), (90, 50), (86, 64), (88, 70)])
    p.route([(88, 82), (70, 86), (50, 92), (34, 108), (54, 112), (64, 100)])
    p.route([(62, 94), (40, 62), (6, 45)])

    p.start(172, 20); p.step(170, 30, 1, "Llegada a pie por el campo de agave, al anochecer.")
    p.group(138, 22, 3, "G1"); p.key(144, 44, 1)
    p.step(132, 36, 2, "Sección 1: las trojes, a oscuras. Primera moneda del pacto.")
    p.group(102, 36, 4, "G2"); p.key(84, 54, 2); p.note(120, 24)
    p.step(110, 46, 3, "Casa grande: 4 cultistas en el patio; segunda moneda en el despacho y nota de la chancla bendita en la cocina.")
    p.altar(88, 76); p.step(98, 76, 4, "Cuarto seguro (tienda de raya): altar y autoguardado.")
    p.stash(124, 66)
    p.group(44, 98, 4, "G3"); p.key(30, 114, 3); p.note(52, 84)
    p.step(64, 82, 5, "Sección 2: cruzar el campo abierto mientras el Charro cabalga; panteón con 4 cultistas y la tercera moneda.")
    p.special(73, 114, "P", (90, 60, 40)); p.special(66, 101, "B", (60, 120, 200))
    p.step(80, 102, 6, "Capilla: colocar las 3 monedas en el pedestal (P). La pila (B) da 1 agua bendita, hasta 3 veces.")
    p.step(73, 95, 7, "El Charro embiste: llenar una canaleta con agua bendita y, cuando la cruce, 3 golpes de cruz de plata cargada.")
    p.group(22, 46, 3, "G4"); p.exit(6, 45)
    p.step(48, 72, 8, "Huida a pie por el camino hasta el portón; cultistas por guion en el camino; sin checkpoint.")

    p.finish("instancia2_hacienda.png", legend_common(p, [
        (lambda x, y: p._badge_at(x, y, "P", (90, 60, 40), "circle"), "Pedestal de las monedas (no es el altar de crafteo)"),
        (lambda x, y: p._badge_at(x, y, "B", (60, 120, 200), "circle"), "Pila de agua bendita (3 usos; se rellena al reaparecer)"),
        (lambda x, y: p._light_at(x, y), "Luz de la capilla; el resto, atardecer que se apaga"),
        (lambda x, y: p._dark_at(x, y), "Zona sin luz"),
    ]), [
        "Objetos clave: 2 monedas en la sección 1 y 1 en la sección 2.",
        "Encuentros: 4 grupos de 3–4 cultistas (GDD §12). Sin barra de ritual: el Charro se vence con la línea de agua bendita y la cruz de plata.",
        "Recetas de esta instancia: cruz de plata (nota de la Llorona) y chancla bendita.",
    ])


# ============================================================ Instance 3: Ex-convento
def instancia3():
    stone = (150, 146, 140)
    p = Plan("Instancia 3 · Ex-convento y catacumbas",
             "Vista desde arriba · ~10 min · 1 cuadro = 10 m · paleta piedra gris y rojo intenso; antorchas y veladoras",
             160, 130, (70, 66, 62))
    levels = {  # level colors: the deeper, the darker
        "+5": (196, 190, 180), "0": stone, "-4": (110, 100, 96), "-9": (82, 64, 64),
    }
    for gx in range(10, 160, 10):
        p.d.line([p.px(gx, 0), p.px(gx, 130)], fill=(255, 255, 255, 18))
    for gy in range(10, 130, 10):
        p.d.line([p.px(0, gy), p.px(160, gy)], fill=(255, 255, 255, 18))

    p.rect(55, 0, 90, 12, (170, 160, 140), outline=INK); p.label(72, 6, "Atrio  (nivel 0)", 14, INK, True)
    p.rect(55, 12, 85, 62, levels["0"], outline=INK, width=4); p.label(70, 30, "Templo", 16, INK, True)
    p.rect(55, 52, 85, 62, levels["+5"], outline=INK, width=2); p.label(70, 57, "Coro alto (+5 m)", 13, INK, True)
    p.rect(90, 18, 140, 68, levels["0"], outline=INK, width=4)
    p.rect(100, 28, 130, 58, (110, 130, 90), outline=INK); p.label(115, 43, "Claustro", 16, INK, True)
    p.rect(126, 58, 140, 68, (120, 116, 110), outline=INK); p.label(133, 63, "sala capitular", 11, PAPER)
    p.rect(92, 72, 110, 86, (120, 100, 80), outline=INK, width=4); p.label(101, 89, "Cuarto seguro", 15, SAFE, True)
    p.label(101, 69.5, "celda del prior", 12, PAPER)
    p.rect(18, 70, 84, 112, levels["-4"], outline=INK, width=4); p.label(51, 115, "Criptas (−4 m)", 16, PAPER, True)
    for ox in range(24, 82, 10):
        p.rect(ox, 76, ox + 4, 106, (90, 84, 80))   # osarios
    p.rect(88, 92, 146, 124, levels["-9"], outline=INK, width=4); p.label(117, 127, "Catacumbas (−9 m)", 16, PAPER, True)
    p.rect(146, 12, 156, 124, (60, 50, 50), outline=INK); p.label(151, 66, "Túnel", 14, PAPER, True, "mm")
    p.rect(146, 0, 160, 12, (110, 120, 90), outline=INK); p.label(153, 6, "Barranca", 12, INK, True)
    # Stairs between levels.
    for (x, y, t) in ((82, 56, "caracol, de 0 a +5"), (86, 78, "escalera, de 0 a −4"), (84, 104, "rampa, de −4 a −9")):
        p.special(x, y, "E", (90, 90, 90)); p.label(x, y - 4, t, 11, PAPER)
    p.hatch(18, 70, 84, 112, (0, 0, 0, 90)); p.hatch(126, 58, 140, 68, (0, 0, 0, 90)); p.hatch(146, 12, 156, 124, (0, 0, 0, 90))
    for (x, y) in ((60, 20), (80, 20), (60, 44), (80, 44), (95, 22), (135, 22), (95, 64), (101, 79), (100, 96), (134, 96), (100, 120), (134, 120)):
        p.light(x, y)
    p.label(117, 99, "niebla roja durante el ritual", 13, (230, 80, 70), True)
    p.loop([(90, 94), (144, 94), (144, 122), (90, 122)])
    p.label(117, 110, "la Tzitzimime, invisible por el techo", 13, ENTITY, True)

    p.route([(72, 2), (70, 16), (70, 40), (82, 56)])
    p.route([(84, 48), (96, 40), (108, 24), (128, 24), (134, 46), (132, 62), (112, 64), (101, 72)])
    p.route([(101, 86), (88, 80), (60, 88), (32, 100), (60, 104), (84, 104), (100, 108), (110, 108)])
    p.route([(124, 108), (151, 112), (151, 14), (153, 4)], color=ROUTE)

    p.start(72, 2); p.step(64, 6, 1, "Llegada por el atrio. El templo se ilumina con antorchas.")
    p.key(76, 59, 1); p.step(64, 48, 2, "Sección 1: subir al coro alto (+5 m) por la escalera de caracol: primer fragmento de obsidiana.")
    p.group(104, 24, 4, "G1"); p.group(136, 34, 5, "G2"); p.key(137, 65, 2); p.note(94, 58); p.stash(129, 20)
    p.step(120, 22, 3, "Claustro: 4 y 5 cultistas; segundo fragmento en la sala capitular, a oscuras. Nota del copal.")
    p.altar(101, 79); p.step(112, 80, 4, "Cuarto seguro (celda del prior): altar y autoguardado.")
    p.group(40, 92, 4, "G3"); p.key(26, 106, 3); p.stash(78, 74)
    p.step(58, 94, 5, "Sección 2: bajar a las criptas (−4 m), sin luz: tercer fragmento entre los osarios.")
    p.anchor(117, 104)
    p.step(104, 116, 6, "Catacumbas (−9 m): ritual con los 3 fragmentos, 4 oleadas de 4. El copal revela a la Tzitzimime 15 s.")
    p.exit(153, 4); p.step(140, 60, 7, "Huida por el túnel que sube hasta la barranca; sin checkpoint (el derrumbe es deseable).")

    p.finish("instancia3_convento.png", legend_common(p, [
        (lambda x, y: p._anchor_at(x, y), "Ancla de la entidad (ritual final)"),
        (lambda x, y: p._badge_at(x, y, "E", (90, 90, 90), "circle"), "Escalera o rampa entre niveles"),
        (lambda x, y: p._light_at(x, y), "Antorcha o veladora"),
        (lambda x, y: p._dark_at(x, y), "Zona sin luz (solo linterna)"),
    ]), [
        "Niveles de altura: coro alto +5 m · planta baja 0 · criptas −4 m · catacumbas −9 m · el túnel sube de −9 a 0.",
        "Objetos clave: 2 fragmentos en la sección 1 y 1 en la sección 2.",
        "Encuentros: 3 grupos de 4–5 cultistas (GDD §12). Ritual: 4 oleadas de 4.",
        "Recetas de esta instancia: copal y ritual final.",
    ])


if __name__ == "__main__":
    instancia1()
    instancia2()
    instancia3()
