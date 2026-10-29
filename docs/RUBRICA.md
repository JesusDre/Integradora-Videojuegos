# Rúbrica del curso (100 puntos) — proyecto 3D

70 puntos generales + 30 específicos de 3D.

> ⚠️ **Por verificar contra la rúbrica original del curso:** tal como está transcrita, la sección A suma **78** puntos (no 70) y la B2 suma **32** (no 30), 110 en total. Algún valor se copió mal. Corrijan las cifras con el documento del profesor y borren esta nota.
Puntuación parcial por criterio: 100 % funciona y está bien implementado · 75 % funciona con detalles o errores menores · 50 % funciona parcialmente · 25 % implementado pero no funciona · 0 % no implementado.

## A. Criterios generales (70)

| Rubro | Criterio | Pts |
|---|---|---|
| Gameplay principal | Objetivo claro y mecánica principal completamente funcional | 5 |
| | Ciclo identificable: acción → consecuencia → progreso/recompensa | 5 |
| | Las mecánicas principales descritas en el GDD están implementadas | 5 |
| Controles | Los controles responden correctamente al input | 3 |
| | Velocidad y sensibilidad permiten jugar cómodamente | 2 |
| | Movimiento normalizado respecto al tiempo (`deltaTime`) | 3 |
| Sistemas de juego | Sistema de vida, puntuación, recursos, tiempo, objetivos o equivalente | 3 |
| | Los valores reaccionan correctamente a las acciones | 3 |
| | Los valores tienen límites y validaciones | 2 |
| | Condiciones claras de victoria y derrota | 2 |
| Nivel y progresión | Mínimo 3 niveles/escenarios jugables de inicio a fin | 3 |
| | Progresión, incremento de dificultad o evolución | 3 |
| | Un segundo nivel/fase/ronda que modifique la experiencia | 2 |
| UI / HUD | Muestra la información necesaria para jugar | 2 |
| | El HUD responde dinámicamente al estado del juego | 2 |
| | Legible, bien distribuido y sin estorbar el gameplay | 1 |
| | Coherente con la dirección artística | 2 |
| Menús y flujo | Menú principal funcional | 2 |
| | Menú de pausa funcional | 2 |
| | Iniciar, pausar, continuar, reiniciar y finalizar una partida | 2 |
| | Pantalla o estado de victoria / derrota / finalización | 2 |
| Enemigos / IA | Desafío mediante enemigos, obstáculos, puzzles, tiempo u otro | 2 |
| | Comportamiento automático o lógica propia en enemigos/objetos | 3 |
| Audio y feedback | Efectos de sonido de acciones importantes | 2 |
| | Música o ambientación sonora acorde | 1 |
| | Feedback visual, sonoro o de animación en acciones importantes | 2 |
| Build y estabilidad | Entrega como ejecutable/build funcional | 3 |
| | Se puede comenzar y terminar una partida sin errores que impidan continuar | 2 |
| | Sin errores graves, cierres inesperados ni comportamientos que rompan el juego | 2 |
| Defensa individual | Cada integrante demuestra conocer el código y las decisiones técnicas | 5 |

## B2. Criterios específicos 3D (30)

| Rubro | Criterio | Pts |
|---|---|---|
| Movimiento y físicas 3D | Movimiento correcto en los ejes necesarios para el género | 3 |
| | Colliders, Character Controller, Rigidbody o equivalente bien configurados | 3 |
| Cámara 3D | Responde correctamente al movimiento/input y al género | 3 |
| | Evita problemas graves (atravesar objetos, perder al jugador, ángulos injugables) | 2 |
| Modelos y animaciones | Escala, orientación y posición coherentes en el escenario | 2 |
| | Personajes/objetos con animaciones apropiadas | 2 |
| | Transiciones de animación correctas | 2 |
| Materiales e iluminación | Materiales/texturas bien configurados | 2 |
| | Iluminación intencional y acorde con la ambientación | 3 |
| Diseño del escenario 3D | Aprovecha el espacio tridimensional | 3 |
| | Límites, obstáculos y elementos que guían al jugador | 2 |
| IA / Navegación | Enemigos autónomos se desplazan bien (NavMesh o equivalente) | 3 |
| Optimización | Sin problemas evidentes por exceso de objetos, luces, físicas, partículas o 3D | 2 |

## Condiciones mínimas para aprobar
1. Build ejecutable y funcional.
2. Gameplay real (no solo escenarios, personajes o menús).
3. Se puede iniciar una partida y llegar a victoria, derrota o finalización.
4. Las mecánicas declaradas como obligatorias en el GDD están implementadas.
5. Sin error crítico que impida continuar la demostración.
6. Los integrantes demuestran conocimiento del desarrollo del proyecto.

> Consecuencia práctica: lo que el GDD marca como "Obligatorio (MVP)" debe existir. Si algo no va a llegar, hay que **actualizar el GDD antes de entregar**, no entregar algo que dice más de lo que hay.
