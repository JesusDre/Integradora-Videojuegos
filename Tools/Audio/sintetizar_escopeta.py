"""The shotgun blast (Jesús asked for a new one after #115: "suena raro").

Run from the repo root:
    python3 Tools/Audio/sintetizar_escopeta.py
It makes the five candidates Jesús listened to (A dry, B echo, C heavy, C1 heavy sustained, C2 very
sustained), always in the same order and with the same seed, so they come out identical every time, and
saves the chosen one, C1, as Assets/_Project/Audio/SFX/Armas/escopeta_disparo.wav. The others go to
Tools/Audio/escopeta_candidatas/ (ignored by git) in case someone wants to compare again.
"""
import os
import sys

import numpy as np

sys.path.append(os.path.dirname(os.path.abspath(__file__)))
import sintesis  # noqa: E402
from sintesis import band, drive, env, mix, noise, place, reverb, thump  # noqa: E402

sintesis.rng = np.random.default_rng(77)
sintesis.ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "escopeta_candidatas")
GAME = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "Assets", "_Project", "Audio")


def blast(length=0.9, low=1.0, crack_gain=1.0, body_decay=0.12, tail_decay=0.45):
    """A real gunshot is three things at once: a very short supersonic crack, a dense mid "bang" that
    dies fast, and a low push of air. Less distortion than before: clipping is what made it sound odd."""
    crack = band(noise(0.03), 1500, 9000) * env(0.03, 0.0001, 0.004) * crack_gain
    bang = band(noise(length), 300, 4000) * env(length, 0.0005, body_decay)
    push = thump(120 * low, 40 * low, length, 0.18) + band(noise(length), 40, 250) * env(length, 0.002, 0.25)
    tail = band(noise(length), 150, 1200) * env(length, 0.01, tail_decay) * 0.25
    return drive(mix(length, (crack, 0, 1.4), (bang, 0, 1.0), (push, 0, 1.3), (tail, 0.005, 1.0)), 1.4)


def pump(length=0.6):
    """Opening and closing the double barrel after the shot (metal clicks)."""
    click = lambda f: band(noise(0.06), f, f * 3) * env(0.06, 0.0003, 0.012)
    return mix(length, (click(1800), 0.0, 0.5), (click(1200), 0.09, 0.4))


# A · Seca y potente: close, punchy, a short room.
a = reverb(blast(), 0.6, 0.15)
# B · Eco de campo: the same blast in the open, with the echo coming back from far away.
b_dry = blast(1.0, tail_decay=0.6)
b = mix(3.2, (b_dry, 0, 1.0), (band(b_dry, 150, 2500), 0.32, 0.35), (band(b_dry, 100, 1500), 0.75, 0.18))
b = reverb(b, 1.8, 0.25)
# C · Pesada: deeper and longer push, a bigger crack, and the barrel clicking after.
c = mix(1.6, (reverb(blast(1.0, low=0.8, crack_gain=1.3, body_decay=0.16, tail_decay=0.6), 0.9, 0.2), 0, 1.0), (pump(), 0.95, 1.0))

for name, x in (("escopeta_A_seca", a), ("escopeta_B_eco", b), ("escopeta_C_pesada", c)):
    sintesis.save("", name, x)
    print(name)


def compress(x, amount=3.0):
    """Loudness: squeeze the peaks so the body and the tail come up close to the first hit."""
    e = np.convolve(np.abs(x), np.ones(400) / 400, mode="same")
    gain = 1 / (1 + amount * e / (e.max() + 1e-9))
    y = x * gain
    return y / (np.max(np.abs(y)) + 1e-9)


def rumble(length, decay):
    """The thunder that rolls after a big blast outdoors: low noise that swells a little and fades long."""
    t = np.arange(int(length * sintesis.SR)) / sintesis.SR
    shape = np.clip(t / 0.08, 0, 1) * np.exp(-t / decay) * (1 + 0.3 * np.sin(2 * np.pi * 1.7 * t))
    return band(noise(length), 40, 600) * shape


# C+ · Pesada sostenida: the same heavy blast, a longer body and a rolling low tail, compressed.
c1_body = blast(1.6, low=0.75, crack_gain=1.4, body_decay=0.28, tail_decay=1.0)
c1 = mix(3.0, (c1_body, 0, 1.0), (rumble(2.6, 0.7), 0.03, 0.9), (pump(), 1.9, 0.8))
c1 = compress(drive(reverb(c1, 1.6, 0.25), 1.6), 3.0)
# C++ · Muy sostenida: even longer thunder and more push, like a cannon in a stone courtyard.
c2_body = blast(2.0, low=0.7, crack_gain=1.5, body_decay=0.4, tail_decay=1.4)
c2 = mix(4.0, (c2_body, 0, 1.0), (rumble(3.5, 1.1), 0.03, 1.1), (pump(), 2.6, 0.8))
c2 = compress(drive(reverb(c2, 2.4, 0.3), 1.8), 4.0)
for name, x in (("escopeta_C1_sostenida", c1), ("escopeta_C2_muy_sostenida", c2)):
    sintesis.save("", name, x, peak=0.95)
    print(name)

# The chosen one goes into the game.
sintesis.ROOT = GAME
sintesis.save("SFX/Armas", "escopeta_disparo", c1, peak=0.95)
print("escopeta_disparo = C1")
