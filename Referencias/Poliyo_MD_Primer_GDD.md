# POLIYO — GDD consolidado del MVP

**Estado:** Documento de diseño consolidado  
**Versión:** 0.1  
**Fecha:** 21 de julio de 2026  
**Motor previsto:** Unity  
**Plataforma inicial:** PC / Steam  
**Modelo comercial previsto:** Juego premium  
**Idioma de diseño:** Español  
**Documento destinado a:** Agente de desarrollo y preproducción

---

## 0. Reglas de uso para el agente

Este documento reúne las decisiones de diseño ya tomadas para el MVP de **Poliyo**.

### Convenciones

- **CERRADO:** decisión canónica. No modificar sin aprobación explícita del responsable del proyecto.
- **PENDIENTE:** debe definirse antes o durante la implementación.
- **BALANCE:** el sistema está definido, pero sus valores exactos deben ajustarse durante las pruebas.
- **FUERA DEL MVP:** no implementar en esta etapa.

### Regla principal

No agregar sistemas nuevos solamente porque “serían interesantes”. Antes de ampliar el alcance se debe comprobar si la propuesta:

1. Es necesaria para que funcione el loop principal.
2. Reemplaza o simplifica un sistema existente.
3. Tiene una relación clara entre impacto y costo de producción.
4. Fue aprobada por el responsable del proyecto.

Este plan corresponde al **MVP real del juego**, no a una vertical slice reducida. Puede reducirse el volumen de localidades o contenido si la producción lo exige, pero no deben eliminarse los sistemas centrales ya definidos.

---

# 1. Visión del juego

## 1.1 Concepto

**Poliyo** es un videojuego de estrategia y simulación política en el que el jugador funda un partido, se convierte en su candidato presidencial y dirige una campaña electoral nacional.

El objetivo es ganar la presidencia mientras se administran:

- La intención de voto.
- La confianza del electorado.
- El dinero de campaña.
- El equipo partidario.
- Los inversores y sus intereses.
- Las relaciones con candidatos rivales.
- Las operaciones políticas.
- Los eventos sociales y nacionales.
- La información, los rumores y las investigaciones.
- Los actos, entrevistas, negociaciones y debates.

La elección no debe ganarse únicamente reuniendo porcentajes. El jugador debe sobrevivir a una campaña donde sus aliados, inversores, rivales y decisiones anteriores pueden fortalecerlo o perjudicarlo.

## 1.2 Fantasía principal

> Fundar un partido, construir una campaña nacional y llegar a la presidencia sin que las deudas, escándalos, operaciones, contradicciones y ambiciones internas destruyan el proyecto antes de las elecciones.

## 1.3 Identidad

El juego combina:

- Estrategia política.
- Simulación electoral.
- Gestión de campaña.
- Relaciones entre personajes.
- Eventos narrativos.
- Humor y sátira política.
- Consecuencias serias.

El mundo político debe sentirse injusto, agresivo e impredecible, pero el videojuego debe respetar reglas comprensibles.

> El mundo puede ser injusto; el sistema no debe ser arbitrario.

## 1.4 Tono

- Sátira política inspirada en la cultura política argentina.
- Humor absurdo, oscuro, exagerado y cínico.
- Titulares, memes, propaganda y situaciones ridículas.
- Las consecuencias sociales graves no deben reducirse siempre a un chiste.
- Los personajes son ficticios, aunque pueden contener alusiones reconocibles a prácticas o arquetipos reales.
- El humor debe dirigirse principalmente al poder, las instituciones, la burocracia y los incentivos políticos.

## 1.5 Alcance general del MVP

El MVP debe incluir:

- Una campaña nacional completa.
- Aproximadamente 60 días de campaña.
- Cinco candidatos en primera vuelta: el jugador y cuatro rivales.
- Posible balotaje.
- Tres dificultades.
- Economía de campaña.
- Equipo partidario.
- Inversor.
- Actividades del candidato.
- Tareas delegadas.
- Eventos y crisis.
- Investigación política.
- Actos, entrevistas y negociaciones.
- Debate presidencial en 3D.
- Niebla Electoral.
- Veda.
- Elección y escrutinio animado.
- Fraude electoral.
- Pantalla final de victoria o derrota.
- Guardado y carga de campaña.

## 1.6 Duración esperada

Una campaña completa debería durar aproximadamente entre **dos y tres horas reales**, sin obligación de jugarla en una sola sesión.

---

# 2. Alcance excluido del MVP

No implementar inicialmente:

- Gestión del país después de ganar.
- Elecciones legislativas, municipales o provinciales.
- PASO o internas electorales jugables.
- Multijugador.
- Campaña cooperativa.
- Políticos contemporáneos reales.
- Simulación individual de cada ciudadano.
- Contabilidad detallada.
- Impuestos, inflación de campaña o préstamos complejos.
- Consumo de energía del candidato.
- Viajes como acción independiente.
- Gestión de comida, electricidad, limpieza u otros gastos administrativos menores.
- Fraude mesa por mesa.
- Fiscales electorales como sistema técnico.
- Voces completas para todos los diálogos.
- Soporte de mods en el MVP.
- Fase jugable posterior a una derrota.
- Finales alternativos considerados victoria: solamente ganar la presidencia cuenta como victoria.

---

# 3. Mundo y contexto político

## 3.1 País

**Nombre oficial:** República Federal de Roscalia  
**Nombre habitual:** Roscalia  
**Capital:** Distrito Federal de Puerto Alba

Los nombres anteriores del país, como Brumaria, quedan reemplazados por **Roscalia**.

## 3.2 Gobierno saliente

Durante la campaña existe un gobierno saliente externo a los cinco candidatos.

Características:

- No presenta candidato propio.
- Su coalición quedó debilitada o destruida después de una mala gestión.
- Continúa gobernando hasta el traspaso.
- Tiene baja aprobación.
- Aparece en noticias y eventos.
- Puede contener o empeorar crisis.
- Puede ocultar información.
- Puede realizar anuncios desesperados.
- Puede sufrir renuncias o escándalos.
- Puede negociar con candidatos.
- Puede apoyar discretamente a un finalista.

La causa principal de su fracaso puede variar entre partidas:

- Crisis económica.
- Corrupción.
- Inseguridad.
- Conflicto social.
- Deterioro de servicios públicos.
- Aislamiento internacional.
- Combinación de varios problemas.

También debe conservar algún logro, apoyo o medida popular para evitar que culpar al gobierno sea siempre la respuesta óptima.

---

# 4. Mapa electoral

## 4.1 Estructura territorial

Roscalia tendrá seis jurisdicciones principales y hasta 24 zonas electorales.

La cantidad definitiva de localidades del MVP puede reducirse por razones de producción, pero debe conservarse:

- La diversidad económica.
- La diversidad social.
- La relevancia territorial.
- La diferencia de costos.
- El peso estratégico del mapa.

Los nombres visibles pueden modificarse sin cambiar sus identificadores internos.

## 4.2 Jurisdicciones

### Provincia de Gran Ribera

Principal provincia ganadera, cerealera y agroexportadora.

Características:

- Producción de soja y granos.
- Ganadería.
- Nivel económico medio.
- Grandes inversiones junto a desigualdad rural.
- Dependencia logística de rutas, trenes y el puerto de Puerto Alba.

Zonas provisionales:

- **San Laureano:** capital provincial; administración, clase media y oficinas agroexportadoras.
- **Las Espigas:** núcleo sojero y cerealero; productores, peones rurales y terratenientes.
- **Las Aguadas:** zona ganadera, tradicional y poco poblada.
- **Estación Ribera:** silos, ferrocarriles, camioneros y trabajadores logísticos.

### Provincia de Sierra Clara

Provincia septentrional con grandes reservas minerales y bajos ingresos.

Zonas provisionales:

- **Santa Elvira:** capital provincial; comercio, empleados públicos y universidades.
- **San Aurelio:** grandes proyectos mineros e inversores extranjeros.
- **Piedra Seca:** ciudad obrera dependiente de la minería.
- **Quebrada Honda:** comunidades aisladas, minería informal y pobreza estructural.

### Distrito Federal de Puerto Alba

Capital nacional y único gran puerto de ingreso de mercadería.

Características:

- Congreso y Gobierno Federal.
- Medios nacionales.
- Comercio exterior.
- Zonas financieras de altos ingresos.
- Barrios densamente poblados y pobres.
- Costos de campaña especialmente elevados.

Distritos provisionales:

- **Casco Federal:** Gobierno, Congreso, medios y funcionarios.
- **Altos del Alba:** zona financiera y residencial de clase alta.
- **Dársena Vieja:** puerto, depósitos, industria y trabajadores sindicalizados.
- **Bajo del Faro:** barrio densamente poblado y de bajos recursos.

### Provincia Austral de Ventisca

Provincia del sur, fría y turística.

Características:

- Turismo interno y externo.
- Paisajes nevados.
- Agricultura comercial limitada por el clima.
- Problemas de abastecimiento y conexión.

Zonas provisionales:

- **Nueva Aurora:** capital provincial, aeropuerto, administración y servicios.
- **Cerro Níveo:** turismo de lujo, hoteles y centros de esquí.
- **Lago Sereno:** turismo familiar, comercios y pequeños empresarios.
- **Paso Blanco:** población aislada, trabajo temporal y dificultades de abastecimiento.

### Provincia del Monte Rojo

Provincia montañosa, poco poblada y pobre, reconocida por el monte rojizo y el trekking.

Zonas provisionales:

- **Rojalba:** capital provincial y centro principal de servicios.
- **Villa Cardenal:** núcleo turístico y punto de partida del trekking.
- **Paso del Cóndor:** zona montañosa, dispersa y difícil de alcanzar.
- **Piedra Sola:** localidad remota y una de las más pobres de Roscalia.

### Provincia de Cumbre Dorada

Provincia metalúrgica y petrolera, árida y con pobreza estructural.

Zonas provisionales:

- **Villa Áurea:** capital provincial; Gobierno y oficinas de petroleras.
- **San Crisol:** núcleo metalúrgico, fabril y sindical.
- **Pozo Negro:** ciudad petrolera dependiente de pocas empresas.
- **Meseta Seca:** desempleo, abandono y dependencia estatal.

## 4.3 Regla territorial

Cada zona debe tener:

- Población o peso electoral.
- Composición social y económica.
- Temas prioritarios.
- Nivel de confianza por candidato.
- Intención de voto.
- Rechazo.
- Participación probable.
- Presencia territorial por partido.
- Costo de campaña.
- Eventos activos.
- Partido predominante estimado.
- Nivel de información disponible.

---

# 5. Estructura temporal de la campaña

## 5.1 Duración

La campaña principal durará aproximadamente **60 días**.

La elección general se realiza el **último día del mes**.

## 5.2 Planificación semanal

El jugador planifica sus decisiones principales por semana.

Cada semana puede programar **dos o tres actividades principales del candidato**.

No habrá sistema de energía.

Las restricciones serán:

- Calendario.
- Dinero.
- Disponibilidad.
- Distancia.
- Logística.
- Saturación mediática o territorial.
- Actividades incompatibles.
- Preparación previa.
- Compromisos anteriores.
- Integrantes ocupados.

## 5.3 Estructura de la semana

### Inicio de semana: Mesa de campaña

Escena obligatoria y gratuita.

Funciones:

- Revisar la semana anterior.
- Ver fondos y compromisos.
- Revisar información electoral.
- Revisar noticias y crisis.
- Asignar tareas al equipo.
- Resolver conflictos internos.
- Programar dos o tres actividades del candidato.
- Definir prioridades.

La Mesa de campaña no consume una de las actividades seleccionables.

### Desarrollo semanal

Las actividades se ejecutan durante los días programados.

Pueden aparecer:

- Eventos.
- Crisis.
- Noticias.
- Ataques rivales.
- Resultados de investigaciones.
- Pedidos de inversores.
- Problemas internos.

### Cierre semanal

Se muestra un resumen de:

- Resultados de las actividades.
- Variaciones conocidas de confianza.
- Variaciones conocidas de intención.
- Relaciones.
- Noticias.
- Investigación.
- Situación del equipo.
- Ingresos o gastos.
- Movimientos de rivales.

## 5.4 División del día

Cada día puede organizarse conceptualmente como:

- Mañana.
- Tarde.
- Noche.
- Noticias y consecuencias.

No es obligatorio que el jugador complete tres acciones diarias.

Algunas actividades ocupan una sola franja. Otras incluyen preparación o logística durante el resto de la jornada.

## 5.5 Avance

El jugador podrá avanzar al día siguiente aunque no tenga una actividad programada.

---

# 6. Inicio de una partida

Flujo previsto:

1. Menú principal.
2. Campaña nueva.
3. Selección de dificultad.
4. Creación del partido.
5. Creación del candidato.
6. Selección del inversor.
7. Selección inicial del equipo.
8. Inicio de la campaña.
9. HUD principal.

## 6.1 Creación del partido

Debe permitir definir al menos:

- Nombre.
- Sigla.
- Colores.
- Logo o símbolo.
- Identidad o posicionamiento.
- Base territorial inicial.
- Rasgos generales de campaña.

## 6.2 Creación del candidato

Dirección prevista:

- Personaje modular.
- Rostro.
- Tono de piel.
- Pelo.
- Barba o bigote.
- Anteojos y accesorios.
- Atuendo.
- Colores del partido.

Los retratos pueden generarse desde el propio modelo mediante una cámara específica para mantener coherencia visual.

---

# 7. Dificultad

Los niveles son:

- **Cagón**
- **Tibio**
- **Latam**

La dificultad debe modificar varias capas del juego. No debe limitarse a sumar o restar votos artificialmente.

## 7.1 Variables por dificultad

| Variable | Cagón | Tibio | Latam |
|---|---|---|---|
| Dinero inicial | Alto | Medio | Bajo |
| Calidad inicial del equipo | Alta | Mixta | Inestable |
| Fiabilidad de información | Alta | Parcial | Ambigua |
| Operaciones políticas rivales | Menos frecuentes | Moderadas | Frecuentes |
| Gravedad de errores | Menor | Normal | Alta |
| Margen de recuperación | Amplio | Medio | Reducido |
| Intensidad territorial rival | Baja | Media | Alta |
| Riesgo de fraude | Bajo | Moderado | Alto |
| Capacidad económica rival | Igual | Igual | Igual |

## 7.2 Dinero inicial

- **Cagón:** entre $420.000 y $500.000.
- **Tibio:** entre $350.000 y $420.000.
- **Latam:** entre $240.000 y $350.000.

Los rangos exactos pueden ajustarse durante el balance.

## 7.3 Principio de justicia

En Latam pueden ocurrir:

- Más operaciones.
- Información menos confiable.
- Consecuencias más graves.
- Rivales más agresivos.
- Mayor fraude.
- Menor margen de recuperación.

Sin embargo:

- Deben existir señales.
- Las causas deben poder reconstruirse.
- Los rivales también pueden sufrir consecuencias.
- Una gran campaña no debe ser anulada por una única tirada arbitraria.
- La dificultad aumenta presión y riesgo; no regala votos a los rivales.

---

# 8. Economía de campaña

## 8.1 Objetivo del sistema

El dinero sirve para:

- Limitar actividades.
- Obligar a priorizar.
- Generar relaciones de dependencia.
- Crear riesgos políticos.
- Dar valor territorial a las decisiones.

No debe convertirse en una simulación contable.

## 8.2 Ingresos

### Inversor

Al comenzar la partida el jugador verá tres tarjetas de inversores seleccionadas aleatoriamente de un catálogo mayor y deberá elegir una.

Información visible:

- Nombre.
- Retrato.
- Actividad pública.
- Reputación conocida.
- Aporte ofrecido.
- Frecuencia de aporte.
- Condiciones explícitas.
- Indicios narrativos.

Información oculta:

- Origen real del dinero.
- Legalidad.
- Afinidad con otros partidos.
- Posibles escándalos.
- Exigencias futuras.
- Objetivos personales.
- Probabilidad de traición.
- Información comprometedora.

El inversor puede:

- Realizar un aporte inicial.
- Aportar nuevamente al cierre mensual.
- Retener fondos.
- Pedir condiciones.
- Retirarse.
- Ser investigado.
- Convertirse en un escándalo.

### Donaciones de afiliados

Dependen de:

- Cantidad de afiliados.
- Compromiso.
- Confianza.
- Situación económica de sus sectores.
- Escándalos.
- Resultados de campaña.

Se recomienda agruparlas en el cierre semanal o mensual según el ritmo definitivo.

## 8.3 Arquetipos de inversor

1. Persona del ámbito privado con dinero limpio.
2. Persona del ámbito privado con un negocio turbio detrás.
3. Persona de actividad sospechosa con dinero no declarado.
4. Persona del ámbito público con dinero limpio.
5. Persona sin trabajo actual pero con una fortuna legítima, como un ganador de lotería.
6. Millonario con escándalos públicos.
7. Persona que en realidad favorece a otro partido.
8. Empresario de alto renombre que exige una contraprestación.

## 8.4 Gastos

### Gastos fijos

- Alquiler o mantenimiento de la sede.
- Estructura partidaria.
- Sueldos generales.
- Periodista sobornado.
- Persona pagada para guardar silencio.
- Servicios contratados.
- Compromisos permanentes.

### Gastos por actividad

- Actos.
- Transporte.
- Logística.
- Publicidad.
- Encuestas.
- Investigaciones.
- Operaciones territoriales.
- Eventos de recaudación.

### Gastos extraordinarios

- Abogados.
- Contención de crisis.
- Contraoperaciones.
- Reparaciones.
- Pagos comprometidos.
- Obtención de información.

## 8.5 Costos territoriales

El costo depende de:

- Localidad.
- Tamaño del evento.
- Estructura territorial.
- Transporte.
- Seguridad.
- Distancia.
- Importancia política.

Ejemplo de jerarquía:

- Capital y Congreso: muy alto.
- Gran ciudad: alto.
- Ciudad media: medio.
- Ciudad pequeña: bajo.
- Ciudad de origen del partido: reducido.
- Zona con fuerte estructura propia: reducido.

Los precios exactos son **BALANCE**, no una decisión de GDD.

## 8.6 Dinero en cero

No existe derrota automática ni deuda monetaria negativa.

Cuando un partido llega a cero:

- No puede pagar actividades.
- No puede realizar actos costosos.
- No puede financiar publicidad o investigaciones pagas.
- Puede continuar con acciones gratuitas.
- Puede recaudar.
- Puede declarar.
- Puede negociar.
- Puede recibir aportes posteriores.

Los gastos fijos impagos pueden generar consecuencias narrativas y operativas.

---

# 9. Equipo partidario

## 9.1 Selección inicial

Para cada puesto, el jugador podrá elegir entre **tres personas**.

Cada candidato a integrar el equipo tendrá:

- Nombre.
- Retrato.
- Trayectoria.
- Entre una y tres características visibles.
- Ventajas.
- Posibles desventajas.
- Contactos.
- Ideología.
- Afinidades.
- Información oculta.

La selección debe variar entre partidas.

La dificultad modifica la calidad y claridad de las opciones ofrecidas.

## 9.2 Roles

- Vicepresidente.
- Jefe de campaña.
- Vocero.
- Jefe de prensa.
- Consultor político.
- Coordinador territorial.
- Responsable legal/contable.
- Jefe de operaciones, si se conserva en el catálogo final de producción.
- Otros personajes que puedan incorporarse durante la campaña.
- Inversor, mostrado en el área de equipo o financiación, pero sin tratarlo como integrante asignable.

**Legal y Contable son un único rol: Responsable legal/contable.**

## 9.3 Comportamiento de integrantes

Pueden:

- Ayudar.
- Fracasar.
- Desobedecer.
- Ocultar información.
- Generar escándalos.
- Ser investigados.
- Filtrar datos.
- Enfrentarse al candidato.
- Construir poder propio.
- Ser detenidos.
- Renunciar.
- Ser expulsados.
- Morir por un evento.
- Tener relaciones con rivales.
- Trabajar en beneficio personal.

## 9.4 Revelación final

Al terminar la partida, se mostrarán sin ocultar:

- Ventajas reales.
- Desventajas reales.
- Lealtad.
- Intereses ocultos.
- Efectos positivos.
- Efectos negativos.
- Información que el jugador no había descubierto.

---

# 10. Tareas paralelas del equipo

Cada integrante puede recibir una tarea mientras el candidato realiza sus propias actividades.

Un integrante ocupado no puede ejecutar simultáneamente otra tarea.

El resultado depende de:

- Rol.
- Habilidad.
- Experiencia.
- Contactos.
- Lealtad.
- Relación con el candidato.
- Personalidad.
- Dificultad de la tarea.
- Relación con el territorio o interlocutor.

## 10.1 Contacto político

Reunirse con un dirigente u opositor.

Objetivos:

- Mejorar relaciones.
- Preparar una negociación.
- Obtener información.
- Transmitir una postura.
- Buscar apoyo.
- Reducir tensiones.

Riesgos:

- Filtración.
- Mala transmisión del mensaje.
- Rechazo.
- Compra o captación del integrante.
- Empeoramiento de relaciones.

## 10.2 Investigación

Investigar personas internas o externas:

- Integrantes.
- Inversores.
- Periodistas.
- Candidatos.
- Dirigentes.
- Empresarios.
- Funcionarios.

Puede obtener:

- Rumores.
- Indicios.
- Pruebas.
- Relaciones ocultas.
- Contradicciones.
- Financiamiento.
- Escándalos.
- Intereses.

## 10.3 Declaraciones en medios

Enviar un integrante a representar al partido.

Objetivos:

- Defender al candidato.
- Responder una polémica.
- Explicar propuestas.
- Atacar a un rival.
- Mantener presencia mediática.

Riesgos:

- Contradicciones.
- Frases polémicas.
- Protagonismo excesivo.
- Internas.
- Daño reputacional.

## 10.4 Análisis de crisis

Enviar a un integrante a analizar un evento.

Puede informar:

- Qué ocurrió.
- Quién está involucrado.
- Qué sectores están afectados.
- Qué postura conviene.
- Qué riesgos existen.
- Qué pueden hacer los rivales.

Esperar mejora la información, pero puede hacer perder tiempo político.

## 10.5 Recaudación

Buscar:

- Donaciones.
- Nuevos aportes.
- Empresarios.
- Eventos privados.
- Socios.
- Potenciales inversores.

Puede incorporar obligaciones o dinero problemático.

## 10.6 Campaña territorial

Enviar a un integrante a una zona.

Efectos posibles:

- Aumentar apoyo local.
- Mejorar estructura.
- Preparar un acto.
- Reducir costos futuros.
- Obtener información local.
- Defender un territorio.

El impacto debe ser menor que la presencia personal del candidato.

## 10.7 Captación de afiliados

Buscar nuevos afiliados.

Efectos:

- Más donaciones.
- Más voluntarios.
- Mayor estructura territorial.
- Nuevos dirigentes.
- Riesgo de infiltración.
- Riesgo de conflictos internos.

---

# 11. Actividades principales del candidato

El catálogo principal del MVP se reduce a tres actividades seleccionables, además de la Mesa de campaña obligatoria.

## 11.1 Acto político

### Costo

Variable por:

- Localidad.
- Escala.
- Transporte.
- Logística.
- Seguridad.
- Estructura partidaria.

### Tiempo

- Ocupa una parte del día.
- La logística puede ocupar el resto.
- Duración aproximada de la escena: cinco minutos reales.

### Interacción

El candidato realiza tres intervenciones o frases relevantes:

1. Apertura o conexión local.
2. Idea, propuesta o posicionamiento.
3. Cierre político.

### Objetivos

- Dar a conocer ideas.
- Presentar propuestas.
- Mejorar intención de voto.
- Mejorar apoyo local.
- Disputar una zona.
- Fortalecer estructura y visibilidad.

### Riesgos

- Baja convocatoria.
- Incidentes.
- Declaraciones polémicas.
- Mala organización.
- Mala imagen.
- Conflicto con rivales.
- Saturación territorial.
- Transporte de asistentes descubierto o cuestionado.

## 11.2 Entrevista periodística

### Costo

Generalmente gratuita.

Puede ocurrir porque:

- El medio invita al candidato.
- El candidato solicita participar.
- El candidato busca defender una postura.

Un medio puede negar, posponer o condicionar la entrevista.

### Tiempo

- Ocupa una parte del día.
- Entre tres y cinco preguntas.
- Duración aproximada: siete minutos reales.

### Objetivos

- Aclarar hechos.
- Defenderse.
- Presentar propuestas.
- Obtener visibilidad nacional.
- Responder ataques.
- Mejorar reputación.

### Riesgos

- Preguntas inesperadas.
- Periodista hostil.
- Contradicciones.
- Respuestas vagas.
- Frases virales negativas.
- Negarse a responder.
- Quedar expuesto por una prueba.

## 11.3 Negociación política

### Costo

- Gratuita si el otro actor busca al jugador.
- Puede exigir dinero, concesiones o compromisos si el jugador inicia el contacto.

El costo puede ser político y no solamente económico.

### Tiempo

- Actividad agendada.
- Duración aproximada: cinco minutos reales.

### Objetivos

- Mejorar relaciones.
- Reducir ataques.
- Conseguir apoyo.
- Intercambiar información.
- Construir acuerdos.
- Preparar el balotaje.

### Riesgos

- Rechazo.
- Filtración.
- Parecer débil.
- Empeorar la relación.
- Entregar demasiado.
- Prometer algo imposible.
- Ser traicionado.

## 11.4 Mesa de campaña

Actividad semanal obligatoria, no seleccionable.

Funciones:

- Informe.
- Conflicto o decisión interna.
- Planificación.
- Asignación de tareas.
- Revisión del liderazgo.

Duración aproximada: cinco minutos.

---

# 12. Piquetes como acción y evento

El piquete pertenece a dos categorías:

- Puede aparecer como evento espontáneo o rival.
- Puede ser organizado por el jugador.

## 12.1 Alcance

Puede afectar:

- Calle importante.
- Avenida.
- Acceso urbano.
- Ruta.
- Zona productiva.
- Localidad.
- Provincia mediante un punto estratégico.

## 12.2 Consecuencias

- Tránsito.
- Comercio.
- Trabajo.
- Abastecimiento.
- Producción.
- Confianza.
- Intención de voto.
- Rechazo.
- Cobertura mediática.
- Relaciones entre partidos.

## 12.3 Banderas políticas

La presencia de una bandera o símbolo partidario puede relacionar el conflicto con un partido, aunque no sea el organizador real.

Esto puede beneficiar al partido entre quienes apoyan el reclamo y perjudicarlo entre quienes sufren el corte.

## 12.4 Organización por el jugador

El jugador selecciona:

- Provincia.
- Localidad o punto territorial.
- Escala.
- Responsable.
- Nivel de presencia partidaria.
- Causa pública.
- Recursos logísticos.

Puede:

- Organizarlo personalmente.
- Delegarlo a un integrante.

### Personalmente

- Consume tiempo del candidato.
- Da mayor control.
- Aumenta el riesgo de exposición directa.

### Delegado

- Conserva el tiempo del candidato.
- Ocupa a un integrante.
- Reduce el control.
- Puede dejar pruebas o cometer errores.

## 12.5 Costos

- Transporte.
- Comida.
- Materiales.
- Carteles.
- Difusión.
- Coordinadores.
- Sonido.
- Pagos a organizaciones.

## 12.6 Escala

- Pequeño.
- Mediano.
- Grande.

Los valores exactos son de balance.

## 12.7 Investigación

La información inicial suele ser completa respecto a:

- Lugar.
- Reclamo público.
- Sectores involucrados.
- Vías afectadas.
- Símbolos visibles.
- Impacto social.

La investigación busca descubrir:

- Organizador real.
- Financiación.
- Relación partidaria.
- Infiltrados.
- Provocadores.
- Intención política.

Resultados posibles:

- Sin resultados.
- Rumor.
- Indicio.
- Prueba.

---

# 13. Eventos y crisis

## 13.1 Catálogo previsto

- Desastre natural: tsunami, terremoto u otro.
- Guerra en un país vecino.
- Ola de inmigrantes.
- Piquete.
- Huelga.
- Pobreza extrema.
- Escándalo de un integrante.
- Escándalo de un rival.
- Crisis sanitaria: enfermedad o virus.
- Ataque de un rival.
- Pedido comprometedor de un inversor.
- Muerte de un rival o integrante.
- Detención de un rival o integrante.
- Saqueos.
- Ola de inseguridad.

La pobreza extrema funciona mejor como condición persistente que puede generar otros eventos.

## 13.2 Frecuencia

- Noticias menores: pueden aparecer diariamente.
- Evento interactivo relevante: aproximadamente uno por semana.
- Crisis nacional grande: aproximadamente una por mes.
- Eventos extraordinarios: activados por decisiones, investigaciones o cadenas previas.

No debe existir una crisis obligatoria todos los días.

## 13.3 Respuestas generales del jugador

Según el evento, podrá:

1. Intervenir personalmente.
2. Delegar.
3. Investigar.
4. Declarar.
5. Negociar.
6. Ignorar.

No existe una opción genérica de ayuda económica.

No todas las respuestas estarán disponibles en todos los eventos.

## 13.4 Plantilla conceptual

Cada evento debe definir:

- Identificador.
- Origen.
- Territorio.
- Gravedad.
- Duración.
- Información pública.
- Información oculta.
- Sectores afectados.
- Respuestas válidas.
- Costos.
- Consecuencias.
- Posibles escaladas.
- Condiciones de resolución.
- Memoria electoral.
- Noticias derivadas.

---

# 14. Rivales

## 14.1 Cantidad

Primera vuelta:

- Jugador.
- Cuatro candidatos rivales.

Total: cinco candidatos.

## 14.2 Relaciones

Los rivales tienen relaciones dinámicas con:

- El jugador.
- Los demás rivales.
- Periodistas.
- Empresarios.
- Gobierno saliente.
- Equipo propio.
- Integrantes del jugador.
- Sectores territoriales.

Las relaciones influyen en:

- Ataques.
- Negociaciones.
- Apoyos.
- Balotaje.
- Filtraciones.
- Operaciones.
- Respuestas a eventos.

## 14.3 Respuesta ante eventos

El comportamiento rival ante eventos será aleatorio.

Flujo:

1. Evaluar si participa.
2. Filtrar respuestas válidas.
3. Elegir una respuesta mediante random.
4. Aplicar consecuencias.

Se recomienda un random condicionado:

- Confrontativo: mayor probabilidad de atacar o declarar.
- Moderado: mayor probabilidad de negociar.
- Calculador: mayor probabilidad de investigar.
- Pasivo: mayor probabilidad de ignorar.
- Autoritario: mayor probabilidad de intervenir.

La selección sigue siendo aleatoria, pero debe mantener una coherencia mínima.

## 14.4 Regla

Los rivales solo pueden utilizar durante el debate información que realmente hayan obtenido.

No deben inventar pruebas por necesidad narrativa.

## 14.5 Pendiente técnico

La elección normal de actos, territorios y agenda semanal de los rivales requiere una implementación simple. No está definida todavía una IA estratégica completa.

Debe priorizarse:

- Bajo costo de implementación.
- Coherencia visible.
- Variación.
- Ausencia de omnisciencia.
- Uso de las mismas restricciones generales del juego.

---

# 15. Electorado y simulación electoral

## 15.1 Principio

El electorado combina:

- Individuos simulados o microelectores.
- Grupos sociales.
- Grupos económicos.
- Regiones.
- Temas políticos.
- Relaciones con partidos.

La composición y afinidades iniciales varían por semilla.

No habrá un sector que siempre pertenezca al mismo partido en todas las partidas.

## 15.2 Valores principales

### Intención de voto

Probabilidad o preferencia actual de votar a un candidato.

Puede existir por:

- Ideología.
- Propuesta.
- Rechazo a otro candidato.
- Voto útil.
- Afinidad territorial.
- Confianza.
- Evento reciente.

### Confianza

Representa cuánto cree el electorado que el candidato es capaz, coherente o confiable.

La confianza funciona como resistencia:

- Un votante con intención alta pero confianza baja puede abandonar.
- Un votante con confianza alta puede tolerar decisiones que no comparte completamente.
- La confianza puede amortiguar escándalos o errores.

### Rechazo

Dificulta convertir votantes y puede movilizar voto en contra.

### Entusiasmo o movilización

Afecta la probabilidad de participación y militancia.

### Participación

No todos los ciudadanos votan.

Una persona con baja intención puede igualmente:

- Asistir a votar.
- Elegir a último momento.
- Votar en blanco.
- Votar por el mal menor.

## 15.3 Determinismo y semilla

La campaña será completamente determinista a partir de:

- Semilla.
- Estado inicial.
- Decisiones del jugador.
- Decisiones rivales.
- Eventos.
- Relaciones.
- Información.
- Fraude.

No habrá una tirada final que cambie arbitrariamente al ganador.

Puede utilizarse aleatoriedad durante la simulación, pero debe provenir de una semilla reproducible y formar parte del desarrollo de la campaña.

## 15.4 Voto en blanco

Existe voto en blanco.

Las condiciones presidenciales se calculan sobre votos afirmativos válidos.

## 15.5 Temas políticos

Catálogo:

1. Economía y trabajo.
2. Salud y protección social.
3. Educación, ciencia y cultura.
4. Seguridad y justicia.
5. Instituciones y corrupción.
6. Tecnología y futuro productivo.
7. Relaciones exteriores y defensa.
8. Federalismo, infraestructura y vivienda.
9. Ambiente y energía.

En cada partida:

- Algunos temas son dominantes.
- Otros son secundarios.
- Otros permanecen latentes.
- Los eventos pueden cambiar la agenda.

## 15.6 Cambios electorales

Las actividades y declaraciones pueden:

- Producir cambios graduales.
- Provocar cambios fuertes.
- Mejorar a un sector y perjudicar a otro.
- Aumentar intención pero reducir confianza.
- Aumentar confianza sin convertir votos inmediatos.
- Generar saturación.
- Reducir rechazo.
- Movilizar votantes ya favorables.

## 15.7 Rendimiento territorial

Una zona favorable puede aportar pocos votos nuevos pero mejorar:

- Entusiasmo.
- Participación.
- Afiliados.
- Recaudación.
- Estructura.

Una zona competitiva puede aportar más conversiones.

Una zona adversa puede permitir reducir rechazo o ganar visibilidad.

Las acciones repetidas tienen rendimientos decrecientes.

## 15.8 Fórmula electoral

**PENDIENTE DE ESPECIFICACIÓN TÉCNICA.**

Antes de programar el núcleo debe definirse cómo se combinan:

- Afinidad temática.
- Intención.
- Confianza.
- Rechazo.
- Entusiasmo.
- Participación.
- Influencia territorial.
- Eventos.
- Noticias.
- Voto útil.
- Voto en blanco.
- Fraude.
- Redistribución en balotaje.

La fórmula debe ser:

- Reproducible.
- Depurable.
- Explicable.
- Testeable.
- Balanceable.
- Sin cambios mágicos de porcentajes.

---

# 16. Información electoral y Niebla Electoral

## 16.1 Antes de la Niebla

El jugador puede acceder a:

- Encuestas.
- Rangos.
- Tendencias.
- Margen de error.
- Información territorial.
- Calidad de la consultora.
- Antigüedad.
- Posible sesgo.

Las encuestas no muestran necesariamente la verdad exacta.

Modelo conceptual:

`Encuesta publicada = estimación real + error + sesgo + posible manipulación`

## 16.2 Campaña principal

La Niebla Electoral comienza aproximadamente **un mes antes de la elección**.

Durante esta fase:

- Se oculta la intención exacta.
- Los datos anteriores envejecen.
- Se muestran estados cualitativos.
- Puede haber encuestas contradictorias.
- El jugador recibe señales indirectas.
- Puede investigar.
- Puede obtener rumores, indicios o pruebas.

Estados cualitativos posibles:

- Favorable.
- Competitivo.
- Adverso.
- Incierto.

## 16.3 Balotaje

La Niebla Electoral comienza **una semana antes** de la elección de segunda vuelta.

---

# 17. Noticias y medios

Las noticias forman parte del gameplay.

Flujo:

`Hecho → interpretación → publicación → amplificación → respuesta → consecuencia → memoria`

Los medios pueden diferenciarse por:

- Alcance.
- Rigor.
- Velocidad.
- Sensacionalismo.
- Intereses.
- Audiencia territorial.
- Relación con el jugador.

Las noticias pueden:

- Informar.
- Sesgar.
- Manipular.
- Instalar agenda.
- Convertir frases en memes.
- Amplificar escándalos.
- Ocultar temas.
- Contradecirse entre sí.

El tono puede ser cínico, exagerado y satírico.

---

# 18. Debate presidencial

## 18.1 Presentación

- Escena 3D.
- Recinto cerrado.
- Escenario.
- Atriles o posiciones.
- Moderador.
- Rivales.
- Público.
- Cámaras.
- Movimiento limitado del jugador.

Duración aproximada: **diez minutos reales**.

## 18.2 Fase temática

- Una pregunta por tema puntual incluido en el debate.
- Máximo de tres respuestas por pregunta.
- Las respuestas dependen de:
  - Ideología.
  - Propuestas.
  - Declaraciones anteriores.
  - Promesas.
  - Investigaciones.
  - Relaciones.
  - Escándalos.

No existe una respuesta universalmente correcta.

## 18.3 Debate abierto

Después de las preguntas formales comienza una fase abierta.

El jugador y los rivales pueden:

- Atacar.
- Defenderse.
- Exponer contradicciones.
- Mostrar rumores.
- Mostrar indicios.
- Mostrar pruebas.
- Revelar financiación.
- Utilizar investigaciones.
- Recordar declaraciones.
- Contraatacar.

## 18.4 Expediente político

Cada hallazgo debe conservar:

- Persona o partido.
- Tema.
- Fuente.
- Nivel de certeza.
- Antigüedad.
- Estado de publicación.
- Uso previo.

Niveles:

- Rumor.
- Indicio.
- Prueba.

## 18.5 Ataques al jugador

El jugador puede:

- Negar.
- Defenderse.
- Justificar.
- Admitir parcialmente.
- Responsabilizar a un integrante.
- Contraatacar.
- Redirigir hacia una propuesta.

Las opciones dependen del contexto y de la calidad de la evidencia.

## 18.6 Tiempo

Las respuestas tienen tiempo limitado.

No responder puede generar:

- Pérdida del turno.
- Mala imagen.
- Titulares.
- Memes.

## 18.7 Preparación

No existe una actividad separada de “preparar debate”.

La preparación surge de:

- La campaña realizada.
- Propuestas.
- Conocimiento.
- Coherencia.
- Investigaciones.
- Equipo.
- Experiencia acumulada.

---

# 19. Veda electoral

La campaña termina el primer día de la veda.

Se muestra una pantalla anunciando:

> Comenzó la veda electoral.

Durante la veda:

- No hay actividades.
- No hay decisiones.
- No hay eventos jugables.
- No hay operaciones.
- No hay cambios artificiales.
- Se avanza hacia la elección.

La veda es una transición narrativa, no una fase jugable.

---

# 20. Elección y victoria

## 20.1 Condiciones

Un candidato gana en primera vuelta si:

- Supera el 45 % de los votos afirmativos; o
- Obtiene al menos 40 % y una diferencia de diez puntos sobre el segundo.

En caso contrario, los dos primeros pasan al balotaje.

## 20.2 Resultado

El resultado está determinado por todo lo ocurrido durante la campaña.

No se recalcula aleatoriamente al comenzar el escrutinio.

## 20.3 Presentación

El escrutinio es una animación:

- Barras de candidatos.
- Carga progresiva de votos.
- Votos en blanco.
- Participación.
- Resultado final.
- Animación de ganador o clasificación.

No será una pantalla técnica.

No habrá decisiones durante el escrutinio.

## 20.4 Derrota

Si el jugador no gana y tampoco clasifica al balotaje, la partida termina inmediatamente con derrota.

La única victoria es obtener la presidencia.

---

# 21. Fraude electoral

## 21.1 Principio

Internamente pueden existir:

- Voto real.
- Voto contabilizado.

El fraude puede modificar parcialmente el resultado oficial.

## 21.2 Factores

- Corrupción territorial.
- Control institucional de un rival.
- Presencia territorial dominante.
- Falta de estructura del jugador.
- Diferencia electoral ajustada.
- Operaciones activas.
- Dificultad.
- Señales ignoradas.

## 21.3 Límites

- No debe convertir una victoria amplia en una derrota sin fundamento.
- Debe ser más importante en elecciones ajustadas.
- Deben existir señales o antecedentes.
- Debe poder aparecer en el resumen final.
- Debe ser reproducible por semilla.
- No debe ser una tirada final independiente.

## 21.4 Investigación

Puede anticiparse mediante:

- Investigación territorial.
- Contactos políticos.
- Campaña territorial.
- Periodistas.
- Investigación de rivales.
- Información de integrantes.

---

# 22. Balotaje

## 22.1 Inicio

La elección general se realiza el último día del mes.

Si el jugador clasifica, el balotaje comienza **al día siguiente**.

Al comenzar el nuevo mes se procesa el cierre económico del mes anterior:

- Aportes.
- Donaciones.
- Gastos fijos.
- Compromisos.
- Pagos incumplidos.

## 22.2 Duración

Aproximadamente dos semanas.

## 22.3 Persistencia

Se mantiene todo:

- Dinero.
- Inversor.
- Equipo.
- Relaciones.
- Escándalos.
- Investigaciones.
- Rumores.
- Indicios.
- Pruebas.
- Promesas.
- Apoyo territorial.
- Rechazo.
- Confianza.
- Fraude posible.

Nada se reinicia.

## 22.4 Actividades

Se mantienen:

- Actos.
- Entrevistas.
- Negociaciones.
- Tareas delegadas.
- Investigaciones.
- Eventos.
- Operaciones.
- Piquetes.

## 22.5 Apoyo de eliminados

Depende de:

- Relación con los finalistas.
- Acuerdos.
- Ataques.
- Afinidad.
- Conveniencia.
- Intención de voto de sus antiguos votantes.

El apoyo de un candidato eliminado influye, pero no transfiere automáticamente todos sus votos.

## 22.6 Debate final

- Una semana antes de la elección.
- Escena 3D.
- Dos candidatos.
- Preguntas temáticas.
- Debate abierto.
- Uso de investigaciones acumuladas.

## 22.7 Niebla

Comienza una semana antes de la elección.

## 22.8 Resultado

- Si el jugador gana: victoria.
- Si pierde: derrota.
- La partida termina.

---

# 23. Pantalla final

Al ganar o perder se muestra:

## 23.1 Resultado

- Lugar final.
- Porcentaje de votos final.
- Diferencia frente al ganador o rival.
- Victoria en primera vuelta o balotaje.
- Derrota.

En esta pantalla debe hablarse de **voto final**, no de intención estimada.

## 23.2 Confianza

- Nivel final de confianza.
- Descripción cualitativa opcional.

## 23.3 Equipo

Mostrar:

- Integrantes.
- Ventajas.
- Desventajas.
- Efectos.
- Lealtad.
- Secretos.
- Información oculta revelada.

## 23.4 Tiempo

- Tiempo real jugado.
- Excluir tiempo excesivo en pausa o menú.

## 23.5 Resumen de campaña

Lista breve de hitos:

- Actos.
- Territorios decisivos.
- Escándalos.
- Investigaciones.
- Debates.
- Piquetes.
- Relaciones.
- Fraude.
- Apoyos.

## 23.6 Botones

- Nueva partida.
- Volver al menú principal.

---

# 24. Interfaz y navegación

Los mockups actuales definen seis pantallas principales.

## 24.1 HUD de partida

Elementos:

- Botón Calendario.
- Botón Mapa.
- Botón Equipo.
- Área central de contenido.
- Confianza.
- Acción o acceso de prensa.
- Panel inferior de Noticias.

Debe funcionar como centro de operaciones.

## 24.2 Menú principal

- Logo.
- Campaña nueva.
- Cargar campaña.
- Opciones.
- Salir.

## 24.3 Menú de equipo

Lista vertical de roles:

- Vice.
- Jefe de campaña.
- Vocero.
- Jefe de prensa.
- Consultor.
- Coordinador territorial.
- Responsable legal/contable.
- Inversor o sección de financiación.

Al seleccionar un rol debe mostrarse su tarjeta e información.

## 24.4 Mapa

- Mapa de Roscalia.
- Selección de provincia.
- Selección de localidad.
- Panel lateral con información y acciones.

## 24.5 Selección de equipo

- Tres tarjetas por puesto.
- Nombre.
- Retrato.
- Información visible.
- Selección rápida.
- Progreso al siguiente rol.

## 24.6 Calendario

- Vista mensual o de campaña.
- Actividades visibles por día.
- Selección de un día para ver detalle.
- Actividades como acto, entrevista y reunión de equipo.

## 24.7 Definiciones próximas de UX/UI

Estas decisiones pueden cerrarse después y no deben bloquear el documento actual:

- Ubicación exacta de fecha y fondos en la HUD.
- Definir si “Hablar con la prensa” es contextual o un acceso al área de prensa.
- Presentación exacta de mañana, tarde y noche.
- Diferenciación visual del inversor respecto del equipo.
- Contenido exacto del panel lateral del mapa.
- Comportamiento de Noticias: fijo, plegable o contextual.
- Información electoral visible permanentemente.
- Estados visuales de alertas.
- Navegación con teclado y mando.
- Accesibilidad y tamaño de tipografía.

---

# 25. Dirección audiovisual

## 25.1 Dirección general

- Experiencia 2.5D.
- Interfaz estratégica 2D.
- Mapa estratégico.
- Escenas especiales 3D estilizadas.
- Colores vivos y diferenciados por partido.
- Rostros expresivos y ligeramente caricaturizados.
- Materiales estilizados, no fotorrealistas.
- Iluminación clara.
- Movimiento limitado en escenarios especiales.

## 25.2 Escenas 3D

Como mínimo:

- Acto político.
- Entrevista.
- Debate presidencial.
- Debate de balotaje, reutilizando el escenario cuando sea posible.

## 25.3 Reutilización

- Esqueleto de animación compartido.
- Animaciones reutilizables.
- Escenarios modulares.
- Cámaras predeterminadas.
- Multitudes simplificadas.
- Variantes visuales por partido.

## 25.4 Pendiente visual

Debe cerrarse una biblia artística que defina:

- Pixel art o ilustración 2D definitiva.
- Escala y resolución.
- Estilo del mapa.
- Estilo de UI.
- Paleta.
- Modelado.
- Materiales.
- Cámaras.
- Animaciones mínimas.
- Retratos.
- VFX.
- Sonido.
- Música.

---

# 26. Guardado y carga

**PENDIENTE.**

Debe definirse:

- Guardado automático o manual.
- Momento del autoguardado.
- Cantidad de partidas.
- Qué ocurre al cerrar durante una escena.
- Persistencia de semilla.
- Versión del archivo de guardado.
- Migraciones.
- Prevención de corrupción.
- Reanudación de actividades.

Recomendación técnica no vinculante:

- Autoguardado al finalizar cada día.
- Guardado antes de escenas importantes.
- Semilla persistida.
- Estado completo serializable.
- No guardar a mitad de una decisión de diálogo salvo que el sistema lo soporte de forma segura.

---

# 27. Contenido mínimo del MVP

## 27.1 CERRADO

- Cinco candidatos.
- Tres dificultades.
- Seis provincias.
- Hasta 24 localidades.
- Ocho arquetipos de inversor.
- Siete tareas delegadas.
- Tres actividades principales.
- Mesa de campaña.
- Quince tipos de eventos listados.
- Debate general.
- Debate de balotaje.
- Niebla Electoral.
- Fraude.
- Pantalla final.

## 27.2 PENDIENTE DE VOLUMEN

Definir cantidades concretas de:

- Personajes candidatos por cada rol.
- Periodistas.
- Medios.
- Preguntas de entrevista.
- Preguntas de debate.
- Respuestas de diálogo.
- Eventos escritos.
- Variantes de cada evento.
- Titulares.
- Memes.
- Propuestas políticas.
- Inversores concretos.
- Escándalos.
- Investigaciones.
- Animaciones.
- Localidades finales si se reduce el mapa.

---

# 28. Arquitectura técnica pendiente

Antes de comenzar la implementación completa debe diseñarse:

- Estado global de campaña.
- Sistema de calendario.
- Motor electoral.
- Datos territoriales.
- Personajes.
- Relaciones.
- Economía.
- Actividades.
- Tareas.
- Eventos.
- Investigaciones.
- Noticias.
- Debates.
- Rivales.
- Fraude.
- Guardado.
- Semillas reproducibles.
- Herramientas de balance.
- Herramientas para crear contenido.

## 28.1 Criterios técnicos

El sistema debe permitir:

- Cambiar nombres visibles sin romper datos.
- Crear eventos sin modificar código central.
- Balancear valores desde datos.
- Reproducir una campaña mediante semilla.
- Ejecutar simulaciones automáticas.
- Inspeccionar por qué cambió una elección.
- Registrar decisiones.
- Evitar dependencias directas innecesarias entre UI y simulación.

## 28.2 Dirección recomendada para Unity

No es todavía una arquitectura aprobada, pero el agente debería evaluar:

- ScriptableObjects para definiciones estáticas.
- Clases serializables para estado mutable.
- Servicios o controladores separados por sistema.
- Bus de eventos o señales para comunicación desacoplada.
- Máquinas de estado para campaña, escenas y elecciones.
- Sistema de IDs estables.
- Separación de definición, estado y presentación.
- Pruebas unitarias del motor electoral.
- Simulaciones masivas sin cargar escenas.
- Guardado versionado.

No sobreingenierizar antes de tener el modelo electoral especificado.

---

# 29. Pendientes prioritarios antes de producción

## Prioridad 0 — Núcleo matemático

1. Fórmula electoral.
2. Confianza, intención, rechazo, entusiasmo y participación.
3. Distribución inicial por semilla.
4. Voto en blanco.
5. Voto útil.
6. Transferencia de preferencias en balotaje.
7. Fraude.
8. Encuestas y error.
9. Rendimientos decrecientes.
10. Depuración y explicación de resultados.

## Prioridad 1 — Alcance de contenido

1. Confirmar 24 localidades o reducir.
2. Cantidad de personajes por rol.
3. Cantidad de eventos.
4. Cantidad de medios y periodistas.
5. Temas incluidos en cada debate.
6. Cantidad de preguntas y respuestas.
7. Cantidad de inversores concretos.

## Prioridad 2 — Guardado

1. Estrategia.
2. Estructura de archivo.
3. Versionado.
4. Puntos de autoguardado.
5. Reanudación de escenas.

## Prioridad 3 — UX/UI

1. HUD.
2. Calendario.
3. Panel del mapa.
4. Equipo.
5. Noticias.
6. Prensa contextual.
7. Accesibilidad.

## Prioridad 4 — Audiovisual

1. Biblia visual.
2. Escenarios.
3. Personajes modulares.
4. Animaciones.
5. Audio.
6. VFX.

---

# 30. Criterios de aceptación del MVP

El MVP se considera funcional cuando el jugador puede:

1. Crear un partido y candidato.
2. Elegir dificultad.
3. Elegir inversor.
4. Formar el equipo.
5. Comenzar con una semilla reproducible.
6. Consultar mapa, calendario, equipo y noticias.
7. Programar actividades.
8. Delegar tareas.
9. Gastar y recibir dinero.
10. Realizar actos.
11. Participar en entrevistas.
12. Negociar con rivales.
13. Investigar personas y eventos.
14. Organizar o sufrir un piquete.
15. Responder a eventos.
16. Sufrir operaciones políticas.
17. Ver reacciones rivales.
18. Participar en un debate 3D.
19. Entrar en Niebla Electoral.
20. Llegar a la veda.
21. Ver el escrutinio.
22. Ganar en primera vuelta, perder o clasificar al balotaje.
23. Jugar las dos semanas de balotaje.
24. Participar en el debate final.
25. Ver fraude cuando corresponda.
26. Ganar o perder.
27. Ver el resumen final.
28. Iniciar otra campaña o volver al menú.
29. Guardar y cargar la partida.
30. Comprender, mediante logs o resumen interno, por qué se produjo el resultado.

---

# 31. Orden recomendado de trabajo

1. Cerrar el motor electoral.
2. Definir modelos de datos.
3. Diseñar arquitectura de Unity.
4. Crear simulador sin UI.
5. Probar miles de campañas automáticas.
6. Implementar calendario.
7. Implementar economía.
8. Implementar mapa.
9. Implementar equipo y tareas.
10. Implementar actividades.
11. Implementar eventos.
12. Implementar rivales.
13. Implementar investigaciones y noticias.
14. Implementar Niebla Electoral.
15. Implementar elecciones y fraude.
16. Implementar balotaje.
17. Implementar HUD.
18. Implementar escenas 3D.
19. Integrar guardado.
20. Producir y balancear contenido.
21. Hacer pruebas completas.
22. Optimizar.
23. Pulir presentación y accesibilidad.

---

# 32. Fuente de este documento

Este archivo fue consolidado a partir de:

- Las decisiones tomadas por el responsable del proyecto en las conversaciones de diseño de Poliyo.
- El plan original recuperado del historial del proyecto.
- Los mockups de HUD, menú principal, equipo, mapa, selección de equipo y calendario compartidos por el responsable.

No sustituye una futura especificación técnica. Su función es actuar como **fuente canónica de diseño del MVP** y evitar que el agente reabra decisiones ya cerradas o aumente el alcance sin aprobación.

---

# 33. Resumen ejecutivo para el agente

Construir un MVP de **Poliyo** en Unity para PC:

- Campaña presidencial de 60 días.
- República Federal de Roscalia.
- Cinco candidatos.
- Tres dificultades.
- Seis provincias y hasta 24 localidades.
- Elección determinista por semilla.
- Confianza, intención, rechazo, participación y voto en blanco.
- Economía, inversor y equipo con información oculta.
- Dos o tres actividades semanales.
- Actos, entrevistas y negociaciones.
- Siete tareas delegadas.
- Eventos sociales, políticos y nacionales.
- Piquetes organizables.
- Investigaciones con rumor, indicio y prueba.
- Debate 3D con fase abierta.
- Niebla Electoral.
- Veda sin gameplay.
- Escrutinio animado.
- Fraude limitado y explicable.
- Balotaje de dos semanas.
- Pantalla final con resultado, confianza, equipo revelado, tiempo y principales decisiones.

No implementar gobierno posterior, otras elecciones, multiplayer ni sistemas administrativos que no impacten el loop principal.

La siguiente tarea de diseño es especificar formalmente el motor electoral antes de comenzar la implementación del núcleo.
