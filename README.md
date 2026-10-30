# Ánimas mexicanas

Juego de terror en primera persona para un jugador, hecho en Unity para la integradora de Videojuegos. El Padre Anselmo Quiroz, un exorcista retirado, tiene que escapar de tres lugares malditos de México: Xochimilco (La Llorona), una hacienda del Bajío (el Charro Negro) y un ex-convento (la Tzitzimime). Entrega a mediados de diciembre de 2026.

## Versión de Unity
Todos usan la misma: **Unity 6.3 LTS `6000.3.27f1`** con el módulo **Windows Build Support (Mono)** (es la que dice `ProjectSettings/ProjectVersion.txt`). Plantilla de inicio: Universal 3D (URP).

Instalación con Unity CLI:
```bash
unity install 6000.3.27f1 --module windows-mono --accept-eula
```

Paquetes además de la plantilla: AI Navigation, ProBuilder y Pipeline (para que Claude controle el Editor con `unity command`). La escena de prueba está en `Assets/_Project/Scenes/Test/Sandbox.unity`.

## Qué hay en el repo
```
├─ CLAUDE.md                 → contexto para Claude Code (lo lee solo)
├─ .mcp.json                 → servidor de Blender MCP para Claude Code
├─ .gitattributes            → Git LFS y fusión de YAML de Unity
├─ .gitignore                → carpetas de Unity que no se suben
├─ .claude/commands/         → comandos: /siguiente-issue, /revisar-rubrica, /importar-asset
└─ docs/
   ├─ GDD.md                 → diseño, fuente de verdad (v2.2)
   ├─ DEFENSA.md             → explicación del proyecto para la defensa (léanla todos)
   ├─ PENDIENTES_DISENO.md   → registro de decisiones de diseño
   ├─ RUBRICA.md             → rúbrica del curso
   ├─ ARQUITECTURA_UNITY.md  → carpetas, scripts y principios de código
   ├─ PIPELINE_ASSETS.md     → cómo hacer e importar modelos y audio
   ├─ HERRAMIENTAS_MCP.md    → instalación de Unity CLI/MCP, Blender MCP, Claude
   ├─ PLAN_IA.md             → quién usa qué herramienta y el ciclo de trabajo
   ├─ PROMPTS.md             → prompts para copiar
   └─ LICENCIAS.md           → registro de licencias de assets
```

## Primeros pasos
1. Instala Git LFS y clona el repo (`git lfs install` antes de clonar).
2. Sigue `docs/HERRAMIENTAS_MCP.md` para instalar Unity, Unity CLI, Blender MCP y configurar Claude. Protege `main` en GitHub.
3. En Claude Desktop crea un Project con los documentos de `docs/` y pega las instrucciones que vienen en `HERRAMIENTAS_MCP.md`.
4. Abre Claude Code en el repo y escribe `/siguiente-issue Sprint 3`. Quien no use Blender puede rechazar el servidor `blender` cuando Claude Code pregunte por el `.mcp.json`.
5. Si cambia el diseño, se actualiza `docs/GDD.md` (es la fuente de verdad para la IA) y la duda o decisión se anota en `docs/PENDIENTES_DISENO.md`.

## Defensa
Todo el trabajo lo hace Jesús con Claude. Para la defensa individual, **cada integrante lee `docs/DEFENSA.md`** y practica con el prompt #9 de `docs/PROMPTS.md`.

## Reglas del repo
- Un issue = una rama = un pull request; nada se sube directo a `main`.
- Todo asset externo se anota en `docs/LICENCIAS.md` al descargarlo.
