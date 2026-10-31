"""Levels every game sound to a loudness that depends on what it is (issue #50).

Run from the repo root, AFTER the synthesis scripts (they normalize by peak, which doesn't match how
loud a sound feels):
    python3 Tools/Audio/nivelar.py
It only changes the gain of each WAV: the sound itself stays the same. Loudness is measured as the
loudest 400 ms of the sound (RMS in dBFS), a simple stand-in for what the ear perceives. The gain is
capped so no sample goes above 0.97 (no clipping).

The AudioMixer (AM_Principal) then balances the categories against each other; this script makes the
sounds of the SAME kind sit at the same level (all the footsteps alike, all the enemy cries alike...).
"""

import fnmatch
import glob
import os
import wave

import numpy as np

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "Assets", "_Project", "Audio")

# First match wins: (pattern relative to Audio/, target dBFS). Louder = closer to 0.
TARGETS = [
    ("SFX/Armas/escopeta_disparo.wav", -10),     # the loudest thing in the game, on purpose
    ("SFX/Armas/pistola_disparo.wav", -12),
    ("SFX/Armas/metralleta_disparo.wav", -14),   # many per second: quieter each
    ("SFX/Armas/ballesta_disparo.wav", -16),     # silent weapon
    ("SFX/Armas/martillo_chillido.wav", -14),
    ("SFX/Armas/*golpe*.wav", -15),
    ("SFX/Armas/*.wav", -19),                    # swing, reload, empty click
    ("SFX/Jugador/pasos_*.wav", -24),            # every surface at the same level
    ("SFX/Jugador/linterna_click.wav", -22),
    ("SFX/Jugador/recoger.wav", -20),
    ("SFX/Jugador/respiracion.wav", -21),
    ("SFX/Jugador/*.wav", -16),                  # hurt, heartbeat, key item, coins
    ("SFX/Objetos/sal.wav", -20),
    ("SFX/Objetos/venda.wav", -20),
    ("SFX/Objetos/campana_*.wav", -14),
    ("SFX/Objetos/*.wav", -17),
    ("SFX/Enemigos/tzitzimime_vibracion.wav", -17),
    ("SFX/Enemigos/muneca_risa_*.wav", -16),
    ("SFX/Enemigos/*.wav", -13),                 # enemies must cut through the music
    ("Ambiente/amb_*.wav", -22),                 # the bed of each level: always there, never in front
    ("Ambiente/antorcha.wav", -24),
    ("Ambiente/*.wav", -19),                     # crying, chant, far bells
    ("Music/stg_horda.wav", -14),
    ("Music/*.wav", -17),
    ("Voices/*.wav", -16),
]


def loudness(x, rate):
    n = min(len(x), int(0.4 * rate))
    # Moving average of the power with a running sum: the same as a 400 ms window, but fast.
    s = np.concatenate([[0.0], np.cumsum(x.astype(float) ** 2)])
    power = (s[n:] - s[:-n]) / n
    return 10 * np.log10(power.max() + 1e-12)


def target_for(rel):
    for pattern, db in TARGETS:
        if fnmatch.fnmatch(rel, pattern):
            return db
    return None


for path in sorted(glob.glob(os.path.join(ROOT, "**", "*.wav"), recursive=True)):
    rel = os.path.relpath(path, ROOT).replace(os.sep, "/")
    target = target_for(rel)
    if target is None:
        print(f"  (sin meta) {rel}")
        continue
    with wave.open(path) as w:
        params = w.getparams()
        x = np.frombuffer(w.readframes(w.getnframes()), "<i2") / 32768.0
    before = loudness(x, params.framerate)
    # Already at its level (or as close as its peak allows): leave the file untouched, so running this
    # again doesn't rewrite every sound with tiny rounding differences.
    if abs(target - before) < 0.3 or (before < target and np.max(np.abs(x)) > 0.95):
        continue
    gain = 10 ** ((target - before) / 20)
    peak = np.max(np.abs(x)) * gain
    limited = peak > 0.97
    if limited:
        gain *= 0.97 / peak
    y = np.clip(x * gain, -1, 1)
    with wave.open(path, "wb") as w:
        w.setparams(params)
        w.writeframes((y * 32767).astype("<i2").tobytes())
    after = loudness(y, params.framerate)
    print(f"{rel:42s} {before:6.1f} -> {after:6.1f} dB{'  (limitado por el pico)' if limited else ''}")
