using System;
using Poliyo.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Poliyo.Presentation
{
/// <summary>
/// Runtime safety net for the four vertical-slice scenes. The editor builder authors the
/// same hierarchy for designers, while this component guarantees that a scene checked out
/// from source still has an actionable UGUI surface when opened in Play Mode.
/// </summary>
[DefaultExecutionOrder(-1000)]
public sealed class VerticalSliceSceneBootstrap : MonoBehaviour
{
    public enum SliceSceneKind
    {
        TeamSelection,
        PoliticalRally,
        Interview,
        PoliticalNegotiation,
    }

    [SerializeField] private SliceSceneKind _sceneKind = SliceSceneKind.TeamSelection;

    private static Sprite _whiteSprite;
    private static TMP_FontAsset _font;

    /// <summary>Sets the scene contract when the editor builder creates a new scene.</summary>
    public void Configure(SliceSceneKind sceneKind)
    {
        _sceneKind = sceneKind;
    }

    private void Start()
    {
        EnsureEventSystem();
        switch (_sceneKind)
        {
            case SliceSceneKind.TeamSelection:
                BuildTeamSelection();
                break;
            case SliceSceneKind.PoliticalRally:
                BuildDecision(CampaignActivity.Rally);
                break;
            case SliceSceneKind.Interview:
                BuildDecision(CampaignActivity.Interview);
                break;
            case SliceSceneKind.PoliticalNegotiation:
                BuildDecision(CampaignActivity.Negotiation);
                break;
        }
    }

    private void BuildTeamSelection()
    {
        TeamSelectionScreenPresenter presenter = GetComponent<TeamSelectionScreenPresenter>() ?? gameObject.AddComponent<TeamSelectionScreenPresenter>();
        UiSceneNavigation navigator = GetComponent<UiSceneNavigation>() ?? gameObject.AddComponent<UiSceneNavigation>();
        // The editor builder uses the designer-facing name while the runtime
        // safety net uses its own prefix. Treat either hierarchy as authored so
        // opening a scene after the builder has run never creates a duplicate UI.
        if (transform.Find("VerticalSliceTeamWorkspace") != null || transform.Find("TeamSelectionWorkspace") != null) return;

        RectTransform workspace = CreatePanel("VerticalSliceTeamWorkspace", transform, new Color(0.035f, 0.067f, 0.145f, 0.98f));
        Stretch(workspace, 0.025f, 0.035f, 0.025f, 0.035f);
        CreateText("Brand", workspace, "POLIYO", 31f, new Vector2(36f, -26f), new Vector2(180f, 42f), new Color(1f, 0.78f, 0.18f, 1f), FontStyles.Bold);
        TMP_Text title = CreateText("SceneTitle", workspace, "Elegí tu equipo", 30f, new Vector2(224f, -28f), new Vector2(650f, 44f), Color.white, FontStyles.Bold);
        TMP_Text progress = CreateText("ProgressLabel", workspace, "0/8 roles confirmados", 17f, new Vector2(-310f, -34f), new Vector2(270f, 30f), new Color(0.2f, 0.9f, 0.85f, 1f), FontStyles.Bold, TextAlignmentOptions.Right);
        CreateText("SceneDeck", workspace, "Ocho roles · tres personas por puesto · señales públicas, costos humanos ocultos", 16f, new Vector2(226f, -72f), new Vector2(900f, 28f), new Color(0.74f, 0.8f, 0.89f, 1f));

        RectTransform rolePanel = CreatePanel("RolePanel", workspace, new Color(0.055f, 0.11f, 0.22f, 1f));
        SetRect(rolePanel, new Vector2(0f, 0f), new Vector2(0.31f, 1f), new Vector2(24f, 88f), new Vector2(-16f, -24f));
        RectTransform candidatePanel = CreatePanel("CandidatePanel", workspace, new Color(0.94f, 0.95f, 0.98f, 1f));
        SetRect(candidatePanel, new Vector2(0.325f, 0f), new Vector2(1f, 1f), new Vector2(18f, 88f), new Vector2(-24f, -24f));
        CreateText("RoleHeading", rolePanel, "PUESTOS DE LA MESA", 14f, new Vector2(24f, -22f), new Vector2(340f, 24f), new Color(0.2f, 0.9f, 0.85f, 1f), FontStyles.Bold);
        CreateText("CandidateHeading", candidatePanel, "PERSONAS, NO PLANILLAS", 14f, new Vector2(30f, -22f), new Vector2(420f, 24f), new Color(0.08f, 0.12f, 0.2f, 1f), FontStyles.Bold);

        string[] roles =
        {
            CampaignTeamRoleIds.VicePresident,
            CampaignTeamRoleIds.CampaignChief,
            CampaignTeamRoleIds.PressChief,
            CampaignTeamRoleIds.Spokesperson,
            CampaignTeamRoleIds.TerritorialCoordinator,
            CampaignTeamRoleIds.LegalAndAccounting,
            CampaignTeamRoleIds.PoliticalConsultant,
            CampaignTeamRoleIds.OperationsChief,
        };
        var roleButtons = new Button[roles.Length];
        for (var index = 0; index < roles.Length; index++)
        {
            string roleId = roles[index];
            roleButtons[index] = CreateButton("RoleButton_" + index, rolePanel, TeamSelectionScreenPresenter.GetRoleName(roleId), new Vector2(20f, -62f - index * 58f), new Vector2(430f, 48f), new Color(0.1f, 0.19f, 0.34f, 1f));
            roleButtons[index].onClick.AddListener(() => presenter.SelectRole(roleId));
        }

        var candidateButtons = new Button[3];
        for (var index = 0; index < candidateButtons.Length; index++)
        {
            int candidateIndex = index;
            candidateButtons[index] = CreateButton("CandidateButton_" + index, candidatePanel, "Perfil " + (index + 1), new Vector2(30f + index * 288f, -62f), new Vector2(266f, 84f), new Color(0.1f, 0.17f, 0.28f, 1f));
            candidateButtons[index].onClick.AddListener(() => presenter.SelectCandidateAtIndex(candidateIndex));
        }

        RectTransform dossier = CreatePanel("CandidateDossier", candidatePanel, new Color(0.035f, 0.067f, 0.145f, 1f));
        SetRect(dossier, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(30f, 126f), new Vector2(-30f, -214f));
        CreateText("InformationChip", dossier, "INFORMACIÓN PARCIAL", 13f, new Vector2(26f, -24f), new Vector2(250f, 28f), new Color(1f, 0.78f, 0.18f, 1f), FontStyles.Bold);
        TMP_Text candidateName = CreateText("CandidateName", dossier, "Elegí una persona, no una estadística", 31f, new Vector2(26f, -68f), new Vector2(980f, 46f), Color.white, FontStyles.Bold);
        TMP_Text candidateRole = CreateText("CandidateRole", dossier, "Rol por seleccionar", 18f, new Vector2(28f, -116f), new Vector2(500f, 28f), new Color(0.2f, 0.9f, 0.85f, 1f), FontStyles.Bold);
        TMP_Text candidateTrajectory = CreateText("CandidateTrajectory", dossier, "Tres perfiles disponibles. La campaña sólo conoce sus señales públicas.", 18f, new Vector2(28f, -160f), new Vector2(1000f, 60f), new Color(0.86f, 0.88f, 0.93f, 1f));
        TMP_Text candidateIdeology = CreateText("CandidateIdeology", dossier, "Ideología declarada: pendiente", 16f, new Vector2(28f, -228f), new Vector2(480f, 44f), new Color(1f, 0.78f, 0.18f, 1f));
        TMP_Text candidateExperience = CreateText("CandidateExperience", dossier, "Experiencia pública: pendiente", 16f, new Vector2(520f, -228f), new Vector2(480f, 44f), Color.white);
        TMP_Text candidateRelationships = CreateText("CandidateRelationships", dossier, "Relaciones públicas: pendiente", 16f, new Vector2(28f, -282f), new Vector2(480f, 60f), new Color(0.75f, 0.82f, 0.91f, 1f));
        TMP_Text candidateClue = CreateText("CandidateClue", dossier, "Pista narrativa: compará trayectoria, tono y antecedentes antes de confirmar.", 16f, new Vector2(520f, -282f), new Vector2(480f, 60f), new Color(0.2f, 0.9f, 0.85f, 1f));
        TMP_Text roleLabel = CreateText("RoleLabel", candidatePanel, "Rol por seleccionar", 16f, new Vector2(30f, 98f), new Vector2(500f, 26f), new Color(0.08f, 0.12f, 0.2f, 1f), FontStyles.Bold);
        TMP_Text status = CreateText("StatusLabel", candidatePanel, "Elegí un rol y compará tres señales públicas.", 15f, new Vector2(30f, 56f), new Vector2(700f, 28f), new Color(0.28f, 0.34f, 0.44f, 1f));
        Button confirm = CreateButton("ConfirmTeamButton", candidatePanel, "Confirmar equipo y comenzar", new Vector2(-420f, 26f), new Vector2(360f, 54f), new Color(0.95f, 0.37f, 0.36f, 1f));
        presenter.Configure(roles, roleButtons, candidateButtons, roleLabel, progress, candidateName, candidateRole, candidateTrajectory, candidateIdeology, candidateExperience, candidateRelationships, candidateClue, status, confirm, navigator);
    }

    private void BuildDecision(CampaignActivity activity)
    {
        CampaignDecisionScenePresenter presenter = GetComponent<CampaignDecisionScenePresenter>() ?? gameObject.AddComponent<CampaignDecisionScenePresenter>();
        UiSceneNavigation navigator = GetComponent<UiSceneNavigation>() ?? gameObject.AddComponent<UiSceneNavigation>();
        // See the team-selection guard above: both names are valid because the
        // source scene may be hand-authored or generated by the editor builder.
        if (transform.Find("VerticalSliceDecisionWorkspace") != null || transform.Find("DecisionWorkspace") != null) return;

        CreateDecisionBackdrop(activity);
        RectTransform workspace = CreatePanel("VerticalSliceDecisionWorkspace", transform, new Color(0.035f, 0.067f, 0.145f, 0.96f));
        Stretch(workspace, 0.035f, 0.05f, 0.035f, 0.05f);
        Color accent = activity == CampaignActivity.Rally ? new Color(0.95f, 0.37f, 0.36f, 1f) : activity == CampaignActivity.Interview ? new Color(0.25f, 0.55f, 1f, 1f) : new Color(0.66f, 0.42f, 0.95f, 1f);
        TMP_Text sceneTitle = CreateText("SceneTitle", workspace, GetActivityTitle(activity), 30f, new Vector2(34f, -28f), new Vector2(700f, 42f), Color.white, FontStyles.Bold);
        TMP_Text stage = CreateText("StageLabel", workspace, "DECISIÓN 1/3", 15f, new Vector2(-360f, -32f), new Vector2(220f, 28f), new Color(0.2f, 0.9f, 0.85f, 1f), FontStyles.Bold, TextAlignmentOptions.Right);
        TMP_Text timer = CreateText("TimerLabel", workspace, "TIEMPO · 45 s", 15f, new Vector2(-112f, -32f), new Vector2(190f, 28f), new Color(1f, 0.78f, 0.18f, 1f), FontStyles.Bold, TextAlignmentOptions.Right);
        RectTransform context = CreatePanel("ContextPanel", workspace, new Color(0.055f, 0.11f, 0.22f, 1f));
        SetRect(context, new Vector2(0f, 0f), new Vector2(0.45f, 1f), new Vector2(24f, 84f), new Vector2(-22f, -118f));
        TMP_Text actor = CreateText("ActorLabel", context, "CONTRAPARTE", 15f, new Vector2(24f, -26f), new Vector2(560f, 26f), accent, FontStyles.Bold);
        TMP_Text location = CreateText("LocationLabel", context, "ALCANCE", 15f, new Vector2(24f, -60f), new Vector2(560f, 26f), new Color(1f, 0.78f, 0.18f, 1f), FontStyles.Bold);
        TMP_Text contextLabel = CreateText("ContextLabel", context, "La campaña reúne señales parciales.", 18f, new Vector2(24f, -104f), new Vector2(560f, 280f), Color.white);
        CreateText("KnownSignal", context, "La información disponible no es una respuesta correcta.", 16f, new Vector2(24f, -390f), new Vector2(560f, 90f), new Color(0.74f, 0.8f, 0.89f, 1f));
        RectTransform decision = CreatePanel("DecisionPanel", workspace, new Color(0.94f, 0.95f, 0.98f, 1f));
        SetRect(decision, new Vector2(0.48f, 0f), new Vector2(1f, 1f), new Vector2(18f, 84f), new Vector2(-22f, -118f));
        TMP_Text prompt = CreateText("PromptLabel", decision, "Leé la situación y respondé.", 22f, new Vector2(26f, -28f), new Vector2(840f, 80f), new Color(0.08f, 0.12f, 0.2f, 1f), FontStyles.Bold);
        TMP_Text cost = CreateText("CostLabel", decision, "COSTO PREVISTO", 15f, new Vector2(26f, -118f), new Vector2(420f, 28f), new Color(0.95f, 0.37f, 0.36f, 1f), FontStyles.Bold);
        TMP_Text risk = CreateText("RiskLabel", decision, "Elegí una opción para conocer su señal de riesgo.", 16f, new Vector2(26f, -154f), new Vector2(840f, 52f), new Color(0.28f, 0.34f, 0.44f, 1f));
        var responses = new Button[3];
        for (var index = 0; index < responses.Length; index++)
        {
            int optionIndex = index;
            responses[index] = CreateButton("ResponseButton_" + index, decision, "Respuesta " + (index + 1), new Vector2(26f, -226f - index * 68f), new Vector2(840f, 54f), new Color(0.1f, 0.17f, 0.28f, 1f));
            responses[index].onClick.AddListener(() => presenter.SelectResponseAtIndex(optionIndex));
        }

        TMP_Text reaction = CreateText("ReactionLabel", workspace, "La reacción se muestra después de confirmar.", 15f, new Vector2(34f, 50f), new Vector2(780f, 38f), new Color(0.8f, 0.84f, 0.91f, 1f));
        TMP_Text status = CreateText("StatusLabel", workspace, "Elegí una respuesta para continuar.", 15f, new Vector2(-720f, 50f), new Vector2(640f, 38f), new Color(1f, 0.78f, 0.18f, 1f), FontStyles.Bold, TextAlignmentOptions.Right);
        var variants = new Button[5];
        for (var index = 0; index < variants.Length; index++)
        {
            int variantIndex = index;
            variants[index] = CreateButton("VariantButton_" + index, workspace, "Escenario " + (index + 1), new Vector2(34f + index * 190f, -78f), new Vector2(174f, 38f), new Color(0.1f, 0.19f, 0.34f, 1f));
            variants[index].onClick.AddListener(() => presenter.SelectVariantAtIndex(variantIndex));
        }

        TMP_Text timerMode = CreateText("TimerModeLabel", workspace, "TIEMPO: normal", 14f, new Vector2(-360f, 14f), new Vector2(220f, 24f), new Color(0.2f, 0.9f, 0.85f, 1f), FontStyles.Bold, TextAlignmentOptions.Right);
        Button timerModeButton = CreateButton("TimerModeButton", workspace, "Accesibilidad", new Vector2(-126f, 12f), new Vector2(180f, 38f), new Color(0.1f, 0.19f, 0.34f, 1f));
        Button confirm = CreateButton("ConfirmDecisionButton", workspace, "Confirmar respuesta", new Vector2(-390f, 10f), new Vector2(250f, 52f), new Color(0.95f, 0.37f, 0.36f, 1f));
        Button back = CreateButton("ReturnToCalendarButton", workspace, "Volver al calendario", new Vector2(-670f, 10f), new Vector2(250f, 52f), new Color(0.1f, 0.19f, 0.34f, 1f));
        confirm.onClick.AddListener(presenter.ConfirmDecision);
        timerModeButton.onClick.AddListener(presenter.CycleTimerMode);
        back.onClick.AddListener(presenter.ReturnToCalendar);
        presenter.Configure(activity, sceneTitle, contextLabel, actor, location, stage, prompt, timer, timerMode, cost, risk, reaction, status, variants, responses, confirm, timerModeButton, back, navigator);
    }

    private void CreateDecisionBackdrop(CampaignActivity activity)
    {
        if (GameObject.Find("VerticalSliceDecisionFloor") != null) return;
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "VerticalSliceDecisionFloor";
        floor.transform.position = new Vector3(0f, -1.5f, 8f);
        floor.transform.localScale = new Vector3(4f, 1f, 4f);
        floor.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.055f, 0.11f, 0.22f, 1f));
        GameObject focal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        focal.name = "VerticalSliceDecisionFocalBlockout";
        focal.transform.position = new Vector3(0f, activity == CampaignActivity.Rally ? 0.4f : 0f, 9f);
        focal.transform.localScale = activity == CampaignActivity.Rally ? new Vector3(7f, 1.2f, 0.6f) : new Vector3(4.5f, 0.6f, 2.3f);
        focal.GetComponent<Renderer>().sharedMaterial = CreateMaterial(activity == CampaignActivity.Rally ? new Color(0.95f, 0.37f, 0.36f, 1f) : new Color(0.25f, 0.55f, 1f, 1f));
        var lightObject = new GameObject("VerticalSliceDecisionKeyLight");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightObject.transform.rotation = Quaternion.Euler(38f, -25f, 0f);
    }

    private static Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
        if (shader == null) return null;
        var material = new Material(shader) { color = color };
        return material;
    }

    private void EnsureEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            eventSystem = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
        }

        UnityEngine.InputSystem.UI.InputSystemUIInputModule inputModule = eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        if (inputModule == null)
        {
            inputModule = eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // A component created from a hand-authored scene has no serialized action
        // asset. The Input System module supplies its safe keyboard/gamepad defaults
        // on enable, but assigning them here also covers a disabled EventSystem that
        // becomes active after the scene bootstrap.
        if (inputModule.actionsAsset == null)
        {
            inputModule.AssignDefaultActions();
        }
    }

    private RectTransform CreatePanel(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.sprite = WhiteSprite;
        image.color = color;
        return go.GetComponent<RectTransform>();
    }

    private Button CreateButton(string name, Transform parent, string label, Vector2 position, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = go.GetComponent<Image>();
        image.sprite = WhiteSprite;
        image.color = color;
        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.15f);
        button.colors = colors;
        TMP_Text text = CreateText("Label", go.transform, label, 16f, new Vector2(14f, -8f), new Vector2(size.x - 28f, size.y - 16f), Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
        text.raycastTarget = false;
        return button;
    }

    private TMP_Text CreateText(string name, Transform parent, string value, float fontSize, Vector2 position, Vector2 size, Color color, FontStyles style = FontStyles.Normal, TextAlignmentOptions alignment = TextAlignmentOptions.Left)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var text = go.GetComponent<TextMeshProUGUI>();
        text.font = Font;
        text.fontSize = fontSize;
        text.color = color;
        text.fontStyle = style;
        text.alignment = alignment;
        text.text = value;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        return text;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private static void Stretch(RectTransform rect, float left, float bottom, float right, float top)
    {
        SetRect(rect, new Vector2(left, bottom), new Vector2(1f - right, 1f - top), Vector2.zero, Vector2.zero);
    }

    private static Sprite WhiteSprite
    {
        get
        {
            if (_whiteSprite == null)
            {
                _whiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
                _whiteSprite.name = "RuntimeWhiteSprite";
            }

            return _whiteSprite;
        }
    }

    private static TMP_FontAsset Font
    {
        get
        {
            if (_font == null)
            {
                _font = TMP_Settings.defaultFontAsset;
                if (_font == null) _font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }

            return _font;
        }
    }

    private static string GetActivityTitle(CampaignActivity activity)
    {
        switch (activity)
        {
            case CampaignActivity.Rally: return "Acto político";
            case CampaignActivity.Interview: return "Entrevista periodística";
            case CampaignActivity.Negotiation: return "Negociación política";
            default: return activity.ToString();
        }
    }
}
}
