# Ficha: Cultista (Hijos de la Estrella Negra)

**Rol:** enemigo común de las tres instancias (GDD §12 y §21). Humano, 40 de vida, machete de 10 de daño. Patrulla, persigue, ataca en grupo.
**Archivo:** `Assets/_Project/Art/Models/Characters/chr_cultista.fbx` · **Script:** `Tools/Blender/chr_cultista.py`

## Silueta y lectura
- Una figura negra con **capucha de tela** redondeada y honda, una **esclavina** (capa corta) sobre los hombros y túnica plisada hasta los tobillos. Se reconoce de lejos por la silueta.
- La cara es una **máscara ritual** de hueso pálido, lisa, con huecos negros en los ojos (dentro brilla un punto rojo) y la **estrella de ocho puntas** pintada en rojo en la frente. Con la linterna, la máscara es lo primero que se ve en la oscuridad.
- La estrella se repite en grande sobre la esclavina, al frente: identifica al culto en las tres instancias.
- Lleva un **machete** en la mano derecha. Las manos son lo único de piel que se ve.

## Medidas (metros)
| Parte | Altura / tamaño |
|---|---|
| Altura total con capucha | 1.82 |
| Hombros | 1.42 (ancho 0.44) |
| Esclavina | de 1.24 a 1.50 |
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
| Estrella (esclavina y máscara) | rojo sangre apagado | `#7A1010` |
| Máscara | hueso pálido | `#D8D0BC` |
| Ojos | rojo con brillo (emisivo) | `#FF2A10` |
| Piel de las manos | moreno grisáceo | `#6E5A4E` |
| Sandalias | café | `#2E2219` |
| Hoja del machete | acero gastado | `#8A8C8E` |
| Mango del machete | madera oscura | `#3A2616` |

## Materiales
Uno por color de la paleta, sin texturas. Túnica con rugosidad alta (tela). Hoja del machete algo metálica. Ojos con emisión para que se vean en la oscuridad.

## Presupuesto
Menos de 3 000 triángulos; el modelo actual tiene unos 1 780 (el GDD permite hasta 15 000 por personaje; low-poly deja margen para grupos de cultistas en pantalla).

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
