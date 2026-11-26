"""PS1-style detail textures for the models made in Blender (PSX textures issue), generated with numpy.

Run from the repo root:
    python3 Tools/Texturas/texturas_psx.py
Writes Assets/_Project/Art/Textures/PSX/T_PSX_<kind>.png, 128 x 128.

Why they look "PS1": tiny (128 px), few gray levels with ordered (Bayer) dithering, and in Unity they use
Point filtering, so the pixels show. They are almost white with darker detail: Unity MULTIPLIES them by
each material's color, so a red boat stays red and a gray stone stays gray; they only gain texture.
All of them tile (the noise is periodic and the patterns wrap).
"""

import os

import numpy as np
from PIL import Image

N = 128
OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "_Project", "Art", "Textures", "PSX")
rng = np.random.default_rng(1999)
yy, xx = np.mgrid[0:N, 0:N] / N

# 4x4 Bayer matrix: the classic ordered dither of old consoles.
BAYER = np.array([[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]) / 16.0 - 0.5


def noise(scale, falloff=1.6):
    """Tileable fractal noise (shaped white noise in the frequency domain), normalized to 0..1."""
    f = np.fft.fftfreq(N)[:, None] ** 2 + np.fft.fftfreq(N)[None, :] ** 2
    f[0, 0] = 1
    spectrum = np.fft.fft2(rng.normal(size=(N, N))) / (f ** (falloff / 2)) * np.exp(-f * (N / scale) ** 2 * 0.002)
    n = np.real(np.fft.ifft2(spectrum))
    return (n - n.min()) / (n.max() - n.min())


def finish(name, value, levels=7):
    """Quantize to a few gray levels with ordered dithering and save."""
    v = np.clip(value, 0, 1)
    d = np.tile(BAYER, (N // 4, N // 4))
    q = np.floor(v * (levels - 1) + 0.5 + d * 0.9) / (levels - 1)
    img = (np.clip(q, 0, 1) * 255).astype(np.uint8)
    Image.fromarray(img, "L").convert("RGB").save(os.path.join(OUT, f"T_PSX_{name}.png"))
    print(f"[psx] T_PSX_{name}.png")


os.makedirs(OUT, exist_ok=True)

# Wood: grain running along one direction, knots, and dark seams between planks.
grain = np.sin((yy + noise(6) * 0.25) * np.pi * 2 * 14) * 0.5 + 0.5
planks = (np.abs(((xx * 4) % 1) - 0.5) > 0.47).astype(float)
finish("Madera", 0.95 - grain * 0.22 - noise(20) * 0.15 - planks * 0.45)

# Stone: lumpy noise and a few dark cracks.
cracks = (np.abs(noise(10) - 0.5) < 0.015).astype(float)
finish("Piedra", 0.98 - noise(24) * 0.35 - noise(6) * 0.15 - cracks * 0.5)

# Metal: brushed streaks, a few scratches and rust stains.
streaks = noise(64, 0.8)[:, :1].repeat(N, 1) * 0.15
scratches = (np.abs(np.sin((xx * 3 + yy * 7) * np.pi * 2 * 3 + noise(8) * 4)) > 0.995).astype(float)
finish("Metal", 0.97 - streaks - scratches * 0.3 - (noise(12) > 0.72) * 0.25)

# Cloth and rope: a woven cross pattern and dirt.
weave = (np.sin(xx * np.pi * 2 * 32) * np.sin(yy * np.pi * 2 * 32) > 0).astype(float)
finish("Tela", 0.96 - weave * 0.12 - noise(16) * 0.3)

# Plants, straw and grass: thin vertical blades of different shades.
blades = noise(64, 0.6)[:1, :].repeat(N, 0)
finish("Vegetal", 0.95 - blades * 0.35 - noise(10) * 0.2)

# Painted wood: the paint is chipped and shows dark wood underneath.
chips = (noise(18) > 0.68).astype(float)
finish("Pintura", 0.97 - chips * 0.5 - noise(30) * 0.12)

# Generic grime: soft stains, for wax, porcelain, clay and anything else.
finish("Mugre", 0.98 - noise(14) * 0.28 - (noise(40) > 0.8) * 0.12)
