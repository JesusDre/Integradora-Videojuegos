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
*Pendiente — issue #13.*

### 5.3 Inventario, materiales y objetos rápidos
*Pendiente — issues #14 y #22.*

### 5.4 IA del Cultista
*Pendiente — issue #15.*

### 5.5 Altar y crafteo
*Pendiente — issue #23.*

### 5.6 Escopeta y balas de sal
*Pendiente — issue #24.*

### 5.7 Rezo y ritual
*Pendiente — issues #25 y #34.*

### 5.8 Cuarto seguro, checkpoint y guardado
*Pendiente — issue #26.*

### 5.9 Encuentros, oleadas y dificultad
*Pendiente — issue #27.*

### 5.10 Objetos de fe y sus efectos
*Pendiente — issue #31.*

### 5.11 Objetos clave, notas y recetas desbloqueables
*Pendiente — issue #32.*

### 5.12 Entidades: La Llorona, el Charro Negro y la Tzitzimime
*Pendiente — issues #35, #39 y #45.*

### 5.13 Niveles: las tres instancias
*Pendiente — issues #16, #36, #40 y #46.*

### 5.14 UI: HUD, menús, pausa, derrota, victoria y créditos
*Pendiente — issues #22, #47 y #48.*

### 5.15 Audio y feedback
*Pendiente — issues #44 y #50.*

### 5.16 Arte: modelos, animaciones e iluminación
*Pendiente — issues #4, #18, #28, #37 y #41.*

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
