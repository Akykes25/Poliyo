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
/// Wires the menu flow and regenerates CampaignSlice and TeamScene from stable UGUI contracts.
/// This builder intentionally leaves simulation, persistence and runtime presenters unchanged.
/// </summary>
public static class CampaignFlowAndTeamSceneBuilder
{
    private const string CampaignCatalogPath = "Assets/_Poliyo/Data/CampaignCatalog.asset";
    private const string MainMenuScenePath = "Assets/_Poliyo/Scenes/MainMenu.unity";
    private const string CampaignSliceScenePath = "Assets/_Poliyo/Scenes/CampaignSlice.unity";
    private const string TeamScenePath = "Assets/_Poliyo/Scenes/TeamScene.unity";
    private const string CalendarScenePath = "Assets/_Poliyo/Scenes/CampaignCalendar.unity";
    private const string MapScenePath = "Assets/_Poliyo/Scenes/CampaignMap.unity";

    [MenuItem("Poliyo/UI/Apply Campaign Flow and Rebuild Campaign + Team")]
    public static void Apply()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        CampaignContentDefinition catalog = LoadCatalog();
        GameUiSceneBuilder.RebuildPrototypeScreens(catalog);
        ConfigureMainMenu(catalog);
        RebuildCampaignSlice(catalog);
        RebuildTeamScene(catalog);
        EnsureBuildScenes();
        AssetDatabase.SaveAssets();
    }

    private static void ConfigureMainMenu(CampaignContentDefinition catalog)
    {
        Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
        FindOrCreateSessionHost(catalog);
        UiSceneNavigation navigator = FindRequiredComponent<UiSceneNavigation>(scene, "MenuFrame");
        MainMenuScreenPresenter menuPresenter = FindRequiredComponent<MainMenuScreenPresenter>(scene, "MenuFrame");
        Button continueCampaign = TryFindComponent<Button>(scene, "ContinueCampaignButton");
        Button newCampaign = FindRequiredComponent<Button>(scene, "NewCampaignButton");
        Button loadCampaign = FindRequiredComponent<Button>(scene, "LoadCampaignButton");

        if (continueCampaign != null)
        {
            Reset(continueCampaign);
            UnityEventTools.AddPersistentListener(continueCampaign.onClick, menuPresenter.LoadAutosave);
            UnityEventTools.AddStringPersistentListener(continueCampaign.onClick, navigator.OpenScene, "CampaignSlice");
        }

        Reset(newCampaign);
        Reset(loadCampaign);
        UnityEventTools.AddPersistentListener(newCampaign.onClick, menuPresenter.StartNewCampaign);
        UnityEventTools.AddStringPersistentListener(newCampaign.onClick, navigator.OpenScene, "CampaignSlice");
        UnityEventTools.AddPersistentListener(loadCampaign.onClick, menuPresenter.LoadAutosave);
        UnityEventTools.AddStringPersistentListener(loadCampaign.onClick, navigator.OpenScene, "CampaignSlice");
        Save(scene, MainMenuScenePath);
    }

    private static void RebuildCampaignSlice(CampaignContentDefinition catalog)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        FindOrCreateSessionHost(catalog);
        GameObject canvas = PoliyoUiTheme.CreateCanvas("CampaignCanvas", out EventSystem eventSystem);
        PoliyoUiTheme.CreateFullScreenBackground(canvas.transform);
        UiSceneNavigation navigator = canvas.AddComponent<UiSceneNavigation>();

        Image masthead = PoliyoUiTheme.CreatePanel(canvas.transform, "CampaignMasthead", PoliyoUiTheme.Ink);
        PoliyoUiTheme.SetRect(masthead.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(PoliyoUiTheme.ReferenceWidth, 140f));
        PoliyoUiTheme.CreateRule(masthead.transform, "MastheadRule", PoliyoUiTheme.Coral, Vector2.zero, new Vector2(PoliyoUiTheme.ReferenceWidth, 10f));
        PoliyoUiTheme.CreateText(masthead.transform, "Brand", "POLIYO", 34f, TextAlignmentOptions.Left, PoliyoUiTheme.Yellow, new Vector2(48f, -29f), new Vector2(190f, 48f), FontStyles.Bold);
        TMP_Text dayLabel = PoliyoUiTheme.CreateText(masthead.transform, "Dia_TXT", "Día 1 · Semana 1", 20f, TextAlignmentOptions.Left, PoliyoUiTheme.Ivory, new Vector2(252f, -31f), new Vector2(250f, 38f), FontStyles.Bold);
        TMP_Text budgetLabel = PoliyoUiTheme.CreateText(masthead.transform, "Presupuesto_TXT", "Presupuesto: $1200", 18f, TextAlignmentOptions.Left, new Color32(195, 207, 225, 255), new Vector2(504f, -33f), new Vector2(260f, 36f));
        PoliyoUiTheme.CreateChip(masthead.transform, "CampaignMode", "CAMPAÑA NACIONAL", PoliyoUiTheme.Violet, PoliyoUiTheme.White, new Vector2(770f, -34f), new Vector2(210f, 34f), 14f);

        Button calendar = PoliyoUiTheme.CreateButton(masthead.transform, "Calendario_Btn", "Calendario", new Vector2(1010f, -39f), new Vector2(174f, 56f), PoliyoButtonStyle.Navigation, 18f);
        Button map = PoliyoUiTheme.CreateButton(masthead.transform, "Mapa_Btn", "Mapa", new Vector2(1200f, -39f), new Vector2(132f, 56f), PoliyoButtonStyle.Navigation, 18f);
        Button team = PoliyoUiTheme.CreateButton(masthead.transform, "Equipo_Btn", "Equipo", new Vector2(1348f, -39f), new Vector2(140f, 56f), PoliyoButtonStyle.Navigation, 18f);
        Button press = PoliyoUiTheme.CreateButton(masthead.transform, "Prensa_Btn", "Prensa", new Vector2(1504f, -39f), new Vector2(140f, 56f), PoliyoButtonStyle.Navigation, 18f);
        Button menu = PoliyoUiTheme.CreateButton(masthead.transform, "MainMenu_Btn", "Menú", new Vector2(1660f, -39f), new Vector2(144f, 56f), PoliyoButtonStyle.Quiet, 18f);

        Image campaignDesk = PoliyoUiTheme.CreatePanel(canvas.transform, "CampaignDesk", PoliyoUiTheme.Paper);
        PoliyoUiTheme.SetRect(campaignDesk.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -170f), new Vector2(1164f, 866f));
        PoliyoUiTheme.CreateRule(campaignDesk.transform, "DeskRule", PoliyoUiTheme.Coral, Vector2.zero, new Vector2(1164f, 10f));
        PoliyoUiTheme.CreateChip(campaignDesk.transform, "DeskEdition", "PARTE DE CAMPAÑA", PoliyoUiTheme.Yellow, PoliyoUiTheme.Ink, new Vector2(34f, -38f), new Vector2(210f, 34f), 14f);
        PoliyoUiTheme.CreateText(campaignDesk.transform, "DeskTitle", "Mesa central", 42f, TextAlignmentOptions.Left, PoliyoUiTheme.Ink, new Vector2(32f, -98f), new Vector2(560f, 60f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(campaignDesk.transform, "DeskDeck", "Una lectura común antes de gastar tiempo, dinero o reputación.", 21f, TextAlignmentOptions.Left, PoliyoUiTheme.MutedInk, new Vector2(34f, -158f), new Vector2(740f, 42f));

        Image priority = PoliyoUiTheme.CreatePanel(campaignDesk.transform, "DailyPriority", PoliyoUiTheme.Ivory);
        PoliyoUiTheme.SetRect(priority.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -230f), new Vector2(514f, 246f));
        PoliyoUiTheme.CreateRule(priority.transform, "PriorityRule", PoliyoUiTheme.Turquoise, Vector2.zero, new Vector2(10f, 246f));
        PoliyoUiTheme.CreateText(priority.transform, "PriorityEyebrow", "PRIORIDAD DEL DÍA", 15f, TextAlignmentOptions.Left, PoliyoUiTheme.Turquoise, new Vector2(28f, -24f), new Vector2(260f, 28f), FontStyles.Bold);
        TMP_Text priorityTitle = PoliyoUiTheme.CreateText(priority.transform, "PriorityTitle", "Definir una acción pública", 26f, TextAlignmentOptions.TopLeft, PoliyoUiTheme.Ink, new Vector2(28f, -66f), new Vector2(430f, 70f), FontStyles.Bold);
        TMP_Text priorityCopy = PoliyoUiTheme.CreateText(priority.transform, "PriorityCopy", "Elegí territorio, mensaje y exposición desde Calendario. El cierre mostrará el costo y la causa.", 18f, TextAlignmentOptions.TopLeft, PoliyoUiTheme.MutedInk, new Vector2(28f, -144f), new Vector2(438f, 76f));

        Image territory = PoliyoUiTheme.CreatePanel(campaignDesk.transform, "TerritorialSignal", PoliyoUiTheme.Ivory);
        PoliyoUiTheme.SetRect(territory.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(574f, -230f), new Vector2(556f, 246f));
        PoliyoUiTheme.CreateRule(territory.transform, "TerritoryRule", PoliyoUiTheme.Blue, Vector2.zero, new Vector2(10f, 246f));
        PoliyoUiTheme.CreateText(territory.transform, "TerritoryEyebrow", "SEÑAL TERRITORIAL", 15f, TextAlignmentOptions.Left, PoliyoUiTheme.Blue, new Vector2(28f, -24f), new Vector2(260f, 28f), FontStyles.Bold);
        TMP_Text territoryTitle = PoliyoUiTheme.CreateText(territory.transform, "TerritoryTitle", "Sin prioridad seleccionada", 26f, TextAlignmentOptions.TopLeft, PoliyoUiTheme.Ink, new Vector2(28f, -66f), new Vector2(466f, 64f), FontStyles.Bold);
        TMP_Text territoryCopy = PoliyoUiTheme.CreateText(territory.transform, "TerritoryCopy", "El mapa muestra localidades y alcance conocido. No convierte una señal en una certeza.", 18f, TextAlignmentOptions.TopLeft, PoliyoUiTheme.MutedInk, new Vector2(28f, -144f), new Vector2(472f, 70f));

        PoliyoUiTheme.CreateText(campaignDesk.transform, "AgendaHeading", "Agenda inmediata", 26f, TextAlignmentOptions.Left, PoliyoUiTheme.Ink, new Vector2(34f, -518f), new Vector2(340f, 42f), FontStyles.Bold);
        CreateDeskRow(campaignDesk.transform, "DeskRow_0", "09:00", "Revisión territorial", "Sin asignar", -572f, PoliyoUiTheme.Blue);
        CreateDeskRow(campaignDesk.transform, "DeskRow_1", "15:30", "Ventana pública", "Acción disponible", -650f, PoliyoUiTheme.Coral);
        CreateDeskRow(campaignDesk.transform, "DeskRow_2", "20:00", "Cierre y explicación", "Pendiente", -728f, PoliyoUiTheme.Turquoise);
        TMP_Text newsTicker = PoliyoUiTheme.CreateText(campaignDesk.transform, "NewsTicker", "ÚLTIMO PARTE  ·  Todavía no hay noticias registradas.", 15f, TextAlignmentOptions.Left, PoliyoUiTheme.MutedInk, new Vector2(34f, -824f), new Vector2(1070f, 30f), FontStyles.Bold);

        Image pulse = PoliyoUiTheme.CreatePanel(canvas.transform, "NationalPulse", PoliyoUiTheme.Ink);
        PoliyoUiTheme.SetRect(pulse.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(1236f, -170f), new Vector2(640f, 866f));
        PoliyoUiTheme.CreateRule(pulse.transform, "PulseRule", PoliyoUiTheme.Turquoise, Vector2.zero, new Vector2(640f, 10f));
        PoliyoUiTheme.CreateText(pulse.transform, "PulseTitle", "Pulso nacional", 34f, TextAlignmentOptions.Left, PoliyoUiTheme.Ivory, new Vector2(34f, -36f), new Vector2(400f, 50f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(pulse.transform, "PulseDeck", "Métricas distintas. Fuentes distintas.\nNinguna merece fe ciega.", 18f, TextAlignmentOptions.TopLeft, new Color32(188, 201, 220, 255), new Vector2(36f, -90f), new Vector2(500f, 60f));

        TMP_Text trustLabel = CreateMetricCard(pulse.transform, "TrustMetric", "Confianza_TXT", "Confianza: 0.0", "Vínculo con el candidato", -184f, PoliyoUiTheme.Turquoise);
        TMP_Text intentionLabel = CreateMetricCard(pulse.transform, "VotingMetric", "Intencion_Voto_TXT", "Intención de voto: 0.0", "Preferencia declarada", -302f, PoliyoUiTheme.Coral);
        TMP_Text rejectionLabel = CreateMetricCard(pulse.transform, "RejectionMetric", "Rechazo_TXT", "Rechazo: 0.0", "Resistencia activa", -420f, PoliyoUiTheme.Violet);
        TMP_Text participationLabel = CreateMetricCard(pulse.transform, "TurnoutMetric", "Participacion_TXT", "Participación: 0.0", "Probabilidad de concurrencia", -538f, PoliyoUiTheme.Blue);

        Image fogBanner = PoliyoUiTheme.CreatePanel(pulse.transform, "Niebla_Electoral", PoliyoUiTheme.Violet);
        fogBanner.raycastTarget = false;
        PoliyoUiTheme.SetRect(fogBanner.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -660f), new Vector2(572f, 58f));
        PoliyoUiTheme.CreateText(fogBanner.transform, "FogLabel", "NIEBLA ELECTORAL · LEÉ TENDENCIAS, NO PORCENTAJES", 14f, TextAlignmentOptions.Center, PoliyoUiTheme.White, Vector2.zero, new Vector2(572f, 58f), FontStyles.Bold);
        PoliyoUiTheme.Fill(fogBanner.transform.GetChild(0).GetComponent<RectTransform>());

        Button nextDay = PoliyoUiTheme.CreateButton(pulse.transform, "Siguiente_Dia_BTN", "Cerrar día y continuar", new Vector2(34f, -748f), new Vector2(572f, 68f), PoliyoButtonStyle.Primary, 21f);
        TMP_Text nextDayNote = PoliyoUiTheme.CreateText(pulse.transform, "NextDayNote", "Se guardará el estado al resolver la jornada.", 14f, TextAlignmentOptions.Center, new Color32(169, 184, 207, 255), new Vector2(34f, -824f), new Vector2(572f, 26f));

        Image pressPanel = CreatePressPanel(canvas.transform, out Button pressClose);
        var dashboard = canvas.AddComponent<CampaignSliceDashboardPresenter>();
        dashboard.Configure(
            dayLabel,
            budgetLabel,
            fogBanner.gameObject,
            trustLabel,
            intentionLabel,
            rejectionLabel,
            participationLabel,
            priorityTitle,
            priorityCopy,
            territoryTitle,
            territoryCopy,
            newsTicker,
            nextDayNote,
            pressPanel.gameObject,
            pressClose,
            nextDay);

        UnityEventTools.AddStringPersistentListener(calendar.onClick, navigator.OpenScene, "CampaignCalendar");
        UnityEventTools.AddStringPersistentListener(map.onClick, navigator.OpenScene, "CampaignMap");
        UnityEventTools.AddStringPersistentListener(team.onClick, navigator.OpenScene, "TeamScene");
        UnityEventTools.AddPersistentListener(press.onClick, dashboard.TogglePressPanel);
        UnityEventTools.AddPersistentListener(pressClose.onClick, dashboard.TogglePressPanel);
        UnityEventTools.AddStringPersistentListener(menu.onClick, navigator.OpenScene, "MainMenu");
        UnityEventTools.AddPersistentListener(nextDay.onClick, dashboard.AdvanceDay);

        fogBanner.gameObject.SetActive(false);
        pressPanel.gameObject.SetActive(false);
        PoliyoUiTheme.SetFirstSelected(eventSystem, nextDay);
        Save(scene, CampaignSliceScenePath);
    }

    private static void RebuildTeamScene(CampaignContentDefinition catalog)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        FindOrCreateSessionHost(catalog);
        GameObject canvas = PoliyoUiTheme.CreateCanvas("TeamCanvas", out EventSystem eventSystem);
        PoliyoUiTheme.CreateFullScreenBackground(canvas.transform);
        UiSceneNavigation navigator = canvas.AddComponent<UiSceneNavigation>();
        CampaignChrome chrome = PoliyoUiTheme.CreateCampaignChrome(
            canvas.transform,
            "Equipo de campaña",
            "Elegí con pistas incompletas; asigná con consecuencias claras.",
            CampaignNavigationSelection.Team);
        WireCampaignChrome(chrome, navigator);

        Image roster = PoliyoUiTheme.CreatePanel(canvas.transform, "TeamRoster", PoliyoUiTheme.Paper);
        PoliyoUiTheme.SetRect(roster.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -164f), new Vector2(958f, 696f));
        PoliyoUiTheme.CreateRule(roster.transform, "RosterRule", PoliyoUiTheme.Coral, Vector2.zero, new Vector2(958f, 10f));
        PoliyoUiTheme.CreateText(roster.transform, "RosterTitle", "La mesa chica", 32f, TextAlignmentOptions.Left, PoliyoUiTheme.Ink, new Vector2(28f, -30f), new Vector2(400f, 48f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(roster.transform, "RosterHint", "SELECCIONÁ UN PERFIL · LOS RASGOS DECISIVOS NO ESTÁN COMPLETOS", 14f, TextAlignmentOptions.Left, PoliyoUiTheme.MutedInk, new Vector2(30f, -78f), new Vector2(760f, 26f), FontStyles.Bold);

        string[] roles =
        {
            "Vicepresidencia",
            "Jefatura de campaña",
            "Jefatura de prensa",
            "Vocería",
            "Coordinación territorial",
            "Responsable legal / contable",
            "Consultoría política",
            "Operaciones"
        };
        string[] memberIds =
        {
            "vicepresidencia",
            "jefatura-campana",
            "jefatura-prensa",
            "voceria",
            "coordinacion-territorial",
            "legal-contable",
            "consultoria-politica",
            "jefatura-operaciones"
        };
        string[] clues =
        {
            "PISTA PROTOTIPO · lealtad por validar",
            "PISTA PROTOTIPO · orden por validar",
            "PISTA PROTOTIPO · agenda por validar",
            "PISTA PROTOTIPO · carisma por validar",
            "PISTA PROTOTIPO · red por validar",
            "PISTA PROTOTIPO · riesgo por validar",
            "PISTA PROTOTIPO · lectura por validar",
            "PISTA PROTOTIPO · eficacia por validar"
        };

        var memberButtons = new Button[roles.Length];
        var availabilityLabels = new TMP_Text[roles.Length];
        Button firstMember = null;
        for (var index = 0; index < roles.Length; index++)
        {
            int column = index % 2;
            int row = index / 2;
            Vector2 position = new Vector2(28f + column * 454f, -124f - row * 132f);
            Button member = PoliyoUiTheme.CreateButton(
                roster.transform,
                "TeamMemberButton_" + index,
                roles[index] + "\n" + clues[index],
                position,
                new Vector2(428f, 112f),
                PoliyoButtonStyle.Quiet,
                17f);
            TMP_Text label = member.GetComponentInChildren<TMP_Text>(true);
            label.margin = Vector4.zero;
            label.alignment = TextAlignmentOptions.TopLeft;
            PoliyoUiTheme.SetRect(label.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -10f), new Vector2(392f, 62f));
            availabilityLabels[index] = PoliyoUiTheme.CreateText(
                member.transform,
                "Availability",
                "DISPONIBLE",
                13f,
                TextAlignmentOptions.Left,
                PoliyoUiTheme.Turquoise,
                new Vector2(18f, -80f),
                new Vector2(220f, 22f),
                FontStyles.Bold);
            memberButtons[index] = member;
            if (firstMember == null)
            {
                firstMember = member;
            }
        }

        Image detail = PoliyoUiTheme.CreatePanel(canvas.transform, "TeamMemberDetail", PoliyoUiTheme.Ink);
        PoliyoUiTheme.SetRect(detail.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(1030f, -164f), new Vector2(846f, 696f));
        PoliyoUiTheme.CreateRule(detail.transform, "DetailRule", PoliyoUiTheme.Turquoise, Vector2.zero, new Vector2(846f, 10f));
        PoliyoUiTheme.CreateChip(detail.transform, "InformationQuality", "INFORMACIÓN PARCIAL", PoliyoUiTheme.Yellow, PoliyoUiTheme.Ink, new Vector2(32f, -38f), new Vector2(222f, 34f), 14f);
        PoliyoUiTheme.CreateText(detail.transform, "SelectedMemberName", "Perfil sin identidad revelada", 38f, TextAlignmentOptions.Left, PoliyoUiTheme.Ivory, new Vector2(32f, -94f), new Vector2(700f, 56f), FontStyles.Bold);
        TMP_Text selectedMemberRole = PoliyoUiTheme.CreateText(detail.transform, "SelectedMemberRole", "Rol por seleccionar", 20f, TextAlignmentOptions.Left, PoliyoUiTheme.Turquoise, new Vector2(34f, -148f), new Vector2(520f, 34f), FontStyles.Bold);
        TMP_Text selectedMemberStatus = PoliyoUiTheme.CreateText(detail.transform, "SelectedMemberStatus", "ESTADO · SIN SELECCIÓN", 15f, TextAlignmentOptions.Left, PoliyoUiTheme.Yellow, new Vector2(34f, -198f), new Vector2(520f, 28f), FontStyles.Bold);

        Image clueCard = PoliyoUiTheme.CreatePanel(detail.transform, "SelectedMemberClues", PoliyoUiTheme.InkSoft);
        PoliyoUiTheme.SetRect(clueCard.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -246f), new Vector2(782f, 154f));
        PoliyoUiTheme.CreateText(clueCard.transform, "ClueHeading", "LO QUE CREÉS SABER", 14f, TextAlignmentOptions.Left, PoliyoUiTheme.Blue, new Vector2(22f, -18f), new Vector2(300f, 26f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(clueCard.transform, "ClueBody", "Contenido provisional: el presenter proyectará pistas reales cuando exista un perfil seleccionado.", 19f, TextAlignmentOptions.TopLeft, PoliyoUiTheme.Ivory, new Vector2(22f, -54f), new Vector2(730f, 74f));

        PoliyoUiTheme.CreateText(detail.transform, "TaskHeading", "Asignación", 24f, TextAlignmentOptions.Left, PoliyoUiTheme.Ivory, new Vector2(32f, -426f), new Vector2(300f, 40f), FontStyles.Bold);
        TMP_Text selectedMemberTask = PoliyoUiTheme.CreateText(detail.transform, "SelectedMemberTask", "Sin tarea asignada", 18f, TextAlignmentOptions.Left, new Color32(190, 202, 220, 255), new Vector2(34f, -470f), new Vector2(720f, 32f));
        Button territorialCampaign = PoliyoUiTheme.CreateButton(detail.transform, "TerritorialCampaign", "Campaña territorial", new Vector2(32f, -522f), new Vector2(246f, 58f), PoliyoButtonStyle.SelectedNavigation, 18f);
        Button investigation = PoliyoUiTheme.CreateButton(detail.transform, "Investigation", "Investigación", new Vector2(294f, -522f), new Vector2(236f, 58f), PoliyoButtonStyle.Secondary, 18f);
        Button mediaStatement = PoliyoUiTheme.CreateButton(detail.transform, "MediaStatement", "Declaración a medios", new Vector2(546f, -522f), new Vector2(268f, 58f), PoliyoButtonStyle.Primary, 18f);
        territorialCampaign.navigation = new Navigation { mode = Navigation.Mode.Automatic };
        investigation.navigation = new Navigation { mode = Navigation.Mode.Automatic };
        mediaStatement.navigation = new Navigation { mode = Navigation.Mode.Automatic };
        PoliyoUiTheme.CreateText(detail.transform, "AssignmentNote", "La tarea se ejecutará al cerrar la jornada y quedará registrada en el autosave.", 14f, TextAlignmentOptions.Left, new Color32(165, 181, 204, 255), new Vector2(34f, -610f), new Vector2(750f, 28f), FontStyles.Bold);

        CampaignTeamScreenPresenter teamPresenter = canvas.AddComponent<CampaignTeamScreenPresenter>();
        teamPresenter.Configure(
            memberIds,
            memberButtons,
            availabilityLabels,
            selectedMemberRole,
            selectedMemberTask,
            selectedMemberStatus,
            territorialCampaign,
            investigation,
            mediaStatement);
        for (var index = 0; index < memberButtons.Length; index++)
        {
            UnityEventTools.AddStringPersistentListener(memberButtons[index].onClick, teamPresenter.SelectMember, memberIds[index]);
        }

        UnityEventTools.AddPersistentListener(territorialCampaign.onClick, teamPresenter.AssignTerritorialCampaign);
        UnityEventTools.AddPersistentListener(investigation.onClick, teamPresenter.AssignInvestigation);
        UnityEventTools.AddPersistentListener(mediaStatement.onClick, teamPresenter.AssignMediaStatement);

        Image financing = PoliyoUiTheme.CreatePanel(canvas.transform, "FinancingPanel", PoliyoUiTheme.Ivory);
        PoliyoUiTheme.SetRect(financing.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -890f), new Vector2(1832f, 146f));
        PoliyoUiTheme.CreateRule(financing.transform, "FinancingRule", PoliyoUiTheme.Violet, Vector2.zero, new Vector2(12f, 146f));
        PoliyoUiTheme.CreateText(financing.transform, "FinancingHeading", "Financiamiento e inversores", 23f, TextAlignmentOptions.Left, PoliyoUiTheme.Ink, new Vector2(28f, -22f), new Vector2(360f, 38f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(financing.transform, "FinancingCopy", "Las condiciones decisivas se revelan con información incompleta. No hay acuerdo seleccionado.", 17f, TextAlignmentOptions.Left, PoliyoUiTheme.MutedInk, new Vector2(30f, -66f), new Vector2(730f, 32f));
        PoliyoUiTheme.CreateChip(financing.transform, "InvestorSignal_0", "EJEMPLO UI · CONDICIÓN OCULTA", PoliyoUiTheme.Coral, PoliyoUiTheme.Ink, new Vector2(820f, -34f), new Vector2(300f, 42f), 13f);
        PoliyoUiTheme.CreateChip(financing.transform, "InvestorSignal_1", "EJEMPLO UI · RED TERRITORIAL", PoliyoUiTheme.Turquoise, PoliyoUiTheme.Ink, new Vector2(1138f, -34f), new Vector2(296f, 42f), 13f);
        Button back = PoliyoUiTheme.CreateButton(financing.transform, "BackToCampaignButton", "Volver a la mesa", new Vector2(1470f, -30f), new Vector2(326f, 62f), PoliyoButtonStyle.Secondary, 19f);
        UnityEventTools.AddStringPersistentListener(back.onClick, navigator.OpenScene, "CampaignSlice");

        PoliyoUiTheme.SetFirstSelected(eventSystem, firstMember);
        Save(scene, TeamScenePath);
    }

    private static TMP_Text CreateMetricCard(Transform parent, string cardName, string labelName, string value, string definition, float y, Color accent)
    {
        Image card = PoliyoUiTheme.CreatePanel(parent, cardName, PoliyoUiTheme.InkSoft);
        PoliyoUiTheme.SetRect(card.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, y), new Vector2(572f, 96f));
        PoliyoUiTheme.CreateRule(card.transform, "Accent", accent, Vector2.zero, new Vector2(9f, 96f));
        TMP_Text label = PoliyoUiTheme.CreateText(card.transform, labelName, value, 21f, TextAlignmentOptions.Left, PoliyoUiTheme.Ivory, new Vector2(24f, -16f), new Vector2(510f, 34f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(card.transform, "Definition", definition, 15f, TextAlignmentOptions.Left, new Color32(178, 191, 211, 255), new Vector2(24f, -54f), new Vector2(510f, 28f));
        return label;
    }

    private static Image CreatePressPanel(Transform parent, out Button closeButton)
    {
        Image panel = PoliyoUiTheme.CreatePanel(parent, "Panel_Noticias", PoliyoUiTheme.Ivory, true);
        PoliyoUiTheme.SetRect(panel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(430f, -194f), new Vector2(1400f, 788f));
        var shadow = panel.gameObject.AddComponent<Shadow>();
        shadow.effectColor = PoliyoUiTheme.WithAlpha(PoliyoUiTheme.Ink, 0.5f);
        shadow.effectDistance = new Vector2(12f, -12f);
        PoliyoUiTheme.CreateRule(panel.transform, "PressRule", PoliyoUiTheme.Coral, Vector2.zero, new Vector2(1400f, 14f));
        PoliyoUiTheme.CreateChip(panel.transform, "PressEdition", "SALA DE PRENSA", PoliyoUiTheme.Yellow, PoliyoUiTheme.Ink, new Vector2(42f, -46f), new Vector2(208f, 36f), 14f);
        PoliyoUiTheme.CreateText(panel.transform, "PressHeading", "Lo que dicen de vos", 40f, TextAlignmentOptions.Left, PoliyoUiTheme.Ink, new Vector2(40f, -108f), new Vector2(620f, 58f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(panel.transform, "PressDeck", "Titular, fuente y calidad antes de cualquier interpretación.", 20f, TextAlignmentOptions.Left, PoliyoUiTheme.MutedInk, new Vector2(42f, -166f), new Vector2(780f, 38f));
        closeButton = PoliyoUiTheme.CreateButton(
            panel.transform,
            "ClosePressPanelButton",
            "Cerrar prensa",
            new Vector2(1088f, -42f),
            new Vector2(270f, 54f),
            PoliyoButtonStyle.Quiet,
            18f);

        CreateHeadline(panel.transform, "Headline_0", "EL OBSERVADOR", "La campaña entra en fase de posicionamiento", "CALIDAD MEDIA · HOY 08:10", -236f, PoliyoUiTheme.Coral);
        CreateHeadline(panel.transform, "Headline_1", "RADIO ROSCALIA", "Los territorios reclaman señales más claras", "CALIDAD ALTA · HOY 09:25", -380f, PoliyoUiTheme.Turquoise);
        CreateHeadline(panel.transform, "Headline_2", "CANAL DIGITAL", "La conversación crece, pero el apoyo no es lo mismo", "CALIDAD BAJA · HOY 11:40", -524f, PoliyoUiTheme.Blue);

        Image note = PoliyoUiTheme.CreatePanel(panel.transform, "PressMethodNote", PoliyoUiTheme.Paper);
        PoliyoUiTheme.SetRect(note.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(42f, -688f), new Vector2(1316f, 66f));
        PoliyoUiTheme.CreateText(note.transform, "MethodText", "LECTURA RECOMENDADA · Separá confianza, intención de voto, rechazo y participación antes de reaccionar.", 15f, TextAlignmentOptions.Center, PoliyoUiTheme.Ink, Vector2.zero, new Vector2(1316f, 66f), FontStyles.Bold);
        PoliyoUiTheme.Fill(note.transform.GetChild(0).GetComponent<RectTransform>());
        return panel;
    }

    private static void CreateHeadline(Transform parent, string name, string source, string headline, string metadata, float y, Color accent)
    {
        Image row = PoliyoUiTheme.CreatePanel(parent, name, PoliyoUiTheme.White);
        PoliyoUiTheme.SetRect(row.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(42f, y), new Vector2(1316f, 122f));
        PoliyoUiTheme.CreateRule(row.transform, "Accent", accent, Vector2.zero, new Vector2(12f, 122f));
        PoliyoUiTheme.CreateChip(row.transform, "Source", source, accent, PoliyoUiTheme.Ink, new Vector2(26f, -20f), new Vector2(210f, 30f), 13f);
        PoliyoUiTheme.CreateText(row.transform, "Headline", headline, 23f, TextAlignmentOptions.Left, PoliyoUiTheme.Ink, new Vector2(262f, -18f), new Vector2(980f, 42f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(row.transform, "Metadata", metadata, 14f, TextAlignmentOptions.Left, PoliyoUiTheme.MutedInk, new Vector2(262f, -68f), new Vector2(720f, 26f), FontStyles.Bold);
    }

    private static void CreateDeskRow(Transform parent, string name, string time, string title, string status, float y, Color accent)
    {
        Image row = PoliyoUiTheme.CreatePanel(parent, name, PoliyoUiTheme.Ivory);
        PoliyoUiTheme.SetRect(row.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, y), new Vector2(1096f, 64f));
        PoliyoUiTheme.CreateRule(row.transform, "Accent", accent, Vector2.zero, new Vector2(8f, 64f));
        PoliyoUiTheme.CreateText(row.transform, "Time", time, 16f, TextAlignmentOptions.Left, accent, new Vector2(24f, -18f), new Vector2(92f, 28f), FontStyles.Bold);
        PoliyoUiTheme.CreateText(row.transform, "Title", title, 19f, TextAlignmentOptions.Left, PoliyoUiTheme.Ink, new Vector2(128f, -15f), new Vector2(520f, 32f), FontStyles.Bold);
        PoliyoUiTheme.CreateChip(row.transform, "Status", status.ToUpperInvariant(), PoliyoUiTheme.Paper, PoliyoUiTheme.Ink, new Vector2(820f, -16f), new Vector2(244f, 32f), 13f);
    }

    private static void WireCampaignChrome(CampaignChrome chrome, UiSceneNavigation navigator)
    {
        UnityEventTools.AddStringPersistentListener(chrome.Calendar.onClick, navigator.OpenScene, "CampaignCalendar");
        UnityEventTools.AddStringPersistentListener(chrome.Map.onClick, navigator.OpenScene, "CampaignMap");
        UnityEventTools.AddStringPersistentListener(chrome.Team.onClick, navigator.OpenScene, "TeamScene");
        UnityEventTools.AddStringPersistentListener(chrome.Campaign.onClick, navigator.OpenScene, "CampaignSlice");
    }

    private static CampaignContentDefinition LoadCatalog()
    {
        CampaignContentDefinition catalog = AssetDatabase.LoadAssetAtPath<CampaignContentDefinition>(CampaignCatalogPath);
        if (catalog == null)
        {
            throw new InvalidOperationException("Campaign catalog is missing. Run the content builder before rebuilding UI scenes.");
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
        string[] canonicalOrder =
        {
            MainMenuScenePath,
            CampaignSliceScenePath,
            CalendarScenePath,
            MapScenePath,
            TeamScenePath
        };

        var scenes = new EditorBuildSettingsScene[canonicalOrder.Length];
        for (var index = 0; index < canonicalOrder.Length; index++)
        {
            scenes[index] = new EditorBuildSettingsScene(canonicalOrder[index], true);
        }

        EditorBuildSettings.scenes = scenes;
    }

    private static void Reset(Button button)
    {
        while (button.onClick.GetPersistentEventCount() > 0)
        {
            UnityEventTools.RemovePersistentListener(button.onClick, 0);
        }
    }

    private static GameObject FindRequiredGameObject(Scene scene, string name)
    {
        GameObject result = TryFindGameObject(scene, name);
        if (result == null)
        {
            throw new InvalidOperationException("Missing required UI object: " + name);
        }

        return result;
    }

    private static GameObject TryFindGameObject(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform result = FindDescendant(root.transform, name);
            if (result != null)
            {
                return result.gameObject;
            }
        }

        return null;
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

    private static T TryFindComponent<T>(Scene scene, string name) where T : Component
    {
        GameObject gameObject = TryFindGameObject(scene, name);
        return gameObject == null ? null : gameObject.GetComponent<T>() ?? gameObject.GetComponentInChildren<T>(true);
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
