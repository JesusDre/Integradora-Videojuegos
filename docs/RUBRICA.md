# Rúbrica del curso (100 puntos) — proyecto 3D

70 puntos generales + 30 específicos de 3D.

> ⚠️ **Por verificar contra la rúbrica original del curso:** tal como está transcrita, la sección A suma **78** puntos (no 70) y la B2 suma **32** (no 30), 110 en total. Algún valor se copió mal. Mientras no se compare con el documento del profesor, **usen los puntos de cada fila como peso relativo** para priorizar (ver "Cobertura en el plan" al final), no como calificación exacta. Al verificarla, corrijan las cifras y borren esta nota.

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

## Cobertura en el plan
Cada rubro, de mayor a menor peso según la transcripción, con dónde lo cubre el GDD v2.2 (sección) y la planeación (sprint). **Estado**: ✅ cubierto · ⚠️ cubierto con riesgo · ❌ hueco.

| Rubro | Pts | Dónde se cubre | Estado y acción |
|---|---|---|---|
| Gameplay principal | 15 | Loop (§4), mecánicas (§5), MVP (§25) · S2–S7 | ⚠️ El MVP es grande (8 recetas, 3 entidades, ritual, trajinera) y toda la programación es de Jesús. Si un sprint se atrasa, recortar del GDD **antes** de entregar, empezando por lo deseable. |
| Sistemas de juego | 10 | Vida, resistencia, inventario, materiales (§5, §12 balance, §19) · S2–S3 | ✅ Límites con `Mathf.Clamp` y validaciones (CLAUDE.md). Victoria y derrota en §6. |
| Controles | 8 | §14, Opciones con sensibilidad (§17) · S1, S5 | ✅ `Time.deltaTime` obligatorio; sensibilidad en Opciones. |
| Nivel y progresión | 8 | 3 instancias (§10), más cultistas por instancia (§12, §20) · S4, S6, S7 | ✅ La instancia 2 cambia la experiencia (persecución a caballo, sin ritual de barra). |
| Menús y flujo | 8 | §17: principal, pausa, derrota, victoria y créditos · S5 | ✅ Adelantado al S5 para tenerlo en el playtest externo. |
| UI / HUD | 7 | §16 · S1–S3 (HUD), S5 (menús) | ✅ Reacciona por eventos (ARQUITECTURA). Coherencia con el arte: usar la paleta del issue #2. |
| Build y estabilidad | 7 | §30, S9 "build probado en otra PC" | ⚠️ No dejar el build para el S9: sacar un build de Windows en cada demo del viernes (PLAN_IA) y probarlo en otra PC al menos en S5 y S8. |
| Movimiento y físicas 3D | 6 | `CharacterController` (ARQUITECTURA) · S1 | ✅ Sin salto ni agacharse por alcance; revisar colliders de puertas, canales y trajinera (§29). |
| Modelos y animaciones | 6 | PIPELINE_ASSETS (brazos del Padre, Mixamo) · S1, S3, S5, S6 | ⚠️ Los **brazos del Padre** y sus animaciones (machete, escopeta, lanzar, rezo) no tienen sprint en §27. Agregarlos al S3–S4. El caballo del Charro es el modelo más riesgoso. |
| Defensa individual | 5 | PLAN_IA (principio), PROMPTS #9 | ⚠️ El criterio pide que **cada integrante** conozca el código, y todo lo hace Jesús. `docs/DEFENSA.md` se escribe junto con cada sistema, se completa en el S8 (#52) y Jesús lo presenta al equipo en el S7 (#66); cada quien practica con el prompt #9. |
| Enemigos / IA | 5 | Cultista con máquina de estados (§21), 3 entidades por fases (§13) · S1–S7 | ✅ |
| Audio y feedback | 5 | §23, `AudioManager` · S1, S4, S6, S7 | ✅ Cada acción importante con un `AudioCue` y feedback visual (CLAUDE.md). |
| Cámara 3D | 5 | §15 (rotación vertical limitada, balanceo leve) · S1 | ✅ Brazos en su propia capa para no atravesar paredes (PIPELINE_ASSETS). |
| Materiales e iluminación | 5 | Paleta e iluminación por instancia (§22) · S5–S7 | ✅ Materiales URP/Lit; luz baja intencional con linterna, veladoras y antorchas. |
| Diseño del escenario 3D | 5 | §10, PROMPTS #3 (blockout) · S1–S7 | ⚠️ Xochimilco es casi plano: aprovechar altura con puentes, embarcaderos y la Isla de las Muñecas; el convento ya tiene criptas y techo. Guiar al jugador con luz (veladoras) hacia los objetos clave. |
| IA / Navegación | 3 | NavMesh (AI Navigation) · S2 | ✅ La trajinera va por ruta fija, no por NavMesh; el Charro a caballo necesita su propio agente (radio y velocidad mayores). |
| Optimización | 2 | §29 (FPS en la zona más pesada) · S8 | ⚠️ Medir FPS desde el S4 con el Profiler; cuidar luces en tiempo real (veladoras) y niebla. |

### Acciones que salen de esta revisión
1. Crear issues para los brazos del Padre (modelo y animaciones) en el S3–S4.
2. Build de Windows en cada demo del viernes; prueba en otra PC en el S5 y el S8.
3. `docs/DEFENSA.md` al día con cada sistema y presentado al equipo antes del S8 (defensa individual).
4. Medir FPS desde el S4.
5. Verificar los puntos contra la rúbrica original del profesor.
