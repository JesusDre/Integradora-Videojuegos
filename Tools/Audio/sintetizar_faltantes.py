"""The sounds that were missing after #115 (issue #123): voices and the effects of the hacienda, the
convent and the canals.

Run from the repo root:
    python3 Tools/Audio/sintetizar_faltantes.py
Needs espeak-ng (speech synthesizer, GPL; installed with the system) for the voices. The words come
out of espeak; everything that makes them sound like a ghost, a choir or a broken doll is processing
done here (layers, pitch, distortion, reverb). The effects use the same building blocks as #115.

The Padre can use a Mexican MBROLA voice (mx1 or mx2) instead of espeak's own sound:
    MBROLA_DIR=<folder> PADRE_VOZ=mx1 python3 Tools/Audio/sintetizar_faltantes.py
where <folder> holds the compiled program in Bin/mbrola and, in esdata/espeak-ng-data/, a copy of
espeak's data with the voices in mbrola/mx1 and mbrola/mx2 (github.com/numediart/MBROLA and
MBROLA-voices). Without MBROLA_DIR the Padre keeps the espeak voice.
"""

import os
import re
import subprocess
import sys
import tempfile
import wave

import numpy as np

sys.path.append(os.path.dirname(os.path.abspath(__file__)))
import sintesis  # noqa: E402
from sintesis import (SR, band, bell, drive, env, fade, make_loop, mix, noise, place, reverb, save,  # noqa: E402
                      sweep, t_axis, thump, tone, glide, lfo, written)

rng = sintesis.rng = np.random.default_rng(123)
REPO = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
# AUDIO_OUT writes everything to another folder, to compare versions without touching the project.
if os.environ.get("AUDIO_OUT"):
    sintesis.ROOT = os.environ["AUDIO_OUT"]


def speak(text, voice="es-419+m3", speed=130, pitch=30, amplitude=120, gap=0, ssml=False):
    """Speech from espeak-ng, as a float array at 44.1 kHz. ssml=True reads <prosody> and <break> tags."""
    with tempfile.NamedTemporaryFile(suffix=".wav", delete=False) as f:
        path = f.name
    subprocess.run(["espeak-ng"] + (["-m"] if ssml else []) + ["-v", voice, "-s", str(speed), "-p", str(pitch),
                    "-a", str(amplitude), "-g", str(gap), "-w", path, text], check=True)
    with wave.open(path) as w:
        rate = w.getframerate()
        x = np.frombuffer(w.readframes(w.getnframes()), "<i2") / 32768.0
    os.remove(path)
    # Trim espeak's silences, then resample to the project's rate.
    loud = np.nonzero(np.abs(x) > 0.01)[0]
    if len(loud):
        x = x[max(loud[0] - 200, 0):loud[-1] + 2000]
    n = int(len(x) * SR / rate)
    return np.interp(np.linspace(0, len(x) - 1, n), np.arange(len(x)), x)


MBROLA_DIR = os.environ.get("MBROLA_DIR")
# Jesús chose mx2 with the "live melody" (humanize below).
PADRE_VOICE = os.environ.get("PADRE_VOZ", "mx2" if MBROLA_DIR else "espeak")


def humanize(pho, rng, range_scale=1.5, jitter=0.02, dur_var=0.08):
    """What gives a synthetic voice away is that it is too exact. Each line of the .pho file is a phoneme,
    its length in ms and pitch points (position %, Hz). Here the melody is widened around its average,
    every pitch point gets a small irregular wobble plus a slow drift, and each sound lasts a bit more or
    less, so it no longer speaks like a metronome."""
    pitches = [float(v) for line in pho.splitlines() for v in line.split()[3::2]]
    mean = np.mean(pitches) if pitches else 100.0
    drift = 1.0
    out = []
    for line in pho.splitlines():
        parts = line.split()
        if len(parts) < 2 or line.startswith(";"):
            out.append(line)
            continue
        phoneme, duration = parts[0], float(parts[1])
        if phoneme != "_":
            duration *= 1 + rng.uniform(-dur_var, dur_var)
        drift = 0.9 * drift + 0.1 * (1 + rng.normal(0, 0.03))
        points = []
        for position, hz in zip(parts[2::2], parts[3::2]):
            hz = (mean + (float(hz) - mean) * range_scale) * drift * (1 + rng.normal(0, jitter))
            points += [position, f"{max(hz, 50):.0f}"]
        out.append(" ".join([phoneme, f"{duration:.0f}"] + points))
    return "\n".join(out) + "\n"


def speak_mbrola(ssml, voice, speed, pitch, rng):
    """espeak decides the phonemes, their length and the melody; MBROLA gives them a real recorded voice.
    The Mexican voices don't know espeak's stressed-vowel mark ("o/"), so it is removed first; otherwise
    MBROLA would replace those vowels with silence."""
    env_vars = dict(os.environ, PATH=os.path.join(MBROLA_DIR, "Bin") + os.pathsep + os.environ["PATH"],
                    ESPEAK_DATA_PATH=os.path.join(MBROLA_DIR, "esdata"))
    pho = subprocess.run(["espeak-ng", "-v", f"mb-{voice}", "-m", "-q", "--pho", "-s", str(speed), "-p", str(pitch), ssml],
                         capture_output=True, text=True, env=env_vars, check=True).stdout
    pho = re.sub(r"^(\S+?)/(?=\s)", r"\1", pho, flags=re.M)
    # espeak writes the "ll" of "muralla" as Spain's l^; in Mexico it sounds like "y" (jj in mx1, dZ in mx2).
    pho = re.sub(r"^l\^(?=\s)", "dZ" if voice == "mx2" else "jj", pho, flags=re.M)
    pho = humanize(pho, rng)
    with tempfile.NamedTemporaryFile(suffix=".wav", delete=False) as f:
        path = f.name
    voice_file = os.path.join(MBROLA_DIR, "esdata", "espeak-ng-data", "mbrola", voice)
    result = subprocess.run([os.path.join(MBROLA_DIR, "Bin", "mbrola"), voice_file, "-", path], input=pho,
                            capture_output=True, text=True)
    if result.returncode != 0:
        raise RuntimeError(f"mbrola failed ({result.returncode}) on: {ssml}\n{result.stderr[-500:]}")
    unknown = [l for l in result.stderr.splitlines() if "unknown" in l]
    if unknown:
        print("  mbrola:", "; ".join(unknown[:3]))
    with wave.open(path) as w:
        rate = w.getframerate()
        x = np.frombuffer(w.readframes(w.getnframes()), "<i2") / 32768.0
    os.remove(path)
    loud = np.nonzero(np.abs(x) > 0.01)[0]
    if len(loud):
        x = x[max(loud[0] - 100, 0):loud[-1] + 1000]
    n = int(len(x) * SR / rate)
    return np.interp(np.linspace(0, len(x) - 1, n), np.arange(len(x)), x)


def resample(x, ratio):
    """Plays x `ratio` times faster (higher and shorter): a cheap pitch shift for layering."""
    n = int(len(x) / ratio)
    return np.interp(np.linspace(0, len(x) - 1, n), np.arange(len(x)), x)


def fit(x, n):
    """Cut or pad x to n samples (seconds-based helpers can round one sample differently)."""
    return x[:n] if len(x) >= n else np.concatenate([x, np.zeros(n - len(x))])


def warble(x, depth, rate, phase=0.0):
    """Playback speed that wavers like a worn tape: the pitch slides up and down `depth` (0.03 = 3 %)."""
    t = np.arange(len(x)) / SR
    position = np.cumsum(1 + depth * np.sin(2 * np.pi * rate * t + phase))
    position = position[position < len(x) - 1]
    return np.interp(position, np.arange(len(x)), x)


def detuned(x, voices):
    """Several copies of x, each at its own pitch (ratio) and wavering out of step with the others:
    they never quite agree, which is what makes a voice sound wrong. voices: (ratio, depth, rate, phase)."""
    return layer(*(warble(resample(x, r), d, f, p) for r, d, f, p in voices)) / len(voices) ** 0.5


def layer(*parts):
    length = max(len(p) for p in parts)
    out = np.zeros(length)
    for p in parts:
        out[:len(p)] += p
    return out


# ---------------------------------------------------------------- voices
# The Padre: an old, tired priest who speaks low. A deep male voice, slow, slightly darkened and with the
# small room of a close microphone, so it reads as a voice in his head rather than a narrator.
def tired(text):
    """Jesús chose this delivery (version E): slow, holding the word before each comma and the last word of
    each sentence like a sigh. Sentence marks become a short pause, because espeak reads a "." that follows
    a tag out loud ("punto")."""
    text = text.replace("…", ".").replace("¿", "").replace("¡", "")
    out = []
    for sentence in re.split(r"[.?!]+", text):
        sentence = sentence.strip()
        if not sentence:
            continue
        clauses = [c.strip() for c in sentence.split(",") if c.strip()]
        held = []
        for k, clause in enumerate(clauses):
            words = clause.split(" ")
            rate = "35%" if k == len(clauses) - 1 else "45%"
            words[-1] = f'<prosody rate="{rate}">{words[-1]}</prosody>'
            held.append(" ".join(words))
        out.append(", ".join(held))
    return "<speak>" + '<break time="150ms"/> '.join(out) + "</speak>"


def deep_breath():
    """When the Padre tells himself "Respira", he does: a long shaky breath in and a slow sigh out."""
    r = np.random.default_rng(66)
    inhale_len, exhale_len = 1.1, 1.6
    inhale = band(r.uniform(-1, 1, int(inhale_len * SR)), 700, 2800) * np.linspace(0.2, 1, int(inhale_len * SR)) ** 1.5
    inhale *= np.hanning(int(inhale_len * SR)) ** 0.3
    shake = 1 + 0.25 * np.sin(2 * np.pi * 7 * np.arange(int(exhale_len * SR)) / SR)   # nerves in the sigh
    exhale = band(r.uniform(-1, 1, int(exhale_len * SR)), 250, 1600) * np.linspace(1, 0, int(exhale_len * SR)) ** 1.2 * shake
    return mix(inhale_len + exhale_len + 0.4, (inhale, 0, 0.5), (exhale, inhale_len + 0.25, 0.6))


lines = re.findall(r"«([^»]+)»", open(os.path.join(REPO, "docs", "TEXTOS.md"), encoding="utf-8").read())
for i, text in enumerate(lines):
    breath_rng = np.random.default_rng(81 + i)    # one seed per line: editing one never changes another
    # "Respira" is what he does, not what he says: the word is replaced by the breath itself.
    breathes = text.rstrip(".").endswith("Respira")
    spoken = text.rstrip(".")[:-len("Respira")].strip() if breathes else text
    if PADRE_VOICE == "espeak":
        v = speak(tired(spoken), "es-419+m3", speed=98, pitch=18, amplitude=110, gap=0, ssml=True)
    else:
        v = speak_mbrola(tired(spoken), PADRE_VOICE, speed=98, pitch=25, rng=np.random.default_rng(500 + i))
    breath = band(breath_rng.uniform(-1, 1, len(v)), 300, 3000) * 0.015
    v = band(v + breath, 90, 5500)
    if breathes:
        v = np.concatenate([v, np.zeros(int(0.3 * SR)), deep_breath() * np.max(np.abs(v)) * 0.8])
    save("Voices", f"padre_{i + 1:02d}", reverb(drive(v, 1.3), 0.5, 0.12), peak=0.8)

# From here on, a fixed seed: changing a line of the Padre must not change the sounds below.
rng = sintesis.rng = np.random.default_rng(1231)

# Prayer in Latin, murmured: the voiced words with a whispered copy under them, in a stone room.
for k, text in enumerate(("Pater noster, qui es in caelis", "Exorcizo te, immundissime spiritus")):
    voiced = speak(text, "la+m3", speed=115, pitch=15, amplitude=90)
    whisper = speak(text, "la+whisper", speed=115, pitch=15, amplitude=130)
    save("SFX/Objetos", f"rezo_latin_{k + 1}", reverb(band(layer(voiced * 0.7, whisper * 0.6), 100, 6000), 1.4, 0.3), peak=0.7)

# La Llorona: a woman's voice stretched and doubled (three slightly detuned copies make it unstable,
# not quite human), plus a whisper. Far: dark and drowned in echo. Near: whispered, almost dry. Scream:
# higher, faster and distorted.
def llorona(text, speed, pitch, ratios=(0.97, 1.0, 1.03)):
    base = speak(text, "es-419+f3", speed=speed, pitch=pitch, amplitude=140, gap=4)
    copies = [resample(base, r) for r in ratios]
    return layer(*copies) / len(copies)


def vowel(source, f1, f2, f3):
    return band(source, f1 * 0.8, f1 * 1.2) + 0.7 * band(source, f2 * 0.88, f2 * 1.12) + 0.3 * band(source, f3 * 0.92, f3 * 1.08)


def lament(seconds, start, peak, end, breath=0.2, sobs=2):
    """The long, sung "aaaay" of the Llorona (espeak can't hold a vowel): a woman's voice that rises a
    little, then falls slowly with a vibrato that grows, breaks into sobs at the end, and closes the
    vowel from "a" to "i" so it says "ay"."""
    n = int(seconds * SR)
    t = np.arange(n) / SR
    rise = min(0.5, seconds * 0.15)
    pitch = np.where(t < rise, start * (peak / start) ** (t / rise),
                     peak * (end / peak) ** (np.clip((t - rise) / (seconds - rise), 0, 1) ** 1.3))
    vibrato = 1 + (0.012 + 0.03 * t / seconds) * np.sin(2 * np.pi * 5.2 * t)
    source = tone(pitch * vibrato, seconds, "saw")
    # Sobs: short dips of the voice near the end, each with a catch of breath.
    loud = np.ones(n)
    for k in range(sobs):
        c = seconds * (0.62 + 0.14 * k)
        loud *= 1 - 0.85 * np.exp(-((t - c) / 0.07) ** 2)
    # Its own random generator, so adding this voice doesn't change the sounds generated after it.
    air = np.random.default_rng(int(start)).uniform(-1, 1, n)
    a = vowel(source, 820, 1250, 2700) + breath * vowel(air, 820, 1250, 2700)
    i = vowel(source, 330, 2300, 3000) + breath * vowel(air, 330, 2300, 3000)
    closing = np.clip((t / seconds - 0.78) / 0.15, 0, 1)
    swell = np.clip(t / 0.35, 0, 1) * np.clip((seconds - t) / 0.5, 0, 1)
    return (a * (1 - closing) + i * closing) * loud * swell


def ay_mis_hijos(seconds, start, peak, end, words_speed, words_pitch, breath, voices):
    """The sung "ay", detuned like the rest of her voice, flowing straight into "mis hijos" from espeak."""
    ay = detuned(lament(seconds, start, peak, end, breath), voices)
    words = detuned(llorona("mis hijooos", words_speed, words_pitch), voices)
    overlap = int(0.25 * SR)
    out = np.zeros(len(ay) + len(words) - overlap)
    out[:len(ay)] += ay / (np.max(np.abs(ay)) + 1e-9)
    out[len(ay) - overlap:] += words / (np.max(np.abs(words)) + 1e-9) * 0.9
    return out


DETUNE = ((0.95, 0.02, 0.6, 0.0), (1.0, 0.025, 0.45, 1.3), (1.05, 0.02, 0.75, 2.6))
far = band(ay_mis_hijos(3.4, 440, 560, 330, 70, 70, 0.25, DETUNE), 200, 2500)
save("SFX/Enemigos", "llorona_ayhijos_lejos", reverb(far, 4.0, 0.7), peak=0.6)
near_voice = ay_mis_hijos(2.8, 360, 430, 290, 80, 60, 0.6, ((0.93, 0.025, 0.7, 0.0), (1.0, 0.03, 0.5, 1.0), (1.07, 0.025, 0.9, 2.0)))
near_whisper = detuned(speak("aaaay, mis hijos", "es-419+whisper", 60, 60, 160, 4), ((0.95, 0.02, 0.6, 0.5), (1.05, 0.02, 0.8, 2.5)))
near = layer(near_voice * 0.6, near_whisper * 0.5)
save("SFX/Enemigos", "llorona_ayhijos_cerca", reverb(band(near, 150, 7000), 1.2, 0.25), peak=0.75)
scream = ay_mis_hijos(3.0, 620, 820, 470, 95, 95, 0.35, ((0.94, 0.03, 0.8, 0.0), (1.0, 0.035, 0.6, 1.2), (1.06, 0.03, 1.0, 2.4), (1.12, 0.025, 0.7, 3.6)))
save("SFX/Enemigos", "llorona_ayhijos_grito", reverb(drive(band(scream, 250, 6000), 3.0), 2.5, 0.45), peak=0.85)


# Cultist grunts and death: a rough glottal pulse (saw with jitter) through the formants of a vowel.
def grunt(seconds, f0, f1, vowel=(650, 1100, 2500), rough=0.4, strain=2.5):
    pitch = glide(f0, f1, seconds, 0.6) * (1 + rng.normal(0, 0.015, int(seconds * SR)))
    source = tone(pitch, seconds, "saw") * (1 + rough * noise(seconds))
    voice = band(source, vowel[0] * 0.8, vowel[0] * 1.2) + 0.6 * band(source, vowel[1] * 0.85, vowel[1] * 1.15) \
        + 0.25 * band(source, vowel[2] * 0.9, vowel[2] * 1.1) + band(noise(seconds), 400, 3000) * 0.25
    return drive(voice, strain)


save("SFX/Enemigos", "cultista_ataque_1", reverb(grunt(0.45, 150, 110) * env(0.45, 0.02, 0.25, 0.1), 0.6, 0.15), peak=0.8)
save("SFX/Enemigos", "cultista_ataque_2", reverb(grunt(0.6, 130, 170, (550, 900, 2400)) * env(0.6, 0.03, 0.3, 0.2), 0.6, 0.15), peak=0.8)
death = grunt(1.6, 190, 70, (700, 1150, 2500), 0.6, 3.5) * env(1.6, 0.03, 1.0, 0.3)
save("SFX/Enemigos", "cultista_muerte", reverb(death, 1.2, 0.25), peak=0.8)

# The cult's chant (loop): eight men singing the same invented Nahuatl verse, each with his own voice,
# pitch and speed, so it sounds like a crowd and not one voice copied; a low drone holds it together.
verse = "Tzitzimimeh, xihualhuian. Ilhuicac tlayohua. Tonatiuh miqui."
choir = []
for k, (variant, pitch, speed) in enumerate((("m1", 10, 92), ("m2", 14, 95), ("m3", 8, 90), ("m4", 18, 97),
                                              ("m5", 12, 93), ("m6", 6, 91), ("m7", 16, 96), ("m3", 22, 94))):
    v = speak(verse, f"es-419+{variant}", speed=speed, pitch=pitch, amplitude=100, gap=6)
    choir.append(np.concatenate([np.zeros(int(rng.uniform(0, 0.12) * SR)), v]))
sung = layer(*choir) / 4
C = 26.0
chant = place(C, sung, 1.0) + place(C, sung, 13.5)
chant = band(chant, 80, 3500)
chant += band(tone(55, C, "saw") + tone(82.4, C, "saw") * 0.6, 40, 400) * 0.35
save("Ambiente", "cantos_secta", make_loop(reverb(chant, 3.0, 0.5)[:int(C * SR)], 1.0), peak=0.6, loop=True)

def sing_mbrola(notes, voice, voice_rate):
    """Writes the .pho by hand instead of asking espeak: notes are (phoneme, ms, pitch curve or None), the
    curve a function of 0..1 (position inside the sound) giving Hz. voice_rate above the voice's 16000 makes
    the throat smaller (formants up), which turns MBROLA's men into a woman or a child."""
    lines = []
    for phoneme, ms, curve in notes:
        if curve is None:
            lines.append(f"{phoneme} {ms}")
            continue
        steps = max(2, int(ms / 40))
        points = " ".join(f"{100 * k / steps:.0f} {curve(k / steps):.0f}" for k in range(steps + 1))
        lines.append(f"{phoneme} {ms} {points}")
    with tempfile.NamedTemporaryFile(suffix=".wav", delete=False) as f:
        path = f.name
    voice_file = os.path.join(MBROLA_DIR, "esdata", "espeak-ng-data", "mbrola", voice)
    subprocess.run([os.path.join(MBROLA_DIR, "Bin", "mbrola"), "-l", str(voice_rate), voice_file, "-", path],
                   input="\n".join(lines) + "\n", text=True, capture_output=True, check=True)
    with wave.open(path) as w:
        rate = w.getframerate()
        x = np.frombuffer(w.readframes(w.getnframes()), "<i2") / 32768.0
    os.remove(path)
    n = int(len(x) * SR / rate)
    return np.interp(np.linspace(0, len(x) - 1, n), np.arange(len(x)), x)


def girl_laugh(vowel, pitches):
    """A little girl's "ji ji ji ji" (mx2's "j" is the Spanish jota): too slow, each one a bit lower, the
    last one drawn out and falling, all with a fast vibrato. Chosen by Jesús over the espeak version."""
    notes = [("_", 60, None)]
    for idx, hz in enumerate(pitches):
        last = idx == len(pitches) - 1
        curve = (lambda u, hz=hz, last=last: hz * (0.85 ** u if last else 1.0) * (1 + 0.05 * np.sin(2 * np.pi * 7 * u * 2.6)))
        notes += [("j", 70, None), (vowel, 520 if last else 190, curve), ("_", 140, None)]
    return sing_mbrola(notes, "mx2", 24000)


# The dolls' laughter: a child's laugh too slow, with a warble like a worn-out toy mechanism and a
# ring-modulated shadow under it. With MBROLA it is sung by its recorded voice; without it, by espeak.
for k, (text, pitch, vowel, notes_hz) in enumerate((("ji ji ji ji ji", 99, "i", (720, 690, 660, 610, 540)),
                                                     ("ja, ja, ja, jaaa", 90, "a", (650, 620, 600, 560, 480)))):
    if MBROLA_DIR:
        laugh = girl_laugh(vowel, notes_hz)
    else:
        laugh = resample(speak(text, "es-419+f5", speed=110, pitch=pitch, amplitude=150, gap=3), 0.85)
    # Four out-of-tune copies of the same child, wavering apart: a toy box that is breaking.
    laugh = detuned(laugh, ((0.9, 0.03, 1.3, 0.0), (1.0, 0.04, 0.8, 1.5), (1.08, 0.03, 1.1, 3.0), (1.17, 0.02, 0.6, 4.5)))
    wobble = fit(lfo(len(laugh) / SR, 6, 0.4, 0.6), len(laugh))
    ring = laugh * fit(tone(170, len(laugh) / SR), len(laugh))
    toy = band(laugh * wobble + ring * 0.35, 300, 7000)
    save("SFX/Enemigos", f"muneca_risa_{k + 1}", reverb(drive(toy, 2.0), 2.0, 0.45), peak=0.7)


# ---------------------------------------------------------------- the Charro and the hacienda
def hoof(hard=False):
    """One hoof: a heavy thump on dirt, or a sharp iron click on stone."""
    if hard:
        return mix(0.25, (band(noise(0.25), 1500, 7000) * env(0.25, 0.0005, 0.02), 0, 1.0),
                   (bell(2600, 0.25, partials=(1, 1.6, 2.3), decays=(0.2, 0.1, 0.1)), 0, 0.25),
                   (thump(160, 90, 0.25, 0.05), 0, 0.6))
    return mix(0.3, (thump(110, 50, 0.3, 0.08), 0, 1.2), (band(noise(0.3), 150, 1500) * env(0.3, 0.002, 0.07), 0, 0.8),
               (band(noise(0.3), 2000, 6000) * env(0.3, 0.01, 0.15), 0.01, 0.15))


# Gallop (loop): the three-beat rhythm of a heavy horse, 1.6 strides per second, with its breath.
G = 5.0
gallop = np.zeros(int(G * SR))
stride = 1 / 1.6
for s in range(int(G / stride)):
    t0 = s * stride
    for dt, g in ((0.0, 0.8), (0.09, 0.9), (0.2, 1.2)):
        gallop += place(G, hoof(), t0 + dt + rng.normal(0, 0.006), g * rng.uniform(0.85, 1.1))
    gallop += place(G, band(noise(0.3), 300, 1500) * np.hanning(int(0.3 * SR)), t0 + 0.35, 0.2)
save("SFX/Enemigos", "charro_galope", make_loop(np.concatenate([gallop, gallop[:int(SR)]]), 1.0), peak=0.8, loop=True)

# Horseshoes on stone (loop): a slow four-beat walk.
H = 4.0
walk = np.zeros(int(H * SR))
for k in range(8):
    walk += place(H, hoof(True), k * 0.5 + rng.normal(0, 0.01), rng.uniform(0.7, 1.0))
save("SFX/Enemigos", "charro_herraduras", make_loop(np.concatenate([reverb(walk, 0.8, 0.2)[:int(H * SR)], walk[:int(SR)]]), 1.0), peak=0.7, loop=True)

# Spurs: small rowels that rattle twice with the step.
spurs = np.zeros(int(1.4 * SR))
for t0 in (0.05, 0.7):
    for _ in range(7):
        f = rng.uniform(3200, 5600)
        spurs += place(1.4, bell(f, 0.35, partials=(1, 1.41, 2.2), decays=(0.4, 0.25, 0.15)), t0 + rng.uniform(0, 0.12), rng.uniform(0.3, 0.8))
save("SFX/Enemigos", "charro_espuelas", reverb(spurs, 0.6, 0.2), peak=0.6)

# Silver coins clinking in the hand.
coins = np.zeros(int(1.2 * SR))
for _ in range(9):
    coins += place(1.2, bell(rng.uniform(3000, 5200), 0.5, partials=(1, 2.2, 3.9), decays=(0.6, 0.3, 0.2)), rng.uniform(0, 0.35) ** 1.5, rng.uniform(0.3, 1.0))
save("SFX/Jugador", "monedas", coins, peak=0.6)


# Church bells: the real partials of a cast bell (hum, prime, tierce, quint, nominal...).
def church_bell(f0, seconds):
    partials = (0.5, 1.0, 1.183, 1.506, 2.0, 2.514, 2.662, 3.011)
    decays = (1.0, 0.7, 0.55, 0.4, 0.35, 0.25, 0.2, 0.15)
    amps = (0.8, 1.0, 0.6, 0.35, 0.5, 0.25, 0.2, 0.15)
    strike = band(noise(0.05), 800, 6000) * env(0.05, 0.0005, 0.01) * 0.6
    return place(seconds, strike, 0) + bell(f0, seconds, partials, decays, amps)


save("SFX/Objetos", "campana_capilla", reverb(church_bell(311, 6.0), 4.0, 0.5), peak=0.7)
far_bells = np.zeros(int(10 * SR))
for k, f in enumerate((392, 330, 392, 294)):
    far_bells += place(10, church_bell(f, 5.0), k * 1.6, 1.0)
save("Ambiente", "campanas_lejanas", reverb(band(far_bells, 150, 2500), 4.0, 0.7), peak=0.5)
save("SFX/Objetos", "campana_convento", reverb(church_bell(98, 9.0), 5.0, 0.55), peak=0.8)

# ---------------------------------------------------------------- water
# Trajinera pole (loop): the pole enters the water, pushes (a swirl and the wood creaking), comes out
# dripping; one stroke every 4 s.
R = 4.0
splash = band(noise(0.4), 400, 4000) * env(0.4, 0.005, 0.15)
swirl = sweep(noise(1.4), 300, 900, 0.7) * np.hanning(int(1.4 * SR)) * 0.5
creak = band(tone(glide(220, 160, 0.8) * lfo(0.8, 30, 0.05, 1.0), 0.8, "saw"), 300, 2000) * env(0.8, 0.1, 0.4) * 0.15
drips = np.zeros(int(R * SR))
for _ in range(10):
    d = 0.05
    f = rng.uniform(900, 2000)
    drips += place(R, tone(glide(f, f * 1.7, d), d) * env(d, 0.001, 0.03), 1.8 + rng.uniform(0, 1.2), rng.uniform(0.2, 0.5))
oar = mix(R + 1.0, (splash, 0.1, 1.0), (swirl, 0.3, 1.0), (creak, 0.6, 1.0), (drips, 0, 1.0))
save("SFX/Objetos", "trajinera_remo", make_loop(oar, 1.0), peak=0.6, loop=True)

for i in range(4):
    plop = band(noise(0.45), 250, 3500) * env(0.45, 0.004, 0.12)
    sub_drops = sum(place(0.45, tone(glide(f, f * 1.6, 0.05), 0.05) * env(0.05, 0.001, 0.03), rng.uniform(0.05, 0.3), 0.3)
                    for f in rng.uniform(800, 1800, 4))
    save("SFX/Jugador", f"pasos_agua_{i + 1}", plop + sub_drops + thump(90, 60, 0.45, 0.05) * 0.5, peak=0.6)

# ---------------------------------------------------------------- the Tzitzimime
# Her presence (loop): two sub tones that beat against each other, a rumble and a slow pulse; on
# small speakers the beating and the rumble still carry it.
Z = 11.0
hum = tone(41, Z) + tone(43.5, Z) + 0.5 * tone(82, Z) * lfo(Z, 0.18, 0.5, 0.5)
rumble = band(noise(Z), 30, 180) * lfo(Z, 0.09, 0.4, 0.6)
whine = band(tone(glide(1800, 1700, Z), Z, "saw"), 1500, 2200) * lfo(Z, 0.27, 0.5, 0.5) ** 4 * 0.05
save("SFX/Enemigos", "tzitzimime_vibracion", make_loop((hum * 0.6 + rumble + whine) * lfo(Z, 0.5, 0.25, 0.75), 1.0), peak=0.8, loop=True)

for name, seconds in written:
    print(f"{name:34s} {seconds:5.1f} s")
print(f"{len(written)} sounds")
