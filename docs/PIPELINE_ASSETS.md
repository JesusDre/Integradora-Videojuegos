# Pipeline de assets

## Reglas generales
- 1 unidad de Unity = 1 metro. El Padre mide 1.8 m. El Cultista, 1.8 m.
- Formatos: modelos FBX; texturas PNG o JPG (las de Poly Haven pueden venir en JPG o EXR); efectos de sonido y voces en WAV 44.1 kHz 16 bits; música y loops largos de ambiente en OGG para no gastar la cuota de Git LFS. Cualquier formato nuevo se agrega a `.gitattributes` antes de subirlo.
- Convención de nombres (sin espacios ni acentos):
  - Modelos: `chr_cultista`, `ent_llorona`, `prop_veladora`, `env_chinampa_01`
  - Audio: `inst1_llorona_llanto_cerca.wav`, `comun_machete_impacto.wav`
  - Prefabs: `PF_Cultista`, `PF_Altar`
- Presupuesto de polígonos (propuesta; ajustar con pruebas de rendimiento): personajes y entidades hasta ~15 000 triángulos, props hasta ~2 000, piezas de ambiente hasta ~5 000. Texturas hasta 2048 px.
- Materiales con shader URP/Lit. Colliders simples (Box/Capsule/Mesh convexo), nunca Mesh Collider no convexo en objetos móviles.
- Todo asset externo: anotar fuente, autor y licencia en `docs/LICENCIAS.md`. Preferir CC0. Si exige atribución, anotarla para los créditos.

## Personajes y entidades (Cultista, La Llorona, Charro Negro, Tzitzimime)
1. **Concepto**: PNG de frente, postura en T, fondo limpio, sin elementos ocultos.
2. **Generar el modelo 3D** con Meshy o Tripo desde la imagen (o con `generate_3d` de Blender MCP). Pedir postura en T y 1.8 m.
3. **Exportar FBX** con texturas.
4. **Rig y animaciones**: Mixamo (idle, caminar, correr, atacar, morir) o el rig automático de la herramienta.
5. **Importar a Unity**: Rig = Humanoid, revisar escala (Scale Factor), orientación, materiales URP.
6. **Probar**: animaciones en un Animator de prueba, transiciones sin saltos, colliders.
7. **Regla de dos intentos**: si en 2 generaciones no queda limpio, usar un pack gratuito como base. El caballo del Charro Negro (cuadrúpedo) es el más riesgoso: probarlo pronto.
8. Registrar licencia y límites del plan gratis usado. Revisen los términos antes de generar: en planes gratis, algunas herramientas piden atribución o dejan públicos los modelos.

## Brazos del Padre (primera persona)
En primera persona el jugador ve los brazos y lo que tiene en la mano todo el tiempo, y la rúbrica califica animaciones y transiciones.
- Modelo: brazos con mangas de sotana, sin cuerpo completo (`chr_padre_brazos`), hasta ~8 000 triángulos.
- Animaciones mínimas: idle, caminar/correr (balanceo leve), golpe de machete, golpe de chancla, disparo y recarga de escopeta, lanzar (agua bendita, molotov, sal), mantener rezo, sostener linterna.
- Se renderizan con su propia cámara o capa para que no atraviesen paredes.

## Props, objetos clave y ambiente
- Primero buscar en bibliotecas CC0 (Poly Haven, Poly Pizza filtrando por licencia CC0, Kenney, Quaternius). Si no hay, generar con IA o modelar con formas simples por código (Blender MCP).
- Props mínimos: linterna, machete, escopeta, cartucho de sal, botella (agua bendita y molotov), veladora, sal, cruz de plata, chancla, copal, vendas, altar, puerta del cuarto seguro, trajinera.
- Objetos clave: muñeca ofrenda, moneda del pacto, fragmento de obsidiana (reconocibles de lejos).
- Kits de ambiente por instancia con piezas modulares reutilizables entre instancias.

## Checklist de importación
- [ ] Escala y orientación correctas · [ ] Origen en la base · [ ] Polígonos dentro del presupuesto
- [ ] Material URP sin rosa · [ ] Collider · [ ] Prefab creado en la carpeta correcta
- [ ] Nombre según convención · [ ] Licencia anotada · [ ] Probado en escena de prueba

## Audio
- Grabar propio cuando se pueda (voces, rezos, cerillos, sal, vidrio): da identidad y evita licencias.
- Bibliotecas libres: revisar licencia por archivo. Loops sin cortes. Normalizar volúmenes antes de importar.
