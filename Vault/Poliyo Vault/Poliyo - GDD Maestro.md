# POLIYO — Game Design Document maestro

**Estado:** Diseño de preproducción  
**Versión:** 1.0  
**Fecha:** 21 de julio de 2026  
**Responsable del proyecto:** Tobias  
**Motor previsto:** Unity  
**Plataforma inicial:** PC / Steam  
**Modelo comercial:** Juego premium  
**Modo prioritario:** Un jugador  
**Idioma base:** Español rioplatense  

---

## 0. Control del documento

### 0.1 Propósito

Este documento define la experiencia, los sistemas, el contenido y el alcance de **Poliyo**. Es la base de diseño para preproducción, producción, pruebas y balance. No contiene código ni obliga a una arquitectura técnica concreta.

### 0.2 Autoridad de las fuentes

Ante una contradicción se aplica este orden:

1. Decisiones y correcciones explícitas más recientes de Tobias en la conversación de diseño.
2. Este GDD, como consolidación vigente de esas decisiones.
3. `Referencias/Poliyo_MD_Primer_GDD.md`, como borrador y fuente complementaria.
4. Los mockups de `Referencias/Imagenes`, como referencias de estructura y navegación.

Los mockups no fijan la paleta final. Su fondo oscuro y sus trazos representan distribución funcional, no la identidad cromática definitiva.

### 0.3 Convenciones

- **CERRADO:** forma parte del diseño aprobado.
- **BALANCE:** el sistema está aprobado; sus cifras se ajustarán en pruebas.
- **CONTENIDO:** requiere escritura, arte o configuración, no una nueva decisión de sistema.
- **POST-MVP:** pertenece a la visión futura y no debe bloquear el primer lanzamiento jugable.
- **DESCARTADO:** no debe implementarse salvo reapertura explícita.

### 0.4 Principio de alcance

Una idea nueva solo entra al MVP si mejora directamente el núcleo de campaña, reemplaza una solución más costosa o resuelve una necesidad que este documento no cubre. El diseño debe favorecer profundidad emergente mediante la combinación de sistemas, no mediante una acumulación ilimitada de minijuegos.

---

# 1. Resumen ejecutivo

## 1.1 Concepto

**Poliyo** es un juego de estrategia, simulación electoral y sátira política ambientado en la ficticia República Federal de Roscalia. El jugador funda un partido, crea a su candidato, arma un equipo lleno de virtudes y secretos, consigue financiación y dirige una campaña presidencial contra cuatro partidos rivales.

El objetivo es ganar la presidencia en primera vuelta o balotaje. Para lograrlo hay que interpretar un electorado distinto en cada partida, elegir dónde y cómo hacer campaña, sobrevivir a crisis y escándalos, negociar con rivales, administrar fondos y decidir hasta dónde se está dispuesto a llegar.

## 1.2 Fantasía del jugador

> Construir desde cero una candidatura nacional, manipular o comprender el caos político de Roscalia y llegar a la presidencia sin que los rivales, los aliados, los inversores o las propias decisiones destruyan la campaña.

## 1.3 Propuesta diferencial

El centro de Poliyo no es encontrar una secuencia fija de respuestas correctas. Es gobernar bajo **información imperfecta**:

- Las afinidades del electorado cambian por semilla.
- Las encuestas pueden equivocarse, tener sesgo o estar manipuladas.
- Las estadísticas decisivas de equipo e inversores permanecen ocultas al seleccionarlos.
- La confianza no equivale a intención de voto.
- Los rivales recuerdan acuerdos, ataques y humillaciones.
- Durante la Niebla Electoral el jugador debe decidir sin porcentajes confiables.

## 1.4 Pilares de diseño

1. **Cada campaña cuenta una historia diferente.** La semilla modifica electorado, agenda, relaciones, equipo, financiación, eventos y comportamiento rival.
2. **Toda decisión favorece a alguien y molesta a otro.** No existen respuestas universalmente óptimas.
3. **La información es un recurso.** Investigar, confirmar y arriesgar una filtración es tan importante como gastar dinero.
4. **Las personas son sistemas, no bonificaciones.** Un integrante competente puede ser desleal; uno problemático puede salvar la campaña.
5. **El mapa importa.** El efecto de una promesa depende de dónde se dice, quién la escucha y qué necesita esa localidad.
6. **El caos debe poder explicarse.** El mundo puede ser cínico e injusto, pero el resultado no debe sentirse como una tirada arbitraria.
7. **El humor convive con consecuencias reales.** El absurdo político puede ser gracioso; la violencia, el abuso o la explotación no se convierten automáticamente en un chiste.

## 1.5 Género, cámara y presentación

- Estrategia y simulación de campaña.
- Gestión ligera de recursos y relaciones.
- Eventos narrativos con decisiones temporizadas.
- Interfaz estratégica 2D.
- Escenas especiales 3D estilizadas.
- Presentación general 2.5D, con posibilidad de composición isométrica en espacios estratégicos.

## 1.6 Público y duración

- Público principal: jóvenes y adultos jóvenes interesados en política, humor de internet, estrategia accesible y alta rejugabilidad.
- Orientación etaria prevista: 16+ o equivalente, sujeta a la clasificación final por violencia, drogas, lenguaje y temas adultos.
- Duración objetivo del MVP: entre dos y tres horas por campaña completa.
- La campaña puede continuarse en varias sesiones.

## 1.7 Modelo comercial

- Lanzamiento premium en PC/Steam.
- Sin publicidad ni microtransacciones dentro del juego.
- Las expansiones o contenido adicional pertenecen a una decisión comercial posterior.

---

# 2. Referentes y oportunidad

Los siguientes juegos funcionan como referencias parciales, no como modelos a copiar:

| Referente | Aprendizaje útil | Diferencia de Poliyo |
|---|---|---|
| Democracy 4 | Legibilidad de relaciones entre políticas y grupos | Poliyo se concentra en ganar una campaña, en personajes ocultos y en escenas narrativas, no en gobernar mediante paneles macroeconómicos |
| Suzerain | Consecuencias políticas, relaciones y tono adulto | Poliyo es sistémico, rejugable y menos lineal; el jugador crea su partido y disputa un mapa electoral variable |
| The Political Process | Fantasía electoral y lectura territorial | Poliyo prioriza sátira rioplatense ficticia, incertidumbre informativa, operaciones y presentación 2.5D |

La oportunidad del proyecto está en combinar una simulación electoral comprensible con el ritmo de un juego narrativo de decisiones y el lenguaje cultural de la política argentina, sin depender de nombres reales ni quedar encerrado en un solo mercado.

---

# 3. Alcance

## 3.1 MVP

El MVP incluye:

- Una campaña presidencial de 60 días.
- Cinco candidatos: jugador y cuatro rivales.
- Seis jurisdicciones y 24 localidades.
- Primera vuelta y posible balotaje de aproximadamente dos semanas.
- Tres dificultades.
- Creación de partido y candidato.
- Brújula ideológica dinámica.
- Economía, afiliados e inversor.
- Equipo partidario con rasgos ocultos.
- Calendario, mapa, noticias y panel de equipo.
- Actos, entrevistas, negociaciones y Mesa de campaña.
- Tareas delegadas.
- Eventos, crisis y piquetes.
- Medios, periodistas, rumores, investigaciones y operaciones.
- Debate presidencial y debate de balotaje.
- Niebla Electoral, veda, Día de Elección hora a hora y escrutinio.
- Fraude limitado, trazable y coherente con la campaña.
- Guardado y carga.
- Pantalla final y revelación del equipo.

## 3.2 Visión completa posterior

La campaña de la versión completa podrá extenderse a seis meses, siempre que el contenido y el ritmo sostengan la duración. El MVP utiliza 60 días para validar el núcleo sin producir seis meses de repetición.

## 3.3 POST-MVP

- Modo local con varios jugadores en una misma PC.
- Multijugador online.
- En online, cada participante termina sus decisiones y pulsa **Siguiente día**; el calendario no avanza hasta que todos estén listos.
- Más jurisdicciones, localidades y contenido.
- Nuevos partidos, medios, eventos y arquetipos.
- Soporte de mods, si la arquitectura y la comunidad lo justifican.

## 3.4 Fuera del MVP

- Gobernar Roscalia después de ganar.
- Elecciones legislativas, provinciales, municipales o internas.
- PASO jugables.
- Cooperativo.
- Políticos contemporáneos reales.
- Simulación administrativa de comida, limpieza o servicios menores.
- Contabilidad fiscal detallada.
- Sistema de energía del candidato.
- Viajes como minijuego o acción separada.
- Voces completas para todos los diálogos.
- Fraude mesa por mesa.
- Finales alternativos considerados victoria.

---

# 4. Mundo y tono

## 4.1 Roscalia

**Nombre oficial:** República Federal de Roscalia  
**Nombre habitual:** Roscalia  
**Capital:** Distrito Federal de Puerto Alba

Roscalia tiene instituciones y reglas electorales inspiradas en Argentina, pero su geografía política, sus personajes y su historia son ficticios. Esto permite conservar el humor reconocible y, al mismo tiempo, construir una identidad exportable.

## 4.2 Gobierno saliente

El gobierno que organiza la transición no pertenece a ninguno de los cinco partidos candidatos.

- Llega al final de una mala gestión y tiene baja aprobación.
- Su coalición quedó debilitada o se disolvió.
- No presenta candidato presidencial.
- Sigue tomando decisiones hasta el traspaso.
- Puede ocultar información, empeorar o contener crisis, sufrir renuncias y negociar discretamente.
- Conserva al menos un logro o sector de apoyo para evitar que atacarlo sea siempre la respuesta correcta.

La causa de su fracaso se determina por semilla entre crisis económica, corrupción, inseguridad, deterioro de servicios, conflicto social, aislamiento exterior o una combinación.

## 4.3 Tono narrativo

- Humor político absurdo, oscuro, exagerado y cínico.
- Titulares agresivos, memes, propaganda y burlas de internet.
- Colores y presentación con vida; la oscuridad está en la escritura, no en cubrir todo con una paleta gris.
- La sátira apunta al poder, la burocracia, los incentivos perversos y el oportunismo.
- Los delitos graves pueden ser acusaciones verdaderas, falsas o parciales; no se tratan todos como remates cómicos.
- Los nombres, retratos y biografías no deben acusar de delitos a personas reales identificables.

## 4.4 Lenguaje

El texto usa español rioplatense y un registro accesible. La acción extrema de asesinar se muestra al público como **Desvivir**, en sintonía con el humor y el lenguaje de redes, sin ocultar dentro del diseño que se trata de violencia letal ni reducir por sí sola la clasificación etaria.

---

# 5. Partidos y candidatos

## 5.1 Contienda inicial

| Partido | Posición base | Inspiración satírica | Fortaleza reconocible | Debilidad reconocible |
|---|---|---|---|---|
| Partido del jugador | Centro al comenzar; evoluciona | Creación del jugador | Adaptabilidad | Falta de identidad y estructura inicial |
| Liberales | Extrema derecha | Espacio libertario | Campaña digital, viralidad y militancia online | Mala relación con medios tradicionales y respuestas polarizantes |
| CONTR | Centro derecha | Espacio conservador republicano | Relaciones empresariales e imagen de gestión | Rigidez, internas y dificultad para conectar fuera de sectores acomodados |
| Zurditos | Extrema izquierda | Frente de izquierda | Militancia, calle y coherencia ideológica | Techo electoral, recursos limitados y alto rechazo en ciertos sectores |
| Federales | Izquierda | Tradición peronista/federal | Territorio, sindicatos y capacidad de negociación | Contradicciones internas, aparato cuestionado y desgaste histórico |

Las fortalezas y debilidades son tendencias, no resultados garantizados. Ningún grupo social o económico pertenece siempre al mismo partido. La semilla define qué sectores están receptivos a cada candidatura.

## 5.2 Brújula política

La brújula tiene dos ejes:

- Horizontal: izquierda ↔ derecha.
- Vertical: progresista ↔ conservador.

El partido del jugador comienza en el centro. No elige una ideología inicial mediante un formulario. La posición se mueve por:

- Respuestas en entrevistas y debates.
- Declaraciones en actos.
- Propuestas anunciadas.
- Alianzas.
- Respuestas a crisis.
- Operaciones y decisiones éticas o institucionales.

Cambiar de posición es válido, pero las contradicciones frecuentes reducen confianza y abren oportunidades de ataque. Una campaña puede terminar, por ejemplo, como izquierda progresista aunque haya comenzado en el centro.

Los rivales tienen zonas ideológicas de origen, pero su personalidad, radicalidad y conducta varían dentro de rangos compatibles con su partido.

## 5.3 Personalidad rival

Cada candidato rival recibe por semilla una combinación de:

- Confrontativo.
- Moderado.
- Calculador.
- Oportunista.
- Institucional.
- Autoritario.
- Carismático.
- Técnico.
- Impulsivo.

La ideología filtra qué combinaciones son válidas. La personalidad modifica el tono y las probabilidades de decisión, pero no vuelve completamente predecible al rival.

## 5.4 Nombres y apellidos

La fuente de nombres es `Referencias/Archivos/nombres_y_apellidos_ficticios.xlsx`.

- El catálogo actual contiene 60 nombres y 80 apellidos.
- Los nombres pueden repetirse.
- Ningún apellido puede repetirse entre dos personajes de una misma campaña.
- El apellido elegido por el jugador se reserva antes de generar el resto.
- Los familiares también deben respetar la unicidad global de apellidos; la relación puede justificarse narrativamente por parentesco materno, matrimonio u otro vínculo.
- Los rivales y personajes de IA combinan nombre y apellido al iniciar la partida.
- Con el catálogo actual, el límite seguro es de 80 personajes con apellido por campaña.

## 5.5 Apariencia

El jugador personaliza rostro, tono de piel, pelo, barba, bigote, accesorios y vestuario. Los candidatos de IA parten de diseños visuales predefinidos o de un catálogo curado, aunque sus nombres y parte de su personalidad cambien.

---

# 6. Creación de partida

## 6.1 Flujo

1. Menú principal.
2. Nueva campaña.
3. Dificultad.
4. Creación del partido.
5. Creación del candidato.
6. Elección de origen territorial.
7. Elección de inversor entre tres tarjetas.
8. Formación del equipo, puesto por puesto.
9. Generación y confirmación de semilla.
10. Primera Mesa de campaña.

## 6.2 Creación del partido

El jugador define:

- Nombre libre.
- Sigla libre.
- Uno o dos colores principales.
- Un logo entre ocho y diez opciones preparadas.
- Localidad de origen en cualquier punto disponible del país.

No se crea:

- Eslogan.
- Historia fundacional.
- Emblema adicional separado del logo.
- Ideología inicial manual.

La localidad de origen recibe bonificaciones iniciales de confianza, intención, afiliados y estructura. La magnitud exacta es **BALANCE**.

## 6.3 Estado inicial variable

La dificultad y la semilla determinan dentro de rangos:

- Afiliados iniciales.
- Reputación.
- Estructura territorial.
- Fuentes menores de financiación.
- Calidad de los indicios sobre equipo e inversores.

El objetivo es que dos partidos con el mismo nombre y origen no comiencen campañas idénticas.

---

# 7. Bucle principal y tiempo

## 7.1 Bucle de campaña

```text
Leer el país y la semana
        ↓
Planificar actividades y delegar tareas
        ↓
Actuar, responder y gastar recursos
        ↓
Medios, rivales y electorado interpretan lo ocurrido
        ↓
Recibir consecuencias, información y nuevos problemas
        ↓
Ajustar la estrategia
```

## 7.2 Bucle de decisión de 30 segundos

En su forma más pequeña, el jugador:

1. Lee una pregunta, noticia o información territorial.
2. Interpreta quién puede verse afectado.
3. Elige una respuesta, acción o silencio.
4. Observa reacciones inmediatas, no siempre el resultado electoral exacto.
5. Incorpora esa experiencia a la siguiente decisión.

## 7.3 Campaña del MVP

- Duración: aproximadamente 60 días.
- Primera vuelta: último día del mes electoral.
- Niebla Electoral: comienza aproximadamente 30 días antes de la elección.
- Balotaje: comienza al día siguiente de la primera vuelta y dura cerca de dos semanas.
- Niebla del balotaje: última semana.

## 7.4 Semana

Cada semana comienza con una **Mesa de campaña** obligatoria y gratuita. Allí se:

- Resume la semana anterior.
- Revisa dinero, compromisos, equipo y noticias.
- Presenta una hoja de ruta de la semana siguiente.
- Eligen dos o tres actividades principales del candidato.
- Asignan tareas paralelas al equipo.
- Resuelven conflictos internos urgentes.

La Mesa no consume una de las actividades principales.

## 7.5 Día

El día se representa mediante mañana, tarde, noche y cierre informativo. No se exige completar una acción en cada franja. Una actividad puede ocupar una franja y bloquear otra por preparación o logística.

No existe energía. Los límites son:

- Calendario.
- Dinero.
- Distancia y logística.
- Disponibilidad del medio o interlocutor.
- Preparación.
- Integrantes ocupados.
- Saturación mediática o territorial.
- Compromisos anteriores.

El jugador puede avanzar sin programar una actividad y asumir la pérdida de presencia que esto ocasione.

---

# 8. Mapa electoral

## 8.1 Navegación

El mapa tiene tres niveles:

1. **País:** seis jurisdicciones y ubicación actual de los cinco candidatos.
2. **Jurisdicción:** sus cuatro localidades.
3. **Localidad:** ficha detallada, actores, eventos, costos y acciones disponibles.

La ubicación de rivales es pública. Coincidir con un adversario puede provocar competencia por atención, incidentes, contraprogramación, una oportunidad de negociación o una confrontación.

## 8.2 Información de localidad

La ficha puede mostrar, según la información disponible:

- Población simulada o peso electoral.
- Estructura social y económica promedio.
- Rangos de edad.
- Temas prioritarios.
- Candidato preferido estimado.
- Confianza por candidato.
- Intención de voto.
- Rechazo.
- Probabilidad de participación.
- Estructura territorial de cada partido.
- Costo de una actividad.
- Influencia potencial de la presencia del candidato.
- Eventos activos.
- Calidad y antigüedad de los datos.

Durante la Niebla Electoral no se muestran valores exactos de confianza ni intención. El mapa, las ubicaciones, la composición histórica y los eventos siguen disponibles.

## 8.3 Jurisdicciones y localidades

### Provincia de Gran Ribera

Principal provincia ganadera, cerealera y agroexportadora. Tiene ingresos medios, inversión concentrada, trabajadores rurales y fuerte dependencia logística.

- **San Laureano:** capital administrativa y de servicios agroexportadores.
- **Las Espigas:** núcleo sojero y cerealero.
- **Las Aguadas:** zona ganadera, tradicional y dispersa.
- **Estación Ribera:** silos, ferrocarril, camioneros y logística.

### Provincia de Sierra Clara

Provincia septentrional con reservas minerales, inversión externa y bajos ingresos.

- **Santa Elvira:** capital, comercio, universidad y empleo público.
- **San Aurelio:** grandes proyectos mineros.
- **Piedra Seca:** ciudad obrera dependiente de la minería.
- **Quebrada Honda:** comunidades aisladas, minería informal y pobreza estructural.

### Distrito Federal de Puerto Alba

Capital nacional, sede del Congreso, centro mediático y único gran puerto de ingreso de mercadería. Combina gran riqueza con barrios de bajos recursos y tiene los costos de campaña más altos.

- **Casco Federal:** Gobierno, Congreso, medios y funcionarios.
- **Altos del Alba:** finanzas y residencias de altos ingresos.
- **Dársena Vieja:** puerto, depósitos, industria y sindicatos.
- **Bajo del Faro:** alta densidad y bajos recursos.

### Provincia Austral de Ventisca

Sur frío y turístico, con agricultura limitada y problemas de conexión y abastecimiento.

- **Nueva Aurora:** capital, aeropuerto y servicios.
- **Cerro Níveo:** turismo de lujo y centros de esquí.
- **Lago Sereno:** turismo familiar y pequeños comercios.
- **Paso Blanco:** población aislada y trabajo temporal.

### Provincia del Monte Rojo

Provincia montañosa, poco poblada y pobre, reconocida por su monte rojizo y el trekking.

- **Rojalba:** capital y centro de servicios.
- **Villa Cardenal:** núcleo turístico.
- **Paso del Cóndor:** territorio disperso y difícil de alcanzar.
- **Piedra Sola:** localidad remota y de pobreza extrema.

### Provincia de Cumbre Dorada

Provincia árida, metalúrgica y petrolera, con poca diversificación, dependencia empresarial y empleo público.

- **Villa Áurea:** capital y oficinas petroleras.
- **San Crisol:** industria metalúrgica y sindical.
- **Pozo Negro:** ciudad petrolera monoproductiva.
- **Meseta Seca:** desempleo, abandono y dependencia estatal.

## 8.4 Regla de variación

La identidad económica de cada provincia actúa como límite, no como guion fijo. La población varía poco entre partidas; el perfil de sus microelectores, preferencias, confianza y agenda puede variar mucho dentro de rangos coherentes.

---

# 9. Electorado

## 9.1 Unidad de simulación

El electorado combina individuos simulados y bloques sociales. Para que el resultado sea legible y escalable, cada **microelector** representa una persona o un pequeño bloque con peso poblacional.

Cada microelector posee:

- Localidad.
- Peso electoral.
- Grupo económico.
- Grupo social principal y etiquetas secundarias.
- Rango de edad.
- Punto ideológico.
- Prioridades temáticas.
- Confianza, intención, rechazo y entusiasmo por candidato.
- Participación base.
- Memoria de hechos relevantes.

Los resultados territoriales siempre se ponderan por población o peso electoral. Una localidad con muchos registros no debe valer más únicamente por tener más objetos simulados.

## 9.2 Grupos económicos

Los nombres visibles aprobados son:

- **Pobres**.
- **Clase media**.
- **Pudientes**.
- **Chetos**.

Una localidad puede tener un grupo dominante y varios secundarios.

## 9.3 Grupos sociales

Catálogo inicial combinable:

- Progresistas.
- Tradicionalistas.
- Sindicalistas.
- Trabajadores industriales.
- Productores rurales.
- Empleados públicos.
- Comerciantes y emprendedores.
- Estudiantes.
- Jubilados.
- Religiosos.
- Ambientalistas.
- Fuerzas de seguridad.
- Despolitizados.

Estas etiquetas no determinan por sí solas el voto. Un progresista puede priorizar seguridad; un productor rural puede votar a la izquierda; un cheto puede rechazar a la derecha. La semilla y la campaña construyen esas relaciones.

## 9.4 Temas políticos

1. Economía y trabajo.
2. Salud y protección social.
3. Educación, ciencia y cultura.
4. Seguridad y justicia.
5. Instituciones y corrupción.
6. Tecnología y futuro productivo.
7. Relaciones exteriores y defensa.
8. Federalismo, infraestructura y vivienda.
9. Ambiente y energía.

En cada campaña algunos temas son dominantes, otros secundarios y otros latentes. Las crisis pueden cambiar la agenda.

## 9.5 Confianza e intención de voto

**Intención de voto** es la preferencia actual. Puede surgir por ideología, propuesta, origen territorial, rechazo a otro candidato o voto útil.

**Confianza** expresa cuánto cree el elector que el candidato es capaz, coherente y fiable.

- Intención alta con confianza baja: voto frágil.
- Confianza alta con desacuerdo puntual: apoyo resistente.
- Confianza alta sin intención: oportunidad de conversión.
- Rechazo alto: limita la persuasión y moviliza voto en contra.

La confianza funciona como memoria y amortiguador, no como un segundo nombre para la intención.

## 9.6 Modelo de localidad

Ejemplo de plantilla, no contenido fijo:

**Casco Federal**

- Población simulada: 120.
- Nivel económico dominante: Pudientes/Chetos.
- Grupo social dominante: Progresistas.
- Edad aproximada: 27–68.
- Temas prioritarios: Educación, Salud y Seguridad.
- Ideología inicial: Centro derecha.
- Confianza por candidato: generada por semilla.
- Intención de voto: generada por semilla.
- Rechazo: generado por semilla.
- Probabilidad de participar: aproximadamente 88 %, con variación.

La identidad de Casco Federal puede conservar esos límites y producir una distribución diferente en cada campaña.

---

# 10. Motor electoral conceptual

## 10.1 Objetivo

El motor debe conseguir tres cosas a la vez:

1. Que el jugador pueda razonar sobre las consecuencias.
2. Que no pueda resolver el juego con una tabla fija de respuestas.
3. Que el ganador surja de la campaña y la semilla, no de una tirada final.

## 10.2 Escalas

- Confianza, rechazo, entusiasmo y participación: 0 a 100.
- Ideología: dos ejes de −100 a 100.
- Intención: distribución porcentual entre cinco candidatos, indecisos y voto en blanco.
- Memoria: efectos con intensidad, alcance y desgaste temporal.

Los rangos son de diseño; los coeficientes exactos son **BALANCE**.

## 10.3 Preferencia de un microelector

La preferencia por un candidato combina:

```text
Afinidad ideológica
+ compatibilidad con temas prioritarios
+ confianza y coherencia percibida
+ arraigo y presencia territorial
+ estructura partidaria
+ memoria de propuestas, hechos y relaciones
+ voto útil o rechazo a terceros
− rechazo al candidato
− contradicciones, escándalos y saturación
```

Los valores se normalizan entre candidatos, indecisos y voto en blanco. Ningún factor aislado entrega directamente puntos nacionales.

## 10.4 Impacto de una declaración o acción

```text
Impacto = alcance × relevancia del tema × compatibilidad del público
          × credibilidad × encuadre mediático × novedad
```

- **Alcance:** cuántas personas reciben el mensaje.
- **Relevancia:** cuánto importa el tema a ese microelector.
- **Compatibilidad:** cuánto coincide o choca la propuesta con sus preferencias.
- **Credibilidad:** confianza previa y coherencia del candidato.
- **Encuadre:** cómo lo presentan medios, rivales y redes.
- **Novedad:** reduce el rendimiento de repetir la misma promesa o visitar sin motivo la misma zona.

Una frase puede ser gradual o devastadora. Los impactos nacionales extremos requieren viralización, gran alcance o una sensibilidad social muy alta; no aparecen como cambios mágicos sin explicación.

## 10.5 Contexto territorial

La misma frase no produce el mismo resultado en todo el país. Prometer un aumento impositivo en una zona muy pobre puede ser interpretado de forma distinta según a quién se grave, cómo se explique, el nivel de confianza y las prioridades locales. El sistema evalúa el contenido y el contexto, no solo palabras positivas o negativas.

## 10.6 Rendimientos decrecientes

- Zona favorable: menor conversión, mayor entusiasmo, afiliación y participación.
- Zona competitiva: mayor capacidad de convertir indecisos.
- Zona adversa: permite reducir rechazo o ganar visibilidad, con mayor riesgo.
- Repetición: cada acción idéntica pierde eficacia y puede generar saturación.

## 10.7 Participación y voto final

La decisión de participar depende de:

- Participación base.
- Entusiasmo.
- Confianza.
- Movilización y estructura territorial.
- Importancia percibida de la elección.
- Crisis, clima político e incidentes.

Al cerrar las urnas, cada microelector o bloque resuelve su voto mediante el estado acumulado y la semilla persistente. El porcentaje final proviene de esos votos ponderados.

## 10.8 Voto en blanco y voto útil

- El voto en blanco compite con candidatos e indecisión.
- Las reglas de primera vuelta se calculan sobre votos afirmativos válidos.
- El voto útil gana peso cuando un elector rechaza fuertemente a un candidato con posibilidades reales.
- En balotaje, cada elector vuelve a evaluar a los dos finalistas; el apoyo de un candidato eliminado influye, pero no transfiere automáticamente sus votos.

## 10.9 Determinismo

La semilla controla generación inicial y decisiones aleatorias. Toda modificación queda registrada en el estado de campaña. Repetir la misma semilla y las mismas decisiones debe producir el mismo resultado.

---

# 11. Encuestas y Niebla Electoral

## 11.1 Información previa

Antes de la Niebla, el jugador accede a intención y confianza mediante mediciones con:

- Fecha.
- Muestra.
- Margen de error.
- Calidad de la consultora.
- Alcance nacional o territorial.
- Posible sesgo.
- Antigüedad.

```text
Encuesta publicada = estimación real + error + sesgo + posible manipulación
```

Poco antes del inicio de la Niebla se publica una última encuesta importante que sirve como fotografía de referencia, no como verdad garantizada.

## 11.2 Niebla Electoral

Comienza aproximadamente un mes antes de la primera vuelta.

- Se ocultan porcentajes exactos de confianza e intención.
- Los datos previos envejecen.
- El HUD reemplaza cifras por tendencias cualitativas.
- El jugador conserva composición social, prioridades, costos, ubicaciones y hechos conocidos.
- Investigación, territorio, prensa y equipo aportan señales, nunca certeza absoluta.

Estados posibles:

- Favorable.
- Competitivo.
- Adverso.
- Incierto.

## 11.3 Encuestas Cegadas

Durante la Niebla aparecen encuestas cuestionables y contradictorias. Una puede mostrar al jugador primero y otra último sin que haya ocurrido un cambio real.

No son números puramente decorativos: cada una tiene un modelo de error, sesgo, intereses y calidad. La contradicción debe enseñar al jugador a evaluar la fuente en lugar de obedecer el último gráfico.

## 11.4 Balotaje

La Niebla del balotaje comienza una semana antes de la votación final.

---

# 12. Economía de campaña

## 12.1 Dinero inicial

| Dificultad | Rango inicial |
|---|---:|
| Cagón | $420.000–$500.000 |
| Tibio | $350.000–$420.000 |
| Latam | $240.000–$350.000 |

El máximo inicial del MVP es $500.000. Las cifras son **BALANCE** y no representan una moneda real.

## 12.2 Ingresos

### Inversor

Al empezar se muestran tres tarjetas aleatorias de un catálogo de ocho arquetipos. El jugador elige una.

Información pública:

- Nombre y retrato.
- Actividad declarada.
- Reputación conocida.
- Aporte y periodicidad ofrecidos.
- Condiciones explícitas.
- Indicios narrativos.

Información oculta:

- Origen y legalidad real del dinero.
- Afinidad con otros partidos.
- Escándalos.
- Contraprestaciones futuras.
- Probabilidad de retiro o traición.
- Evidencia comprometedora.

Arquetipos:

1. Persona privada con dinero limpio.
2. Persona privada con negocio turbio.
3. Persona de actividad sospechosa y dinero no declarado.
4. Persona del ámbito público con dinero limpio.
5. Persona sin trabajo actual con fortuna legítima, como lotería o herencia.
6. Millonario con escándalos.
7. Persona que favorece a otro partido.
8. Empresario de alto renombre que espera algo a cambio.

### Afiliados

Las donaciones dependen de cantidad de socios, compromiso, confianza, economía de sus sectores y escándalos. Se consolidan al cierre mensual; el resumen semanal puede anticipar tendencia.

### Aliados

Un aliado puede inyectar una suma fija o un porcentaje según dificultad, relación y acuerdo. Todo aporte importante genera una memoria política y puede crear obligaciones.

## 12.3 Gastos

Gastos fijos:

- Sede partidaria.
- Estructura y sueldos.
- Servicios contratados.
- Periodistas sobornados.
- Personas pagadas para guardar silencio.
- Compromisos con inversores o aliados.

Gastos variables:

- Actos, transporte y logística.
- Publicidad y difusión.
- Encuestas.
- Investigaciones e infiltración.
- Operaciones.
- Recaudación.
- Contención legal y mediática de crisis.

## 12.4 Costos territoriales

El costo depende de localidad, escala, distancia, seguridad, estructura propia y eventos. Puerto Alba y el área del Congreso son especialmente caros. La localidad de origen y las zonas con estructura consolidada son más baratas.

## 12.5 Cierre mensual

Al finalizar cada mes se procesan:

1. Aportes del inversor y aliados.
2. Donaciones de afiliados.
3. Gastos fijos.
4. Compromisos vencidos.
5. Consecuencias por impago.

Si hay balotaje, el nuevo mes se procesa antes de comenzar sus dos semanas.

## 12.6 Dinero en cero

No hay derrota automática ni saldo negativo. El partido conserva acciones gratuitas, negociación y posibilidades de recaudar, pero no puede pagar actividades u operaciones. Los gastos fijos impagos generan consecuencias narrativas y funcionales.

---

# 13. Equipo partidario

## 13.1 Roles iniciales

1. Vicepresidente.
2. Jefe de campaña.
3. Jefe de prensa.
4. Vocero.
5. Coordinador territorial.
6. Responsable legal/contable.
7. Consultor político.
8. Jefe de Operaciones.

Pueden incorporarse otros miembros durante la campaña. El inversor aparece en el área de equipo/financiación, pero no ocupa un rol operativo asignable.

## 13.2 Selección por tarjetas

Para cada puesto aparecen tres tarjetas. El objetivo es elegir personas, no comparar hojas de estadísticas.

Visible:

- Nombre, retrato y trayectoria pública.
- Ideología declarada.
- Experiencia conocida.
- Relaciones o antecedentes públicos.
- Descripción narrativa con posibles pistas.

Oculto:

- Valores exactos de habilidades.
- Una a tres fortalezas reales.
- Costos o debilidades.
- Lealtad.
- Ambición.
- Estrés y tolerancia al conflicto.
- Relaciones secretas.
- Escándalos y delitos.
- Afinidad real con rivales.

La dificultad cambia la claridad de las pistas y la distribución de calidad, pero nunca transforma la pantalla inicial en una comparación numérica perfecta.

## 13.3 Regla de costes humanos

No todos esconden corrupción. Un coste puede ser ego, poca experiencia, rigidez ideológica, rivalidad interna, estrés, exigencia de poder, incompetencia puntual o una relación incómoda. Esto evita que despedir preventivamente sea siempre óptimo.

Tampoco todos poseen un escándalo. Una acusación puede ser verdadera, falsa o parcialmente cierta.

## 13.4 Ideología y evolución

La mayoría comienza cerca del jugador, pero no todos piensan igual. Sus afinidades cambian según promesas, alianzas, giros ideológicos, trato personal y resultados. Un integrante puede apoyar, discutir, filtrar, renunciar, construir poder propio o desertar.

## 13.5 Familiares

Un miembro del equipo puede ser familiar del candidato. El vínculo aumenta confianza personal o lealtad inicial, pero eleva el costo reputacional de favoritismo y dificulta responder a un escándalo.

## 13.6 Escándalo interno

Ante una acusación urgente, el jugador puede:

- Echar al integrante.
- Pedirle la renuncia.
- Defenderlo públicamente.

Según el caso también puede investigar, ganar tiempo o guardar silencio. La reacción depende de la evidencia, la relación, el grupo afectado y la coherencia previa.

## 13.7 Revelación final

La pantalla de cierre revela habilidades, efectos, lealtad, intereses, secretos y consecuencias que el jugador no descubrió durante la campaña.

---

# 14. Tareas delegadas

Cada integrante puede ejecutar una tarea mientras el candidato realiza sus actividades. Un integrante ocupado no puede realizar otra simultáneamente.

El resultado depende de rol, habilidad, experiencia, contactos, lealtad, relación, territorio y dificultad.

## 14.1 Catálogo del MVP

| Tarea | Objetivos | Riesgos principales |
|---|---|---|
| Contacto político | Mejorar relación, buscar apoyo, transmitir postura | Filtración, mensaje deformado, captación del integrante |
| Investigación | Obtener rumor, indicio o prueba | Coste, información falsa, exposición del encargo |
| Declaración en medios | Defender, explicar, atacar, mantener presencia | Contradicción, frase viral, protagonismo personal |
| Análisis de crisis | Entender causas, actores y respuestas | Llegar tarde o recibir un análisis sesgado |
| Recaudación | Conseguir aportes, socios o empresarios | Dinero problemático y nuevas obligaciones |
| Campaña territorial | Mejorar estructura, preparar acto, defender zona | Impacto bajo, incidente o apropiación personal del mérito |
| Captación de afiliados | Sumar donantes, voluntarios y estructura | Infiltrados, conflictos y afiliaciones de baja calidad |

La presencia personal del candidato debe ser más potente que una campaña territorial delegada, pero delegar permite cubrir el país.

---

# 15. Actividades del candidato

## 15.1 Catálogo principal

El MVP ofrece tres actividades programables:

1. Acto político.
2. Entrevista periodística.
3. Negociación política.

La Mesa de campaña es obligatoria y no ocupa una selección. Investigar suele ser una tarea del equipo o una acción contextual desde los paneles de personas y rivales.

## 15.2 Acto político

- Se realiza en cualquier localidad accesible.
- Su costo depende de lugar, escala, logística, seguridad y estructura.
- La escena dura cerca de cinco minutos reales.
- El candidato realiza entre tres y cinco declaraciones según escala y formato.
- No hay periodista haciendo preguntas.
- Las opciones abordan conexión local, tema/propuesta y cierre político.
- Puede mejorar intención, confianza, entusiasmo, afiliados y estructura.
- Puede fracasar por baja convocatoria, incidentes, saturación, contradicciones o presencia rival.

El jugador ve antes del acto la información que su campaña cree conocer sobre la localidad, no la respuesta correcta.

## 15.3 Entrevista

- Escena 3D con periodista y candidato.
- Entre tres y cinco preguntas.
- Tres respuestas posibles por pregunta.
- Suele ser gratuita, aunque un medio puede negar, posponer o condicionar la invitación.
- Permite anunciar propuestas, aclarar filtraciones, enfrentar polémicas o atacar rivales.
- Dura aproximadamente siete minutos reales.

## 15.4 Negociación

- Encuentro con rival, aliado, dirigente, empresario, medio o Gobierno saliente.
- Puede ser gratis si el otro actor invita.
- Si el jugador inicia, puede exigir dinero, información, favores o concesiones.
- Construye relaciones que influyen en ataques, filtraciones y apoyos de balotaje.
- Toda promesa queda registrada y puede ser exigida o filtrada.

## 15.5 Propuestas

No existe una actividad separada llamada “Anunciar propuesta”. Las propuestas se presentan en actos, entrevistas, debates, declaraciones o negociaciones. Su credibilidad depende de la brújula, la trayectoria y la confianza.

## 15.6 Inactividad y temporizador

Las decisiones de escena tienen tiempo limitado.

1. Aparecen las opciones.
2. Si el jugador demora, periodista, moderador o público lo apura.
3. Si continúa sin responder, pierde la pregunta o la escena termina.

Consecuencias:

- En entrevista, el periodista corta o convierte el silencio en noticia.
- En debate, se pierde el turno y los rivales aprovechan el vacío.
- En acto, el público se impacienta y puede retirarse, reduciendo confianza.

Las opciones de accesibilidad permiten extender o desactivar el temporizador sin eliminarlo del diseño base.

---

# 16. Medios, periodistas y noticias

## 16.1 Medios del MVP

| Medio | Periodista principal | Identidad editorial inicial |
|---|---|---|
| NT | Fidel | Canal nacional rápido, visual y de agenda diaria |
| Nación | Luján | Señal institucional, empresarial y de opinión |
| 5C | Silvester | Canal combativo, popular y editorializado |
| Cadena | Samu | Señal sensacionalista, urgente y callejera |
| N48 | Neiman | Canal oportunista, cambiante y centrado en primicias |

Son parodias ficticias inspiradas en formatos televisivos conocidos. Su sesgo exacto, relación con candidatos y temas preferidos puede variar por semilla sin perder su identidad de formato.

## 16.2 Acciones con medios

El jugador puede:

- Dar entrevistas.
- Ofrecer una exclusiva.
- Aclarar una filtración o polémica.
- Entregar información.
- Pedir que investiguen a un rival, aliado o integrante.
- Sobornar o comprar cobertura favorable.
- Pagar para atacar o silenciar un tema.

Comprar cobertura crea un gasto fijo o extraordinario, aumenta el riesgo de exposición y no garantiza obediencia eterna.

## 16.3 Ciclo de noticia

```text
Hecho → interpretación → publicación → amplificación
      → meme o respuesta → consecuencia → memoria
```

La banda de Noticias del HUD presenta constantemente:

- Titulares.
- Declaraciones de candidatos.
- Filtraciones.
- Resultados parciales de investigaciones.
- Memes.
- Publicaciones en redes sociales.
- Eventos territoriales y nacionales.

## 16.4 Redes sociales

Las redes no son un modo independiente en el MVP. Funcionan como capa de amplificación y reinterpretación. Una frase puede convertirse en meme, ser recortada, revivir días después o reforzar la fortaleza digital de Liberales.

## 16.5 Memoria mediática

Cada noticia registra tema, fuente, alcance, veracidad conocida, encuadre, fecha y candidatos afectados. El impacto se desgasta, pero puede reactivarse ante una contradicción, debate o nueva prueba.

---

# 17. Debates

## 17.1 Debate de primera vuelta

- Escena 3D con los cinco candidatos.
- Escenario, atriles o posiciones, moderación, público y cámaras.
- Se muestran las respuestas de rivales antes o después del turno del jugador según la ronda.
- El jugador puede caminar de forma limitada y realizar gestos de brazos, cabeza y cuerpo.
- Duración objetivo: unos diez minutos reales.

## 17.2 Fase temática

Cada bloque plantea una pregunta sobre economía, salud, educación, seguridad, instituciones, tecnología, relaciones exteriores, federalismo o ambiente.

- Tres respuestas por pregunta.
- La eficacia depende de público, brújula, propuestas, confianza y coherencia.
- No existe una opción universalmente correcta.

## 17.3 Cruce entre candidatos

Los rivales pueden atacar al jugador y entre sí. El jugador elige cómo responder y puede devolver el ataque.

Opciones contextuales:

- Negar.
- Defenderse.
- Justificar.
- Admitir parcialmente.
- Responsabilizar a un integrante.
- Contraatacar.
- Redirigir a una propuesta.
- Mostrar rumor, indicio o prueba.

## 17.4 Expediente político

Cada hallazgo conserva:

- Objetivo.
- Tema.
- Fuente.
- Certeza: rumor, indicio o prueba.
- Antigüedad.
- Estado de publicación.
- Usos anteriores.

Los rivales solo pueden usar información que realmente hayan obtenido. No generan una prueba porque la narrativa la necesite.

## 17.5 Preparación

No hay una actividad separada de preparación. La preparación emerge de propuestas, coherencia, investigaciones, relaciones, equipo y experiencia acumulada.

## 17.6 Debate de balotaje

Se reutiliza el escenario con dos candidatos, nuevas cámaras y una puesta más confrontativa. Incluye fase temática, cruces y uso de investigaciones acumuladas.

---

# 18. Relaciones y negociación política

## 18.1 Valor de relación

Cada actor recuerda:

- Acuerdos cumplidos e incumplidos.
- Ataques públicos.
- Favores.
- Filtraciones.
- Afinidad ideológica.
- Competencia territorial.
- Humillaciones en debates.
- Beneficios esperados.

Una relación puede ser cordial y desconfiada, hostil pero útil o cercana y oportunista; no se reduce a amistad.

## 18.2 Balotaje

Los candidatos eliminados deciden apoyar, rechazar, permanecer neutrales o negociar. Influyen:

- Conversaciones mantenidas durante toda la campaña.
- Acuerdos y promesas.
- Ataques y agravios.
- Afinidad ideológica.
- Conveniencia política.
- Preferencias independientes de sus votantes.

Un respaldo modifica percepción y estructura, pero no transfiere un porcentaje fijo.

---

# 19. Investigación e inteligencia

## 19.1 Objetivos

Se puede investigar a:

- Equipo propio.
- Inversor.
- Aliados.
- Rivales y sus integrantes.
- Periodistas y medios.
- Empresarios.
- Gobierno saliente.
- Organizadores de eventos y piquetes.

## 19.2 Métodos

- Pedir ayuda a un periodista: buen acceso, riesgo de publicación o traición.
- Enviar un infiltrado: información profunda, costo y riesgo altos.
- Auditoría legal/contable: sólida para dinero y documentos, más lenta.
- Contacto político: rápida, pero sesgada por el informante.
- Jefe de Operaciones: flexible, con mayor huella y consecuencias.

No hay límite numérico de investigaciones. El límite emerge de dinero, tiempo, personal, relaciones y exposición.

## 19.3 Calidad de evidencia

- **Rumor:** abre líneas de investigación o ataques riesgosos.
- **Indicio:** vuelve creíble una acusación, pero permite defensa.
- **Prueba:** alta fuerza pública y legal; aún puede discutirse su origen o contexto.

La evidencia puede ser verdadera, falsa, manipulada o incompleta. El juego diferencia la certeza interna de lo que cree el público.

## 19.4 Riesgo de filtración

Investigar también comunica intención. El periodista puede contar el encargo, el infiltrado ser descubierto y el objetivo lanzar una contraoperación.

---

# 20. Escándalos, filtraciones y polémicas

## 20.1 Catálogo temático

- Escándalo sexual.
- Corrupción.
- Pasado delictivo.
- Conexiones con narcotráfico.
- Violencia contra la pareja.
- Conexiones con otro partido.
- Infiltración partidaria.
- Narcotráfico encubierto.
- Red de trata de personas.
- Colocación de explosivos o terrorismo.
- Militancia anarquista presentada como polémica mediática.

No todo integrante o rival posee un elemento del catálogo. La militancia ideológica no equivale por sí sola a delito; puede convertirse en escándalo por el encuadre del medio o la intolerancia de ciertos sectores.

## 20.2 Veracidad

Cada caso puede ser:

- Verdadero.
- Falso.
- Parcialmente verdadero.
- Verdadero con culpable equivocado.
- Fabricado a partir de un hecho real.

La IA también decide si echa, pide la renuncia o defiende a un integrante según evidencia, personalidad, utilidad y costo electoral.

## 20.3 Respuesta pública

El efecto depende de gravedad, prueba, confianza previa, grupos afectados, coherencia, velocidad de respuesta, medio que publica y memoria de casos anteriores.

---

# 21. Operaciones políticas y acciones extremas

## 21.1 Libertad del jugador

El juego no impone una barrera moral o legal que deshabilite una acción porque sea ilícita. El jugador puede intentar operaciones cada vez más graves y enfrentar sus consecuencias.

La disponibilidad práctica depende de contactos, dinero, oportunidad e información. La libertad no significa éxito garantizado ni ausencia de trazabilidad.

## 21.2 Catálogo conceptual

- Infiltrar un partido.
- Plantar o filtrar información.
- Fabricar una acusación.
- Comprar cobertura.
- Sobornar.
- Pagar silencio.
- Sabotear una actividad.
- Espiar.
- Organizar una provocación.
- **Desvivir** a una persona.

Las operaciones se representan de forma abstracta. El juego no enseña procedimientos reales para cometer delitos.

## 21.3 Modelo de consecuencias

Toda operación registra:

- Costo.
- Probabilidad de éxito.
- Exposición.
- Huella o evidencia.
- Personas que conocen el encargo.
- Gravedad si se descubre.
- Beneficiarios y perjudicados.
- Posibilidad de represalia.

Una operación puede funcionar y descubrirse después, fracasar sin exposición, ser atribuida a un tercero o producir un efecto no deseado.

## 21.4 Violencia contra el jugador

Los rivales y actores hostiles también pueden intentar acciones extremas. La probabilidad de que el candidato jugador muera debe ser baja y estar precedida por señales o escalada.

Si el protagonista muere:

- La campaña termina inmediatamente.
- Se presenta una derrota especial.
- Se desbloquea un logro asociado.
- El resumen revela, cuando corresponda, la cadena que produjo el desenlace.

Si muere un rival, su vice puede reemplazarlo y heredar parcialmente estructura, apoyo y conflictos. Si muere un integrante, el puesto queda vacante o debe reemplazarse. Estos casos requieren **BALANCE** y contenido específico.

---

# 22. Eventos, crisis y piquetes

## 22.1 Frecuencia

- Noticias menores: pueden aparecer a diario.
- Evento interactivo relevante: alrededor de uno por semana.
- Crisis nacional: alrededor de una por mes.
- Evento extraordinario: consecuencia de cadenas, operaciones o relaciones.

La campaña no debe convertirse en una crisis obligatoria cada día.

## 22.2 Catálogo inicial

- Desastre natural.
- Guerra en un país vecino.
- Ola migratoria.
- Piquete.
- Huelga.
- Pobreza extrema como condición persistente.
- Escándalo propio o rival.
- Crisis sanitaria.
- Ataque político.
- Pedido comprometedor del inversor.
- Muerte o detención de un personaje.
- Saqueos.
- Ola de inseguridad.
- Conflicto productivo o de abastecimiento.
- Crisis institucional del Gobierno saliente.

## 22.3 Respuestas

Según el evento:

- Intervenir personalmente.
- Delegar.
- Investigar.
- Declarar.
- Negociar.
- Ignorar.

No hay una opción genérica de “resolver con dinero”. Las respuestas válidas dependen de contexto, equipo y relaciones.

## 22.4 Piquetes

Pueden surgir espontáneamente, ser organizados por un rival o ser provocados por el jugador.

El jugador elige:

- Provincia y punto estratégico.
- Causa pública.
- Escala: pequeña, mediana o grande.
- Responsable.
- Presencia de símbolos partidarios.
- Recursos logísticos.

Impactan tránsito, comercio, abastecimiento, confianza, intención, rechazo, medios y relaciones. Una bandera puede atribuir el piquete a un partido aunque no sea el organizador real.

Investigar revela organizador, financiación, infiltrados, provocadores e intención política mediante rumor, indicio o prueba.

---

# 23. Inteligencia de rivales

## 23.1 Principios

Los rivales:

- Usan las mismas restricciones generales de tiempo, dinero e información.
- No conocen datos ocultos del jugador sin obtenerlos.
- Eligen territorios según oportunidad, identidad, costo y presencia enemiga.
- Recuerdan acciones y relaciones.
- Cometen errores.
- Pueden sufrir sus propios escándalos y crisis.

## 23.2 Planificación semanal

Cada rival evalúa:

1. Amenazas y oportunidades territoriales conocidas.
2. Agenda temática.
3. Fondos y estructura.
4. Relaciones y conflictos.
5. Personalidad.
6. Un margen de variación determinado por semilla.

Después selecciona actividades, tareas y respuestas válidas mediante pesos. La aleatoriedad aporta variedad; la evaluación evita conductas absurdas constantes.

## 23.3 Estilos

- Confrontativo: ataca y ocupa medios.
- Moderado: negocia y reduce rechazo.
- Calculador: investiga y espera oportunidades.
- Territorial: prioriza actos y estructura.
- Oportunista: cambia de agenda con rapidez.
- Pasivo: participa menos y conserva recursos.

El partido modifica los pesos. Liberales deben ser especialmente competentes en campaña digital; su debilidad se concentra en medios tradicionales, no en redes.

## 23.4 Juego limpio sistémico

La dificultad aumenta agresividad, calidad de decisión, ambigüedad informativa y gravedad de errores. No entrega votos gratuitos ni permite omnisciencia.

---

# 24. Primera vuelta, veda y Día de Elección

## 24.1 Reglas de victoria

Un candidato gana en primera vuelta si:

- Supera el 45 % de votos afirmativos; o
- Obtiene al menos 40 % y diez puntos de diferencia sobre el segundo.

Si nadie cumple, los dos primeros pasan al balotaje.

## 24.2 Veda

La veda bloquea actos, entrevistas de campaña, anuncios y propaganda pública. Funciona como cambio de reglas, no como borrado de toda participación.

Durante la transición se puede:

- Revisar el equipo y el mapa.
- Recibir alertas.
- Preparar logística electoral.
- Responder internamente a incidentes.

No se pueden ganar puntos mediante una declaración pública ordinaria.

## 24.3 Día de Elección hora a hora

El último día cambia del avance diario a bloques horarios, inspirado en la tensión del cierre del mercado de fichajes de FIFA.

Fases sugeridas:

1. Apertura de urnas.
2. Primer informe de participación.
3. Media mañana.
4. Mediodía.
5. Últimas horas de votación.
6. Cierre de urnas.
7. Bocas de urna.
8. Escrutinio provisional.
9. Resultado oficial.

El jugador puede tomar decisiones finales de carácter operativo:

- Priorizar recursos territoriales.
- Resolver problemas logísticos.
- Enviar al equipo ante una irregularidad.
- Verificar o investigar una alerta.
- Reaccionar internamente a una boca de urna.
- Proteger una zona de posible fraude.

Estas decisiones afectan participación, información, estructura y fraude; no permiten pronunciar una frase mágica que cambie la intención de todo el país después de abierta la elección.

## 24.4 Bocas de urna

Se muestran filtraciones y estimaciones contradictorias. La fuente y calidad importan. La tensión proviene de no saber cuál creer, no de recalcular aleatoriamente al ganador.

## 24.5 Escrutinio

El resultado ya está determinado por votos ponderados, participación, acciones del día y fraude acumulado.

Presentación:

- Porcentaje de mesas o voto contabilizado.
- Barras de cinco candidatos.
- Participación.
- Voto en blanco.
- Cambios de posiciones.
- Clasificación al balotaje o victoria.

La animación revela el resultado; no lanza una nueva tirada.

---

# 25. Fraude electoral

## 25.1 Dos resultados internos

- **Voto real:** decisión de los electores.
- **Voto contabilizado:** resultado oficial después de irregularidades.

## 25.2 Factores

- Corrupción territorial.
- Control institucional.
- Estructura dominante de un rival.
- Debilidad territorial del jugador.
- Elección ajustada.
- Operaciones activas.
- Dificultad.
- Alertas ignoradas.

## 25.3 Límites

- No convierte una victoria amplia en derrota sin una cadena previa extraordinaria.
- Tiene más peso en márgenes estrechos.
- Deja señales, antecedentes o evidencia.
- Se reproduce con la semilla.
- Se explica en el resumen final.
- Puede beneficiar o perjudicar a distintos candidatos.

## 25.4 Anticipación

Puede detectarse mediante contactos, investigación, estructura territorial, periodistas, integrantes y decisiones durante el Día de Elección.

---

# 26. Balotaje

## 26.1 Inicio y persistencia

Si el jugador clasifica, el balotaje comienza al día siguiente. Se mantienen dinero, inversor, equipo, relaciones, escándalos, pruebas, promesas, estructura, confianza, rechazo y posible fraude.

Nada se reinicia.

## 26.2 Duración y actividades

- Aproximadamente dos semanas.
- Actos, entrevistas, negociaciones, tareas, operaciones, eventos y piquetes siguen disponibles.
- Los eliminados negocian apoyos.
- El debate final ocurre una semana antes.
- La Niebla comienza esa misma semana.

## 26.3 Redistribución

Cada elector eliminado considera:

- Afinidad con los dos finalistas.
- Confianza y rechazo.
- Apoyo de su antiguo candidato.
- Acuerdos conocidos.
- Voto útil.
- Abstención o voto en blanco.

El balotaje debe sentirse como una nueva disputa construida sobre la primera vuelta, no como una suma directa de porcentajes.

---

# 27. Dificultad

## 27.1 Niveles

- **Cagón**.
- **Tibio**.
- **Latam**.

## 27.2 Capas modificadas

| Variable | Cagón | Tibio | Latam |
|---|---|---|---|
| Dinero inicial | Alto | Medio | Bajo |
| Claridad de indicios | Alta | Parcial | Ambigua |
| Calidad del equipo ofrecido | Tendencia favorable | Mixta | Inestable |
| Operaciones rivales | Menos frecuentes | Moderadas | Frecuentes |
| Gravedad de errores | Menor | Normal | Alta |
| Recuperación | Amplia | Media | Reducida |
| Presión territorial rival | Baja | Media | Alta |
| Riesgo de fraude | Bajo | Moderado | Alto |

La capacidad económica base de los rivales no aumenta artificialmente en Latam. La dificultad mejora su presión, coherencia y aprovechamiento de oportunidades.

## 27.3 Justicia

- Las amenazas graves deben dejar señales.
- Las causas deben poder reconstruirse.
- Los rivales también pueden fallar.
- Una campaña excelente no se anula por una única tirada invisible.
- Un desastre puede ocurrir, pero debe surgir de estado, riesgo aceptado y semilla.

---

# 28. Interfaz y experiencia de usuario

## 28.1 Principios

- Información por capas: resumen primero, detalle al seleccionar.
- Una sola pantalla central de operaciones.
- Colores, iconos y texto; nunca color como único indicador.
- Toda cifra electoral debe mostrar fecha y calidad de fuente.
- Las consecuencias inmediatas no revelan necesariamente todos los cambios internos.
- Las alertas urgentes no deben perderse dentro del flujo de noticias.

## 28.2 HUD principal

Basado en el mockup `Captura de pantalla 2026-07-21 103557.png`:

- Navegación superior: Calendario, Mapa y Equipo.
- Área central contextual.
- Resumen de confianza nacional, numérico antes de la Niebla y cualitativo durante ella.
- Acceso contextual a prensa.
- Panel inferior permanente o plegable de Noticias.
- Fecha, franja horaria y fondos visibles en la cabecera definitiva.
- Botón **Siguiente día** con advertencias de tareas sin asignar o eventos urgentes.

## 28.3 Menú principal

- Nueva campaña.
- Continuar.
- Cargar campaña.
- Opciones.
- Créditos.
- Salir.

## 28.4 Mapa

- Vista de Roscalia.
- Acercamiento a jurisdicción.
- Selección de localidad.
- Marcadores de los cinco candidatos.
- Panel lateral de información, costo, datos y acciones.

## 28.5 Equipo

- Lista vertical de los ocho roles.
- Área separada para inversor y financiación.
- Tarjeta pública del integrante.
- Estado actual, tarea y relación.
- Botón Investigar.
- Historial de incidentes y promesas.

El mockup que separa Legal y Contable debe interpretarse como referencia antigua: el rol final es **Responsable legal/contable**.

## 28.6 Calendario

- Vista mensual.
- Eventos obligatorios, actividades y tareas.
- Detalle por día y franja.
- Aviso de Niebla, debate, veda y elección.
- Resumen de costos comprometidos.

## 28.7 Escenas de decisión

- Pregunta o declaración en zona legible.
- Tres botones de respuesta.
- Indicador de tiempo y advertencias verbales/visuales.
- Subtítulos para todas las intervenciones.
- Reacción facial, sonora y de público antes de volver al resumen sistémico.

## 28.8 Accesibilidad

- Escala de interfaz y tipografía.
- Alto contraste.
- Patrones o iconos para partidos.
- Remapeo de teclado y mando.
- Subtítulos y control de velocidad de texto.
- Temporizador normal, extendido, muy extendido o desactivado.
- Reducción de movimiento de menús y cámaras.
- Control independiente de música, ambiente, voces y efectos.
- Avisos de contenido para violencia, abuso, drogas y explotación.

---

# 29. Dirección artística

## 29.1 Identidad

- 2.5D estilizado.
- Sin pixel art.
- Interfaz y mapas ilustrados en 2D.
- Personajes y escenarios especiales en 3D estilizado.
- Colores vibrantes, alto contraste y sensación de vida.
- Humor visual expresivo sin convertir todo el mundo en caricatura infantil.

## 29.2 Personajes

- Proporciones humanas reconocibles con estilización moderada.
- Rostros expresivos y siluetas claras.
- Sistema modular de piel, pelo, barba, bigote, accesorios y ropa.
- Animaciones limitadas pero visibles: caminar, girar cabeza, mover brazos, gesticular y reaccionar.
- Los retratos se generan desde el personaje 3D o una interpretación ilustrada coherente del mismo diseño.

## 29.3 Escenarios 3D

- Estudio de entrevista.
- Acto político modular.
- Debate presidencial.
- Debate de balotaje mediante reutilización del escenario.

Se priorizan cámaras preparadas, iluminación clara, multitudes simplificadas y animaciones reutilizables.

## 29.4 Mapa y UI

- Mapa ilustrado con lectura geográfica clara.
- Paneles con profundidad ligera, sombras suaves y bordes redondeados.
- Transiciones de menús por desplazamiento, escala y despliegue.
- El teletipo de noticias aporta energía constante.
- La estética de los mockups negros es estructural, no cromática.

## 29.5 Paleta orientativa

La paleta definitiva se cierra en la biblia visual. Base recomendada para prototipos de diseño:

- Marfil cálido: `#F6F1E7`.
- Tinta azul: `#17233B`.
- Coral: `#FF5A5F`.
- Turquesa: `#18B6A4`.
- Amarillo: `#F2C94C`.
- Azul vivo: `#4DA3FF`.
- Violeta: `#7657D6`.

Los colores del partido del jugador son libres dentro de combinaciones legibles. Los partidos rivales utilizan color más icono para evitar confusiones.

## 29.6 Efectos visuales

- Destellos y flashes de prensa.
- Reacciones de público.
- Cambios de luces en debate.
- Animaciones de tendencia y escrutinio.
- Memes y placas televisivas.
- Alertas territoriales.

Los efectos deben reforzar información, no taparla.

---

# 30. Dirección de audio

## 30.1 Música

- Tema principal enérgico, satírico y reconocible.
- Música estratégica ligera para mapa y calendario.
- Capas de tensión para crisis, Niebla y Día de Elección.
- Stings televisivos distintos por medio.
- Música de acto con variantes por escala y partido.
- Debate con base mínima para priorizar diálogo y tensión.

La banda sonora evita imitar himnos o canciones partidarias reales.

## 30.2 Efectos y ambiente

- Multitudes, cánticos ficticios, aplausos y abucheos.
- Cámaras, flashes y estudio televisivo.
- Notificaciones, teletipo, calendario y dinero.
- Ambiente territorial por provincia.
- Urnas, centro de cómputos y reacción final.

## 30.3 Voz

El MVP no necesita doblaje completo. Puede usar:

- Sonidos breves de reacción.
- Frases cortas clave.
- Voz parcial del moderador o periodista si el presupuesto lo permite.
- Texto y subtítulos como fuente completa de información.

---

# 31. Guardado y carga

## 31.1 Estrategia de usuario

- Cinco espacios de campaña.
- Autoguardado rotativo al terminar cada día.
- Autoguardado antes de debate, elección y escenas críticas.
- Guardado manual desde pantallas estratégicas.
- No guardar en mitad de una respuesta temporizada.
- Si se cierra durante una escena, se reanuda desde el punto seguro anterior.

## 31.2 Persistencia necesaria

- Semilla.
- Calendario y ubicación.
- Electorado y memoria.
- Estado de rivales.
- Equipo, tareas y secretos descubiertos.
- Economía y compromisos.
- Noticias, relaciones, pruebas y operaciones.
- Brújula ideológica.
- Eventos y cadenas activas.
- Estado de Niebla, elección o balotaje.

El formato de guardado debe tener versión y permitir migraciones, pero esa implementación pertenece al documento técnico futuro.

---

# 32. Rejugabilidad

Cada campaña cambia por:

- Semilla reproducible.
- Perfil electoral y agenda.
- Población con variación controlada.
- Tres inversores ofrecidos.
- Equipo y rasgos ocultos.
- Nombres y apellidos.
- Personalidad rival.
- Relaciones iniciales.
- Eventos y crisis.
- Sesgo de medios y encuestas.
- Escándalos verdaderos o falsos.
- Estrategia del jugador.

La pantalla final puede mostrar la semilla y permitir iniciar otra campaña con la misma para comparar decisiones.

---

# 33. Final de partida y logros

## 33.1 Victoria

La única victoria principal es asumir la presidencia en primera vuelta o balotaje.

## 33.2 Derrota

- No clasificar al balotaje.
- Perder el balotaje.
- Morir durante la campaña.
- Otra incapacidad definitiva solo si surge de un evento explícito y legible.

## 33.3 Pantalla final

Muestra:

- Puesto y voto final.
- Participación y voto en blanco.
- Diferencia con el ganador o rival.
- Confianza final.
- Mapa territorial.
- Tiempo real jugado, excluyendo pausas prolongadas.
- Hitos de actos, debates, escándalos, relaciones, operaciones y fraude.
- Equipo con estadísticas y secretos revelados.
- Explicación resumida de los factores decisivos.
- Semilla.
- Botones Nueva campaña, Repetir semilla y Menú principal.

## 33.4 Logros iniciales

El catálogo se definirá como **CONTENIDO**, pero debe incluir un logro de derrota por muerte del protagonista. Los logros no otorgan ventajas sistémicas.

---

# 34. Volumen de contenido del MVP

Las cantidades son objetivos de producción y pueden ajustarse sin cambiar sistemas.

| Contenido | Mínimo jugable | Objetivo recomendado |
|---|---:|---:|
| Jurisdicciones | 6 | 6 |
| Localidades | 24 | 24 |
| Partidos/candidatos | 5 | 5 |
| Medios/periodistas | 5/5 | 5/5 |
| Arquetipos de inversor | 8 | 8 |
| Roles de equipo | 8 | 8 |
| Opciones generables por rol | 6 perfiles base | 8 perfiles base |
| Eventos interactivos | 20 | 30 |
| Variantes de eventos | 1 por evento | 2–3 por evento |
| Preguntas de entrevista | 40 | 60 |
| Preguntas temáticas de debate | 18 | 27 |
| Ataques o cruces contextuales | 20 | 40 |
| Plantillas de titulares | 80 | 120 |
| Plantillas de memes/redes | 30 | 50 |
| Propuestas o posturas | 3 por tema | 5 por tema |
| Escándalos configurables | 11 temas | 20 casos escritos |
| Logros | 8 | 15 |

Las respuestas deben construirse con etiquetas de tema, posición, tono, gravedad y contexto para reutilizar contenido sin que todas las escenas suenen iguales.

---

# 35. Priorización del MVP

## 35.1 Debe tener

- Loop semanal completo.
- Motor electoral explicable.
- 24 localidades ponderadas.
- Economía e inversor.
- Equipo oculto y tareas.
- Acto, entrevista y negociación.
- Eventos, medios, investigación y rivales.
- Niebla, primera vuelta, elección hora a hora y balotaje.
- Guardado.
- HUD funcional y escenas 3D mínimas.

## 35.2 Debería tener

- Piquetes organizables.
- Operaciones extremas.
- Fraude investigable.
- Memes y redes con buena variedad.
- Revelación final detallada.
- Accesibilidad completa de temporizadores.

## 35.3 Podría tener si el calendario lo permite

- Más escenarios de entrevista.
- Variantes de público y clima.
- Mayor personalización de ropa.
- Más animaciones contextuales.
- Semillas compartibles mediante código corto.

## 35.4 No entra ahora

- Multijugador.
- Gobierno posterior.
- Más de seis jurisdicciones.
- Elecciones adicionales.
- Doblaje integral.

---

# 36. Ruta de producción recomendada

Esta sección ordena el trabajo futuro; no autoriza implementación durante la etapa actual de GDD.

1. **Especificación de simulación:** formalizar coeficientes, estados y trazabilidad del motor electoral.
2. **Prototipo de papel/datos:** validar que confianza, intención, rechazo y participación produzcan decisiones interesantes.
3. **Vertical slice de diseño:** 14 días, una jurisdicción representativa, una Mesa, un acto, una entrevista, una crisis y un cierre electoral simulado.
4. **Núcleo del MVP:** 60 días, cinco candidatos, seis jurisdicciones, economía, equipo y elecciones.
5. **Contenido:** preguntas, eventos, titulares, escándalos, perfiles e inversores.
6. **Presentación 2.5D:** HUD definitivo y escenas 3D reutilizables.
7. **Balance y accesibilidad:** simulaciones, sesiones completas y ajustes.
8. **Cierre comercial:** estabilidad, textos, clasificación, página de tienda y QA.

El vertical slice no reemplaza al MVP; reduce temporalmente el volumen para probar el núcleo antes de producir todo el contenido.

---

# 37. Riesgos y mitigaciones

| Riesgo | Impacto | Mitigación de diseño |
|---|---|---|
| Alcance demasiado grande | Alto | Vertical slice, sistemas por datos, reutilización de escenarios y contenido etiquetado |
| Motor electoral opaco | Alto | Registro de causas, resumen final, indicadores de calidad y pruebas con semillas |
| Encuestas percibidas como azar falso | Alto | Fuente, sesgo y margen coherentes; patrones aprendibles |
| Explosión de diálogos | Alto | Plantillas contextuales, etiquetas y bancos por tema |
| Incoherencia entre 2D y 3D | Medio | Biblia visual, personaje modular y retratos derivados del mismo diseño |
| Una operación extrema domina la estrategia | Alto | Coste, acceso, exposición, represalia y beneficio incierto |
| Sátira confundida con acusación real | Alto | Mundo ficticio, diseños transformativos, nombres propios y revisión legal |
| Temas graves tratados con frivolidad | Alto | Consecuencias diferenciadas, avisos de contenido y humor dirigido al poder |
| Timers excluyen jugadores | Medio | Multiplicadores y desactivación accesible |
| 24 localidades abruman | Medio | Navegación en tres niveles y resúmenes cualitativos |
| Campaña de tres horas pierde ritmo | Medio | Dos o tres actividades semanales, noticias breves y escenas con duración limitada |
| Multijugador futuro condiciona el MVP | Medio | Documentar el avance por consenso, pero diseñar y validar primero un jugador |

---

# 38. Criterios de aceptación de diseño del MVP

El MVP cumple su objetivo cuando un jugador puede:

1. Crear partido y candidato sin elegir ideología inicial.
2. Elegir una localidad de origen en cualquier lugar de Roscalia.
3. Elegir dificultad, inversor y ocho integrantes con información parcial.
4. Comprender mapa, calendario, equipo, fondos y noticias.
5. Planificar una semana y delegar tareas.
6. Realizar actos, entrevistas y negociaciones con decisiones contextuales.
7. Ver que una misma postura afecta distinto a grupos y territorios.
8. Diferenciar confianza de intención de voto.
9. Investigar y obtener rumores, indicios o pruebas.
10. Sufrir o provocar noticias, escándalos, operaciones y crisis.
11. Observar rivales activos, coherentes y no omniscientes.
12. Participar en un debate con respuestas y cruces.
13. Entrar en Niebla Electoral y seguir tomando decisiones razonadas sin porcentajes.
14. Atravesar veda y Día de Elección hora a hora.
15. Ver un resultado determinado por la campaña, incluida participación y fraude limitado.
16. Ganar, perder o jugar un balotaje persistente.
17. Guardar, cargar y reanudar sin perder la semilla.
18. Entender en la pantalla final qué factores fueron decisivos.
19. Iniciar otra campaña sustancialmente distinta.

---

# 39. Decisiones cerradas y trabajo de balance

## 39.1 Cerrado

- País: Roscalia.
- Singleplayer primero.
- PC/Steam premium.
- MVP de 60 días; visión completa de seis meses.
- Cinco candidatos y cuatro rivales definidos.
- Seis jurisdicciones y 24 localidades.
- Brújula dinámica desde el centro.
- Grupos económicos con los nombres Pobres, Clase media, Pudientes y Chetos.
- Niebla de un mes y encuestas cegadas.
- Equipo de ocho roles, incluido Jefe de Operaciones.
- Estadísticas decisivas ocultas al seleccionar.
- Cinco medios y cinco periodistas.
- Arte 2.5D sin pixel art.
- Día de Elección hora a hora con decisiones operativas.
- Balotaje y fraude explicable.
- Apellido único por campaña.

## 39.2 Balance pendiente, no bloqueo conceptual

- Coeficientes del motor electoral.
- Rangos de variación poblacional.
- Magnitud de la bonificación de origen.
- Precios y aportes.
- Frecuencia y exposición de operaciones.
- Probabilidad excepcional de muerte del protagonista.
- Cantidad final de contenido por categoría.
- Duración exacta de escenas.
- Paleta y biblia visual finales.

---

# 40. Referencias del proyecto

## 40.1 Documento base

- `Referencias/Poliyo_MD_Primer_GDD.md` — borrador complementario; no se modifica desde este GDD.

## 40.2 Datos

- `Referencias/Archivos/nombres_y_apellidos_ficticios.xlsx` — catálogo de nombres y apellidos.

## 40.3 Mockups visuales

- `Referencias/Imagenes/Captura de pantalla 2026-07-21 103557.png` — HUD.
- `Referencias/Imagenes/Captura de pantalla 2026-07-21 103618.png` — menú principal.
- `Referencias/Imagenes/Captura de pantalla 2026-07-21 103630.png` — equipo.
- `Referencias/Imagenes/Captura de pantalla 2026-07-21 103638.png` — mapa.
- `Referencias/Imagenes/Captura de pantalla 2026-07-21 103651.png` — selección por tarjetas.
- `Referencias/Imagenes/Captura de pantalla 2026-07-21 103700.png` — calendario.

---

# 41. Glosario

- **Afiliado/Socio:** persona asociada al partido que aporta estructura, voluntariado o donaciones.
- **Brújula política:** posición dinámica del partido en los ejes izquierda/derecha y progresista/conservador.
- **Confianza:** resistencia del apoyo y percepción de capacidad/coherencia.
- **Encuesta Cegada:** estudio publicado durante la Niebla con error, sesgo o manipulación difíciles de evaluar.
- **Entusiasmo:** movilización que afecta militancia y participación.
- **Estructura territorial:** capacidad del partido para operar, informar y movilizar en una localidad.
- **Intención de voto:** preferencia electoral actual.
- **Microelector:** individuo o bloque pequeño con peso poblacional y perfil propio.
- **Niebla Electoral:** fase en la que desaparecen cifras exactas y la estrategia se apoya en señales.
- **Prueba:** evidencia fuerte y utilizable públicamente.
- **Rechazo:** resistencia a votar por un candidato y posible motivación de voto en contra.
- **Rumor:** información no confirmada y riesgosa.
- **Semilla:** valor reproducible que gobierna la variación de una campaña.
- **Voto contabilizado:** resultado oficial después de posibles irregularidades.
- **Voto real:** decisión simulada del electorado antes del fraude.

---

# 42. Resumen operativo

Poliyo debe entregar una campaña presidencial compacta, rejugable y tensa. El jugador crea un partido sin ideología prefijada, interpreta un país variable, elige personas sin conocer toda la verdad y actúa en un ecosistema donde territorio, medios, relaciones y confianza transforman cada declaración.

La experiencia culmina con un mes de Niebla Electoral, un Día de Elección hora a hora y un resultado reproducible que puede explicarse. El humor hace reconocible el mundo; los sistemas y sus consecuencias hacen que ganar la presidencia tenga valor.
