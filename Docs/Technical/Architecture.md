# Arquitectura técnica inicial de Poliyo

## Objetivo

La primera entrega jugable será un vertical slice sistémico de 60 días. La arquitectura debe permitir ejecutar la campaña completa de manera determinista, explicable y testeable antes de ampliar el volumen de contenido o la presentación final.

## Límite de módulos

| Módulo | Responsabilidad | Dependencias permitidas |
| --- | --- | --- |
| `Poliyo.Core` | Identificadores, semilla, aleatoriedad determinista, resultados y tipos compartidos. | Ninguna. |
| `Poliyo.Simulation` | Estado de campaña y reglas electorales, económicas y sociales. | Core. |
| `Poliyo.Content` | Definiciones de autoría, catálogos y validación de contenido. | Core. |
| `Poliyo.AI` | Planificación y conocimiento limitado de rivales. | Core, Simulation, Content. |
| `Poliyo.Application` | Comandos, consultas, fases de campaña, orquestación y trazabilidad. | Core, Simulation, Content, AI. |
| `Poliyo.Presentation` | UI, escenas, audio y puentes Unity. | Application, Content. |

Las dependencias fluyen hacia los módulos estables. El dominio nunca depende de Unity, escenas ni UI. Las herramientas de editor se crearán en una assembly Editor separada cuando exista la primera necesidad concreta.

## Flujo de una decisión

```text
Jugador o rival
  -> comando validado
  -> simulación determinista
  -> estado de campaña y registros causales
  -> snapshot de lectura
  -> UI, audio y escena
```

La presentación no modifica el estado de campaña. Todas las decisiones pasan por comandos y producen causas legibles para el jugador y para depuración.

## Propiedad de datos

- C# puro: estado de campaña, reglas, decisiones, cálculos, IA y persistencia.
- ScriptableObject: definiciones editables de contenido y configuración de balance.
- Escenas y prefabs: composición visual y referencias explícitas de presentación.
- Archivos de guardado: snapshots versionados de una campaña; nunca assets de Unity mutables.

## Reglas de implementación

- Un único bootstrap inicializará los servicios de presentación y mantendrá el orden explícito.
- No se utilizarán singletons ocultos ni búsquedas globales durante el juego.
- La misma semilla y secuencia de comandos deben devolver el mismo estado final.
- Confianza, intención de voto, rechazo y participación se almacenan y calculan por separado.
- Las pruebas de reglas viven en EditMode; los flujos de escena y presentación, en PlayMode.
