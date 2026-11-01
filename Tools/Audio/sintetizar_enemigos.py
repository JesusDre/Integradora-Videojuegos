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


# ---- Chaneques (#173): a raspy little cackle, "je je je", too fast and a bit too high, in three voices
# that never quite agree, with grit. espeak-ng says the syllables; the rest is processing.
def speak(text, voice, speed, pitch):
    """Speech from espeak-ng as a float array at the project's rate (the same idea as sintetizar_faltantes)."""
    import subprocess
    import tempfile
    import wave
    with tempfile.NamedTemporaryFile(suffix=".wav", delete=False) as f:
        path = f.name
    subprocess.run(["espeak-ng", "-v", voice, "-s", str(speed), "-p", str(pitch), "-a", "150", "-w", path, text], check=True)
    with wave.open(path) as w:
        rate = w.getframerate()
        x = np.frombuffer(w.readframes(w.getnframes()), "<i2") / 32768.0
    os.remove(path)
    loud = np.nonzero(np.abs(x) > 0.01)[0]
    x = x[max(loud[0] - 200, 0):loud[-1] + 2000]
    n = int(len(x) * sintesis.SR / rate)
    return np.interp(np.linspace(0, len(x) - 1, n), np.arange(len(x)), x)


def resample(x, ratio):
    n = int(len(x) / ratio)
    return np.interp(np.linspace(0, len(x) - 1, n), np.arange(len(x)), x)


def cackle(text, speed, pitch, ratios):
    base = speak(text, "es-419+m4", speed, pitch)
    voices = [resample(base, r) for r in ratios]
    n = max(len(v) for v in voices)
    voices = [np.concatenate([v, np.zeros(n - len(v))]) for v in voices]
    x = sum(voices) / len(voices) ** 0.5
    return reverb(drive(band(x, 250, 6500), 3.0), 0.9, 0.25)


from sintesis import drive  # noqa: E402

save("SFX/Enemigos", "chaneque_risa_1", cackle("je je je je je", 240, 85, (1.12, 1.18, 1.27)), peak=0.75)
save("SFX/Enemigos", "chaneque_risa_2", cackle("ji ji ji, jijiji", 280, 95, (1.2, 1.3, 1.38)), peak=0.75)
save("SFX/Enemigos", "chaneque_risa_3", cackle("ja ja, je je je", 220, 70, (1.05, 1.1, 1.2)), peak=0.75)
# The theft: a quick swipe, a jingle of what it took, and a short snicker.
swipe = band(noise(0.25), 800, 5000) * np.hanning(int(0.25 * sintesis.SR)) * 0.6
jingle = mix(0.6, *((bell(f, 0.4, decays=(0.15, 0.1, 0.08, 0.05)) * 0.3, s, 1.0) for f, s in ((2600, 0.08), (3100, 0.14), (2850, 0.21))))
snicker = cackle("je je", 300, 90, (1.25, 1.33))
save("SFX/Enemigos", "chaneque_robo", mix(1.6, (swipe, 0.0, 1.0), (jingle, 0.05, 1.0), (snicker, 0.45, 0.9)))
# Its death: a shrill squeal that falls apart.
save("SFX/Enemigos", "chaneque_muere", cackle("iiiiaa", 160, 99, (1.3, 1.45)), peak=0.75)
