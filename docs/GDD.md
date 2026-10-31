<!-- Fuente de verdad del diseño. Si cambia el GDD, actualiza este archivo y avisa al equipo. -->
<!-- Las decisiones de diseño se registran en docs/PENDIENTES_DISENO.md. Si aparece una nueva duda, se anota ahí y se marca aquí como "(Pendiente Pn)"; esa parte no se implementa sin preguntar. -->

# GDD — Ánimas mexicanas

**Equipo de desarrollo:** 6 integrantes. Todo el trabajo (código, arte, audio, UI, niveles y textos) lo hace Jesús con Claude; los demás integrantes presentan la defensa individual con `docs/DEFENSA.md`.

**Versión del documento:** 2.2 (29 oct 2026: correcciones de la revisión y 16 decisiones de diseño; ver `PENDIENTES_DISENO.md`)

**Fecha:** octubre 2026

## 1. Información general

| Elemento             | Descripción                                                                                                                                                                                     |
|----------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Nombre del juego     | *Ánimas mexicanas*                                                                                                                                                                              |
| Concepto / Pitch     | Un sacerdote exorcista queda atrapado en tres lugares malditos de México. Debe craftear objetos de fe, vencer a los cultistas y a las entidades del folclore, y escapar de cada lugar con vida. |
| Género               | Terror en primera persona con campaña por niveles lineales                                                                                                                                      |
| Subgénero            | Horror de escape + crafteo en altares + oleadas de cultistas (estructura tipo Left 4 Dead)                                                                                                      |
| Plataformas objetivo | PC (Windows)                                                                                                                                                                                    |
| Audiencia objetivo   | Jugadores de 16 años en adelante, fans del terror, del folclore mexicano, de Left 4 Dead y de Phasmophobia                                                                                      |
| Clasificación ESRB   | M (Mature): violencia, sangre y temas de horror                                                                                                                                                 |
| Tipo de arte         | 3D semi-realista estilizado, low-poly para ser viable en 10 semanas                                                                                                                             |
| Modo de juego        | Un jugador. Sin multijugador ni compañeros de IA.                                                                                                                                               |

## 2. Visión del juego

### Concepto principal

- **¿De qué trata el juego?** Tres lugares malditos llevan al Padre Anselmo a descubrir que un mismo culto, los Hijos de la Estrella Negra, está despertando a una entidad del folclore en cada uno. Para salir debe romper el ancla de cada entidad.

- **¿Qué hará principalmente el jugador?** Avanzar por niveles lineales, recoger materiales, craftear objetos de fe en altares, sobrevivir encuentros y oleadas de cultistas, rezar exorcismos y escapar.

- **¿Qué hace diferente al juego?** Une la estructura de campaña de Left 4 Dead (cuartos seguros, oleadas de enemigos) con criaturas del folclore mexicano que se vencen con objetos de fe y crafteo, no solo con balas, y con la idea de purificar lugares tipo Phasmophobia.

- **¿Por qué alguien querría jugarlo?** Ofrece tensión, decisiones de inventario limitado y un terror mexicano reconocible (la Llorona, el Charro Negro, la Tzitzimime) con un objetivo claro: escapar.

### Pilares de diseño

1.  **Escapar es la meta; exorcizar es el medio.** Cada instancia termina cuando sales, y para abrir la salida debes romper el ancla de la entidad.

2.  **Folclore con mecánica propia.** Cada entidad actúa según su leyenda, no es un monstruo genérico con otro nombre.

3.  **Crafteo con decisiones.** El inventario es limitado: llevas agua bendita o balas de sal, pero no todo.

## 3. Experiencia de juego

**Emociones buscadas:** tensión, miedo, alivio al llegar a un cuarto seguro, ingenio al resolver una entidad con el objeto correcto y satisfacción al escapar.

- La **música** es casi silenciosa en la calma y se activa con percusión creciente cuando aparecen los cultistas y durante el ritual.

- El **arte** usa luz baja: linterna, veladoras y antorchas como fuentes principales, con niebla y sombras profundas.

- El **crafteo** da progreso tangible: pasar de un machete a agua bendita y cruz de plata se siente como pasar de huir a poder enfrentar.

- La **dificultad** crece por diseño: cada instancia trae más cultistas por encuentro. El Director de ritmo (deseable) la ajustaría según el desempeño del jugador.

- El **ritual final** de cada instancia es el momento de máxima tensión: rezar quieto mientras todo ataca.

## 4. Gameplay

### Core gameplay loop

**Avanzar → Recoger materiales → Sobrevivir → Llegar al cuarto seguro → Craftear y curarse → Siguiente sección → Ritual en el ancla → Huida**

Cada instancia es una campaña lineal de dos secciones separadas por un cuarto seguro, y dura unos 10 minutos.

1.  **Sección 1, reconocimiento.** Primer contacto con cultistas, primer y segundo objeto clave.

2.  **Cuarto seguro.** Altar de crafteo, checkpoint y preparación para el final.

3.  **Sección 2, clímax.** Tercer objeto clave y llegada al ancla de la entidad.

4.  **Ritual.** El Padre reza en el ancla mientras cultistas y entidad lo atacan.

5.  **Huida.** Carrera hasta la salida con la entidad persiguiéndolo.

### Acciones del jugador

- Caminar y correr (con resistencia).

- Interactuar con objetos, puertas, notas y altares.

- Atacar cuerpo a cuerpo

- Disparar la escopeta con balas de sal y apuntar con ella.

- Cambiar entre el arma cuerpo a cuerpo y la escopeta.

- Rezar (acción propia del Padre).

- Usar objetos rápidos (agua bendita, línea de sal, veladora, venda, molotov).

- Encender y apagar la linterna.

- Craftear, solo en el altar de un cuarto seguro.

## 5. Mecánicas

### Crafteo en el altar

- **Descripción:** menú de recetas fijas que combina materiales para producir objetos de fe y armas.

- **Activación:** F.

- **Efecto:** genera el objeto y el Padre lo bendice.

- **Restricciones:** solo en altares; inventario de 8 espacios; recetas desbloqueadas por notas y por instancia.

- **Interacción:** depende del Sistema de objetos (sección 18) y de los materiales de cada lugar.

### Combate cuerpo a cuerpo y a distancia

- **Descripción:** machete o chancla bendita cuerpo a cuerpo y escopeta con balas de sal a distancia.

- **Activación:** clic izquierdo ataca con el arma en mano o dispara la escopeta.

- **Efecto:** daño variable según arma; los espíritus reciben poco daño sin bendición.

- **Restricciones:** cada golpe gasta resistencia; la escopeta tiene munición escasa y su ruido atrae cultistas.

- **Interacción:** se combina con el crafteo (balas de sal, molotov) y con el rezo para contener entidades.

### Rezo

- **Descripción:** el Padre canaliza una oración que repele espíritus.

- **Activación:** mantener R durante 1.5 segundos.

- **Efecto:** círculo de 4 m de radio que repele espíritus durante 10 segundos. Durante esos mismos 10 segundos un escudo bendito rodea al Padre y sus armas hacen +50 % de daño.

- **Restricciones:** el Padre queda casi inmóvil y vulnerable a humanos; enfriamiento de 20 segundos.

- **Interacción:** es la base del ritual final y se complementa con la línea de sal y las veladoras.

### Salud y cuartos seguros

- **Descripción:** 100 puntos de vida sin regeneración; el cuarto seguro es la única zona sin amenazas.

- **Activación:** vendas con la tecla de objeto rápido; el altar restaura salud hasta 60 %.

- **Efecto:** la venda cura 30; el cuarto seguro cierra la puerta y guarda el progreso.

- **Restricciones:** pocas vendas por sección.

- **Interacción:** las vendas se fabrican en el altar o se hallan como parte de los secretos.

### Ritual final

- **Descripción:** el Padre reza en el ancla de la entidad mientras llegan oleadas.

- **Activación:** tras colocar los 3 objetos clave, se dibuja un círculo dorado alrededor del ancla. Estar dentro del círculo llena la barra sola, sin teclas; salir la pausa. El rezo normal (R) se puede usar dentro.

- **Efecto:** una barra de progreso avanza; al llenarse, la entidad se debilita y se abre la huida.

- **Restricciones:** si el Padre recibe un golpe de 15 o más de daño, la barra retrocede un 10 %.

- **Interacción:** es el clímax de cada instancia, se prepara con el crafteo en el cuarto seguro y abre la huida.

### Encuentros y oleadas

- **Descripción:** grupos de cultistas colocados a mano en cada sección y una oleada programada durante el ritual.

- **Activación:** al entrar a una zona marcada o al iniciar el ritual.

- **Efecto:** los cultistas aparecen y patrullan o atacan en grupo; en el ritual llegan en oleadas hasta que la barra se llena.

- **Restricciones:** no aparecen en cuartos seguros y cada encuentro tiene un número fijo de cultistas.

- **Interacción:** el molotov y las líneas de sal ayudan a controlar los grupos; un Director de ritmo (deseable, sección 21) podría regularlos.

## 6. Condiciones de victoria y derrota

### ¿Cómo gana el jugador?

- Cada instancia se completa cruzando la salida tras el ritual y la huida.

- La campaña termina al escapar de la instancia 3 tras vencer a la Tzitzimime.

### ¿Cómo pierde?

- La vida del Padre llega a 0.

- Si muere durante la huida, también vuelve al último cuarto seguro: la huida no tiene checkpoint propio.

**Después de perder:** aparece la pantalla de derrota; con Reintentar, el Padre reaparece en el último cuarto seguro con 60 de vida (o la que tenía al guardarse, si era más) y con el inventario y los materiales del último guardado. Los objetos clave de la sección 2 y el ritual se repiten.

## 7. Progresión

- **Recetas por instancia:** cada instancia deja notas que desbloquean recetas nuevas en el altar.

- **Dificultad creciente:** cada instancia trae más cultistas por encuentro y entidades más exigentes; el Director de ritmo (deseable) lo ajustaría según el desempeño.

- **Progresión narrativa:** tres actos, uno por instancia, cada uno con su entidad.

- **Nivel de personaje:** no aplica en el MVP; el árbol de habilidades es un extra.

- **Mejora de estaciones:** no aplica; hay un solo altar en cada cuarto seguro y siempre ofrece las recetas ya descubiertas.

**Ejemplo:**

Instancia 1 (Xochimilco) → agua bendita, balas de sal, línea de sal, veladora y molotov

Instancia 2 (Hacienda) → cruz de plata y chancla bendita

Instancia 3 (Ex-convento) → copal y ritual final.

## 8. Historia y narrativa

### Sinopsis

El Padre Anselmo Quiroz es un sacerdote retirado desde que un exorcismo salió mal, hace veinte años. Al ver tres videos virales de lugares malditos reconoce una estrella negra de ocho puntas que ya vio en aquel caso. Decide ir a cada lugar a purificarlo y, sobre todo, a salir con vida.

### Contexto

- **¿Dónde ocurre?** En tres lugares de México: Xochimilco (Ciudad de México), una hacienda abandonada del Bajío y un ex-convento con catacumbas.

- **¿Cuándo ocurre?** Época contemporánea.

- **¿Qué está pasando en el mundo?** Los Hijos de la Estrella Negra, una secta ficticia inspirada en las tzitzimime, dejan ofrendas en cada lugar para liberar a una entidad. Sus miembros son los mismos en las tres instancias: túnicas negras con la estrella, machetes y antorchas.

### Desarrollo de la historia

**Inicio** (los videos y el símbolo) → **Conflicto** (Xochimilco: el culto existe y la Llorona despierta) → **Desarrollo** (Hacienda: el culto hace pactos con el Charro Negro) → **Clímax** (Ex-convento: el templo y la Tzitzimime) → **Final** (el Padre sella a la Tzitzimime y escapa)

**Decisiones del jugador:** ninguna en el MVP. El trato del Charro Negro y un final alternativo son extras.

## 9. Mundo del juego

| Zona                                  | Apariencia                                              | Mecánicas especiales                                     | Enemigos                | Recursos                                                    |
|---------------------------------------|---------------------------------------------------------|----------------------------------------------------------|-------------------------|-------------------------------------------------------------|
| **Xochimilco** (Instancia 1)          | Canales estrechos, chinampas, niebla y muñecas colgadas | Cruce en trajinera, líneas de sal contra la Llorona      | Cultistas, La Llorona   | Sal, botellas, cera, mecha, mezcal, trapo, tela, casquillos |
| **Hacienda abandonada** (Instancia 2) | Terracería, casa grande, capilla y panteón              | Pila de agua bendita limitada, huida a pie por el camino | Cultistas, Charro Negro | Plata, madera, chancla, mezcal; tela y casquillos en el botín de los cultistas |
| **Ex-convento** (Instancia 3)         | Claustro, criptas y catacumbas                          | Oleadas en catacumbas, niebla roja durante el ritual     | Cultistas, Tzitzimime   | Resina, brasas, cera; sal y botellas en alijos; tela y casquillos en el botín de los cultistas |

## 10. Diseño de niveles

- **Cantidad de niveles/zonas:** 3 instancias, cada una con 2 secciones y 1 cuarto seguro en medio.

- **Duración aproximada:** unos 10 minutos por instancia.

- **Objetivo por nivel:** encontrar los 3 objetos clave, hacer el ritual y escapar.

- **Objetos clave:** 3 muñecas ofrenda del culto en Xochimilco, 3 monedas del pacto en la hacienda y 3 fragmentos de obsidiana en el convento.

- **Obstáculos:** cultistas patrullando, oscuridad, zonas con niebla y ventanas de tiempo en la huida final.

- **Checkpoints:** un cuarto seguro por instancia, entre las dos secciones, con altar y autoguardado.

- **Secretos:** notas que desbloquean recetas, alijos de sal y vendas

- **Jefes:** una entidad por instancia, con el ritual como parte del combate.

## 11. Personajes

**Número de personajes:** 1 jugable, 1 enemigo común y 3 entidades jefe.

### Personaje jugable

**Padre Anselmo Quiroz**

- **Rol:** exorcista retirado, único personaje controlable.

- **Altura:** 1.8 m; es la referencia de escala del juego (1 unidad de Unity = 1 metro).

- **Descripción:** sacerdote de edad madura, cansado y culpable por un exorcismo fallido, pero con fe y conocimiento de rituales.

- **Habilidades:** rezo que repele espíritus, bendición de objetos en el altar y lectura de símbolos del culto, que le permite entender las notas que desbloquean recetas.

- **Motivación:** cerrar el caso que dejó abierto hace veinte años y salir con vida.

- **Relación:** reconoce el símbolo del culto desde el inicio; los cultistas lo reconocen a él.

### NPC destacado

Ninguno en esta versión. La historia llega por notas, grabaciones y los comentarios del Padre.

## 12. Enemigos

Valores iniciales para ajustar en playtest; los números de balance van en ScriptableObjects (ver la tabla siguiente).

### Valores iniciales de balance

| Sistema | Valor inicial |
|---|---|
| Movimiento | Caminar 3 m/s · correr 5.5 m/s |
| Resistencia | Máximo 100 · correr gasta 15/s · golpe de machete 15 · golpe de chancla 10 · recupera 20/s tras 1 s sin gastar · al llegar a 0 no corre ni golpea hasta subir a 25 |
| Machete | 20 de daño · un golpe cada 0.4 s · alcance 2.8 m |
| Chancla bendita | 15 de daño a humanos, 30 a espíritus · silenciosa |
| Escopeta | 8 perdigones en un cono de 5°: 48 de daño si todos pegan a menos de 8 m (20 más lejos), mata a un cultista de un tiro de cerca · 2 cartuchos, recarga de 2 s · máximo 24 balas de sal · el disparo se oye a 20 m |
| Molotov | 15 de daño por segundo en 3 m de radio durante 5 s |
| Espíritus sin bendición | Reciben el 25 % del daño de machete y escopeta |
| Cultista | Vista 15 m en un cono de 110° · oye al jugador correr a 10 m · patrulla a 2 m/s y persigue a 3.5 m/s · ataca cada 1.5 s |
| Cultistas por encuentro | Instancia 1: 2–3 · instancia 2: 3–4 · instancia 3: 4–5 |
| Oleadas del ritual | Instancia 1: 3 oleadas de 3 · instancia 3: 4 oleadas de 4 · instancia 2: sin ritual con barra; los cultistas llegan en encuentros por guion durante la persecución |

| Nombre          | Tipo                              | Vida      | Ataques                        | Comportamiento                                       | Debilidad                                 | Recompensa                             |
|-----------------|-----------------------------------|-----------|--------------------------------|------------------------------------------------------|-------------------------------------------|----------------------------------------|
| Cultista        | Básico                            | 40        | Machete, 10 de daño            | Patrulla, detecta por vista y sonido, ataca en grupo | Cualquier arma; el molotov golpea en área | Sal, trapo, a veces munición; en las instancias 2 y 3 también tela y casquillos |
| La Llorona      | Entidad (jefe, instancia 1)       | Por fases | Agarre que drena vida          | Recorre los canales y su llanto delata su cercanía   | Líneas de sal, veladoras y rezo           | Nota con la receta de la cruz de plata |
| El Charro Negro | Entidad (jefe, instancia 2)       | Por fases | Embestida a caballo            | Persigue y te ofrece un trato                        | Cruz de plata cargada sobre agua bendita  | Objeto maldito (si aceptas el trato)   |
| La Tzitzimime   | Entidad (jefe final, instancia 3) | Por fases | Ataques de área desde el techo | Aparece en el ritual y el convento colapsa           | Luz y rezo prolongado                     | Fin de la campaña                      |

## 13. Jefes

Cada entidad se vence con crafteo y rezo, no con daño bruto.

### Jefe 1: La Llorona (Isla de las Muñecas)

- **Fase 1, acecho:** recorre los canales y su llanto distorsiona la pantalla cuando está cerca. Si te alcanza, te agarra y drena vida.

- **Fase 2, ritual:** colocar los 3 objetos clave en el ancla y encender con F las 3 veladoras fijas del ancla mientras ella ataca; no gastan inventario.

- **Fase final, cierre:** rezo prolongado hasta que se disuelve; después empieza la huida en trajinera. La trajinera avanza sola por una ruta fija; el Padre no la maneja, solo mira, dispara y usa objetos rápidos desde ella. Lo mismo vale para el cruce en trajinera de la sección 1.

- **Condición para derrotarla:** completar el ritual; sus agarres cuentan como golpe fuerte y hacen retroceder la barra.

### Jefe 2: El Charro Negro (Hacienda)

- **Fase 1, persecución:** cabalga por el terreno y no puede entrar a la capilla (los cultistas sí entran).

- **Fase 2, el trato:** te ofrece un objeto raro y maldito. Aceptar es opcional y da ventaja con costo (extra).

- **Fase final, derrota:** cuando embiste y cruza la línea de agua bendita, golpearlo con la cruz de plata cargada; con tres golpes se debilita y se disuelve. Después, huida a pie por el camino hasta el portón de la hacienda.

- **Línea de agua bendita:** frente a la capilla hay 2 o 3 canaletas marcadas en las rutas por donde embiste el Charro. Con F y 1 agua bendita se llena una, que brilla y funciona durante 20 segundos.

- **Condición para derrotarlo:** colocar las 3 monedas del pacto en el altar de la capilla (un pedestal, no el altar de crafteo) abre la fase final; luego trazar la línea de agua bendita y acertar tres golpes de cruz de plata cargada cuando él la cruza. Esta instancia no usa la barra del ritual.

### Jefe final: La Tzitzimime (Ex-convento)

- **Fase 1:** ritual en las catacumbas con oleadas de cultistas.

- **Fase 2:** se mueve invisible por el techo y ataca en área. El humo del copal la revela durante 15 segundos, y solo mientras está revelada se le puede dañar con luz y rezo.

- **Fase 3:** forma final de la entidad y huida a la salida; el derrumbe del convento es deseable.

- **Condición para derrotarla:** llenar la barra del ritual con los 3 fragmentos de obsidiana colocados, usando luz y rezo y resistiendo las oleadas con todo lo aprendido (líneas de sal, agua bendita, cruz de plata y copal).

## 14. Controles

| Acción            | Teclado y mouse | Gamepad               |
|-------------------|-----------------|-----------------------|
| Moverse           | WASD            | Stick Izquierdo       |
| Cámara            | Mouse           | Stick derecho         |
| Correr            | Shift           | Click stick Izquierdo |
| Atacar o disparar | Clic izquierdo  | RT                    |
| Apuntar (escopeta) | Clic derecho   | LT                    |
| Recargar (escopeta) | C             | B                     |
| Cambiar de arma   | Q o rueda del mouse | X                |
| Interactuar       | F               | A                     |
| Rezar (mantener)  | R               | Y                     |
| Objeto rápido     | 1 a 4           | Cruceta               |
| Linterna          | E               | RB                    |
| Inventario        | Tab             | Menú                  |
| Pausa             | Esc             | Start                 |

Los objetos rápidos (1 a 4) se usan al instante: el agua bendita y el molotov se lanzan hacia donde mira el Padre, la línea de sal se traza frente a él, y las vendas y la veladora se usan en el momento. Q alterna entre el arma cuerpo a cuerpo equipada y la escopeta.

## 15. Cámara

- **Tipo:** primera persona, para maximizar la inmersión de terror.

- **Comportamiento especial:**

  - Zoom ligero al apuntar con la escopeta.

  - Balanceo leve al correr, sin efectos que causen mareo.

  - Rotación horizontal libre de 360°; la vertical se limita (por ejemplo ±80°) para que la cámara nunca se voltee.

## 16. UI, HUD y GUI

- Barra de vida y de resistencia, abajo a la izquierda, con estilo minimalista.

- Cuatro espacios de objeto rápido y contador de munición.

- Inventario de 8 espacios, accesible con Tab.

- Menú del altar con la lista de recetas, materiales necesarios y vista previa del objeto.

- Icono de interacción y subtítulos de lo que dice el Padre.

- Barra de progreso del ritual, visible solo durante el ritual.

## 17. Flujo de pantallas

Splash Screen  
↓  
Menú principal  
├── Nueva partida (Historia)  
├── Continuar  
├── Opciones  
└── Salir  
↓  
Selección de instancia (se desbloquean en orden)  
↓  
Gameplay (Sección 1 → Cuarto seguro → Sección 2 → Ritual → Huida)  
↓  
Pausa: Continuar · Reiniciar · Opciones · Salir al menú principal  
↓  
Derrota (vida en 0): pantalla de derrota → Reintentar (desde el último cuarto seguro) · Salir al menú  
↓  
Fin de instancia (Resumen) → Siguiente instancia  
↓  
Victoria y créditos (tras la instancia 3) → Menú principal

El inventario se abre con Tab durante el gameplay, no desde la pausa. La pausa, la pantalla de derrota y la de victoria son obligatorias por la rúbrica. Reiniciar (en la pausa) carga el último cuarto seguro, igual que Reintentar. Opciones: sensibilidad del mouse, invertir eje Y, volumen general, de música, de efectos y de voces, y subtítulos sí/no.

## 18. Sistema de objetos

**Categorías:**

- Armas cuerpo a cuerpo (machete, chancla bendita).

- Arma a distancia (escopeta con balas de sal).

- Munición (balas de sal).

- Objetos de fe (agua bendita, línea de sal, veladora, cruz de plata, copal).

- Consumibles (vendas, molotov de mezcal).

- Materiales (sal, botellas, cera, mecha, plata, madera, mezcal, trapo, tela, casquillos, chancla, resina, brasas).

- Objetos clave de misión (muñecas ofrenda, monedas del pacto, fragmentos de obsidiana).

- Coleccionables narrativos (notas, grabaciones).

### Recetas del altar

| Objeto            | Materiales             | Efecto                                                 | Sirve contra          | Categoría            |
|-------------------|------------------------|--------------------------------------------------------|-----------------------|----------------------|
| Agua bendita      | Botella, sal           | Daño en área a espíritus; interrumpe su ataque         | Espíritus             | Objeto de fe         |
| Balas de sal      | Casquillos, sal        | Munición de la escopeta                                | Cultistas y espíritus | Munición             |
| Línea de sal      | Sal                    | Barrera de 20 segundos que los espíritus no cruzan     | Espíritus             | Objeto de fe         |
| Veladora          | Cera, mecha            | Luz y humo que delata entidades cercanas               | Utilidad              | Objeto de fe         |
| Molotov de mezcal | Botella, mezcal, trapo | Fuego en área                                          | Cultistas             | Consumible           |
| Cruz de plata     | Plata, madera          | Ataque cargado; necesaria contra el Charro Negro       | Entidades             | Objeto de fe         |
| Chancla bendita   | Chancla, agua bendita  | Arma cuerpo a cuerpo silenciosa, buen daño a espíritus | Espíritus             | Arma cuerpo a cuerpo |
| Copal             | Resina, brasas         | Revela a la Tzitzimime invisible durante 15 segundos (necesario en su fase 2) | Utilidad              | Objeto de fe         |
| Vendajes          | Tela                   | Cura 30 puntos de vida                                 | Utilidad              | Consumible           |
| Vendajes+         | Tela, mezcal           | Cura más que la venda normal (deseable, fuera del MVP)                      | Utilidad              | Consumible           |

### Ejemplo de objeto importante

- **Nombre:** Agua bendita.

- **Función:** daño en área a espíritus y herramienta clave contra las tres entidades.

- **Rareza:** común; se fabrica con sal y botellas.

- **Efecto:** interrumpe el ataque de una entidad durante 3 segundos.

- **Obtención:** crafteada en el altar desde la instancia 1.

Todos los objetos se fabrican en el altar de los cuartos seguros. Los materiales se recogen en el mapa y se obtienen al saquear cultistas. Los objetos, materiales, municiones y ofrendas se recogen al pasar sobre ellos; las armas, con F. Cada cuarto seguro tiene una mesa de suministros (vendas, balas de sal y sal). El agua bendita también se fabrica en el altar (botella y sal); en la hacienda, además, hay una pila en la capilla: con F da 1 agua bendita al inventario, hasta 3 veces, y vuelve a tener 3 cuando el Padre reaparece tras morir.

## 19. Economía del juego

No hay moneda: la economía se basa en materiales y en el espacio limitado del inventario.

- **¿Cómo se consiguen?** Recogiendo materiales en el mapa y saqueando a los cultistas.

- **¿En qué se gastan?** En craftear objetos de fe, munición y vendas en el altar.

- **¿Existe tienda?** No; no se compran ni se venden objetos.

- **¿Pasa algo a la siguiente instancia?** Sí: el inventario y los materiales pasan completos de una instancia a la siguiente. Además, los cultistas de las instancias 2 y 3 sueltan tela y casquillos y el convento tiene alijos de sal y botellas (sección 9), para que siempre haya vendas, munición y agua bendita.

- **¿Cuánto cuestan las cosas?** Balanceado por rareza: la sal y las botellas son abundantes en Xochimilco; la plata y la resina son escasas.

- **¿Se pueden perder?** Al morir, el Padre vuelve al último cuarto seguro con el inventario del último guardado (al entrar o al salir de ese cuarto), así que lo fabricado en el altar no se pierde.

- **¿Existe límite?** Sí, 8 espacios de inventario para objetos; los materiales se acumulan hasta 99 por tipo y no ocupan espacio.

## 20. Sistema de dificultad

**Escalado:** en el MVP la dificultad sube por diseño (más cultistas por encuentro en cada instancia). El Director de ritmo (deseable) la ajustaría según la salud y los recursos del jugador.

**Dificultad por instancia** (valores iniciales, en `Data/Difficulty` y `Data/Enemies`):

| Instancia | Cultistas por encuentro | Persecución | Daño por golpe | Vista | Espera entre ataques | Oleadas del ritual | Botín de los cultistas |
|---|---|---|---|---|---|---|---|
| 1\. Xochimilco | 2–3 | 3.5 m/s | 10 | 15 m | 1.5 s | 3 de 3 | sal, trapo, a veces balas |
| 2\. Hacienda | 3–4 | 3.7 m/s | 12 | 16 m | 1.4 s | sin barra (encuentros por guion) | sal, trapo, tela, casquillos |
| 3\. Ex-convento | 4–5 | 3.9 m/s | 14 | 17 m | 1.3 s | 4 de 4 | sal, tela, casquillos, a veces balas |

En el ritual, cada oleada llega cuando se vence la anterior o, como máximo, 25 s después.

**Modos:**

| Modo               | Cambios                                           |
|--------------------|---------------------------------------------------|
| Normal             | Balance estándar de tensión y combate             |
| Fácil (deseable)   | Menos cultistas, más vendas, ritual más rápido    |
| Difícil (deseable) | Más cultistas, menos materiales, ritual más lento |

## 21. Inteligencia artificial

**Cultista (máquina de estados):**

Patrulla → Detecta jugador (vista/sonido) → Persigue → Ataca → Pierde de vista → Busca (últimos 10 s) → Regresa a patrulla

**Entidades:** cada una tiene su propia máquina de estados por fases (sección 13). Ejemplo, La Llorona: Acecha → Detecta → Persigue → Se retira si hay línea de sal → Reaparece junto al agua.

**Charro Negro:** Persigue a caballo → Embiste → Se debilita al cruzar la línea de agua bendita → Se disuelve.

**Tzitzimime:** Oleadas de cultistas → Aparece invisible en el techo → Ataque de área (el copal la revela para dañarla) → Forma final.

**Director de ritmo (deseable):** en el MVP los encuentros y las oleadas van por guion. Si hay tiempo, un Director simple alternaría estos estados según la salud del jugador y el tiempo sin amenazas, y no generaría nada en cuartos seguros.

Calma (45-90 s) → Tensión → Pico (oleada o evento) → Relajación (20-30 s) → Calma

## 22. Arte

**Dirección visual:** semi-realista estilizado, con una paleta distinta por instancia.

| Instancia       | Paleta                     | Iluminación                              |
|-----------------|----------------------------|------------------------------------------|
| 1\. Xochimilco  | Azul-verde, niebla blanca  | Luz fría de luna, linterna y veladoras   |
| 2\. Hacienda    | Ocre, negro y rojo apagado | Atardecer que se apaga, luz de capilla   |
| 3\. Ex-convento | Piedra gris y rojo intenso | Antorchas y veladoras como luz principal |

- **Personajes:** el Padre con sotana desgastada y rasgos realistas con leves toques estilizados; en primera persona se ven sobre todo sus brazos y manos.

- **Escenarios:** kits modulares y packs low-poly con licencia abierta como base.

- **Enemigos:** cultistas con túnicas negras y estrella de ocho puntas; entidades fieles a la leyenda, sin caricatura (La Llorona de blanco y empapada, el Charro Negro con traje y caballo negros, la Tzitzimime como figura estelar de gran tamaño).

- **UI:** minimalista y diegética cuando sea posible.

- **Iluminación:** linternas, veladoras y antorchas como fuente principal.

- **Colores:** azul-verde en Xochimilco, ocre y rojo en la hacienda, gris piedra y rojo intenso en el ex-convento.

## 23. Audio

### Música

- Menú: ambiental, tono misterioso.

- Calma: casi silencio, con ambiente del lugar (agua, viento, campanas).

- Oleadas y ritual: percusión creciente cuando llegan los cultistas o empieza el rezo.

- Entidades: un tema o sonido guía por cada una; el llanto de la Llorona se oye en el espacio y avisa su cercanía.

- Victoria: tema esperanzador pero agridulce.

- Derrota: silencio con eco de latidos.

### Efectos de sonido

Rezo, sal, veladoras, golpes de machete, escopeta, pasos según terreno, puertas de cuartos seguros y gritos de cultistas.

### Voces

MVP con subtítulos y pocas líneas del Padre; versión deseable con unas 15 a 20 líneas grabadas (oraciones, reacciones y comentarios). Cultistas y entidades no tienen diálogo, solo sonidos guturales y gritos.

## 24. Referencias

| Referencia      | Elemento                                                                                            |
|-----------------|-----------------------------------------------------------------------------------------------------|
| Left 4 Dead 2   | Estructura de campaña, cuartos seguros, oleadas, enemigos especiales y Director de ritmo (deseable) |
| Phasmophobia    | Exorcismo y purificación de lugares, linterna, tensión sin combate directo                          |
| La Llorona      | Leyenda urbana; llora junto al agua y no debe sobresaltarse                                         |
| El Charro Negro | Leyenda rural; ofrece pactos en los caminos                                                         |
| Tzitzimime      | Seres de las estrellas del mito mexica; inspiran al culto                                           |

## 25. Alcance del proyecto

### Obligatorio (MVP)

- Primera persona con movimiento, linterna e interacción.

- Salud, daño, combate cuerpo a cuerpo y escopeta con balas de sal.

- Instancia 1 (Xochimilco) con 2 secciones y 1 cuarto seguro.

- Instancia 2 (Hacienda) con 2 secciones, 1 cuarto seguro y el Charro Negro.

- Instancia 3 (Ex-convento) con 2 secciones, 1 cuarto seguro y la Tzitzimime.

- Cultista con IA de estados, con encuentros y oleadas por guion.

- La Llorona con acecho, ritual y huida.

- Altar con 8 recetas (agua bendita, balas de sal, línea de sal, veladora, molotov, cruz de plata, chancla bendita y copal), vendajes y rezo. Vendajes+ es deseable.

- Inventario de 8 espacios y notas que desbloquean recetas.

- HUD funcional, audio mínimo y guardado en cuartos seguros.

### Deseable

- Tres espacios de guardado y modos Fácil y Difícil.

- Director de ritmo con reglas simples por tiempo y salud.

- Soporte de gamepad

- Vendajes+ (tela y mezcal).

- Final con derrumbe del convento.

### Extra 

1.  Muñecas, que solo se mueven cuando no las miras.

2.  Medidor de fe como complemento.

3.  Chaneque.

4.  Nahual.

5.  Árbol de habilidades.

6.  Final alternativo con el trato del Charro Negro.

## 26. Sistema de guardado

- **¿Qué información se guarda?** Instancia y sección actuales, vida, inventario y materiales (se conservan entre instancias), recetas desbloqueadas, notas encontradas y objetos clave recogidos.

- **¿Cuándo se guarda?** Autoguardado al entrar a cada cuarto seguro, al salir de él (para conservar lo fabricado en el altar) y al terminar una instancia.

- **¿Existen checkpoints?** Sí, el cuarto seguro de cada instancia.

- **¿Existe autoguardado?** Sí, solo en cuartos seguros (al entrar y al salir) y al final de cada instancia; no hay guardado manual ni checkpoint en la huida.

- **¿Existen diferentes partidas?** Un espacio de guardado en el MVP; tres en la versión deseable.

## 27. Planeación

Kickoff el 10 y 11 de octubre y sprints de una semana (lunes a domingo) del 12 de octubre al 13 de diciembre, con dos días de colchón antes de la entrega de mediados de diciembre. Todo el trabajo lo hace Jesús con Claude. El detalle de cada tarea vive en GitHub Issues (un hito por sprint); esta tabla es el resumen.

| **Spt** | **Fechas**     | **Meta**                                                    | **Issues principales** |
|---------|----------------|-------------------------------------------------------------|------------------------|
| 0       | 10 y 11 oct    | Kickoff                                                     | — |
| 1       | 12 a 18 oct    | Repo y dirección visual                                     | Repo, tablero y licencias (#1), paleta (#2), concepto del Cultista (#3) |
| 2       | 19 a 25 oct    | Proyecto base, referencias y primeros assets                | Cultista 3D (#4), plano de la instancia 1 (#5), bocetos de UI y menús (#6, #7), sonidos y música (#8–#10), proyecto de Unity (#11) |
| 3       | 26 oct a 1 nov | Caminar y pelear contra un Cultista; **Control 1**          | Controlador y linterna (#12), salud y combate (#13), inventario (#14), IA del Cultista (#15), blockout de la instancia 1 (#16), conceptos y Llorona 3D (#17, #18), props (#19), sonidos (#20), playtests (#21) |
| 4       | 2 a 8 nov      | Altar, crafteo, rezo y guardado                             | HUD e inventario (#22), altar (#23), escopeta (#24), rezo (#25), cuarto seguro (#26), encuentros (#27), brazos del Padre (#28), objetos clave (#29), plano de la instancia 2 (#30) |
| 5       | 9 a 15 nov     | Instancia 1 de inicio a fin; **Control 2**                  | Efectos de objetos (#31), notas y recetas (#32), textos (#33), ritual (#34), La Llorona (#35), nivel de la instancia 1 (#36), Charro 3D (#37), plano de la instancia 3 (#38) |
| 6       | 16 a 22 nov    | Instancia 2 completa                                        | Charro Negro (#39), nivel de la instancia 2 (#40), Tzitzimime 3D (#41), sonidos (#42, #43), feedback y VFX (#44) |
| 7       | 23 a 29 nov    | Instancia 3 y menús; **Control 3**; congelamiento de funciones | Tzitzimime (#45), nivel de la instancia 3 (#46), menús (#47), victoria, derrota y créditos (#48), congelamiento (#49), presentar `DEFENSA.md` (#66) |
| 8       | 30 nov a 6 dic | Pulido y playtest de las 3 instancias; **Control 4**        | Audio (#50), pasada de rúbrica y bugs (#51), `DEFENSA.md` completo (#52) |
| 9       | 7 a 13 dic     | Build final y entrega                                       | Build final (#58), presentación (#59) |
| Colchón | 14 y 15 dic    | Imprevistos y entrega                                       | — |

## 28. Riesgos del proyecto

| Riesgo                                                         | Solución propuesta                                                                                                                             |
|----------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------|
| El modelado de personajes y entidades consume demasiado tiempo | Generación con herramientas de IA 3D, rig automático y animaciones de biblioteca; en primera persona el Padre solo necesita brazos y equipo    |
| El Director de ritmo es difícil de balancear                   | Queda fuera del MVP (deseable): los encuentros y oleadas van por guion y, si entra, empieza con reglas simples por tiempo y salud (sección 21) |
| Cada entidad necesita scripting a mano                         | Una entidad por instancia, definida por fases y eventos programados                                                                            |
| Una sola persona hace todo el trabajo                          | Issues pequeños, assets de bibliotecas CC0 e IA 3D antes que modelar a mano, recortar primero lo deseable, y build cada viernes para detectar atrasos pronto |
| Dependencia de assets externos                                 | Packs low-poly con licencia abierta como base                                                                                                  |
| Balance muy fácil o imposible                                  | Playtest desde la semana 4, con el primer blockout                                                                                             |
| El equipo no sabe explicar lo que no hizo                      | `docs/DEFENSA.md` se escribe junto con cada sistema y Jesús lo presenta al equipo antes de la defensa (#66) |
| Finales y exámenes de diciembre                                | Congelamiento de funciones el 29 de noviembre (fin del sprint 7); los sprints 8 y 9 son solo pulido, bugs y build                              |

## 29. Testing

**Aspectos a evaluar:**

- Claridad de controles: si el jugador entiende el crafteo y el rezo.

- Que se entienda el objetivo de cada sección y de los objetos clave.

- Balance de las entidades en sus fases y de las huidas.

- Rendimiento (FPS) en la zona más pesada de cada instancia.

- Bugs de colisión en puertas, canales y trajinera.

- Experiencia general: si genera tensión sin frustrar.

**Registro:** una bitácora por sesión de playtest con fecha, problema encontrado y corrección aplicada. Primer playtest interno en el sprint 3, playtest externo de la instancia 1 en el sprint 5 y playtest de las 3 instancias en el sprint 8 (como en la tabla de la sección 27), además de la demo de cada viernes.

## 30. Criterios para considerar el juego terminado (MVP)

- [ ] Se juegan las 3 instancias de inicio a fin sin errores que impidan continuar.

- [ ] El Cultista y los jefes funcionan con su IA y sus fases.

- [ ] El altar craftea las recetas y cada una tiene un efecto notable.

- [ ] Existe condición de victoria (salir) y de derrota (vida en 0, vuelve al cuarto seguro).

- [ ] HUD, audio básico y guardado funcionan.

- [ ] Hay un ejecutable probado en una máquina distinta a la de desarrollo.

## Anexo: consideraciones culturales y de propiedad intelectual

| Tema                                 | Decisión                                                                                                        |
|--------------------------------------|-----------------------------------------------------------------------------------------------------------------|
| Chipote Chillón                      | No se usa: es un personaje protegido de El Chapulín Colorado. Se sustituye por la chancla bendita               |
| Santa Muerte                         | No aparece. El culto es ficticio (Hijos de la Estrella Negra) para no retratar una devoción real como villana   |
| Isla de las Muñecas                  | Es un lugar real ligado a Don Julián Santana. Se toma como inspiración, con diseño propio y sin personas reales |
| La Llorona, Charro Negro, Tzitzimime | Figuras de tradición popular y prehispánica. Se respetan sus leyendas y no se parodian                          |
| Religión                             | El Padre y los rituales cristianos y prehispánicos se tratan con seriedad; el mal lo encarna el culto ficticio  |
| Clasificación                        | ESRB M por violencia y horror; sin contenido sexual                                                             |
