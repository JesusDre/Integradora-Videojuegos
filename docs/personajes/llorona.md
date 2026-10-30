# Ficha: La Llorona

**Rol:** entidad de la instancia 1, Xochimilco (GDD §12, §13 y §21). Acecha por los canales, su llanto delata su cercanía, agarra al Padre y le drena vida. Se retira ante la línea de sal y se vence con el ritual.
**Archivo:** `Assets/_Project/Art/Models/Characters/ent_llorona.fbx` · **Script:** `Tools/Blender/ent_llorona.py`

## Silueta y lectura
- Mujer muy alta y muy delgada, de hombros caídos, con **vestido blanco largo y empapado** que cae suelto hasta el suelo, con el **borde roto en jirones**. No se le ven los pies: parece flotar.
- **Cabello negro, largo y mojado** en mechones de distinto largo, hasta la cadera, con una raya al centro. Casi siempre le tapa la cara, pero **a veces se le ve entre los mechones**: un rostro demacrado, cuencas hundidas con **ojos rojos que brillan** y la boca abierta. Desde abajo (el jugador es más bajo) o cuando echa la cabeza atrás, la cara se ve más.
- Brazos largos y huesudos, con codos y muñecas marcados, y dedos alargados que se curvan (más que un humano normal: es lo que la hace inquietante).
- Fiel a la leyenda, sin caricatura (anexo del GDD): ni sangre exagerada ni rasgos de monstruo.

## Medidas (metros)
Se modela con las medidas de abajo (para 1.85 m) y el script la escala ×1.19 hasta 2.20 m.

| Parte | Altura / tamaño |
|---|---|
| Altura total | 2.20 (bastante más alta que el Padre, de 1.80) |
| Hombros | 1.48 (ancho 0.36) |
| Cintura | 1.05 |
| Borde del vestido | 0.00 (toca el suelo) |
| Cabeza (centro) | 1.66 |
| Brazos en T | de 0.18 a 0.92 del centro (más largos que lo normal) |
| Cabello | de la coronilla hasta 1.05 |

## Paleta
| Uso | Color | Hex |
|---|---|---|
| Vestido | blanco hueso húmedo | `#D9DCD8` |
| Sombra del vestido (borde) | gris verdoso de agua | `#7F8B86` |
| Cabello | negro con brillo húmedo | `#0B0B0D` |
| Piel (brazos y manos) | gris azulado pálido | `#AEB6BA` |
| Cuencas y boca | negro rojizo | `#0A0606` |
| Ojos | rojo intenso (emisivo) | `#FF1A0A` |
| Brillo espectral (opcional) | azul verde tenue (emisivo) | `#7FE0D0` |

## Materiales
Vestido con rugosidad baja (mojado brilla un poco). Cabello liso brillante. Piel mate. El brillo espectral se usa para el efecto de desvanecerse y lo controla el código (#35).

## Presupuesto
Menos de 3 500 triángulos; el modelo actual tiene unos 2 330.

## Esqueleto
Humanoide estándar. Las piernas existen (Unity Humanoid las pide) pero quedan dentro del vestido; el cabello sigue al hueso `Head` y a `Chest` en la parte baja.

## Animaciones
| Clip | Duración | Bucle | Qué se ve | Lo usa |
|---|---|---|---|---|
| `Idle` | 3.0 s | sí | de pie, encorvada, sollozo (hombros que suben y bajan) | Acecha (parada) |
| `Stalk` | 2.0 s | sí | se desliza lenta, cuerpo inclinado, brazos colgando | Acecha |
| `Walk` | 1.2 s | sí | avanza más rápido hacia el jugador, brazos hacia delante | Persigue |
| `Grab` | 1.0 s | no | estira los dos brazos al frente y los cierra | Agarre que drena vida |
| `Vanish` | 1.5 s | no | se encoge y se hunde, brazos al cuerpo | Se retira por la sal / fin del ritual |
| `Scream` | 1.2 s | no | echa la cabeza atrás y abre los brazos: el cabello se aparta y se ven la cara y los ojos rojos | Susto cerca del jugador (con el grito) |

## Notas
- El cabello son mechones sueltos (un tubo afilado cada uno). La parte de arriba de cada mechón sigue a la cabeza y la de abajo al pecho, separada del cuerpo, para que no atraviese el vestido cuando se encorva. Los mechones del frente dejan una raya un poco más ancha que los ojos y desigual: según el ángulo se ven uno o los dos.
- En Unity: `AC_Llorona` mezcla `Idle`, `Stalk` y `Walk` con el parámetro `Speed` (0, 0.8 y 2) y tiene los triggers `Grab`, `Vanish` y `Scream`.
- El llanto (audio) y la distorsión de pantalla cuando está cerca son lo que más asusta; el modelo debe ser simple y quedar casi siempre en penumbra.
- No se ve caminar: como el vestido toca el suelo, el movimiento se lee como un deslizamiento.
