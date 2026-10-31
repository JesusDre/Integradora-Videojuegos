# Sonidos del juego: los que tenemos y los que faltan

Lista para comparar o reemplazar con sonidos generados (por ejemplo, Gemini Lyria) o grabados.
Juego de terror estilo Left 4 Dead: el silencio y el ambiente pesan más que la música, y la música solo
sube cuando hay peligro.

**Después de generar o reemplazar sonidos**, corre `python3 Tools/Audio/nivelar.py`: iguala el volumen de cada archivo con los de su tipo (#50).

**Para reemplazar un archivo** se guarda con el **mismo nombre y en la misma carpeta**
(`Assets/_Project/Audio/...`). Unity lo toma solo y no hay que reconectar nada.
- **Formato:** WAV, mono para los efectos y estéreo para la música; 44.1 o 48 kHz.
- **Loops:** que el final empate con el inicio, sin silencio en las orillas.
- **Licencia:** cada archivo nuevo se anota en `docs/LICENCIAS.md` con la herramienta y sus términos.

**Qué herramienta conviene.** Lyria genera sobre todo **música instrumental**. La música y los fondos de ambiente son los mejores candidatos. Las voces con palabras y los efectos cortos (un golpe, un clic) suelen salir mejor grabados o con una herramienta de efectos de sonido. La columna "Lyria" indica dónde vale la pena intentarlo.

Los prompts están en inglés porque así los entienden mejor estas herramientas.

---

## 1. Música (lo más importante para Lyria)

| Archivo | Ahora (sintetizado) | Qué necesitamos | Duración | Loop | Lyria |
|---|---|---|---|---|---|
| `Music/mus_menu.wav` | Acorde oscuro de sintetizador en re menor y una campana lejana | Tema del menú: inquietante, lento, mexicano y religioso a la vez | 40–60 s | Sí | Sí |
| `Music/mus_tension.wav` | Cuerdas graves pulsando a 120 bpm que chocan medio tono, tambor y un racimo agudo | Persecución: un cultista te vio. Debe subir el pulso sin tapar los pasos | 16–32 s | Sí | Sí |
| `Music/stg_horda.wav` | Metales disonantes que crecen y un golpe de tambor | "Ahí vienen": aviso corto cuando empieza un encuentro | 3–5 s | No | Sí |
| `Music/mus_ritual.wav` | Tambores huehuetl, teponaztli y coro sin palabras | El ritual final: tambores prehispánicos y coro, trance y peligro | 16–32 s | Sí | Sí |
| `Music/mus_victoria.wav` | Acordes la menor, fa, do y mi con campanas | Victoria agridulce: salió vivo, pero no hay alegría | 20–30 s | No | Sí |
| `Music/mus_derrota.wav` | Latido que se detiene y un zumbido grave | Muerte: el corazón se apaga | 8–10 s | No | Sí |

**Prompts sugeridos**
- **mus_menu:** *Slow dark ambient horror theme, Mexican folk horror, distant church bell, low male choir humming, detuned guitar harmonics, deep drone in D minor, sparse, eerie, no drums, seamless loop.*
- **mus_tension:** *Tense horror chase music, Left 4 Dead style, pulsing low string ostinato at 120 bpm, dissonant minor second clashes, heavy tom hits on the downbeat, rising high string cluster, no melody, relentless, seamless loop.*
- **stg_horda:** *Short horror stinger, dissonant brass swell rising for two seconds then a huge low drum hit, Left 4 Dead horde warning, 4 seconds.*
- **mus_ritual:** *Pre-Hispanic Aztec ritual music, huehuetl drums and teponaztli log drum in a hypnotic pattern, ominous wordless choir drone, death whistle accents, dark trance, 90 bpm, seamless loop.*
- **mus_victoria:** *Bittersweet slow ending theme, solo nylon guitar and soft strings, A minor to F to C to E, distant church bells, melancholic relief, not triumphant.*
- **mus_derrota:** *Horror death sting, slowing heartbeat that stops, low cello drone fading out, silence at the end, 9 seconds.*

---

## 2. Ambiente (fondos en loop; buenos candidatos para Lyria o efectos)

| Archivo | Ahora | Qué necesitamos | Duración | Loop | Lyria |
|---|---|---|---|---|---|
| `Ambiente/amb_xochimilco.wav` | Grillos, ranas, agua que golpea y viento bajo | Canales de Xochimilco de noche: agua quieta, ranas, grillos, una brisa fría | 30–60 s | Sí | Puede |
| `Ambiente/amb_hacienda.wav` | Viento seco que silba y cuervos a lo lejos | Hacienda abandonada al atardecer: viento seco, cuervos, madera que cruje lejos | 30–60 s | Sí | Puede |
| `Ambiente/amb_convento.wav` | Zumbido grave de piedra, gotas con eco y retumbo lejano | Ex-convento de noche: eco de piedra, goteo, corrientes de aire | 30–60 s | Sí | Puede |
| `Ambiente/antorcha.wav` | Crepitar de fuego | Antorcha encendida, de cerca | 10 s | Sí | No |
| `Ambiente/llorona_llanto.wav` | Sollozos sin palabras con reverberación | La Llorona llorando a lo lejos (sin palabras) | 10–20 s | Sí | Puede |

**Prompts sugeridos (ambiente)**
- **amb_xochimilco:** *Night ambience of Mexican canals, still water, frogs and crickets, cold light breeze, distant eerie silence, no music.*
- **amb_hacienda:** *Abandoned hacienda at dusk, dry whistling wind, distant crows, old wood creaking, desolate, no music.*
- **amb_convento:** *Inside a ruined stone convent at night, deep stone reverb, water drips echoing, low air rumble, oppressive, no music.*

---

## 3. Efectos que ya tenemos (sintetizados; se pueden mejorar)

| Archivo | Qué es | Dónde suena |
|---|---|---|
| `SFX/Armas/escopeta_disparo.wav` | Disparo: chasquido, estruendo grave y cola de eco | Al disparar |
| `SFX/Armas/escopeta_recarga.wav` | Abrir la escopeta, meter 2 cartuchos y cerrar (1.5 s) | Recarga (C o automática) |
| `SFX/Armas/escopeta_vacia.wav` | Clic seco | Disparar o recargar sin balas |
| `SFX/Armas/machete_swing.wav` | Silbido de corte en el aire | Al blandir el machete, la chancla o la cruz; lanzar agua bendita |
| `SFX/Armas/machete_golpe.wav` | Golpe sordo con carne | Machete que conecta |
| `SFX/Armas/chancla_golpe.wav` | Chanclazo | Chancla que conecta |
| `SFX/Armas/cruz_golpe.wav` | Golpe con resonancia metálica | Cruz de plata que conecta |
| `SFX/Jugador/pasos_tierra_1–4.wav` | Pasos en tierra | Caminar en tierra |
| `SFX/Jugador/pasos_madera_1–4.wav` | Pasos huecos en tablones | Puente, muelle, trajinera |
| `SFX/Jugador/pasos_piedra_1–4.wav` | Pasos duros con eco | Convento |
| `SFX/Jugador/jugador_dano.wav` | Golpe al cuerpo y aire que sale | Recibir daño |
| `SFX/Jugador/latido.wav` | Un latido (se repite) | Vida de 30 o menos |
| `SFX/Jugador/respiracion.wav` | Jadeo (loop) | Sin resistencia |
| `SFX/Jugador/linterna_click.wav` | Clic doble | Prender o apagar la linterna |
| `SFX/Jugador/recoger.wav` | Roce de tela y un tintineo | Recoger material |
| `SFX/Jugador/objeto_clave.wav` | Campanas graves en tritono, inquietantes | Recoger muñeca, moneda u obsidiana |
| `SFX/Objetos/altar_campanita.wav` | Campanita aguda | Bendecir en el altar |
| `SFX/Objetos/agua_llenar.wav` | Burbujeo de agua | Pila y canaletas de agua bendita |
| `SFX/Objetos/veladora_encender.wav` | Cerillo y flama | Veladoras, copal, molotov |
| `SFX/Objetos/vidrio_romper.wav` | Vidrio roto | Frasco de agua bendita o molotov que choca |
| `SFX/Objetos/molotov_fuego.wav` | Llamarada que crepita | Fuego de la molotov |
| `SFX/Objetos/sal.wav` | Granos de sal cayendo | Línea de sal |
| `SFX/Objetos/venda.wav` | Tela que se rasga | Usar venda |
| `SFX/Objetos/puerta_abrir.wav` | Bisagra que rechina | Abrir puerta |
| `SFX/Objetos/puerta_cerrar.wav` | Golpe de madera y pestillo | Cerrar puerta |
| `SFX/Objetos/rezo.wav` | Coro grave "ah" que crece en 3 s | Mantener R |
| `SFX/Objetos/rezo_completo.wav` | Campanas brillantes | Rezo completado |
| `SFX/Enemigos/cultista_alerta.wav` | Silbato de la muerte azteca | Un cultista te detecta |
| `SFX/Enemigos/llorona_grito.wav` | Lamento fantasmal sin palabras | Grito de la Llorona |
| `SFX/Enemigos/charro_relincho.wav` | Relincho grave y deformado con gruñido | El caballo del Charro |
| `SFX/Enemigos/charro_herido.wav` | Golpe metálico y gemido grave | El Charro recibe un golpe |
| `SFX/Enemigos/tzitzimime_chillido.wav` | Chillido de varias voces desafinadas | La Tzitzimime se revela |
| `SFX/Enemigos/tzitzimime_golpe.wav` | Golpe grave con crujido de hueso | Ataque de la Tzitzimime |

---

## 4. Lo que faltaba (hecho en #123)

Todo lo de esta sección ya se generó por código y está conectado (ver DEFENSA, "Voces y sonidos que faltaban"). Si alguien del equipo graba la voz del Padre, se reemplazan `Voices/padre_01.wav` a `padre_24.wav` con el mismo nombre.

### Lista original

### Voces (no se pueden sintetizar bien; grabar o usar voz con licencia)
| Qué | Descripción | Cuántos | Duración | Issue |
|---|---|---|---|---|
| Voz del Padre Anselmo | Las 21 frases de `docs/TEXTOS.md` (vida baja, reaparecer, cuarto seguro, objetos clave, ritual y llegadas a cada lugar). Voz grave, cansada, de 60 años, en voz baja y con humor seco | 21 | 1–4 s c/u | #8 |
| Rezo corto en latín | Padre nuestro o *Exorcizo te* murmurado en latín; reemplazaría al coro de `rezo.wav` | 2 opciones | 3 s | #8 |
| Llanto de la Llorona con palabras | "¡Ay, mis hijos!" lejos, cerca y como grito; mujer, lamento largo | 3 | 2–5 s | #9 |
| Cultista: gruñido y grito | Hombre fanático: un gruñido al atacar y un grito al morir | 2–4 | 1–2 s | #8 |
| Cantos de la secta | Coro de hombres cantando en náhuatl inventado o en latín, solemne; para el ritual del convento | 1 loop | 20–30 s | #43 |
| Risas de las muñecas | Risa infantil distorsionada, lejana | 2 opciones | 1–3 s | #8 |

### Efectos que faltan (se pueden sintetizar o conseguir)
| Qué | Descripción | Duración | Loop | Issue |
|---|---|---|---|---|
| Galope del Charro | Caballo galopando sobre tierra, pesado, que se acerca | 4–8 s | Sí | #42 |
| Herraduras | Cascos sobre piedra, al caminar | 2–4 s | Sí | #42 |
| Espuelas | Tintineo metálico de espuelas al moverse | 1–2 s | No | #42 |
| Monedas | Monedas de plata antiguas al recogerlas | 1 s | No | #42 |
| Campana de la capilla | Campana de iglesia con mucha reverberación, sola y lejana | 4–6 s | No | #42 |
| Campanas lejanas en Xochimilco | Campanario de un pueblo a lo lejos, de noche | 6–10 s | No | #9 |
| Remos de la trajinera | Pértiga que entra al agua y empuja, rítmico | 4 s | Sí | #9 |
| Campana grave del convento | Campana muy grave, un solo golpe que vibra largo | 6–8 s | No | #43 |
| Vibración de la Tzitzimime | Zumbido grave que se siente más de lo que se oye, cuando está cerca | 8–10 s | Sí | #43 |
| Pasos en agua | Chapoteo en agua poco profunda | 0.4 s ×4 | No | #8 |

**Prompts de ejemplo para efectos** (si la herramienta los acepta):
- **Galope:** *Heavy black horse galloping on dry dirt, approaching, menacing.*
- **Campana de capilla:** *Single old church bell toll, large stone chapel reverb, distant, at dusk.*
- **Vibración de la Tzitzimime:** *Sub-bass otherworldly hum, pulsing slowly, felt more than heard, cosmic dread, seamless loop.*
