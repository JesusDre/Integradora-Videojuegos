# Ficha: La Tzitzimime

**Rol:** entidad final, instancia 3, el ex-convento (GDD §12, §13 y §21). Aparece en el ritual, se mueve invisible por el techo en la fase 2 (el copal la revela 15 s), ataca en área desde arriba y tiene una forma final.
**Archivo:** `Assets/_Project/Art/Models/Characters/ent_tzitzimime.fbx` · **Script:** `Tools/Blender/ent_tzitzimime.py`

## Silueta y lectura
- **Figura estelar de gran tamaño** inspirada en las tzitzimime del mito mexica (seres de las estrellas): esquelética, alargada, con **cráneo descarnado**, un **tocado de rayos o picos** como una corona de estrellas y **garras** largas.
- Cuerpo oscuro como cielo nocturno con **puntos de luz** (estrellas) incrustados que brillan; son lo que se ve cuando el copal la revela.
- Respeto al mito, sin caricatura (anexo del GDD). Se evita copiar códices o piezas arqueológicas concretas: diseño propio inspirado en ellos.
- **Forma final:** la misma figura, más grande, con el tocado encendido y los brazos abiertos.

## Medidas (metros)
| Parte | Tamaño |
|---|---|
| Altura de pie | 3.20 |
| Envergadura de brazos | 3.60 |
| Garras | 0.35 |
| Tocado (radio) | 0.80 |
| Forma final | escala ×1.4 |

## Paleta
| Uso | Color | Hex |
|---|---|---|
| Cuerpo | azul noche casi negro | `#0A0E1A` |
| Huesos y cráneo | hueso gastado | `#CFC6B0` |
| Estrellas del cuerpo | blanco azulado (emisivo) | `#DDE8FF` |
| Tocado | obsidiana con filos rojos | `#1A0A0C` / `#C0201A` |
| Ojos | rojo intenso (emisivo) | `#FF1A0A` |

## Presupuesto
Menos de 6 000 triángulos (es un jefe y aparece sola).

## Esqueleto
Humanoide estándar (para reutilizar la lógica de animación), con huesos extra genéricos para el tocado (`Crown`) y las garras.

## Animaciones
| Clip | Bucle | Qué se ve | Lo usa |
|---|---|---|---|
| `CeilingCrawl` | sí | colgada boca abajo, avanza por el techo | Fase 2 |
| `CeilingAttack` | no | se descuelga a medias y golpea en área | Ataque de área |
| `Reveal` | no | se sacude al ser revelada por el copal | Copal |
| `FinalIdle` | sí | de pie, brazos abiertos, tocado brillando | Forma final |
| `FinalAttack` | no | barre con las garras | Forma final |
| `Defeat` | no | se rompe en luces y desaparece | Fin del ritual |

## Notas
- La invisibilidad la hace el material (transparencia controlada por código, #45), no el modelo.
