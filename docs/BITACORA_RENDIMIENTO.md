# Bitácora de rendimiento (issue #65)

Cómo se mide: en la build (no en el Editor) con el modo de prueba `-benchmark` (ver DEFENSA, "Rendimiento").
- **Recorrido:** el juego abre cada instancia y lleva al Padre a su zona más pesada: el ancla con las veladoras, o el terreno del Charro.
- **Carga:** empieza todos los encuentros del nivel, con la niebla, el ambiente animado y la música encendidos.
- **Medición:** mide 10 s sin sincronía vertical, para ver el margen real y no el tope de 60 Hz del monitor.

**Equipo de prueba:**
- **Tarjeta de video:** AMD Radeon RX 6600.
- **Procesador:** AMD Ryzen 3 3200G.
- **Pantalla:** monitor de 1366 × 768 a 60 Hz, en pantalla completa.
- **Sistema:** Linux (Fedora 43).
- **Calidad:** PC.

## 31 de octubre de 2026 · build de Linux

| Instancia | Cultistas activos | FPS promedio (3 corridas) | 1 % más lento (peor corrida) |
|---|---|---|---|
| Xochimilco | 11 | 332 · 334 · 336 | 141 |
| Hacienda | 14 | 371 · 386 · 382 | 149 |
| Ex-convento | 13 | 421 · 431 · 420 | 201 |

**Resultado:** las tres instancias están muy por encima de 60 FPS, incluso en su peor 1 % de cuadros. En ninguna corrida hubo un cuadro de más de 50 ms (un tirón visible). No hace falta optimizar.

**Luces en tiempo real** (el issue pide revisarlas):

| Instancia | Luces | Con sombra | Partículas |
|---|---|---|---|
| Xochimilco | 12 | 2 | 1 |
| Hacienda | 20 | 2 | 2 |
| Ex-convento | 25 | 2 | 14 |

- **Por qué no pesan:** URP dibuja como máximo 4 luces extra por objeto y solo 2 proyectan sombra (la luna o el sol y la linterna).
- **Antorchas y veladoras:** tienen luz sin sombra, que es barata.

**Lo que se aprendió en la medición:** en las primeras corridas el juego parecía congelarse al cargar Xochimilco. La pila del hilo principal mostró que estaba en pausa: Unity detiene el juego cuando su ventana pierde el foco, y es lo correcto en un juego normal. El modo de prueba ahora sigue corriendo en segundo plano. El "1 % a 9 FPS" de la hacienda en la primera medición era esa misma pausa, no lentitud.

## 31 de octubre de 2026 · build de Windows

La build de Windows (`AnimasMexicanas.exe`, Direct3D 11) se probó en la misma PC con Proton, la capa de Steam que corre juegos de Windows en Linux:

| Instancia | Cultistas activos | FPS promedio | 1 % más lento |
|---|---|---|---|
| Xochimilco | 9 | 258 | 158 |
| Hacienda | 14 | 283 | 172 |
| Ex-convento | 13 | 301 | 111 |

- **Resultado:** sin excepciones en el registro del juego.
- **Por qué es un poco menor que en Linux:** Proton traduce Direct3D a Vulkan, y eso cuesta algo. En Windows real debería rendir igual o mejor.
- **Al cerrar:** Wine reporta una función de accesibilidad que no implementa. No es del juego.

**Pendiente:**
- **Windows real:** repetir en una PC con Windows; la prueba de Proton no la sustituye del todo.
- **Otra PC:** repetir en otra computadora, si el equipo tiene una más modesta.
