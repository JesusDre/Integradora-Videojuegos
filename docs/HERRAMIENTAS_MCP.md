# Herramientas y cómo instalarlas

Los comandos de Unity CLI y de Blender MCP vienen de su documentación oficial a octubre de 2026. El Unity CLI con MCP está en **beta** y Blender MCP es un proyecto comunitario: si algo cambió, manda lo que diga su README.

## Resumen

| Herramienta | Para qué |
|---|---|
| Git, Git LFS, GitHub CLI (`gh`) | Repo, issues y pull requests |
| Unity Hub + Unity 6 LTS (misma versión) | El juego |
| Unity CLI + MCP de Unity | Que Claude edite escenas, prefabs y GameObjects en el Editor abierto |
| Claude Desktop (Projects) | Diseño, textos, planificación, dudas |
| Claude Code (en el repo) | Código C#, git, `gh`, Unity MCP |
| `uv` + Blender + Blender MCP | Que Claude limpie, modele y exporte assets sin saber Blender |
| Meshy / Tripo (web) + Mixamo | Personajes 3D con rig y animaciones |
| Audacity + bibliotecas de audio | Editar y conseguir sonidos |

Orden de instalación: Git/gh/LFS → Unity → Unity CLI y MCP → Claude Desktop/Code → uv, Blender y Blender MCP → IA 3D.

## 1. Git, LFS y GitHub CLI
```bash
git lfs install
gh auth login
gh auth status
```
En el repo: `.gitignore` de Unity y el `.gitattributes` que trae este kit (LFS para modelos, texturas, HDRI, audio, fuentes y `.blend`; ver el archivo). Si alguien agrega un formato nuevo, se suma ahí **antes** del primer commit de ese archivo.
El plan gratis de GitHub LFS tiene 10 GiB de almacenamiento y 10 GiB de descarga al mes (verifícalo en tu cuenta); si se agota sin método de pago, LFS se desactiva hasta el mes siguiente. Con 6 personas clonando y haciendo pull, la descarga se gasta rápido: texturas comprimidas, música en OGG (no WAV), sin duplicados y sin subir los `.blend` de trabajo salvo que hagan falta.

Para que `merge=unityyamlmerge` funcione, cada persona configura una vez el fusionador que trae Unity (ruta según su instalación; en Windows suele ser `C:/Program Files/Unity/Hub/Editor/<versión>/Editor/Data/Tools/UnityYAMLMerge.exe`):
```bash
git config --global merge.unityyamlmerge.name "Unity SmartMerge"
git config --global merge.unityyamlmerge.driver '"<ruta a UnityYAMLMerge>" merge -p %O %B %A %A'
```
Sin esto, Git usa su fusión normal; no se rompe nada, solo se fusiona peor.

Protege `main` en GitHub (Settings → Branches → Add rule): exigir pull request con 1 aprobación y bloquear el push directo. La regla de `CLAUDE.md` solo se cumple de verdad si GitHub la impone.

## 2. Unity CLI y MCP de Unity (beta)
Instalar el CLI:
```powershell
# Windows (PowerShell)
$env:UNITY_CLI_CHANNEL='beta'; irm https://public-cdn.cloud.unity3d.com/hub/prod/cli/install.ps1 | iex
```
```bash
# macOS / Linux
curl -fsSL https://public-cdn.cloud.unity3d.com/hub/prod/cli/install.sh | UNITY_CLI_CHANNEL=beta bash
```
Abre una terminal nueva y verifica con `unity --version`. Luego, dentro de la carpeta del proyecto:
```bash
unity pipeline install              # agrega el paquete com.unity.pipeline (Unity 6.0+) al manifest
unity mcp configure --list          # clientes soportados y sus rutas
unity mcp configure claude          # registra el MCP de Unity en Claude Desktop
unity mcp configure claude-code     # y/o en Claude Code
```
Con el proyecto abierto en el Editor: `unity status` debe mostrar un Editor en estado `ready`, y `unity command` lista lo que Claude puede hacer.
- Si el proyecto tiene errores de compilación, el Editor entra en **Safe Mode** y no responde: arregla el C# y reinicia Unity (`unity pipeline list` lo confirma).
- Reinicia Claude Desktop después de configurar el MCP.
- Funciona en tu máquina, contra tu Editor abierto; no es acceso remoto.

## 3. Blender y Blender MCP (comunitario)
Es el proyecto de `ahujasid` (antes `blender-mcp`; desde la 2.0 de septiembre de 2026 se llama `mcp-for-blender` y `uvx blender-mcp` sigue funcionando como alias). Repo: https://github.com/ahujasid/mcp-for-blender.
Requisitos: Blender 3.0 o más nuevo, Python 3.10+, `uv` (instalador oficial, no `pip install uv`).
Versión: `uvx mcp-for-blender` baja siempre la última. Para no llevarte sorpresas, elige una versión probada y fíjenla en `.mcp.json` y en Desktop como `"args": ["mcp-for-blender@X.Y.Z"]`.
Nota: el README nombra el botón del addon de dos formas ("Start MCP Server" y "Connect to Claude"); es el mismo paso.
Instalación en un comando (instala uv, configura Claude Desktop y Claude Code, instala y activa el addon):
```powershell
# Windows
irm https://www.mcp-for-blender.com/install.ps1 | iex
```
```bash
# macOS / Linux
curl -LsSf https://www.mcp-for-blender.com/install.sh | sh
# si ya tienes uv:
uvx mcp-for-blender setup
```
Manual: en Claude Desktop → Settings → Developer → Edit Config:
```json
{ "mcpServers": { "blender": { "command": "uvx", "args": ["mcp-for-blender"] } } }
```
En Claude Code: `claude mcp add blender uvx mcp-for-blender` (este kit ya trae un `.mcp.json` con el servidor y `BLENDER_MCP_SAFE_MODE=1`).
Addon: `uvx mcp-for-blender install-addon`, luego en Blender **Edit → Preferences → Add-ons → "Interface: MCP for Blender"**. Panel `N` → pestaña **MCP for Blender** → **Start MCP Server** (o Connect to Claude).
Cierra y abre Claude por completo; verás el icono de herramientas.

Herramientas que expone: `execute_blender_code` (Python en tu Blender), `look` (ver el viewport o una cámara), `get_scene_info`, `generate_3d` (Tripo, Hunyuan3D, Hyper3D Rodin, Meshy), `search_assets`/`import_asset` (Poly Haven, Sketchfab, Poly Pizza).

Problemas típicos:
- `spawn uvx ENOENT`: Claude Desktop no encuentra `uvx`. Usa la ruta completa (`where uvx` / `which uvx`) o en Windows `"command": "cmd", "args": ["/c", "uvx", "mcp-for-blender"]`.
- No conecta: primero abre Blender con el servidor iniciado, y no corras `uvx` a mano. Solo un cliente MCP a la vez (Claude Desktop o Cursor, no ambos).
- Python raro (conda/pyenv): `"args": ["--python", "3.11", "mcp-for-blender"]`.

Seguridad y costos:
- `execute_blender_code` ejecuta Python arbitrario: **guarda el .blend antes** de cada sesión. El modo seguro (`BLENDER_MCP_SAFE_MODE=1`) bloquea archivos, red y procesos; el modelado, los materiales y la importación/exportación siguen funcionando. Si bloquea algo que necesitas, quítalo temporalmente.
- El socket del addon no tiene autenticación: déjalo en `localhost`.
- La telemetría de contenido es opt-in; `DISABLE_TELEMETRY=true` apaga también el registro mínimo (ya viene en el `.mcp.json`).
- `generate_3d` funciona con su servicio Premium o con tu propia clave; la de Meshy requiere un plan de pago de Meshy.
- Poly Pizza: cerca del 69 % de sus modelos son CC-BY (exigen crédito). Filtra por CC0 o anota la atribución.

## 4. Claude Desktop y Claude Code
- **Claude Desktop → Projects:** crea el proyecto "Ánimas mexicanas" y súbele `docs/GDD.md`, `docs/PENDIENTES_DISENO.md`, `docs/RUBRICA.md`, `docs/ARQUITECTURA_UNITY.md`, `docs/PIPELINE_ASSETS.md` y `docs/PLAN_IA.md` como conocimiento. En las instrucciones del proyecto pega el bloque de abajo.
- **Claude Code:** abre la terminal en la raíz del repo y corre `claude`. Lee `CLAUDE.md` solo. Los comandos `/siguiente-issue`, `/revisar-rubrica` y `/importar-asset` están en `.claude/commands/`.

Instrucciones para el Project de Claude Desktop:
```text
Eres el asistente del equipo de "Ánimas mexicanas", un juego de terror en primera persona en Unity para un curso, con entrega a mediados de diciembre de 2026. Responde en español. Usa los documentos del proyecto como fuente: si algo contradice al GDD, gana el GDD; si es ambiguo, pregunta. No amplíes el alcance: el MVP son 3 instancias de unos 10 minutos y no hay multijugador, sigilo, empujar ni vehículos. Prefiere soluciones simples que el equipo pueda explicar en la defensa individual. Cuando propongas números, indica que son valores iniciales a probar.
```

## 5. IA 3D y audio
- **Meshy** y **Tripo** (web): generan modelos 3D desde imagen o texto y exportan FBX/GLB; los precios y límites del plan gratis cambian, revísalos antes de contratar.
- **Mixamo** (cuenta de Adobe): rig y animaciones; en Unity, importa como Humanoid.
- **Audacity** para editar; para conseguir sonidos usa Freesound, Pixabay u OpenGameArt y anota la licencia de cada archivo.
