# Prompts listos para copiar

Cambia lo que está entre `<...>`. En Claude Code, `CLAUDE.md` ya da el contexto; en Claude Desktop, el Project ya trae los documentos.

## 1. Empezar un issue (Claude Code)
```text
Voy a trabajar el issue #<n> de GitHub. Léelo con `gh issue view <n>`, y lee docs/GDD.md y docs/ARQUITECTURA_UNITY.md.
Antes de escribir código dame un plan corto: qué scripts y prefabs se crean o cambian, qué datos van en ScriptableObjects, cómo lo voy a probar en Play Mode y qué riesgos ves. No cambies nada todavía.
```

## 2. Programar un sistema (Claude Code)
```text
Implementa el plan que aprobé para <sistema, por ejemplo: inventario de 8 espacios>.
Reglas: C# claro y comentado (tengo que explicarlo en una defensa), movimiento con Time.deltaTime, valores con límites y validaciones, eventos C# entre sistemas, datos en ScriptableObjects. Haz pasos pequeños y compila tras cada uno. Al final explícame qué hace cada script y cómo probarlo.
```

## 3. Armar un blockout con el MCP de Unity
```text
Hay un Editor abierto. Revisa con `unity status` y `unity command` qué puedes hacer. Con el plano de la instancia <1/2/3> (imagen adjunta) arma el blockout con cubos o ProBuilder: 2 secciones y el cuarto seguro en medio, a escala (el jugador mide 1.8 m), con los puntos de inicio, los 3 objetos clave, el altar y la salida. Hazlo en la escena Test/<nombre> y dime qué creaste.
```

## 4. Limpiar un modelo con Blender MCP
```text
Hay un Blender abierto con el servidor MCP. Importa <ruta del modelo>. Revisa con `get_scene_info` y `look`: escala (1 unidad = 1 m, altura <1.8 m si es personaje>), origen en la base, orientación, número de triángulos (máximo <15000>) y materiales. Corrige lo que falle, no borres el original, y exporta un FBX a <carpeta> con el nombre <chr_cultista>. Dime qué cambiaste.
```

## 5. Crear un prop con Blender MCP
```text
Crea en Blender un prop low-poly de <veladora en vaso de vidrio> de unos <15 cm> de alto, con pocos polígonos (menos de 2000 triángulos), origen en la base y materiales simples. Búscalo primero en Poly Haven o Poly Pizza con licencia CC0; si no hay, modélalo con formas simples. Muéstrame el resultado con `look` desde 3 ángulos y exporta FBX.
```

## 6. Pasar un boceto de UI a Unity (Claude Code)
```text
Te adjunto el boceto de <HUD / inventario / pausa>. Impleméntalo en Unity con <UGUI o UI Toolkit, según lo que ya use el proyecto> usando la paleta de docs/GDD.md y la del issue #2. Debe reaccionar a eventos del juego (no polling), ser legible y no estorbar. Crea el prefab en Assets/_Project/Prefabs/UI y explícame cómo se conecta.
```

## 7. Revisar un pull request contra la rúbrica
```text
/revisar-rubrica
```
(o, en Desktop: "Revisa este diff contra la rúbrica y el GDD; dime criterios afectados, errores, contradicciones y qué tendría que explicar el autor.")

## 8. Escribir notas del culto (Claude Desktop)
```text
Escribe <6> notas cortas del culto de los Hijos de la Estrella Negra para la instancia <1>, de <40 a 60 palabras> cada una, en español, tono de diario o recado encontrado. Cada nota debe insinuar una pista (símbolo de la estrella, ofrendas, la entidad <La Llorona>) y desbloquear la receta <cruz de plata>. Sin santería ni Santa Muerte; el culto es ficticio. Evita clichés y no uses personas reales.
```

## 9. Practicar la defensa individual (cualquier integrante)
```text
Voy a defender el proyecto "Ánimas mexicanas" aunque no lo programé yo. Te paso docs/DEFENSA.md y docs/GDD.md. Explícame en palabras simples <el sistema o parte que me toca explicar>, y luego hazme 5 preguntas difíciles que podría hacer el profesor, una por una, corrigiéndome si me equivoco. Si te pregunto algo que no está en DEFENSA.md, dímelo para avisarle a Jesús.
```

## 10. Cuando algo se rompe
```text
Esto falla: <error o comportamiento>. Estoy en la rama <rama>, issue #<n>. No cambies código todavía: dime las 3 causas más probables, cómo comprobar cada una en Unity (consola, Inspector, Debug.Log) y cuál revisar primero.
```
