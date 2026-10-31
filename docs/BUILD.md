# Cómo sacar la build (issue #64)

## Build de la demo del viernes
1. Abre el proyecto en Unity.
2. En el menú, elige **Ánimas → Build → Windows (demo)**.
3. Espera. La primera vez tarda unos 8 minutos, porque Unity prepara la plataforma Windows; después, menos de un minuto.
4. Busca el resultado en `Builds/Windows/`:
   - `AnimasMexicanas_<fecha>/`: la carpeta del juego. Se abre con `AnimasMexicanas.exe`.
   - `AnimasMexicanas_<fecha>.zip`: la misma carpeta comprimida, para copiarla a otra computadora.

`Builds/` no se sube a git. Cada build se guarda con su fecha, así que se puede comparar una demo con otra. También existe **Ánimas → Build → Linux**.

## Probar el rendimiento
Abre la build desde una terminal con el argumento `-benchmark`:

```
AnimasMexicanas.exe -benchmark
```

El juego recorre las tres instancias, mide los FPS y se cierra solo. El resultado queda en `benchmark.txt`:
- **Windows:** en `%USERPROFILE%\AppData\LocalLow\DefaultCompany\Ánimas mexicanas\`.
- **Linux:** en `~/.config/unity3d/DefaultCompany/Ánimas mexicanas/`.

Anótalo en `docs/BITACORA_RENDIMIENTO.md`.

## En otra computadora (sprints 5 y 8)
1. Copia el `.zip` y descomprímelo.
2. Juega una instancia completa.
3. Si puedes, corre también `-benchmark`.
4. Anota en la bitácora cualquier error que solo pase en la build.
