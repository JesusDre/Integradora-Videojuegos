# Personajes: fichas y cómo se construyen

Cada personaje tiene una **ficha** (este folder) que funciona como concept art en texto: silueta, medidas reales, paleta, materiales, accesorios, presupuesto de polígonos, huesos y animaciones. La ficha es la fuente de verdad del modelo; si cambia el diseño, se cambia aquí primero.

## Del papel a Unity
1. **Ficha** (`docs/personajes/<personaje>.md`).
2. **Script de Blender** (`Tools/Blender/<personaje>.py`): construye el modelo low-poly con formas simples, el esqueleto, el pesado y las animaciones, todo con código. Se ejecuta sin abrir Blender:
   ```bash
   blender -b --factory-startup --python Tools/Blender/chr_cultista.py
   ```
   Como el modelo sale de un script, se puede regenerar cuando cambie la ficha y cualquiera puede leer cómo se hizo (útil para la defensa).
3. **Salidas del script:**
   - `Assets/_Project/Art/Models/Characters/<nombre>.fbx`: malla, materiales, esqueleto y animaciones.
   - `docs/personajes/img/<nombre>_ficha.png`: render de frente y de perfil en postura en T (la "imagen de concepto" de los issues #3 y #17).
4. **Unity:** el FBX se importa como **Humanoid** (los huesos usan los nombres estándar, así que Unity los reconoce solo) y cada animación sale como un clip.

## Por qué así y no con IA 3D + Mixamo
- No depende de planes gratis ni de cuentas externas (Meshy y Tripo limitan descargas y piden atribución; Mixamo pide cuenta de Adobe y subir el modelo).
- Low-poly con formas simples encaja con el arte del GDD ("3D semi-realista estilizado, low-poly") y con un solo desarrollador.
- Todo es nuestro: no hay licencia de terceros que anotar.
- Si un personaje necesita más detalle, el FBX del script sirve de base para retocarlo a mano en Blender o con Blender MCP.

## Reglas comunes
- 1 unidad = 1 metro; origen en los pies; mira hacia +Z en Unity (−Y en Blender).
- Postura de descanso en T, brazos horizontales.
- Huesos con nombres estándar: `Hips`, `Spine`, `Chest`, `Neck`, `Head`, `LeftUpperLeg`, `LeftLowerLeg`, `LeftFoot`, `LeftShoulder`, `LeftUpperArm`, `LeftLowerArm`, `LeftHand` (y `Right…`).
- Pesado rígido por pieza (cada pieza sigue a un hueso): es robusto en low-poly y se ve bien con ropa holgada.
- Animaciones en el sitio (sin desplazamiento de raíz): el movimiento lo hace el `NavMeshAgent` o el código.
- Materiales simples que se convierten a URP/Lit al importar; sin texturas en la primera versión (color por material).

## Fichas
| Personaje | Ficha | Issues |
|---|---|---|
| Cultista | [cultista.md](cultista.md) | #3, #4 |
| La Llorona | [llorona.md](llorona.md) | #17, #18 |
| El Charro Negro y su caballo | [charro.md](charro.md) | #17, #37 |
| La Tzitzimime | [tzitzimime.md](tzitzimime.md) | #17, #41 |
