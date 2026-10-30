# Ficha: El Charro Negro y su caballo

**Rol:** entidad de la instancia 2, la hacienda (GDD §12, §13 y §21). Persigue a caballo, embiste, no entra a la capilla y se vence con 3 golpes de cruz cuando cruza la línea de agua bendita.
**Archivos:** `ent_charro.fbx` (jinete) y `ent_caballo.fbx` (caballo) · **Scripts:** `Tools/Blender/ent_charro.py`, `Tools/Blender/ent_caballo.py`

## Silueta y lectura
- Jinete con **traje de charro negro** (chaqueta corta, pantalón ajustado con botonadura plateada) y **sombrero de ala ancha**. Bajo el sombrero no hay cara: solo oscuridad y un brillo tenue de ojos.
- **Caballo negro** grande, crin larga, ojos con brillo rojo y aliento de vapor (partículas en Unity).
- Elegante y amenazante, no caricaturesco (anexo del GDD).
- Jinete y caballo son **dos modelos**: el jinete se monta en un punto del caballo (`Saddle`). Así el caballo, que es cuadrúpedo, tiene su propio esqueleto genérico.

## Medidas (metros)
| Parte | Tamaño |
|---|---|
| Caballo: altura a la cruz | 1.65 |
| Caballo: largo del cuerpo | 2.40 |
| Jinete de pie | 1.85 (con sombrero 1.95) |
| Ala del sombrero | 0.70 de diámetro |
| Altura de la cabeza del jinete montado | 2.60 |

## Paleta
| Uso | Color | Hex |
|---|---|---|
| Traje y sombrero | negro | `#0E0E10` |
| Botonadura y bordados | plata | `#B9BCC0` |
| Moño (corbata) | rojo oscuro | `#5A0B0B` |
| Caballo | negro azabache | `#09090B` |
| Crin y cola | negro con brillo | `#151518` |
| Ojos (jinete y caballo) | rojo (emisivo) | `#FF3010` |
| Silla de montar | cuero café oscuro | `#2A1A10` |

## Presupuesto
Jinete menos de 3 500 triángulos; caballo menos de 5 000. Los modelos actuales tienen unos 1 800 (jinete) y 1 590 (caballo).

## Esqueletos
- **Jinete:** humanoide estándar (Unity Humanoid).
- **Caballo:** esqueleto genérico (Unity Generic) con `Root`, `Spine1–3`, `Neck`, `Head`, cuatro patas de 3 huesos (`FrontLeftUpper/Lower/Hoof`…), `Tail1–2` y un hueso `Saddle` para montar al jinete.

## Animaciones
| Modelo | Clip | Bucle | Qué se ve | Lo usa |
|---|---|---|---|---|
| Caballo | `Idle` | sí | respira, mueve la cola | Espera |
| Caballo | `Gallop` | sí | galope | Persigue |
| Caballo | `Charge` | no | se encabrita y arranca | Embiste |
| Caballo | `Hit` | no | sacudida al recibir la cruz | 3 golpes |
| Jinete | `RideIdle` | sí | sentado, mano en las riendas | Montado |
| Jinete | `RideGallop` | sí | se mueve con el galope | Persigue |
| Jinete | `Offer` | no | extiende la mano (el trato; extra del GDD) | — |
| Ambos | `Dissolve` | no | se encogen y se desvanecen | Derrota |

## Notas
- Es el modelo más riesgoso (PIPELINE_ASSETS): se hizo primero el caballo en formas simples y se probó el galope antes de pulir.
- **Cómo quedó:** `Tools/Blender/ent_caballo.py` y `Tools/Blender/ent_charro.py`. El caballo tiene 21 huesos (columna de 3, cuello de 2, cabeza, cola de 2, 4 patas de 3 huesos y `Saddle`); las patas traseras tienen el muslo y el corvejón en ángulo como un caballo real. Sus clips son `Idle`, `Gallop`, `Charge` (se encabrita y arremete), `Hit` y `Dissolve`.
- El jinete se modela de pie en T y la postura de montar sale de sus clips: `RideIdle`, `RideGallop`, `Offer` y `Dissolve`. Debajo del sombrero no hay cara: oscuridad y dos ojos rojos.
- En Unity, `PF_CharroNegro` (Prefabs/Enemies) es el caballo con el jinete como hijo del hueso `Saddle`, así que el jinete sigue cada movimiento (al encabritarse, la silla sube 40 cm y él va con ella). Cada uno tiene su Animator: `AC_Caballo` (Speed: Idle → Gallop a 6 m/s; triggers `Charge`, `Hit`, `Dissolve`) y `AC_Charro` (Speed: RideIdle → RideGallop; triggers `Offer`, `Dissolve`).
