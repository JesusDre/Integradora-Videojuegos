"""360° panoramic skies for each instance (issue #102), generated with numpy.

Run from the repo root:
    python3 Tools/Texturas/generar_cielos.py
Writes Assets/_Project/Art/Textures/Cielos/T_Cielo_<instance>.png (4096 x 2048, latitude-longitude), for
Unity's Skybox/Panoramic shader.

Two rules keep the sky coherent with the level:
- Below the horizon and a few degrees above it the sky IS the fog color of the scene, so the far edge of
  the fog melts into the sky instead of ending in a black cut (Unity's fog does not touch the skybox).
- The moon or the sun is drawn exactly where the scene's directional light comes from, so shadows and
  the bright spot in the sky agree.
"""

import math
import os

import numpy as np
from PIL import Image

W, H = 4096, 2048
OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "_Project", "Art", "Textures", "Cielos")
rng = np.random.default_rng(102)

# Direction of every pixel, with the same mapping as Unity's panoramic shader:
# u = 0.5 - atan2(z, x) / 2pi, v = 1 - acos(y) / pi (row 0 is the top of the image).
u = (np.arange(W) + 0.5) / W
v = 1 - (np.arange(H) + 0.5) / H
lon = (0.5 - u) * 2 * math.pi
lat_from_top = (1 - v) * math.pi
X = np.sin(lat_from_top)[:, None] * np.cos(lon)[None, :]
Y = np.repeat(np.cos(lat_from_top)[:, None], W, 1)
Z = np.sin(lat_from_top)[:, None] * np.sin(lon)[None, :]
elev = np.degrees(np.arcsin(np.clip(Y, -1, 1)))          # -90 (down) .. 90 (up)


def fbm(scale=6.0, falloff=1.9, stretch=2.0):
    """Fractal noise that wraps around horizontally (no seam at the back of the sky)."""
    fy = np.fft.fftfreq(H)[:, None] * H
    fx = np.fft.fftfreq(W)[None, :] * W * stretch / 4
    f = np.sqrt(fx ** 2 + fy ** 2)
    spec = np.fft.fft2(rng.normal(size=(H, W))) / np.maximum(f, 1.0) ** falloff
    spec *= np.exp(-(f / (scale * 40)) ** 2)
    x = np.real(np.fft.ifft2(spec))
    return (x - x.min()) / (x.max() - x.min())


def light_to_sky(pitch, yaw):
    """Where a directional light with these Euler angles comes FROM (the moon or the sun)."""
    p, y = math.radians(pitch), math.radians(yaw)
    forward = np.array([math.sin(y) * math.cos(p), -math.sin(p), math.cos(y) * math.cos(p)])
    return -forward


def disc(direction, radius_deg, soft_deg):
    """1 inside a disc around `direction`, fading to 0 over soft_deg."""
    cos_angle = X * direction[0] + Y * direction[1] + Z * direction[2]
    ang = np.degrees(np.arccos(np.clip(cos_angle, -1, 1)))
    return np.clip(1 - (ang - radius_deg) / max(soft_deg, 1e-3), 0, 1), ang


def srgb(hex_or_rgb):
    if isinstance(hex_or_rgb, str):
        h = hex_or_rgb.lstrip("#")
        return np.array([int(h[i:i + 2], 16) / 255 for i in (0, 2, 4)])
    return np.array(hex_or_rgb, float)


def gradient(stops):
    """stops: [(elevation_deg, color), ...] from low to high; returns H x W x 3."""
    out = np.zeros((H, W, 3))
    es = [s[0] for s in stops]
    for c in range(3):
        out[..., c] = np.interp(elev, es, [s[1][c] for s in stops])
    return out


def stars(count, max_size, color, min_elev=8):
    img = np.zeros((H, W))
    for _ in range(count):
        e = math.degrees(math.asin(rng.uniform(math.sin(math.radians(min_elev)), 1)))
        row = int((90 - e) / 180 * H)
        col = int(rng.uniform(0, W))
        b = rng.uniform(0.2, 1.0) ** 2
        s = 1 if rng.uniform() < 0.9 else max_size
        img[max(row - s // 2, 0):row + s // 2 + 1, col:col + s] = np.maximum(img[max(row - s // 2, 0):row + s // 2 + 1, col:col + s], b)
    return img[..., None] * np.array(color)[None, None, :]


def fog_blend(sky, fog, start, end):
    """Below `start` degrees the sky is exactly the fog color; it blends to the sky by `end`."""
    t = np.clip((elev - start) / (end - start), 0, 1)[..., None]
    t = t * t * (3 - 2 * t)
    return fog[None, None, :] * (1 - t) + sky * t


def save(name, img):
    os.makedirs(OUT, exist_ok=True)
    Image.fromarray((np.clip(img, 0, 1) * 255).astype(np.uint8)).save(os.path.join(OUT, f"T_Cielo_{name}.png"))
    print("T_Cielo_" + name)


# ---- Xochimilco: cold night with a big pale moon behind thin clouds, a few stars, white-green mist low.
fog = srgb((0.15, 0.19, 0.20))
moon_dir = light_to_sky(40, 200)
sky = gradient([(-90, fog), (0, fog), (10, srgb("#1E2B2E")), (35, srgb("#101A1F")), (90, srgb("#070C10"))])
sky += stars(2500, 2, (0.75, 0.82, 0.9))
halo, _ = disc(moon_dir, 0, 35)
sky += (halo ** 3)[..., None] * srgb("#3E5458") * 0.8
moon, ang = disc(moon_dir, 2.2, 0.15)
craters = fbm(30, 1.4)
sky = sky * (1 - moon[..., None]) + moon[..., None] * (srgb("#D8DED6") * (0.85 + craters[..., None] * 0.15))
clouds = np.clip((fbm(5, 2.0, 3.0) - 0.5) * 2.5, 0, 1) * np.clip((elev - 5) / 25, 0, 1) * np.clip((70 - elev) / 30, 0, 1)
lit = np.clip(1 - ang / 50, 0, 1)[..., None]
cloud_color = srgb("#1A2629") + lit * srgb("#3A4A4C")
sky = sky * (1 - clouds[..., None] * 0.85) + clouds[..., None] * 0.85 * cloud_color
save("Xochimilco", fog_blend(sky, fog, 1, 14))

# ---- Hacienda: a sunset that is dying out. Burnt orange and blood red at the horizon on the sun's
# side, ocher and violet above, the dark of night already coming from the opposite side.
fog = srgb((0.26, 0.19, 0.15))
sun_dir = light_to_sky(12, 250)
_, ang = disc(sun_dir, 0, 1)
toward_sun = np.clip(1 - ang / 120, 0, 1)[..., None]
warm = gradient([(-90, fog), (0, srgb("#7A3A1E")), (6, srgb("#A04A22")), (15, srgb("#6A3524")), (35, srgb("#3A2230")), (90, srgb("#1A1220"))])
cold = gradient([(-90, fog), (0, srgb("#3A2A26")), (15, srgb("#251C22")), (40, srgb("#140F18")), (90, srgb("#0C0A12"))])
sky = cold * (1 - toward_sun ** 1.5) + warm * toward_sun ** 1.5
glow, _ = disc(sun_dir, 0, 25)
sky += (glow ** 2)[..., None] * srgb("#C0602A") * 0.6
sun, _ = disc(sun_dir, 1.6, 0.6)
sky = sky * (1 - sun[..., None]) + sun[..., None] * srgb("#F0A060")
bands = np.clip((fbm(4, 2.1, 6.0) - 0.45) * 3, 0, 1) * np.clip((elev - 3) / 10, 0, 1) * np.clip((40 - elev) / 25, 0, 1)
band_color = srgb("#2A1A1A") + toward_sun * srgb("#7A3420")
sky = sky * (1 - bands[..., None] * 0.8) + bands[..., None] * 0.8 * band_color
sky += stars(400, 1, (0.6, 0.55, 0.6), 35) * (1 - toward_sun)
save("Hacienda", fog_blend(sky, fog, 1, 10))

# ---- Ex-convento: closed night. Heavy clouds, no moon in sight, and between the clouds a few stars
# with a reddish tint (the tzitzimime are star beings) around a black eight-pointed star overhead.
fog = srgb((0.11, 0.09, 0.10))
sky = gradient([(-90, fog), (0, fog), (12, srgb("#0E0B0D")), (90, srgb("#050405"))])
gaps = np.clip((fbm(6, 2.0, 2.5) - 0.55) * 4, 0, 1) * np.clip((elev - 15) / 20, 0, 1)
sky += stars(3000, 2, (0.75, 0.55, 0.55), 15) * gaps[..., None]
heavy = np.clip((fbm(4, 2.0, 2.5) - 0.3) * 2, 0, 1)
sky = sky * (1 - heavy[..., None] * 0.7) + heavy[..., None] * 0.7 * srgb("#141012")
# The black star: an eight-pointed shape, darker than the night, with a faint red rim, high to the north.
star_dir = np.array([0.0, math.sin(math.radians(62)), math.cos(math.radians(62))])
_, ang = disc(star_dir, 0, 1)
side = np.cross(star_dir, [0, 1, 0]); side /= np.linalg.norm(side)
up = np.cross(side, star_dir)
theta = np.arctan2(X * up[0] + Y * up[1] + Z * up[2], X * side[0] + Y * side[1] + Z * side[2])
radius = 4.0 * (0.55 + 0.45 * np.abs(np.cos(4 * theta)) ** 6)
inside = np.clip((radius - ang) / 0.3, 0, 1)[..., None]
rim = np.clip(1 - np.abs(ang - radius) / 0.8, 0, 1)[..., None] * (1 - inside)
sky = sky * (1 - inside) + inside * srgb("#020102") + rim * srgb("#4A0E0E") * 0.7
save("Convento", fog_blend(sky, fog, 1, 16))
