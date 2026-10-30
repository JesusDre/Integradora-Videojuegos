# Defensa del proyecto: Ánimas mexicanas

Este documento es para **todo el equipo**. Explica qué hace el juego, cómo está construido y por qué se tomó cada decisión, en palabras que se entienden sin saber programar. La rúbrica da 5 puntos a que **cada integrante** demuestre conocer el código y las decisiones técnicas; si lees esto completo, puedes contestar las preguntas del profesor.

Cómo usarlo:
- Lee primero las secciones 1 a 4 (10 minutos). Después, el sistema que te toque explicar.
- Para practicar, usa el prompt #9 de `docs/PROMPTS.md`: Claude te hace preguntas como el profesor.
- Este documento crece con el juego: **cada sistema nuevo agrega su sección en el mismo pull request** (regla de `CLAUDE.md`). Las secciones marcadas "Pendiente" todavía no existen en el juego.

---

## 1. El juego en dos minutos
- **Qué es:** terror en primera persona para un jugador, en Unity, para PC con Windows.
- **Historia:** el Padre Anselmo Quiroz, un exorcista retirado, va a tres lugares malditos de México donde una secta ficticia, los Hijos de la Estrella Negra, está despertando a una entidad del folclore. Para salir de cada lugar tiene que romper el ancla de la entidad.
- **Los tres niveles ("instancias"):** Xochimilco con La Llorona, una hacienda del Bajío con el Charro Negro y un ex-convento con la Tzitzimime. Cada uno dura unos 10 minutos.
- **El ciclo de juego:** avanzar → recoger materiales → sobrevivir a los cultistas → llegar al cuarto seguro → craftear en el altar y curarse → segunda sección → ritual en el ancla → huida.
- **Cómo se gana y se pierde:** se gana cruzando la salida de la instancia 3. Se pierde si la vida llega a 0; entonces aparece la pantalla de derrota y con Reintentar se vuelve al último cuarto seguro.
- **Qué lo hace distinto:** las entidades no se vencen con balas, sino con objetos de fe fabricados en el altar (agua bendita, línea de sal, cruz de plata, copal) y con el rezo.

El diseño completo está en `docs/GDD.md` (versión 2.2).

## 2. Cómo se hizo
- **Quién:** todo el trabajo (código, arte, audio, UI, niveles y textos) lo hizo Jesús, apoyado en Claude (IA). El equipo revisó el diseño y prepara la defensa con este documento.
- **Herramientas:**
  - **Unity 6.3 LTS** (`6000.3.27f1`): el motor del juego. LTS significa versión con soporte largo; se eligió por estabilidad con la entrega en diciembre.
  - **URP** (Universal Render Pipeline): el sistema de gráficos de Unity pensado para PC y consolas comunes; da luces, niebla y post-procesado sin pedir una computadora muy potente.
  - **Claude Code**: escribe y explica el código C# dentro del repositorio. Lee `CLAUDE.md`, que contiene las reglas del proyecto.
  - **Unity CLI y el paquete Pipeline**: permiten que Claude controle el Editor de Unity (crear objetos, abrir escenas) en vez de editar archivos a mano.
  - **Blender MCP, Meshy/Tripo y Mixamo**: para modelos 3D y animaciones.
  - **GitHub**: guarda el proyecto, las tareas (issues, una por tarea con su sprint) y los cambios (pull requests).
- **Cómo se trabajó cada tarea:** un issue → una rama → plan antes de programar → cambios pequeños probados en Play Mode → revisión contra la rúbrica (`/revisar-rubrica`) → pull request → fusión a `main`. La rama `main` está protegida: nada entra sin pull request.

## 3. Conceptos de Unity que hay que conocer
| Término | Qué es, en simple |
|---|---|
| **Escena** | Un nivel o pantalla (menú, Xochimilco…). Contiene todos los objetos de ese momento. |
| **GameObject** | Cualquier cosa en la escena: el jugador, una puerta, una luz. Por sí solo no hace nada. |
| **Componente** | Una pieza que se le pone a un GameObject para darle comportamiento: un collider (choca), una luz, un script. |
| **Script (C#)** | Un componente escrito en código. Ejemplo: el que mueve al jugador cuando presionas WASD. |
| **Prefab** | Un GameObject guardado como plantilla para reutilizarlo: el Cultista se hace una vez y se coloca muchas veces. |
| **ScriptableObject** | Un archivo de datos (no de código): por ejemplo, cuánto cura una venda o qué pide una receta. Sirve para ajustar el balance sin tocar el código. |
| **`Update` y `Time.deltaTime`** | `Update` se ejecuta una vez por cuadro (frame). Multiplicar el movimiento por `Time.deltaTime` (el tiempo que tardó el último cuadro) hace que el jugador se mueva igual de rápido en una computadora lenta que en una rápida. La rúbrica lo revisa. |
| **CharacterController** | Componente de Unity para mover a un personaje que choca con paredes y sube escalones sin usar física realista. |
| **NavMesh** | Un mapa de por dónde se puede caminar. Los enemigos lo usan para encontrar el camino hacia el jugador. |
| **Evento** | Un aviso entre sistemas: cuando la vida cambia, el sistema de salud "avisa" y el HUD se actualiza solo, sin preguntar a cada cuadro. |
| **Máquina de estados** | La forma de organizar la IA: el Cultista está en un estado (patrullar, perseguir, atacar, buscar) y cambia de uno a otro según lo que ve. |

## 4. Configuración del proyecto (ya hecho)
| Decisión | Por qué |
|---|---|
| Unity 6.3 LTS con plantilla **Universal 3D** | Versión estable con soporte largo; URP da buena iluminación con poco costo. |
| Módulo **Windows Build Support (Mono)** | El juego se entrega para Windows y se desarrolla en Linux; el módulo permite generar el `.exe` desde Linux. |
| Paquetes **Input System, AI Navigation, ProBuilder** | Input System lee teclado, mouse y gamepad; AI Navigation crea el NavMesh de los enemigos; ProBuilder permite armar los niveles con bloques dentro de Unity. |
| Se quitaron **Collab, Multiplayer Center y Visual Scripting** | El proyecto usa Git, es de un jugador y se programa en C#; quitarlos deja el proyecto más limpio. |
| **Asset Serialization = Force Text** y **Visible Meta Files** | Unity guarda escenas y prefabs como texto y cada archivo lleva su `.meta` (su identificador). Así Git puede guardar y comparar los cambios. |
| **Git LFS** (`.gitattributes`) | Los archivos pesados (modelos, texturas, audio) se guardan aparte para que el repositorio no se vuelva lento. |
| Carpetas en `Assets/_Project/` | Arte, audio, datos, prefabs, escenas y scripts separados por tipo, como dice `docs/ARQUITECTURA_UNITY.md`. Los assets de terceros van en `Assets/ThirdParty/` con su licencia. |

## 5. Sistemas del juego
Cada sistema sigue el mismo formato: **qué hace**, **cómo funciona** (scripts y piezas), **por qué así** (y qué alternativas había) y **preguntas probables**. Se llena cuando el sistema existe en el juego.

### 5.1 Jugador: movimiento, cámara, linterna e interacción
*Issue #12.*

**Qué hace.** El jugador camina con WASD, corre con Shift mientras le dure la resistencia, mira con el mouse, prende y apaga la linterna con E y usa objetos con F (abrir puertas, recoger cosas). Cuando mira algo usable, el punto del centro de la pantalla se ilumina y aparece el texto "[F] Abrir puerta".

**Cómo funciona.** El prefab `PF_Player` (en `Prefabs/Player`) tiene estas piezas:

| Script | Dónde está | Qué hace |
|---|---|---|
| `PlayerController` | raíz del jugador, con un `CharacterController` | Lee WASD y Shift, calcula la dirección y la velocidad (3 m/s caminando, 5.5 m/s corriendo), aplica gravedad y mueve al jugador multiplicando por `Time.deltaTime`. |
| `PlayerStamina` | raíz | Resistencia de 0 a 100. Correr gasta 15 por segundo; después de 1 s sin gastar se recupera 20 por segundo. Si llega a 0, el jugador queda "agotado" y no puede correr hasta volver a 25. Avisa con un evento cuando cambia (lo usará la barra del HUD). |
| `PlayerLook` | raíz, con una referencia a `CameraPivot` | El mouse gira el cuerpo a los lados y solo la cámara arriba y abajo, con un límite de 80° para que nunca se voltee. |
| `Flashlight` | `CameraPivot/Flashlight`, con una luz tipo foco | E alterna la luz y avisa con un evento. Tiene un espacio para el sonido de clic, que llega con los issues de audio. |
| `Interactor` | `CameraPivot` | Lanza un rayo invisible de 2.5 m hacia donde mira la cámara. Si choca con algo que tenga `IInteractable`, avisa a la UI; si se presiona F, lo usa. |
| `IInteractable` | (interfaz) | El "contrato" que cumple todo lo usable: un texto (`Prompt`) y una acción (`Interact`). La puerta (`Door`) y el objeto de prueba (`PickupItem`) lo implementan; después lo harán el altar, las notas y los objetos clave. |
| `InteractionPromptUI` | prefab `PF_InteractionPrompt` (UI) | Escucha al `Interactor` y muestra u oculta el texto. No pregunta cada cuadro: reacciona al evento `FocusChanged`. |
| `PlayerTuning` | archivo de datos `Data/Player/PlayerTuning.asset` | Guarda todos los números (velocidades, resistencia, sensibilidad, distancia de interacción) para ajustarlos en el Inspector sin tocar el código. |

Los controles están en `Data/Input/AnimasControls.inputactions` (Input System) y son las acciones del proyecto ("project-wide actions"), así que cualquier script las encuentra con `InputSystem.actions.FindAction("Player/Move")`. Ya incluyen todas las teclas del GDD (atacar, apuntar, rezar, objetos rápidos, inventario, pausa) para los siguientes issues. Se quitó el archivo de controles de la plantilla porque traía saltar y agacharse, que no existen en el juego, y ponía interactuar en la E.

Se prueba en `Scenes/Test/Sandbox`: un cuarto oscuro con una puerta y una botella sobre una mesa.

**Por qué así.**
- **`CharacterController` y no `Rigidbody`:** el jugador no necesita física realista (no lo empujan ni salta). El `CharacterController` choca con las paredes y sube escalones sin temblar, y es más fácil de controlar y de explicar.
- **`Time.deltaTime` en el movimiento, pero no en el mouse:** el movimiento se calcula en metros por segundo, así que se multiplica por el tiempo del cuadro para que sea igual en cualquier computadora. El mouse ya entrega "cuánto se movió en este cuadro"; multiplicarlo por `deltaTime` haría la cámara más lenta en computadoras rápidas.
- **Resistencia en su propio script:** el combate (#13) también la gasta y el HUD (#22) la muestra; separarla evita que dependan del movimiento.
- **Una interfaz para todo lo usable:** el `Interactor` no necesita saber si es una puerta o una nota; agregar algo nuevo usable no cambia el `Interactor`.
- **Eventos en vez de revisar cada cuadro:** la UI solo cambia cuando cambia lo que se mira. Es más eficiente y es lo que pide `ARQUITECTURA_UNITY.md`.
- **Números en un ScriptableObject:** se balancean en los playtests sin recompilar.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Por qué el jugador no corre más rápido en diagonal? | La dirección se limita a largo 1 (`Vector3.ClampMagnitude`); sin eso, W + D sumaría las dos direcciones y sería un 41 % más rápido. |
| ¿Cómo evitan que la cámara se voltee? | El ángulo vertical se limita con `Mathf.Clamp` entre −80° y 80°. |
| ¿Qué pasa si la resistencia llega a 0? | El jugador queda agotado: camina, pero no corre hasta recuperar 25 puntos. Así no "tartamudea" corriendo con un poquito de resistencia. |
| ¿Cómo sabe el juego qué objeto estás mirando? | Con un `Physics.Raycast` desde la cámara de 2.5 m de largo; busca el componente `IInteractable` en lo que toca. |
| ¿Por qué la puerta se abre poco a poco? | `Quaternion.RotateTowards` con `deltaTime` la gira 180° por segundo; da feedback visual claro, que la rúbrica pide. |

### 5.2 Salud, daño y combate cuerpo a cuerpo
*Issue #13.*

**Qué hace.** El Padre tiene 100 de vida que no se regenera. Con clic izquierdo da un machetazo: gasta 15 de resistencia, tarda 0.6 s en poder volver a golpear y quita 20 de vida, así que un Cultista (40) cae en 2 golpes. Las vendas curan 30. Si la vida llega a 0, el Padre muere y deja de responder a los controles. Cada golpe se nota: la cámara da un pequeño tirón al golpear, el enemigo se pone rojo un instante y, cuando al Padre le pegan, los bordes de la pantalla se tiñen de rojo.

**Cómo funciona.**
| Script | Dónde está | Qué hace |
|---|---|---|
| `Health` | Padre, Cultista y entidades | Vida actual y máxima. `TakeDamage` y `Heal` siempre la mantienen entre 0 y el máximo (`Mathf.Clamp`). Avisa con eventos: `Changed` (para la barra del HUD), `Damaged` (para los efectos) y `Died`. Tiene un tipo, humano o espíritu, para el daño reducido a espíritus. |
| `MeleeWeaponDefinition` | archivo de datos `Data/Items/Weapons/WPN_Machete.asset` | Daño a humanos (20) y a espíritus (5, una cuarta parte), espera entre golpes, alcance, gasto de resistencia y los sonidos. La chancla bendita será otro archivo igual con otros números. |
| `PlayerCombat` | Padre | Al hacer clic revisa la espera y la resistencia (`TrySpend`), lanza un rayo grueso (`Physics.SphereCastNonAlloc`) desde la cámara y le quita vida al `Health` más cercano que toque, ignorando el cuerpo del propio Padre. |
| `PlayerDeathHandler` | Padre | Al morir apaga movimiento, cámara, combate, interacción y linterna, y libera el cursor. La pantalla de derrota (`DeathScreen`, #47) escucha el mismo evento. |
| `HitFlash` | enemigos | Tiñe de rojo el modelo 0.12 s al recibir daño, con un `MaterialPropertyBlock` (no modifica el material compartido). |
| `DeathAnimation` | enemigos | Al morir dispara la animación `Die` y apaga sus colliders para que el cuerpo no estorbe. |
| `DamageFlashUI` | prefab `PF_DamageFlash` (UI) | Imagen roja a pantalla completa que aparece con cada golpe (más fuerte si el golpe es mayor) y se desvanece. |
| `DamageZone` | escena de prueba | Zona que quita 10 de vida por segundo a quien esté dentro (servirá para fuego o agua maldita). |

En `Sandbox` hay un Cultista con 40 de vida detrás de la puerta, vendas sobre la mesa (se recogen al inventario y se usan con 1–4, ver 5.3) y una zona de daño en la esquina.

**Por qué así.**
- **Un solo `Health` para todos:** el Padre, los cultistas y las entidades se dañan igual; el combate, la IA y el HUD solo necesitan conocer ese componente.
- **Eventos:** `Health` no sabe nada de la UI ni de las animaciones; cada sistema se suscribe a lo que le interesa. Agregar un sonido de dolor después no cambia `Health`.
- **Rayo grueso en vez de fino:** en un juego de terror con poca luz, exigir puntería exacta frustra; un radio de 35 cm perdona rozar al enemigo.
- **Datos del arma en un ScriptableObject:** el balance se ajusta en el Inspector durante los playtests.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Cómo garantizan que la vida no pase de 100 ni baje de 0? | Toda modificación pasa por `TakeDamage` o `Heal`, que usan `Mathf.Clamp(valor, 0, máximo)`; nadie cambia la vida directamente. |
| ¿Por qué el machete mata en 2 golpes? | El GDD da 40 de vida al Cultista y 20 de daño al machete (tabla de balance, sección 12). |
| ¿Qué pasa si atacas sin resistencia? | `TrySpend` devuelve falso y el golpe no sale; el Padre debe esperar a recuperar 25 puntos. |
| ¿Por qué los espíritus reciben menos daño? | El GDD dice que las entidades se vencen con objetos de fe; un machete normal les hace la cuarta parte para empujar al jugador al crafteo. |
| ¿Cómo sabe el jugador que golpeó? | Tirón de cámara al golpear, destello rojo en el enemigo y sonido de impacto (espacio listo para el clip); al recibir daño, la pantalla se tiñe de rojo. |

### 5.3 Inventario, materiales y objetos rápidos
*Issue #14 (la pantalla del inventario y el HUD son #22).*

**Qué hace.** El Padre carga 8 espacios de objetos; los primeros 4 son los objetos rápidos y se usan con las teclas 1–4 (por ejemplo, una venda cura 30). Los materiales de crafteo (sal, botellas…) no ocupan espacios: van a una bolsa aparte con un máximo de 99 de cada uno. Con F se recogen objetos del suelo; si no caben, se quedan ahí. Cada acción muestra un aviso corto abajo de la pantalla: "Recogiste: venda", "Inventario lleno", "Ya tienes la vida completa".

**Cómo funciona.**
| Pieza | Dónde | Qué hace |
|---|---|---|
| `ItemDefinition` | archivos de datos en `Data/Items/` (`ITM_Venda`, `ITM_AguaBendita`, `MAT_Sal`, `MAT_Botella`) | Nombre, descripción, icono, categoría (arma, munición, objeto de fe, consumible, material, objeto clave, nota), cuánto se apila, cuánto cura y qué modelo usa al tirarse. Un objeto nuevo es un archivo nuevo, sin código. |
| `Inventory` | Padre | Los 8 espacios (`InventorySlot`: objeto + cantidad) y la bolsa de materiales (`Dictionary`). `TryAdd` mete lo que quepa y devuelve cuánto entró; `Remove` quita todo o nada (para el crafteo); `UseSlot` usa un objeto; `Drop` lo tira al suelo. Avisa con eventos: `Changed` (para la UI) y `Notice` (para los avisos). |
| `PickupItem` | objetos en el mundo (`PF_Pickup_Generic`) | Implementa `IInteractable`: muestra "Recoger venda ×2" y con F llama a `TryAdd`. Si solo entra una parte, el resto se queda en el suelo. |
| `QuickSlotInput` | Padre | Lee las acciones `QuickSlot1`–`QuickSlot4` del Input System y llama a `UseSlot`. Se apaga al morir. |
| `NotificationUI` | prefab `PF_Notifications` (UI) | Escucha `Notice` y muestra el texto 2 s; luego se desvanece en medio segundo. |

**Detalles que importan.**
- **Nunca se pierde nada en silencio:** con el inventario lleno o 99 materiales, el objeto no desaparece; se queda en el suelo y el jugador recibe un aviso.
- **Una venda no se gasta con la vida llena:** `UseSlot` usa el valor que devuelve `Health.Heal`; si curó 0, el objeto se conserva.
- **Recoger de cerca:** al mirar hacia abajo un objeto pegado, el rayo de interacción empezaba dentro del cuerpo del Padre y chocaba con él. Ahora `Interactor` ignora el cuerpo propio y, si el rayo fino toca la mesa, usa un rayo grueso (20 cm) que elige el objeto usable más cercano. El machete usa la misma corrección.
- **Sin basura de memoria:** los rayos usan versiones `NonAlloc` con un arreglo reutilizado, porque se lanzan en cada cuadro.

**Cómo se probó (Play Mode).** 9 vendas → entran 8 y aparece "Inventario lleno", la novena se queda en la mesa; 120 de sal → entran 99 con aviso; usar una venda cura 30; tirar un objeto lo deja en el suelo con el mismo objeto; mirando la venda desde 75, 45, 30, 20 y 10 cm de la mesa siempre aparece "Recoger venda" (antes fallaba de cerca). Sin errores en la consola.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Por qué los materiales van aparte? | El GDD pide juntar muchos materiales para craftear; si ocuparan espacios, el inventario de 8 se llenaría enseguida. |
| ¿Por qué ScriptableObjects para los objetos? | Los datos (cuánto cura, cuánto se apila) se cambian en el Inspector durante los playtests sin tocar código, y varios objetos del mundo comparten el mismo archivo. |
| ¿Cómo se entera la UI de los cambios? | Por el evento `Changed`; el inventario no conoce a la UI. La pantalla de inventario (#22) solo tendrá que suscribirse. |
| ¿Qué pasa con el agua bendita? | Ya se puede recoger y guardar; su uso (contra entidades) llega con los objetos de fe. Por ahora avisa "todavía no se puede usar". |

### 5.4 IA del Cultista
*Issue #15.*

**Qué hace.** El Cultista camina entre puntos de patrulla. Si ve al Padre (hasta 15 m, en un cono de 110° al frente y sin paredes ni puertas en medio) o lo oye correr (hasta 10 m), lo persigue a 3.5 m/s, más lento que el Padre corriendo. Al llegar a 1.6 m lo ataca con el machete (10 de daño cada 1.5 s). Si lo pierde de vista, va al último lugar donde lo vio y busca alrededor durante 10 segundos; luego vuelve a su ruta y sigue patrullando. Al morir se detiene.

**Cómo funciona.**
| Pieza | Dónde | Qué hace |
|---|---|---|
| `CultistAI` | prefab `PF_Cultista` (Prefabs/Enemies) | Máquina de estados: `Patrol`, `Chase`, `Attack`, `Search` y `Return`. Cada cuadro revisa si ve u oye al jugador y ejecuta el estado actual. Avisa con el evento `StateChanged` (para sonidos o música de tensión). |
| `NavMeshAgent` | el mismo prefab | Calcula el camino y mueve al Cultista sobre el NavMesh esquivando paredes y mesas. La IA solo le dice a dónde ir (`SetDestination`). |
| `NavMeshSurface` | `TestRoom` en `Sandbox` | El mapa de por dónde se puede caminar, horneado de los pisos y paredes (`NavMesh-TestRoom.asset`). |
| `NavMeshObstacle` (carving) | la hoja de la puerta | La puerta no se hornea: es un obstáculo que "recorta" el NavMesh. Cerrada bloquea el paso; abierta lo deja libre. |
| `CultistTuning` | `Data/Enemies/CultistTuning.asset` | Distancias de vista y oído, ángulo de visión, velocidades, alcance y daño del ataque, espera entre golpes y duración de la búsqueda. |
| `AC_Cultista` | Animator | El parámetro `Speed` recibe la velocidad real del agente, así que pasa solo de quieto a caminar y a correr; el trigger `Attack` lanza el machetazo. |

**Detalles que importan.**
- **Vista realista:** primero la distancia, luego el ángulo (no ve por la espalda) y al final un `Physics.Raycast` desde su cabeza al pecho del Padre: si el rayo choca con una pared o con la puerta cerrada, no lo ve.
- **Oído:** caminar es silencioso; correr se oye a 10 m. La escopeta (#24) llamará a `HearNoise` para atraer a los cultistas.
- **El golpe llega con la animación:** el daño se aplica 0.45 s después de empezar el machetazo, cuando el machete baja; si el jugador se aleja a tiempo, esquiva.
- **Sin buscar en cada cuadro:** al jugador lo encuentra una sola vez al empezar (por la etiqueta `Player`), como pide la arquitectura.

**Cómo se probó (Play Mode).** Con el jugador frente a su ruta: patrulla → persigue (1 s) → ataca (la vida del Padre baja 100 → 90 → 80 → 70) → el Padre escapa al otro cuarto → busca durante 10 s → regresa → patrulla de nuevo. El camino siempre fue completo (`PathComplete`), sin atorarse.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Qué es una máquina de estados? | Una forma de organizar el comportamiento: el Cultista siempre está en un estado (patrullar, perseguir, atacar, buscar, regresar) y hay reglas claras para pasar de uno a otro. |
| ¿Cómo encuentra el camino? | Con el NavMesh de Unity: un mapa de zonas caminables. El `NavMeshAgent` calcula la ruta más corta y la sigue esquivando obstáculos. |
| ¿Por qué no ve a través de la puerta? | Porque la vista usa un rayo (`Physics.Raycast`): si lo primero que toca no es el Padre, no lo ve. |
| ¿Por qué el jugador puede escapar? | El Cultista persigue a 3.5 m/s y el Padre corre a 5.5 m/s; además, si lo pierde de vista solo busca 10 segundos. |
| ¿Cómo sabe la animación si camina o corre? | El Animator recibe la velocidad real del agente en `Speed` y mezcla quieto, caminar y correr. |

### 5.5 Altar y crafteo
*Issue #23.*

**Qué hace.** En cada cuarto seguro hay un altar. Con F se abre su menú: a la izquierda, las recetas; a la derecha, qué hace el objeto, qué materiales pide (cuántos tienes / cuántos pide, en rojo los que faltan) y el botón **Bendecir y fabricar**. Al fabricar, se gastan los materiales, el objeto entra al inventario, la luz de las velas brilla un instante (el Padre lo bendice) y aparece "Bendijiste: agua bendita". Usar el altar también devuelve la vida hasta 60 si tenías menos (GDD §5). Las recetas que aún no conoces salen como "Receta desconocida" hasta que encuentres su nota (#32).

![Altar](img/ui/altar.png)

**Las recetas** (GDD §18): agua bendita, balas de sal, línea de sal, veladora, molotov de mezcal y venda se conocen desde el inicio; cruz de plata, chancla bendita y copal se aprenden con notas. Las cantidades exactas están en `docs/PENDIENTES_DISENO.md`.

**Cómo funciona.**
| Pieza | Dónde | Qué hace |
|---|---|---|
| `RecipeDefinition` | `Data/Recipes/RCP_*.asset` | Una receta: el resultado, cuántos hace y sus ingredientes. Un ingrediente puede ser un material o un objeto (la chancla bendita pide un agua bendita). |
| `RecipeBook` | Padre | Las recetas que conoce. `Unlock` agrega una (lo usarán las notas) y avisa con el evento `Unlocked`. |
| `AltarCrafting` | código, sin escena | Las reglas: la receta debe conocerse, deben estar todos los ingredientes y el resultado debe caber. Solo entonces gasta y entrega. |
| `Altar` | prefab `PF_Altar` (Prefabs/Levels) | Lo que se usa con F: cura hasta 60 % y abre el menú. `Bless()` hace brillar las velas. |
| `AltarUI` | dentro de `PF_HUD` | El menú. Se abre con el evento `Altar.Opened`, así cualquier altar de cualquier nivel funciona sin conectarlo a mano. Mientras está abierto apaga la cámara, el ataque, la interacción y el inventario. |
| `Inventory.CanFit` | Padre | Dice si algo cabe sin meterlo, para no gastar materiales en vano. |

**Detalles que importan.**
- **Nunca se pierden materiales:** si el resultado no cabe, los ingredientes regresan y aparece "Inventario lleno".
- **Gastar puede liberar espacio:** para la chancla bendita se gasta un agua bendita que ocupaba un espacio; por eso primero se sacan los ingredientes y luego se revisa si cabe el resultado.
- **No se cierra al abrir:** la misma F que abre el altar no lo cierra en ese mismo cuadro.

**Cómo se probó (Play Mode, `EnemyArena`).** Con 30 de vida, usar el altar la subió a 60. Sin materiales: "faltan ingredientes". Con materiales se fabricaron las 6 recetas conocidas y la sal bajó de 20 a 14 (1 + 2 + 3). La venda con el inventario lleno dio "sin espacio" sin perder la tela. La cruz de plata sin aprenderla quedó bloqueada. La chancla bendita, ya aprendida y con el inventario lleno, sí se fabricó porque gastó el agua bendita. Con el botón del menú se fabricó un agua bendita y salió el aviso. Al cerrar se recuperaron la cámara y el inventario. Consola sin errores.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Cómo sabe el altar si alcanzan los materiales? | `AltarCrafting.HasIngredients` recorre la receta y compara con `Inventory.CountOf` (materiales y objetos). |
| ¿Por qué las recetas son archivos y no código? | Para balancear en el Inspector y agregar recetas sin programar (ScriptableObject). |
| ¿Por qué el altar cura solo hasta 60? | Así lo pide el GDD: el cuarto seguro ayuda, pero las vendas siguen siendo necesarias. |

### 5.6 Escopeta y balas de sal
*Issue #24.*

**Qué hace.** Q (o la rueda del mouse) cambia del machete a la escopeta. Clic izquierdo dispara y clic derecho apunta (la cámara se acerca un poco). Tiene 2 cartuchos; al vaciarse recarga sola en 2 segundos con las balas de sal del inventario, que se fabrican en el altar. Hace 30 de daño a menos de 8 m y 15 más lejos; a los espíritus solo el 25 %. El disparo hace ruido: todo cultista a 20 m lo oye y va a ver (deja de patrullar y persigue). Abajo a la derecha, el HUD muestra el arma en la mano y la munición: "2 | 10" (cargados | en el inventario).

![Escopeta](img/ui/escopeta.png)

**Cómo funciona.**
| Pieza | Dónde | Qué hace |
|---|---|---|
| `RangedWeaponDefinition` | `Data/Items/Weapons/WPN_Escopeta.asset` | Daño cerca y lejos, distancia, capacidad, recarga, radio del ruido, zoom y sonidos. |
| `PlayerShotgun` | Padre | Dispara con un rayo grueso (`SphereCastNonAlloc`, ignorando el cuerpo del Padre); el daño depende de la distancia y del tipo de vida. Avisa a los cultistas con `Physics.OverlapSphereNonAlloc` + `CultistAI.HearNoise`. Recarga con un temporizador, sin corrutinas. |
| `WeaponSwitcher` | Padre | Solo una de las dos armas está encendida (`enabled`), así el clic hace una sola cosa. Si un menú o la muerte lo apagan, apaga también el arma en la mano. |
| `HUDController` | `PF_HUD` | Escucha `WeaponSwitcher.Switched` y `PlayerShotgun.AmmoChanged`. |

**Detalles que importan.**
- **Munición con límites:** los cartuchos nunca pasan de 2 y solo se sacan del inventario los que caben; si no hay balas, no recarga y suena el "clic" vacío.
- **Un error que se corrigió en las pruebas:** al terminar de recargar, sacar las balas del inventario avisaba "cambió el inventario" cuando la escopeta aún marcaba 0 cartuchos, y eso empezaba una segunda recarga. Ahora los cartuchos se cargan antes de sacar las balas.
- **El ruido es una decisión de juego:** disparar resuelve un problema pero atrae a otros; por eso la escopeta no reemplaza al machete.

**Cómo se probó (Play Mode, `EnemyArena`).**
- **Recarga:** con 6 balas, cargó 2 en 2 s (inventario 6 → 4). Durante la recarga no dispara; sin balas en el inventario no recarga.
- **Daño:** un blanco a 5 m bajó 30 y otro a 19 m bajó 15; un espíritu recibió 7.5 (el 25 % de 30).
- **Ruido:** tras los disparos, los dos cultistas a 15–16 m pasaron de patrullar a perseguir.
- **Cambio de arma:** Q enciende una arma y apaga la otra; con el inventario abierto ninguna dispara y al cerrarlo vuelve la escopeta.
- Consola sin errores.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Cómo atrae el disparo a los cultistas? | `Physics.OverlapSphereNonAlloc` busca colliders a 20 m; a cada `CultistAI` le llama `HearNoise(posición)` y él va a ese punto. |
| ¿Por qué el daño baja con la distancia? | Así lo marca la tabla de balance (30 a menos de 8 m, 15 más lejos): la escopeta es para distancias cortas. |
| ¿Cómo evitan que dispare al hacer clic en un menú? | El menú apaga `WeaponSwitcher`, y este apaga el arma en la mano. |

### 5.7 Rezo y ritual
*Rezo: issue #25. Ritual final: issue #34.*

**Qué hace el rezo.** Manteniendo R durante 3 segundos, el Padre reza: casi no puede moverse (va al 15 % de su velocidad) y una barra bajo la mira se llena. Si suelta R antes, se cancela sin castigo. Al completarse deja en el suelo un círculo de luz dorada de 4 m de radio que los espíritus no pueden cruzar durante 10 segundos; en el último segundo se desvanece. Después hay que esperar 20 segundos para volver a rezar: el HUD dice "Rezo listo [R]" o "Rezo en 13 s", y si aprietas R antes avisa "Aún no puedes rezar". El círculo no detiene a los cultistas: son humanos (GDD §5).

![Rezo](img/ui/rezo.png)

**Cómo funciona.**
| Pieza | Dónde | Qué hace |
|---|---|---|
| `PlayerPrayer` | Padre | Lee la acción `Pray` (R): mide cuánto se mantiene, frena al Padre con `PlayerController.SpeedMultiplier`, crea el círculo y lleva el enfriamiento. Avisa con eventos (`ProgressChanged`, `Completed`, `Notice`). |
| `PrayerCircle` | prefab `PF_PrayerCircle` | El anillo (`LineRenderer`) y su luz. Es un `ISpiritWard`: dice si un punto está dentro y a dónde empujarlo afuera. |
| `SpiritWards` | código | La lista de barreras activas. Un espíritu pregunta ahí si puede estar donde está. La línea de sal (#31) se sumará como otra barrera. |
| `SpiritBody` | cada espíritu | En cada cuadro pregunta a `SpiritWards`; si entró a una barrera, lo regresa al borde (`NavMeshAgent.Warp`) y avisa con `Repelled`. |
| `PlayerTuning` | `Data` | 3 s, 4 m, 10 s, 20 s y la velocidad al rezar: se ajustan sin tocar código. |

**Por qué así.** La barrera es una interfaz (`ISpiritWard`) para que el rezo, la línea de sal y lo que venga funcionen igual: el espíritu no necesita saber qué lo detuvo. Rezar cuesta (quedarse quieto y vulnerable) para que sea una decisión, no un botón para spamear.

**Cómo se probó (Play Mode, `EnemyArena`, con la tecla R real simulada).** A los 1.2 s: rezando, velocidad al 15 % y barra visible. A los 3.5 s: círculo creado, enfriamiento de 19 s y velocidad normal. El espíritu de prueba (botón "Aparecer espíritu de prueba") se quedó a 4.3 m del Padre mientras duró el círculo; al acabar los 10 s llegó hasta 0.7 m. R durante el enfriamiento no rezó y mostró "Aún no puedes rezar (14 s)". Consola sin errores.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Cómo sabe el espíritu que no puede entrar? | `SpiritBody` llama a `SpiritWards.TryRepel(posición)` en cada cuadro; si está dentro de una barrera, recibe un punto afuera y se mueve ahí. |
| ¿Por qué el círculo se queda donde rezó y no sigue al Padre? | Así es una zona segura que el jugador debe usar con cabeza; si lo siguiera, sería invencible 10 s. |
| ¿Qué pasa si sueltas R antes? | Se cancela: el Padre recupera su velocidad y no empieza el enfriamiento. |

**El ritual final** (*issue #34*, el mismo código para las 3 instancias; GDD §5, P3 y P6):
1. Se colocan los 3 objetos clave en el ancla (5.11).
2. Si el ancla tiene veladoras fijas, como la de la Llorona, se encienden con F (no gastan inventario). Avisos: "Enciende las 3 veladoras del ancla" y "Mantén R junto al ancla".
3. Junto al ancla (4 m), mantener R llena la barra del ritual, arriba en la pantalla, sin enfriamiento. Soltar R o alejarse la pausa. Ahí R es del ritual: el rezo normal no se activa (vuelve al alejarse).
4. Al empezar llegan las oleadas de cultistas (5.9). Un golpe de 15 o más hace retroceder la barra un 10 % ("¡El golpe te hizo perder el rezo!"). Los golpes del cultista de Xochimilco (10) no la mueven, pero un agarre de la Llorona sí.
5. Con la barra llena (60 s de rezo): "El ritual está completo. ¡Huye!", se detienen las oleadas y se desbloquea la trajinera de la huida. La entidad escucha `Completed` para debilitarse (#35, #45).
6. Si el Padre muere, el ritual empieza de cero: el ancla se vacía, las veladoras se apagan, la barra vuelve a 0 y desaparecen los cultistas de las oleadas.

| Pieza | Qué hace |
|---|---|
| `RitualController` | Las etapas (esperar ofrendas, encender veladoras, listo, rezando, completo), la barra, el retroceso con `Health.Damaged`, las oleadas (`WaveSpawner.Begin/Stop/ResetWaves`) y el reinicio con `PlayerRespawn.Respawned`. |
| `RitualCandle` | Cada veladora fija del ancla: F la enciende. |
| `PlayerPrayer.Suppressed` | Mientras el Padre está en el ancla durante el ritual, R no hace el rezo normal. |
| `TrajineraRide.Unlock` | La huida en trajinera espera al ritual. |

**Cómo se probó (Play Mode, `I1_Xochimilco`).**
- **Antes del ritual:** la trajinera de huida no avanzó. Con las ofrendas: "Enciende las 3 veladoras"; con las veladoras: listo.
- **Rezando:** con R mantenida (simulada con el Input System) empezó el ritual, la barra subió, salió la oleada 1 y el rezo normal quedó suprimido.
- **Golpes:** uno de 10 no movió la barra; uno de 20 la bajó un 10 %.
- **Pausa:** al soltar R la barra se quedó igual (0.041 por 2 s). Lejos del ancla no subió y el rezo normal volvió.
- **Muerte:** el ritual se reinició (ancla 0, veladoras apagadas, sin cultistas de oleada).
- **Final:** en un segundo intento se completó tras las 3 oleadas, la trajinera se desbloqueó y las oleadas se detuvieron.
- Consola sin errores.

### 5.8 Cuarto seguro, checkpoint y guardado
*Issue #26.*

**Qué hace.** Cada instancia tiene un cuarto seguro con puerta y altar. Al entrar, la puerta se cierra detrás del Padre y el juego guarda; al salir guarda otra vez, para conservar lo fabricado en el altar (GDD §26). Adentro no hay amenazas: los cultistas no lo ven ni lo oyen y los espíritus no pueden entrar. Si el Padre muere, reaparece en el último cuarto seguro con lo que tenía al guardarse y con al menos 60 de vida (o más, si se guardó con más). Desde #47 lo trae de vuelta la pantalla de derrota (Reintentar); antes reaparecía solo a los 3 segundos.

**Qué se guarda** (un archivo `partida.json` en `Application.persistentDataPath`): escena, sección, cuarto seguro, vida, los 8 espacios del inventario, los materiales y las recetas conocidas; ya hay lugar para las notas y los objetos clave (#32).

**Cómo funciona.**
| Pieza | Dónde | Qué hace |
|---|---|---|
| `SafeRoom` | objeto con una caja (`BoxCollider`) que cubre el interior | Cada cuadro revisa si el Padre está adentro. Al cambiar (entrar o salir) cierra la puerta y guarda. Es un `ISpiritWard` (los espíritus no entran) y expone `PlayerInside` para que los cultistas lo ignoren. |
| `SaveData` | código | Los datos que se guardan, en una forma que `JsonUtility` puede escribir. |
| `SaveSystem` | código | Escribe y lee el JSON. Escribe primero a un archivo temporal y luego lo cambia, para no romper la partida si el juego se cierra a la mitad; si el archivo está dañado, avisa y no truena. |
| `ItemDatabase` | `Data/ItemDatabase.asset` | Lista de todos los objetos y recetas: el archivo guarda nombres ("ITM_Venda") y esta lista los convierte de nuevo en objetos. |
| `PlayerPersistence` | Padre | `Capture` toma el estado del Padre; `Restore` lo vuelve a poner (`Inventory.SetContents`, `RecipeBook.SetKnown`, `Health.Revive`). |
| `PlayerRespawn` | Padre | Al morir: lleva al Padre al punto de reaparición del último cuarto seguro, restaura el guardado y le devuelve los controles (`PlayerDeathHandler.Revive`). |

**Por qué así.**
- **Revisar la posición en vez de usar triggers:** al reaparecer el Padre se teletransporta, y Unity no siempre avisa la salida de un trigger en ese caso. Revisar si está dentro de la caja nunca falla.
- **JSON en un archivo:** es legible (se puede abrir y revisar) y Unity lo trae incluido (`JsonUtility`).
- **Nombres en vez de referencias:** un archivo no puede guardar un objeto de Unity; guarda su nombre y la base de datos lo encuentra.

**Cómo se probó (Play Mode, `EnemyArena`; el cuarto de inicio ahora es un cuarto seguro con puerta).**
1. Al empezar, el Padre está adentro: la puerta se cerró y se guardó.
2. Recibió objetos de prueba y 10 de daño, y salió: se guardó con vida 90, 4 objetos, 13 materiales y 6 recetas.
3. Tiró objetos, gastó sal y murió afuera.
4. A los 3 s reapareció en el cuarto con vida 90 y exactamente el inventario guardado: vendas, agua bendita, balas y 20 de sal. La cámara, el arma y el cursor bloqueado volvieron, y la puerta se cerró.
5. Un cultista frente a la puerta abierta no detectó al Padre adentro; a 3.5 m, ya afuera, sí lo vio y atacó.
6. Un espíritu de prueba se quedó afuera (z −6.8) con la puerta abierta.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Dónde se guarda la partida? | En `partida.json`, dentro de la carpeta de datos del juego (`Application.persistentDataPath`), en formato JSON. |
| ¿Por qué se reaparece con 60 de vida? | Lo pide el GDD: castiga morir sin dejar al jugador sin opciones; si se guardó con más vida, se respeta. |
| ¿Qué pasa si se borra un objeto del juego y había uno guardado? | `PlayerPersistence` lo salta y deja un aviso en la consola; la partida carga igual. |

### 5.9 Encuentros, oleadas y dificultad
*Issue #27.*

**Qué hace.** Los cultistas no están todos desde el inicio: cada sección tiene zonas de encuentro. Cuando el Padre entra a una, aparece un grupo (2–3 en Xochimilco, 3–4 en la hacienda, 4–5 en el convento) que patrulla o embosca. Pasa una sola vez y nunca dentro del cuarto seguro. En el ritual llegan oleadas (3 de 3 en Xochimilco, 4 de 4 en el convento): cada una sale cuando se vence la anterior o, a lo más, 25 s después. Cada instancia es más difícil que la anterior: cultistas más rápidos, que pegan más fuerte y ven más lejos. Al morir, un cultista puede soltar materiales (sal, trapo, tela, casquillos, balas). La tabla completa está en el GDD §20.

**Cómo funciona.**
| Pieza | Dónde | Qué hace |
|---|---|---|
| `InstanceDifficulty` | `Data/Difficulty/DIF_Instancia1..3` | Cuántos cultistas por encuentro, su `CultistTuning`, las oleadas y el botín. Una instancia más difícil es otro archivo con otros números. |
| `CultistTuning_I2`, `_I3` | `Data/Enemies` | Copias del ajuste del cultista con más velocidad, daño, vista y menos espera. |
| `CultistSpawner` | código | Crea un cultista con el ajuste de la instancia, su ruta, alerta si es emboscada y, al morir, tira el botín (`PickupItem`). |
| `Encounter` | zona con `BoxCollider` en el nivel | Cuando el Padre entra (revisando su posición), aparece el grupo una vez. Avisa con `Started` y `Cleared`. |
| `WaveSpawner` | objeto en la zona del ritual | `Begin`, `Stop` y los eventos `WaveStarted` y `AllWavesDefeated`, para el ritual (#34). |
| `CultistAI.SetTuning` | Cultista | Cambia su ajuste al aparecer. Un cultista que nace persiguiendo ya no se reinicia a patrullar. |

**Por qué así.** Los encuentros por guion (no aleatorios) hacen que cada partida sea justa y que el nivel se pueda diseñar: el GDD deja el "Director de ritmo" como deseable. La dificultad vive en datos para ajustarla en los playtests.

**Cómo se probó (Play Mode, `EnemyArena`, que tiene una zona de encuentro en el centro y un botón "Iniciar oleadas del ritual").**
- **Encuentro:** al entrar a la zona aparecieron 2 cultistas (un tiro de 2–3). Al vencerlos aparecieron 2 objetos de botín, y al volver a entrar no salieron más.
- **Oleadas:** con `Begin`, a los 2 s salió la oleada 1 (3 cultistas persiguiendo). Al vencerla salieron la 2 y la 3; al vencer la 3 terminaron (`Finished`).
- **Instancia 3:** un cultista creado con su dificultad usó `CultistTuning_I3` (daño 14, persecución 3.9 m/s) y siguió persiguiendo después de su `Start`.
- Consola sin errores.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Cómo se nota que cada instancia es más difícil? | Más cultistas por grupo, más rápidos, más daño y más oleadas (tabla del GDD §20). |
| ¿Por qué el encuentro no se repite? | Para que el jugador avance; si muere, reaparece en el cuarto seguro y lo que ya limpió no vuelve. |
| ¿Por qué las oleadas no esperan siempre a que mueras a todos? | Para que la tensión no baje: si el jugador se esconde, a los 25 s llega la siguiente. |

### 5.10 Objetos de fe y sus efectos
*Issue #31.*

**Qué hace cada objeto** (se usan con las teclas 1–4, GDD §14 y §18):
| Objeto | Cómo se usa | Efecto | Contra |
|---|---|---|---|
| Agua bendita | Se lanza hacia donde miras | Al romperse: 20 de daño a los espíritus en 3 m y los detiene 3 s. No daña humanos. | Espíritus |
| Molotov de mezcal | Se lanza | Fuego de 3 m: 15 de daño por segundo durante 5 s. No quema al Padre. | Cultistas |
| Línea de sal | Se pone en el suelo, de lado a lado frente al Padre | Durante 20 s ningún espíritu la cruza | Espíritus |
| Veladora | Se pone en el suelo | Luz durante 45 s; delata (revela) a los espíritus a 8 m | Utilidad |
| Copal | Se pone en el suelo | Humo que revela a los espíritus a 7 m durante 15 s (la Tzitzimime, fase 2) | Tzitzimime |
| Chancla bendita | Se equipa en lugar del machete | 15 a humanos y 30 a espíritus | Espíritus |
| Cruz de plata | Se equipa | Golpe normal de 10; manteniendo clic 1 s sale el golpe cargado (40 a espíritus), con barra "¡Cruz cargada!" | Charro Negro |

Usar otra vez la chancla o la cruz regresa el machete; si se tira, también. Las cantidades que el GDD no fija están en `docs/PENDIENTES_DISENO.md`.

**Cómo funciona.**
| Pieza | Qué hace |
|---|---|
| `ItemEffect` | Clase base (ScriptableObject) de "qué pasa al usar". Cada objeto apunta a uno en `Data/Items/Effects`. `Inventory.UseSlot` lo llama y gasta el objeto si el efecto lo pide (las armas no se gastan). |
| `ThrowEffect` + `ThrownFlask` | Lanzan un frasco con física (`Rigidbody`) que ignora el cuerpo del Padre; al chocar deja su efecto en el suelo. |
| `HolyWaterSplash` / `FireArea` | La salpicadura y el fuego: buscan vidas cercanas con `Physics.OverlapSphere` y les quitan vida. El fuego aplica el daño en pasos de 0.25 s. |
| `PlaceEffect` | Pone un objeto en el suelo frente al Padre, mirando hacia donde él mira; si no hay piso, no lo gasta. |
| `SaltLine` | Es un `ISpiritWard` como el círculo del rezo: si un espíritu entra a la franja, lo regresa del lado de donde venía. |
| `RevealArea` | Veladora y copal: cada medio segundo marca como revelados a los espíritus cercanos (`SpiritBody.Reveal`). |
| `EquipMeleeEffect` + `ItemUser` | Cambian el arma cuerpo a cuerpo (`PlayerCombat.SetWeapon`) y regresan al machete cuando toca. |
| `PlayerCombat` (cambio) | Armas con carga: mantener clic llena la barra y al soltar sale el golpe cargado; avisa con `ChargedHitLanded` (el Charro, #39, solo cuenta esos). |
| `SpiritBody` (cambio) | `Stun` (lo detiene) y `Reveal` (lo hace visible), con eventos para las IA de las entidades. |

**Por qué así.** Con un efecto por archivo, el inventario no tiene un `if` por cada objeto: el agua bendita y el molotov usan el mismo código de lanzar con distinto frasco. Y la línea de sal reutiliza las barreras del rezo, así que cualquier espíritu nuevo las respeta sin programar nada más.

**Cómo se probó (Play Mode, `EnemyArena`).**
- **Agua bendita:** un espíritu bajó de 60 a 40 y quedó detenido; un cultista junto a él siguió con 40.
- **Molotov:** un cultista en el fuego murió en 3 s (40 de vida); el Padre siguió con 100.
- **Línea de sal:** un espíritu que corría hacia el Padre se quedó del otro lado (a 0.6 m de la línea).
- **Veladora y copal:** revelaron a los espíritus cercanos.
- **Chancla:** un golpe le quitó 30 a un espíritu.
- **Cruz:** con el clic mantenido (simulado con el Input System), la barra mostró "¡Cruz cargada!" y al soltar el golpe cargado mató al espíritu (30 → 0). Al tirar la cruz volvió el machete.
- Consola sin errores.

### 5.11 Objetos clave, notas y recetas desbloqueables
*Issue #32.*

**Qué hace.** Cada instancia tiene 3 objetos clave (muñecas ofrenda, monedas del pacto o fragmentos de obsidiana). Se toman con F y no ocupan los 8 espacios: van aparte, como los materiales, para que el jugador nunca tenga que tirar una venda por una ofrenda. Al tomar uno aparece "Muñeca ofrenda (2/3)", y el inventario los lista. En el ancla, F coloca todos los que traigas; con los 3 aparece "Las ofrendas están completas" y empieza el ritual (#34). Las notas se leen con F en un papel en pantalla. La primera vez que lees una que enseña algo, aprendes su receta: "Receta nueva en el altar: cruz de plata".

![Nota](img/ui/nota.png)

**Si el Padre muere** (GDD §6), todo vuelve al último guardado sin que el nivel quede trabado:
- vuelven a aparecer en el mundo solo los objetos clave que tomó después de guardar;
- el ancla se vacía (el ritual se repite);
- las notas leídas y las recetas guardadas se conservan.

**Cómo funciona.**
| Pieza | Dónde | Qué hace |
|---|---|---|
| `KeyItemBag` | Padre | Cuántos objetos clave lleva, cuáles del mundo ya tomó (por un id único, p. ej. `I1_Muneca3`) y qué notas leyó. Todo eso entra al guardado. |
| `KeyItemPickup` | cada objeto clave | No se destruye al tomarlo: se oculta mientras su id esté en la bolsa y reaparece si un guardado anterior ya no lo tiene. |
| `NoteDefinition` | `Data/Notes/NOTE_*` | Título, texto y la receta que enseña (si enseña una). |
| `NotePickup` + `NoteUI` | nota en el mundo + `PF_HUD` | Abre el papel en pantalla (la cámara y los controles se pausan, el juego no) y desbloquea la receta en el `RecipeBook`. |
| `Anchor` | ancla de la entidad | Recibe las ofrendas, muestra los modelos colocados y lanza `OfferingsComplete`. Se vacía con `PlayerRespawn.Respawned`. |
| `PlayerPersistence` (cambio) | Padre | Guarda y restaura objetos clave, ids tomados y notas leídas (`ItemDatabase` ahora también lista las notas). |

**Las notas de ahora** (textos cortos que #33 puede ampliar): en Xochimilco, 3 notas que dicen dónde están las ofrendas, que la sal detiene a la Llorona y para qué son las veladoras del ancla. Además, la que deja la Llorona enseña la cruz de plata, la de la hacienda la chancla bendita y la del convento el copal.

**Cómo se probó (Play Mode, `I1_Xochimilco`).**
1. Leer la nota abrió el papel y desactivó la cámara; la nota de la Llorona enseñó la cruz de plata y avisó.
2. Se tomaron las muñecas 1 y 2 (desaparecieron, "1/3" y "2/3") y se entró a la choza: se guardaron 2 muñecas, 2 ids y 1 nota.
3. Al salir se tomó la 3 y el Padre murió. Reapareció con 2 muñecas: la 3 volvió a su lugar y la 1 y la 2 no.
4. Retomó la 3 y en el ancla colocó las 3 (completa, evento lanzado, modelos visibles).
5. Al morir ahí, el ancla se vació y la muñeca 3 volvió al mundo para retomarla.

**Un error que se encontró en la prueba:** el cuarto seguro podía guardar en el mismo cuadro en que el Padre moría (teletransporte). Ahora `SafeRoom` nunca guarda a un Padre muerto.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Por qué los objetos clave no ocupan espacio? | Son de misión: si ocuparan espacio, el jugador podría tirarlos o quedarse sin lugar para curarse, y eso no aporta decisión, solo frustración. |
| ¿Cómo evitan que el nivel quede sin la tercera muñeca si mueres? | Cada muñeca tiene un id; al cargar, las que no están en la bolsa guardada vuelven a aparecer. |
| ¿Cómo se desbloquea una receta? | `NotePickup` llama a `RecipeBook.Unlock` la primera vez que se lee la nota; el altar muestra la receta desde ese momento. |

### 5.12 Entidades: La Llorona, el Charro Negro y la Tzitzimime
*La Llorona: issue #35. El Charro Negro: #39. La Tzitzimime: #45.*

#### La Llorona (instancia 1)

**Qué hace.** La Llorona ronda los canales de Xochimilco. De lejos se la ve flotando sobre el agua. Cuando se acerca, la pantalla se deforma y pierde color, y aparece el subtítulo "(Se oye un llanto) ¡Ay, mis hijos!". Si el Padre pasa a menos de 12 m, lo persigue. Si lo alcanza, lo agarra 2 s: no puede moverse, pierde 15 de vida de golpe y luego 4 por segundo. Después ella se hunde en el agua y vuelve a salir 12 s más tarde, lejos de él.

Las armas no la matan (GDD §13: "se vence con crafteo y rezo"). Lo que sí funciona:
- la **línea de sal** y el **círculo de rezo**: no los puede cruzar;
- la luz de la **veladora**;
- el **agua bendita** o un golpe bendito (chancla, cruz).

Con cualquiera de ellos grita y se hunde. Solo se vence **completando el ritual** en el ancla. Durante el ritual siempre sabe dónde está el Padre, y cada agarre hace retroceder la barra un 10 %, porque el primer daño es de 15 (GDD P3). Al llenarse la barra se disuelve: se hunde y se encoge. Deja junto al ancla la nota que enseña la cruz de plata.

![La Llorona cerca](img/ui/llorona_presencia.png)

**Cómo funciona.** Es una máquina de estados, igual que el Cultista:

`Hidden` (bajo el agua) → `Stalk` (recorre los canales) → `Chase` (lo persigue) → `Grab` (lo agarra) → `Retreat` (vuelve al canal) → `Hidden`. Al final del ritual pasa a `Dissolved`.

| Pieza | Dónde | Qué hace |
|---|---|---|
| `LloronaEntity` | `PF_Llorona` | La máquina de estados. Escucha a `SpiritBody` (sal, rezo, agua bendita, veladora), a `Health` (golpes), a `RitualController.Completed` y a `PlayerRespawn.Respawned`. |
| `LloronaTuning` | `Data/Enemies/LloronaTuning.asset` | Todos sus números: velocidades (1.6 al acechar, 3.4 al perseguir), distancias (12 m para verlo, 22 m para perderlo), daño del agarre y tiempo bajo el agua. |
| `EntityPresence` | `PF_Llorona` | Mientras más cerca está, más peso le da a un Volume de posprocesado (`Art/VFX/VP_LloronaPresencia`: distorsión de lente, aberración cromática, viñeta y menos color). También sube el volumen de su llanto y muestra el subtítulo. Sirve igual para las otras entidades. |
| `Llorona_Canales` | `I1_Xochimilco` | 12 puntos sobre el agua. Va al que esté más cerca del Padre y reaparece en el más cercano que esté al menos a 15 m. |
| `Nota_Llorona` | junto al ancla | Empieza oculta; se muestra cuando ella se disuelve. |

Es un fantasma: se desliza en línea recta sobre el agua y la tierra, sin NavMesh. Así puede "recorrer los canales", donde el Padre no puede entrar. La choza es un cuarto seguro: no la deja entrar, y con el Padre adentro ella no sale del agua.

Para que la pantalla se deforme, la cámara del Padre ahora tiene activado el posprocesado (`PF_Player`).

**Cómo se probó (Play Mode, `I1_Xochimilco`).**
- Con el Padre en el embarcadero, ella salió del agua a 20 m. Se acercó por el canal y lo persiguió al estar a menos de 12 m. Lo agarró: la vida bajó de 100 a 77 (15 de golpe y el resto drenado) y la velocidad del Padre quedó en 0 durante el agarre. Luego se hundió.
- El peso de la distorsión subió de 0 a 0.93 mientras se acercaba y volvió a 0 al hundirse.
- Con el rezo, con el agua bendita y con una veladora colocada, en cada caso se hundió al instante.
- Al completar el ritual se disolvió (bajó 2 m y se ocultó), la distorsión volvió a 0 y apareció su nota.
- Consola sin errores.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Por qué no usa NavMesh como el Cultista? | Flota sobre el agua, y el NavMesh solo cubre la tierra. Moverla en línea recta es más simple y queda con la leyenda. |
| ¿Cómo sabe el ritual que el agarre es un golpe fuerte? | El agarre quita 15 de un solo golpe; `RitualController` escucha `Health.Damaged` y retrocede la barra con cualquier golpe de 15 o más. |
| ¿Por qué no se le puede matar? | El GDD dice que las entidades se vencen con crafteo y rezo. Si recibe daño, se cura y huye. |
| ¿Cómo se ve que está cerca? | `EntityPresence` cambia el peso de un Volume de posprocesado según la distancia, y muestra un subtítulo por si no hay sonido. |

#### El Charro Negro (instancia 2)

**Qué hace.** El Charro trota detrás del Padre a 5 m/s, un poco más lento que correr (5.5). Cuando el Padre está a menos de 14 m, embiste: corre en línea recta a 12 m/s hacia donde estaba el Padre y sigue 8 m más. La embestida no gira, así que hacerse a un lado en el último momento la esquiva. Si lo alcanza, quita 25. Después se detiene 1.5 s para dar la vuelta.

- **La capilla:** no puede entrar (los cultistas sí); espera en la puerta.
- **Fase 1:** las armas no le hacen nada, porque no tiene barra de vida que vaciar.
- **Fase final (GDD P2 y P8):** empieza cuando el Padre coloca las 3 monedas del pacto en el pedestal de la capilla. Entonces las 3 canaletas frente a la capilla aceptan agua bendita: con F y 1 agua bendita se llenan, brillan y duran 20 s. Si el Charro embiste y cruza una canaleta llena, queda débil 4 s: se detiene, brilla y aparece "¡El Charro está débil! Golpéalo con la cruz cargada".
- **Cómo se vence:** un golpe cargado de la cruz de plata mientras está débil cuenta como 1 de 3. Con el tercero se disuelve, el portón se abre ("¡Corre al portón!") y el Padre huye a pie hasta la salida.
- **Si el Padre muere:** vuelve la fase 1. Las monedas regresan al mundo, los golpes se reinician y las canaletas se vacían.

![Charro débil sobre la canaleta](img/ui/charro_debil.png)

**Cómo funciona.** También es una máquina de estados:

`Chase` (lo persigue) → `Charge` (embiste) → `Recover` (se detiene) → `Chase`. Si cruza el agua bendita: `Weakened` (débil). Con 3 golpes: `Dissolved`. Si el Padre está muerto o en el cuarto seguro: `Idle` (vuelve a su sitio).

| Pieza | Dónde | Qué hace |
|---|---|---|
| `CharroNegroEntity` | `PF_Charro` | La máquina de estados. Usa NavMeshAgent (va a caballo por tierra). Escucha `PlayerCombat.ChargedHitLanded` (solo la cruz se carga), `Anchor.OfferingsComplete` del pedestal y `PlayerRespawn.Respawned`. |
| `CharroTuning` | `Data/Enemies/CharroTuning.asset` | Velocidades, distancia de embestida, daño, segundos débil y golpes necesarios. |
| `HolyWaterChannel` | cada canaleta | F + 1 agua bendita la llena 20 s (el brillo se apaga en los últimos 3 s). `Crossed` revisa si el Charro pasó por encima en este cuadro. |
| `SpiritBarrier` | capilla | Zona que los espíritus no pueden pisar; a la gente no la afecta. Es otra "barrera" como el círculo de rezo y la sal, así que `SpiritBody` lo saca sin código nuevo. |
| `Anchor` (reutilizado) | pedestal | El mismo script del ancla de la Llorona, pero pidiendo monedas del pacto. |
| `Door` (cambio) | portón | Ahora puede estar con candado (`locked`): F dice "Cerrado" hasta que el Charro se disuelve. |

**Escena de prueba:** `Scenes/Test/CharroArena`. Tiene un campo de 70 × 70 m con:
- la capilla y su pedestal;
- 3 canaletas;
- 3 monedas;
- el cuarto seguro;
- el portón y la salida;
- agua bendita y cruz junto al inicio.

El nivel real de la hacienda es #40.

**Cómo se probó (Play Mode, `CharroArena`).**
- Persiguió al Padre a ~5 m/s. Al estar cerca, embistió y le quitó 25.
- Con el Padre dentro de la capilla, se quedó parado en la puerta (0, 14) y no entró.
- Antes de las monedas, la canaleta respondió "Coloca primero las monedas…" y no gastó agua bendita. Después, se llenó y gastó 1.
- Al cruzar la canaleta llena quedó débil. Tres golpes cargados con la cruz (con el ataque real de `PlayerCombat`) lo disolvieron: el portón se abrió y la salida marcó `Done`.
- Si se quedaba parado sobre la canaleta, la siguiente embestida ya no lo debilitaba: cada golpe pide un cruce nuevo.
- Al morir con 1 golpe dado: golpes = 0, pedestal vacío, canaletas cerradas.
- Consola sin errores.

**Errores que se encontraron en la prueba.**
- Con el Editor en segundo plano (pocos cuadros por segundo), el caballo a 12 m/s "saltaba" la canaleta de 1 m sin tocarla. Ahora `Crossed` revisa todo el tramo que avanzó en el cuadro, no solo dónde terminó.
- Tras un golpe, el Charro quedaba sobre el agua y su siguiente embestida lo debilitaba al instante, así que los 3 golpes salían seguidos. Ahora solo cuenta una embestida que empieza fuera de la canaleta.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Por qué el Charro usa NavMesh y la Llorona no? | Él va a caballo por tierra y debe rodear la capilla y los muros; ella flota sobre el agua. |
| ¿Cómo se sabe que el golpe fue con la cruz cargada? | `PlayerCombat` lanza `ChargedHitLanded` solo con golpes cargados, y la cruz es la única arma que carga. El Charro además revisa que esté débil. |
| ¿Por qué no entra a la capilla? | La capilla tiene un `SpiritBarrier`; `SpiritBody` lo saca de cualquier barrera, igual que del círculo de rezo. |
| ¿Cómo se puede esquivar la embestida? | Apunta a donde estaba el Padre y no gira durante la carrera; basta con moverse de lado. |

#### La Tzitzimime (instancia 3, jefe final)

**Qué hace.** Sigue la barra del mismo ritual de la Llorona (`RitualController`), en tres fases:

1. **Fase 1:** se colocan los 3 fragmentos de obsidiana en el ancla y se reza con R. Solo llegan oleadas de cultistas (tabla de la instancia 3: 4 oleadas de 4).
2. **Fase 2, a la mitad de la barra:** aparece colgada del techo de la catacumba, invisible ("Algo se arrastra por el techo… Revélala con copal"). Sigue al Padre por arriba y cada 7 s ataca un área: primero aparece un círculo rojo en el piso y 1.5 s después golpea con 20. Si el Padre sale del círculo a tiempo, no le pasa nada; si no, el golpe hace retroceder la barra.
   - La barra **no avanza** mientras ella es invisible ("La barra no avanza: usa copal para revelarla").
   - Con el humo del copal queda revelada 15 s y la barra avanza normal.
   - Si además la alumbra con la linterna, avanza al doble: "luz y rezo" (GDD P9).
3. **Fase 3, barra llena:** baja al piso en su forma final, con brillo rojo, y camina tras el Padre atacando cada 4 s mientras él huye a la salida. Llegar a la salida termina la campaña y muestra la victoria final (#48).

Las armas no la dañan. Si el Padre muere, ella vuelve a dormir y el ritual empieza de nuevo.

![Tzitzimime revelada en el techo](img/ui/tzitzimime_revelada.png)

**Cómo funciona.**
| Pieza | Dónde | Qué hace |
|---|---|---|
| `TzitzimimeEntity` | `PF_Tzitzimime` | Estados `Dormant` → `Ceiling` → `FinalForm`. Lee el avance del ritual para pasar a la fase 2 y le dice a la barra qué tan rápido llenarse. Escucha `RitualController.Completed` y `PlayerRespawn.Respawned`. |
| `TzitzimimeTuning` | `Data/Enemies/TzitzimimeTuning.asset` | Dónde empieza la fase 2 (50 %), velocidades de llenado, altura del techo, ataques (cada 7 s / 4 s, 1.5 s de aviso, 3 m, 20 de daño) y velocidad en la huida. |
| `RitualController.FillMultiplier` (cambio) | ritual | Multiplica la velocidad de la barra. Vale 1 normalmente; la Tzitzimime lo pone en 0, 1 o 2. |
| `Flashlight.Illuminates` (nuevo) | linterna | Dice si un objeto está dentro del haz: distancia, ángulo del cono y que no haya pared en medio. |
| Copal (ya existía, #31) | `PF_HumoCopal` | Revela espíritus 15 s (`SpiritBody.Reveal`). |

Mientras está en el techo cuelga de cabeza: el modelo se gira 180°.

**Escena de prueba:** `Scenes/Test/TzitzimimeArena`. Es una catacumba de 24 × 20 m con techo a 5 m, antorchas, el ancla y las oleadas. También tiene:
- 3 fragmentos de obsidiana, copal y agua bendita;
- el cuarto seguro;
- la salida.

El nivel real del convento es #46.

**Cómo se probó (Play Mode, `TzitzimimeArena`).**
- **Fase 1:** con las ofrendas colocadas y R mantenida (simulada), la barra llegó a 0.5 y apareció en el techo.
- **Barra detenida:** sin copal, la barra se quedó fija en 0.51.
- **Ataque:** el círculo rojo apareció sobre el Padre; él no se movió, perdió 20 y la barra bajó a 0.41. En otro ataque se movió 3.6 m durante el aviso y no perdió vida.
- **Copal y linterna:** con copal el multiplicador pasó a 1; con la linterna apuntándole, a 2; al apagarla volvió a 1.
- **Fase 3:** con la barra llena bajó al piso y siguió al Padre (atacándolo una vez) hasta la salida, que marcó `Done`.
- **Muerte:** al morir en la fase 2 volvió a `Dormant`, con la barra en 0 y el multiplicador en 1.
- Consola sin errores. Las oleadas se apagaron en esta prueba porque ya se probaron en #34.

**Errores que se encontraron en la prueba.**
- De pie en el techo, desde abajo solo se le veían los pies. Ahora cuelga de cabeza.
- La linterna nunca "la alumbraba": el rayo para revisar paredes chocaba con su propio cuerpo. Ahora `Illuminates` acepta que lo primero que toque el rayo sea ella misma.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Cómo se "daña" con luz y rezo si no tiene vida? | Su "vida" es la barra del ritual: el rezo la llena, la luz la llena más rápido, y sin copal no se llena. |
| ¿Por qué no se reutilizó el código de la Llorona? | Comparten piezas (`SpiritBody`, `RitualController`, `EntityPresence`), pero cada entidad tiene fases distintas según el GDD. Un script por entidad es más fácil de explicar que uno lleno de condiciones. |
| ¿Cómo se esquiva el ataque? | El círculo rojo avisa 1.5 s antes; basta con salir de él. |

### 5.13 Niveles: las tres instancias
*Planos: issues #5, #30 y #38. Blockout de la instancia 1: #16. Niveles finales: #36, #40 y #46.*

**Qué hay.** Un plano por instancia, visto desde arriba, con el recorrido numerado en orden, los 3 objetos clave, el inicio y la salida, el altar del cuarto seguro, los grupos de cultistas (con cuántos son), las notas y los alijos secretos, el ancla de la entidad, por dónde ronda la entidad, las luces y las zonas oscuras, y la escala (10 m y la altura del Padre, 1.8 m). Todos siguen la misma estructura del GDD: **sección 1 → cuarto seguro → sección 2 → final de la entidad → huida**.

![Instancia 1](img/niveles/instancia1_xochimilco.png)
![Instancia 2](img/niveles/instancia2_hacienda.png)
![Instancia 3](img/niveles/instancia3_convento.png)

**Resumen de cada una.**
| Instancia | Sección 1 | Cuarto seguro | Sección 2 y final | Huida |
|---|---|---|---|---|
| 1. Xochimilco | Embarcadero y cruce en trajinera por guion; 2 muñecas | Choza del cuidador | La isla con la Llorona rondando los canales; 3.ª muñeca; ritual en el ancla con 3 veladoras (3 oleadas de 3) | En trajinera, por guion |
| 2. Hacienda | Trojes y casa grande; 2 monedas | Tienda de raya | Campo abierto con el Charro a caballo y panteón (3.ª moneda); capilla con pedestal, pila y 3 canaletas | A pie por el camino hasta el portón |
| 3. Ex-convento | Templo, coro alto (+5 m) y claustro; 2 fragmentos | Celda del prior | Criptas (−4 m) a oscuras (3.er fragmento) y catacumbas (−9 m) con el ritual (4 oleadas de 4) | Túnel que sube a la barranca |

**Cómo se hicieron.** Con el script `Tools/Planos/dibujar_planos.py`, que dibuja cada plano a partir de coordenadas en metros (x al este, y al norte). Así, cambiar el nivel es cambiar unos números y volver a correr el script, y el blockout en Unity copia las mismas coordenadas (1 unidad = 1 m). La leyenda calcula sola cuánto se camina.

**Por qué así.**
- **Recorrido en orden y legible sin explicación** (criterio del issue #5): números grandes en el mapa y la descripción de cada paso al lado.
- **Objetos clave a la vista:** el jugador no debe perderse buscando; el reto son los cultistas, la oscuridad y la entidad.
- **Dificultad creciente:** grupos de 2–3 cultistas en la instancia 1, de 3–4 en la 2 y de 4–5 en la 3, como dice la tabla de balance del GDD.
- **La entidad marca el ritmo de la sección 2:** la Llorona en los canales, el Charro en el campo abierto (no entra a la capilla) y la Tzitzimime en las catacumbas.

**Blockout de la instancia 1** (*issue #16*). La escena `Scenes/Levels/I1_Xochimilco` arma el plano con formas simples y las mismas coordenadas:
- las chinampas son cajas y el agua no tiene colisión;
- barandales invisibles impiden caer a los canales;
- la choza es el cuarto seguro, con dos puertas, altar y veladoras;
- hay marcadores para las 3 muñecas, las notas y el ancla con sus 3 veladoras;
- los alijos son objetos reales que se pueden recoger;
- las 4 zonas de encuentro (G1 a G4) usan `DIF_Instancia1`, y las oleadas del ritual salen desde la orilla de la isla;
- hay dos trajineras (el cruce y la huida) y la salida;
- la noche se arma con luz de luna fría, niebla blanca y fondo oscuro.

![Blockout: la choza](img/niveles/i1_blockout_choza.png)

| Pieza nueva | Qué hace |
|---|---|
| `TrajineraRide` | El viaje por guion (GDD P1): F para subir; la trajinera sigue su ruta sola y deja al Padre en la otra orilla. Arriba no puede caminar, pero sí mirar, atacar y usar objetos rápidos. La de la huida se puede bloquear hasta que termine el ritual (#34). Si el Padre muere arriba, la trajinera vuelve a su muelle. |
| `LevelExit` | Cuando el Padre llega vivo a la salida, avisa "Escapaste de Xochimilco" y lanza `Reached`. La pantalla de victoria (#48) guarda y muestra el resumen. |
| `SafeRoom` (cambio) | Ahora acepta varias puertas y cierra todas al entrar. |
| `PathWalker` (pruebas) | Camina al Padre por el NavMesh moviendo su `CharacterController`, como un jugador, para probar recorridos completos de forma automática. |

**Un detalle del NavMesh:** se hornea desde los colliders (no desde las mallas visibles); si no, el agua contaba como piso. Las zonas invisibles (encuentros, cuarto seguro, salida) se excluyen del horneado porque tapaban el puente.

**Cómo se probó (Play Mode, recorrido automático a velocidad ×3).** El Padre caminó del inicio al muelle (38 m; se activó el encuentro G1) y subió a la trajinera. Llegó a la chinampa de las muñecas, caminó a la choza y al entrar se guardó en `I1_Choza` con las puertas cerradas. Salió por la puerta este, cruzó el puente a la isla hasta el muelle norte y subió a la trajinera de huida. Llegó a la salida, que guardó `salida`. Sin la trajinera, el NavMesh no une el embarcadero con la chinampa (correcto: ahí se cruza en trajinera). Consola sin errores.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Cómo saben que dura unos 10 minutos? | Cada plano camina entre 150 y 470 m; a 3 m/s eso son 1 a 3 minutos de puro caminar, y el resto se va en explorar, pelear, craftear y el final. Se confirmará en los playtests (#21). |
| ¿Dónde se guarda el progreso? | Al entrar y al salir del cuarto seguro de cada instancia; la huida no tiene checkpoint (GDD §5). |
| ¿Son definitivos? | Son la base para el blockout; si un playtest muestra que algo es muy largo o difícil, se mueven los números en el script y se redibuja. |

### 5.14 UI: HUD, menús, pausa, derrota, victoria y créditos
*HUD e inventario: issue #22. Menús, pausa, derrota y créditos: #47. Victoria y flujo entre instancias: #48.*

**Qué hace el HUD.** Siguiendo el GDD (sección 16): abajo a la izquierda, la barra de vida (roja, con el número) y la de resistencia (dorada); abajo al centro, los 4 espacios rápidos con su tecla (1–4) y cuántos hay. Con poca vida (30 o menos) la barra parpadea; cuando el Padre se agota, la resistencia se pone gris hasta que puede volver a correr. Arriba al centro aparece la barra del ritual solo durante el ritual, y abajo, los subtítulos de lo que dice el Padre. El punto del centro y el "[F] …" ya existían (5.1).

Con **Tab** se abre el inventario: los 8 espacios (los 4 primeros son los rápidos), el nombre y la descripción del objeto elegido, los botones **Usar** y **Tirar**, y la lista de materiales. Mientras está abierto, el mouse queda libre y el Padre no puede mirar, atacar ni interactuar; el juego **no se pausa**, así que abrirlo en plena pelea es arriesgado (la pausa es Esc, #47).

![HUD](img/ui/hud.png)
![Inventario](img/ui/inventario.png)

**Cómo funciona.**
| Script | Dónde | Qué hace |
|---|---|---|
| `HUDController` | prefab `PF_HUD` | Escucha `Health.Changed`, `PlayerStamina.Changed` e `Inventory.Changed` y redibuja las barras y los espacios rápidos solo cuando algo cambia. |
| `StatBar` | cada barra | Calcula vida actual ÷ vida máxima y ajusta el relleno de la imagen (`fillAmount`). Si el máximo es 0, no divide. |
| `ItemSlotView` | cada espacio | Muestra el icono del objeto (o su nombre mientras no haya iconos), la cantidad y la tecla. |
| `InventoryUI` | prefab `PF_HUD` | Abre y cierra con la acción `Inventory` (Tab); al abrir apaga `PlayerLook`, `PlayerCombat` e `Interactor`, y al cerrar los vuelve a encender. Si el Padre muere con el inventario abierto, lo cierra sin volver a encender los controles. |
| `SubtitleUI` | prefab `PF_HUD` | `Show(texto)`: lo deja en pantalla según su largo (mínimo 2 s). Lo usarán las notas y las frases del Padre (#33). |
| `RitualBarUI` | prefab `PF_HUD` | `Begin`, `SetProgress(0–1)` y `End`, para el ritual (#34). |

**Paleta de la interfaz.** Paneles casi negros semitransparentes (#0E0F12), texto color hueso (#E8E1D3), vida rojo sangre (#B3261E) con alerta más clara (#FF4536), resistencia dorado de veladora (#C9A327) y ritual azul pálido (#8CBFF2). Los colores están en el Inspector de `HUDController`, así que se ajustan sin código.

**Por qué así.**
- **Eventos y no revisar cada cuadro:** la rúbrica y la arquitectura piden que la UI reaccione al juego; el HUD no pregunta 60 veces por segundo cuánta vida hay, se entera cuando cambia.
- **Un prefab con todo:** cada escena solo arrastra `PF_HUD` y le asigna al jugador.
- **Minimalista:** el GDD pide una UI discreta para no romper la tensión; solo se ve lo necesario.

**Cómo se probó (Play Mode, `EnemyArena`).** Daño 100 → 65: la barra bajó al 65 %; resistencia gastada: la barra bajó y se recuperó; objetos de prueba: aparecieron en los espacios 1–4; en el inventario, **Usar** en una venda curó 25 → 55 y **Tirar** dejó el agua bendita en el suelo; al cerrar, el cursor se volvió a bloquear; morir con el inventario abierto lo cerró y dejó los controles apagados. Consola sin errores.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Cómo se actualiza la barra de vida? | `Health` lanza el evento `Changed(actual, máximo)`; `HUDController` lo escucha y le pasa los números a `StatBar`, que llena la imagen en proporción. |
| ¿Por qué el inventario no pausa? | Elegir objetos tiene un costo: si te persiguen, abrirlo te deja vulnerable. Para detener el juego está la pausa. |
| ¿Qué pasa si el Padre muere con el inventario abierto? | `InventoryUI` escucha `Died` y lo cierra sin devolver los controles, para no interferir con `PlayerDeathHandler`. |

#### Menús, pausa, opciones y derrota (#47)

**Qué hay.** Todo se maneja con el mouse o con el teclado (flechas, Enter y Esc).
- **Menú principal** (escena `MainMenu`, la primera del build):
  - **Nueva partida:** abre la selección de instancia.
  - **Continuar:** carga la escena guardada y pone al Padre en el cuarto seguro donde se guardó. Está apagado si no hay partida guardada.
  - **Opciones**, **Créditos** y **Salir**.
- **Selección de instancia:** las 3 instancias en orden. Las que no se han desbloqueado dicen "(bloqueada)" y no se pueden elegir. Al pasar el mouse o el teclado por una, abajo aparece su descripción.
- **Opciones** (GDD §17): sensibilidad del mouse (×0.25 a ×3), invertir eje Y, volumen general, música, efectos, voces y subtítulos sí/no. Se guardan al momento y sirven igual desde el menú y desde la pausa.
- **Pausa (Esc):** Continuar · Reiniciar (vuelve al último cuarto seguro, igual que Reintentar) · Opciones · Salir al menú. El juego se congela.
- **Derrota:** 1.5 s después de morir aparece "Has caído" con **Reintentar** y **Salir al menú**. Reemplaza la reaparición automática de 3 s que había desde #26.
- **Créditos:** proyecto, desarrollo, equipo, herramientas, fuente y la nota de respeto a las leyendas.

![Menú principal](img/ui/menu_principal.png)
![Selección de instancia](img/ui/menu_instancias.png)
![Opciones](img/ui/menu_opciones.png)
![Pausa](img/ui/pausa.png)
![Derrota](img/ui/derrota.png)
![Créditos](img/ui/menu_creditos.png)

**Cómo funciona.**
| Script | Dónde | Qué hace |
|---|---|---|
| `MainMenu` | escena `MainMenu` | Muestra un panel a la vez (principal, selección, opciones, créditos) y deja seleccionado el primer botón para que el teclado funcione. Esc regresa al panel principal. |
| `InstanceCatalog` | `Data/InstanceCatalog.asset` | La lista de instancias: nombre, descripción y escena. Mientras no existan los niveles finales, la 2 y la 3 cargan `CharroArena` y `TzitzimimeArena`; luego basta cambiar el nombre de escena en el Inspector. |
| `GameProgress` | estático | Qué instancias están abiertas (la 1 siempre). La pantalla de victoria (#48) abre la siguiente. |
| `SceneFlow` | estático | Cargar menú, empezar instancia o continuar. Siempre devuelve el tiempo a velocidad normal antes de cambiar de escena. |
| `GameSettings` | estático | Los valores de Opciones en `PlayerPrefs` (aparte de la partida). Lanza `Changed` y cada sistema se actualiza solo: `PlayerLook` (sensibilidad e invertir Y), `SubtitleUI` (no muestra nada si están apagados), `AudioListener` (volumen general) y `AudioVolume` (música, efectos y voces). |
| `OptionsUI` | prefab `PF_OptionsPanel` | Lee los valores al abrirse y escribe cada cambio en `GameSettings`. |
| `PauseMenu` | prefab `PF_PauseMenu` | Con Esc pone `Time.timeScale = 0` y pausa el audio. Apaga todas las acciones del Padre menos la pausa y apaga `PlayerLook`, lo que suelta el mouse. Si hay un inventario, altar o nota abierto, Esc cierra eso y no pausa. |
| `DeathScreen` | prefab `PF_DeathScreen` | Escucha `Health.Died`, espera y aparece con un fundido. Reintentar llama a `PlayerRespawn.Respawn()`; si aún no se llegó a ningún cuarto seguro, recarga la instancia. |
| `SafeRoom.FindById` y `PlayerRespawn.Start` (cambios) | — | Para Continuar: al cargar la escena, buscan el cuarto seguro guardado y ponen ahí al Padre. |

**Por qué así.**
- Las opciones van en `PlayerPrefs` y no en la partida: borrar o empezar una partida no debe cambiar la sensibilidad de nadie.
- Los menús son prefabs: cada escena con el Padre solo arrastra `PF_PauseMenu` y `PF_DeathScreen` y les asigna al jugador.
- Pausar apagando las acciones del Input System, y no script por script, evita olvidar alguno: ni ataque, ni rezo, ni inventario funcionan con el menú abierto.

**Cómo se probó (Play Mode).**
- **Menú:** con las flechas se pasó por Nueva partida → Continuar → Opciones y de regreso; Enter abrió la selección (solo la instancia 1 abierta, con su descripción) y Esc volvió.
- **Opciones:** con la flecha derecha la sensibilidad pasó a ×1.55. Volumen general 0.5 → `AudioListener.volume` 0.5. Subtítulos apagados → guardado. Luego se regresaron a sus valores.
- **Pausa en I1:** Esc → `timeScale` 0, ataque apagado, mouse libre. Opciones y Esc regresaron a la pausa. Continuar devolvió todo.
- **Esc con el inventario abierto:** lo cerró y no pausó.
- **Derrota:**
  - Sin cuarto seguro: Reintentar recargó la instancia (vida 100).
  - Después de pasar por la choza: Reintentar y Reiniciar pusieron al Padre en la choza.
- **Salir y continuar:** Salir al menú → menú con tiempo normal y mouse libre. Continuar → I1, en la choza guardada.
- Sin errores en la consola en las sesiones limpias.

**Un error que salió en la prueba.** El inventario no se cerraba con Esc, así que había que cerrarlo con Tab antes de poder pausar. Ahora Esc también lo cierra.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Cómo se pausa el juego? | `Time.timeScale = 0`: todo lo que usa `Time.deltaTime` se detiene. Además se apagan las acciones del Padre y se pausa el audio. |
| ¿Dónde se guardan las opciones? | En `PlayerPrefs`, el almacén pequeño de Unity para configuraciones, separado de `partida.json`. |
| ¿Cómo se navega con teclado? | El `EventSystem` con `InputSystemUIInputModule` mueve la selección entre botones según su posición; cada menú deja seleccionado su primer botón al abrirse. |
| ¿Qué pasa si das Reintentar sin haber llegado a un cuarto seguro? | `Respawn()` devuelve `false` y la instancia se carga desde el principio. |

#### Victoria, paso entre instancias y créditos finales (#48)

**Qué hace.** Al llegar **vivo** a la salida de una instancia, el juego se detiene y aparece el resumen:
- el título: "Escapaste de Xochimilco", "Escapaste de la hacienda"…;
- tiempo, muertes y notas leídas;
- los botones **Siguiente instancia** y **Salir al menú**.

En ese momento:
- se desbloquea la siguiente instancia en la selección;
- se guarda el **equipaje**: los 8 espacios, los materiales, las recetas y las notas pasan completos (GDD P4). No pasan los objetos clave, porque cada instancia tiene los suyos, y la vida empieza en 100.
- **Continuar** en el menú queda apuntando al inicio de la siguiente instancia.

Al escapar del ex-convento llega la **victoria final**:
- dice "¡La Tzitzimime ha sido sellada!" con un cierre de la historia;
- **Ver créditos** lleva al menú con los créditos abiertos;
- se borra la partida, porque ya no hay nada que continuar; la selección de instancias sigue abierta.

![Victoria de instancia](img/ui/victoria_instancia.png)
![Victoria final](img/ui/victoria_final.png)

**Cómo funciona.**
| Pieza | Dónde | Qué hace |
|---|---|---|
| `VictoryScreen` | prefab `PF_VictoryScreen` en cada nivel | Escucha `LevelExit.Reached`. Busca en `InstanceCatalog` qué instancia es; si hay siguiente, la desbloquea y guarda el equipaje; si es la última, borra la partida. Detiene el juego (`timeScale` 0 y acciones del Padre apagadas) y cuenta las muertes con `Health.Died`. |
| `PlayerPersistence.CaptureForNextInstance` | Padre | Hace una foto del Padre para la siguiente instancia: inventario, materiales, recetas y notas, sin objetos clave y con la vida completa. |
| `SaveSystem.SaveCarry` / `LoadCarry` | estático | El equipaje va en su propio archivo (`equipaje_<escena>.json`), aparte de `partida.json`. Así elegir la instancia 2 desde la selección también te da lo que llevabas al terminar la 1. |
| `PlayerRespawn.Start` (cambio) | Padre | Al cargar un nivel: con Continuar, va al cuarto seguro guardado (o a la entrada con el equipaje); al empezar una instancia, aplica su equipaje si existe. |
| `SceneFlow` (cambio) | estático | `StartInstance` marca "instancia nueva". Siempre vuelve a encender las acciones del Padre al cambiar de escena, porque viven en un asset que sobrevive entre escenas. `ShowCreditsOnMenu` abre los créditos al volver al menú. |
| `LevelExit` (cambio) | salida | Ya no guarda (eso lo hace la victoria) y no cuenta a un Padre muerto. |

**Por qué así.**
- El equipaje va aparte de la partida: si el jugador vuelve a elegir una instancia desde el menú, empieza con lo que llevaba al terminar la anterior, y no vacío ni con cosas de otra partida.
- Los objetos clave no pasan: las muñecas de Xochimilco no sirven en la hacienda, y cada ancla pide sus propias ofrendas.

**Cómo se probó (Play Mode, desde el menú).**
1. Se borraron la partida, el equipaje y el progreso. Nueva partida → I1. Se le dieron al Padre 2 vendas, 3 aguas benditas, 7 de sal, 1 muñeca y 30 de daño, y se le llevó a la salida.
2. Victoria: "Escapaste de Xochimilco", juego detenido, instancia 2 desbloqueada.
3. **Siguiente instancia** → `CharroArena` con las vendas, el agua bendita y la sal, 0 muñecas y vida 100.
4. Salida → "Siguiente instancia". Luego Salir al menú → **Continuar** → `TzitzimimeArena` con el mismo equipaje.
5. Desde la pausa, Salir al menú → selección → instancia 2 → salida → siguiente → `TzitzimimeArena` con equipaje.
6. Salida final: "¡La Tzitzimime ha sido sellada!" → **Ver créditos** → menú con los créditos abiertos, sin partida guardada y con las 3 instancias abiertas.

**Errores que se encontraron en la prueba.**
- Si el Padre moría y su cuerpo terminaba en la zona de salida, contaba como escape. Ahora solo cuenta un Padre vivo.
- Decía "Escapaste de el ex-convento". Ahora cada nivel escribe su propio "de"/"del".

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Cómo pasa el inventario a la siguiente instancia? | La victoria guarda un `SaveData` con el inventario en `equipaje_<escena>.json`. Al cargar la siguiente escena, `PlayerRespawn` lo lee y se lo devuelve al Padre con `PlayerPersistence.Restore`. |
| ¿Cómo se sabe cuál es la siguiente instancia? | `InstanceCatalog` tiene las 3 en orden; se busca la escena actual y se toma la que sigue. |
| ¿Qué pasa al terminar el juego? | Victoria final, créditos y menú. Se borra la partida, pero la selección de instancias sigue abierta para rejugar. |

### 5.15 Audio y feedback
*Pendiente — issues #44 y #50.*

### 5.16 Arte: modelos, animaciones e iluminación
*Cultista: issues #3 y #4. La Llorona: issues #17 y #18. El Charro Negro y su caballo: issues #17 y #37. La Tzitzimime: issues #17 y #41. Pendiente: brazos del Padre (#28) e iluminación.*

**Qué hace.** Los personajes se diseñan en una **ficha** (`docs/personajes/`) y se construyen en Blender **con código**: un script arma el modelo low-poly con formas simples, le pone esqueleto, lo anima y lo exporta a Unity. El Cultista es el primero: túnica negra plisada, capucha de tela honda, máscara ritual de hueso con la estrella de ocho puntas en la frente y ojos rojos, la estrella plana en el pecho y un machete. Tiene unos 1 620 triángulos y 5 animaciones: `Idle`, `Walk`, `Run`, `Attack` y `Death`.

**Cómo funciona.**
| Pieza | Dónde | Qué hace |
|---|---|---|
| Ficha | `docs/personajes/cultista.md` | Silueta, medidas en metros, paleta en hex, presupuesto de polígonos, huesos y lista de animaciones. Es el "concept art" en texto. |
| Renders de la ficha | `docs/personajes/img/chr_cultista_frente.png` y `_perfil.png` | Vista de frente y de perfil en postura en T, generadas por el script. |
| `Tools/Blender/lowpoly_humanoid.py` | herramientas compartidas | Crea materiales con los colores de la ficha, el esqueleto humanoide con nombres estándar, las piezas (tubos, cajas, esferas, tela plisada por revolución, la estrella, la hoja del machete) y las animaciones; exporta el FBX. |
| `Tools/Blender/chr_cultista.py` | el personaje | Usa esas herramientas para armar al Cultista pieza por pieza y definir sus 5 animaciones. Se ejecuta con `blender -b --factory-startup --python Tools/Blender/chr_cultista.py`. |
| `chr_cultista.fbx` | `Art/Models/Characters` | Malla, materiales, esqueleto y animaciones. Unity lo importa como **Humanoid** y separa cada animación en un clip. |
| `AC_Cultista.controller` | `Art/Animations` | Animator: un *blend tree* "Locomotion" pasa de `Idle` a `Walk` y `Run` según el parámetro `Speed`; los triggers `Attack` y `Die` lanzan esas animaciones. Lo usará la IA del Cultista (#15). |

**Por qué así.**
- **Modelar con código en vez de IA 3D y Mixamo:** no dependemos de planes gratis, cuentas externas ni licencias de terceros; el modelo se puede regenerar al cambiar la ficha; y el estilo low-poly encaja con el arte del GDD. Para la defensa, el script es una explicación paso a paso de cómo se construyó.
- **Pesado rígido:** cada pieza sigue a un solo hueso. En personajes low-poly con ropa holgada se ve bien y nunca deforma raro. La túnica cubre las piernas, así que al caminar solo se ven las sandalias.
- **Humanoid en Unity:** los huesos usan los nombres estándar (`Hips`, `Spine`, `LeftUpperArm`…), así que Unity los reconoce. Ser Humanoid permite reutilizar el mismo Animator con otros personajes humanos.
- **Animaciones en el sitio:** el personaje no se desplaza dentro de la animación; lo mueve el `NavMeshAgent`. Así la velocidad real y la del dibujo no se pelean.
- **Rotaciones en ejes del mundo:** el script describe las poses como "el brazo apunta hacia adelante y abajo", no con ángulos locales de cada hueso, que son difíciles de razonar.

**La Llorona** (`Tools/Blender/ent_llorona.py`, ficha `docs/personajes/llorona.md`) se hizo con el mismo método: muy delgada y de 2.20 m (más alta que el Padre), vestido blanco suelto hasta el suelo con el borde en jirones (parece flotar), cabello negro en mechones largos que casi siempre le tapa la cara, brazos huesudos con codos marcados y dedos largos; unos 2 330 triángulos. Entre los mechones a veces se ve su cara demacrada con ojos rojos que brillan. Cada mechón está partido: arriba sigue a la cabeza y abajo al pecho, para que no atraviese el vestido cuando se encorva. Sus animaciones son `Idle` (solloza encorvada), `Stalk` (se desliza lenta), `Walk` (se acerca con los brazos al frente), `Grab` (abre los brazos y los cierra sobre el jugador) `Vanish` (se dobla y se hunde en el suelo; el desvanecido lo hará el material en el #35) y `Scream` (echa la cabeza atrás y se le ve la cara). Su Animator es `AC_Llorona`.

**El Charro Negro y su caballo** (`Tools/Blender/ent_charro.py` y `ent_caballo.py`, ficha `docs/personajes/charro.md`) son dos modelos. El caballo no es humano, así que su esqueleto es **Generic** (no Humanoid): 21 huesos propios, con las patas traseras en ángulo como un caballo real, y clips `Idle`, `Gallop`, `Charge` (se encabrita y arremete), `Hit` y `Dissolve`. El caballo se modela con un **loft**: anillos a lo largo del cuerpo, cada uno pegado a su hueso, que forman una sola pieza (cruz, lomo y grupa) en vez de esferas sueltas; igual el cuello y la cabeza. El caballo es un caballo de guerra escalado ×1.15 (cruz a ~1.9 m), musculoso, con patas de cilindros gruesos, cabeza ancha y crin corta; monta a pelo (sin silla). El jinete sí es Humanoid y robusto: chaqueta de charro entallada con solapas y bordado de plata, guantes con dedos, moño rojo oscuro, sombrero de ala ancha asentado sobre una calavera con brasas rojas en los ojos. Las riendas llegan a sus manos: cada rienda tiene un extremo pegado a la cabeza del caballo y el otro al hueso donde va montado, justo donde quedan sus manos en `RideIdle` (se midió en Blender). Se modela de pie y la postura de montar sale de sus clips (`RideIdle`, `RideGallop`, `Offer`, `Dissolve`). En el prefab `PF_CharroNegro`, el jinete es hijo del hueso `Saddle` del caballo, así que va pegado a la silla en cualquier movimiento.

**La Tzitzimime** (`Tools/Blender/ent_tzitzimime.py`, ficha `docs/personajes/tzitzimime.md`) es el jefe final: una figura de 3.2 m inspirada en los seres de las estrellas del mito mexica, encorvada, con cráneo de ojos rojos y mandíbula abierta, un sol negro de obsidiana detrás de la cabeza (tocado ceremonial), cuerpo esquelético oscuro como el cielo con puntos de luz, taparrabo de tiras y huesos, púas en la espalda y garras muy largas. Sus clips son `CeilingCrawl` (gatea; el código la pone en el techo), `CeilingAttack`, `Reveal` (el copal la quema), `FinalIdle`, `FinalAttack` y `Defeat`.

**Cómo se probó.** En Blender se renderizaron cuadros de cada animación desde el FBX exportado. En Unity, el avatar es Humanoid válido con los 19 huesos mapeados, el personaje mira hacia +Z, y en Play Mode pasa por `Locomotion`, `Attack` (0.87 s) y `Death` (termina tirado en el suelo) sin errores.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Por qué no usaron Mixamo como decía el plan? | Mixamo pide cuenta de Adobe y subir el modelo; con código el esqueleto y las animaciones son nuestros, se regeneran y no dependen de un servicio. |
| ¿Qué es un rig? | El esqueleto: huesos dentro del modelo. Al girar un hueso, la parte del modelo pegada a él se mueve. |
| ¿Por qué tan pocos polígonos? | El GDD pide low-poly y en una oleada hay varios cultistas a la vez; unos 1 620 triángulos dejan rendimiento de sobra. |
| ¿Cómo camina la Tzitzimime por el techo si su animación es en el suelo? | La animación es un gateo normal; en el juego el código gira el modelo 180° y lo pega al techo. Así una sola animación sirve y no hay que hacer física de techo. |
| ¿Por qué el caballo no es Humanoid? | Humanoid es solo para cuerpos humanos (cabeza, dos brazos, dos piernas). Un cuadrúpedo usa un esqueleto Generic con sus propios huesos y sus clips se reproducen tal cual. |
| ¿Por qué a veces un modelo se veía hueco? | Unity dibuja cada cara de un solo lado (por fuera). Si un tubo queda abierto y dos piezas se separan al moverse, por la abertura se ve el interior y el modelo parece vacío. Se arregló cerrando las piezas del torso y haciéndolas traslapar. |
| ¿Por qué la manga empieza en el hombro y no dentro del cuerpo? | Con pesado rígido, la parte de la manga que quedara dentro del torso giraría hacia arriba al bajar el brazo y saldría por el hombro. Por eso la manga empieza en la articulación y una esfera redondea el hombro. |
| ¿Cómo va el jinete arriba del caballo? | Es hijo del hueso `Saddle` del caballo: Unity mueve a los hijos con su padre, así que cuando el caballo galopa o se encabrita, la silla se mueve y el jinete con ella. Cada uno tiene su propio Animator. |
| ¿Por qué casi no se le ve la cara a la Llorona? | Da más miedo lo que se ve a medias: el cabello la tapa casi siempre y solo a veces, entre los mechones o cuando grita, aparecen la cara y los ojos rojos. Los ojos son un material emisivo, así que brillan en la oscuridad sin necesitar luz. |
| ¿Cómo "flota" la Llorona? | El vestido toca el suelo y las piernas casi no se mueven al deslizarse, así que el movimiento se lee como un deslizamiento, no como pasos. |
| ¿Por qué una máscara y no una cara? | Se ve de lejos con la linterna en la oscuridad, refuerza la identidad del culto (la estrella en la frente) y evita modelar y animar un rostro. |
| ¿Cómo sabe Unity qué animación poner? | El Animator Controller tiene estados y transiciones: `Speed` elige entre quieto, caminar y correr; los triggers `Attack` y `Die` saltan a esas animaciones. |
| ¿Qué es Humanoid? | Un tipo de esqueleto que Unity entiende como cuerpo humano (cabeza, brazos, piernas). Permite compartir animaciones entre personajes humanos. |

### 5.17 Cuarto de prueba de enemigos
*Issue #77. Solo para desarrollo: no forma parte de los niveles.*

**Qué es.** La escena `Scenes/Test/EnemyArena` es un cuarto para probar enemigos, combate y objetos sin jugar un nivel completo. Tiene tres zonas:
- **Cuarto de inicio (sur):** el Padre aparece aquí. En la pared izquierda hay botones rojos que se usan con F: aparecer un cultista, quitar los cultistas extra, curar al Padre, recibir 20 de daño, recibir objetos de prueba y apagar/encender las luces (para probar con la linterna). En la mesa de la derecha hay vendas, agua bendita, sal y botellas para recoger a mano. Una pared con una puerta de 3 m tapa la vista de los cultistas hasta que el Padre sale.
- **Zona de cultistas (centro):** dos cultistas patrullan entre pilares y cajas, que sirven para probar que no ven a través de las paredes.
- **Galería (norte):** la Llorona, el Charro Negro con su caballo y la Tzitzimime, detrás de un muro bajo. Frente a cada uno hay un atril: cada F reproduce la siguiente animación (caminar, agarrar, gritar, desvanecerse, ataque desde el techo…). Así se revisan las animaciones antes de que exista su IA.

Arriba a la izquierda, un texto muestra la vida y la resistencia del Padre, sus 8 espacios de objetos y el estado de cada cultista (patrulla, persigue, ataca, busca), su vida y su distancia.

**Cómo funciona.**
| Script | Qué hace |
|---|---|
| `TestButton` | Un `IInteractable` con un `UnityEvent`: lo que hace se conecta en el Inspector, sin código nuevo por cada botón. |
| `EnemyArenaTools` | Las acciones de los botones. Los cultistas aparecen con `Instantiate` y reciben su ruta con `CultistAI.SetPatrol` (máximo 6, para no llenar el cuarto). |
| `AnimationTestStation` | Recorre una lista de pasos: `Grab` dispara un trigger, `Speed=2` cambia un número, `OnCeiling=true` un booleano y `Reset` regresa a la pose inicial. |
| `DebugOverlay` | El texto de depuración. El HUD real es otra cosa (#22). |

**Por qué existe.** Probar a un enemigo dentro de un nivel obliga a recorrerlo cada vez. Aquí todo está a pocos pasos, y el NavMesh (`NavMesh-Arena.asset`) y los prefabs son los mismos que usarán los niveles, así que lo que funciona aquí funciona allá.

**Cómo se probó (Play Mode).** Los botones aparecieron cultistas, dañaron y dieron objetos; un cultista extra persiguió al Padre y le bajó la vida de 80 a 60; los atriles dispararon las animaciones de las tres entidades; las luces se apagaron y prendieron; sin errores en la consola.

## 6. Decisiones de diseño importantes
Las 16 decisiones que cerraron los huecos del GDD están en `docs/PENDIENTES_DISENO.md` (sección "Decididas"). Las que más preguntan:
- **El inventario pasa de una instancia a otra:** así lo que fabricaste en Xochimilco te sirve en la hacienda, y es más simple de guardar.
- **La trajinera no es un vehículo:** avanza sola por una ruta fija; el jugador solo se defiende desde ella. El alcance del curso no incluye vehículos.
- **El rezo del ritual es distinto al rezo normal:** en el ancla, mantener R llena la barra sin enfriamiento; un golpe fuerte la hace retroceder.
- **El Charro Negro no usa la barra del ritual:** se vence con 3 golpes de cruz cuando cruza la línea de agua bendita. Así la segunda instancia se siente distinta (la rúbrica pide un nivel que cambie la experiencia).
- **Al morir** se vuelve al último cuarto seguro con 60 de vida; se guarda al entrar y al salir del cuarto para no perder lo fabricado.

## 7. Cómo cumple la rúbrica
La tabla "Cobertura en el plan" de `docs/RUBRICA.md` dice, rubro por rubro, qué parte del juego lo cubre. Al final del proyecto esta sección resume dónde se ve cada criterio dentro del juego (qué mostrar en la demo).

*Pendiente — se completa en el issue #52.*

## 8. Preguntas probables del profesor
| Pregunta | Respuesta corta |
|---|---|
| ¿Por qué Unity y no otro motor? | Es el que pide el curso, tiene NavMesh, URP y herramientas de IA integradas, y Claude puede controlarlo con Unity CLI. |
| ¿Por qué usaron IA? ¿Quién decidió? | La IA escribió borradores de código y documentos; Jesús revisó, probó en Play Mode y decidió cada cambio. Todo cambio pasó por un pull request. |
| ¿Por qué `Time.deltaTime`? | Para que la velocidad no dependa de los FPS de la computadora. |
| ¿Qué es un ScriptableObject y para qué lo usaron? | Un archivo de datos de Unity; ahí viven los valores de balance (daño, vida, recetas) para ajustarlos sin cambiar código. |
| ¿Cómo evitan que el proyecto se rompa entre versiones? | Git con ramas y pull requests, `main` protegida, una sola versión de Unity fija en el README y `Force Text` para que los cambios se puedan comparar. |

*Se agregan preguntas con cada sistema.*
