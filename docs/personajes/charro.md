# Ficha: El Charro Negro y su caballo

**Rol:** entidad de la instancia 2, la hacienda (GDD §12, §13 y §21). Persigue a caballo, embiste, no entra a la capilla y se vence con 3 golpes de cruz cuando cruza la línea de agua bendita.
**Archivos:** `ent_charro.fbx` (jinete) y `ent_caballo.fbx` (caballo) · **Scripts:** `Tools/Blender/ent_charro.py`, `Tools/Blender/ent_caballo.py`

## Silueta y lectura
- Jinete con **traje de charro negro**: chaqueta corta entallada con solapas en V sobre camisa oscura, botonadura de plata y una franja bordada en la espalda; pantalón ajustado con botonadura plateada; guantes negros. **Sombrero de ala ancha**.
- Bajo el sombrero, una **calavera** color hueso: cuencas negras con brasas rojas, cavidad de la nariz y mandíbula con dientes marcados. Es el charro que vendió su alma; con la linterna se ve bajo el ala.
- **Caballo negro de guerra**, más grande que uno normal (cruz a ~1.9 m) y musculoso, con algo demoníaco: costillas marcadas bajo la piel y ojos rojos muy brillantes. Cuerpo continuo (cruz, lomo y grupa marcados), patas hechas solo de cilindros gruesos que se ensanchan hacia los cascos grandes (sin esferas a la vista), cabeza con frente, quijada y hocico, brida y riendas, crin corta en forma de cresta (una aleta con mechones de punta a lo largo del cuello), copete corto y cola hasta casi el suelo. Ojos con brillo rojo y aliento de vapor (partículas en Unity).
- Elegante y amenazante, no caricaturesco (anexo del GDD).
- Jinete y caballo son **dos modelos**: el jinete se monta en un punto del caballo (`Saddle`). Así el caballo, que es cuadrúpedo, tiene su propio esqueleto genérico.

## Medidas (metros)
| Parte | Tamaño |
|---|---|
| Caballo: altura a la cruz | 1.90 (se modela a 1.65 y se escala ×1.15) |
| Caballo: largo del cuerpo | 2.75 |
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
| Calavera | hueso | `#CFC6B0` |
| Camisa | gris muy oscuro | `#2A2A2E` |
| Silla de montar | cuero café oscuro | `#2A1A10` |
| Manta de la silla | rojo muy oscuro | `#4A0A0A` |
| Huesos marcados | gris hueso oscuro | `#26221E` |

## Presupuesto
Jinete menos de 3 500 triángulos; caballo menos de 5 000. Los modelos actuales tienen unos 3 510 (jinete) y 2 240 (caballo).

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
- **Silla discreta a la medida del jinete:** manta roja oscura y asiento bajos, pegados al lomo, teja (respaldo) baja y cuerno, sin faldones (chocaban con las piernas del jinete), y estribos de plata justo donde caen sus pies en `RideIdle` (se midió la posición de los pies en Blender y se dividió entre la escala del caballo). En Unity el tobillo queda 6 cm sobre el estribo.
- **Torso sin huecos:** el torso del jinete son tres piezas cerradas que se traslapan (cadera, cintura y pecho). Con tubos abiertos, al inclinarse quedaba una abertura y de cerca o de lejos se veía hueco por dentro.
- **Charro robusto:** torso, brazos y piernas más gruesos; el sombrero se asienta sobre la calavera (el ala justo arriba de la frente y la copa abraza el cráneo).
- Es el modelo más riesgoso (PIPELINE_ASSETS): se hizo primero el caballo en formas simples y se probó el galope antes de pulir.
- **Cómo quedó:** `Tools/Blender/ent_caballo.py` y `Tools/Blender/ent_charro.py`. El caballo tiene 21 huesos (columna de 3, cuello de 2, cabeza, cola de 2, 4 patas de 3 huesos y `Saddle`); las patas traseras tienen el muslo y el corvejón en ángulo como un caballo real. Sus clips son `Idle`, `Gallop`, `Charge` (se encabrita y arremete), `Hit` y `Dissolve`.
- El jinete se modela de pie en T y la postura de montar sale de sus clips: `RideIdle`, `RideGallop`, `Offer` y `Dissolve`. Debajo del sombrero hay una calavera con brasas rojas en los ojos.
- En Unity, `PF_CharroNegro` (Prefabs/Enemies) es el caballo con el jinete como hijo del hueso `Saddle`, así que el jinete sigue cada movimiento (al encabritarse, la silla sube 40 cm y él va con ella). Cada uno tiene su Animator: `AC_Caballo` (Speed: Idle → Gallop a 6 m/s; triggers `Charge`, `Hit`, `Dissolve`) y `AC_Charro` (Speed: RideIdle → RideGallop; triggers `Offer`, `Dissolve`).
