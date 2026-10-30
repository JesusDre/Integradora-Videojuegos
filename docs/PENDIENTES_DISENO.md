# Decisiones de diseño pendientes

Huecos o contradicciones del GDD (revisión de la v2.1) que el equipo tiene que decidir. En el GDD aparecen marcados como *(Pendiente Pn)*.
Cuando decidan uno: escríbanlo en el GDD, quiten la marca y borren el punto de esta lista. Hasta entonces, Claude debe preguntar antes de implementar esa parte.

La columna "Urgencia" indica en qué sprint bloquea trabajo según la planeación (sección 27 del GDD).

**No hay pendientes abiertos** (29 oct 2026). Si surge uno nuevo, agréguenlo con esta tabla:

| # | Tema | Qué falta decidir | Urgencia |
|---|---|---|---|
| P17 | | | |

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
