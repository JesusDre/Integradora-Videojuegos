# Ficha: Cultista (Hijos de la Estrella Negra)

**Rol:** enemigo común de las tres instancias (GDD §12 y §21). Humano, 40 de vida, machete de 10 de daño. Patrulla, persigue, ataca en grupo.
**Archivo:** `Assets/_Project/Art/Models/Characters/chr_cultista.fbx` · **Script:** `Tools/Blender/chr_cultista.py`

## Silueta y lectura
- Una figura negra con capucha puntiaguda y túnica hasta los tobillos: se reconoce de lejos en la oscuridad por la silueta de la capucha, no por detalles.
- La cara no se ve: dentro de la capucha solo hay sombra y **dos puntos rojos tenues** (los ojos reflejan la linterna). Da miedo sin modelar rostro.
- La **estrella de ocho puntas** roja en el pecho es lo único con color: identifica al culto en las tres instancias.
- Lleva un **machete** en la mano derecha. Las manos son lo único de piel que se ve.

## Medidas (metros)
| Parte | Altura / tamaño |
|---|---|
| Altura total con capucha | 1.80 |
| Hombros | 1.42 (ancho 0.44) |
| Cintura (cuerda) | 1.00 |
| Borde de la túnica | 0.12 |
| Cabeza (centro) | 1.60 |
| Brazos en T | de 0.22 a 0.86 del centro, a 1.42 de alto |
| Machete | 0.55 de largo (hoja 0.42) |

## Paleta
| Uso | Color | Hex |
|---|---|---|
| Túnica y capucha | negro azulado mate | `#141418` |
| Interior de la capucha | negro puro | `#050505` |
| Cuerda de la cintura | café oscuro | `#3B2A1E` |
| Estrella del pecho | rojo sangre apagado | `#7A1010` |
| Ojos | rojo con brillo (emisivo) | `#FF2A10` |
| Piel de las manos | moreno grisáceo | `#6E5A4E` |
| Sandalias | café | `#2E2219` |
| Hoja del machete | acero gastado | `#8A8C8E` |
| Mango del machete | madera oscura | `#3A2616` |

## Materiales
Uno por color de la paleta, sin texturas. Túnica con rugosidad alta (tela). Hoja del machete algo metálica. Ojos con emisión para que se vean en la oscuridad.

## Presupuesto
Menos de 3 000 triángulos (el GDD permite hasta 15 000 por personaje; low-poly deja margen para grupos de cultistas en pantalla).

## Esqueleto
Humanoide estándar (ver `README.md`): 19 huesos. El machete va pegado al hueso `RightHand`.

## Animaciones
| Clip | Duración | Bucle | Qué se ve | Lo usa |
|---|---|---|---|---|
| `Idle` | 2.0 s | sí | respiración lenta, cabeza que se mueve un poco | Patrol (parado) |
| `Walk` | 1.0 s | sí | pasos cortos, brazos casi quietos, machete colgando | Patrol |
| `Run` | 0.66 s | sí | pasos largos, torso inclinado, brazos que se mueven | Chase |
| `Attack` | 0.9 s | no | levanta el machete sobre la cabeza y lo baja en diagonal | Attack |
| `Death` | 1.4 s | no | se dobla y cae de espaldas | Health = 0 |

## Notas
- La túnica cubre las piernas: al caminar se ven solo las sandalias. Por eso el pesado rígido funciona sin que la tela se deforme raro.
- Variedad sin modelos extra: en Unity se puede cambiar el color de la cuerda o escalar ±5 % para que el grupo no se vea clonado.
