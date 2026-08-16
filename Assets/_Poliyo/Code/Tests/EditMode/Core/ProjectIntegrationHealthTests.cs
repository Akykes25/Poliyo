using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Poliyo.Presentation;
using Poliyo.Simulation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Poliyo.Core.EditModeTests
{
public sealed class ProjectIntegrationHealthTests
{
    private const string MainMenuScenePath = "Assets/_Poliyo/Scenes/MainMenu.unity";
    private const string CampaignSliceScenePath = "Assets/_Poliyo/Scenes/CampaignSlice.unity";
    private const string CampaignCalendarScenePath = "Assets/_Poliyo/Scenes/CampaignCalendar.unity";
    private const string CampaignMapScenePath = "Assets/_Poliyo/Scenes/CampaignMap.unity";
    private const string TeamScenePath = "Assets/_Poliyo/Scenes/TeamScene.unity";
    private const string TeamSelectionScenePath = "Assets/_Poliyo/Scenes/TeamSelection.unity";
    private const string PoliticalRallyScenePath = "Assets/_Poliyo/Scenes/PoliticalRally.unity";
    private const string InterviewScenePath = "Assets/_Poliyo/Scenes/Interview.unity";
    private const string PoliticalNegotiationScenePath = "Assets/_Poliyo/Scenes/PoliticalNegotiation.unity";
    private const string SampleScenePath = "Assets/_Poliyo/Scenes/Tests/SampleScene.unity";

    private static readonly string[] CanonicalScenePaths =
    {
        MainMenuScenePath,
        TeamSelectionScenePath,
        CampaignSliceScenePath,
        CampaignCalendarScenePath,
        CampaignMapScenePath,
        TeamScenePath,
        PoliticalRallyScenePath,
        InterviewScenePath,
        PoliticalNegotiationScenePath
    };

    private static readonly string[] OrthographicScenePaths =
    {
        MainMenuScenePath,
        TeamSelectionScenePath,
        CampaignSliceScenePath,
        CampaignCalendarScenePath,
        CampaignMapScenePath,
        TeamScenePath,
    };

    private static readonly string[] PerspectiveScenePaths =
    {
        PoliticalRallyScenePath,
        InterviewScenePath,
        PoliticalNegotiationScenePath,
    };

    [Test]
    public void BuildSettings_FirstEnabledScene_IsMainMenu()
    {
        string[] enabledScenePaths = GetEnabledBuildScenePaths();

        Assert.That(enabledScenePaths, Is.Not.Empty, "Build Settings must contain at least one enabled scene.");
        Assert.That(enabledScenePaths[0], Is.EqualTo(MainMenuScenePath));
    }

    [Test]
    public void BuildSettings_SampleScene_IsNotEnabled()
    {
        string[] enabledScenePaths = GetEnabledBuildScenePaths();

        Assert.That(enabledScenePaths, Does.Not.Contain(SampleScenePath));
    }

    [Test]
    public void BuildSettings_EnabledScenes_MatchCanonicalOrder()
    {
        string[] enabledScenePaths = GetEnabledBuildScenePaths();

        Assert.That(enabledScenePaths, Is.EqualTo(CanonicalScenePaths));
    }

    [TestCaseSource(nameof(CanonicalScenePaths))]
    public void CanonicalScenePath_IsImportedSceneAsset(string scenePath)
    {
        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

        Assert.That(sceneAsset, Is.Not.Null, $"Expected an imported scene asset at '{scenePath}'.");
    }

    [TestCaseSource(nameof(CanonicalScenePaths))]
    public void CanonicalScene_WhenOpened_HasNoMissingScripts(string scenePath)
    {
        WithPreviewScene(
            scenePath,
            scene => Assert.That(
                CountMissingScripts(scene),
                Is.Zero,
                $"'{scenePath}' contains missing MonoBehaviour scripts."));
    }

    [TestCaseSource(nameof(CanonicalScenePaths))]
    public void CanonicalScene_WhenOpened_HasOneActiveMainCamera(string scenePath)
    {
        WithPreviewScene(scenePath, scene =>
        {
            Camera camera = GetSingleComponent<Camera>(scene);

            Assert.That(camera.gameObject.scene, Is.EqualTo(scene));
            Assert.That(camera.gameObject.activeInHierarchy, Is.True);
            Assert.That(camera.enabled, Is.True);
            Assert.That(camera.CompareTag("MainCamera"), Is.True);
            Assert.That(camera.targetDisplay, Is.Zero);
            Assert.That(camera.rect, Is.EqualTo(new Rect(0f, 0f, 1f, 1f)));
            Assert.That(camera.GetComponent<AudioListener>(), Is.Not.Null);
            Assert.That(
                camera.GetComponents<Component>().Any(component =>
                    component != null && component.GetType().Name == "UniversalAdditionalCameraData"),
                Is.True,
                $"'{scenePath}' camera must include URP additional camera data.");
        });
    }

    [TestCaseSource(nameof(OrthographicScenePaths))]
    public void TwoDimensionalScene_WhenOpened_UsesOrthographicCamera(string scenePath)
    {
        WithPreviewScene(scenePath, scene => Assert.That(GetSingleComponent<Camera>(scene).orthographic, Is.True));
    }

    [TestCaseSource(nameof(PerspectiveScenePaths))]
    public void DecisionScene_WhenOpened_UsesPerspectiveCamera(string scenePath)
    {
        WithPreviewScene(scenePath, scene => Assert.That(GetSingleComponent<Camera>(scene).orthographic, Is.False));
    }

    [Test]
    public void TeamSelectionScene_WhenOpened_HasRuntimeBootstrapAndHost()
    {
        WithPreviewScene(TeamSelectionScenePath, scene =>
        {
            VerticalSliceSceneBootstrap bootstrap = GetSingleComponent<VerticalSliceSceneBootstrap>(scene);
            TeamSelectionScreenPresenter presenter = GetSingleComponent<TeamSelectionScreenPresenter>(scene);
            CampaignGameSessionHost host = GetSingleComponent<CampaignGameSessionHost>(scene);

            Assert.That(bootstrap.enabled, Is.True);
            Assert.That(presenter.enabled, Is.True);
            Assert.That(host.enabled, Is.True);
            Assert.That(new SerializedObject(bootstrap).FindProperty("_sceneKind").intValue, Is.EqualTo(0));
        });
    }

    [TestCase(PoliticalRallyScenePath, CampaignActivity.Rally)]
    [TestCase(InterviewScenePath, CampaignActivity.Interview)]
    [TestCase(PoliticalNegotiationScenePath, CampaignActivity.Negotiation)]
    public void DecisionScene_WhenOpened_HasActivityPresenterAndRuntimeBootstrap(
        string scenePath,
        CampaignActivity expectedActivity)
    {
        WithPreviewScene(scenePath, scene =>
        {
            VerticalSliceSceneBootstrap bootstrap = GetSingleComponent<VerticalSliceSceneBootstrap>(scene);
            CampaignDecisionScenePresenter presenter = GetSingleComponent<CampaignDecisionScenePresenter>(scene);
            CampaignGameSessionHost host = GetSingleComponent<CampaignGameSessionHost>(scene);

            Assert.That(bootstrap.enabled, Is.True);
            Assert.That(presenter.enabled, Is.True);
            Assert.That(host.enabled, Is.True);
            Assert.That(
                new SerializedObject(presenter).FindProperty("_activity").enumValueIndex,
                Is.EqualTo((int)expectedActivity));
        });
    }

    [TestCaseSource(nameof(CanonicalScenePaths))]
    public void CanonicalScene_WhenOpened_UsesResponsiveCanvasScaling(string scenePath)
    {
        WithPreviewScene(scenePath, scene =>
        {
            Canvas[] rootCanvases = GetRootCanvases(scene);
            Assert.That(rootCanvases, Has.Length.EqualTo(1), $"'{scenePath}' must have exactly one root Canvas.");

            Canvas canvas = rootCanvases[0];
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            Assert.That(canvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
            Assert.That(canvas.GetComponent<GraphicRaycaster>(), Is.Not.Null);
            Assert.That(scaler, Is.Not.Null);
            Assert.That(scaler.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
            Assert.That(scaler.referenceResolution, Is.EqualTo(new Vector2(1920f, 1080f)));
            Assert.That(scaler.screenMatchMode, Is.EqualTo(CanvasScaler.ScreenMatchMode.Expand));
        });
    }

    [TestCase(MainMenuScenePath, "MenuFrame")]
    [TestCase(MainMenuScenePath, "RoscaliaBrief")]
    [TestCase(CampaignSliceScenePath, "CampaignMasthead")]
    [TestCase(CampaignSliceScenePath, "CampaignDesk")]
    [TestCase(CampaignSliceScenePath, "NationalPulse")]
    [TestCase(CampaignSliceScenePath, "Panel_Noticias")]
    [TestCase(CampaignCalendarScenePath, "CampaignChrome")]
    [TestCase(CampaignCalendarScenePath, "CalendarPanel")]
    [TestCase(CampaignCalendarScenePath, "CalendarActions")]
    [TestCase(CampaignCalendarScenePath, "InterviewDrawer")]
    [TestCase(CampaignMapScenePath, "CampaignChrome")]
    [TestCase(CampaignMapScenePath, "RoscaliaMap")]
    [TestCase(CampaignMapScenePath, "ZoneDetailsDrawer")]
    [TestCase(TeamScenePath, "CampaignChrome")]
    [TestCase(TeamScenePath, "TeamRoster")]
    [TestCase(TeamScenePath, "TeamMemberDetail")]
    [TestCase(TeamScenePath, "FinancingPanel")]
    public void ResponsivePanel_WhenOpened_UsesProportionalStretchAnchors(string scenePath, string objectName)
    {
        WithPreviewScene(scenePath, scene =>
        {
            RectTransform rectTransform = GetSingleNamedComponent<RectTransform>(scene, objectName);

            Assert.That(rectTransform.anchorMax.x, Is.GreaterThan(rectTransform.anchorMin.x));
            Assert.That(rectTransform.anchorMax.y, Is.GreaterThan(rectTransform.anchorMin.y));
            Assert.That(rectTransform.anchoredPosition, Is.EqualTo(Vector2.zero));
            Assert.That(rectTransform.sizeDelta, Is.EqualTo(Vector2.zero));
        });
    }

    [TestCaseSource(nameof(CanonicalScenePaths))]
    public void CampaignGameSessionHost_WhenOpened_ExposesValidSerializedNewCampaignStartDay(string scenePath)
    {
        WithPreviewScene(scenePath, scene =>
        {
            CampaignGameSessionHost host = GetSingleComponent<CampaignGameSessionHost>(scene);
            SerializedProperty startDay = new SerializedObject(host).FindProperty("_newCampaignStartDay");
            FieldInfo field = typeof(CampaignGameSessionHost).GetField(
                "_newCampaignStartDay",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(startDay, Is.Not.Null);
            Assert.That(startDay.propertyType, Is.EqualTo(SerializedPropertyType.Integer));
            Assert.That(startDay.intValue, Is.InRange(1, CampaignCalendar.TotalCampaignDays));
            Assert.That(field, Is.Not.Null);
            Assert.That(field.GetCustomAttribute<SerializeField>(), Is.Not.Null);
            UnityEngine.RangeAttribute range = field.GetCustomAttribute<UnityEngine.RangeAttribute>();
            Assert.That(range, Is.Not.Null);
            Assert.That(range.min, Is.EqualTo(1f));
            Assert.That(range.max, Is.EqualTo(CampaignCalendar.TotalCampaignDays));
        });
    }

    [Test]
    public void MainMenu_WhenOpened_UsesPresenterOwnedCampaignActions()
    {
        WithPreviewScene(MainMenuScenePath, scene =>
        {
            MainMenuScreenPresenter presenter = GetSingleComponent<MainMenuScreenPresenter>(scene);
            UiSceneNavigation navigator = GetSingleComponent<UiSceneNavigation>(scene);
            Button continueButton = GetSerializedReference<Button>(presenter, "_continueButton");
            Button newCampaignButton = GetSerializedReference<Button>(presenter, "_newCampaignButton");
            Button loadCampaignButton = GetSerializedReference<Button>(presenter, "_loadButton");
            GetSerializedReference<TMP_Text>(presenter, "_saveStatusLabel");

            Assert.That(new[] { continueButton, newCampaignButton, loadCampaignButton }, Is.Unique);
            AssertPersistentListener(continueButton, presenter, nameof(MainMenuScreenPresenter.LoadAutosave));
            AssertPersistentListener(continueButton, navigator, nameof(UiSceneNavigation.OpenScene), "CampaignSlice");

            AssertPersistentListener(newCampaignButton, presenter, nameof(MainMenuScreenPresenter.StartNewCampaign));
            AssertPersistentListener(newCampaignButton, navigator, nameof(UiSceneNavigation.OpenScene), "CampaignSlice");

            AssertPersistentListener(loadCampaignButton, presenter, nameof(MainMenuScreenPresenter.LoadAutosave));
            AssertPersistentListener(loadCampaignButton, navigator, nameof(UiSceneNavigation.OpenScene), "CampaignSlice");

            Button quitButton = GetSingleButtonWithPersistentListener(scene, navigator, nameof(UiSceneNavigation.QuitGame));
            AssertPersistentListener(quitButton, navigator, nameof(UiSceneNavigation.QuitGame));
        });
    }

    [Test]
    public void CampaignSlice_WhenOpened_UsesOverlayAndSafeContextualPanels()
    {
        WithPreviewScene(CampaignSliceScenePath, scene =>
        {
            Canvas[] rootCanvases = GetRootCanvases(scene);
            Assert.That(rootCanvases, Has.Length.EqualTo(1), "CampaignSlice must have exactly one Canvas on a scene root.");
            Assert.That(
                rootCanvases[0].renderMode,
                Is.EqualTo(RenderMode.ScreenSpaceOverlay),
                "CampaignSlice root Canvas must render in Screen Space - Overlay mode.");
            Assert.That(
                rootCanvases[0].GetComponent<GraphicRaycaster>(),
                Is.Not.Null,
                "CampaignSlice Canvas must include a GraphicRaycaster for UGUI input.");

            CampaignSliceDashboardPresenter presenter = GetSingleComponent<CampaignSliceDashboardPresenter>(scene);
            GameObject fogObject = GetSerializedReference<GameObject>(presenter, "_fogOverlay");
            GameObject pressObject = GetSerializedReference<GameObject>(presenter, "_newsPanel");
            Button pressCloseButton = GetSerializedReference<Button>(presenter, "_pressCloseButton");
            Button nextDayButton = GetSerializedReference<Button>(presenter, "_nextDayButton");
            GetSerializedReference<TMP_Text>(presenter, "_trustLabel");
            GetSerializedReference<TMP_Text>(presenter, "_votingIntentionLabel");
            GetSerializedReference<TMP_Text>(presenter, "_rejectionLabel");
            GetSerializedReference<TMP_Text>(presenter, "_participationLabel");
            Button[] pressToggleButtons = GetComponentsInScene<Button>(scene)
                .Where(button => GetPersistentListenerIndexes(
                    button,
                    presenter,
                    nameof(CampaignSliceDashboardPresenter.TogglePressPanel)).Length == 1)
                .ToArray();
            Assert.That(pressToggleButtons, Has.Length.EqualTo(2));
            Button pressButton = pressToggleButtons.Single(button => button != pressCloseButton);

            Graphic[] fogGraphics = fogObject.GetComponentsInChildren<Graphic>(includeInactive: true);
            Assert.That(fogGraphics, Is.Not.Empty, "Electoral fog must contain a visible UGUI graphic.");
            Assert.That(
                fogGraphics.All(graphic => !graphic.raycastTarget),
                Is.True,
                "Every electoral-fog graphic is informational and must ignore pointer input.");

            Graphic pressGraphic = pressObject.GetComponent<Graphic>();
            Assert.That(pressGraphic, Is.Not.Null, "The press dossier root must contain a UGUI graphic.");
            Assert.That(pressGraphic.raycastTarget, Is.True, "The visible press dossier must prevent click-through in its bounds.");
            Assert.That(fogObject.activeSelf, Is.False, "Electoral fog must start hidden and be driven by campaign state.");
            Assert.That(pressObject.activeSelf, Is.False, "The press dossier must start closed.");
            Assert.That(pressCloseButton.transform.IsChildOf(pressObject.transform), Is.True);

            AssertPersistentListener(pressButton, presenter, nameof(CampaignSliceDashboardPresenter.TogglePressPanel));
            AssertPersistentListener(pressCloseButton, presenter, nameof(CampaignSliceDashboardPresenter.TogglePressPanel));
            AssertPersistentListener(nextDayButton, presenter, nameof(CampaignSliceDashboardPresenter.AdvanceDay));
        });
    }

    [Test]
    public void CampaignCalendar_WhenOpened_UsesStableModalDrawerScriptAndFocusTarget()
    {
        WithPreviewScene(CampaignCalendarScenePath, scene =>
        {
            CalendarInterviewDrawer drawer = GetSingleComponent<CalendarInterviewDrawer>(scene);
            CanvasGroup canvasGroup = GetSerializedReference<CanvasGroup>(drawer, "_drawer");
            Selectable initialSelection = GetSerializedReference<Selectable>(drawer, "_initialSelection");
            MonoScript drawerScript = MonoScript.FromMonoBehaviour(drawer);

            Assert.That(canvasGroup.gameObject, Is.SameAs(drawer.gameObject));
            Assert.That(initialSelection.transform.IsChildOf(drawer.transform), Is.True);
            Assert.That(
                AssetDatabase.GetAssetPath(drawerScript),
                Is.EqualTo("Assets/_Poliyo/Code/UI/CalendarInterviewDrawer.cs"));
        });
    }

    [Test]
    public void TeamScene_WhenOpened_HasEightConfiguredMembersAndThreeTaskActions()
    {
        WithPreviewScene(TeamScenePath, scene =>
        {
            CampaignTeamScreenPresenter presenter = GetSingleComponent<CampaignTeamScreenPresenter>(scene);
            string[] memberIds = GetSerializedStringArray(presenter, "_memberIds");
            Button[] memberButtons = GetSerializedReferenceArray<Button>(presenter, "_memberButtons");
            TMP_Text[] availabilityLabels = GetSerializedReferenceArray<TMP_Text>(presenter, "_availabilityLabels");

            Assert.That(memberIds, Has.Length.EqualTo(8));
            Assert.That(memberIds.All(memberId => !string.IsNullOrWhiteSpace(memberId)), Is.True);
            Assert.That(memberIds, Is.Unique);
            Assert.That(memberButtons, Has.Length.EqualTo(8));
            Assert.That(memberButtons, Is.Unique);
            Assert.That(availabilityLabels, Has.Length.EqualTo(8));
            Assert.That(availabilityLabels, Is.Unique);

            for (var index = 0; index < memberButtons.Length; index++)
            {
                AssertPersistentListener(
                    memberButtons[index],
                    presenter,
                    nameof(CampaignTeamScreenPresenter.SelectMember),
                    memberIds[index]);
            }

            Button territorialCampaign = GetSerializedReference<Button>(presenter, "_territorialCampaignButton");
            Button investigation = GetSerializedReference<Button>(presenter, "_investigationButton");
            Button mediaStatement = GetSerializedReference<Button>(presenter, "_mediaStatementButton");
            GetSerializedReference<TMP_Text>(presenter, "_roleLabel");
            GetSerializedReference<TMP_Text>(presenter, "_assignmentLabel");
            GetSerializedReference<TMP_Text>(presenter, "_statusLabel");

            Assert.That(new[] { territorialCampaign, investigation, mediaStatement }, Is.Unique);

            AssertPersistentListener(territorialCampaign, presenter, nameof(CampaignTeamScreenPresenter.AssignTerritorialCampaign));
            AssertPersistentListener(investigation, presenter, nameof(CampaignTeamScreenPresenter.AssignInvestigation));
            AssertPersistentListener(mediaStatement, presenter, nameof(CampaignTeamScreenPresenter.AssignMediaStatement));
        });
    }

    private static void WithPreviewScene(string scenePath, Action<Scene> assertion)
    {
        SceneSetup[] originalSceneSetup = EditorSceneManager.GetSceneManagerSetup();
        Scene previewScene = default;

        try
        {
            previewScene = EditorSceneManager.OpenPreviewScene(scenePath);

            Assert.That(previewScene.IsValid(), Is.True, $"'{scenePath}' could not be opened as a preview scene.");
            Assert.That(previewScene.isLoaded, Is.True, $"'{scenePath}' preview scene is not loaded.");
            assertion(previewScene);
        }
        finally
        {
            try
            {
                if (previewScene.IsValid() && previewScene.isLoaded)
                {
                    EditorSceneManager.ClosePreviewScene(previewScene);
                }
            }
            finally
            {
                RestoreSceneSetupIfChanged(originalSceneSetup);
            }
        }
    }

    private static T GetSingleComponent<T>(Scene scene) where T : Component
    {
        T[] components = GetComponentsInScene<T>(scene);
        Assert.That(components, Has.Length.EqualTo(1), $"Expected exactly one {typeof(T).Name} in '{scene.path}'.");
        return components[0];
    }

    private static T GetSingleNamedComponent<T>(Scene scene, string objectName) where T : Component
    {
        T[] components = GetComponentsInScene<T>(scene)
            .Where(component => component.gameObject.name == objectName)
            .ToArray();
        Assert.That(
            components,
            Has.Length.EqualTo(1),
            $"Expected exactly one {typeof(T).Name} named '{objectName}' in '{scene.path}'.");
        return components[0];
    }

    private static Button GetSingleButtonWithPersistentListener(
        Scene scene,
        UnityEngine.Object expectedTarget,
        string expectedMethod)
    {
        Button[] buttons = GetComponentsInScene<Button>(scene)
            .Where(button => GetPersistentListenerIndexes(button, expectedTarget, expectedMethod).Length == 1)
            .ToArray();
        Assert.That(
            buttons,
            Has.Length.EqualTo(1),
            $"Expected exactly one Button calling {expectedTarget.GetType().Name}.{expectedMethod} in '{scene.path}'.");
        return buttons[0];
    }

    private static T[] GetComponentsInScene<T>(Scene scene) where T : Component
    {
        return scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<T>(includeInactive: true))
            .ToArray();
    }

    private static void AssertPersistentListener(
        Button button,
        UnityEngine.Object expectedTarget,
        string expectedMethod,
        string expectedStringArgument = null)
    {
        int[] listenerIndexes = GetPersistentListenerIndexes(button, expectedTarget, expectedMethod);

        Assert.That(
            listenerIndexes,
            Has.Length.EqualTo(1),
            $"'{button.name}' must have exactly one listener for {expectedTarget.GetType().Name}.{expectedMethod}.");

        int listenerIndex = listenerIndexes[0];
        Assert.That(
            button.onClick.GetPersistentListenerState(listenerIndex),
            Is.Not.EqualTo(UnityEventCallState.Off),
            $"'{button.name}' listener for {expectedMethod} must be enabled.");

        if (expectedStringArgument != null)
        {
            Assert.That(
                GetPersistentStringArgument(button, listenerIndex),
                Is.EqualTo(expectedStringArgument),
                $"'{button.name}' must pass '{expectedStringArgument}' to {expectedMethod}.");
        }
    }

    private static int[] GetPersistentListenerIndexes(
        Button button,
        UnityEngine.Object expectedTarget,
        string expectedMethod)
    {
        return Enumerable.Range(0, button.onClick.GetPersistentEventCount())
            .Where(index =>
                button.onClick.GetPersistentTarget(index) == expectedTarget &&
                button.onClick.GetPersistentMethodName(index) == expectedMethod)
            .ToArray();
    }

    private static string GetPersistentStringArgument(Button button, int listenerIndex)
    {
        SerializedProperty calls = new SerializedObject(button).FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        Assert.That(calls, Is.Not.Null, "Unity Button persistent-call data could not be inspected.");
        Assert.That(listenerIndex, Is.InRange(0, calls.arraySize - 1));

        SerializedProperty arguments = calls
            .GetArrayElementAtIndex(listenerIndex)
            .FindPropertyRelative("m_Arguments");
        SerializedProperty stringArgument = arguments?.FindPropertyRelative("m_StringArgument");
        Assert.That(stringArgument, Is.Not.Null, "Unity Button string-argument data could not be inspected.");
        return stringArgument.stringValue;
    }

    private static T GetSerializedReference<T>(
        UnityEngine.Object owner,
        string propertyName)
        where T : UnityEngine.Object
    {
        SerializedProperty property = new SerializedObject(owner).FindProperty(propertyName);
        Assert.That(property, Is.Not.Null, $"'{owner.GetType().Name}.{propertyName}' is not serialized.");
        Assert.That(
            property.objectReferenceValue,
            Is.InstanceOf<T>(),
            $"'{owner.GetType().Name}.{propertyName}' must reference a {typeof(T).Name}.");
        return (T)property.objectReferenceValue;
    }

    private static T[] GetSerializedReferenceArray<T>(
        UnityEngine.Object owner,
        string propertyName)
        where T : UnityEngine.Object
    {
        SerializedProperty property = new SerializedObject(owner).FindProperty(propertyName);
        Assert.That(property, Is.Not.Null, $"'{owner.GetType().Name}.{propertyName}' is not serialized.");
        Assert.That(property.isArray, Is.True, $"'{owner.GetType().Name}.{propertyName}' must be an array.");

        var references = new T[property.arraySize];
        for (var index = 0; index < references.Length; index++)
        {
            UnityEngine.Object reference = property.GetArrayElementAtIndex(index).objectReferenceValue;
            Assert.That(reference, Is.InstanceOf<T>(), $"'{propertyName}[{index}]' must reference a {typeof(T).Name}.");
            references[index] = (T)reference;
        }

        return references;
    }

    private static string[] GetSerializedStringArray(UnityEngine.Object owner, string propertyName)
    {
        SerializedProperty property = new SerializedObject(owner).FindProperty(propertyName);
        Assert.That(property, Is.Not.Null, $"'{owner.GetType().Name}.{propertyName}' is not serialized.");
        Assert.That(property.isArray, Is.True, $"'{owner.GetType().Name}.{propertyName}' must be an array.");

        var values = new string[property.arraySize];
        for (var index = 0; index < values.Length; index++)
        {
            values[index] = property.GetArrayElementAtIndex(index).stringValue;
        }

        return values;
    }

    private static string[] GetEnabledBuildScenePaths()
    {
        return EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
    }

    private static int CountMissingScripts(Scene scene)
    {
        int missingScriptCount = 0;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                missingScriptCount += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
            }
        }

        return missingScriptCount;
    }

    private static Canvas[] GetRootCanvases(Scene scene)
    {
        var rootCanvases = new List<Canvas>();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.TryGetComponent(out Canvas canvas))
            {
                rootCanvases.Add(canvas);
            }
        }

        return rootCanvases.ToArray();
    }

    private static void RestoreSceneSetupIfChanged(SceneSetup[] originalSceneSetup)
    {
        SceneSetup[] currentSceneSetup = EditorSceneManager.GetSceneManagerSetup();
        if (!SceneSetupsMatch(currentSceneSetup, originalSceneSetup))
        {
            EditorSceneManager.RestoreSceneManagerSetup(originalSceneSetup);
        }
    }

    private static bool SceneSetupsMatch(IReadOnlyList<SceneSetup> left, IReadOnlyList<SceneSetup> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        for (int index = 0; index < left.Count; index++)
        {
            if (left[index].path != right[index].path ||
                left[index].isLoaded != right[index].isLoaded ||
                left[index].isActive != right[index].isActive)
            {
                return false;
            }
        }

        return true;
    }
}
}
