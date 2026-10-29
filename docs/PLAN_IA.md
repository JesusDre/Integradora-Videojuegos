# Plan para construir el juego con IA

## Principio
La IA acelera; **la persona responsable decide, prueba y explica**. La rúbrica da 5 puntos a la defensa individual: cada quien debe poder explicar su parte aunque la haya hecho con IA.

## Qué herramienta para cada trabajo
Toda la programación la hace Jesús con Claude Code; no hay más programadores.

| Trabajo | Herramienta | Responsable humano |
|---|---|---|
| Repo, README, issues, hitos | Claude Code + `gh` | Jesús |
| Conceptos y planos | Dibujo propio (si usan IA de imágenes, revisar licencia y mantener coherencia) | Ali |
| Personajes y entidades 3D | Meshy o Tripo → Mixamo → Unity; Blender MCP para limpiar escala, origen y polígonos y exportar FBX | Juan |
| Props y kits de ambiente | Blender MCP (Poly Haven y Poly Pizza en CC0, generación o formas simples por código) → FBX | Juan y Ali |
| Armar escenas, blockout, colocar prefabs | Claude Code + MCP de Unity (o ProBuilder a mano) | Ali o Jesús |
| Código C# (sistemas, IA, entidades, UI) | Claude Code en el repo, con `CLAUDE.md` y los docs | Jesús |
| Pasar bocetos de UI a Unity | Claude Code con las imágenes de David | David y Jesús |
| Notas del culto, subtítulos, textos | Claude Desktop (Project) para borradores; revisión humana | Nadia con Ali |
| Audio | Personas graban o recopilan; Claude solo ayuda a integrarlo (`AudioManager`, mixer) | Nadia y Hanna |
| Revisión y rúbrica | `/revisar-rubrica` + revisión humana del pull request | Jesús |
| Playtest y balance | Personas (la IA no juega por ustedes) | Todos |

## Ciclo de cada issue
1. Elegir issue con `/siguiente-issue Sprint N` (o a mano en el tablero) y moverlo a **En curso**.
2. Crear rama `sprint-N/issue-<n>-nombre-corto`.
3. Abrir Claude Code y pedir un **plan antes del código** (ver `PROMPTS.md`). Corregir el plan.
4. Implementar en pasos pequeños; compilar y probar en Play Mode tras cada paso.
5. Pedirle a Claude que explique lo que hizo; si no lo entiendes, no lo subas.
6. `/revisar-rubrica`, arreglar lo que salga.
7. Pull request con descripción y el criterio de finalización del issue marcado; mover a **En revisión**.
8. Jesús revisa; al aprobar, merge y **Hecho**.

## Ritmo semanal
- **Lunes (30 min):** revisar el tablero del sprint y repartir. 
- **Martes a jueves:** trabajo por issue; avisar bloqueos en el chat.
- **Viernes:** demo con un build jugable y revisión de pull requests; actualizar bitácora de playtest.
- **Domingo:** cierre del sprint; lo que no termina vuelve a Backlog con una nota.

## Foco de IA por sprint
Sigue la tabla de planeación del GDD (sección 27); si cambia allá, cambia aquí.

| Sprint | Fechas | Qué pedirle a la IA |
|---|---|---|
| 1 | 12–18 oct | Estructura del repo y del proyecto, controlador en primera persona, linterna, interacción, Cultista de prueba que persigue, blockout de la sección 1 |
| 2 | 19–25 oct | Inventario de 8 espacios, salud, daño y combate, IA completa del Cultista con NavMesh, HUD de vida y resistencia |
| 3 | 26 oct–1 nov | Altar y crafteo con ScriptableObjects, guardado, escopeta y efectos de objetos, encuentros y oleadas, La Llorona (primera versión), menú del altar |
| 4 | 2–8 nov | Rezo, ritual reutilizable, La Llorona completa, trajinera y arena del ritual: instancia 1 de inicio a fin |
| 5 | 9–15 nov | Recetas de cruz y chancla, Charro Negro (persecución), blockout de la hacienda, luz, niebla y **menús principal, pausa, derrota y victoria** |
| 6 | 16–22 nov | Cruz cargada y línea de agua bendita, derrota del Charro y huida a pie: instancia 2 completa |
| 7 | 23–29 nov | Copal, Tzitzimime (3 fases) y oleadas, catacumbas y ritual final, créditos y guardado completo. **Congelamiento de funciones el 29 nov** |
| 8 | 30 nov–6 dic | Pasada de rúbrica (`/revisar-rubrica`), bugs, balance, ajuste de audio, notas de defensa |
| 9 | 7–13 dic | Build final probado en otra PC, README, presentación |

Los menús van en el sprint 5 y no al final: son condición mínima para aprobar y conviene tenerlos desde el playtest externo.

## Reglas de oro
- Una persona por escena a la vez; el resto trabaja en prefabs o en escenas de prueba.
- Nada de claves de API en el repo ni en los prompts.
- Guardar el `.blend` antes de usar Blender MCP; nunca aceptar assets sin licencia anotada.
- Lo que no se pueda terminar se recorta del GDD **antes** de la entrega (primero Director, derrumbe, Fácil/Difícil, 3 slots).
- Los números de balance son iniciales: se prueban, no se asumen.
