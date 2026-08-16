# Poliyo — Sistema visual de la mesa de campaña

## Producto y objetivo

Poliyo es un juego premium de estrategia, simulación electoral y sátira política para PC/Steam. La experiencia principal es una campaña presidencial de 60 días en la República Federal de Roscalia. El jugador debe interpretar un electorado imperfectamente conocido, administrar equipo y fondos, elegir dónde intervenir y entender las consecuencias de sus decisiones.

Este sistema guía la escena estratégica central del vertical slice: la Mesa de campaña. Debe sentirse como una central de operaciones viva, legible y con personalidad rioplatense, no como un dashboard corporativo ni como una planilla fría.

## Patrones de referencia investigados

Usar los juegos como referencias de interacción, no como estilos para copiar:

- Democracy 4: relaciones causales, nodos, indicadores y lectura de confianza vs. intención.
- Suzerain: prensa como objeto narrativo, documentos seleccionables y decisiones que interrumpen la rutina.
- The Political Machine 2024: campaña sobre mapa, rivales visibles, tarjetas de acción y feedback rápido.
- Rebel Inc.: capas territoriales, urgencias localizadas y estado del mapa como información accionable.
- Football Manager: calendario, bandeja de noticias, estado del equipo y prioridades agrupadas.

Fuente primaria de estilo para este proyecto: **Lumina SaaS Landing Page**, adaptada al lenguaje de juego de Poliyo. No mezclar con otra fuente de estilo.

## Dirección visual

Estética: neo-brutalismo cálido, editorial y táctil, con sátira política gráfica. La interfaz debe parecer armada con afiches de campaña, carpetas de prensa, tarjetas de cartón y anotaciones de una mesa llena de trabajo.

Evitar: aspecto militar, cyberpunk, glassmorphism, neón sobre negro, pixel art, gradientes, sombras difusas, UI genérica de SaaS, estética de terminal o realismo fotográfico.

La escena es 2.5D estilizada: fondos planos con grano y patrones de puntos, tarjetas con volumen mediante sombras duras, iconos simples y una ilustración de mapa como foco contextual. Los recursos visuales deben sugerir collage editorial sin sacrificar la lectura a 1920×1080.

## Paleta

Superficies principales y marca:

- Amarillo campaña: `#FFE17C` — fondo de cabecera, foco, acciones principales y estados de atención.
- Carbón Roscalia: `#171E19` — fondo profundo, barra superior, texto sobre amarillo y zonas de contraste.
- Salvia institucional: `#B7C6C2` — superficies secundarias, paneles informativos, líneas y datos neutros.
- Blanco papel: `#FFFFFF` — tarjetas, documentos y áreas de lectura.
- Tinta: `#000000` — texto principal, bordes y sombras duras.

Colores semánticos y de datos, sólo para información electoral o estados; nunca deben reemplazar la etiqueta escrita:

- Alerta / escándalo: `#E85D4A`.
- Tendencia favorable: `#4B8E63`.
- Señal informativa: `#57B8D1`.
- Rival / presión: `#D86A9A`.
- Dato incierto: `#8D79C7`.

Los cinco candidatos pueden usar pequeñas bandas o puntos de sus colores. El color nunca es el único indicador: acompañar siempre con nombre, icono, forma o texto.

## Tipografía

- Display y titulares: **Cabinet Grotesk**, 700–800. Titulares grandes, compactos, con personalidad de afiche.
- Texto y UI: **Satoshi**, 500–700. Alta legibilidad en mayúsculas cortas, etiquetas y botones.
- Datos breves: Satoshi Semibold con números tabulares si están disponibles.
- No usar serif ornamental, manuscrita ni monospace como tipografía principal.

Jerarquía de referencia: título de escena 32–40 px; nombre de módulo 18–22 px; cuerpo 14–16 px; metadatos 11–12 px con tracking ligeramente abierto. Mantener contraste alto y no comprimir párrafos largos en tarjetas pequeñas.

## Geometría y componentes

- Bordes negros sólidos de 2 px en tarjetas, botones, pestañas y separadores importantes.
- Sombras duras sin blur: 4 px × 4 px para tarjetas; 8 px × 8 px para contenedores o acción primaria.
- Radios pequeños y controlados: 0–12 px. No usar cápsulas grandes salvo badges de estado.
- Botones con sensación física: al hover/press se desplazan 3–4 px hacia la sombra y la sombra se reduce.
- Tarjetas asimétricas sólo en elementos editoriales o de noticia; paneles operativos deben conservar alineación y lectura.
- Iconos sólidos, geométricos y simples, siempre acompañados por texto en acciones críticas.
- Usar patrones de puntos de baja opacidad sobre amarillo para dar textura; no usar gradientes.

## Composición de la escena central

Viewport principal: escritorio 16:9, 1920×1080. La composición debe conservar márgenes seguros en resoluciones menores.

1. Cabecera fija de 88 px: logo Poliyo, día de campaña, hora/franja, fondos disponibles y accesos Calendario / Mapa / Equipo. El tab activo es amarillo con borde negro y sombra dura.
2. Columna izquierda de 270 px: bloque “Pulso nacional” con confianza, intención de voto y rechazo separados. Antes de la Niebla mostrar cifra, fecha y calidad de fuente; durante la Niebla mostrar tendencia cualitativa, icono de incertidumbre y última medición conocida.
3. Centro amplio: Mesa de campaña con una tarjeta de prioridad semanal, mini-mapa ilustrado de Roscalia o resumen territorial, y cinco barras compactas de candidatos. Debe poder entenderse quién sube, quién cae y por qué sin leer todos los detalles.
4. Columna derecha de 330 px: “Agenda de hoy” con tareas sin asignar, entrevista pendiente, fondos comprometidos y alertas urgentes. Las alertas críticas deben estar arriba y usar icono + texto + color.
5. Franja inferior persistente de 190–220 px: Noticias. Mostrar tres titulares como recortes de prensa, fuente, fecha y una etiqueta de impacto. El botón “Hablar con la prensa” vive dentro de esta franja o abre un drawer contextual.
6. Acción de avance: botón “SIGUIENTE DÍA” visible en la esquina inferior derecha, grande, amarillo o carbón según contraste. Debe mostrar una advertencia si hay tareas sin asignar o eventos urgentes.

## Contenido de referencia para el draft

Usar textos ficticios y concretos:

- “DÍA 18 / 60 — MARTES 14 DE MAYO”.
- Fondos: “₱ 184.200” y compromiso semanal “₱ 62.000”.
- Pulso: Confianza 42% (fuente mixta); intención 28% (encuesta de ayer); rechazo 19%.
- Estado: “La Niebla Electoral comienza en 12 días”.
- Titulares: “Puerto Alba discute el precio del transporte”; “La vocera de Federales acusa doble discurso”; “Un video viejo vuelve a circular”.
- Agenda: “Asignar investigación — Seguridad”, “Confirmar entrevista — Canal 6”, “Revisar promesa territorial — Valle Gris”.
- Acción contextual: “Hablar con la prensa”.

## UX y accesibilidad

Información por capas: resumen primero y detalle al seleccionar. No llenar la pantalla con paneles simultáneos. Cada métrica debe diferenciar confianza, intención, rechazo y participación. Toda cifra debe llevar fecha y calidad de fuente. Las alertas no pueden depender sólo del color. Mantener foco visible, tamaños de texto escalables, iconos con etiquetas, subtítulos y estados legibles en alto contraste.

## Motion

Microinteracciones cortas y físicas: desplazamiento de 3–4 px al presionar, entrada de titulares como fichas de papel, actualización de barras con un pequeño rebote y pulso sutil para alertas nuevas. Sin parallax agresivo ni movimiento constante. Preparar una variante de movimiento reducido.

## Restricciones de fidelidad

Usar únicamente las fuentes, colores, espaciados, bordes, sombras y estilos de componentes definidos aquí. No introducir tipografías, colores, gradientes, glassmorphism, sombras suaves ni estilos visuales externos al sistema. La dirección puede adaptar la composición a una escena de juego, pero no cambiar la identidad neo-brutalista cálida ni la legibilidad estratégica.
