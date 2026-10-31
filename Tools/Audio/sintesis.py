"""Shared synthesis tools for the sound scripts (issues #115 and #123): filters, envelopes, tones, bells,
reverb, loops and WAV writing. Imported by sintetizar_sonidos.py and sintetizar_faltantes.py.
"""

import os
import wave

import numpy as np

SR = 44100
ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "Assets", "_Project", "Audio")
rng = np.random.default_rng(115)
written = []


# ---------------------------------------------------------------- building blocks
def t_axis(seconds):
    return np.arange(int(seconds * SR)) / SR


def noise(seconds):
    return rng.uniform(-1, 1, int(seconds * SR))


def env(seconds, attack=0.005, decay=0.2, hold=0.0):
    """Fast attack, optional hold, then exponential decay (decay = time to fall to ~5 %)."""
    t = t_axis(seconds)
    e = np.minimum(t / max(attack, 1e-4), 1.0)
    tail = np.clip(t - attack - hold, 0, None)
    return e * np.exp(-3.0 * tail / max(decay, 1e-4))


def band(x, lo=None, hi=None):
    """Brick-wall-ish band filter in the frequency domain (soft 10 % edges)."""
    spec = np.fft.rfft(x)
    f = np.fft.rfftfreq(len(x), 1 / SR)
    gain = np.ones_like(f)
    if lo:
        gain *= np.clip((f - lo * 0.9) / (lo * 0.2), 0, 1)
    if hi:
        gain *= np.clip((hi * 1.1 - f) / (hi * 0.2), 0, 1)
    return np.fft.irfft(spec * gain, len(x))


def sweep(x, f_start, f_end, width=0.5):
    """A band that moves from f_start to f_end over the sound (the whoosh of a swing), via STFT."""
    n, hop = 1024, 256
    win = np.hanning(n)
    out = np.zeros(len(x) + n)
    norm = np.zeros(len(x) + n)
    f = np.fft.rfftfreq(n, 1 / SR)
    frames = max(1, (len(x) - n) // hop + 1)
    for i in range(frames):
        s = i * hop
        center = f_start * (f_end / f_start) ** (i / max(frames - 1, 1))
        gain = np.exp(-0.5 * (np.log(np.maximum(f, 1) / center) / width) ** 2)
        seg = np.fft.irfft(np.fft.rfft(x[s:s + n] * win) * gain, n)
        out[s:s + n] += seg * win
        norm[s:s + n] += win ** 2
    return (out / np.maximum(norm, 1e-3))[:len(x)]


def tone(freq, seconds, kind="sine", phase_mod=None):
    """freq may be a number or an array (a glide). kind: sine, saw, square, tri."""
    t = t_axis(seconds)
    freq = np.broadcast_to(np.asarray(freq, dtype=float), t.shape)
    phase = 2 * np.pi * np.cumsum(freq) / SR
    if phase_mod is not None:
        phase = phase + phase_mod
    if kind == "sine":
        return np.sin(phase)
    p = (phase / (2 * np.pi)) % 1.0
    if kind == "saw":
        return 2 * p - 1
    if kind == "square":
        return np.where(p < 0.5, 1.0, -1.0)
    return 4 * np.abs(p - 0.5) - 1          # triangle


def glide(f0, f1, seconds, curve=1.0):
    t = np.linspace(0, 1, int(seconds * SR)) ** curve
    return f0 * (f1 / f0) ** t


def bell(f0, seconds, partials=(1, 2.76, 5.40, 8.93), decays=(1.0, 0.6, 0.35, 0.2), amps=(1, 0.6, 0.4, 0.25)):
    out = np.zeros(int(seconds * SR))
    for p, d, a in zip(partials, decays, amps):
        out += a * tone(f0 * p, seconds) * env(seconds, 0.002, seconds * d)
    return out


def thump(f0, f1, seconds, decay=0.15):
    return tone(glide(f0, f1, seconds, 0.4), seconds) * env(seconds, 0.002, decay)


def reverb(x, seconds=1.2, wet=0.3, bright=6000):
    """Convolution with exponentially decaying filtered noise: a cheap, convincing room."""
    ir = band(noise(seconds), 80, bright) * np.exp(-6.0 * t_axis(seconds) / seconds)
    ir /= np.sqrt(np.sum(ir ** 2))
    n = len(x) + len(ir)
    y = np.fft.irfft(np.fft.rfft(x, n) * np.fft.rfft(ir, n), n)
    return np.concatenate([x, np.zeros(len(ir))]) * (1 - wet) + y * wet


def mix(length_seconds, *parts):
    """parts: (signal, start_seconds, gain)."""
    out = np.zeros(int(length_seconds * SR))
    for sig, start, gain in parts:
        s = int(start * SR)
        if s < 0:                     # a start slightly before 0 (random jitter) loses its first samples
            sig, s = sig[-s:], 0
        e = min(len(out), s + len(sig))
        if e > s:
            out[s:e] += sig[:e - s] * gain
    return out


def place(seconds, sig, start, gain=1.0):
    return mix(seconds, (sig, start, gain))


def drive(x, amount=3.0):
    return np.tanh(x * amount) / np.tanh(amount)


def fade(x, fin=0.005, fout=0.02):
    x = x.copy()
    a, b = int(fin * SR), int(fout * SR)
    if a:
        x[:a] *= np.linspace(0, 1, a)
    if b:
        x[-b:] *= np.linspace(1, 0, b)
    return x


def make_loop(x, cross=1.0):
    """Seamless loop: the last `cross` seconds fade into the first ones."""
    c = int(cross * SR)
    body = x[:-c].copy()
    ramp = np.linspace(0, 1, c)
    body[:c] = body[:c] * ramp + x[-c:] * (1 - ramp)
    return body


def lfo(seconds, rate, depth=1.0, offset=0.0, phase=0.0):
    return offset + depth * np.sin(2 * np.pi * rate * t_axis(seconds) + phase)


def save(folder, name, x, peak=0.89, loop=False):
    x = np.asarray(x, dtype=float)
    if not loop:
        x = fade(x, 0.0005, 0.03)
    x = x / (np.max(np.abs(x)) + 1e-9) * peak
    path = os.path.join(ROOT, folder, name + ".wav")
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes((x * 32767).astype("<i2").tobytes())
    written.append((folder + "/" + name, len(x) / SR))
