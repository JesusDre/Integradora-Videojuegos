"""Sounds of the weapons added in #135: pistola, metralleta, ballesta and the squeaky toy hammer.

Run from the repo root (then Tools/Audio/nivelar.py):
    python3 Tools/Audio/sintetizar_armas.py
Uses the shared building blocks of sintesis.py, with its own seed so it never changes other sounds.
"""

import os
import sys

import numpy as np

sys.path.append(os.path.dirname(os.path.abspath(__file__)))
import sintesis  # noqa: E402
from sintesis import band, bell, drive, env, glide, mix, noise, reverb, save, thump, tone  # noqa: E402

sintesis.rng = np.random.default_rng(135)


def shot(length, crack, body_decay, low, tail):
    """A gunshot: a short crack, a mid bang that dies fast, a low push and a tail."""
    c = band(noise(0.02), 1500, 9000) * env(0.02, 0.0001, 0.003) * crack
    bang = band(noise(length), 400, 5000) * env(length, 0.0005, body_decay)
    push = thump(150 * low, 55 * low, length, 0.08)
    t = band(noise(length), 200, 1500) * env(length, 0.01, tail) * 0.2
    return drive(mix(length, (c, 0, 1.3), (bang, 0, 1.0), (push, 0, 1.0), (t, 0.004, 1.0)), 1.6)


def click(f, length=0.08, gain=1.0):
    return band(noise(length), f, f * 3) * env(length, 0.0003, 0.012) * gain + 0.2 * bell(f, length, decays=(0.2, 0.1, 0.1, 0.1))


# Pistola: drier and shorter than the escopeta.
save("SFX/Armas", "pistola_disparo", reverb(shot(0.6, 1.2, 0.07, 1.2, 0.25), 0.9, 0.2))
# Metralleta: very short, so the bursts don't smear into a roar.
save("SFX/Armas", "metralleta_disparo", reverb(shot(0.25, 1.0, 0.035, 1.4, 0.08), 0.5, 0.12))
# Magazine reload: out, in, slide.
save("SFX/Armas", "cargador_recarga", mix(1.2, (click(1400), 0.0, 1.0), (band(noise(0.15), 1500, 5000) * env(0.15, 0.03, 0.06), 0.3, 0.5),
                                           (click(1900), 0.55, 1.2), (click(1100), 0.95, 1.0)))

# Ballesta: no bang, the twang of the string and the bolt whistling away.
twang = tone(glide(180, 120, 0.5), 0.5, "saw") * env(0.5, 0.002, 0.15)
twang = band(twang, 120, 2500) + band(noise(0.5), 800, 4000) * env(0.5, 0.001, 0.03)
whoosh = band(noise(0.4), 1500, 6000) * np.hanning(int(0.4 * sintesis.SR)) * 0.4
save("SFX/Armas", "ballesta_disparo", mix(0.7, (twang, 0, 1.0), (whoosh, 0.05, 1.0)))
# Ballesta reload: cranking the string back (ratchet clicks) and the bolt sliding in.
crank = np.zeros(int(1.8 * sintesis.SR))
for i in range(10):
    crank += mix(1.8, (click(2200, 0.05, 0.7), 0.1 + i * 0.12, 1.0))
save("SFX/Armas", "ballesta_recarga", mix(1.8, (crank, 0, 1.0), (band(noise(0.25), 800, 3000) * env(0.25, 0.04, 0.1), 1.45, 0.6), (click(1500), 1.65, 1.0)))

# Martillo chillón: the squeak of a toy hammer (air through a rubber whistle: a fast rising-falling
# tone with a buzzy edge) over a soft rubbery thud, and a faint bell (it is blessed).
sq_len = 0.35
n = int(sq_len * sintesis.SR)
rise = int(0.12 * sintesis.SR)
pitch = np.concatenate([glide(900, 1700, 0.12)[:rise], glide(1700, 1200, sq_len)[: n - rise]])
squeak = tone(pitch, sq_len, "square") * 0.5 + tone(pitch * 2, sq_len) * 0.3
squeak = band(squeak, 600, 6000) * env(sq_len, 0.01, 0.22, 0.05)
thud = thump(220, 140, 0.3, 0.05) * 0.6
save("SFX/Armas", "martillo_chillido", mix(0.9, (thud, 0, 1.0), (squeak, 0.01, 1.0), (bell(2400, 0.8) * 0.12, 0.02, 1.0)))
save("SFX/Armas", "martillo_swing", band(noise(0.3), 500, 2500) * np.hanning(int(0.3 * sintesis.SR)) * 0.6)
