# Plan para construir el juego con IA

## Principio
La IA acelera; **Jesús decide, prueba y explica**. Todo el trabajo del juego (código, arte, audio, UI, niveles y textos) lo hace Jesús con Claude. Los demás integrantes no producen piezas, pero la rúbrica da 5 puntos a la defensa individual: **cada integrante** debe poder explicar el proyecto. Para eso existe `docs/DEFENSA.md`, que se escribe junto con cada sistema y que todo el equipo lee antes de la defensa.

## Qué herramienta para cada trabajo
| Trabajo | Herramienta |
|---|---|
| Repo, README, issues, hitos | Claude Code + `gh` |
| Conceptos y planos | Dibujo propio o IA de imágenes (revisar licencia y mantener coherencia) |
| Personajes y entidades 3D | Meshy o Tripo → Mixamo → Unity; Blender MCP para limpiar escala, origen y polígonos y exportar FBX |
| Props y kits de ambiente | Blender MCP (Poly Haven y Poly Pizza en CC0, generación o formas simples por código) → FBX |
| Armar escenas, blockout, colocar prefabs | Claude Code + MCP de Unity (o ProBuilder a mano) |
| Código C# (sistemas, IA, entidades, UI) | Claude Code en el repo, con `CLAUDE.md` y los docs |
| Bocetos de UI y su paso a Unity | Boceto propio → Claude Code |
| Notas del culto, subtítulos, textos | Claude Desktop (Project) para borradores; revisión de Jesús |
| Audio | Grabar o recopilar (Freesound, Pixabay, OpenGameArt); Claude ayuda a integrarlo (`AudioManager`, mixer) |
| Revisión y rúbrica | `/revisar-rubrica` + revisión del pull request |
| Explicación para la defensa | `docs/DEFENSA.md`, actualizado en el mismo pull request de cada sistema |
| Playtest y balance | Jesús juega y, cuando se pueda, alguien externo (la IA no juega por ti) |

## Ciclo de cada issue
1. Elegir issue con `/siguiente-issue Sprint N` (o a mano en el tablero) y moverlo a **En curso**.
2. Crear rama `sprint-N/issue-<n>-nombre-corto`.
3. Pedir a Claude un **plan antes del código** (ver `PROMPTS.md`). Corregir el plan.
4. Implementar en pasos pequeños; compilar y probar en Play Mode tras cada paso.
5. Pedirle a Claude que explique lo que hizo; si no lo entiendes, no lo subas.
6. Actualizar la sección del sistema en `docs/DEFENSA.md`.
7. `/revisar-rubrica`, arreglar lo que salga.
8. Pull request con descripción y el criterio de finalización del issue marcado; fusionar y mover a **Hecho**.

## Ritmo semanal
- **Lunes:** revisar el tablero del sprint y ordenar los issues.
- **Martes a jueves:** trabajo por issue.
- **Viernes:** build de Windows jugable (#64); actualizar la bitácora de playtest.
- **Domingo:** cierre del sprint; lo que no termina pasa al siguiente con una nota.

## Foco por sprint
Sigue los hitos de GitHub Issues (el backlog real); si se mueve un issue, se actualiza aquí y en la sección 27 del GDD.

| Sprint | Fechas | Issues principales |
|---|---|---|
| 1 | 12–18 oct | Repo, tablero y licencias (#1), paleta de pantallas (#2), concepto del Cultista (#3) |
| 2 | 19–25 oct | Cultista 3D (#4), plano de la instancia 1 (#5), bocetos de HUD, inventario, crafteo y menús (#6, #7), sonidos y música (#8–#10), proyecto base de Unity (#11) |
| 3 | 26 oct–1 nov | Controlador, linterna e interacción (#12), salud y combate (#13), inventario (#14), IA del Cultista (#15), blockout de la instancia 1 (#16), conceptos y Llorona 3D (#17, #18), props (#19), sonidos (#20), playtests (#21), rúbrica (#63) |
| 4 | 2–8 nov | HUD e inventario en Unity (#22), altar y crafteo (#23), escopeta (#24), rezo (#25), cuarto seguro y guardado (#26), encuentros y oleadas (#27), brazos del Padre (#28), objetos clave (#29), plano de la instancia 2 (#30), builds del viernes (#64), FPS (#65) |
| 5 | 9–15 nov | Efectos de objetos (#31), objetos clave, notas y recetas (#32), textos (#33), ritual (#34), La Llorona (#35), nivel de la instancia 1 (#36), Charro 3D (#37), plano de la instancia 3 (#38) |
| 6 | 16–22 nov | Charro Negro (#39), nivel de la instancia 2 (#40), Tzitzimime 3D (#41), sonidos (#42, #43), feedback y VFX (#44) |
| 7 | 23–29 nov | Tzitzimime (#45), nivel de la instancia 3 (#46), menús (#47), victoria, derrota y créditos (#48), **congelamiento el 29 nov** (#49), presentar `DEFENSA.md` al equipo (#66) |
| 8 | 30 nov–6 dic | Audio (#50), pasada de rúbrica y bugs (#51), `DEFENSA.md` completo (#52) |
| 9 | 7–13 dic | Build final y entrega (#58), presentación y video (#59) |

## Reglas de oro
- Nada de claves de API en el repo ni en los prompts.
- Guardar el `.blend` antes de usar Blender MCP; nunca aceptar assets sin licencia anotada.
- Lo que no se pueda terminar se recorta del GDD **antes** de la entrega (primero Director, derrumbe, Fácil/Difícil, 3 slots).
- Los números de balance son iniciales: se prueban, no se asumen.
