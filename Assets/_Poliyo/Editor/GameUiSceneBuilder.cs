using System.Collections.Generic;
using Poliyo.Content;
using Poliyo.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Poliyo.Presentation.Editor
{
/// <summary>Builds the authored UGUI screen baseline without touching the existing TeamScene.</summary>
public static class GameUiSceneBuilder
{
    private const string SceneFolder = "Assets/_Poliyo/Scenes";
    private const string MainMenuScenePath = SceneFolder + "/MainMenu.unity";
    private const string CalendarScenePath = SceneFolder + "/CampaignCalendar.unity";
    private const string MapScenePath = SceneFolder + "/CampaignMap.unity";
    private const string ContentCatalogPath = "Assets/_Poliyo/Content/CampaignCatalog.asset";

    private static readonly Color BackgroundColor = new Color(0.04f, 0.07f, 0.12f, 1f);
    private static readonly Color SurfaceColor = new Color(0.08f, 0.16f, 0.24f, 1f);
    private static readonly Color SurfaceAltColor = new Color(0.12f, 0.25f, 0.33f, 1f);
    private static readonly Color AccentColor = new Color(1f, 0.63f, 0.16f, 1f);
    private static readonly Color TextColor = new Color(0.94f, 0.96f, 0.97f, 1f);
    private static readonly Color MutedTextColor = new Color(0.68f, 0.77f, 0.81f, 1f);

    [MenuItem("Poliyo/UI/Create or Update Prototype Screens")]
    public static void CreateOrUpdatePrototypeScreens()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        CreateMainMenu();
        CreateCalendar();
        CreateMap();
        AddScenesToBuildSettings();
        AssetDatabase.SaveAssets();
    }

    private static void CreateMainMenu()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CampaignGameSessionHost sessionHost = CreateSessionHost(LoadContentCatalog());
        GameObject canvas = CreateCanvas("MainMenuCanvas");
        AddBackground(canvas.transform);

        Image frame = CreatePanel(canvas.transform, "MenuFrame", SurfaceColor);
        Stretch(frame.rectTransform, 0.08f, 0.08f, 0.08f, 0.08f);

        CreateText(frame.transform, "GameLogo", "POLIYO", 62, TextAlignmentOptions.Left, AccentColor, new Vector2(84f, -88f), new Vector2(620f, 96f));
        CreateText(frame.transform, "Subtitle", "Estrategia electoral en la República Federal de Roscalia", 22, TextAlignmentOptions.Left, MutedTextColor, new Vector2(88f, -178f), new Vector2(650f, 54f));

        UiSceneNavigation navigator = frame.gameObject.AddComponent<UiSceneNavigation>();
        Button newCampaign = CreateButton(frame.transform, "NewCampaignButton", "Campaña nueva", new Vector2(100f, -300f), new Vector2(420f, 72f));
        Button loadCampaign = CreateButton(frame.transform, "LoadCampaignButton", "Cargar campaña", new Vector2(100f, -390f), new Vector2(420f, 72f));
        Button options = CreateButton(frame.transform, "OptionsButton", "Opciones", new Vector2(100f, -480f), new Vector2(420f, 72f));
        Button quit = CreateButton(frame.transform, "QuitButton", "Salir", new Vector2(100f, -570f), new Vector2(420f, 72f));
        CreateText(frame.transform, "MenuNote", "Vertical slice · interfaz en construcción", 18, TextAlignmentOptions.Left, MutedTextColor, new Vector2(100f, -690f), new Vector2(560f, 44f));

        UnityEventTools.AddPersistentListener(newCampaign.onClick, sessionHost.StartNewCampaign);
        UnityEventTools.AddStringPersistentListener(newCampaign.onClick, navigator.OpenScene, "CampaignSlice");
        UnityEventTools.AddPersistentListener(loadCampaign.onClick, sessionHost.LoadAutosave);
        UnityEventTools.AddStringPersistentListener(loadCampaign.onClick, navigator.OpenScene, "CampaignSlice");
        UnityEventTools.AddPersistentListener(quit.onClick, navigator.QuitGame);
        options.interactable = false;

        SaveScene(scene, MainMenuScenePath);
    }

    private static void CreateCalendar()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateSessionHost(LoadContentCatalog());
        GameObject canvas = CreateCanvas("CampaignCalendarCanvas");
        AddBackground(canvas.transform);
        UiSceneNavigation navigator = canvas.AddComponent<UiSceneNavigation>();

        CreateTopBar(canvas.transform, "Calendario", navigator, out Button mapButton, out Button teamButton);
        UnityEventTools.AddStringPersistentListener(mapButton.onClick, navigator.OpenScene, "CampaignMap");
        UnityEventTools.AddStringPersistentListener(teamButton.onClick, navigator.OpenScene, "TeamScene");

        Image calendarPanel = CreatePanel(canvas.transform, "CalendarPanel", SurfaceColor);
        Stretch(calendarPanel.rectTransform, 0.06f, 0.15f, 0.38f, 0.16f);
        TMP_Text dayLabel = CreateText(calendarPanel.transform, "CalendarHeading", "Semana 1 · Día 1", 30, TextAlignmentOptions.Left, TextColor, new Vector2(42f, -34f), new Vector2(480f, 54f));
        CreateText(calendarPanel.transform, "CalendarHint", "Planificá una actividad pública por día.", 19, TextAlignmentOptions.Left, MutedTextColor, new Vector2(42f, -84f), new Vector2(550f, 36f));

        var dayNames = new[] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes" };
        for (var column = 0; column < dayNames.Length; column++)
        {
            float x = 36f + (column * 176f);
            CreateText(calendarPanel.transform, "DayHeader" + column, dayNames[column], 18, TextAlignmentOptions.Center, AccentColor, new Vector2(x, -136f), new Vector2(160f, 42f));
            for (var row = 0; row < 4; row++)
            {
                Image cell = CreatePanel(calendarPanel.transform, "CalendarCell_" + row + "_" + column, SurfaceAltColor);
                SetRect(cell.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, -194f - (row * 132f)), new Vector2(160f, 112f));
            }
        }

        Button rally = CreateButton(calendarPanel.transform, "CapitalRallyButton", "Acto capital", new Vector2(212f, -212f), new Vector2(136f, 70f));
        Button interview = CreateButton(calendarPanel.transform, "InterviewButton", "Entrevista", new Vector2(388f, -212f), new Vector2(136f, 70f));
        Button teamMeeting = CreateButton(calendarPanel.transform, "TeamMeetingButton", "Reunión de equipo", new Vector2(740f, -212f), new Vector2(136f, 70f));
        teamMeeting.interactable = false;

        Image actionPanel = CreatePanel(canvas.transform, "CalendarActions", SurfaceColor);
        Stretch(actionPanel.rectTransform, 0.65f, 0.15f, 0.06f, 0.16f);
        CreateText(actionPanel.transform, "ActionsHeading", "Acciones del día", 27, TextAlignmentOptions.Left, TextColor, new Vector2(36f, -34f), new Vector2(320f, 46f));
        TMP_Text statusLabel = CreateText(actionPanel.transform, "ActionsText", "Fondos: $1200 · alcance nacional", 18, TextAlignmentOptions.Left, MutedTextColor, new Vector2(36f, -86f), new Vector2(440f, 78f));
        Button nextDay = CreateButton(actionPanel.transform, "NextDayButton", "Siguiente día", new Vector2(36f, -230f), new Vector2(310f, 68f));
        Button negotiation = CreateButton(actionPanel.transform, "NegotiationButton", "Negociación", new Vector2(36f, -312f), new Vector2(310f, 58f));
        CampaignCalendarScreenPresenter presenter = calendarPanel.gameObject.AddComponent<CampaignCalendarScreenPresenter>();
        presenter.Configure(dayLabel, statusLabel, rally, nextDay);
        UnityEventTools.AddPersistentListener(rally.onClick, presenter.ResolveRally);
        UnityEventTools.AddPersistentListener(negotiation.onClick, presenter.ResolveNegotiation);
        UnityEventTools.AddPersistentListener(nextDay.onClick, presenter.AdvanceDay);

        Image drawerPanel = CreatePanel(canvas.transform, "InterviewDrawer", new Color(0.06f, 0.12f, 0.18f, 0.98f));
        Stretch(drawerPanel.rectTransform, 0.52f, 0.18f, 0.08f, 0.18f);
        CanvasGroup drawerCanvasGroup = drawerPanel.gameObject.AddComponent<CanvasGroup>();
        CalendarInterviewDrawer drawer = drawerPanel.gameObject.AddComponent<CalendarInterviewDrawer>();
        drawer.Configure(drawerCanvasGroup);
        CreateText(drawerPanel.transform, "InterviewHeading", "Entrevista", 30, TextAlignmentOptions.Left, AccentColor, new Vector2(36f, -32f), new Vector2(310f, 50f));
        CreateText(drawerPanel.transform, "InterviewDescription", "Elegí un medio para preparar la aparición.", 18, TextAlignmentOptions.Left, MutedTextColor, new Vector2(36f, -84f), new Vector2(390f, 40f));
        Button radio = CreateButton(drawerPanel.transform, "RadioButton", "Radio Roscalia", new Vector2(36f, -148f), new Vector2(340f, 52f));
        Button digital = CreateButton(drawerPanel.transform, "DigitalButton", "Canal Digital", new Vector2(36f, -214f), new Vector2(340f, 52f));
        Button national = CreateButton(drawerPanel.transform, "NationalButton", "Noticias Nacionales", new Vector2(36f, -280f), new Vector2(340f, 52f));
        Button closeDrawer = CreateButton(drawerPanel.transform, "CloseInterviewDrawerButton", "Cerrar", new Vector2(36f, -364f), new Vector2(180f, 52f));
        UnityEventTools.AddPersistentListener(interview.onClick, drawer.Toggle);
        UnityEventTools.AddPersistentListener(closeDrawer.onClick, drawer.Close);
        UnityEventTools.AddPersistentListener(radio.onClick, presenter.ResolveInterview);
        UnityEventTools.AddPersistentListener(digital.onClick, presenter.ResolveInterview);
        UnityEventTools.AddPersistentListener(national.onClick, presenter.ResolveInterview);

        SaveScene(scene, CalendarScenePath);
    }

    private static void CreateMap()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateSessionHost(LoadContentCatalog());
        GameObject canvas = CreateCanvas("CampaignMapCanvas");
        AddBackground(canvas.transform);
        UiSceneNavigation navigator = canvas.AddComponent<UiSceneNavigation>();
        CreateTopBar(canvas.transform, "Mapa de Roscalia", navigator, out Button mapButton, out Button teamButton);
        mapButton.interactable = false;
        UnityEventTools.AddStringPersistentListener(teamButton.onClick, navigator.OpenScene, "TeamScene");

        Image mapPanel = CreatePanel(canvas.transform, "RoscaliaMap", SurfaceColor);
        Stretch(mapPanel.rectTransform, 0.06f, 0.15f, 0.47f, 0.16f);
        CreateText(mapPanel.transform, "MapInstructions", "Seleccioná una jurisdicción", 26, TextAlignmentOptions.Left, TextColor, new Vector2(40f, -38f), new Vector2(400f, 44f));
        CreateText(mapPanel.transform, "MapCaption", "Mapa ilustrado de Roscalia · 6 jurisdicciones", 18, TextAlignmentOptions.Left, MutedTextColor, new Vector2(40f, -82f), new Vector2(460f, 38f));

        Image detailsPanel = CreatePanel(canvas.transform, "ZoneDetailsDrawer", SurfaceColor);
        Stretch(detailsPanel.rectTransform, 0.61f, 0.15f, 0.06f, 0.16f);
        TMP_Text zoneName = CreateText(detailsPanel.transform, "ZoneName", "Seleccioná una jurisdicción", 28, TextAlignmentOptions.Left, AccentColor, new Vector2(36f, -38f), new Vector2(360f, 48f));
        TMP_Text zoneDescription = CreateText(detailsPanel.transform, "ZoneDescription", "Elegí una zona del mapa para consultar su información territorial.", 18, TextAlignmentOptions.TopLeft, MutedTextColor, new Vector2(36f, -100f), new Vector2(420f, 180f));
        CreateText(detailsPanel.transform, "ZoneActions", "Acciones territoriales", 22, TextAlignmentOptions.Left, TextColor, new Vector2(36f, -314f), new Vector2(320f, 40f));
        Button back = CreateButton(detailsPanel.transform, "BackToCalendarButton", "Volver al calendario", new Vector2(36f, -412f), new Vector2(330f, 62f));
        UnityEventTools.AddStringPersistentListener(back.onClick, navigator.OpenScene, "CampaignCalendar");

        CampaignMapScreenPresenter selection = detailsPanel.gameObject.AddComponent<CampaignMapScreenPresenter>();
        selection.Configure(zoneName, zoneDescription);
        var zoneIds = new[] { "puerto-alba", "gran-ribera", "ventisca", "cumbre-dorada", "monte-rojo", "sierra-clara" };
        var zones = new[] { "Puerto Alba", "Gran Ribera", "Ventisca", "Cumbre Dorada", "Monte Rojo", "Sierra Clara" };
        for (var index = 0; index < zones.Length; index++)
        {
            var column = index % 2;
            var row = index / 2;
            Button zone = CreateButton(mapPanel.transform, "ZoneButton_" + index, zones[index], new Vector2(72f + (column * 250f), -166f - (row * 132f)), new Vector2(212f, 98f));
            UnityEventTools.AddStringPersistentListener(zone.onClick, selection.SelectJurisdiction, zoneIds[index]);
        }

        SaveScene(scene, MapScenePath);
    }

    private static CampaignContentDefinition LoadContentCatalog()
    {
        CampaignContentDefinition catalog = AssetDatabase.LoadAssetAtPath<CampaignContentDefinition>(ContentCatalogPath);
        if (catalog == null)
        {
            throw new System.InvalidOperationException("Campaign catalog is missing. Run Poliyo/Vertical Slice/Create or Update Content and Scene first.");
        }

        return catalog;
    }

    private static CampaignGameSessionHost CreateSessionHost(CampaignContentDefinition catalog)
    {
        var hostObject = new GameObject("CampaignGameSessionHost");
        CampaignGameSessionHost host = hostObject.AddComponent<CampaignGameSessionHost>();
        host.Configure(catalog, 20260725UL, 1200f);
        return host;
    }
    private static GameObject CreateCanvas(string name)
    {
        var canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        CreateEventSystem();
        return canvasObject;
    }

    private static void CreateEventSystem()
    {
        var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystemObject.GetComponent<EventSystem>().sendNavigationEvents = true;
    }

    private static void AddBackground(Transform parent)
    {
        Image background = CreatePanel(parent, "Background", BackgroundColor);
        Stretch(background.rectTransform, 0f, 0f, 0f, 0f);
    }

    private static void CreateTopBar(Transform parent, string title, UiSceneNavigation navigator, out Button mapButton, out Button teamButton)
    {
        Image topBar = CreatePanel(parent, "TopBar", SurfaceColor);
        Stretch(topBar.rectTransform, 0f, 0.88f, 0f, 0f);
        CreateText(topBar.transform, "Brand", "POLIYO", 34, TextAlignmentOptions.Left, AccentColor, new Vector2(56f, -26f), new Vector2(250f, 52f));
        CreateText(topBar.transform, "ScreenTitle", title, 25, TextAlignmentOptions.Left, TextColor, new Vector2(290f, -28f), new Vector2(460f, 50f));
        Button calendarButton = CreateButton(topBar.transform, "CalendarNavigationButton", "Calendario", new Vector2(870f, -25f), new Vector2(160f, 54f));
        mapButton = CreateButton(topBar.transform, "MapNavigationButton", "Mapa", new Vector2(1050f, -25f), new Vector2(130f, 54f));
        teamButton = CreateButton(topBar.transform, "TeamNavigationButton", "Equipo", new Vector2(1200f, -25f), new Vector2(140f, 54f));
        UnityEventTools.AddStringPersistentListener(calendarButton.onClick, navigator.OpenScene, "CampaignCalendar");
    }

    private static Image CreatePanel(Transform parent, string name, Color color)
    {
        var panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(parent, false);
        Image image = panelObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, float fontSize, TextAlignmentOptions alignment, Color color, Vector2 anchoredPosition, Vector2 size)
    {
        var textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        var text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        SetRect(text.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), anchoredPosition, size);
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size)
    {
        var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.GetComponent<Image>();
        image.color = SurfaceAltColor;
        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
        colors.pressedColor = new Color(1f, 1f, 1f, 0.75f);
        colors.disabledColor = new Color(1f, 1f, 1f, 0.35f);
        button.colors = colors;
        SetRect(buttonObject.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), anchoredPosition, size);
        TMP_Text text = CreateText(buttonObject.transform, "Label", label, 20, TextAlignmentOptions.Center, TextColor, Vector2.zero, size);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        return button;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect, float left, float bottom, float right, float top)
    {
        rect.anchorMin = new Vector2(left, bottom);
        rect.anchorMax = new Vector2(1f - right, 1f - top);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SaveScene(Scene scene, string path)
    {
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, path);
    }

    private static void AddScenesToBuildSettings()
    {
        var buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        var paths = new[] { MainMenuScenePath, CalendarScenePath, MapScenePath, "Assets/_Poliyo/Scenes/TeamScene.unity" };
        foreach (string path in paths)
        {
            if (!buildScenes.Exists(scene => scene.path == path))
            {
                buildScenes.Add(new EditorBuildSettingsScene(path, true));
            }
        }

        EditorBuildSettings.scenes = buildScenes.ToArray();
    }
}
}
