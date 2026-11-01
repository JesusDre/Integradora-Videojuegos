# Ánimas mexicanas — contexto para Claude

Juego de terror en primera persona, **un jugador**, hecho en **Unity** para un curso. Un sacerdote exorcista (Padre Anselmo Quiroz) debe escapar de 3 lugares malditos de México. Equipo de 6 personas, pero **todo el trabajo (código, arte, audio, UI, niveles y textos) lo hace Jesús contigo**; los demás integrantes solo presentan la defensa individual, apoyados en `docs/DEFENSA.md`. Entrega a mediados de diciembre de 2026.

Antes de trabajar, lee `docs/GDD.md` (diseño), `docs/RUBRICA.md` (cómo se califica) y `docs/ARQUITECTURA_UNITY.md` (cómo se organiza el código). Si algo contradice al GDD, gana el GDD; si el GDD es ambiguo, **pregunta antes de inventar**. Las decisiones abiertas están en `docs/PENDIENTES_DISENO.md`: si un issue toca una marcada "(Pendiente Pn)" en el GDD, dilo y pregunta antes de implementar esa parte.

## Idioma y estilo
- Responde en español. Código, nombres de clases y comentarios técnicos en inglés; textos del juego en español.
- Código simple y legible: **cada integrante** tiene que explicar el proyecto en una defensa individual, aunque no lo haya escrito. Prefiere soluciones claras a trucos. Comenta el porqué, no el qué.
- Cambios pequeños y revisables: un issue = una rama = un pull request.

## Alcance (no lo amplíes)
**MVP:** 3 instancias de ~10 minutos (Xochimilco/La Llorona, Hacienda/Charro Negro, Ex-convento/Tzitzimime), cada una con 2 secciones y 1 cuarto seguro en medio, ritual final (en la hacienda, en su lugar, el enfrentamiento con el Charro Negro: barra de vida que solo baja con daño bendito, el triple cuando cruza el agua bendita) y huida.
**Armas:** machete, chancla, cruz, escopeta, pistola, metralleta, ballesta y martillo chillón (#134, #135).
**Enemigos menores (#172–#174, pasados del alcance Extra por Jesús el 31 oct 2026):** muñecas poseídas en Xochimilco (solo se mueven si no las miras), chaneques en la hacienda y Xochimilco (risa que delata su cercanía; roban un objeto), arañas del techo en el ex-convento (levantan al Padre).
**Sigilo (#136):** Ctrl agacharse; los cultistas lo ven solo a la mitad de su rango y un ojo en el HUD avisa si está oculto, si lo buscan o si lo ven.
**No existen:** multijugador, empujar, salto, construcción de base, hambre/sed, vehículos (la trajinera es un recorrido por guion sobre una ruta fija; el jugador no la maneja), árbol de habilidades.
**Deseable (no lo programes salvo que se pida):** Director de ritmo, gamepad, Vendajes+, 3 slots de guardado, modos Fácil/Difícil, derrumbe del convento.
**Extra (no lo programes):** nahual, medidor de fe, final alternativo.

## Números del diseño (valores iniciales, ajustables en playtest)
Vida 0–100 sin regeneración · venda cura 30 · altar restaura hasta 60 % · inventario 8 espacios (materiales hasta 99 por tipo, no ocupan espacio) · 4 espacios rápidos · 2 espacios de arma (cualquier tipo) · munición aparte del inventario con límite por arma (escopeta 99, pistola 160, metralleta 360, ballesta 99; sal + normal) · Cultista 40 de vida, 10 de daño, busca 10 s tras perder de vista · rezo: mantener R 1.5 s, círculo 4 m, repele 10 s, escudo de 10 s con +50 % de daño, enfriamiento 20 s · ritual: estar dentro del círculo del ancla llena la barra en 60 s, salir la pausa, golpe de 15+ la hace retroceder 10 % · línea de sal dura 20 s · jugador mide 1.8 m · 1 unidad de Unity = 1 metro.
Resistencia, daño por arma, munición, detección del Cultista y tamaño de encuentros: tabla "Valores iniciales de balance" del GDD (sección 12); van en ScriptableObjects.
Inventario y materiales pasan completos entre instancias. Autoguardado al entrar y al salir del cuarto seguro; al morir se reaparece ahí con 60 de vida (o más si se guardó con más); la huida no tiene checkpoint.

## Controles
WASD mover · Mouse cámara · Shift correr (con resistencia) · Ctrl agacharse · Clic izq. atacar/disparar · Clic der. apuntar (escopeta) · C recargar (escopeta) · Q o rueda cambiar de arma · F interactuar · R rezar (mantener) · E linterna · 1–4 objetos rápidos (se usan al instante; agua bendita y molotov se lanzan hacia donde miras) · Tab inventario · Esc pausa.

## Cómo trabajar en este repo
- El backlog vive en **GitHub Issues** con hitos "Sprint 1…9". Usa `gh issue list --milestone "Sprint N"` y `gh issue view <n>`; no inventes tareas fuera de los issues.
- Antes de tocar una escena, prefab o asset en Unity, revisa si hay un Editor conectado con `unity status`. Si lo hay, usa los comandos del Editor (Unity MCP); no edites a mano `.unity`, `.prefab` ni `.asset`. Si no hay Editor, dilo explícitamente.
- Si hay errores de compilación, el Editor entra en Safe Mode y no responde: arregla los errores de C# primero.
- Configuración obligatoria del proyecto: Asset Serialization = Force Text; Version Control = Visible Meta Files; `.gitignore` de Unity; Git LFS según `.gitattributes`. Si agregas un formato binario nuevo, súmalo a `.gitattributes` en el mismo pull request.
- Nunca subas `Library/`, `Temp/`, `Logs/`, `Obj/` ni `Builds/`. No hagas push a `main`: abre pull request y que lo revise Jesús.
- Todo asset externo lleva su fuente y licencia en `docs/LICENCIAS.md`.
- Cada issue que agregue o cambie un sistema actualiza su sección en `docs/DEFENSA.md` en el mismo pull request: qué hace, cómo funciona (con los scripts reales), por qué se hizo así y qué preguntas podría hacer el profesor. Escríbelo para alguien que no programa.

## Reglas que la rúbrica castiga (revísalas siempre)
- Movimiento con `Time.deltaTime`. Valores (vida, resistencia, inventario, munición) con límites y validaciones.
- Cámara: rotación vertical limitada (no se voltea) y sin atravesar paredes; sensibilidad del mouse ajustable.
- Cada acción importante tiene feedback visual o sonoro.
- Debe existir: menú principal, pausa (Continuar, Reiniciar, Salir al menú), pantalla de derrota con Reintentar, pantalla de victoria o créditos.
- El build debe abrir y llegar a victoria o derrota sin errores.

## Definición de terminado de un issue
Compila sin errores ni warnings nuevos · probado en Play Mode · criterio de finalización del issue cumplido · `docs/DEFENSA.md` actualizado · pull request pequeño y descrito · Jesús puede explicar cada línea.
