using System;
using System.Collections.Generic;
using Poliyo.Content;
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
/// <summary>Creates the reproducible Canvas-based authoring baseline for the vertical slice.</summary>
public static class VerticalSliceContentAndSceneBuilder
{
    private const string ContentRoot = "Assets/_Poliyo/Content/Localities";
    private const string CatalogPath = "Assets/_Poliyo/Content/CampaignCatalog.asset";
    private const string ScenePath = "Assets/_Poliyo/Scenes/CampaignSlice.unity";

    [MenuItem("Poliyo/Vertical Slice/Create or Update Content and Scene")]
    public static void CreateOrUpdate()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        EnsureFolder("Assets/_Poliyo/Content");
        EnsureFolder(ContentRoot);
        EnsureFolder("Assets/_Poliyo/Scenes");

        LocalityDefinition[] localities = CreateLocalities();
        CampaignContentDefinition catalog = CreateCatalog(localities);
        CreateScene(catalog);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Poliyo", "Contenido y escena Canvas del vertical slice actualizados.", "Aceptar");
    }

    private static LocalityDefinition[] CreateLocalities()
    {
        LocalitySeed[] seeds =
        {
            new LocalitySeed("gran-ribera", "san-laureano", "San Laureano", 120), new LocalitySeed("gran-ribera", "las-espigas", "Las Espigas", 82), new LocalitySeed("gran-ribera", "las-aguadas", "Las Aguadas", 54), new LocalitySeed("gran-ribera", "estacion-ribera", "Estacion Ribera", 88),
            new LocalitySeed("sierra-clara", "santa-elvira", "Santa Elvira", 92), new LocalitySeed("sierra-clara", "san-aurelio", "San Aurelio", 64), new LocalitySeed("sierra-clara", "piedra-seca", "Piedra Seca", 71), new LocalitySeed("sierra-clara", "quebrada-honda", "Quebrada Honda", 38),
            new LocalitySeed("puerto-alba", "casco-federal", "Casco Federal", 180), new LocalitySeed("puerto-alba", "altos-del-alba", "Altos del Alba", 76), new LocalitySeed("puerto-alba", "darsena-vieja", "Darsena Vieja", 105), new LocalitySeed("puerto-alba", "bajo-del-faro", "Bajo del Faro", 150),
            new LocalitySeed("ventisca", "nueva-aurora", "Nueva Aurora", 76), new LocalitySeed("ventisca", "cerro-niveo", "Cerro Niveo", 42), new LocalitySeed("ventisca", "lago-sereno", "Lago Sereno", 48), new LocalitySeed("ventisca", "paso-blanco", "Paso Blanco", 24),
            new LocalitySeed("monte-rojo", "rojalba", "Rojalba", 58), new LocalitySeed("monte-rojo", "villa-cardenal", "Villa Cardenal", 33), new LocalitySeed("monte-rojo", "paso-del-condor", "Paso del Condor", 19), new LocalitySeed("monte-rojo", "piedra-sola", "Piedra Sola", 15),
            new LocalitySeed("cumbre-dorada", "villa-aurea", "Villa Aurea", 83), new LocalitySeed("cumbre-dorada", "san-crisol", "San Crisol", 72), new LocalitySeed("cumbre-dorada", "pozo-negro", "Pozo Negro", 51), new LocalitySeed("cumbre-dorada", "meseta-seca", "Meseta Seca", 29),
        };

        var definitions = new List<LocalityDefinition>(seeds.Length);
        foreach (LocalitySeed seed in seeds)
        {
            string path = ContentRoot + "/" + seed.Id + ".asset";
            LocalityDefinition definition = AssetDatabase.LoadAssetAtPath<LocalityDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<LocalityDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            definition.Configure(seed.Id, seed.JurisdictionId, seed.DisplayName, seed.PopulationWeight);
            EditorUtility.SetDirty(definition);
            definitions.Add(definition);
        }

        return definitions.ToArray();
    }

    private static CampaignContentDefinition CreateCatalog(LocalityDefinition[] localities)
    {
        CampaignContentDefinition catalog = AssetDatabase.LoadAssetAtPath<CampaignContentDefinition>(CatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<CampaignContentDefinition>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }

        catalog.Configure(localities);
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    private static void CreateScene(CampaignContentDefinition contentCatalog)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera();
        CreateEventSystem();

        GameObject canvasRoot = CreateCanvas();
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Image background = CreatePanel(canvasRoot.transform, "Background", new Color(0.07f, 0.14f, 0.18f, 1f));
        Stretch(background.rectTransform, 0f, 0f, 0f, 0f);

        Text brand = CreateText(canvasRoot.transform, "Brand", "POLIYO", font, 32, TextAnchor.MiddleLeft, new Color(0.95f, 0.64f, 0.23f));
        SetRect(brand.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -18f), new Vector2(240f, 42f));
        Text day = CreateText(canvasRoot.transform, "DayLabel", "Dia 1", font, 16, TextAnchor.MiddleRight, Color.white);
        SetRect(day.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-500f, -18f), new Vector2(180f, 32f));
        Text funds = CreateText(canvasRoot.transform, "FundsLabel", "Fondos: $0", font, 16, TextAnchor.MiddleRight, Color.white);
        SetRect(funds.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-305f, -18f), new Vector2(145f, 32f));
        Text fog = CreateText(canvasRoot.transform, "FogLabel", "Informacion abierta", font, 16, TextAnchor.MiddleRight, new Color(0.70f, 0.80f, 0.84f));
        SetRect(fog.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-155f, -18f), new Vector2(145f, 32f));

        Image navigation = CreatePanel(canvasRoot.transform, "Navigation", new Color(0.17f, 0.35f, 0.42f, 1f));
        SetRect(navigation.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -82f), new Vector2(0f, -132f));

        Image workspace = CreatePanel(canvasRoot.transform, "Workspace", new Color(0.14f, 0.28f, 0.34f, 1f));
        SetRect(workspace.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(28f, 94f), new Vector2(-320f, -150f));
        Text screenTitle = CreateText(workspace.transform, "ScreenTitle", "Mesa de campana", font, 28, TextAnchor.UpperLeft, Color.white);
        SetRect(screenTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(22f, -58f), new Vector2(-22f, -18f));
        Text screenDescription = CreateText(workspace.transform, "ScreenDescription", "", font, 18, TextAnchor.UpperLeft, new Color(0.85f, 0.90f, 0.91f));
        SetRect(screenDescription.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(22f, 78f), new Vector2(-22f, -70f));
        Text status = CreateText(workspace.transform, "StatusLabel", "", font, 15, TextAnchor.LowerLeft, new Color(1f, 0.85f, 0.55f));
        SetRect(status.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(22f, 22f), new Vector2(-22f, 58f));

        Image summary = CreatePanel(canvasRoot.transform, "Summary", new Color(0.14f, 0.28f, 0.34f, 1f));
        SetRect(summary.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-292f, 94f), new Vector2(-28f, -150f));
        Text summaryLabel = CreateText(summary.transform, "SummaryLabel", "Resumen nacional", font, 18, TextAnchor.UpperLeft, Color.white);
        SetRect(summaryLabel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -90f), new Vector2(-18f, -18f));
        Text news = CreateText(canvasRoot.transform, "NewsLabel", "Noticias: sin novedades", font, 15, TextAnchor.MiddleLeft, new Color(0.72f, 0.81f, 0.84f));
        SetRect(news.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(28f, 48f), new Vector2(-28f, 78f));
        Text phase = CreateText(canvasRoot.transform, "PhaseLabel", "Planificacion", font, 15, TextAnchor.MiddleLeft, new Color(0.72f, 0.81f, 0.84f));
        SetRect(phase.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 14f), new Vector2(240f, 44f));

        Button calendarButton = CreateButton(navigation.transform, "CalendarButton", "Calendario", font, new Vector2(92f, -25f));
        Button mapButton = CreateButton(navigation.transform, "MapButton", "Mapa", font, new Vector2(205f, -25f));
        Button teamButton = CreateButton(navigation.transform, "TeamButton", "Equipo", font, new Vector2(295f, -25f));
        Button pressButton = CreateButton(navigation.transform, "PressButton", "Prensa", font, new Vector2(1070f, -25f));
        Button rallyButton = CreateButton(canvasRoot.transform, "RallyButton", "Acto", font, new Vector2(300f, -690f));
        Button interviewButton = CreateButton(canvasRoot.transform, "InterviewButton", "Entrevista", font, new Vector2(420f, -690f));
        Button negotiationButton = CreateButton(canvasRoot.transform, "NegotiationButton", "Negociacion", font, new Vector2(555f, -690f));
        Button taskButton = CreateButton(canvasRoot.transform, "TaskButton", "Asignar tarea", font, new Vector2(710f, -690f));
        Button nextDayButton = CreateButton(canvasRoot.transform, "NextDayButton", "Siguiente dia", font, new Vector2(1080f, -750f));

        CampaignCanvasController controller = canvasRoot.AddComponent<CampaignCanvasController>();
        controller.Configure(day, funds, fog, phase, screenTitle, screenDescription, status, summaryLabel, news, nextDayButton);
        UnityEventTools.AddPersistentListener(calendarButton.onClick, controller.ShowCalendar);
        UnityEventTools.AddPersistentListener(mapButton.onClick, controller.ShowMap);
        UnityEventTools.AddPersistentListener(teamButton.onClick, controller.ShowTeam);
        UnityEventTools.AddPersistentListener(pressButton.onClick, controller.ShowPress);
        UnityEventTools.AddPersistentListener(rallyButton.onClick, controller.ResolveRally);
        UnityEventTools.AddPersistentListener(interviewButton.onClick, controller.ResolveInterview);
        UnityEventTools.AddPersistentListener(negotiationButton.onClick, controller.ResolveNegotiation);
        UnityEventTools.AddPersistentListener(taskButton.onClick, controller.AssignTerritorialTask);
        UnityEventTools.AddPersistentListener(nextDayButton.onClick, controller.AdvanceDay);

        CampaignSliceBootstrap bootstrap = canvasRoot.AddComponent<CampaignSliceBootstrap>();
        ConfigureBootstrap(bootstrap, contentCatalog, controller);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    private static void CreateCamera()
    {
        var cameraObject = new GameObject("CampaignCamera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.07f, 0.14f, 0.18f, 1f);
        camera.orthographic = true;
    }

    private static void CreateEventSystem()
    {
        var eventSystemObject = new GameObject("PoliyoEventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }

    private static GameObject CreateCanvas()
    {
        var canvasObject = new GameObject("CampaignCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1200f, 800f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        return canvasObject;
    }

    private static Image CreatePanel(Transform parent, string name, Color color)
    {
        var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        Image image = panel.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static Text CreateText(Transform parent, string name, string value, Font font, int size, TextAnchor alignment, Color color)
    {
        var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.text = value;
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, Font font, Vector2 position)
    {
        var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.18f, 0.35f, 0.42f, 1f);
        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.8f, 0.4f, 1f);
        colors.pressedColor = new Color(0.85f, 0.6f, 0.2f, 1f);
        button.colors = colors;
        SetRect(button.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), position, new Vector2(110f, 36f));
        Text text = CreateText(buttonObject.transform, "Label", label, font, 15, TextAnchor.MiddleCenter, Color.white);
        Stretch(text.rectTransform, 0f, 0f, 0f, 0f);
        return button;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect, float left, float bottom, float right, float top)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static void ConfigureBootstrap(CampaignSliceBootstrap bootstrap, CampaignContentDefinition contentCatalog, CampaignCanvasController controller)
    {
        var serializedBootstrap = new SerializedObject(bootstrap);
        serializedBootstrap.FindProperty("_contentCatalog").objectReferenceValue = contentCatalog;
        serializedBootstrap.FindProperty("_canvasController").objectReferenceValue = controller;
        serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(bootstrap);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
        string name = System.IO.Path.GetFileName(path);
        if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name)) throw new InvalidOperationException("Invalid asset folder: " + path);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private readonly struct LocalitySeed
    {
        public LocalitySeed(string jurisdictionId, string id, string displayName, int populationWeight)
        {
            JurisdictionId = jurisdictionId;
            Id = id;
            DisplayName = displayName;
            PopulationWeight = populationWeight;
        }

        public string JurisdictionId { get; }
        public string Id { get; }
        public string DisplayName { get; }
        public int PopulationWeight { get; }
    }
}
}