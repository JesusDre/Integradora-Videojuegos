"""The jump-scare sting (issue #137): a hard, dissonant hit for the moment the Llorona or the Charro
catches the Padre. Run from the repo root (then Tools/Audio/nivelar.py):
    python3 Tools/Audio/sintetizar_sustos.py
"""

import os
import sys

import numpy as np

sys.path.append(os.path.dirname(os.path.abspath(__file__)))
import sintesis  # noqa: E402
from sintesis import band, drive, env, glide, mix, noise, reverb, save, thump, tone  # noqa: E402

sintesis.rng = np.random.default_rng(137)
L = 1.8
# A wall of noise that hits at once, a low boom under it, and a screech of out-of-tune high voices
# sliding down: the three things that make a scare land.
blast = band(noise(L), 300, 9000) * env(L, 0.003, 0.35)
boom = thump(80, 30, L, 0.5)
screech = sum(tone(glide(f, f * 0.7, L, 0.6), L, "saw") for f in (1250, 1330, 1870, 2650))
screech = band(screech, 900, 7000) * env(L, 0.01, 0.9, 0.15)
save("SFX/Enemigos", "susto", reverb(drive(mix(L, (blast, 0, 0.9), (boom, 0, 1.4), (screech, 0.02, 0.8)), 2.6), 1.5, 0.3), peak=0.97)
