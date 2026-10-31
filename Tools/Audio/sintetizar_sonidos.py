"""Every sound the game can make without a recording (issue #115), synthesized with numpy.

Run from the repo root:
    python3 Tools/Audio/sintetizar_sonidos.py
Writes 16-bit mono WAVs (44.1 kHz) to Assets/_Project/Audio/{SFX,Ambiente,Music}/. The same seed gives the
same files, so a sound can be tweaked here and regenerated instead of edited by hand. Being made from
code, every file is our own work (no third-party license).

The tools are the classic ones of sound design: noise shaped by filters (wind, steps, the shot),
sines with envelopes (thumps, heartbeats), inharmonic partials (bells, metal) and a reverb made by
convolving with decaying noise (rooms, the convent's echo). Loops crossfade their end into their start.
"""

import os
import sys

import numpy as np

sys.path.append(os.path.dirname(os.path.abspath(__file__)))
from sintesis import *  # noqa: E402,F401,F403  (the shared building blocks)

# ---------------------------------------------------------------- player
for i in range(4):
    # Dirt: a soft heel and toe of low noise, with some grit.
    heel = band(noise(0.25), 80, 900) * env(0.25, 0.004, 0.06)
    toe = band(noise(0.25), 150, 2500) * env(0.25, 0.003, 0.05) * 0.6
    grit = band(noise(0.25), 2000, 7000) * env(0.25, 0.01, 0.1) * 0.15 * rng.uniform(0.5, 1.2)
    save("SFX/Jugador", f"pasos_tierra_{i + 1}", mix(0.3, (heel, 0, 1), (toe, 0.05 + rng.uniform(0, 0.02), 1), (grit, 0.02, 1)), peak=0.6)

    # Wood: a hollow thump that rings a little, plus a knock.
    f0 = rng.uniform(110, 160)
    hollow = thump(f0 * 1.4, f0, 0.35, 0.18) + 0.4 * tone(f0 * 2.7, 0.35) * env(0.35, 0.002, 0.08)
    knock = band(noise(0.35), 500, 2500) * env(0.35, 0.001, 0.03)
    creak = tone(glide(rng.uniform(300, 400), rng.uniform(250, 350), 0.35), 0.35, "saw") * env(0.35, 0.05, 0.15) * 0.04
    save("SFX/Jugador", f"pasos_madera_{i + 1}", mix(0.4, (hollow, 0, 1), (knock, 0, 0.8), (creak, 0.05, 1)), peak=0.6)

    # Stone: a short hard click with a bit of room.
    click = band(noise(0.2), 1200, 6000) * env(0.2, 0.0005, 0.025)
    low = band(noise(0.2), 100, 600) * env(0.2, 0.002, 0.05) * 0.7
    save("SFX/Jugador", f"pasos_piedra_{i + 1}", reverb(mix(0.25, (click, 0, 1), (low, 0, 1), (click * 0.5, 0.045, 1)), 0.5, 0.25), peak=0.6)

# Getting hit: a body blow and a short pushed-out breath.
blow = thump(120, 50, 0.5, 0.15) + band(noise(0.5), 200, 2000) * env(0.5, 0.001, 0.06)
breath = band(noise(0.5), 400, 1800) * env(0.5, 0.02, 0.18) * 0.5
save("SFX/Jugador", "jugador_dano", mix(0.5, (blow, 0, 1), (breath, 0.03, 1)))

# Heartbeat (one lub-dub, played in a loop by the low-health warning).
beat = thump(70, 40, 0.3, 0.09)
save("SFX/Jugador", "latido", mix(0.75, (beat, 0, 1), (beat, 0.17, 0.7)), peak=0.8)

# Out-of-breath panting (loop).
pant = np.zeros(int(2.0 * SR))
for k in range(4):
    inhale = band(noise(0.25), 700, 3000) * np.hanning(int(0.25 * SR)) * 0.45
    exhale = band(noise(0.3), 400, 2000) * np.hanning(int(0.3 * SR))
    pant += mix(2.0, (inhale, k * 0.5, 1), (exhale, k * 0.5 + 0.22, 1))
save("SFX/Jugador", "respiracion", pant, peak=0.5, loop=True)

clk = band(noise(0.08), 2000, 9000) * env(0.08, 0.0003, 0.006)
save("SFX/Jugador", "linterna_click", mix(0.12, (clk, 0, 1), (clk, 0.035, 0.6)), peak=0.6)

rustle = band(noise(0.35), 1500, 7000) * env(0.35, 0.03, 0.15) * lfo(0.35, 18, 0.4, 0.6)
save("SFX/Jugador", "recoger", mix(0.4, (rustle, 0, 0.6), (bell(1800, 0.4) * 0.15, 0.08, 1)), peak=0.6)

# Key item: an eerie low chime that hangs in the air.
chime = bell(392, 3.0) + 0.7 * bell(392 * 1.414, 3.0)     # a tritone: unsettling on purpose
save("SFX/Jugador", "objeto_clave", reverb(chime, 2.5, 0.45), peak=0.7)

# ---------------------------------------------------------------- weapons
crack = noise(0.02) * env(0.02, 0.0002, 0.006)
boom = band(noise(1.2), 40, 3000) * env(1.2, 0.001, 0.35)
sub = thump(90, 35, 1.2, 0.4)
shot = drive(mix(1.2, (crack, 0, 1.5), (boom, 0, 1.0), (sub, 0, 1.2)), 2.5)
# The first shotgun blast (#115). It is still computed so the random sounds below stay exactly the same,
# but no longer saved: the shotgun the game uses now comes from sintetizar_escopeta.py.
shot = reverb(shot, 1.6, 0.3)

metal = lambda f: band(noise(0.15), f, f * 3) * env(0.15, 0.0005, 0.02) + 0.3 * bell(f, 0.15, decays=(0.2, 0.1, 0.1, 0.1))
slide = band(noise(0.18), 1500, 5000) * env(0.18, 0.04, 0.08) * 0.5
save("SFX/Armas", "escopeta_recarga", mix(1.5, (metal(1200), 0, 1), (slide, 0.35, 1), (metal(2200), 0.5, 0.8),
                                          (slide, 0.75, 1), (metal(2400), 0.9, 0.8), (metal(900), 1.25, 1.2)), peak=0.7)
save("SFX/Armas", "escopeta_vacia", mix(0.2, (metal(1800), 0, 1)), peak=0.6)

swing = sweep(noise(0.35), 300, 2200, 0.6) * np.hanning(int(0.35 * SR)) ** 1.5
save("SFX/Armas", "machete_swing", swing, peak=0.7)

chop = mix(0.4, (thump(140, 60, 0.4, 0.1), 0, 1), (band(noise(0.4), 300, 2500) * env(0.4, 0.0005, 0.05), 0, 0.9),
           (band(noise(0.4), 3000, 8000) * env(0.4, 0.0005, 0.015), 0, 0.4))
save("SFX/Armas", "machete_golpe", chop)

slap = band(noise(0.25), 600, 8000) * env(0.25, 0.0003, 0.03) + thump(200, 120, 0.25, 0.04) * 0.6
save("SFX/Armas", "chancla_golpe", reverb(slap, 0.4, 0.15))

cross = mix(1.5, (thump(120, 60, 1.5, 0.1), 0, 1), (bell(880, 1.5, partials=(1, 1.63, 2.51, 3.7)), 0, 0.6))
save("SFX/Armas", "cruz_golpe", reverb(cross, 1.0, 0.3))

# ---------------------------------------------------------------- objects and the altar
save("SFX/Objetos", "altar_campanita", reverb(bell(2093, 2.5) + 0.5 * bell(2637, 2.5), 1.5, 0.35), peak=0.6)

bubbles = np.zeros(int(1.6 * SR))
for _ in range(45):
    d = rng.uniform(0.03, 0.08)
    f = rng.uniform(300, 1200)
    b = tone(glide(f, f * 1.8, d), d) * env(d, 0.002, d * 0.7)
    s = int(rng.uniform(0, 1.5) * SR)
    bubbles[s:s + len(b)] += b * rng.uniform(0.2, 0.6)
water = band(noise(1.6), 200, 1500) * np.hanning(int(1.6 * SR)) * 0.4
save("SFX/Objetos", "agua_llenar", bubbles + water, peak=0.6)

strike = band(noise(0.15), 2000, 9000) * env(0.15, 0.002, 0.08) * lfo(0.15, 60, 0.5, 0.5)
flame = band(noise(0.8), 100, 1200) * env(0.8, 0.05, 0.4)
save("SFX/Objetos", "veladora_encender", mix(0.9, (strike, 0, 1), (flame, 0.08, 0.7)), peak=0.6)

glass = np.zeros(int(1.0 * SR))
for _ in range(25):
    f = rng.uniform(2500, 7000)
    d = rng.uniform(0.05, 0.3)
    p = bell(f, d, partials=(1, 1.47, 2.09), decays=(0.5, 0.3, 0.2), amps=(1, 0.5, 0.3))
    s = int(rng.uniform(0, 0.4) ** 2 * SR)
    glass[s:s + len(p)] += p * rng.uniform(0.2, 0.7)
glass += band(noise(1.0), 1500, 9000) * env(1.0, 0.0005, 0.08) * 1.5
save("SFX/Objetos", "vidrio_romper", glass, peak=0.75)

fire = sweep(noise(1.6), 150, 900, 0.8) * env(1.6, 0.05, 0.9, 0.3)
crackle = np.zeros(int(1.6 * SR))
for _ in range(40):
    s = int(rng.uniform(0.1, 1.5) * SR)
    c = band(noise(0.01), 1500, 8000) * env(0.01, 0.0002, 0.003)
    crackle[s:s + len(c)] += c * rng.uniform(0.3, 1)
save("SFX/Objetos", "molotov_fuego", fire + crackle * 0.5, peak=0.8)

grains = np.zeros(int(0.9 * SR))
for _ in range(400):
    s = int(rng.uniform(0, 0.8) * SR)
    g = band(noise(0.004), 2500, 9000) * env(0.004, 0.0001, 0.0015)
    grains[s:s + len(g)] += g * rng.uniform(0.1, 0.6)
save("SFX/Objetos", "sal", grains * np.linspace(1, 0.4, len(grains)), peak=0.5)

rip = band(noise(0.6), 1000, 6000) * np.abs(rng.normal(0, 1, int(0.6 * SR))).clip(0, 3) * env(0.6, 0.02, 0.4, 0.2)
save("SFX/Objetos", "venda", band(rip, 800, 7000), peak=0.5)

# Door creak: a stick-slip pulse train (the hinge) through a wooden body resonance.
creak_len = 1.4
pulses = np.zeros(int(creak_len * SR))
pos, i = 0.05, 0
while pos < creak_len - 0.05:
    rate = 90 + 70 * np.sin(pos * 3.2) + rng.normal(0, 6)
    pulses[int(pos * SR)] = rng.uniform(0.6, 1.0)
    pos += 1.0 / max(rate, 30)
creak = band(np.convolve(pulses, np.exp(-np.arange(200) / 25.0) * np.sin(np.arange(200) * 2 * np.pi * 700 / SR))[:len(pulses)], 250, 3500)
save("SFX/Objetos", "puerta_abrir", reverb(creak * np.hanning(len(creak)) ** 0.3, 0.8, 0.2), peak=0.7)
save("SFX/Objetos", "puerta_cerrar", reverb(mix(0.6, (thump(110, 55, 0.6, 0.12), 0, 1),
                                                   (band(noise(0.6), 300, 2000) * env(0.6, 0.001, 0.05), 0, 0.7),
                                                   (metal(1500), 0.06, 0.4)), 0.8, 0.25))

# Prayer: a soft "ah" of low voices (detuned saws through vowel formants), growing for the 3 s of R.
prayer_len = 3.2
voices = sum(tone(f * lfo(prayer_len, 5.0, 0.004, 1.0, rng.uniform(0, 6)), prayer_len, "saw")
             for f in (110, 110.6, 164.8, 220.4, 261.6))
ah = band(voices, 600, 900) + 0.6 * band(voices, 1050, 1300) + 0.3 * band(voices, 100, 300)
ah *= np.clip(t_axis(prayer_len) / 2.5, 0, 1) ** 1.5
save("SFX/Objetos", "rezo", reverb(ah, 2.0, 0.4), peak=0.5)
done_len = 3.0
shimmer = sum(bell(f, done_len) for f in (523, 659, 784, 1046))
save("SFX/Objetos", "rezo_completo", reverb(shimmer, 2.5, 0.5), peak=0.6)

# ---------------------------------------------------------------- enemies
# Cultist alert: an Aztec "death whistle" (ehecachichtli), the most chilling sound the cult could carry.
w_len = 1.6
air = noise(w_len)
scream = band(air, 900, 1500) + 0.8 * band(air, 1700, 2600) + 0.3 * band(air, 3000, 5000)
howl = tone(glide(1300, 900, w_len, 0.7) * lfo(w_len, 7, 0.02, 1.0), w_len, "saw") * 0.15
whistle = drive((scream + howl) * env(w_len, 0.15, 0.6, 0.6), 2.0)
save("SFX/Enemigos", "cultista_alerta", reverb(whistle, 1.5, 0.35), peak=0.8)

# La Llorona: a ghostly wordless wail (no words: the "¡Ay, mis hijos!" needs a real voice).
l_len = 3.2
pitch = np.concatenate([glide(330, 620, 0.9, 0.6), glide(620, 260, l_len - 0.9, 1.4)]) * lfo(l_len, 6, 0.025, 1.0)
voice = tone(pitch, l_len, "saw")
wail = band(voice, 700, 1100) + 0.7 * band(voice, 1100, 1500) + 0.3 * band(voice, 2400, 3000)
wail += band(noise(l_len), 1000, 4000) * 0.15
wail *= env(l_len, 0.2, 1.4, 1.0)
save("SFX/Enemigos", "llorona_grito", reverb(wail, 3.0, 0.55), peak=0.8)
sob_len = 10.0
sob = np.zeros(int(sob_len * SR))
for start in (0.5, 3.6, 6.4):
    d = rng.uniform(1.6, 2.4)
    p = glide(rng.uniform(420, 500), rng.uniform(260, 320), d, 0.8) * lfo(d, 7, 0.03, 1.0)
    v = tone(p, d, "saw")
    s = (band(v, 600, 1000) + 0.5 * band(v, 1100, 1500)) * env(d, 0.15, d * 0.7, 0.2) * lfo(d, 3.5, 0.5, 0.5) ** 2
    sob += place(sob_len, s, start)
save("Ambiente", "llorona_llanto", make_loop(reverb(sob, 3.0, 0.6)[:int((sob_len + 1.0) * SR)], 1.0), peak=0.6, loop=True)

# El Charro Negro's horse: a strangled, too-deep neigh with a growl under it.
n_len = 1.9
trill = lfo(n_len, 14, 0.06, 1.0)
neigh_pitch = np.concatenate([glide(700, 900, 0.3), glide(900, 380, n_len - 0.3, 1.2)]) * trill
horse = tone(neigh_pitch, n_len, "saw")
neigh = band(horse, 500, 900) + 0.6 * band(horse, 1300, 2000) + band(noise(n_len), 800, 3000) * 0.25
growl = band(tone(glide(85, 60, n_len), n_len, "saw") * (1 + 0.5 * noise(n_len)), 60, 600) * 0.6
save("SFX/Enemigos", "charro_relincho", reverb(drive((neigh + growl) * env(n_len, 0.05, 0.8, 0.6), 1.8), 1.8, 0.35))
hurt = mix(2.0, (bell(330, 2.0, partials=(1, 1.41, 2.32, 3.17)), 0, 0.8), (thump(90, 40, 2.0, 0.2), 0, 1),
           (band(tone(glide(110, 70, 1.5), 1.5, "saw"), 80, 700) * env(1.5, 0.05, 0.9, 0.2), 0.1, 0.8))
save("SFX/Enemigos", "charro_herido", reverb(hurt, 1.5, 0.35))

# The Tzitzimime: a dissonant screech of several voices that never agree.
z_len = 2.2
screech = sum(tone(f * lfo(z_len, rng.uniform(9, 14), 0.03, 1.0), z_len, "saw") for f in (1180, 1265, 1790, 2510))
screech = band(screech, 900, 6000) + band(noise(z_len), 2000, 8000) * 0.4
save("SFX/Enemigos", "tzitzimime_chillido", reverb(drive(screech * env(z_len, 0.08, 1.0, 0.7), 2.5), 2.0, 0.45), peak=0.8)
strike_z = mix(1.6, (thump(70, 30, 1.6, 0.4), 0, 1.3), (band(noise(1.6), 1500, 7000) * env(1.6, 0.0005, 0.03), 0, 0.8),
               (band(noise(1.6), 60, 400) * env(1.6, 0.002, 0.5), 0, 0.8))
save("SFX/Enemigos", "tzitzimime_golpe", reverb(drive(strike_z, 2.0), 1.5, 0.3))

# ---------------------------------------------------------------- ambience loops (30 s)
A = 31.0
# Xochimilco: crickets, frogs, water lapping and a low wind.
crickets = np.zeros(int(A * SR))
for _ in range(3):
    f = rng.uniform(4200, 5200)
    rate = rng.uniform(15, 22)
    chirp = tone(f, A) * (lfo(A, rate, 0.5, 0.5) > 0.6) * lfo(A, rng.uniform(0.3, 0.6), 0.5, 0.5, rng.uniform(0, 6)) ** 4
    crickets += band(chirp, 3000, 7000) * 0.15
frogs = np.zeros(int(A * SR))
for _ in range(22):
    d = rng.uniform(0.15, 0.3)
    f = rng.uniform(180, 320)
    croak = band(tone(f, d, "square") * lfo(d, rng.uniform(25, 40), 0.5, 0.5), 150, 1500) * env(d, 0.01, d * 0.6)
    s = int(rng.uniform(0, A - 1) * SR)
    frogs[s:s + len(croak)] += croak * rng.uniform(0.15, 0.4)
lapping = band(noise(A), 150, 900) * lfo(A, 0.35, 0.35, 0.5) ** 2 * 0.35
wind = sweep(noise(A), 200, 400, 1.0) * lfo(A, 0.07, 0.3, 0.6) * 0.3
save("Ambiente", "amb_xochimilco", make_loop(crickets + frogs + lapping + wind, 1.0), peak=0.5, loop=True)

# Hacienda: a dry wind that whistles, and crows far away.
dry = band(noise(A), 300, 2500) * lfo(A, 0.05, 0.4, 0.6) + 0.2 * band(noise(A), 1800, 2300) * lfo(A, 0.11, 0.5, 0.5) ** 3
crows = np.zeros(int(A * SR))
for start in (3.0, 4.0, 13.5, 22.0, 22.6, 23.2):
    d = 0.35
    caw = tone(glide(780, 520, d) * lfo(d, 40, 0.05, 1.0), d, "saw")
    caw = drive(band(caw, 500, 2500) + band(noise(d), 800, 2500) * 0.6, 2.0) * env(d, 0.02, 0.25, 0.05)
    crows += place(A, reverb(caw, 1.2, 0.5), start, 0.25)
save("Ambiente", "amb_hacienda", make_loop(dry * 0.4 + crows, 1.0), peak=0.45, loop=True)

# Ex-convento: a stone drone, water drops echoing, a far rumble.
drone = band(noise(A), 40, 160) * lfo(A, 0.04, 0.3, 0.7) + 0.2 * tone(55, A) * lfo(A, 0.09, 0.5, 0.5)
drops = np.zeros(int(A * SR))
for _ in range(18):
    d = 0.06
    f = rng.uniform(900, 1800)
    drop = tone(glide(f, f * 1.6, d), d) * env(d, 0.001, 0.04)
    drops += place(A, drop, rng.uniform(0, A - 2), rng.uniform(0.2, 0.5))
save("Ambiente", "amb_convento", make_loop(drone + reverb(drops, 2.5, 0.6)[:int(A * SR)], 1.0), peak=0.45, loop=True)

# Torch crackle loop (the convent and the ritual).
torch = band(noise(11.0), 100, 800) * 0.25 * lfo(11.0, 0.7, 0.3, 0.7)
for _ in range(220):
    torch += place(11.0, band(noise(0.012), 1500, 8000) * env(0.012, 0.0002, 0.004), rng.uniform(0, 10.9), rng.uniform(0.1, 0.6))
save("Ambiente", "antorcha", make_loop(torch, 1.0), peak=0.4, loop=True)


# ---------------------------------------------------------------- music
def pad(freqs, seconds, bright=900, detune=0.003):
    """Detuned saw voices through a low-pass: the dark synth bed under everything."""
    x = np.zeros(int(seconds * SR))
    for f in freqs:
        for d in (-detune, 0, detune):
            x += tone(f * (1 + d) * lfo(seconds, 0.2, 0.002, 1.0, rng.uniform(0, 6)), seconds, "saw")
    return band(x, 40, bright)


def drum(f0, decay=0.35, length=0.8):
    return thump(f0 * 1.6, f0, length, decay) + band(noise(length), 60, 400) * env(length, 0.001, 0.05) * 0.4


def woodblock(f, length=0.25):
    return bell(f, length, partials=(1, 2.1, 3.4), decays=(0.25, 0.15, 0.1), amps=(1, 0.4, 0.2))


# Main menu: a slow dark chord (D minor with an added flat 2), a far bell every few bars.
M = 41.0
menu = pad((73.4, 110, 146.8, 174.6), M, 700) * lfo(M, 1 / 20, 0.25, 0.75)
menu += pad((77.8, 155.6), M, 500) * lfo(M, 1 / 13, 0.5, 0.5) ** 3 * 0.5
for start in (2.0, 12.0, 22.0, 32.0):
    menu += place(M, reverb(bell(293.7, 4.0), 3.0, 0.5), start, 3.0)
save("Music", "mus_menu", make_loop(menu, 1.0), peak=0.5, loop=True)

# Tension (a cultist is chasing): pulsing low strings at 120 bpm, a semitone clash and a rising cluster,
# like Left 4 Dead's "they're coming" music.
T = 17.0
eighth = 0.25
tension = np.zeros(int(T * SR))
for k in range(int((T - 1) / eighth)):
    note = 73.4 if (k // 8) % 2 == 0 else 77.8
    hit = pad((note, note * 2), eighth * 0.95, 1400) * env(eighth * 0.95, 0.005, 0.18)
    tension += place(T, hit, k * eighth, 1.0 if k % 4 == 0 else 0.6)
for k in range(int((T - 1) / 1.0)):
    tension += place(T, drum(55, 0.25), k * 1.0, 1.4)
cluster = pad((587, 622, 659), T, 3000) * lfo(T, 1 / 8, 0.5, 0.5) ** 2 * 0.25
save("Music", "mus_tension", make_loop(tension + cluster, 1.0), peak=0.6, loop=True)

# Horde stinger (an encounter starts): a brass-like dissonant swell and a drum hit.
S_len = 3.5
brass = drive(pad((110, 116.5, 164.8, 233), S_len, 2500) * np.clip(t_axis(S_len) / 1.8, 0, 1) ** 2, 2.0)
brass *= np.where(t_axis(S_len) < 2.0, 1.0, np.exp(-(t_axis(S_len) - 2.0) * 3))
save("Music", "stg_horda", reverb(mix(S_len, (brass, 0, 1), (drum(45, 0.6, 1.5), 1.95, 2.0)), 2.0, 0.3), peak=0.8)

# Ritual: huehuetl drums, a teponaztli (two-tone log drum) and a choir drone.
R = 17.0
ritual = pad((110, 164.8), R, 900) * 0.25
beat_s = 0.5
pattern = [1, 0, 0, 1, 0, 1, 0, 0]
for k in range(int((R - 1) / beat_s)):
    if pattern[k % 8]:
        ritual += place(R, drum(60, 0.3), k * beat_s, 1.6)
    ritual += place(R, woodblock(420 if k % 2 else 560), k * beat_s + 0.25, 0.25)
choir_v = sum(tone(f * lfo(R, 5, 0.004, 1.0, rng.uniform(0, 6)), R, "saw") for f in (220, 261.6, 329.6))
ritual += (band(choir_v, 600, 900) + 0.5 * band(choir_v, 1000, 1300)) * lfo(R, 1 / 8, 0.4, 0.6) * 0.5
save("Music", "mus_ritual", make_loop(reverb(ritual, 1.5, 0.25)[:int(R * SR)], 1.0), peak=0.6, loop=True)

# Victory: bittersweet, A minor to F to C to E, slow bells over a soft pad.
V = 22.0
victory = np.zeros(int(V * SR))
for k, chord in enumerate(((110, 220, 261.6, 329.6), (87.3, 174.6, 220, 261.6), (130.8, 196, 261.6, 329.6), (82.4, 164.8, 207.7, 246.9))):
    seg = pad(chord, 5.5, 1200) * np.hanning(int(5.5 * SR)) ** 0.5
    victory += place(V, seg, k * 5.0, 0.6)
    victory += place(V, reverb(bell(chord[-1] * 2, 3.0), 2.0, 0.5), k * 5.0 + 0.2, 2.0)
save("Music", "mus_victoria", victory, peak=0.6)

# Defeat: the heart slows and stops, and a low drone remains.
D = 9.0
defeat = pad((55, 58.3), D, 400) * env(D, 1.0, 6.0) * 0.6
pos, gap = 0.2, 0.7
while pos < 5.5:
    defeat += place(D, mix(0.75, (beat, 0, 1), (beat, 0.17, 0.7)), pos, 1.5 * (1 - pos / 7))
    pos += gap
    gap *= 1.25
save("Music", "mus_derrota", defeat, peak=0.7)

for name, seconds in written:
    print(f"{name:34s} {seconds:5.1f} s")
print(f"{len(written)} sounds")
