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
| #24 | No hay tecla de recarga en los controles del GDD: la escopeta recarga sola (2 s) cuando se vacía o cuando recoges balas teniéndola vacía | `PlayerShotgun` |
| #24 | El Padre tiene la escopeta desde el inicio; Q o la rueda alternan machete y escopeta | `WeaponSwitcher` |
| #24 | La escopeta con balas de sal hace el 25 % a los espíritus (tabla de balance), aunque la receta diga que sirve contra ellos | `WPN_Escopeta.asset` (spiritMultiplier) |
| #25 | Mientras mantiene R, el Padre va al 15 % de su velocidad; el círculo se queda donde rezó (no lo sigue) y el enfriamiento de 20 s empieza al completar el rezo | `PlayerTuning` |
| #26 | Mientras no exista la pantalla de derrota (#48), el Padre reaparece solo a los 3 s | `PlayerRespawn` (autoRespawnSeconds) |
| #26 | Dentro del cuarto seguro los cultistas no ven ni oyen al Padre (el GDD dice que es la única zona sin amenazas) | `CultistAI` / `SafeRoom` |
| #27 | Escalado por instancia (velocidad, daño, vista y espera de los cultistas), oleadas cada vez que se vence la anterior o a los 25 s, y el botín con sus probabilidades | `Data/Difficulty/DIF_*`, `Data/Enemies/CultistTuning_*` (tabla en GDD §20) |
| #31 | Agua bendita: 20 de daño a espíritus en 3 m. Línea de sal de 3 m. Veladora: 45 s, revela a 8 m. Copal: humo de 7 m. Cruz: 10 normal, carga de 1 s, 40 a espíritus cargada. El molotov no quema al Padre | `Data/Items/Effects`, `Prefabs/Items/Effects`, `WPN_CruzPlata` |
| #32 | Los objetos clave no ocupan espacios del inventario (bolsa aparte, como los materiales). Textos iniciales de 6 notas (se pueden reescribir en #33) | `KeyItemBag`, `Data/Notes` |
| #34 | El ritual se llena con 60 s de R mantenida, a 4 m del ancla; al morir se reinicia completo | `RitualController` en cada nivel |
| #35 | Agarre de la Llorona: 2 s, 15 de golpe + 4/s. Persigue a 3.4 m/s (el Padre corre a 5.5), lo ve a 12 m, se rinde a 22 m, queda 12 s bajo el agua. La veladora y el agua bendita la hunden, igual que la sal y el rezo. Las armas no la dañan. | `Data/Enemies/LloronaTuning.asset`, `LloronaEntity` |
| #39 | Charro: trota a 5 m/s, embiste a 12 m/s cuando el Padre está a 14 m (25 de daño, sigue 8 m de largo), queda débil 4 s tras cruzar una canaleta llena; las armas no le hacen nada. Las canaletas solo aceptan agua bendita después de colocar las monedas | `Data/Enemies/CharroTuning.asset`, `HolyWaterChannel` |
| #45 | Tzitzimime: aparece en el techo a la mitad de la barra; invisible la barra no avanza, revelada avanza normal y con la linterna al doble; ataque de área cada 7 s (4 s en la huida) con 1.5 s de aviso, 3 m y 20 de daño; en la forma final camina a 2.2 m/s tras el Padre. Sin derrumbe (deseable) | `Data/Enemies/TzitzimimeTuning.asset`, `TzitzimimeEntity` |
| #47 | No hay bocetos de menús (#7): se usó la paleta del HUD. Créditos con "6 integrantes · nombres pendientes": Jesús debe poner los nombres. Las instancias 2 y 3 cargan las escenas de prueba hasta #40 y #46. Música, efectos y voces se aplican con `AudioVolume` en cada fuente; el AudioMixer llega con los issues de sonido | `MainMenu.unity` (CreditsPanel), `Data/InstanceCatalog.asset`, `AudioVolume` |
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
