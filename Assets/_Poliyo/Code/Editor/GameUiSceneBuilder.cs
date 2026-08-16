using System;
using Poliyo.Content;
using Poliyo.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Poliyo.Presentation.Editor
{
/// <summary>
/// Rebuilds the authored main-menu, calendar and territorial-map scenes.
/// Runtime presenters remain the source of screen state; this class owns only editor-time composition.
/// </summary>
public static class GameUiSceneBuilder
{
    private const string SceneFolder = "Assets/_Poliyo/Scenes";
    private const string MainMenuScenePath = SceneFolder + "/MainMenu.unity";
    private const string CalendarScenePath = SceneFolder + "/CampaignCalendar.unity";
    private const string MapScenePath = SceneFolder + "/CampaignMap.unity";
    private const string CampaignSliceScenePath = SceneFolder + "/CampaignSlice.unity";
    private const string TeamScenePath = SceneFolder + "/TeamScene.unity";
    private const string CampaignCatalogPath = "Assets/_Poliyo/Data/CampaignCatalog.asset";

    [MenuItem("Poliyo/UI/Create or Update Prototype Screens")]
    public static void CreateOrUpdatePrototypeScreens()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        CampaignContentDefinition catalog = LoadContentCatalog();
        RebuildPrototypeScreens(catalog);
        AssetDatabase.SaveAssets();
    }

    internal static void RebuildPrototypeScreens(CampaignContentDefinition catalog)
    {
        if (catalog == null)
        {
            throw new ArgumentNullException(nameof(catalog));
        }

        CreateMainMenu(catalog);
        CreateCalendar(catalog);
        CreateMap(catalog);
        AddScenesToBuildSettings();
    }

    private static void CreateMainMenu(CampaignContentDefinition catalog)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateSessionHost(catalog);
        GameObject canvas = PoliyoUiTheme.CreateCanvas("MainMenuCanvas", out EventSystem eventSystem);
        PoliyoUiTheme.CreateFullScreenBackground(canvas.transform);

        Image editorialField = PoliyoUiTheme.CreatePanel(canvas.transform, "MenuFrame", PoliyoUiTheme.Ink);
        PoliyoUiTheme.SetRect(
            editorialField.rectTransform,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            Vector2.zero,
            new Vector2(1110f, PoliyoUiTheme.ReferenceHeight));
        UiSceneNavigation navigator = editorialField.gameObject.AddComponent<UiSceneNavigation>();

        PoliyoUiTheme.CreateRule(editorialField.transform, "CoralMastheadRule", PoliyoUiTheme.Coral, Vector2.zero, new Vector2(1110f, 14f));
        PoliyoUiTheme.CreateChip(editorialField.transform, "EditionChip", "EDICIÓN ELECTORAL", PoliyoUiTheme.Yellow, PoliyoUiTheme.Ink, new Vector2(76f, -74f), new Vector2(238f, 38f), 16f);
        PoliyoUiTheme.CreateText(editorialField.transform, "GameLogo", "POLIYO", 78f, TextAlignmentOptions.Left, PoliyoUiTheme.Ivory, new Vector2(72f, -130f), new Vector2(720f, 104f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(editorialField.transform, "MenuKicker", "REPÚBLICA FEDERAL DE ROSCALIA", 19f, TextAlignmentOptions.Left, PoliyoUiTheme.Turquoise, new Vector2(78f, -236f), new Vector2(600f, 34f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(
            editorialField.transform,
            "Subtitle",
            "Armá una campaña, negociá con información incompleta\ny sobreviví a sesenta días de política nacional.",
            24f,
            TextAlignmentOptions.TopLeft,
            new Color32(216, 222, 232, 255),
            new Vector2(78f, -286f),
            new Vector2(770f, 88f));

        Button continueCampaign = PoliyoUiTheme.CreateButton(editorialField.transform, "ContinueCampaignButton", "Continuar", new Vector2(78f, -420f), new Vector2(454f, 62f), PoliyoButtonStyle.Primary, 22f);
        Button newCampaign = PoliyoUiTheme.CreateButton(editorialField.transform, "NewCampaignButton", "Nueva campaña", new Vector2(78f, -498f), new Vector2(454f, 62f), PoliyoButtonStyle.Secondary, 22f);
        Button loadCampaign = PoliyoUiTheme.CreateButton(editorialField.transform, "LoadCampaignButton", "Cargar partida", new Vector2(78f, -576f), new Vector2(454f, 62f), PoliyoButtonStyle.Quiet, 21f);
        Button options = PoliyoUiTheme.CreateButton(editorialField.transform, "OptionsButton", "Opciones", new Vector2(78f, -654f), new Vector2(220f, 58f), PoliyoButtonStyle.Quiet, 20f);
        Button credits = PoliyoUiTheme.CreateButton(editorialField.transform, "CreditsButton", "Créditos", new Vector2(312f, -654f), new Vector2(220f, 58f), PoliyoButtonStyle.Quiet, 20f);
        Button quit = PoliyoUiTheme.CreateButton(editorialField.transform, "QuitButton", "Salir", new Vector2(78f, -728f), new Vector2(454f, 58f), PoliyoButtonStyle.Danger, 20f);
        TMP_Text saveStatus = PoliyoUiTheme.CreateText(editorialField.transform, "SaveStatus", "SIN AUTOSAVE · INICIÁ UNA NUEVA CAMPAÑA", 15f, TextAlignmentOptions.Left, PoliyoUiTheme.Turquoise, new Vector2(554f, -438f), new Vector2(460f, 54f), FontStyles.Bold);

        options.interactable = false;
        credits.interactable = false;
        PoliyoUiTheme.CreateText(editorialField.transform, "UnavailableNote", "OPCIONES Y CRÉDITOS · PRÓXIMAMENTE", 15f, TextAlignmentOptions.Left, PoliyoUiTheme.Blue, new Vector2(554f, -671f), new Vector2(390f, 28f), FontStyles.Bold);
        PoliyoUiTheme.CreateRule(editorialField.transform, "MenuDivider", PoliyoUiTheme.WithAlpha(PoliyoUiTheme.Ivory, 0.25f), new Vector2(78f, -834f), new Vector2(890f, 2f));
        PoliyoUiTheme.CreateText(editorialField.transform, "MenuNote", "VERTICAL SLICE  ·  SINGLEPLAYER  ·  MANDO, TECLADO Y MOUSE", 16f, TextAlignmentOptions.Left, new Color32(176, 187, 206, 255), new Vector2(78f, -864f), new Vector2(840f, 34f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(editorialField.transform, "SatireNote", "Toda semejanza con la realidad es, por supuesto, culpa de la realidad.", 17f, TextAlignmentOptions.Left, PoliyoUiTheme.Ivory, new Vector2(78f, -918f), new Vector2(820f, 44f));

        Image brief = PoliyoUiTheme.CreatePanel(canvas.transform, "RoscaliaBrief", PoliyoUiTheme.Paper);
        PoliyoUiTheme.SetRect(brief.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(1160f, -64f), new Vector2(696f, 936f));
        PoliyoUiTheme.CreateRule(brief.transform, "BriefBlueRule", PoliyoUiTheme.Blue, Vector2.zero, new Vector2(696f, 12f));
        PoliyoUiTheme.CreateChip(brief.transform, "BriefStatus", "60 DÍAS · 5 CANDIDATOS", PoliyoUiTheme.Turquoise, PoliyoUiTheme.Ink, new Vector2(40f, -42f), new Vector2(260f, 36f), 15f);
        PoliyoUiTheme.CreateText(brief.transform, "BriefTitle", "La mesa está servida", 40f, TextAlignmentOptions.Left, PoliyoUiTheme.Ink, new Vector2(40f, -108f), new Vector2(590f, 58f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(brief.transform, "BriefDeck", "Encuestas imperfectas. Provincias orgullosas.\nUn equipo que nunca cuenta todo.", 22f, TextAlignmentOptions.TopLeft, PoliyoUiTheme.MutedInk, new Vector2(42f, -174f), new Vector2(570f, 86f));

        CreateBriefRow(brief.transform, "BriefRow_0", "01", "Leé el territorio", "Seis jurisdicciones, veinticuatro localidades.", PoliyoUiTheme.Coral, -304f);
        CreateBriefRow(brief.transform, "BriefRow_1", "02", "Elegí la jugada", "Una acción pública por jornada.", PoliyoUiTheme.Turquoise, -430f);
        CreateBriefRow(brief.transform, "BriefRow_2", "03", "Explicá el costo", "Cada cambio deja una causa trazable.", PoliyoUiTheme.Yellow, -556f);

        Image clipping = PoliyoUiTheme.CreatePanel(brief.transform, "RoscaliaClipping", PoliyoUiTheme.Ivory);
        PoliyoUiTheme.SetRect(clipping.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -706f), new Vector2(616f, 164f));
        PoliyoUiTheme.CreateChip(clipping.transform, "ClippingSource", "EL OBSERVADOR", PoliyoUiTheme.Violet, PoliyoUiTheme.White, new Vector2(18f, -18f), new Vector2(170f, 30f), 13f);
        PoliyoUiTheme.CreateText(clipping.transform, "ClippingHeadline", "“Roscalia exige certezas”\nRoscalia procede a no ofrecer ninguna.", 23f, TextAlignmentOptions.TopLeft, PoliyoUiTheme.Ink, new Vector2(18f, -58f), new Vector2(560f, 82f), FontStyles.Bold);

        MainMenuScreenPresenter menuPresenter = editorialField.gameObject.AddComponent<MainMenuScreenPresenter>();
        menuPresenter.Configure(continueCampaign, loadCampaign, newCampaign, saveStatus);

        UnityEventTools.AddPersistentListener(continueCampaign.onClick, menuPresenter.LoadAutosave);
        UnityEventTools.AddStringPersistentListener(continueCampaign.onClick, navigator.OpenScene, "CampaignSlice");
        UnityEventTools.AddPersistentListener(newCampaign.onClick, menuPresenter.StartNewCampaign);
        UnityEventTools.AddStringPersistentListener(newCampaign.onClick, navigator.OpenScene, "CampaignSlice");
        UnityEventTools.AddPersistentListener(loadCampaign.onClick, menuPresenter.LoadAutosave);
        UnityEventTools.AddStringPersistentListener(loadCampaign.onClick, navigator.OpenScene, "CampaignSlice");
        UnityEventTools.AddPersistentListener(quit.onClick, navigator.QuitGame);
        PoliyoUiTheme.SetFirstSelected(eventSystem, continueCampaign);

        SaveScene(scene, MainMenuScenePath);
    }

    private static void CreateCalendar(CampaignContentDefinition catalog)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateSessionHost(catalog);
        GameObject canvas = PoliyoUiTheme.CreateCanvas("CampaignCalendarCanvas", out EventSystem eventSystem);
        PoliyoUiTheme.CreateFullScreenBackground(canvas.transform);
        UiSceneNavigation navigator = canvas.AddComponent<UiSceneNavigation>();

        CampaignChrome chrome = PoliyoUiTheme.CreateCampaignChrome(
            canvas.transform,
            "Calendario editorial",
            "Ordená el día antes de que el día te ordene a vos.",
            CampaignNavigationSelection.Calendar);
        WireCampaignChrome(chrome, navigator);

        Image calendarPanel = PoliyoUiTheme.CreatePanel(canvas.transform, "CalendarPanel", PoliyoUiTheme.Paper);
        PoliyoUiTheme.SetRect(calendarPanel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -164f), new Vector2(1260f, 872f));
        PoliyoUiTheme.CreateRule(calendarPanel.transform, "CalendarAccent", PoliyoUiTheme.Coral, Vector2.zero, new Vector2(1260f, 10f));
        TMP_Text dayLabel = PoliyoUiTheme.CreateText(calendarPanel.transform, "CalendarHeading", "Semana 1 · Día 1", 32f, TextAlignmentOptions.Left, PoliyoUiTheme.Ink, new Vector2(28f, -30f), new Vector2(460f, 50f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(calendarPanel.transform, "CalendarHint", "VENTANA DE CAMPAÑA · UNA ACCIÓN PÚBLICA POR JORNADA", 16f, TextAlignmentOptions.Left, PoliyoUiTheme.MutedInk, new Vector2(30f, -78f), new Vector2(620f, 28f), FontStyles.Bold);
        Image phaseChip = PoliyoUiTheme.CreateChip(calendarPanel.transform, "CalendarPhaseChip", "FASE: POSICIONAMIENTO", PoliyoUiTheme.Blue, PoliyoUiTheme.Ink, new Vector2(936f, -36f), new Vector2(292f, 34f), 14f);
        TMP_Text phaseLabel = phaseChip.GetComponentInChildren<TMP_Text>(true);

        string[] dayNames = { "LUN", "MAR", "MIÉ", "JUE", "VIE", "SÁB", "DOM" };
        const float gridX = 26f;
        const float cellWidth = 166f;
        const float columnGap = 8f;
        const float cellHeight = 124f;
        const float rowGap = 8f;
        const float gridTop = -154f;
        for (var column = 0; column < dayNames.Length; column++)
        {
            float x = gridX + column * (cellWidth + columnGap);
            PoliyoUiTheme.CreateText(calendarPanel.transform, "DayHeader_" + column, dayNames[column], 16f, TextAlignmentOptions.Center, PoliyoUiTheme.Ink, new Vector2(x, -116f), new Vector2(cellWidth, 28f), FontStyles.Bold);
        }

        for (var index = 0; index < 35; index++)
        {
            int column = index % 7;
            int row = index / 7;
            float x = gridX + column * (cellWidth + columnGap);
            float y = gridTop - row * (cellHeight + rowGap);
            Color cellColor = row % 2 == 0 ? PoliyoUiTheme.Ivory : (Color)new Color32(240, 233, 219, 255);
            Image cell = PoliyoUiTheme.CreatePanel(calendarPanel.transform, "CalendarCell_" + row + "_" + column, cellColor);
            PoliyoUiTheme.SetRect(cell.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, y), new Vector2(cellWidth, cellHeight));
            Color dayColor = index == 0 ? PoliyoUiTheme.Coral : PoliyoUiTheme.Ink;
            PoliyoUiTheme.CreateText(cell.transform, "DayNumber", (index + 1).ToString("00"), 18f, TextAlignmentOptions.TopLeft, dayColor, new Vector2(12f, -9f), new Vector2(52f, 28f), FontStyles.Bold);
            if (index == 0)
            {
                PoliyoUiTheme.CreateChip(cell.transform, "CampaignStartChip", "INICIO", PoliyoUiTheme.Coral, PoliyoUiTheme.Ink, new Vector2(82f, -10f), new Vector2(70f, 24f), 12f);
            }
        }

        Button rally = PoliyoUiTheme.CreateButton(calendarPanel.transform, "CapitalRallyButton", "Acto capital", new Vector2(200f, -214f), new Vector2(142f, 48f), PoliyoButtonStyle.Primary, 17f);
        Button interview = PoliyoUiTheme.CreateButton(calendarPanel.transform, "InterviewButton", "Entrevista", new Vector2(548f, -346f), new Vector2(142f, 48f), PoliyoButtonStyle.SelectedNavigation, 17f);
        Button teamMeeting = PoliyoUiTheme.CreateButton(calendarPanel.transform, "TeamMeetingButton", "Reunión", new Vector2(896f, -478f), new Vector2(142f, 48f), PoliyoButtonStyle.Quiet, 17f);
        teamMeeting.interactable = false;
        PoliyoUiTheme.CreateChip(calendarPanel.transform, "DebateMilestone", "DEBATE", PoliyoUiTheme.Violet, PoliyoUiTheme.White, new Vector2(722f, -742f), new Vector2(142f, 32f), 13f);
        PoliyoUiTheme.CreateChip(calendarPanel.transform, "PollMilestone", "CORTE ENCUESTA", PoliyoUiTheme.Yellow, PoliyoUiTheme.Ink, new Vector2(1070f, -610f), new Vector2(142f, 32f), 12f);

        Image actionPanel = PoliyoUiTheme.CreatePanel(canvas.transform, "CalendarActions", PoliyoUiTheme.Ink);
        PoliyoUiTheme.SetRect(actionPanel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(1332f, -164f), new Vector2(544f, 872f));
        PoliyoUiTheme.CreateRule(actionPanel.transform, "ActionsAccent", PoliyoUiTheme.Turquoise, Vector2.zero, new Vector2(544f, 10f));
        PoliyoUiTheme.CreateText(actionPanel.transform, "ActionsHeading", "Parte diario", 32f, TextAlignmentOptions.Left, PoliyoUiTheme.Ivory, new Vector2(32f, -34f), new Vector2(360f, 48f), FontStyles.Bold);
        TMP_Text statusLabel = PoliyoUiTheme.CreateText(actionPanel.transform, "ActionsText", "Fondos: $1200 · alcance nacional", 19f, TextAlignmentOptions.TopLeft, new Color32(205, 216, 230, 255), new Vector2(32f, -94f), new Vector2(474f, 70f));
        Image actionCapacityChip = PoliyoUiTheme.CreateChip(actionPanel.transform, "ActionCapacity", "1 ACCIÓN DISPONIBLE", PoliyoUiTheme.Yellow, PoliyoUiTheme.Ink, new Vector2(32f, -178f), new Vector2(244f, 34f), 14f);
        TMP_Text actionCapacityLabel = actionCapacityChip.GetComponentInChildren<TMP_Text>(true);

        CreateAgendaRow(actionPanel.transform, "AgendaRow_0", "09:00", "Reunión de estrategia", "Interna · sin costo", -242f, PoliyoUiTheme.Blue);
        CreateAgendaRow(actionPanel.transform, "AgendaRow_1", "15:30", "Ventana pública", "Elegí un acto o entrevista", -360f, PoliyoUiTheme.Coral);
        CreateAgendaRow(actionPanel.transform, "AgendaRow_2", "20:00", "Cierre de jornada", "Se explican costos y efectos", -478f, PoliyoUiTheme.Turquoise);

        Button negotiation = PoliyoUiTheme.CreateButton(actionPanel.transform, "NegotiationButton", "Resolver negociación", new Vector2(32f, -620f), new Vector2(480f, 58f), PoliyoButtonStyle.Secondary, 20f);
        Button nextDay = PoliyoUiTheme.CreateButton(actionPanel.transform, "NextDayButton", "Cerrar día y continuar", new Vector2(32f, -696f), new Vector2(480f, 68f), PoliyoButtonStyle.Primary, 21f);
        PoliyoUiTheme.CreateText(actionPanel.transform, "CostLegend", "CLAVE  ·  CORAL: PÚBLICO   TURQUESA: INTERNO   VIOLETA: HITO", 13f, TextAlignmentOptions.Left, new Color32(168, 181, 202, 255), new Vector2(32f, -790f), new Vector2(474f, 42f), FontStyles.Bold);

        CampaignCalendarScreenPresenter presenter = calendarPanel.gameObject.AddComponent<CampaignCalendarScreenPresenter>();
        presenter.Configure(dayLabel, statusLabel, phaseLabel, actionCapacityLabel, rally, interview, negotiation, nextDay);
        UnityEventTools.AddPersistentListener(rally.onClick, presenter.ResolveRally);
        UnityEventTools.AddPersistentListener(negotiation.onClick, presenter.ResolveNegotiation);
        UnityEventTools.AddPersistentListener(nextDay.onClick, presenter.AdvanceDay);

        Image drawerPanel = PoliyoUiTheme.CreatePanel(canvas.transform, "InterviewDrawer", new Color32(23, 35, 59, 252), true);
        PoliyoUiTheme.SetRect(drawerPanel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(730f, -184f), new Vector2(1120f, 790f));
        CanvasGroup drawerCanvasGroup = drawerPanel.gameObject.AddComponent<CanvasGroup>();
        CalendarInterviewDrawer drawer = drawerPanel.gameObject.AddComponent<CalendarInterviewDrawer>();
        PoliyoUiTheme.CreateRule(drawerPanel.transform, "DrawerAccent", PoliyoUiTheme.Coral, Vector2.zero, new Vector2(1120f, 12f));
        PoliyoUiTheme.CreateChip(drawerPanel.transform, "DrawerTypeChip", "APARICIÓN PÚBLICA", PoliyoUiTheme.Yellow, PoliyoUiTheme.Ink, new Vector2(42f, -42f), new Vector2(220f, 34f), 14f);
        PoliyoUiTheme.CreateText(drawerPanel.transform, "InterviewHeading", "Elegí el medio", 38f, TextAlignmentOptions.Left, PoliyoUiTheme.Ivory, new Vector2(42f, -104f), new Vector2(520f, 56f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(drawerPanel.transform, "InterviewDescription", "En este vertical slice la entrevista es una acción nacional única: prioriza confianza y consume la actividad pública del día.", 20f, TextAlignmentOptions.TopLeft, new Color32(204, 214, 229, 255), new Vector2(44f, -166f), new Vector2(920f, 66f));

        CreateMediaBrief(drawerPanel.transform, "InterviewBrief", "ENTREVISTA NACIONAL", "Prioriza confianza · alcance nacional", "Costo $35 · una acción", -292f, PoliyoUiTheme.Turquoise);
        Button confirmInterview = PoliyoUiTheme.CreateButton(drawerPanel.transform, "ConfirmInterviewButton", "Confirmar entrevista", new Vector2(774f, -304f), new Vector2(286f, 58f), PoliyoButtonStyle.Primary, 18f);
        Button closeDrawer = PoliyoUiTheme.CreateButton(drawerPanel.transform, "CloseInterviewDrawerButton", "Volver al calendario", new Vector2(42f, -684f), new Vector2(350f, 56f), PoliyoButtonStyle.Quiet, 19f);
        drawer.Configure(drawerCanvasGroup, confirmInterview);
        UnityEventTools.AddPersistentListener(interview.onClick, drawer.Toggle);
        UnityEventTools.AddPersistentListener(closeDrawer.onClick, drawer.Close);
        UnityEventTools.AddPersistentListener(confirmInterview.onClick, presenter.ResolveInterview);
        UnityEventTools.AddPersistentListener(confirmInterview.onClick, drawer.Close);

        PoliyoUiTheme.SetFirstSelected(eventSystem, rally);
        SaveScene(scene, CalendarScenePath);
    }

    private static void CreateMap(CampaignContentDefinition catalog)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateSessionHost(catalog);
        GameObject canvas = PoliyoUiTheme.CreateCanvas("CampaignMapCanvas", out EventSystem eventSystem);
        PoliyoUiTheme.CreateFullScreenBackground(canvas.transform);
        UiSceneNavigation navigator = canvas.AddComponent<UiSceneNavigation>();

        CampaignChrome chrome = PoliyoUiTheme.CreateCampaignChrome(
            canvas.transform,
            "Mapa de Roscalia",
            "Elegí una prioridad territorial; el mapa no promete certezas.",
            CampaignNavigationSelection.Map);
        WireCampaignChrome(chrome, navigator);

        Image mapPanel = PoliyoUiTheme.CreatePanel(canvas.transform, "RoscaliaMap", PoliyoUiTheme.Paper);
        PoliyoUiTheme.SetRect(mapPanel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -164f), new Vector2(1210f, 872f));
        PoliyoUiTheme.CreateRule(mapPanel.transform, "MapAccent", PoliyoUiTheme.Blue, Vector2.zero, new Vector2(1210f, 10f));
        PoliyoUiTheme.CreateText(mapPanel.transform, "MapInstructions", "Tablero territorial", 32f, TextAlignmentOptions.Left, PoliyoUiTheme.Ink, new Vector2(30f, -30f), new Vector2(420f, 48f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(mapPanel.transform, "MapCaption", "SEIS JURISDICCIONES · DATOS PARCIALES · PRIORIDAD ÚNICA", 15f, TextAlignmentOptions.Left, PoliyoUiTheme.MutedInk, new Vector2(32f, -78f), new Vector2(620f, 28f), FontStyles.Bold);
        PoliyoUiTheme.CreateChip(mapPanel.transform, "MapScaleChip", "24 LOCALIDADES", PoliyoUiTheme.Yellow, PoliyoUiTheme.Ink, new Vector2(980f, -42f), new Vector2(198f, 34f), 14f);

        Image territoryField = PoliyoUiTheme.CreatePanel(mapPanel.transform, "TerritoryField", PoliyoUiTheme.Ivory);
        PoliyoUiTheme.SetRect(territoryField.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -130f), new Vector2(1150f, 684f));
        PoliyoUiTheme.CreateRule(territoryField.transform, "NorthAxis", PoliyoUiTheme.WithAlpha(PoliyoUiTheme.Ink, 0.18f), new Vector2(574f, -34f), new Vector2(2f, 600f));
        PoliyoUiTheme.CreateRule(territoryField.transform, "WestAxis", PoliyoUiTheme.WithAlpha(PoliyoUiTheme.Ink, 0.18f), new Vector2(80f, -334f), new Vector2(988f, 2f));
        PoliyoUiTheme.CreateText(territoryField.transform, "NorthMarker", "NORTE ↑", 13f, TextAlignmentOptions.Center, PoliyoUiTheme.MutedInk, new Vector2(520f, -8f), new Vector2(110f, 24f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(territoryField.transform, "OceanMarker", "MAR DE ROSCALIA", 13f, TextAlignmentOptions.Center, PoliyoUiTheme.Blue, new Vector2(910f, -610f), new Vector2(190f, 26f), FontStyles.Bold);

        var zoneIds = new[] { "puerto-alba", "gran-ribera", "ventisca", "cumbre-dorada", "monte-rojo", "sierra-clara" };
        var zones = new[] { "Puerto Alba", "Gran Ribera", "Ventisca", "Cumbre Dorada", "Monte Rojo", "Sierra Clara" };
        var positions = new[]
        {
            new Vector2(72f, -108f),
            new Vector2(386f, -76f),
            new Vector2(722f, -138f),
            new Vector2(160f, -300f),
            new Vector2(520f, -342f),
            new Vector2(344f, -520f)
        };
        var sizes = new[]
        {
            new Vector2(292f, 120f),
            new Vector2(318f, 136f),
            new Vector2(318f, 118f),
            new Vector2(336f, 142f),
            new Vector2(314f, 132f),
            new Vector2(370f, 116f)
        };
        var styles = new[]
        {
            PoliyoButtonStyle.SelectedNavigation,
            PoliyoButtonStyle.Primary,
            PoliyoButtonStyle.Secondary,
            PoliyoButtonStyle.Quiet,
            PoliyoButtonStyle.SelectedNavigation,
            PoliyoButtonStyle.Secondary
        };

        CampaignMapScreenPresenter selection;
        Image detailsPanel = CreateMapDetails(canvas.transform, navigator, out selection);
        Button firstZone = null;
        for (var index = 0; index < zones.Length; index++)
        {
            Button zone = PoliyoUiTheme.CreateButton(territoryField.transform, "ZoneButton_" + index, zones[index], positions[index], sizes[index], styles[index], 20f);
            UnityEventTools.AddStringPersistentListener(zone.onClick, selection.SelectJurisdiction, zoneIds[index]);
            if (firstZone == null)
            {
                firstZone = zone;
            }
        }

        PoliyoUiTheme.CreateText(mapPanel.transform, "MapSourceNote", "FUENTE: PARTES TERRITORIALES · CALIDAD VARIABLE · ÚLTIMA ACTUALIZACIÓN: HOY", 13f, TextAlignmentOptions.Left, PoliyoUiTheme.MutedInk, new Vector2(32f, -826f), new Vector2(880f, 26f), FontStyles.Bold);
        detailsPanel.transform.SetAsLastSibling();
        PoliyoUiTheme.SetFirstSelected(eventSystem, firstZone);
        SaveScene(scene, MapScenePath);
    }

    private static Image CreateMapDetails(Transform parent, UiSceneNavigation navigator, out CampaignMapScreenPresenter presenter)
    {
        Image detailsPanel = PoliyoUiTheme.CreatePanel(parent, "ZoneDetailsDrawer", PoliyoUiTheme.Ink);
        PoliyoUiTheme.SetRect(detailsPanel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(1280f, -164f), new Vector2(596f, 872f));
        PoliyoUiTheme.CreateRule(detailsPanel.transform, "DetailsAccent", PoliyoUiTheme.Turquoise, Vector2.zero, new Vector2(596f, 10f));
        PoliyoUiTheme.CreateChip(detailsPanel.transform, "KnownDataChip", "INFORMACIÓN CONOCIDA", PoliyoUiTheme.Yellow, PoliyoUiTheme.Ink, new Vector2(34f, -40f), new Vector2(238f, 34f), 14f);
        TMP_Text zoneName = PoliyoUiTheme.CreateText(detailsPanel.transform, "ZoneName", "Seleccioná una jurisdicción", 31f, TextAlignmentOptions.TopLeft, PoliyoUiTheme.Ivory, new Vector2(34f, -104f), new Vector2(510f, 84f), FontStyles.Bold);
        TMP_Text zoneDescription = PoliyoUiTheme.CreateText(detailsPanel.transform, "ZoneDescription", "Elegí una zona para consultar sus localidades y concentrar allí las próximas acciones.", 20f, TextAlignmentOptions.TopLeft, new Color32(205, 216, 231, 255), new Vector2(34f, -202f), new Vector2(512f, 196f));

        Image dossier = PoliyoUiTheme.CreatePanel(detailsPanel.transform, "TerritorialDossier", PoliyoUiTheme.InkSoft);
        PoliyoUiTheme.SetRect(dossier.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -424f), new Vector2(528f, 210f));
        PoliyoUiTheme.CreateText(dossier.transform, "ZoneActions", "PRÓXIMA CONSECUENCIA", 15f, TextAlignmentOptions.Left, PoliyoUiTheme.Turquoise, new Vector2(22f, -20f), new Vector2(310f, 28f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(dossier.transform, "ConsequenceSummary", "Las actividades del calendario impactarán la jurisdicción seleccionada hasta cambiar la prioridad.", 20f, TextAlignmentOptions.TopLeft, PoliyoUiTheme.Ivory, new Vector2(22f, -60f), new Vector2(478f, 84f));
        PoliyoUiTheme.CreateText(dossier.transform, "ConfidenceNote", "SEÑAL DISPONIBLE · NO ES UNA ENCUESTA EXACTA", 13f, TextAlignmentOptions.Left, PoliyoUiTheme.Yellow, new Vector2(22f, -154f), new Vector2(470f, 28f), FontStyles.Bold);

        Button back = PoliyoUiTheme.CreateButton(detailsPanel.transform, "BackToCalendarButton", "Planificar en calendario", new Vector2(34f, -686f), new Vector2(528f, 66f), PoliyoButtonStyle.Primary, 21f);
        UnityEventTools.AddStringPersistentListener(back.onClick, navigator.OpenScene, "CampaignCalendar");
        PoliyoUiTheme.CreateText(detailsPanel.transform, "DetailsSource", "FUENTE · EQUIPO TERRITORIAL", 13f, TextAlignmentOptions.Left, new Color32(164, 179, 201, 255), new Vector2(34f, -782f), new Vector2(380f, 26f), FontStyles.Bold);

        presenter = detailsPanel.gameObject.AddComponent<CampaignMapScreenPresenter>();
        presenter.Configure(zoneName, zoneDescription);
        return detailsPanel;
    }

    private static void CreateBriefRow(Transform parent, string name, string number, string title, string description, Color accent, float y)
    {
        Image row = PoliyoUiTheme.CreatePanel(parent, name, PoliyoUiTheme.WithAlpha(PoliyoUiTheme.White, 0.55f));
        PoliyoUiTheme.SetRect(row.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, y), new Vector2(616f, 108f));
        PoliyoUiTheme.CreateChip(row.transform, "Number", number, accent, PoliyoUiTheme.Ink, new Vector2(16f, -18f), new Vector2(52f, 52f), 18f);
        PoliyoUiTheme.CreateText(row.transform, "Title", title, 22f, TextAlignmentOptions.Left, PoliyoUiTheme.Ink, new Vector2(86f, -14f), new Vector2(470f, 34f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(row.transform, "Description", description, 17f, TextAlignmentOptions.Left, PoliyoUiTheme.MutedInk, new Vector2(86f, -54f), new Vector2(480f, 34f));
    }

    private static void CreateAgendaRow(Transform parent, string name, string time, string title, string detail, float y, Color accent)
    {
        Image row = PoliyoUiTheme.CreatePanel(parent, name, PoliyoUiTheme.InkSoft);
        PoliyoUiTheme.SetRect(row.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, y), new Vector2(480f, 100f));
        PoliyoUiTheme.CreateRule(row.transform, "Accent", accent, Vector2.zero, new Vector2(8f, 100f));
        PoliyoUiTheme.CreateText(row.transform, "Time", time, 16f, TextAlignmentOptions.Left, accent, new Vector2(24f, -16f), new Vector2(82f, 28f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(row.transform, "Title", title, 19f, TextAlignmentOptions.Left, PoliyoUiTheme.Ivory, new Vector2(112f, -14f), new Vector2(330f, 32f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(row.transform, "Detail", detail, 16f, TextAlignmentOptions.Left, new Color32(176, 189, 209, 255), new Vector2(112f, -52f), new Vector2(332f, 30f));
    }

    private static void CreateMediaBrief(Transform parent, string name, string title, string audience, string risk, float y, Color accent)
    {
        Image row = PoliyoUiTheme.CreatePanel(parent, name, PoliyoUiTheme.InkSoft);
        PoliyoUiTheme.SetRect(row.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(42f, y), new Vector2(700f, 94f));
        PoliyoUiTheme.CreateRule(row.transform, "Accent", accent, Vector2.zero, new Vector2(9f, 94f));
        PoliyoUiTheme.CreateText(row.transform, "Title", title, 18f, TextAlignmentOptions.Left, PoliyoUiTheme.Ivory, new Vector2(24f, -15f), new Vector2(300f, 30f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(row.transform, "Audience", audience, 16f, TextAlignmentOptions.Left, new Color32(188, 200, 219, 255), new Vector2(24f, -50f), new Vector2(400f, 28f));
        PoliyoUiTheme.CreateChip(row.transform, "Risk", risk.ToUpperInvariant(), accent, PoliyoUiTheme.Ink, new Vector2(500f, -30f), new Vector2(170f, 32f), 13f);
    }

    private static void WireCampaignChrome(CampaignChrome chrome, UiSceneNavigation navigator)
    {
        UnityEventTools.AddStringPersistentListener(chrome.Calendar.onClick, navigator.OpenScene, "CampaignCalendar");
        UnityEventTools.AddStringPersistentListener(chrome.Map.onClick, navigator.OpenScene, "CampaignMap");
        UnityEventTools.AddStringPersistentListener(chrome.Team.onClick, navigator.OpenScene, "TeamScene");
        UnityEventTools.AddStringPersistentListener(chrome.Campaign.onClick, navigator.OpenScene, "CampaignSlice");
    }

    private static CampaignContentDefinition LoadContentCatalog()
    {
        CampaignContentDefinition catalog = AssetDatabase.LoadAssetAtPath<CampaignContentDefinition>(CampaignCatalogPath);
        if (catalog == null)
        {
            throw new InvalidOperationException("Campaign catalog is missing. Run Poliyo/Vertical Slice/Create or Update Content and Scene first.");
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

    private static void SaveScene(Scene scene, string path)
    {
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, path);
    }

    private static void AddScenesToBuildSettings()
    {
        string[] canonicalOrder =
        {
            MainMenuScenePath,
            CampaignSliceScenePath,
            CalendarScenePath,
            MapScenePath,
            TeamScenePath
        };

        var buildScenes = new EditorBuildSettingsScene[canonicalOrder.Length];
        for (var index = 0; index < canonicalOrder.Length; index++)
        {
            buildScenes[index] = new EditorBuildSettingsScene(canonicalOrder[index], true);
        }

        EditorBuildSettings.scenes = buildScenes;
    }
}
}
