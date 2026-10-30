# Arquitectura propuesta en Unity

Propuesta inicial para que Claude y Jesús escriban código consistente. Se puede ajustar, pero cualquier cambio se anota aquí.

## Stack
- **Unity 6.3 LTS `6000.3.27f1`** (la de `ProjectSettings/ProjectVersion.txt`, anotada en el README; todos usan la misma).
- **URP** (luces, niebla, post-procesado por Volume).
- Paquetes: Input System, AI Navigation (NavMesh), ProBuilder (blockout), TextMeshPro, Cinemachine solo si hace falta.
- Control de versiones: Git + Git LFS en GitHub (el `.gitattributes` del kit define qué va en LFS y fusiona el YAML de Unity con UnityYAMLMerge).

## Carpetas
```
Assets/
  _Project/
    Art/        Models/ Materials/ Textures/ Animations/ VFX/
    Audio/      Music/ SFX/ Voices/ Mixers/
    Data/       Items/ Recipes/ Encounters/ Difficulty/   (ScriptableObjects)
    Prefabs/    Player/ Enemies/ Items/ Props/ UI/ Levels/
    Scenes/     00_Boot  01_Menu  10_Inst1_Xochimilco  20_Inst2_Hacienda  30_Inst3_Convento  Test/
    Scripts/    Core/ Player/ Items/ Enemies/ Entities/ Levels/ UI/ Audio/ Save/
    UI/         Sprites/ Fonts/
  ThirdParty/   (packs externos, cada uno con su licencia)
```

## Namespaces y scripts principales
| Área | Scripts | Notas |
|---|---|---|
| Core | `GameManager`, `GameState`, `SceneLoader`, `SaveSystem` | Estados: Menu, Playing, Paused, Ritual, Escape, Dead, Victory. Guardado en JSON en `Application.persistentDataPath` |
| Player | `PlayerController` (CharacterController), `PlayerStats`, `Flashlight`, `Interactor`, `PrayerSystem`, `PlayerCombat` | Movimiento × `Time.deltaTime`; resistencia y vida con `Mathf.Clamp` |
| Items | `ItemDefinition`, `RecipeDefinition` (ScriptableObjects), `Inventory`, `AltarCrafting`, `QuickSlots`, `Consumable`, efectos por objeto | 8 espacios; materiales hasta 99 por tipo en un diccionario aparte |
| Enemies | `CultistAI` (NavMeshAgent + máquina de estados), `EncounterTrigger`, `WaveSpawner`, `Health` | Estados: Patrol, Chase, Attack, Search (10 s), Return |
| Entities | `EntityBase`, `LloronaEntity`, `CharroNegroEntity`, `TzitzimimeEntity` | Máquina de estados por fases. La Llorona y la Tzitzimime reutilizan `RitualController`; el Charro Negro se vence con 3 golpes de cruz, no con barra: colocar las monedas en el pedestal de la capilla abre su fase final |
| Levels | `SafeRoom`, `Checkpoint`, `KeyItem`, `Anchor`, `RitualController`, `LevelFlow` | El ritual es genérico: barra, oleadas y retroceso con daño fuerte. Mantener R en el ancla llena la barra sin el enfriamiento del rezo normal (`PrayerSystem` tiene un modo ritual) |
| UI | `HUDController`, `InventoryUI`, `AltarUI`, `PauseMenu`, `MainMenu`, `EndScreens`, `SubtitleUI` | Todo reacciona por eventos, no por polling |
| Audio | `AudioManager` + `AudioMixer` (Master, Music, SFX, Voices) | Loops sin cortes; un `AudioCue` por acción |

## Principios
- **Datos en ScriptableObjects** (objetos, recetas, tablas de dificultad por instancia, encuentros) para balancear sin tocar código.
- **Eventos C#** (`event Action`) entre sistemas; evitar `FindObjectOfType` en `Update`.
- Un MonoBehaviour = una responsabilidad. Nada de "God scripts".
- Valores con límites siempre (`Mathf.Clamp`, validación al recoger/usar).
- Prefabs para todo lo que se repite (Cultista, objetos clave, puertas, altar, cuarto seguro).
- Escenas de prueba en `Scenes/Test/`; nadie rompe las escenas de nivel.
- Dueño por escena: una persona a la vez edita cada escena (se fusionan mal).
- Propuesta para no esperar turnos: cada instancia es una escena base (`10_Inst1_Xochimilco`: jugador, luz, `LevelFlow`) más escenas aditivas por parte (`11_Inst1_Seccion1`, `12_Inst1_CuartoSeguro`, `13_Inst1_Seccion2`). Así dos personas pueden trabajar en la misma instancia a la vez. Si la adoptan, actualicen la lista de escenas de arriba.
- Items: una receta puede pedir materiales **y** objetos del inventario (la chancla bendita usa agua bendita), así que `RecipeDefinition` acepta ambos tipos de ingrediente.

## Instancias (misma estructura)
`Entrada → Sección 1 (2 objetos clave) → Cuarto seguro (altar, checkpoint) → Sección 2 (1 objeto clave) → Ancla → Ritual → Huida → Salida`.
Cambia la entidad, la paleta, los assets y la tabla de dificultad; los sistemas son los mismos.
