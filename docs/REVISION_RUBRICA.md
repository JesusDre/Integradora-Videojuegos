# Pasada de rúbrica (issue #51)

Revisión del 31 de octubre de 2026 contra `docs/RUBRICA.md`, criterio por criterio.

Estado:
- ✅ **cumple:** probado en Play Mode o en la build, con la evidencia anotada.
- 🔧 **se corrigió aquí:** fallaba y se arregló en este mismo PR.
- 👤 **depende del equipo:** no se puede comprobar con código.

## La prueba de punta a punta
Una corrida automática en Play Mode recorrió el juego como lo haría un jugador. Solo se saltó el combate, que ya se probó en cada issue:
- **Inicio:** menú principal → *Nueva partida* → Xochimilco.
- **Derrota:** morir muestra la pantalla de derrota, y *Reintentar* revive al Padre.
- **Xochimilco:** ritual → salida → pantalla de victoria → *Continuar* → Hacienda.
- **Hacienda:** se vence al Charro → salida → victoria → Ex-convento.
- **Ex-convento:** ritual → salida → victoria → menú con los créditos.
- **Pausa** (Esc): *Continuar* quita la pausa, *Reiniciar* recarga el nivel y *Salir* lleva al menú.

En toda la corrida no hubo ni una excepción en la consola. La build de Windows y la de Linux también corren sin errores (#64, #65).

## A. Criterios generales

| Rubro | Criterio | Estado | Evidencia |
|---|---|---|---|
| Gameplay principal | Objetivo claro y mecánica principal funcional | ✅ | Reunir ofrendas, hacer el ritual y escapar; corrida de punta a punta |
| | Ciclo acción → consecuencia → progreso | ✅ | Recolectar → craftear en el altar → objetos de fe → ritual → siguiente instancia (#23, #31, #34) |
| | Mecánicas del GDD implementadas | ✅ | Cada mecánica MVP tiene su sección en DEFENSA §5 |
| Controles | Responden al input | ✅ | Input System; controles del GDD §14 más C para recargar (#113) |
| | Velocidad y sensibilidad cómodas | ✅ | Sensibilidad e invertir Y en Opciones (#47) |
| | Movimiento con `deltaTime` | ✅ | Revisión del código: todo movimiento y rotación por tiempo usa `Time.deltaTime`; el mouse, a propósito, no (DEFENSA §5.1) |
| Sistemas de juego | Vida, recursos u objetivos | ✅ | Vida, resistencia, inventario, materiales, munición, objetos clave |
| | Los valores reaccionan a las acciones | ✅ | HUD por eventos (#22) |
| | Límites y validaciones | ✅ | `Mathf.Clamp` en vida y resistencia; topes en inventario (8 espacios, 99 por material) y en munición (#13, #22, #24) |
| | Victoria y derrota claras | ✅ | Pantallas de victoria y derrota (#48), probadas en la corrida |
| Nivel y progresión | 3 niveles de inicio a fin | ✅ | Xochimilco, Hacienda y Ex-convento, recorridos en la corrida |
| | Progresión o dificultad | ✅ | Escalado de cultistas por instancia (#27) |
| | Un segundo nivel que cambie la experiencia | ✅ | La hacienda es una persecución a caballo con golpes en el agua bendita, no un ritual de barra (#39) |
| UI / HUD | Información necesaria | ✅ | Vida, resistencia, rezo, munición, espacios rápidos (#22) |
| | Responde al estado | ✅ | Eventos de vida, munición y rezo |
| | Legible y sin estorbar | ✅ | Bocetos de HUD (#7) |
| | Coherente con el arte | ✅ | Paleta del #2 |
| Menús y flujo | Menú principal | ✅ | Probado en la corrida |
| | Menú de pausa | ✅ | Continuar, Reiniciar y Salir probados |
| | Iniciar, pausar, continuar, reiniciar, finalizar | ✅ | Probado en la corrida |
| | Pantalla de victoria/derrota | ✅ | Probadas en la corrida |
| Enemigos / IA | Desafío | ✅ | Cultistas, encuentros y oleadas; tres entidades |
| | Lógica propia | ✅ | Máquina de estados del Cultista (#3); fases de cada entidad (#35, #39, #45) |
| Audio y feedback | Sonido de acciones importantes | ✅ | #115, #123, #125 |
| | Música o ambientación | ✅ | Música dinámica y ambiente por nivel (#115); mezcla con AudioMixer (#50) |
| | Feedback visual o sonoro | ✅ | #44 |
| Build y estabilidad | Ejecutable funcional | ✅ | Build de Windows (#64) y de Linux (#65) |
| | Iniciar y terminar sin errores | ✅ | Corrida de punta a punta sin excepciones |
| | Sin errores graves | ✅ | Corrida y builds sin excepciones. Falta probar en otra PC con Windows (#64) |
| Defensa individual | Cada integrante conoce el código | 👤 | `docs/DEFENSA.md`; presentarlo al equipo (#66) y practicar las preguntas |

## B2. Criterios específicos 3D

| Rubro | Criterio | Estado | Evidencia |
|---|---|---|---|
| Movimiento y físicas | Movimiento en los ejes del género | ✅ | `CharacterController` (#11) |
| | Colliders y Character Controller | ✅ | Revisados en los niveles (#36, #40, #46) |
| Cámara 3D | Responde al input | ✅ | `PlayerLook` |
| | Sin atravesar objetos ni ángulos injugables | 🔧 | La cámara no atraviesa paredes (su plano cercano de 5 cm queda dentro de la cápsula de 35 cm) y no se voltea (±80°). **Pegado a una pared, la pared tapaba el brazo izquierdo y el machete se hundía en ella.** Ahora los brazos y las armas se dibujan con una cámara aparte, encima del mundo (ver DEFENSA §5.1) |
| Modelos y animaciones | Escala y orientación | ✅ | 1 unidad = 1 m; los modelos se hacen a escala real en Blender |
| | Animaciones apropiadas | ✅ | Cultista, entidades, caballo, brazos del Padre (#4, #17, #28) |
| | Transiciones correctas | ✅ | Animator con triggers y estados (#28) |
| Materiales e iluminación | Materiales bien configurados | ✅ | Texturas por metro (#101), cielos (#102) |
| | Iluminación intencional | ✅ | Luna, atardecer o noche cerrada por instancia; linterna, veladoras y antorchas |
| Diseño del escenario | Aprovecha el 3D | ✅ | Puentes y chinampas, coro alto, criptas y catacumbas del convento |
| | Límites y guía | ✅ | Bardas, canales, veladoras que llevan a los objetos clave |
| IA / Navegación | NavMesh | ✅ | Cultistas y Charro con `NavMeshAgent` |
| Optimización | Sin problemas de rendimiento | ✅ | 332–431 FPS en la build, 1 % más lento ≥ 141 (#65) |

## Condiciones mínimas para aprobar
1. **Build ejecutable:** ✅ sí.
2. **Gameplay real:** ✅ sí.
3. **De inicio a victoria o derrota:** ✅ sí.
4. **Mecánicas obligatorias del GDD:** ✅ sí.
5. **Sin errores críticos:** ✅ sí.
6. **Conocimiento del equipo:** 👤 con `DEFENSA.md` (#66).

## Correcciones hechas en esta pasada
- **Brazos y armas siempre visibles** (cámara superpuesta en la capa `PrimeraPersona`): antes, pegado a una pared, la pared tapaba el brazo izquierdo y el machete se hundía en ella.

## Pendientes que no son de código
- **Probar la build en otra PC con Windows** (#64, sprint 8).
- **Playtests y bitácora** (#21): los hace Jesús.
- **Presentar `DEFENSA.md` al equipo** (#66), para que cada quien practique las preguntas.
