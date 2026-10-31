# Textos del juego (issue #33)

Todas las notas del culto y lo que dice el Padre Anselmo Quiroz, en un solo lugar para revisarlas.
Lo que se ve en el juego vive en Unity:
- notas en `Assets/_Project/Data/Notes/NOTE_*.asset`;
- frases que responden a lo que pasa en `Data/Textos/PadreLines.asset`;
- frases de lugar en los `SubtitleTrigger` de cada nivel.

Si se cambia un texto aquí, hay que cambiarlo también allá (y al revés).

**Tono.** El Padre es un exorcista cansado, creyente y con humor seco; habla poco y en voz baja. Los Hijos de la Estrella Negra escriben como fanáticos: frases cortas, solemnes, en plural. Las leyendas (la Llorona, el Charro Negro, las tzitzimime) se tratan con respeto: dan miedo, no risa.

**El trasfondo que cuentan las notas.** Los Hijos de la Estrella Negra quieren que las tzitzimime, los seres de las estrellas que según los mexicas bajarían cuando el sol se debilitara, desciendan al fin del ciclo. Para eso necesitan abrir tres "anclas":
- el llanto de la Llorona en Xochimilco;
- el pacto del Charro Negro en la hacienda;
- la obsidiana del ex-convento, donde bajará la Tzitzimime.

Hace veinte años otro sacerdote, el Padre Ignacio, casi los detuvo.

## Notas

| # | Dónde | Título | Enseña |
|---|---|---|---|
| 1 | Xochimilco · embarcadero | Nota del embarcadero | — |
| 2 | Xochimilco · chinampa de las muñecas | Diario de un hermano | — |
| 3 | Xochimilco · isla | Página arrancada | — |
| 4 | Xochimilco · ancla, al vencer a la Llorona | Lo que dejó la Llorona | Cruz de plata |
| 5 | Hacienda · cocina de la casa grande | Recado de la cocinera | Chancla bendita |
| 6 | Hacienda · panteón | Hoja arrancada de un misal | — |
| 7 | Ex-convento · claustro | Códice del prior | Copal |
| 8 | Ex-convento · criptas | Sermón de la Estrella Negra | — |

### 1. Nota del embarcadero
> Tres ofrendas para la que llora en el agua. Una en el embarcadero, otra entre las muñecas de la chinampa y la última en la isla, junto al árbol viejo.
>
> Cuando las tres estén en su lugar, ella despertará y la estrella negra brillará sobre el canal. No la miren a la cara. Ella busca a sus hijos y cualquiera le sirve.

### 2. Diario de un hermano
> Las muñecas miran aunque no tengan ojos. El cuidador las colgaba para alejarla; nosotros las colgamos para llamarla.
>
> Anoche el hermano Rufino regó sal sin querer al cargar los costales. Ella llegó hasta la línea blanca y no pasó. Se quedó ahí, llorando, hasta que amaneció. No se lo digan al Gran Hermano.

### 3. Página arrancada
> No te acerques al agua cuando la oigas llorar. Las veladoras del ancla la obligan a quedarse quieta mientras se reza: enciéndelas todas antes de empezar.
>
> El sacerdote que vino hace veinte años casi lo logra. Se llamaba Ignacio. Dicen que todavía reza en algún lado.

### 4. Lo que dejó la Llorona
> Plata y madera: una cruz como la que usaban los abuelos contra el jinete negro. Cargada con fe, lo hiere cuando cruza el agua bendita.
>
> — escrito con la letra del Padre Ignacio, en el reverso de una estampa empapada.

### 5. Recado de la cocinera
> Mi abuela decía que no hay espanto que aguante un chanclazo bendito. Échale agua bendita a la chancla y verás cómo corren.
>
> Y ya no le abran la puerta al señor del caballo negro, por más que ofrezca. Nadie ha cobrado nunca lo que él paga.

### 6. Hoja arrancada de un misal
> El jinete no pisa la casa de Dios. Pon las tres monedas en el pedestal de la capilla y llena con agua bendita las canaletas del frente.
>
> Cuando las cruce, golpéalo con la cruz de plata. Tres veces, o volverá. — I.

### 7. Códice del prior
> La que baja de las estrellas no se ve. Resina y brasas: el humo del copal la muestra, y solo mostrada puede herirla la luz.
>
> Los antiguos la esperaban en los eclipses, cuando el sol parece morir. Estos locos quieren que baje para siempre.

### 8. Sermón de la Estrella Negra
> Hermanos: la mujer que llora ya abrió el agua; el jinete ya firmó con plata. Falta la piedra.
>
> Bajo este convento, la obsidiana espera. Cuando las tres piedras negras estén en su lugar y el último cura rece su última oración, ella descenderá del techo del mundo y no volverá a amanecer.
>
> No teman al sacerdote. Ignacio también rezó.

## Frases del Padre

### Cuando pasa algo (`PadreLines`)
| Momento | Frases (se elige una al azar) |
|---|---|
| Vida baja (30 o menos; como mucho una vez por minuto) | «Todavía no, Señor. Todavía no.» · «Necesito una venda… o un milagro.» |
| Primer rezo completo | «Que este círculo sea muralla.» |
| Reaparecer tras morir | «Otra vez de pie. Él no ha terminado conmigo.» · «Levántate, Anselmo.» |
| Primera vez en cada cuarto seguro | «Aquí no entran. Respira.» · «Un altar. Gracias a Dios.» |
| Primer objeto clave | «Una ofrenda. Me la llevo antes de que alguien la use.» |
| Segundo objeto clave | «Otra. Ya casi.» |
| Tercer objeto clave | «Las tres. Ahora, al ancla.» |
| Empieza el ritual | «Padre nuestro, que estás en el cielo…» |
| Termina el ritual | «Se acabó. Ahora hay que salir de aquí.» |

### Al llegar a un lugar (`SubtitleTrigger`)
| Instancia | Lugar | Frase |
|---|---|---|
| Xochimilco | Inicio | «Xochimilco… dicen que de noche nadie navega estos canales.» |
| Xochimilco | Chinampa de las muñecas | «Cuántas muñecas. Y todas me están mirando.» |
| Xochimilco | Choza del cuidador | «La choza del cuidador. Aquí puedo descansar un momento.» |
| Xochimilco | Isla | «El árbol viejo. El ancla debe estar cerca.» |
| Hacienda | Inicio | «La hacienda. Atardece y ya huele a azufre.» |
| Hacienda | Trojes | «Alguien guardó algo más que maíz aquí.» |
| Hacienda | Capilla | «Su casa. Aquí el jinete no entra.» |
| Hacienda | Panteón | «Descansen en paz… si los dejan.» |
| Ex-convento | Inicio | «El convento. Ignacio vino aquí hace veinte años y no salió.» |
| Ex-convento | Coro alto | «Desde aquí cantaban las monjas. Ahora solo hay polvo.» |
| Ex-convento | Criptas | «Más abajo. Siempre más abajo.» |
| Ex-convento | Catacumbas | «Aquí la esperan. Que Dios me acompañe.» |
