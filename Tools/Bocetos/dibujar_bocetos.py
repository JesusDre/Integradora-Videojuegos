"""Draws the screen palette (issue #2) and the UI sketches (issues #6 and #7) at 1920x1080.

The sketches are not drawn by hand: layouts/*.json are the positions, colors and texts of every UI element,
measured in Play Mode from the real screens (Unity's RectTransforms on a 1920x1080 game view). This script
redraws them flat, with a numbered legend, so the sketch always matches the game. If a screen changes,
measure it again and run:
    python3 Tools/Bocetos/dibujar_bocetos.py
Output: docs/img/bocetos/*.png
"""

import json
import os

from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
LAYOUTS = os.path.join(os.path.dirname(__file__), "layouts")
OUT = os.path.join(ROOT, "docs", "img", "bocetos")
FONT = "/usr/share/fonts/liberation-sans-fonts/LiberationSans-Regular.ttf"
FONT_BOLD = "/usr/share/fonts/liberation-sans-fonts/LiberationSans-Bold.ttf"
W, H = 1920, 1080

# ---- The palette (issue #2). The same values the HUD and menus use in Unity.
PALETTE = [
    ("Fondo de paneles", "#0E0F12", "Inventario, altar, pausa y opciones (al 90 % de opacidad)"),
    ("Fondo del menú", "#120908", "Menú principal: casi negro con un tinte rojizo"),
    ("Texto hueso", "#E8E1D3", "Todo el texto: legible sobre lo oscuro sin ser blanco puro"),
    ("Acento veladora", "#DBAD54", "Títulos, valores y el botón elegido"),
    ("Botón elegido", "#6B331F", "Botón con el foco del mouse o del teclado"),
    ("Vida", "#B3261E", "Barra de vida"),
    ("Alerta", "#FF4536", "Vida baja (≤ 30), daño y \"Has caído\""),
    ("Resistencia", "#C9A327", "Barra de resistencia y \"Rezo listo\""),
    ("Ritual", "#8CBFF2", "Barra del ritual y agua bendita"),
]
VARIANTS = [
    ("Instancia 1 · Xochimilco", "Azul-verde, niebla blanca, luz fría de luna",
     ["#0F1C1F", "#2E5A5C", "#6B7A80", "#9CC8E8"]),
    ("Instancia 2 · Hacienda", "Ocre, negro y rojo apagado; atardecer que se apaga",
     ["#1A110C", "#7A3324", "#C4A876", "#E0A050"]),
    ("Instancia 3 · Ex-convento", "Piedra gris y rojo intenso; antorchas y veladoras",
     ["#121012", "#615D5C", "#8A1414", "#FF8C40"]),
]

# ---- What each sketch shows, and which elements get a numbered note: (name, text that must appear, note).
SCREENS = [
    ("hud", "HUD en juego", "#6", True, [
        ("Title", "Vida", "Vida: barra roja con el número; parpadea con 30 o menos"),
        ("Title", "Resistencia", "Resistencia: se gasta al correr y golpear; gris cuando se agota"),
        ("PrayerStatus", None, "Estado del rezo: \"Rezo listo [R]\" o los segundos que faltan"),
        ("Slot1", None, "Espacios rápidos 1–4: objeto, cantidad y tecla"),
        ("Name", "machete", "Arma en la mano y munición de la escopeta"),
        ("Title", "Ritual", "Barra del ritual: solo aparece durante el ritual"),
        ("Subtitles", None, "Subtítulos de lo que dice el Padre"),
        ("Prompt", None, "Acción con F sobre lo que mira el Padre"),
        ("Crosshair", None, "Mira: un punto pequeño"),
    ]),
    ("inventario", "Inventario (Tab)", "#6", True, [
        ("Title", "Inventario", "Inventario: el juego no se pausa"),
        ("Slot1", None, "8 espacios; los 4 primeros son los rápidos"),
        ("Name", "venda", "Objeto elegido: nombre y descripción"),
        ("Label", "Usar", "Usar y Tirar"),
        ("Materials", None, "Materiales (hasta 99 cada uno, no ocupan espacio) y objetos clave"),
        ("Hint", None, "Cómo cerrar"),
    ]),
    ("altar", "Altar de crafteo (F en el altar)", "#6", True, [
        ("Title", "Altar", "Altar del cuarto seguro"),
        ("Label", "agua bendita", "Recetas conocidas; las demás dicen \"Receta desconocida\""),
        ("Ingredients", None, "Materiales que pide la receta y cuántos tienes"),
        ("CraftButton", None, "Fabricar"),
        ("Label", "Cerrar", "Cerrar"),
    ]),
    ("menu_principal", "Menú principal", "#7", False, [
        ("Title", None, "Título del juego"),
        ("NuevaPartida", None, "Nueva partida → selección de instancia"),
        ("Continuar", None, "Continuar: apagado si no hay partida guardada"),
        ("Opciones", None, "Opciones"),
        ("Creditos", None, "Créditos"),
        ("Salir", None, "Salir"),
        ("Controls", None, "Ayuda de controles: mouse o teclado"),
    ]),
    ("menu_seleccion", "Selección de instancia", "#7", False, [
        ("Instancia1", None, "Instancias en orden; las bloqueadas no se pueden elegir"),
        ("Description", None, "Descripción de la instancia que tiene el foco"),
        ("Volver", None, "Volver (o Esc)"),
    ]),
    ("pausa", "Pausa (Esc)", "#7", True, [
        ("Title", "Pausa", "El juego se congela"),
        ("Continuar", None, "Continuar"),
        ("Reiniciar", None, "Reiniciar: vuelve al último cuarto seguro"),
        ("Opciones", None, "Opciones"),
        ("Salir", None, "Salir al menú principal"),
    ]),
    ("menu_opciones", "Opciones (desde el menú o la pausa)", "#7", False, [
        ("Sensibilidad del mouse_Label", None, "Sensibilidad del mouse (×0.25 a ×3)"),
        ("Invertir eje Y_Label", None, "Invertir eje Y"),
        ("Volumen general_Label", None, "Volumen general, música, efectos y voces"),
        ("Subtítulos_Label", None, "Subtítulos sí/no"),
        ("Volver", None, "Volver"),
    ]),
    ("derrota", "Derrota (vida en 0)", "#7", True, [
        ("Title", "caído", "\"Has caído\""),
        ("Reintentar", None, "Reintentar: desde el último cuarto seguro"),
        ("Salir", None, "Salir al menú"),
    ]),
    ("victoria", "Victoria de instancia / final", "#7", True, [
        ("Title", None, "Título: \"Escapaste de…\" o la victoria final"),
        ("Summary", None, "Resumen: tiempo, muertes y notas leídas"),
        ("Siguiente", None, "Siguiente instancia (en la final: Ver créditos)"),
        ("Salir", None, "Salir al menú"),
    ]),
    ("menu_creditos", "Créditos", "#7", False, [
        ("Body", None, "Proyecto, equipo, herramientas y nota sobre las leyendas"),
        ("Volver", None, "Volver"),
    ]),
]


def font(size, bold=False):
    return ImageFont.truetype(FONT_BOLD if bold else FONT, max(10, int(size)))


def hex_rgb(h):
    h = h.lstrip("#")
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))


def rgba(c):
    return tuple(int(round(v * 255)) for v in c)


def game_backdrop():
    """A dark stand-in for the 3D scene behind the HUD: a night gradient with a ground line."""
    img = Image.new("RGBA", (W, H), (0, 0, 0, 255))
    d = ImageDraw.Draw(img)
    for y in range(H):
        t = y / H
        sky = (int(10 + 14 * t), int(12 + 16 * t), int(16 + 18 * t)) if y < 560 else (int(30 - 10 * (t - 0.52)), 34, 36)
        d.line([(0, y), (W, y)], fill=(*sky, 255))
    d.text((W - 24, 20), "(escena del juego)", font=font(22), fill=(110, 115, 120, 255), anchor="ra")
    return img


def draw_layout(img, elements):
    layer = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(layer)
    # Canvases first by sorting order, then big shapes before small ones so backgrounds stay behind.
    ordered = sorted(elements, key=lambda e: (e["order"], e["kind"] == "text", -(e["r"][2] - e["r"][0]) * (e["r"][3] - e["r"][1])))
    for e in ordered:
        x0, y0, x1, y1 = e["r"]
        color = rgba(e["c"])
        if e["kind"] == "text":
            if not e["text"].strip() or color[3] < 10:
                continue
            size = e["size"] or 20
            text = e["text"].replace("<b>", "").replace("</b>", "")
            f = font(size, bold=size >= 40)
            align = e.get("align", "Center")
            anchor_x = {"Left": x0, "Right": x1}.get(align, (x0 + x1) / 2)
            anchor = {"Left": "lm", "Right": "rm"}.get(align, "mm")
            if " / " in text:   # multi-line (credits body)
                lines = text.split(" / ")
                line_h = size * 1.2
                top = (y0 + y1) / 2 - line_h * len(lines) / 2 + line_h / 2
                for i, line in enumerate(lines):
                    d.text((anchor_x, top + i * line_h), line, font=f, fill=color, anchor=anchor)
            else:
                d.text((anchor_x, (y0 + y1) / 2), text, font=f, fill=color, anchor=anchor)
        else:
            if color[3] < 5:
                continue
            fill_w = x0 + (x1 - x0) * e.get("fill", 1.0)
            d.rectangle([x0, y0, fill_w, y1], fill=color)
            if e["kind"] == "button" or (x1 - x0) * (y1 - y0) > 4000:
                # Sketch outline: shows each block's edges like a wireframe.
                d.rectangle([x0, y0, x1, y1], outline=(232, 225, 211, 70), width=1)
    img.alpha_composite(layer)


def annotate(img, elements, notes, title, issue):
    d = ImageDraw.Draw(img)
    accent = hex_rgb("#DBAD54")
    legend = []
    for i, (name, text, note) in enumerate(notes, start=1):
        # If several elements match (the same item name in a slot and in the details), take the biggest text.
        matches = [e for e in elements if e["name"] == name and (text is None or text in e["text"])]
        match = max(matches, key=lambda e: e["size"]) if matches else None
        legend.append((i, note, match is not None))
        if match is None:
            continue
        x0, y0 = match["r"][0], match["r"][1]
        cx, cy = max(18, x0 - 16), max(18, y0 - 6)
        d.ellipse([cx - 15, cy - 15, cx + 15, cy + 15], fill=(*accent, 255), outline=(14, 15, 18, 255), width=3)
        d.text((cx, cy), str(i), font=font(18, True), fill=(14, 15, 18, 255), anchor="mm")
    # Header and legend box.
    d.rectangle([0, 0, W, 54], fill=(14, 15, 18, 235))
    d.text((24, 27), f"Boceto · {title}", font=font(28, True), fill=(*accent, 255), anchor="lm")
    d.text((W - 24, 27), f"Ánimas mexicanas · issue {issue} · 1920×1080", font=font(20), fill=(232, 225, 211, 255), anchor="rm")
    box_w, line_h = 560, 30
    bx, by = W - box_w - 20, 70
    d.rectangle([bx, by, bx + box_w, by + 16 + line_h * len(legend)], fill=(14, 15, 18, 225), outline=(*accent, 160), width=2)
    for k, (i, note, found) in enumerate(legend):
        y = by + 8 + k * line_h + line_h / 2
        d.text((bx + 14, y), f"{i}", font=font(18, True), fill=(*accent, 255), anchor="lm")
        d.text((bx + 40, y), note, font=font(17), fill=(232, 225, 211, 255 if found else 120), anchor="lm")


def draw_screen(name, title, issue, over_game, notes):
    elements = json.load(open(os.path.join(LAYOUTS, name + ".json"), encoding="utf-8"))
    img = game_backdrop() if over_game else Image.new("RGBA", (W, H), (0, 0, 0, 255))
    draw_layout(img, elements)
    annotate(img, elements, notes, title, issue)
    img.convert("RGB").save(os.path.join(OUT, f"boceto_{name}.png"))


def draw_palette():
    img = Image.new("RGB", (W, H), hex_rgb("#0E0F12"))
    d = ImageDraw.Draw(img)
    text, accent = hex_rgb("#E8E1D3"), hex_rgb("#DBAD54")
    d.text((60, 50), "Paleta de las pantallas", font=font(48, True), fill=accent)
    d.text((60, 110), "Ánimas mexicanas · issue #2 · los mismos colores que usan el HUD y los menús en Unity", font=font(22), fill=text)
    # Base colors.
    for i, (label, hexc, use) in enumerate(PALETTE):
        x, y = 60 + (i % 3) * 600, 170 + (i // 3) * 150
        d.rectangle([x, y, x + 110, y + 110], fill=hex_rgb(hexc), outline=(*text,), width=1)
        d.text((x + 130, y + 10), label, font=font(24, True), fill=text)
        d.text((x + 130, y + 44), hexc, font=font(22), fill=accent)
        d.text((x + 130, y + 76), use, font=font(15), fill=text)
    # Variants per instance, each with a mini HUD applied on its own background.
    d.text((60, 650), "Variante por instancia (fondo · tono · niebla o metal · acento) y el HUD aplicado", font=font(26, True), fill=accent)
    for i, (label, desc, colors) in enumerate(VARIANTS):
        x, y = 60 + i * 610, 700
        panel = [x, y, x + 580, y + 340]
        d.rectangle(panel, fill=hex_rgb(colors[0]))
        d.text((x + 20, y + 16), label, font=font(24, True), fill=text)
        d.text((x + 20, y + 50), desc, font=font(15), fill=text)
        for k, c in enumerate(colors):
            d.rectangle([x + 20 + k * 90, y + 82, x + 100 + k * 90, y + 142], fill=hex_rgb(c), outline=text, width=1)
            d.text((x + 60 + k * 90, y + 152), c, font=font(14), fill=text, anchor="ma")
        # Mini HUD: life, stamina, 4 quick slots, all in the base palette over the instance colors.
        d.text((x + 20, y + 190), "Vida", font=font(16), fill=text)
        d.text((x + 260, y + 190), "65", font=font(16), fill=text, anchor="ra")
        d.rectangle([x + 20, y + 212, x + 260, y + 226], fill=(0, 0, 0))
        d.rectangle([x + 20, y + 212, x + 176, y + 226], fill=hex_rgb("#B3261E"))
        d.text((x + 20, y + 240), "Resistencia", font=font(16), fill=text)
        d.rectangle([x + 20, y + 262, x + 220, y + 270], fill=(0, 0, 0))
        d.rectangle([x + 20, y + 262, x + 190, y + 270], fill=hex_rgb("#C9A327"))
        d.text((x + 230, y + 258), "Rezo listo [R]", font=font(14), fill=hex_rgb("#C9A327"))
        for k in range(4):
            sx = x + 340 + k * 56
            d.rectangle([sx, y + 220, sx + 48, y + 268], fill=hex_rgb("#0E0F12"), outline=hex_rgb(colors[1]), width=2)
            d.text((sx + 5, y + 222), str(k + 1), font=font(13), fill=hex_rgb("#C9A327"))
        d.rectangle([x + 340, y + 290, x + 556, y + 300], fill=(0, 0, 0))
        d.rectangle([x + 340, y + 290, x + 460, y + 300], fill=hex_rgb("#8CBFF2"))
        d.text((x + 340, y + 306), "Ritual", font=font(13), fill=text)
    img.save(os.path.join(OUT, "paleta.png"))


if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    draw_palette()
    for screen in SCREENS:
        draw_screen(*screen)
    print("Bocetos en", OUT)
