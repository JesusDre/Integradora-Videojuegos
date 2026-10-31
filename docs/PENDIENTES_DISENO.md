# Decisiones de diseño pendientes

Huecos o contradicciones del GDD (revisión de la v2.1) que el equipo tiene que decidir. En el GDD aparecen marcados como *(Pendiente Pn)*.
Cuando decidan uno: escríbanlo en el GDD, quiten la marca y borren el punto de esta lista. Hasta entonces, Claude debe preguntar antes de implementar esa parte.

La columna "Urgencia" indica en qué sprint bloquea trabajo según la planeación (sección 27 del GDD).

**No hay pendientes abiertos** (29 oct 2026). Si surge uno nuevo, agréguenlo con esta tabla:

| # | Tema | Qué falta decidir | Urgencia |
|---|---|---|---|
| P17 | | | |

## Decisiones de Claude para revisar
El GDD no fija estos detalles. Claude eligió un valor para no detener el trabajo; Jesús puede cambiarlo en el Inspector o pedir otro. No bloquean nada.

| Issue | Decisión | Dónde se cambia |
|---|---|---|
| #23 | Cantidades de las recetas: agua bendita = 1 botella + 1 sal; balas de sal = 2 casquillos + 2 sal → 4 balas; línea de sal = 3 sal; veladora = 1 cera + 1 mecha; molotov = 1 botella + 1 mezcal + 1 trapo; venda = 1 tela; cruz = 1 plata + 1 madera; chancla bendita = 1 chancla + 1 agua bendita; copal = 1 resina + 1 brasas | `Data/Recipes/RCP_*.asset` |
| #23 | Recetas conocidas desde el inicio: las 5 de la instancia 1 y la venda. Cruz de plata, chancla bendita y copal se desbloquean con notas (#32) | `RecipeBook` en `PF_Player` |
| #23 | Las balas de sal ocupan un espacio del inventario (hasta 24 en uno) | `ITM_BalasSal.asset` |
| #22 | El inventario (Tab) no pausa el juego | `InventoryUI` |
| #24 | La escopeta recarga sola (2 s) cuando se vacía o cuando recoges balas teniéndola vacía. Desde #113 también con C (elegida por Jesús; R ya es rezar) | `PlayerShotgun` |
| #24 → #134 | ~~El Padre tiene la escopeta desde el inicio~~. Desde #134 las armas se encuentran: se empieza con machete o escopeta al azar y Q recorre las armas que se cargan | `WeaponBag`, `WeaponSwitcher`, `StartingLoadout` |
| #24 | La escopeta con balas de sal hace el 25 % a los espíritus (tabla de balance), aunque la receta diga que sirve contra ellos | `WPN_Escopeta.asset` (spiritMultiplier) |
| #25 | Mientras mantiene R, el Padre va al 15 % de su velocidad; el círculo se queda donde rezó (no lo sigue) y el enfriamiento de 20 s empieza al completar el rezo | `PlayerTuning` |
| #26 | Dentro del cuarto seguro los cultistas no ven ni oyen al Padre (el GDD dice que es la única zona sin amenazas) | `CultistAI` / `SafeRoom` |
| #27 | Escalado por instancia (velocidad, daño, vista y espera de los cultistas), oleadas cada vez que se vence la anterior o a los 25 s, y el botín con sus probabilidades | `Data/Difficulty/DIF_*`, `Data/Enemies/CultistTuning_*` (tabla en GDD §20) |
| #31 | Agua bendita: 20 de daño a espíritus en 3 m. Línea de sal de 3 m. Veladora: 45 s, revela a 8 m. Copal: humo de 7 m. Cruz: 10 normal, carga de 1 s, 40 a espíritus cargada. El molotov no quema al Padre | `Data/Items/Effects`, `Prefabs/Items/Effects`, `WPN_CruzPlata` |
| #32 | Los objetos clave no ocupan espacios del inventario (bolsa aparte, como los materiales). Textos iniciales de 6 notas (se pueden reescribir en #33) | `KeyItemBag`, `Data/Notes` |
| #34 | El ritual se llena con 60 s de R mantenida, a 4 m del ancla; al morir se reinicia completo | `RitualController` en cada nivel |
| #35 | Agarre de la Llorona: 2 s, 15 de golpe + 4/s. Persigue a 3.4 m/s (el Padre corre a 5.5), lo ve a 12 m, se rinde a 22 m, queda 12 s bajo el agua. La veladora y el agua bendita la hunden, igual que la sal y el rezo. Las armas no la dañan. | `Data/Enemies/LloronaTuning.asset`, `LloronaEntity` |
| #39 | Charro: trota a 5 m/s, embiste a 12 m/s cuando el Padre está a 14 m (25 de daño, sigue 8 m de largo), queda débil 4 s tras cruzar una canaleta llena; las armas no le hacen nada. Las canaletas solo aceptan agua bendita después de colocar las monedas | `Data/Enemies/CharroTuning.asset`, `HolyWaterChannel` |
| #45 | Tzitzimime: aparece en el techo a la mitad de la barra; invisible la barra no avanza, revelada avanza normal y con la linterna al doble; ataque de área cada 7 s (4 s en la huida) con 1.5 s de aviso, 3 m y 20 de daño; en la forma final camina a 2.2 m/s tras el Padre. Sin derrumbe (deseable) | `Data/Enemies/TzitzimimeTuning.asset`, `TzitzimimeEntity` |
| #47 | Los menús usan la paleta del HUD (bocetos y paleta en #2, #6 y #7, generados desde las pantallas reales). Música, efectos y voces se aplican con `AudioVolume` en cada fuente; el AudioMixer llega con los issues de sonido | `MainMenu.unity` (CreditsPanel), `Data/InstanceCatalog.asset`, `AudioVolume` |
| #48 | Resumen de instancia con tiempo, muertes y notas leídas. El equipaje no lleva objetos clave y empieza con vida 100. Elegir una instancia desde la selección también da el equipaje de la anterior. Al terminar la campaña se borra la partida (la selección sigue abierta) | `VictoryScreen`, `PlayerPersistence.CaptureForNextInstance` |
| #40 | Hacienda: el Charro solo caza dentro de su terreno (x 55–145, z 66–129); la tercera nota (panteón) explica la pelea; la tienda de raya reutiliza la choza de la instancia 1 hasta tener arte propio | `I2_Hacienda` (Charro_Territorio, NOTE_I2_Capilla) |
| #46 | Ex-convento: la escalera de caracol del coro es una rampa recta; el túnel de huida se abre con el ritual (reja); niebla roja durante el ritual (densidad 0.05); la celda del prior reutiliza la choza; sin derrumbe (deseable) | `I3_Convento`, `RitualAtmosphere` |
| #136 | Agacharse con Ctrl izquierdo (C ya es recargar): mitad de velocidad, sin correr, altura 1.15 m; los cultistas ven al Padre agachado solo a la mitad de su rango (15 → 7.5 m). El ojo aparece agachado o cuando alguien lo persigue | `PlayerController`, `CultistAI`, `StealthEyeUI` |
| #135 | Pistola 24/16 de daño, 8 tiros, recarga 1.4 s. Metralleta 10/7 por bala, 25 tiros, 0.09 s entre balas. Ballesta 75/65, 1 virote, recarga 2 s, sin ruido; los virotes no son benditos. Martillo chillón 14 a personas y 22 a espíritus. Dónde están: martillo en la mesa de Xochimilco; pistola y ballesta en la de la hacienda (con 16 balas y 6 virotes); metralleta en la del convento (con 30 balas) | `WPN_*`, mesas de suministros |
| #134 | Arma inicial al azar (50 %): machete, o escopeta con 6 balas de sal. Balas de sal = ×1.5 de daño y hieren espíritus (al 25 %, como antes); balas normales = solo personas. Al recargar se prefieren las de sal y los tipos no se mezclan en el arma | `StartingLoadout`, `RangedWeaponDefinition` |
| #138, #133 | Se recogen solos (a 1.3 m) los objetos, materiales, municiones y ofrendas; las armas con F. Mesa de suministros con 2 vendas, 6 balas de sal y 3 de sal en cada cuarto seguro | `PickupItem`, `MesaSuministros` |
| #131 | Rezo de 1.5 s; escudo de 10 s (lo mismo que el círculo) con +50 % de daño de todas las armas; el ritual se llena solo dentro de un círculo de 4 m alrededor del ancla en 60 s; dentro se puede usar el rezo normal | `PlayerTuning`, `PlayerPrayer`, `RitualController` |
| #50 | Niveles de la mezcla: Música −3 dB, Ambiente −4 dB, Efectos y Voces 0; la música y el ambiente bajan 8 dB mientras habla el Padre. Metas por tipo en `nivelar.py` (pasos −24, voces −16, enemigos −13, disparo −10) | `AM_Principal`, `VoiceDucking`, `nivelar.py` |
| #125 | Disparo de escopeta nuevo: la versión «pesada sostenida» (C1, 4.5 s con trueno y clic del cañón), elegida por Jesús entre cinco | `sintetizar_escopeta.py`, `SFX/Armas/escopeta_disparo.wav` |
| #123 | Voz del Padre generada (MBROLA mx2) hasta que alguien del equipo la grabe; en la frase 6 respira en vez de decir «Respira» (el subtítulo conserva el texto). La Llorona llama al salir del agua (lejos o cerca a partir de 15 m) y grita al perseguir. La campana de la capilla dobla cada 50 a 90 s; las risas de las muñecas, cada 15 a 30 s | `VoiceLines`, `Audio_123` en cada nivel |
| #102 | El cielo del ex-convento muestra la estrella negra de ocho puntas de la secta (el GDD solo pide noche cerrada). La luz ambiental sigue siendo de color fijo, no la del cielo, para no cambiar la iluminación ya ajustada | `M_Cielo_*`, escenas |
| #101 | Texturas propias generadas por código (sin packs). Los modelos de Blender (personajes, props, vegetación) siguen con color plano por ahora: su estilo low-poly se lee bien así y no tienen UV | `Art/Textures/Niveles`, `WorldUVBaker` |
| #115 | El cultista avisa con un silbato de la muerte azteca (no hay voces grabadas). La música de tensión entra cuando un cultista persigue o ataca y sigue 4 s más; la hacienda no tiene música de ritual (allí es la pelea con el Charro). Pasos: madera en el puente, el muelle y la trajinera de Xochimilco; piedra en todo el convento salvo el jardín | `MusicDirector`, `PlayerFootsteps`, escenas |
| #113 | Machete de 1.2 m, un golpe cada 0.4 s y alcance 2.8 m; escopeta en primera persona a 2.4× (pedido de Jesús) | `WPN_Machete`, `PF_Vista_Machete`, `PF_Player` |
| #111 | Escopeta con 8 perdigones y 5° de dispersión, 48/20 de daño (mata a un cultista de un tiro de cerca); machete más largo (alcance 2.4 m); manos de cubo sin dedos. Pedido directo de Jesús | `WPN_Escopeta`, `WPN_Machete`, `chr_brazos_padre.py` |
| #35 | Se activó el posprocesado en la cámara del Padre para la distorsión del llanto (también se ven la viñeta y el bloom del Global Volume en todas las escenas) | `PF_Player` (cámara) |

## Decididas (29 oct 2026, ya están en el GDD)
- **P4** El inventario y los materiales pasan completos a la siguiente instancia.
- **P10** Clic derecho apunta con la escopeta; Q o la rueda cambian de arma; los objetos rápidos 1–4 se usan al instante (lanzables hacia donde mira el Padre).
- **P11** Valores iniciales de balance: tabla en la sección 12 del GDD.
- **P12** Autoguardado al entrar y al salir del cuarto seguro; se reaparece con 60 de vida (o más si se guardó con más); la huida no tiene checkpoint.
- **P14** Vendajes+ pasa a deseable; el MVP queda con 8 recetas y vendajes.
- **P1** La trajinera es un recorrido por guion sobre una ruta fija; el jugador no la maneja.
- **P3** En el ancla, mantener R llena la barra del ritual sin enfriamiento; soltar R la pausa; un golpe de 15 o más la hace retroceder 10 % (también los agarres de la Llorona).
- **P6** El ancla de la Llorona tiene 3 veladoras fijas que se encienden con F; no gastan inventario.
- **P16** Círculo de rezo de 4 m de radio y Padre de 1.8 m, escritos en el GDD.
- **P2** El Charro Negro se vence solo con 3 golpes de cruz sobre la línea de agua bendita, sin barra de ritual. Las monedas van en un pedestal de la capilla (no es el altar de crafteo) y abren la fase final. Él no entra a la capilla; los cultistas sí.
- **P5** Los cultistas de las instancias 2 y 3 sueltan tela y casquillos; el convento tiene alijos de sal y botellas.
- **P13** Reiniciar (pausa) carga el último cuarto seguro, igual que Reintentar. Opciones: sensibilidad, invertir eje Y, volúmenes (general, música, efectos, voces) y subtítulos.
- **P15** Todo el trabajo (código, arte, audio, UI, niveles y textos) lo hace Jesús con Claude; los demás integrantes solo presentan la defensa con `docs/DEFENSA.md`.
- **P7** La pila de la capilla da 1 agua bendita con F, hasta 3 veces; se rellena al reaparecer tras morir.
- **P8** La línea de agua bendita se traza en 2 o 3 canaletas fijas frente a la capilla: F + 1 agua bendita, dura 20 s.
- **P9** El copal revela durante 15 s a la Tzitzimime, que en su fase 2 es invisible; solo revelada se le daña con luz y rezo.

## También por verificar (no es diseño)
- **Rúbrica:** tal como está transcrita, la sección A suma 78 y la B2 suma 32 (110 en total). Revisen contra el documento del profesor.
