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
| `PlayerCombat` | Padre | Al hacer clic revisa la espera y la resistencia (`TrySpend`), lanza un rayo grueso (`Physics.SphereCast`) desde la cámara y le quita vida al primer `Health` que toque. |
| `PlayerDeathHandler` | Padre | Al morir apaga movimiento, cámara, combate, interacción y linterna, y libera el cursor. La pantalla de derrota (#48) escuchará el mismo evento. |
| `HealingPickup` | venda de prueba | Con F cura 30; si la vida está llena no se gasta. Cuando exista el inventario (#14), las vendas irán a los objetos rápidos. |
| `HitFlash` | enemigos | Tiñe de rojo el modelo 0.12 s al recibir daño, con un `MaterialPropertyBlock` (no modifica el material compartido). |
| `DeathAnimation` | enemigos | Al morir dispara la animación `Die` y apaga sus colliders para que el cuerpo no estorbe. |
| `DamageFlashUI` | prefab `PF_DamageFlash` (UI) | Imagen roja a pantalla completa que aparece con cada golpe (más fuerte si el golpe es mayor) y se desvanece. |
| `DamageZone` | escena de prueba | Zona que quita 10 de vida por segundo a quien esté dentro (servirá para fuego o agua maldita). |

En `Sandbox` hay un Cultista con 40 de vida detrás de la puerta, una venda sobre la mesa y una zona de daño en la esquina.

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
*Pendiente — issues #14 y #22.*

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
