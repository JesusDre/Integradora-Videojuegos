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

**Qué hace.** Q (o la rueda del mouse) cambia del machete a la escopeta. Clic izquierdo dispara y clic derecho apunta (la cámara se acerca un poco). Tiene 2 cartuchos; al vaciarse recarga sola en 2 segundos con las balas de sal del inventario, que se fabrican en el altar; con **C** se recarga antes, aunque quede un cartucho (issue #113). Si no hay balas, C suena el "clic" vacío. Cada disparo suelta **8 perdigones** que se abren en un cono de 5°: de cerca pegan casi todos y hacen 48 de daño (un cultista, con 40 de vida, muere de un tiro); a menos de 8 m cada perdigón vale 6 y más lejos 2.5, y además muchos ya no le atinan al cuerpo, así que de lejos casi no hace nada. A los espíritus solo el 25 %. El disparo hace ruido: todo cultista a 20 m lo oye y va a ver (deja de patrullar y persigue). Abajo a la derecha, el HUD muestra el arma en la mano y la munición: "2 | 10" (cargados | en el inventario).

![Escopeta](img/ui/escopeta.png)

**Cómo funciona.**
| Pieza | Dónde | Qué hace |
|---|---|---|
| `RangedWeaponDefinition` | `Data/Items/Weapons/WPN_Escopeta.asset` | Daño cerca y lejos, distancia, capacidad, recarga, radio del ruido, zoom y sonidos. |
| `PlayerShotgun` | Padre | Lanza un rayo por perdigón (`SphereCastNonAlloc`, ignorando el cuerpo del Padre), cada uno desviado al azar dentro del cono (`Random.insideUnitCircle`). Suma el daño de los perdigones que tocan a cada blanco y aplica **un solo** `TakeDamage` por blanco, así el golpe se ve y suena una vez. El daño depende de la distancia y del tipo de vida. Avisa a los cultistas con `Physics.OverlapSphereNonAlloc` + `CultistAI.HearNoise`. Recarga con un temporizador, sin corrutinas. |
| Acción `Reload` (C) | `Data/Input/AnimasControls.inputactions` | `PlayerShotgun.TryReload` revisa que falten cartuchos y que haya balas, y usa el mismo temporizador de la recarga automática. |
| `WeaponSwitcher` | Padre | Solo una de las dos armas está encendida (`enabled`), así el clic hace una sola cosa. Si un menú o la muerte lo apagan, apaga también el arma en la mano. |
| `HUDController` | `PF_HUD` | Escucha `WeaponSwitcher.Switched` y `PlayerShotgun.AmmoChanged`. |

**Detalles que importan.**
- **Munición con límites:** los cartuchos nunca pasan de 2 y solo se sacan del inventario los que caben; si no hay balas, no recarga y suena el "clic" vacío.
- **Un error que se corrigió en las pruebas:** al terminar de recargar, sacar las balas del inventario avisaba "cambió el inventario" cuando la escopeta aún marcaba 0 cartuchos, y eso empezaba una segunda recarga. Ahora los cartuchos se cargan antes de sacar las balas.
- **Dispersión (issue #111):** el `damageNear`/`damageFar` del asset es el total si todos los perdigones pegan; cada uno lleva su parte (`daño / pellets`). Así se ajusta el balance con dos números (`pellets` y `spreadDegrees`) sin tocar código.
- **El ruido es una decisión de juego:** disparar resuelve un problema pero atrae a otros; por eso la escopeta no reemplaza al machete.

**Cómo se probó (Play Mode, `EnemyArena`).**
- **Recarga:** con 6 balas, cargó 2 en 2 s (inventario 6 → 4). Durante la recarga no dispara; sin balas en el inventario no recarga.
- **Daño (con dispersión, #111):** en un campo de tiro limpio, un cultista a 3 m y otro a 5 m murieron de un tiro; a 7 m bajó 30, a 10 m bajó 10 y a 14 m bajó 5. A 3 m, 400 de 400 perdigones pegaron.
- **Ruido:** tras los disparos, los dos cultistas a 15–16 m pasaron de patrullar a perseguir.
- **Recarga con C (#113):** con 2 cartuchos, C no hace nada; tras disparar uno, C recargó en 2 s (1 → 2 cartuchos, inventario 4 → 3).
- **Cambio de arma:** Q enciende una arma y apaga la otra; con el inventario abierto ninguna dispara y al cerrarlo vuelve la escopeta.
- Consola sin errores.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Cómo atrae el disparo a los cultistas? | `Physics.OverlapSphereNonAlloc` busca colliders a 20 m; a cada `CultistAI` le llama `HearNoise(posición)` y él va a ese punto. |
| ¿Por qué el daño baja con la distancia? | Así lo marca la tabla de balance (48 a menos de 8 m, 20 más lejos) y además los perdigones se abren: la escopeta es para distancias cortas. |
| ¿Cómo funciona la dispersión? | Cada perdigón sale en la dirección de la mira más un desvío al azar dentro de un círculo; el tamaño del círculo sale de la tangente del ángulo (5°). Entre más lejos, más separados quedan los perdigones. |
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

#### Textos del culto y del Padre (#33)

**Qué hay.** Todos los textos del juego están en `docs/TEXTOS.md`, para leerlos y revisarlos en un solo lugar.
- **8 notas del culto.** Cuentan la historia de los Hijos de la Estrella Negra y del Padre Ignacio, el sacerdote que casi los detuvo hace veinte años, y dan pistas de cómo vencer a cada entidad. Tres de ellas enseñan recetas. La octava, el *Sermón de la Estrella Negra* en las criptas del convento, revela el plan del culto.
- **Frases del Padre.** Van en subtítulos con «comillas» y son de dos tipos:
  - las que responden a lo que pasa: vida baja, primer rezo, reaparecer, primera vez en cada cuarto seguro, cada objeto clave, y el inicio y el final del ritual;
  - las que dice al llegar a un lugar: 4 por instancia, como el inicio, la chinampa de las muñecas, la capilla o las catacumbas.

![Subtítulo del Padre](img/ui/subtitulo_padre.png)

**Cómo funciona.**
| Pieza | Dónde | Qué hace |
|---|---|---|
| `NoteDefinition` | `Data/Notes/NOTE_*` | El título y el texto de cada nota. |
| `PadreLines` | `Data/Textos/PadreLines.asset` | Las frases de cada momento. Cuando hay varias, se elige una al azar. |
| `PadreVoice` | `PF_Player` | Escucha eventos que ya existen (`Health.Changed`, `PlayerPrayer.Completed`, `PlayerRespawn.Respawned`, `SafeRoom.Saved`, `KeyItemBag.Changed` y `RitualController.StageChanged`) y muestra la frase en `SubtitleUI`. La vida baja habla como mucho una vez por minuto. Cada cuarto seguro habla solo la primera vez. Al cargar una partida no anuncia los objetos clave restaurados. |
| `SubtitleTrigger` | `Frases_Padre` en cada nivel | Una caja: la primera vez que el Padre entra, dice su frase. Revisa la posición cada cuadro, como los encuentros, así que no falla ni con teletransportes. |

Los textos se pasaron de `docs/TEXTOS.md` a Unity con un script, no a mano, para que no haya diferencias entre el documento y el juego. Si se apagan los subtítulos en Opciones, no se muestra ninguna frase.

**Cómo se probó (Play Mode).**
- En I1 se vieron, en orden:
  - la frase del inicio;
  - «Una ofrenda…» al tomar la primera muñeca;
  - «Todavía no, Señor…» al bajar a 25 de vida;
  - «Aquí no entran. Respira.» al entrar a la choza;
  - la frase de la isla.
- En el convento se vio la frase del inicio.
- Consola sin errores.

**Un error que salió en la prueba.** La referencia de `PadreVoice` a `PadreLines` quedaba vacía: el asset se había creado en el mismo script antes de cambiar de escena, y Unity lo descargó. Se reasignó, y la prueba se repitió hasta que salieron todas las frases.

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
*Planos: issues #5, #30 y #38. Blockout de la instancia 1: #16. Niveles de las instancias 2 y 3: #40 y #46. Arte de la instancia 1: #36.*

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

**Cómo se probó (Play Mode, recorrido automático a velocidad ×3).** El Padre caminó del inicio al muelle (38 m; se activó el encuentro G1) y subió a la trajinera. Llegó a la chinampa de las muñecas, caminó a la choza y al entrar se guardó en `I1_Choza` con las puertas cerradas. Salió por la puerta este, cruzó el puente a la isla hasta el muelle norte y subió a la trajinera de huida. Llegó a la salida. Sin la trajinera, el NavMesh no une el embarcadero con la chinampa (correcto: ahí se cruza en trajinera). Consola sin errores.

**Arte de la instancia 1** (*issue #36*). El blockout de Xochimilco se vistió con un kit propio, `Tools/Blender/env_xochimilco.py`: 10 piezas low-poly (de 56 a 730 triángulos) en `Art/Models/Environment/Xochimilco`.
- **Trajineras:** el casco, el toldo y el arco pintado de colores. El modelo es hijo de la trajinera, así que viaja con ella por la ruta.
- **La choza del cuidador** (el cuarto seguro): adobe y techo de zacate, con las puertas en el mismo lugar que las del nivel.
- **Ahuejotes:** 71 en las orillas de las chinampas, con tule a la orilla del agua, como en los canales reales.
- **El ahuehuete** de la isla, con heno colgando.
- **Embarcaderos y puente:** 4 embarcaderos donde se sube o se baja de la trajinera, y el puente de madera hacia la isla.
- **Postes:** los 10 postes de madera donde cuelgan las muñecas.
- **El ancla de la Llorona:** un altar de piedra con cempasúchil. Sus 3 veladoras fijas son ahora el modelo de veladora.

**Cómo se cambió sin romper nada.** Las cajas grises se ocultaron (su `MeshRenderer` se apagó), pero sus colliders siguen ahí. Por eso el Padre choca y camina exactamente igual que antes y el modelo solo se pone encima. El ahuehuete sí cambió su caja de 5 × 8 m por un collider de tronco, y cada ahuejote tiene uno delgado. El NavMesh se volvió a hornear con los árboles como obstáculos.

**Luz y niebla.** De noche el agua salía casi blanca: el material era muy liso y reflejaba la luna. Ahora es oscura, sin reflejos, y la niebla es gris azulado oscuro, para que el horizonte no brille. Las texturas, el cielo y el agua en movimiento quedan para #101, #102 y #103.

![Xochimilco: embarcadero, muñecas, choza e isla](img/niveles/i1_arte.png)

**Cómo se probó (Play Mode).**
- Se tomaron capturas desde el embarcadero, la chinampa de las muñecas, la choza y la isla.
- La trajinera del cruce viajó con su modelo y dejó al Padre en la otra orilla (57.5, 46).
- En el NavMesh siguen completos los caminos: inicio → embarcadero, chinampa B → choza, puerta de la choza → ancla, y ancla → trajinera de huida.
- Consola sin errores.

**Ambientación de terror** (*issue #109, pedido de Jesús*). Las tres instancias se hicieron más tétricas:
- **Niebla:** más densa, con un tono por instancia: gris verdoso en Xochimilco, ocre en la hacienda y casi negra en el convento. Además hay **niebla baja** (`PF_NieblaBaja` + `GroundFog`): grandes manchas de partículas que nacen alrededor del Padre y se quedan en su lugar, así que al caminar se atraviesan como neblina real.
- **Vegetación** (`Tools/Blender/env_terror.py`): pasto verde en Xochimilco y seco en la hacienda, arbustos y árboles secos de ramas torcidas, que se recortan contra la niebla. Hay unos 1 100 manojos de pasto en Xochimilco, 750 en la hacienda y 260 en el convento (atrio y jardín del claustro).
- **Camino de tierra** en Xochimilco: une el embarcadero, la chinampa de las muñecas, la choza, el puente, el ancla y la trajinera de huida, y guía al jugador en la niebla.
- **Muñecas tétricas.** La muñeca ahora tiene:
  - cuencas negras sin ojos y manchas rojas de lágrimas;
  - grietas, la cabeza ladeada, el pelo apelmazado y el vestido con lodo.

  Hay tres variantes de decoración: colgada del cuello (sin un brazo), un hilo con cabezas y una rota en el lodo. En Xochimilco hay 111 colgando de ahuejotes y árboles secos (8 de ellas clavadas en la choza) y 55 tiradas. La muñeca ofrenda (objeto clave) usa el mismo modelo, pero sigue siendo clara para que se vea de lejos.

![Xochimilco con niebla, camino, árboles secos y muñecas](img/niveles/terror_xochimilco.png)
![Xochimilco, hacienda, panteón y convento con niebla](img/niveles/terror_mapas.png)

**Cómo se repartió.** Un script del Editor pone cada objeto en un punto al azar, pero solo si:
- el suelo es el correcto (la chinampa, la terracería o el jardín);
- está lejos del camino, de los cuartos seguros y de todo lo que se usa con F;
- no hay ninguna pared ahí.

Los árboles secos llevan un collider delgado y el NavMesh se volvió a hornear en las tres instancias. Los pisos de la hacienda y el convento se veían casi blancos (eran lisos y reflejaban el sol bajo); se hicieron mates y más oscuros.

**Cómo se probó.**
- Capturas en Play Mode de las tres instancias.
- En el NavMesh de Xochimilco siguen completos todos los caminos del recorrido.
- En el Editor el juego corre a 55–58 FPS con la vegetación. Falta medirlo en el build (#65).

**Nivel de la instancia 2: la hacienda** (*issue #40*). La escena `Scenes/Levels/I2_Hacienda` arma el plano de 180 × 130 m con formas simples. Las coordenadas son las mismas del plano; los planos dibujan "y" y en Unity es "z".

**Sección 1.**
- El Padre llega por el campo de agave, entre hileras de agaves.
- **Trojes** a oscuras, con techo: un muro interior, puertas al este y al oeste, la primera moneda y el encuentro G1 (3 cultistas).
- **Casa grande:** muros de 4.5 m alrededor de un patio abierto con fuente. El encuentro G2 (4 cultistas) está en el patio. La segunda moneda está en el despacho y la nota de la chancla bendita en la cocina, con dos lámparas.

**Cuarto seguro:** la **tienda de raya**. Es la misma choza de la instancia 1 movida aquí, con el id `I2_TiendaRaya`, altar y dos puertas.

**Sección 2.**
- Campo abierto donde cabalga el Charro (`Charro_Territorio`).
- **Panteón:** bardas bajas y filas de tumbas. Tiene el encuentro G3 (4 cultistas), la tercera moneda y una nota nueva que explica cómo vencer al Charro.
- **Capilla:** techo, 3 veladoras, el pedestal de las monedas, la **pila de agua bendita** y la barrera que el Charro no cruza. Enfrente están las **3 canaletas**.

**Huida.** Un camino de tierra lleva al **portón**, que está con candado hasta vencer al Charro. En el camino está el encuentro G4 (3 cultistas) y la salida queda afuera.

**Luz.** Un atardecer que se apaga: sol naranja muy bajo, niebla café y luces cálidas solo en la casa y la capilla (GDD §22: ocre, negro y rojo apagado).

![Hacienda: la capilla y sus canaletas](img/niveles/i2_blockout_capilla.png)
![Hacienda: de las trojes a la casa grande](img/niveles/i2_blockout_casa.png)

| Pieza nueva | Qué hace |
|---|---|
| `HolyWaterFont` | La pila de la capilla (GDD §18): F da 1 agua bendita, hasta 3 veces; se rellena cuando el Padre reaparece. Si no hay espacio en el inventario, no gasta usos. |
| `CharroNegroEntity.territory` (cambio) | Una caja opcional con el terreno donde cabalga el Charro (como en el plano). Si el Padre sale de ella, el Charro lo deja y regresa a su lugar. Así no persigue al Padre por las trojes desde el primer minuto. |
| `NOTE_I2_Capilla` | Nota del panteón: pedestal, canaletas y tres golpes de cruz. |

**Cómo se probó (Play Mode, recorrido automático con `PathWalker` a velocidad ×4; enemigos apagados para probar la geometría).**
1. Inicio → trojes (moneda 1) → despacho (moneda 2) → cocina.
2. Tienda de raya: se guardó `I2_TiendaRaya` con 2 monedas.
3. Panteón: nota y moneda 3 (estaba sobre una tumba; se movió al pasillo) → pedestal de la capilla con las 3.
4. Pila: dio 3 aguas benditas y quedó vacía.
5. Canaleta llena y el Charro despierto: 3 embestidas sobre el agua y 3 golpes de cruz cargada. Se disolvió y el portón se abrió.
6. Camino → portón → salida: victoria "Escapaste de la hacienda" (2:10 a velocidad ×4).

**Con los enemigos encendidos:**
- G1, G2 y G3 aparecieron con 3, 4 y 4 cultistas, todos sobre el NavMesh y patrullando.
- El Charro esperó quieto mientras el Padre estaba en la sección 1. Cuando entró a su terreno, embistió (−25), y al salir el Padre regresó a su lugar.
- Consola sin errores.

**Detalles que salieron en la prueba.**
- La choza se movió de su posición original. Al hornear el NavMesh hay que sincronizar la física (`Physics.SyncTransforms`); si no, los muros cuentan donde estaban antes. Se volvieron a hornear `I2_Hacienda`, `CharroArena` y `TzitzimimeArena`.
- Las puertas del cuarto seguro cuentan como muro en el NavMesh porque empiezan cerradas: los cultistas no deben entrar. El Padre no usa el NavMesh, así que no le afecta.

**Nivel de la instancia 3: el ex-convento** (*issue #46*). La escena `Scenes/Levels/I3_Convento` (160 × 130 m) es la única con **cuatro alturas**, como pide el plano:

| Altura | Zona | Qué hay |
|---|---|---|
| +5 m | Coro alto del templo | Se sube por una rampa junto al muro este (en el plano, la escalera de caracol). Ahí está el primer fragmento. |
| 0 m | Atrio, templo, claustro, celda | Templo con bancas, altar mayor y antorchas. Claustro de galerías techadas alrededor de un jardín con columnas, con G1 (4) y G2 (5). La sala capitular está a oscuras y guarda el segundo fragmento. La nota del copal está en el claustro y el alijo de resina y brasas, en una esquina. La celda del prior es el cuarto seguro (`I3_CeldaPrior`). |
| −4 m | Criptas | Bajo tierra, por una escalera desde el patio de la celda. Filas de osarios, el encuentro G3 (4), el tercer fragmento y un alijo de sal y botellas (GDD P5). |
| −9 m | Catacumbas | Por un pasillo en rampa desde las criptas. Aquí están el ancla, el ritual con 4 oleadas de 4 (`DIF_Instancia3`) y la Tzitzimime: el techo está a 5 m del piso, justo donde ella cuelga. |

**Huida.** Un túnel de 100 m sube de −9 a 0 hasta la barranca, donde está la salida. La reja del túnel tiene candado y el ritual la abre al completarse. No hay checkpoint en la huida y el derrumbe (deseable) no se hizo.

![Ex-convento: el templo y el coro alto](img/niveles/i3_blockout_templo.png)

| Pieza nueva | Qué hace |
|---|---|
| `RitualController.escapeDoor` (cambio) | Una puerta opcional que el ritual desbloquea y abre al terminar. Es la reja del túnel, igual que la trajinera de huida en la instancia 1. |
| `RitualAtmosphere` | La "niebla roja durante el ritual" del GDD (§9): mientras se reza en el ancla, la niebla se vuelve roja y más densa en 4 s. Al terminar o reiniciar el ritual, regresa a la niebla del nivel. |

**Cómo se probó (Play Mode, `PathWalker` a ×4; cultistas apagados para probar la geometría).**
1. Atrio → rampa → coro alto (+5 m): fragmento 1.
2. Claustro → sala capitular: fragmento 2. Luego la nota del copal.
3. Celda del prior: se guardó con 2 fragmentos.
4. Escalera → criptas (−4 m): fragmento 3.
5. Pasillo → catacumbas (−9 m): las 3 ofrendas en el ancla.
6. Ritual: la Tzitzimime subió al techo a la mitad de la barra. Con copal y linterna se llenó; tomó su forma final, la reja se abrió y la niebla se puso roja durante el rezo.
7. Túnel → barranca: "¡La Tzitzimime ha sido sellada!".

**Con cultistas:** G1, G2 y G3 aparecieron con 4, 5 y 4, y la primera oleada del ritual con 4. Todos estaban sobre el NavMesh en su piso (0, 0, −4 y −9). Consola sin errores.

**Detalles que salieron en la prueba.**
- La rampa de las criptas a las catacumbas pasaba bajo el techo de las catacumbas con 1.5 m libres y nadie cabía. Ahora es un pasillo que baja a lo largo y entra cuando ya está abajo.
- Las bancas del templo estaban pegadas a la rampa del coro y el Padre se atoraba en su orilla. Se separaron 2 m.
- La reja del túnel abría hacia adentro del túnel y lo tapaba (y la Tzitzimime alcanzaba al Padre atorado). Ahora abre hacia las catacumbas.

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
| `SubtitleUI` | prefab `PF_HUD` | `Show(texto)`: lo deja en pantalla según su largo (mínimo 2 s). Lo usan las frases del Padre (`PadreVoice`, `SubtitleTrigger`, #33) y el llanto de la Llorona. |
| `RitualBarUI` | prefab `PF_HUD` | `Begin`, `SetProgress(0–1)` y `End`, para el ritual (#34). |

**Paleta de la interfaz** (*issue #2*). Son 9 colores. La base es el fondo casi negro de los paneles (#0E0F12) y el texto color hueso (#E8E1D3), que se lee bien sin ser blanco puro. El acento es dorado de veladora (#DBAD54) y la vida es rojo sangre (#B3261E), con una alerta más clara (#FF4536). La resistencia es dorada (#C9A327) y el ritual, azul pálido (#8CBFF2). Cada instancia tiene una variante (fondo, tono, niebla y acento) que acompaña al HUD sin cambiarlo. Los colores están en el Inspector de `HUDController` y de los menús, así que se ajustan sin código.

![Paleta](img/bocetos/paleta.png)

**Bocetos** (*issues #6 y #7*). Hay un PNG de 1920×1080 por pantalla, con notas numeradas: HUD, inventario, altar, menú principal, selección de instancia, pausa, opciones, derrota, victoria y créditos. No se dibujaron a mano. En Play Mode se midió la posición, el color y el texto de cada elemento de las pantallas reales (`Tools/Bocetos/layouts/*.json`), y `Tools/Bocetos/dibujar_bocetos.py` los redibuja planos con su leyenda. Por eso el boceto siempre coincide con el juego: si una pantalla cambia, se mide otra vez y se corre el script.

| HUD | Inventario | Altar |
|---|---|---|
| ![HUD](img/bocetos/boceto_hud.png) | ![Inventario](img/bocetos/boceto_inventario.png) | ![Altar](img/bocetos/boceto_altar.png) |

| Menú principal | Selección | Pausa |
|---|---|---|
| ![Menú](img/bocetos/boceto_menu_principal.png) | ![Selección](img/bocetos/boceto_menu_seleccion.png) | ![Pausa](img/bocetos/boceto_pausa.png) |

| Opciones | Derrota | Victoria | Créditos |
|---|---|---|---|
| ![Opciones](img/bocetos/boceto_menu_opciones.png) | ![Derrota](img/bocetos/boceto_derrota.png) | ![Victoria](img/bocetos/boceto_victoria.png) | ![Créditos](img/bocetos/boceto_menu_creditos.png) |

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
- **Créditos:** proyecto, los 6 integrantes del equipo, herramientas, fuente y la nota de respeto a las leyendas.

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
| `InstanceCatalog` | `Data/InstanceCatalog.asset` | La lista de instancias: nombre, descripción y escena. Cada instancia carga su nivel (`I1_Xochimilco`, `I2_Hacienda`, `I3_Convento`); cambiar de escena es cambiar un nombre en el Inspector. |
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
*Feedback visual: #44. Sonido: #115. Pendiente: #50 y las voces grabadas (#8, #9).*

#### Feedback visual y partículas (#44)

**Qué hace.**
- **Daño:** al recibir un golpe, los bordes de la pantalla se ponen rojos (una viñeta, ya no un recuadro completo) y la cámara tiembla. Las dos cosas son proporcionales al daño, así que un drenado pequeño casi no se nota y una embestida sí. Con 30 de vida o menos, los bordes laten en rojo como un corazón.
- **Golpes del Padre:** cada golpe que conecta suelta una ráfaga: rojo oscuro en personas, azul pálido en espíritus.
- **Objetos de fe:**
  - el agua bendita salpica gotas azules;
  - la veladora tiene flama y chispas;
  - la línea de sal destella;
  - la molotov arde con llamas y humo;
  - el copal echa humo de verdad, en lugar de la esfera blanca.
- **Luces de fuego:** todas titilan (veladoras, antorchas, lámparas y velas de las tres instancias y de los objetos).

![Efectos: fuego, humo y veladora · agua bendita · daño · golpe](img/ui/efectos_feedback.png)

**Cómo funciona.**
| Pieza | Dónde | Qué hace |
|---|---|---|
| `PlayerLook.AddShake` | Padre | Un temblor con ruido de Perlin (suave, no saltos al azar) que se apaga con `Time.deltaTime`. Solo mueve la cámara, no hacia dónde mira el cuerpo. |
| `DamageFlashUI` (cambio) | `PF_DamageFlash` | Usa la viñeta `UI_Vineta.png` y llama a `AddShake` en cada golpe. El latido de vida baja usa un seno sobre el tiempo. |
| `HitEffects` | Padre | Escucha `PlayerCombat.HitLanded` y crea `VFX_GolpeHumano` o `VFX_GolpeEspiritu` según el tipo de `Health`. |
| `LightFlicker` | luces de fuego | Cambia la intensidad con ruido de Perlin; cada luz empieza en un punto distinto para que no titilen todas juntas. |
| Partículas | `Prefabs/VFX` y los prefabs de efectos | Sistemas de partículas de Unity con dos materiales: aditivo (fuego, chispas, gotas) y con transparencia (humo, sangre). Todos usan la textura `vfx_punto.png`, un punto suave. |

**Un error viejo que salió en la prueba.** El componente `DamageFlashUI` estaba apagado dentro del prefab desde #13, así que el destello rojo nunca había funcionado en ninguna escena. Se encendió y ahora se ve.

**Cómo se probó (Play Mode, `EnemyArena`).**
- Se crearon el fuego, la veladora, la línea de sal y el copal frente al Padre, y una salpicadura de agua bendita.
- Al recibir 25 de daño, la viñeta llegó a 0.78 de opacidad y la cámara tembló (inclinación de 11.5°, que se recupera).
- Un golpe de machete a un cultista soltó la ráfaga roja.
- En una sesión limpia no hubo errores nuevos en la consola.

#### Sonido sintetizado y música dinámica (#115)

**Qué hace.** Todo el juego suena, y casi todo el sonido está hecho **con código**, sin grabaciones. Son 53 archivos:
- **Armas:** el disparo y la recarga de la escopeta, el clic vacío, el machetazo y el golpe al conectar, la chancla y la cruz.
- **El Padre:** pasos en tierra, madera y piedra (4 de cada uno, para que no se repitan), un quejido al recibir daño, el latido con 30 de vida o menos y el jadeo al quedarse sin resistencia.
- **Objetos y altar:** la campanita del altar, el rezo (un coro bajo que crece mientras se mantiene R) y su final, el agua bendita, el cerillo de la veladora, el vidrio que se rompe, el fuego de la molotov, la sal, la venda y las puertas que rechinan.
- **Enemigos:** el cultista avisa con un **silbato de la muerte** azteca, un sonido real de la época mexica que suena a grito humano. La Llorona llora y grita, el caballo del Charro relincha y la Tzitzimime chilla con voces que desafinan entre sí.
- **Ambiente:** cada instancia tiene su fondo en loop. En Xochimilco hay grillos, ranas, agua y viento; en la hacienda, viento seco y cuervos; en el convento, un zumbido de piedra y gotas con eco, y además cada antorcha chisporrotea.
- **Música:** menú, tensión, ritual, aviso de horda, victoria y derrota (el corazón se detiene).

**Como en Left 4 Dead, la música avisa.** Normalmente solo se oye el ambiente. Cuando un cultista **persigue o ataca** al Padre, entra la música de tensión (cuerdas graves a 120 pulsos por minuto que chocan medio tono) y se va 4 s después de que lo pierden. Cuando empieza un encuentro suena el aviso de horda, unos metales que crecen y un tambor. Durante el ritual suenan tambores huehuetl, un teponaztli y un coro. Al morir, todo se apaga y suena la música de derrota.

**Cómo funciona.**
| Pieza | Dónde | Qué hace |
|---|---|---|
| `Tools/Audio/sintetizar_sonidos.py` | herramientas | Genera los 53 WAV con numpy. Usa la síntesis clásica: ruido filtrado (viento, pasos, disparo), senos con envolvente (golpes, latido), parciales inarmónicos (campanas, metal) y una reverberación hecha por convolución con ruido que se apaga (cuartos, eco del convento). Los loops cruzan su final con su inicio para no tener corte. Con la misma semilla salen los mismos archivos. |
| `MusicDirector` | objeto `Audio_Director` de cada nivel | Cuatro capas: ambiente, tensión, ritual y horda. Cada medio segundo revisa si algún `CultistAI` está en `Chase` o `Attack`, escucha `Encounter.Started` y la etapa del `RitualController`, y sube o baja cada capa con `Mathf.MoveTowards` (fundido, nunca un corte). |
| `PlayerFootsteps` + `FootstepSurface` | Padre / pisos | Un paso cada 1.6 m caminando (2.3 m corriendo), medido por distancia, así que al estar quieto no suena. Un rayo hacia abajo busca `FootstepSurface` en el piso; si no hay, usa el suelo por defecto de la escena (piedra en el convento). Nunca repite el mismo clip seguido y varía un poco el tono. |
| `PlayerAudio` | Padre | Quejido con `Health.Damaged`, latido y jadeo en loop con fundido, y el sonido de cada objeto usado (`ItemDefinition.useClip`, avisado por el nuevo evento `Inventory.Used`). |
| `ThrownFlask.breakClip` | frascos | El vidrio al romperse, con `PlayClipAtPoint` porque el frasco se destruye. |
| `AudioVolume` | cada fuente | Las fuentes siguen el volumen de Efectos o Música de las Opciones. |

**Cómo se probó (Play Mode, Xochimilco y convento).**
- **Ambiente:** suena desde el inicio.
- **Pasos:** al caminar con W sonaron con tonos distintos (0.97, 1.01, 1.06…).
- **Persecución:** con un cultista persiguiendo, la tensión subió a su volumen y sonó la horda.
- **Poca vida:** al bajar a 25 de vida empezó el latido.
- **Convento:** el suelo por defecto es piedra y las 12 antorchas suenan.
- **Consola:** sin excepciones.
- **Archivos:** un script revisó que ningún WAV salga en silencio y que los loops cierren sin salto.

**Lo que no se puede sintetizar** (queda en los issues #8 y #9 para grabarlo o conseguirlo con licencia):
- las voces con palabras: el Padre, el "¡Ay, mis hijos!" de la Llorona y los cultistas gritando;
- el rezo en latín;
- las risas de las muñecas.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿De dónde salieron los sonidos? | Los generó un script de Python con fórmulas de síntesis; no se descargó nada, por eso la licencia es propia. Se pueden cambiar y volver a generar. |
| ¿Cómo sabe la música cuándo ponerse tensa? | `MusicDirector` revisa el estado de los cultistas (`CultistAI.Current`). Si alguno persigue o ataca, sube la capa de tensión; si no, baja. |
| ¿Por qué los pasos van por distancia y no por tiempo? | Así coinciden con la velocidad (corriendo suenan más seguido) y no suenan al estar quieto o rezando. |
| ¿Por qué un silbato de la muerte para el cultista? | Es un instrumento mexica real usado en rituales, encaja con la secta y da miedo. Además se puede sintetizar con ruido filtrado, cosa que un grito humano no. |

### 5.16 Arte: modelos, animaciones e iluminación
*Cultista: issues #3 y #4. La Llorona: issues #17 y #18. El Charro Negro y su caballo: issues #17 y #37. La Tzitzimime: issues #17 y #41. Objetos clave: #29. Props: #19. Brazos del Padre: #28. Pendiente: iluminación, texturas (#101), cielo (#102) y ambiente (#103).*

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

**Los objetos clave** (*issue #29*) se hicieron con el mismo método, pero sin esqueleto. `Tools/Blender/lowpoly_props.py` tiene las piezas para objetos fijos: revolución, cilindro, caja, esfera, prisma y "casco convexo" para piedras. `obj_objetos_clave.py` arma los tres:

| Objeto | Tamaño | Cómo se reconoce de lejos | Triángulos |
|---|---|---|---|
| Muñeca ofrenda (`obj_muneca.fbx`) | 0.40 m | Porcelana blanca sucia, pelo negro largo y listón rojo, como las muñecas colgadas de Xochimilco; brilla blanco | ~700 |
| Moneda del pacto (`obj_moneda.fbx`) | Ø 0.26 m en el juego | Oro viejo con la estrella de ocho puntas del culto en rojo por ambas caras; brillo dorado | ~300 |
| Fragmento de obsidiana (`obj_obsidiana.fbx`) | 0.38 m en el juego | Esquirla negra brillante con vetas rojas que emiten luz; brillo rojo | ~70 |

![Objetos clave](img/props/objetos_clave.png)

En Unity cada uno tiene su prefab (`Prefabs/Items/ObjetosClave/PF_ObjetoClave_*`) con:
- `KeyItemPickup`;
- un collider fijo para que F siempre lo encuentre;
- una luz pequeña de su color;
- `PickupSpin`, que gira el modelo y lo hace flotar 5 cm para que llame la atención en la oscuridad.

Al tomarlo se apagan el modelo, el collider y la luz. Los mismos modelos aparecen sobre cada ancla al colocar las ofrendas. Las 10 muñecas colgadas de Xochimilco también son ahora el modelo de la muñeca.

![Muñeca a 8 m](img/props/muneca_8m.png)

**Los props de crafteo y combate** (*issue #19*) salen de `Tools/Blender/obj_props.py`. Son 11 modelos de 40 a 260 triángulos, a tamaño real:
- machete;
- escopeta de dos cañones;
- tres botellas que se distinguen por el color del vidrio: verde vacía, azul pálido con una cruz para el agua bendita y ámbar con trapo para la molotov;
- veladora roja con flama;
- costal de sal;
- cruz de plata;
- chancla;
- sahumerio de copal con brasas;
- venda.

![Props](img/props/props.png)

**Dónde se usan.**
- **En el suelo:** `ItemDefinition` tiene un campo nuevo, `model`. `PickupItem` oculta su caja y muestra ese modelo un poco más grande (×1.4) para que lo pequeño se vea. Así un solo prefab de pickup sirve para todo: alijos, botín de cultistas y objetos tirados.
- **Efectos:** el frasco que se lanza (agua bendita o molotov) es la botella, y la veladora colocada y el sahumerio del copal usan sus modelos.
- **Primera persona:** la escopeta es el modelo nuevo. El machete, la cruz y la chancla se verán en la mano cuando existan los brazos del Padre (#28).

![Escopeta, veladoras y copal en el juego](img/props/escopeta_vela_copal.png)

**Detalles que salieron en la prueba.**
- La linterna estaba en el mismo lugar que la escopeta (abajo a la derecha) y le daba de frente a centímetros: la culata salía blanca. Ahora la linterna va a la izquierda (la mano libre) y la escopeta es de metal mate.
- El humo del copal era una esfera blanca opaca que tapaba la vista; ahora es semitransparente. El efecto final del humo queda para #44.

**Los brazos del Padre** (*issue #28*, `Tools/Blender/chr_brazos_padre.py`) son el único modelo que el jugador ve todo el tiempo:
- mangas de sotana negra con puño blanco y un rosario de madera en la muñeca izquierda; desde el issue #111 las manos son **cubos sin dedos** (un estilo low-poly más simple que se lee mejor de lejos; ~780 triángulos);
- se modelaron **desde la cámara**: la cámara está en el origen y los hombros quedan justo detrás y abajo de ella, así que en Unity el modelo cuelga de la cámara sin acomodarlo;
- esqueleto Generic de 8 huesos, uno de ellos `RightGrip` en la palma, donde se sujeta el arma;
- 5 clips: `Idle` (respiración), `Attack` (sube el brazo y corta en diagonal), `Pray` (manos juntas), `Throw` (atrás del hombro y al frente) y `PickUp` (se estira al frente).

![Brazos en el juego](personajes/img/brazos_juego.png)
![Machete, ataque, cruz y chancla en la mano](personajes/img/brazos_armas.png)
![Las 5 poses vistas desde la cámara (render de Blender)](personajes/img/brazos_hoja.png)

| Pieza | Dónde | Qué hace |
|---|---|---|
| `AC_BrazosPadre` | `Art/Animations` | `Idle` ↔ `Pray` con el bool `Praying`. Los triggers `Attack`, `Throw` y `PickUp` saltan desde cualquier estado y regresan a `Idle`. |
| `FirstPersonArms` | cámara del Padre | No decide nada, solo escucha lo que otros sistemas ya avisan: `PlayerCombat.Swung` → `Attack`; `PlayerPrayer.ProgressChanged` y R en el ritual → `Praying`; `ItemUser.Threw` → `Throw`; `ItemUser.Placed` e `Interactor.Interacted` → `PickUp`. Pone en la mano el `viewModel` del arma cuerpo a cuerpo y se oculta con la escopeta, que tiene su propio modelo. |
| `MeleeWeaponDefinition.viewModel` | `WPN_*` | El modelo en la mano: `PF_Vista_Machete`, `PF_Vista_Cruz` y `PF_Vista_Chancla`. |
| Eventos nuevos | `Interactor`, `ItemUser`, `ThrowEffect`, `PlaceEffect` | Avisos de "usó F", "lanzó" y "colocó", para que los brazos reaccionen sin que esos scripts conozcan a los brazos. |

**Detalles que salieron en la prueba.**
- En reposo, las manos apenas asomaban abajo, y el ataque y el recoger sacaban el brazo del cuadro: el hombro está debajo de la cámara, así que el antebrazo tiene que subir para verse. Se subieron los brazos (las manos quedan unos 18° abajo del centro) y se ajustaron las poses viendo cada cuadro desde la cámara.
- **Issue #111:** el machete se alargó a 0.85 m (hoja de 0.66 m) y llega a 2.4 m; la escopeta en primera persona se ve un 20 % más grande.
- **Issue #113:** aún parecía cuchillo porque la hoja apuntaba al frente y se veía de canto. Ahora mide 1.2 m, se ensancha hacia la punta, va casi vertical y muestra su cara plana a la cámara (un script calculó el giro desde la cámara). Golpea cada 0.4 s y llega a 2.8 m. La escopeta se ve 2.4 veces su tamaño y más cerca de la cámara.
- **Issue #117:** vertical y de frente parecía una espada enorme. Ahora va **inclinado** hacia el centro de la pantalla (arriba, a la izquierda y un poco al frente), con el mango dentro del puño y al 70 % de tamaño. Un script en Play Mode calculó la rotación desde la cámara: primero apunta la hoja en esa dirección, luego la gira sobre su eje para mostrar la cara plana, y coloca el centro del mango en el hueso `RightGrip`.

![Machete inclinado: reposo y golpe](img/ui/machete_agarre.png)

![Puño, machetazo y escopeta nueva](img/ui/armas_nuevas.png)

- Las armas salían apuntando a un lado. En vez de adivinar ángulos, un script mide el eje largo de cada modelo y lo gira hacia la dirección deseada vista desde la cámara: el machete arriba y al frente, la cruz casi vertical.

**Cómo se probó.** En Blender se renderizaron cuadros de cada animación desde el FBX exportado. En Unity, el avatar es Humanoid válido con los 19 huesos mapeados, el personaje mira hacia +Z, y en Play Mode pasa por `Locomotion`, `Attack` (0.87 s) y `Death` (termina tirado en el suelo) sin errores.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Por qué no usaron Mixamo como decía el plan? | Mixamo pide cuenta de Adobe y subir el modelo; con código el esqueleto y las animaciones son nuestros, se regeneran y no dependen de un servicio. |
| ¿Qué es un rig? | El esqueleto: huesos dentro del modelo. Al girar un hueso, la parte del modelo pegada a él se mueve. |
| ¿Cómo sabe el brazo cuándo atacar? | `FirstPersonArms` escucha el evento `Swung` de `PlayerCombat` y activa el trigger `Attack` del Animator. El combate no sabe que existen los brazos. |
| ¿Por qué los brazos desaparecen con la escopeta? | La escopeta ya tiene su propio modelo en primera persona; mostrar ambos taparía la pantalla. |
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

#### Voces y sonidos que faltaban (#123)

**Qué hace.** El Padre ya habla: sus 24 frases suenan con voz cuando salen los subtítulos, y también con los subtítulos apagados. Además:
- **Rezo:** al rezar murmura en latín sobre el coro.
- **La Llorona:**
  - al salir del agua llama «¡Ay, mis hijos!»: lejos con eco o cerca susurrado, según dónde esté el Padre;
  - cuando lo persigue, lo grita.
- **El cultista:** gruñe en cada machetazo y grita al morir.
- **El caballo del Charro:**
  - galopa cuando embiste y va al paso cuando lo sigue;
  - las espuelas suenan al embestir;
  - quieto, no suena.
- **La Tzitzimime:** vibra cuando está cerca.
- **Xochimilco:** en la chinampa de las muñecas se oyen risas de niña, lentas y desafinadas; a lo lejos suenan campanas de pueblo, y el remo de la trajinera suena mientras avanza.
- **Hacienda:** la campana de la capilla dobla de vez en cuando, y las monedas tintinean al recogerlas.
- **Convento:** junto al ancla canta la secta, y una campana muy grave marca el inicio del ritual.

**Cómo se hicieron las voces.** No hay grabaciones: se generan con código y Jesús las aprobó una por una en una página para escucharlas.

| Herramienta | Qué hace |
|---|---|
| **espeak-ng** | Programa que convierte texto en voz. Decide los sonidos, su duración y la melodía de cada frase. |
| **MBROLA**, voz **mx2** | Pone el timbre de una voz mexicana grabada en pedacitos. |
| `Tools/Audio/sintetizar_faltantes.py` | Lo demás: la voz cansada del Padre, que sostiene la última palabra de cada frase; la "melodía viva", con variaciones de tono y ritmo para que no suene a robot; las copias desafinadas de la Llorona y las muñecas; el eco y la distorsión. |

La risa de las muñecas se escribió sonido por sonido (duración y tono), con la voz más aguda y la garganta más chica para que suene a niña. Los efectos (galope, campanas, remo…) se sintetizan igual que en #115.

**Cómo funciona.**
| Pieza | Dónde | Qué hace |
|---|---|---|
| `VoiceLines` | `Data/Textos/VoiceLines.asset` | Lista de frase → audio. `SubtitleUI.Show` busca ahí el texto y reproduce su voz, así `PadreVoice` y los `SubtitleTrigger` no cambiaron. |
| `AmbientOneShots` | objetos `Audio_123` de cada nivel | Cada cierto tiempo al azar (por ejemplo, de 15 a 30 s) toca un sonido de su lista, sin repetir el mismo dos veces, y solo si el Padre está cerca. |
| Cambios en entidades | `LloronaEntity`, `CultistAI`, `CharroNegroEntity`, `PlayerPrayer`, `TrajineraRide`, `MusicDirector` | Cada script reproduce su sonido en el momento que ya conocía: al salir del agua, al atacar, al morir, según la velocidad del caballo, al rezar, mientras la trajinera avanza, al empezar el ritual. |
| Pasos en agua | `FootstepSurface.Water` | Listos para cualquier charco u orilla pisable; hoy no hay agua donde caminar. |

**Cómo se probó (Play Mode).**
- **Voz del Padre:** «Levántate, Anselmo» sonó con su voz. Todas las frases de `PadreLines` y de los letreros encontraron su audio.
- **Cultista:** al atacar sonó el gruñido y al morir su grito.
- **Charro:** galope al embestir, herraduras al seguir y silencio al detenerse.
- **Llorona:** su llamado al salir del agua.
- **Trajinera:** el remo suena solo mientras avanza. Había dos trajineras y al principio solo una tenía remo; se corrigió.
- **Consola:** sin errores del juego.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Cómo sabe el juego qué audio va con cada frase? | `VoiceLines` guarda pares texto-audio. Cuando se muestra un subtítulo, se busca su texto sin las comillas « ». |
| ¿Qué es MBROLA? | Un sintetizador de voz de la Universidad de Mons que arma la voz con pedacitos de grabaciones reales. espeak le dice qué decir y con qué melodía. |
| ¿Por qué la voz suena un poco irregular? | A propósito: una voz real nunca sostiene un tono exacto ni un ritmo de metrónomo. Se agregaron pequeñas variaciones al azar. |

#### Texturas de los niveles (#101)

**Qué hace.** Los pisos y paredes de las tres instancias ya no son colores planos. Hay tierra con piedritas en los caminos, pasto en las chinampas, tablones en la madera, adobe con aplanado caído en la hacienda, mampostería de piedra en el convento y las tumbas, lajas en el piso del convento y ondas en el agua. Cada textura trae un **mapa de normales**, así que la linterna marca las juntas y las vetas.

![Texturas: color arriba, relieve abajo](img/texturas.png)
![Xochimilco, hacienda y convento con texturas](img/niveles_texturas.png)

**Cómo funciona.**
| Pieza | Dónde | Qué hace |
|---|---|---|
| `Tools/Texturas/generar_texturas.py` | herramientas | Genera 7 texturas de 512 × 512 con numpy. Usa ruido fractal hecho en frecuencias (se repite sin costura), Voronoi que da la vuelta (piedras, grietas) y patrones de ladrillos y tablones. De cada una sale el color y el mapa de normales, calculado desde una altura. |
| Color casi blanco | materiales `M_BO_*` | La textura **multiplica** el color del material, así que cada material conserva su color de la paleta y solo gana detalle. |
| `WorldUVBaker` | `Assets/_Project/Editor` (menú *Ánimas → Texturas*) | Los niveles están hechos con cubos de Unity, cuyos UV van de 0 a 1 en cada cara sin importar su tamaño: la textura se estiraría en un muro de 20 m. La herramienta crea una copia de la malla con UV **en metros**: cada cara usa los dos ejes que cubre (proyección de caja). La repetición del material dice cuántos metros abarca una copia de la textura (2 a 3 m). Las copias se reutilizan por tamaño: 167 mallas para 309 objetos. |

**Cómo se probó.**
- **Play Mode:** se recorrieron las tres instancias sin errores en la consola.
- **FPS:** en el Editor, el promedio fue de 52 en Xochimilco y 59 en hacienda y convento. El costo lo ponen los modelos de vegetación, no las texturas. La medición formal en el build es el issue #65.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿De dónde salieron las texturas? | Las generó un script con fórmulas de ruido; la licencia es propia. |
| ¿Por qué no se estiran en las paredes largas? | Porque `WorldUVBaker` les da UV medidos en metros. Si un muro mide el doble, la textura se repite el doble de veces. |
| ¿Qué es un mapa de normales? | Una imagen que le dice a la luz hacia dónde apunta cada punto de la superficie. Así una pared plana parece tener juntas y relieve sin agregar polígonos. |

#### Cielo de cada instancia (#102)

**Qué hace.** El fondo ya no es negro: cada instancia tiene su cielo de 360°.
- **Xochimilco:** noche fría con una luna grande y pálida entre nubes delgadas, y estrellas.
- **Hacienda:** atardecer que se apaga, rojo y naranja hacia el sol, violeta arriba, y la noche que ya llega por el otro lado.
- **Ex-convento:** noche cerrada con nubes pesadas, estrellas rojizas entre los huecos y una **estrella negra de ocho puntas** con borde rojo, el símbolo de la secta.
- **Catacumbas:** no se ve el cielo porque tienen techo.

![Cielos: Xochimilco, hacienda y convento](img/cielos.png)

**Cómo funciona.**
| Pieza | Dónde | Qué hace |
|---|---|---|
| `Tools/Texturas/generar_cielos.py` | herramientas | Genera tres panorámicas de 4096 × 2048 (latitud-longitud) con numpy: degradados por altura, nubes de ruido fractal que da la vuelta sin costura, estrellas, y la luna o el sol. |
| La luna o el sol en su lugar | el mismo script | Se dibujan exactamente en la dirección de donde viene la luz direccional de la escena (`Moon`, `Sol_Atardecer`, `Luna`). Así el brillo del cielo y las sombras coinciden. |
| Horizonte = color de la niebla | el mismo script | La niebla de Unity no toca el cielo. Por eso, debajo del horizonte y unos grados arriba, el cielo es **exactamente** el color de la niebla de esa escena y se mezcla hacia arriba: el borde de la niebla se funde con el cielo y no queda un corte. |
| `M_Cielo_*` | `Art/Materials` | Materiales con el shader `Skybox/Panoramic` de Unity, puestos en *Lighting → Skybox* de cada escena. La cámara del Padre limpia con *Skybox* en los tres niveles. |

**Cómo se probó.**
- **Play Mode:** en cada nivel se miró hacia la luna, el sol o la estrella negra. Se ven en su lugar y el horizonte se funde con la niebla.
- **Consola:** sin errores.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Qué es un skybox? | Una imagen que Unity dibuja detrás de todo, como si el mundo estuviera dentro de una esfera pintada. |
| ¿Por qué ya no se ve el corte negro? | Porque la parte baja del cielo tiene el mismo color que la niebla: donde la niebla termina, empieza un cielo del mismo color. |
| ¿Cómo sabían dónde poner la luna? | Con la rotación de la luz direccional de la escena: la luna va en la dirección opuesta a hacia donde apunta la luz. |

#### Animaciones ambientales (#103)

**Qué hace.** Los niveles ya no están congelados:
| Instancia | Qué se mueve |
|---|---|
| Xochimilco | Las muñecas y las cabezas colgadas se mecen en sus cuerdas, el pasto y el tule se doblan con ráfagas de viento, los árboles se balancean apenas y el agua de los canales corre. |
| Hacienda | El pasto seco, el agave y los árboles se mueven con el viento, y hay polvo ocre que el viento arrastra. |
| Ex-convento | Polvo flotando en el aire (brilla con la linterna), flamas en las 12 antorchas y el pasto del jardín que se mueve. |
| Las tres | La niebla baja ya no está quieta: avanza despacio con el viento. Las luces de fuego titilan desde el #44. |

![Xochimilco, hacienda y convento con ambiente animado](img/ambiente_animado.png)

**Cómo funciona.**
| Pieza | Dónde | Qué hace |
|---|---|---|
| `AmbientSway` | objeto `Ambiente_Animado` de cada nivel | **Un solo** componente mueve un grupo entero, por ejemplo 1 171 matas de pasto en Xochimilco, en un solo `Update`, en vez de un script por objeto. Al iniciar busca los objetos cuyo nombre de malla o material empieza con lo indicado ("Pasto", "MunecaColgada", "M_BO_Agave") y guarda su rotación original. Cada cuadro gira solo los que están a menos de 45 m del Padre (la niebla oculta el resto): un empuje en la dirección del viento más un temblor más chico y rápido de lado, para que no parezca un péndulo perfecto. Con `wave` = 1 la fase depende de la posición, así el viento pasa como ola por el campo; las muñecas usan fases al azar. |
| `TextureScroll` | agua de Xochimilco | Desliza poco a poco la textura de ondas (#101), así el agua parece correr. Usa una copia del material para no modificar el archivo mientras se juega. |
| Partículas de polvo | `Polvo_Viento`, `Polvo_Flotante` | Sistemas de partículas en espacio de mundo que siguen al Padre con el mismo componente de la niebla (`GroundFog`): siempre hay polvo cerca y se queda en su lugar al caminar. |
| Flamas de antorcha | antorchas del convento | Copia de la flama de la veladora (#44). |
| Niebla que avanza | `PF_NieblaBaja` | Se activó la velocidad de las partículas hacia la dirección del viento. |

**Cómo se probó (Play Mode).**
- **Objetos que se mueven:** en Xochimilco, 1 171 de pasto, 111 muñecas y 169 árboles; en la hacienda, 750 de pasto, 45 agaves y 79 árboles; en el convento, 260 de pasto y 20 árboles.
- **Prueba de movimiento:** en un segundo el agua avanzó (desplazamiento de 0.027 a 0.056) y la muñeca más cercana cambió de ángulo.
- **FPS:** con el ambiente animado encendido y apagado dio 60 en las tres instancias. Es el tope de la sincronía vertical, así que el costo no se nota a ese nivel, pero no sabemos cuánto margen queda. La medición en el build es el issue #65.
- **Consola:** sin errores.

**Preguntas probables.**
| Pregunta | Respuesta |
|---|---|
| ¿Por qué un solo script para todo el pasto? | Unity llama a `Update` de cada componente por separado; con más de mil matas son más de mil llamadas por cuadro. Un administrador hace un solo recorrido por una lista. |
| ¿Por qué solo se mueve lo cercano? | Lo lejano está tapado por la niebla; moverlo sería trabajo que nadie ve. |
| ¿Por qué el pasto se mueve como ola? | La fase de cada mata depende de su posición en la dirección del viento: las vecinas se mueven casi juntas y la ráfaga se ve pasar. |

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
