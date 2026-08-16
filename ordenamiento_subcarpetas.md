Buenas prácticas para organizar la carpeta Assets en Unity

Una buena estructura dentro de Assets/ ayuda a mantener el proyecto escalable, facilita encontrar archivos y reduce errores al trabajar con escenas, prefabs, scripts, ScriptableObjects y assets externos.

Unity no obliga a utilizar una estructura concreta para todo el proyecto, pero sí existen carpetas con comportamiento especial como Editor, Resources, StreamingAssets y Plugins.

Estructura recomendada

Para un proyecto como Poliyo, una estructura equilibrada podría ser:

Assets/
│
├── _Poliyo/
│   │
│   ├── Art/
│   │   ├── Animations/
│   │   ├── Fonts/
│   │   ├── Materials/
│   │   ├── Sprites/
│   │   │   ├── Characters/
│   │   │   ├── Environment/
│   │   │   ├── Icons/
│   │   │   └── UI/
│   │   └── VFX/
│   │
│   ├── Audio/
│   │   ├── Music/
│   │   ├── SFX/
│   │   └── Ambience/
│   │
│   ├── Code/
│   │   ├── Core/
│   │   ├── Gameplay/
│   │   │   ├── Campaign/
│   │   │   ├── Economy/
│   │   │   ├── Elections/
│   │   │   ├── Events/
│   │   │   ├── Opponents/
│   │   │   └── Party/
│   │   ├── UI/
│   │   ├── Data/
│   │   ├── Services/
│   │   ├── Utilities/
│   │   ├── Editor/
│   │   └── Tests/
│   │
│   ├── Data/
│   │   ├── Config/
│   │   ├── Difficulty/
│   │   ├── Events/
│   │   ├── Candidates/
│   │   └── Parties/
│   │
│   ├── Prefabs/
│   │   ├── Gameplay/
│   │   ├── Systems/
│   │   ├── UI/
│   │   └── VFX/
│   │
│   ├── Scenes/
│   │   ├── Bootstrap/
│   │   ├── Gameplay/
│   │   ├── Menus/
│   │   └── Tests/
│   │
│   ├── Settings/
│   └── Localization/
│
├── ThirdParty/
│
├── Plugins/
│
└── StreamingAssets/

¿Por qué usar una carpeta _Poliyo?

Todo lo desarrollado específicamente para el juego queda dentro de un único lugar.

Esto permite separar claramente:

Código propio.

Arte propio.

Prefabs propios.

Datos del juego.

Assets comprados.

Plugins.

SDK externos.

El prefijo _ no tiene ningún comportamiento especial en Unity. Se utiliza simplemente para hacer que la carpeta aparezca arriba al ordenar alfabéticamente.

Organización del código

Evitar una carpeta Scripts/ con cientos de archivos mezclados.

Una organización tradicional como:

Scripts/
├── Managers/
├── Controllers/
├── Models/
└── Helpers/

puede funcionar inicialmente, pero suele perder claridad cuando el proyecto crece.

Es preferible organizar el código según el dominio o sistema del juego:

Code/
├── Core/
├── Gameplay/
│   ├── Economy/
│   ├── Elections/
│   ├── Campaign/
│   └── Events/
├── UI/
└── Services/

Por ejemplo:

Gameplay/
└── Economy/
    ├── EconomyManager.cs
    ├── EconomyService.cs
    ├── MonthlyIncomeCalculator.cs
    ├── CampaignExpense.cs
    └── InvestorIncome.cs

Por qué

Los archivos relacionados con una funcionalidad se encuentran juntos.

Para qué

Facilita:

mantenimiento;

refactors;

debugging;

navegación;

incorporación de nuevas mecánicas.

Trade-off

Una organización excesivamente granular genera demasiadas carpetas.

Por eso conviene dividir solamente los sistemas que realmente tengan suficiente contenido.

Alternativa para proyectos grandes: organización por Features

En proyectos considerablemente más grandes puede resultar útil agrupar todo lo relacionado con una feature:

Features/
├── Economy/
│   ├── Scripts/
│   ├── Prefabs/
│   ├── Data/
│   └── UI/
│
├── Elections/
│   ├── Scripts/
│   ├── Prefabs/
│   ├── Data/
│   └── UI/
│
└── Campaign/
    ├── Scripts/
    ├── Prefabs/
    ├── Data/
    └── UI/

Para un MVP como Poliyo probablemente todavía sea innecesario.

Una estructura híbrida suele ofrecer un mejor equilibrio.

Reglas importantes

1. No crear carpetas innecesarias

No hace falta crear toda la estructura desde el primer día.

Si actualmente el juego no tiene música, por ejemplo:

Audio/Music/

puede crearse cuando realmente sea necesario.

Tener muchas carpetas vacías tampoco significa tener un proyecto organizado.

2. Evitar carpetas ambiguas

Evitar nombres como:

Misc/
Others/
New/
Old/
Temp/
Things/
Pruebas/
Scripts2/
Backup/

Estas carpetas suelen transformarse rápidamente en depósitos de archivos sin una responsabilidad clara.

Una carpeta debería expresar claramente qué tipo de contenido contiene.

3. Separar escenas reales de escenas de pruebas

Ejemplo:

Scenes/
├── Bootstrap/
│   └── Bootstrap.unity
├── Menus/
│   ├── MainMenu.unity
│   └── CharacterCreation.unity
├── Gameplay/
│   └── Campaign.unity
└── Tests/
    ├── UI_Test.unity
    └── Economy_Test.unity

Evitar:

Main.unity
Main2.unity
MainFinal.unity
MainFinal2.unity
Test.unity
TestNuevo.unity

Las versiones anteriores deberían conservarse mediante Git, no duplicando escenas.

4. Separar ScriptableObjects del código que los define

Código:

Code/
└── Gameplay/
    └── Economy/
        └── DifficultyConfig.cs

Instancias:

Data/
└── Difficulty/
    ├── CowardDifficulty.asset
    ├── ModerateDifficulty.asset
    └── LatamDifficulty.asset

Esto diferencia claramente:

la definición C#;

los datos configurables desde Unity.

Carpetas especiales de Unity

Unity reconoce determinadas carpetas por nombre y les asigna comportamiento especial.

Editor

Los scripts colocados dentro de una carpeta llamada Editor se consideran herramientas del Unity Editor.

Ejemplo:

Code/
├── Gameplay/
│   └── ...
│
└── Editor/
    ├── EventDatabaseEditor.cs
    └── EconomyDebugWindow.cs

No colocar gameplay runtime dentro de Editor.

Resources

Unity permite cargar objetos en tiempo de ejecución mediante:

Resources.Load(...)

Sin embargo, no es recomendable utilizar Resources como depósito general de assets.

Evitar estructuras como:

Assets/
└── Resources/
    └── TODO_EL_JUEGO

Guardar grandes cantidades de contenido dentro de Resources puede complicar la administración de memoria y del contenido incluido en el build.

Para sistemas de carga dinámica más grandes existe el sistema Addressables.

Trade-off

Addressables agrega complejidad.

Para un MVP pequeño o mediano no conviene introducirlo salvo que exista una necesidad concreta.

StreamingAssets

Los archivos dentro de:

StreamingAssets/

se copian al build conservando su formato original.

Puede ser útil para contenido externo como:

StreamingAssets/
├── ExternalData/
└── Config/

No debería utilizarse como carpeta genérica.

Plugins

Unity utiliza Plugins para plugins y bibliotecas externas.

No conviene colocar código normal del juego dentro de:

Plugins/

Debería reservarse para componentes que realmente sean plugins o dependencias externas.

Assembly Definitions (.asmdef)

Cuando el código empieza a crecer, puede ser conveniente dividirlo en assemblies.

Ejemplo:

Code/
├── Core/
│   └── Poliyo.Core.asmdef
│
├── Gameplay/
│   └── Poliyo.Gameplay.asmdef
│
├── UI/
│   └── Poliyo.UI.asmdef
│
└── Tests/
    └── Poliyo.Tests.asmdef

Una dependencia razonable sería:

Poliyo.Core
      ↑
Poliyo.Gameplay
      ↑
Poliyo.UI

Evitar dependencias circulares como:

UI ↔ Gameplay ↔ Core ↔ Todo

Por qué

Los Assembly Definitions permiten definir explícitamente qué partes del proyecto dependen de cuáles.

Para qué

Ayudan a:

reducir acoplamiento;

mejorar la arquitectura;

disminuir recompilaciones innecesarias en proyectos grandes;

controlar dependencias entre sistemas.

Trade-off

Crear demasiados .asmdef en un proyecto pequeño introduce complejidad innecesaria.

Para Poliyo sería razonable comenzar, si hace falta, con:

Core

Gameplay

UI

Tests

Mover archivos siempre desde Unity

Unity utiliza archivos .meta para mantener información de cada asset, incluyendo su GUID.

Por ejemplo:

Player.prefab
Player.prefab.meta

Las referencias entre assets dependen de estos identificadores.

Por eso es preferible mover y renombrar assets desde la ventana Project del Unity Editor.

Esto permite que Unity mantenga correctamente el archivo .meta asociado.

Los archivos .meta deben incluirse en Git.

Convenciones de nombres

La regla más importante es elegir una convención y mantenerla durante todo el proyecto.

Una opción limpia es utilizar PascalCase.

Escenas

MainMenu.unity
CampaignMap.unity
CharacterCreation.unity

Prefabs

Player.prefab
ElectionCard.prefab
CandidatePanel.prefab

Scripts

DifficultyConfig.cs
EconomyManager.cs
ElectionSimulator.cs

Audio

ButtonClick.wav
ElectionVictory.wav
CampaignMusic.wav

Sprites

CandidateBackground.png
PartyLogo.png
ElectionIcon.png

No es necesario utilizar prefijos como:

pref_Player.prefab
sc_MainMenu.unity
scr_EconomyManager.cs
snd_Button.wav
tex_Background.png

Unity ya permite identificar y filtrar assets por tipo.

Estos prefijos solamente son recomendables si el equipo decidió explícitamente utilizarlos y existe una razón concreta.

Prioridades para organizar un proyecto existente

Prioridad

Cambio

Impacto

Esfuerzo

1

Crear Assets/_Poliyo

Muy alto

Bajo

2

Separar Code, Art, Audio, Prefabs, Scenes y Data

Muy alto

Bajo

3

Organizar código por sistemas de gameplay

Muy alto

Medio

4

Eliminar Misc, duplicados y carpetas temporales

Alto

Bajo

5

Separar escenas de producción y pruebas

Alto

Bajo

6

Organizar ScriptableObjects dentro de Data

Alto

Bajo

7

Añadir algunos .asmdef

Medio/alto

Medio

8

Implementar Addressables

Bajo actualmente

Alto

Estructura recomendada específicamente para Poliyo

Assets/
└── _Poliyo/
    ├── Art/
    ├── Audio/
    ├── Code/
    │   ├── Core/
    │   ├── Gameplay/
    │   │   ├── Campaign/
    │   │   ├── Economy/
    │   │   ├── Elections/
    │   │   ├── Events/
    │   │   ├── Opponents/
    │   │   └── Party/
    │   ├── UI/
    │   ├── Services/
    │   ├── Utilities/
    │   └── Tests/
    ├── Data/
    ├── Prefabs/
    ├── Scenes/
    ├── Settings/
    └── Localization/

Esta estructura debería ser suficiente para llevar Poliyo desde el MVP hasta una versión considerablemente más grande sin necesitar una reorganización completa.

Principio general

Si no podés adivinar dónde debería estar un archivo sin buscarlo, probablemente la estructura del proyecto necesite mejorar.

La estructura debe facilitar el desarrollo, no convertirse en una obligación burocrática.