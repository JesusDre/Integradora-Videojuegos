"""Sounds of the minor enemies (#172–#174): the possessed doll, the chaneques and the ceiling spiders.

Run from the repo root (then Tools/Audio/nivelar.py):
    python3 Tools/Audio/sintetizar_enemigos.py
Uses the shared building blocks of sintesis.py, with its own seed so it never changes other sounds.
The dolls laugh with the MBROLA laughs approved in #123 (SFX/Enemigos/muneca_risa_*.wav).
"""

import os
import sys

import numpy as np

sys.path.append(os.path.dirname(os.path.abspath(__file__)))
import sintesis  # noqa: E402
from sintesis import band, bell, env, mix, noise, reverb, save, thump  # noqa: E402

sintesis.rng = np.random.default_rng(172)

# ---- Porcelain breaking (#172): a dull crack, then shards tinkling as they scatter and settle.
length = 1.4
crack = band(noise(0.08), 900, 7000) * env(0.08, 0.0005, 0.02) * 1.4
body = thump(260, 120, 0.25, 0.05) * 0.5
parts = [(crack, 0.0, 1.0), (body, 0.0, 1.0)]
for i in range(14):
    f = sintesis.rng.uniform(2200, 6500)
    start = 0.02 + sintesis.rng.exponential(0.18)
    if start > length - 0.3:
        continue
    parts.append((bell(f, 0.25, decays=(0.08, 0.05, 0.04, 0.03)) * sintesis.rng.uniform(0.15, 0.4), start, 1.0))
save("SFX/Enemigos", "muneca_rompe", reverb(mix(length, *parts), 0.8, 0.18))
