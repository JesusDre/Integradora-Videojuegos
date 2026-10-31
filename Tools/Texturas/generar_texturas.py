"""Tileable textures for the level materials (issue #101), generated with numpy.

Run from the repo root:
    python3 Tools/Texturas/generar_texturas.py
Writes Assets/_Project/Art/Textures/Niveles/T_<name>_Albedo.png and T_<name>_Normal.png (512 x 512).

The albedo is almost white with darker detail (cracks, joints, grain): in Unity it MULTIPLIES the material's
base color, so each material keeps its palette color and only gains texture. The normal map is made from
a height map so the flashlight catches the joints and the grain. Everything tiles because the noise is
built in the frequency domain (periodic by construction) and the patterns (bricks, planks, stones) wrap.
"""

import os

import numpy as np
from PIL import Image

N = 512
OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "_Project", "Art", "Textures", "Niveles")
rng = np.random.default_rng(101)
yy, xx = np.mgrid[0:N, 0:N] / N          # 0..1, y down


def fbm(scale=8.0, falloff=1.8):
    """Tileable fractal noise: white noise shaped by 1/f^falloff in the frequency domain."""
    f = np.fft.fftfreq(N)[:, None] ** 2 + np.fft.fftfreq(N)[None, :] ** 2
    f = np.sqrt(f) * N
    spec = np.fft.fft2(rng.normal(size=(N, N))) / np.maximum(f, 1.0) ** falloff
    spec *= np.exp(-(f / (N / scale * 4)) ** 2) if scale else 1
    x = np.real(np.fft.ifft2(spec))
    return (x - x.min()) / (x.max() - x.min())


def voronoi(cells, jitter=0.9):
    """Wrapping Voronoi: distance to the nearest and second-nearest point, and the cell id."""
    pts = (np.stack(np.meshgrid(np.arange(cells), np.arange(cells)), -1).reshape(-1, 2)
           + 0.5 + rng.uniform(-jitter / 2, jitter / 2, (cells * cells, 2))) / cells
    d1 = np.full((N, N), 9.0)
    d2 = np.full((N, N), 9.0)
    ids = np.zeros((N, N), int)
    for i, (px, py) in enumerate(pts):
        dx = np.abs(xx - px)
        dy = np.abs(yy - py)
        d = np.sqrt(np.minimum(dx, 1 - dx) ** 2 + np.minimum(dy, 1 - dy) ** 2)
        closer = d < d1
        d2 = np.where(closer, d1, np.minimum(d2, d))
        ids = np.where(closer, i, ids)
        d1 = np.where(closer, d, d1)
    return d1, d2, ids


def normal_map(height, strength=4.0):
    """Tangent-space normals from a height map (wrapping differences, so it tiles too)."""
    dx = (np.roll(height, -1, 1) - np.roll(height, 1, 1)) * strength
    dy = (np.roll(height, -1, 0) - np.roll(height, 1, 0)) * strength
    n = np.stack([-dx, dy, np.ones_like(height)], -1)
    n /= np.linalg.norm(n, axis=-1, keepdims=True)
    return n * 0.5 + 0.5


def save(name, albedo, height, strength=4.0):
    os.makedirs(OUT, exist_ok=True)
    a = np.clip(albedo, 0, 1)
    if a.ndim == 2:
        a = np.stack([a] * 3, -1)
    Image.fromarray((a * 255).astype(np.uint8)).save(os.path.join(OUT, f"T_{name}_Albedo.png"))
    Image.fromarray((normal_map(height, strength) * 255).astype(np.uint8)).save(os.path.join(OUT, f"T_{name}_Normal.png"))
    print("T_" + name)


def per_cell(ids, low, high):
    """A random value per cell id (each brick/stone/plank its own shade)."""
    values = rng.uniform(low, high, ids.max() + 1)
    return values[ids]


# Dirt (camino, terracería, chinampa): mottled earth, pebbles and small cracks. Tile: 3 m.
base = fbm(16)
fine = fbm(0, 1.2)
d1, d2, _ = voronoi(40, 1.0)
pebbles = np.clip(1 - d1 * 90, 0, 1) * (rng.uniform(size=(N, N)) > -1)
cracks = np.clip(1 - (d2 - d1) * 250, 0, 1) * np.clip((fbm(6) - 0.7) * 6, 0, 1)
height = base * 0.6 + fine * 0.25 + pebbles * 0.4 - cracks * 0.3
save("Tierra", 0.72 + base * 0.22 + fine * 0.1 + pebbles * 0.08 - cracks * 0.25, height, 3.0)

# Grassy ground (chinampa, jardín): dirt with tufts of darker grass streaks.
streaks = fbm(0, 1.0)
grass = np.clip((fbm(10) - 0.45) * 4, 0, 1)
save("Pasto", 0.7 + base * 0.15 + (streaks - 0.5) * 0.25 * (0.4 + grass) - grass * 0.15, height * 0.5 + streaks * grass * 0.6, 3.0)

# Wood planks (embarcadero, puente, vigas): 4 planks across, grain along X, a knot now and then. Tile: 2 m.
planks = 4
row = np.floor(yy * planks).astype(int)
offset = rng.uniform(0, 1, planks)[row]
v = (yy * planks) % 1
grain = np.sin((yy * planks * 40 + fbm(4) * 6) * np.pi) * 0.5 + 0.5
grain = grain * 0.6 + fbm(0, 1.0) * 0.4
seam_x = ((xx + offset) % 0.5) < 0.004
gap = (np.minimum(v, 1 - v) < 0.02) | seam_x
shade = rng.uniform(0.8, 1.0, planks)[row]
knots = np.zeros((N, N))
for _ in range(6):
    kx, ky = rng.uniform(0, 1, 2)
    d = np.sqrt(np.minimum(np.abs(xx - kx), 1 - np.abs(xx - kx)) ** 2 * 4 + np.minimum(np.abs(yy - ky), 1 - np.abs(yy - ky)) ** 2 * 30)
    knots += np.clip(1 - d * 20, 0, 1)
albedo = shade * (0.75 + grain * 0.25) - knots * 0.3
albedo = np.where(gap, 0.25, albedo)
save("Madera", albedo, np.where(gap, 0.0, 0.6 + grain * 0.15 - knots * 0.1), 5.0)

# Adobe (walls of the hacienda and huts): big mud bricks under worn plaster that has fallen in patches. Tile: 2 m.
rows = 8
r = np.floor(yy * rows).astype(int)
bx = (xx * 4 + (r % 2) * 0.5) % 1
by = (yy * rows) % 1
joint = (np.minimum(bx, 1 - bx) < 0.025) | (np.minimum(by, 1 - by) < 0.05)
brick_id = r * 8 + np.floor((xx * 4 + (r % 2) * 0.5) % 4).astype(int)
brick_shade = per_cell(brick_id, 0.82, 1.0)
plaster = fbm(8) > 0.48
straw = (fbm(0, 0.9) > 0.8) * 0.08
mud = brick_shade * (0.8 + fbm(0, 1.2) * 0.2) - straw
albedo = np.where(plaster, 0.95 + fbm(0, 1.3) * 0.05, np.where(joint, 0.6, mud))
height = np.where(plaster, 0.8, np.where(joint, 0.3, 0.6 + fbm(0, 1.2) * 0.1))
save("Adobe", albedo, height, 4.0)

# Stone masonry (convent walls, tombs, crypt): irregular blocks with deep joints and lichen stains. Tile: 2 m.
d1, d2, ids = voronoi(7, 0.7)
edge = d2 - d1
joint = edge < 0.012
stone_shade = per_cell(ids, 0.72, 1.0)
rough = fbm(0, 1.4)
stains = np.clip((fbm(6) - 0.6) * 3, 0, 1)
albedo = np.where(joint, 0.35, stone_shade * (0.8 + rough * 0.2) - stains * 0.2)
height = np.where(joint, 0.0, np.clip(edge * 12, 0, 1) * 0.5 + rough * 0.3)
save("Piedra", albedo, height, 6.0)

# Flagstone floor (convent nave, patio): square slabs, worn and cracked. Tile: 2 m (4 x 4 slabs).
slabs = 4
sx = (xx * slabs) % 1
sy = (yy * slabs) % 1
slab_id = np.floor(yy * slabs).astype(int) * slabs + np.floor(xx * slabs).astype(int)
joint = (np.minimum(sx, 1 - sx) < 0.015) | (np.minimum(sy, 1 - sy) < 0.015)
d1, d2, _ = voronoi(12, 1.0)
crack = (d2 - d1 < 0.003) & (fbm(5) > 0.72)
wear = fbm(10)
albedo = np.where(joint, 0.4, per_cell(slab_id, 0.78, 1.0) * (0.85 + wear * 0.15)) - crack * 0.3
height = np.where(joint, 0.0, 0.6 + wear * 0.2) - crack * 0.4
save("Losa", albedo, height, 5.0)

# Water: no color (the material keeps its dark green); only ripples for the normal map.
ripples = fbm(12, 2.2) * 0.7 + fbm(0, 1.6) * 0.3
save("Agua", np.ones((N, N)), ripples, 2.0)
