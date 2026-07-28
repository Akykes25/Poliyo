using System;
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
/// <summary>Wires the authored campaign dashboard and rebuilds the team screen without changing CampaignSlice visuals.</summary>
public static class CampaignFlowAndTeamSceneBuilder
{
    private const string ContentCatalogPath = "Assets/_Poliyo/Content/CampaignCatalog.asset";
    private const string MainMenuScenePath = "Assets/_Poliyo/Scenes/MainMenu.unity";
    private const string CampaignSliceScenePath = "Assets/_Poliyo/Scenes/CampaignSlice.unity";
    private const string TeamScenePath = "Assets/_Poliyo/Scenes/TeamScene.unity";

    [MenuItem("Poliyo/UI/Apply Campaign Flow and Rebuild Team")]
    public static void Apply()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        CampaignContentDefinition catalog = LoadCatalog();
        ConfigureMainMenu(catalog);
        ConfigureCampaignSlice(catalog);
        RebuildTeamScene(catalog);
        EnsureBuildScenes();
        AssetDatabase.SaveAssets();
    }

    private static void ConfigureMainMenu(CampaignContentDefinition catalog)
    {
        Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
        CampaignGameSessionHost host = FindOrCreateSessionHost(catalog);
        UiSceneNavigation navigator = FindRequiredComponent<UiSceneNavigation>(scene, "MenuFrame");
        Button newCampaign = FindRequiredComponent<Button>(scene, "NewCampaignButton");
        Button loadCampaign = FindRequiredComponent<Button>(scene, "LoadCampaignButton");

        Reset(newCampaign);
        Reset(loadCampaign);
        UnityEventTools.AddPersistentListener(newCampaign.onClick, host.StartNewCampaign);
        UnityEventTools.AddStringPersistentListener(newCampaign.onClick, navigator.OpenScene, "CampaignSlice");
        UnityEventTools.AddPersistentListener(loadCampaign.onClick, host.LoadAutosave);
        UnityEventTools.AddStringPersistentListener(loadCampaign.onClick, navigator.OpenScene, "CampaignSlice");
        Save(scene, MainMenuScenePath);
    }

    private static void ConfigureCampaignSlice(CampaignContentDefinition catalog)
    {
        Scene scene = EditorSceneManager.OpenScene(CampaignSliceScenePath, OpenSceneMode.Single);
        CampaignGameSessionHost host = FindOrCreateSessionHost(catalog);
        GameObject canvas = FindRequiredGameObject(scene, "CampaignCanvas");

        RemoveComponent<CampaignSliceBootstrap>(canvas);
        RemoveComponent<CampaignCanvasController>(canvas);
        RemoveComponent<CampaignSliceDashboardPresenter>(canvas);

        UiSceneNavigation navigator = canvas.GetComponent<UiSceneNavigation>() ?? canvas.AddComponent<UiSceneNavigation>();
        var dashboard = canvas.AddComponent<CampaignSliceDashboardPresenter>();
        dashboard.Configure(
            FindRequiredComponent<TMP_Text>(scene, "Dia_TXT"),
            FindRequiredComponent<TMP_Text>(scene, "Presupuesto_TXT"),
            FindRequiredGameObject(scene, "Niebla_Electoral"),
            FindRequiredComponent<TMP_Text>(scene, "Confianza_TXT"),
            FindRequiredComponent<TMP_Text>(scene, "Intencion_Voto_TXT"),
            FindRequiredGameObject(scene, "Panel_Noticias"),
            FindRequiredComponent<Button>(scene, "Siguiente_Dia_BTN"));

        Button calendar = FindRequiredComponent<Button>(scene, "Calendario_Btn");
        Button map = FindRequiredComponent<Button>(scene, "Mapa_Btn");
        Button team = FindRequiredComponent<Button>(scene, "Equipo_Btn");
        Button press = FindRequiredComponent<Button>(scene, "Prensa_Btn");
        Button nextDay = FindRequiredComponent<Button>(scene, "Siguiente_Dia_BTN");
        Reset(calendar);
        Reset(map);
        Reset(team);
        Reset(press);
        Reset(nextDay);
        UnityEventTools.AddStringPersistentListener(calendar.onClick, navigator.OpenScene, "CampaignCalendar");
        UnityEventTools.AddStringPersistentListener(map.onClick, navigator.OpenScene, "CampaignMap");
        UnityEventTools.AddStringPersistentListener(team.onClick, navigator.OpenScene, "TeamScene");
        UnityEventTools.AddPersistentListener(press.onClick, dashboard.TogglePressPanel);
        UnityEventTools.AddPersistentListener(nextDay.onClick, dashboard.AdvanceDay);

        EditorSceneManager.MarkSceneDirty(scene);
        Save(scene, CampaignSliceScenePath);
    }

    private static void RebuildTeamScene(CampaignContentDefinition catalog)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        FindOrCreateSessionHost(catalog);
        GameObject canvas = CreateCanvas();
        CreateImage(canvas.transform, "Background", new Color(0.04f, 0.07f, 0.12f, 1f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        UiSceneNavigation navigator = canvas.AddComponent<UiSceneNavigation>();

        CreateText(canvas.transform, "Title", "EQUIPO DE CAMPAÑA", 48, new Color(1f, 0.63f, 0.16f, 1f), new Vector2(84f, -74f), new Vector2(720f, 72f), TextAlignmentOptions.Left);
        CreateText(canvas.transform, "Subtitle", "Seleccioná a quién asignar la próxima tarea.", 22, new Color(0.68f, 0.77f, 0.81f, 1f), new Vector2(88f, -144f), new Vector2(760f, 42f), TextAlignmentOptions.Left);
        Button back = CreateButton(canvas.transform, "BackToCampaignButton", "Volver a campaña", new Vector2(1510f, -72f), new Vector2(300f, 64f));
        UnityEventTools.AddStringPersistentListener(back.onClick, navigator.OpenScene, "CampaignSlice");

        string[] names = { "Vicepresidencia", "Jefe de campaña", "Jefe de prensa", "Vocería", "Coordinación territorial", "Legal y contable", "Consultoría política", "Operaciones" };
        for (var index = 0; index < names.Length; index++)
        {
            int column = index % 4;
            int row = index / 4;
            Vector2 position = new Vector2(92f + (column * 438f), -270f - (row * 300f));
            Image card = CreateImage(canvas.transform, "TeamCard_" + index, new Color(0.08f, 0.16f, 0.24f, 1f), Vector2.zero, Vector2.zero, position, new Vector2(386f, 242f));
            CreateText(card.transform, "Role", names[index], 25, Color.white, new Vector2(28f, -34f), new Vector2(330f, 72f), TextAlignmentOptions.Left);
            CreateText(card.transform, "Availability", "Disponible", 18, new Color(0.55f, 0.88f, 0.66f, 1f), new Vector2(28f, -112f), new Vector2(260f, 38f), TextAlignmentOptions.Left);
            CreateText(card.transform, "Description", "Rasgos y tareas se conectarán a esta tarjeta en el próximo paso.", 17, new Color(0.68f, 0.77f, 0.81f, 1f), new Vector2(28f, -156f), new Vector2(320f, 62f), TextAlignmentOptions.TopLeft);
        }

        Save(scene, TeamScenePath);
    }

    private static CampaignContentDefinition LoadCatalog()
    {
        CampaignContentDefinition catalog = AssetDatabase.LoadAssetAtPath<CampaignContentDefinition>(ContentCatalogPath);
        if (catalog == null)
        {
            throw new InvalidOperationException("Campaign catalog is missing.");
        }

        return catalog;
    }

    private static CampaignGameSessionHost FindOrCreateSessionHost(CampaignContentDefinition catalog)
    {
        GameObject hostObject = GameObject.Find("CampaignGameSessionHost");
        if (hostObject == null)
        {
            hostObject = new GameObject("CampaignGameSessionHost");
        }

        CampaignGameSessionHost host = hostObject.GetComponent<CampaignGameSessionHost>() ?? hostObject.AddComponent<CampaignGameSessionHost>();
        host.Configure(catalog, 20260725UL, 1200f);
        return host;
    }

    private static void EnsureBuildScenes()
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (string path in new[] { MainMenuScenePath, CampaignSliceScenePath, TeamScenePath, "Assets/_Poliyo/Scenes/CampaignCalendar.unity", "Assets/_Poliyo/Scenes/CampaignMap.unity" })
        {
            if (!scenes.Exists(scene => scene.path == path))
            {
                scenes.Add(new EditorBuildSettingsScene(path, true));
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static GameObject CreateCanvas()
    {
        var canvas = new GameObject("TeamCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        return canvas;
    }

    private static Image CreateImage(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        var gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        gameObject.transform.SetParent(parent, false);
        Image image = gameObject.GetComponent<Image>();
        image.color = color;
        RectTransform rect = image.rectTransform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0f, 1f);
        if (anchorMax == Vector2.one)
        {
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        else
        {
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        return image;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, float size, Color color, Vector2 position, Vector2 dimensions, TextAlignmentOptions alignment)
    {
        var gameObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        gameObject.transform.SetParent(parent, false);
        TMP_Text text = gameObject.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size)
    {
        Image image = CreateImage(parent, name, new Color(0.12f, 0.25f, 0.33f, 1f), Vector2.zero, Vector2.zero, position, size);
        Button button = image.gameObject.AddComponent<Button>();
        TMP_Text text = CreateText(image.transform, "Label", label, 20, Color.white, Vector2.zero, size, TextAlignmentOptions.Center);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        return button;
    }

    private static void Reset(Button button)
    {
        while (button.onClick.GetPersistentEventCount() > 0)
        {
            UnityEventTools.RemovePersistentListener(button.onClick, 0);
        }
    }

    private static void RemoveComponent<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component != null)
        {
            UnityEngine.Object.DestroyImmediate(component);
        }
    }

    private static GameObject FindRequiredGameObject(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform result = FindDescendant(root.transform, name);
            if (result != null)
            {
                return result.gameObject;
            }
        }

        throw new InvalidOperationException("Missing required UI object: " + name);
    }

    private static T FindRequiredComponent<T>(Scene scene, string name) where T : Component
    {
        GameObject gameObject = FindRequiredGameObject(scene, name);
        T component = gameObject.GetComponent<T>() ?? gameObject.GetComponentInChildren<T>(true);
        if (component == null)
        {
            throw new InvalidOperationException("UI object is missing required component: " + name);
        }

        return component;
    }

    private static Transform FindDescendant(Transform current, string name)
    {
        if (current.name == name)
        {
            return current;
        }

        for (var index = 0; index < current.childCount; index++)
        {
            Transform result = FindDescendant(current.GetChild(index), name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static void Save(Scene scene, string path)
    {
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, path);
    }
}
}
