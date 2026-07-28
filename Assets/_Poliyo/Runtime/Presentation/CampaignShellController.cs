using System;
using System.Collections.Generic;
using Poliyo.Application;
using Poliyo.Content;
using Poliyo.Simulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace Poliyo.Presentation
{
[RequireComponent(typeof(UIDocument))]
public sealed class CampaignShellController : MonoBehaviour
{
    [SerializeField] private UIDocument _document;

    private CampaignRuntime _runtime;
    private CampaignTeam _team;
    private CampaignContentDefinition _contentCatalog;
    private IReadOnlyList<MicroElector> _electorate;
    private NewsMemory _news;
    private Func<CampaignDayAdvanceResult> _advanceDay;
    private Func<CampaignActivity, string, CampaignActionResolution> _resolveAction;
    private Action<string, DelegatedTaskType, string> _assignTeamTask;
    private Label _dayLabel;
    private Label _timeLabel;
    private Label _fundsLabel;
    private Label _fogLabel;
    private Label _phaseLabel;
    private Label _statusLabel;
    private Label _screenTitle;
    private Label _screenDescription;
    private Label _newsHeadline;
    private Label _trustLabel;
    private Label _selectedTerritoryLabel;
    private ScrollView _contextContent;
    private Button _calendarButton;
    private Button _mapButton;
    private Button _teamButton;
    private Button _pressButton;
    private Button _rallyButton;
    private Button _interviewButton;
    private Button _negotiationButton;
    private Button _nextDayButton;
    private string _selectedLocalityId;

    public void Bind(
        CampaignRuntime runtime,
        CampaignTeam team,
        CampaignContentDefinition contentCatalog,
        IReadOnlyList<MicroElector> electorate,
        NewsMemory news,
        Func<CampaignDayAdvanceResult> advanceDay,
        Func<CampaignActivity, string, CampaignActionResolution> resolveAction,
        Action<string, DelegatedTaskType, string> assignTeamTask)
    {
        if (runtime == null) throw new ArgumentNullException(nameof(runtime));
        if (team == null) throw new ArgumentNullException(nameof(team));
        if (electorate == null) throw new ArgumentNullException(nameof(electorate));
        if (news == null) throw new ArgumentNullException(nameof(news));
        if (advanceDay == null) throw new ArgumentNullException(nameof(advanceDay));
        if (resolveAction == null) throw new ArgumentNullException(nameof(resolveAction));
        if (assignTeamTask == null) throw new ArgumentNullException(nameof(assignTeamTask));

        if (_document == null)
        {
            _document = GetComponent<UIDocument>();
        }

        _runtime = runtime;
        _team = team;
        _contentCatalog = contentCatalog;
        _electorate = electorate;
        _news = news;
        _advanceDay = advanceDay;
        _resolveAction = resolveAction;
        _assignTeamTask = assignTeamTask;
        CacheElements(EnsureVisualTree());
        RegisterCallbacks();
        Refresh();
        ShowCalendar();
    }

    private void OnDisable()
    {
        UnregisterCallbacks();
    }

    private VisualElement EnsureVisualTree()
    {
        VisualElement root = _document.rootVisualElement;
        if (root.Q<VisualElement>("campaign-shell") != null)
        {
            return root;
        }

        VisualTreeAsset visualTreeAsset = _document.visualTreeAsset;
        if (visualTreeAsset == null)
        {
            throw new InvalidOperationException("CampaignShell UIDocument has no source visual tree asset.");
        }

        root.Clear();
        visualTreeAsset.CloneTree(root);
        return root;
    }
    private void CacheElements(VisualElement root)
    {
        _dayLabel = RequireElement<Label>(root, "day-label");
        _timeLabel = RequireElement<Label>(root, "time-label");
        _fundsLabel = RequireElement<Label>(root, "funds-label");
        _fogLabel = RequireElement<Label>(root, "fog-label");
        _phaseLabel = RequireElement<Label>(root, "phase-label");
        _statusLabel = RequireElement<Label>(root, "status-label");
        _screenTitle = RequireElement<Label>(root, "screen-title");
        _screenDescription = RequireElement<Label>(root, "screen-description");
        _newsHeadline = RequireElement<Label>(root, "news-headline");
        _trustLabel = RequireElement<Label>(root, "trust-label");
        _selectedTerritoryLabel = RequireElement<Label>(root, "selected-territory-label");
        _contextContent = RequireElement<ScrollView>(root, "context-content");
        _calendarButton = RequireElement<Button>(root, "calendar-button");
        _mapButton = RequireElement<Button>(root, "map-button");
        _teamButton = RequireElement<Button>(root, "team-button");
        _pressButton = RequireElement<Button>(root, "press-button");
        _rallyButton = RequireElement<Button>(root, "rally-button");
        _interviewButton = RequireElement<Button>(root, "interview-button");
        _negotiationButton = RequireElement<Button>(root, "negotiation-button");
        _nextDayButton = RequireElement<Button>(root, "next-day-button");
    }

    private void RegisterCallbacks()
    {
        UnregisterCallbacks();
        _calendarButton.clicked += ShowCalendar;
        _mapButton.clicked += ShowMap;
        _teamButton.clicked += ShowTeam;
        _pressButton.clicked += ShowPress;
        _rallyButton.clicked += ResolveRally;
        _interviewButton.clicked += ResolveInterview;
        _negotiationButton.clicked += ResolveNegotiation;
        _nextDayButton.clicked += AdvanceDay;
    }

    private void UnregisterCallbacks()
    {
        if (_calendarButton != null) _calendarButton.clicked -= ShowCalendar;
        if (_mapButton != null) _mapButton.clicked -= ShowMap;
        if (_teamButton != null) _teamButton.clicked -= ShowTeam;
        if (_pressButton != null) _pressButton.clicked -= ShowPress;
        if (_rallyButton != null) _rallyButton.clicked -= ResolveRally;
        if (_interviewButton != null) _interviewButton.clicked -= ResolveInterview;
        if (_negotiationButton != null) _negotiationButton.clicked -= ResolveNegotiation;
        if (_nextDayButton != null) _nextDayButton.clicked -= AdvanceDay;
    }

    private void ShowCalendar()
    {
        SetScreen("Calendario", "Planificá la jornada y revisá los hitos de la campaña.");
        AddContextCard("Jornada actual", $"Día {_runtime.State.Calendar.CurrentDay} de {CampaignCalendar.TotalCampaignDays}. Semana {_runtime.State.Calendar.CurrentWeek}.");
        AddContextCard("Regla de actividades", "Podés realizar una actividad pública por día y hasta tres por semana. Las tareas del equipo corren en paralelo.");
        AddContextCard("Próximo hito", GetNextCalendarMilestone());
        SetStatus("Mesa de campaña lista para decidir la próxima jornada.");
    }

    private void ShowMap()
    {
        SetScreen("Mapa de Roscalia", "Elegí una localidad para enfocar actos, entrevistas y negociaciones.");
        if (_contentCatalog == null || _contentCatalog.Localities == null || _contentCatalog.Localities.Length == 0)
        {
            AddContextCard("Contenido pendiente", "No hay un catálogo territorial disponible en esta escena.");
            return;
        }

        var localitiesByJurisdiction = new Dictionary<string, List<LocalityDefinition>>();
        foreach (LocalityDefinition locality in _contentCatalog.Localities)
        {
            if (locality == null) continue;
            if (!localitiesByJurisdiction.TryGetValue(locality.JurisdictionId, out List<LocalityDefinition> localities))
            {
                localities = new List<LocalityDefinition>();
                localitiesByJurisdiction.Add(locality.JurisdictionId, localities);
            }

            localities.Add(locality);
        }

        foreach (KeyValuePair<string, List<LocalityDefinition>> jurisdiction in localitiesByJurisdiction)
        {
            var card = CreateContextCard(jurisdiction.Key);
            card.Add(CreateCopyLabel($"{jurisdiction.Value.Count} localidades disponibles"));
            foreach (LocalityDefinition locality in jurisdiction.Value)
            {
                LocalityDefinition capturedLocality = locality;
                var localityButton = new Button(() => SelectLocality(capturedLocality))
                {
                    text = $"{capturedLocality.DisplayName} · peso {capturedLocality.PopulationWeight}",
                };
                localityButton.AddToClassList("locality-button");
                card.Add(localityButton);
            }
        }

        SetStatus(string.IsNullOrWhiteSpace(_selectedLocalityId)
            ? "Sin foco territorial: las actividades impactarán en toda la campaña."
            : "Foco territorial seleccionado: las próximas actividades impactarán en la localidad elegida.");
    }

    private void ShowTeam()
    {
        SetScreen("Equipo", "Asigná una tarea por integrante mientras continuás con la agenda del candidato.");
        foreach (CampaignTeamMember member in _team.Members.Values)
        {
            CampaignTeamMember capturedMember = member;
            var card = CreateContextCard(GetRoleName(capturedMember.RoleId));
            string availability = capturedMember.IsAvailable
                ? "Disponible para una tarea hoy."
                : $"Ocupado: {capturedMember.CurrentAssignment.TaskType} sobre {capturedMember.CurrentAssignment.TargetId}.";
            card.Add(CreateCopyLabel(availability));
            if (capturedMember.IsAvailable)
            {
                var assignButton = new Button(() => AssignDefaultTask(capturedMember))
                {
                    text = "Asignar tarea",
                };
                assignButton.AddToClassList("locality-button");
                card.Add(assignButton);
            }
        }

        AddContextCard("Financiación", "El inversor aparece acá como fuente de compromiso mensual; no ocupa un rol operativo asignable.");
        SetStatus("Las habilidades exactas y rasgos del equipo siguen reservados para la capa de selección narrativa.");
    }

    private void ShowPress()
    {
        SetScreen("Prensa", "Los titulares activos mantienen memoria y pierden intensidad con el paso de los días.");
        AddContextCard("Sala de prensa", "Las entrevistas consumen una actividad pública. Los comunicados delegados no bloquean al candidato.");
        if (_news.Items.Count == 0)
        {
            SetStatus("Todavía no hay noticias publicadas: las próximas consecuencias se registrarán acá.");
            return;
        }

        for (var index = _news.Items.Count - 1; index >= 0; index--)
        {
            NewsItem item = _news.Items[index];
            AddContextCard(GetNewsTitle(item), $"Día {item.Day} · evidencia: {GetEvidenceName(item.Evidence)} · intensidad: {item.CurrentIntensity:0.00}");
        }

        _newsHeadline.text = GetNewsTitle(_news.Items[_news.Items.Count - 1]);
        SetStatus("Las noticias muestran consecuencias resumidas y trazables de la campaña.");
    }

    private void ResolveRally() => ResolveAction(CampaignActivity.Rally);
    private void ResolveInterview() => ResolveAction(CampaignActivity.Interview);
    private void ResolveNegotiation() => ResolveAction(CampaignActivity.Negotiation);

    private void ResolveAction(CampaignActivity activity)
    {
        try
        {
            CampaignActionResolution result = _resolveAction(activity, _selectedLocalityId);
            Refresh();
            string scope = string.IsNullOrWhiteSpace(_selectedLocalityId) ? "alcance nacional" : "foco territorial";
            SetStatus(result.WasPaid
                ? $"{GetActivityName(activity)} resuelto con {scope}; revisá las consecuencias de la jornada."
                : $"{GetActivityName(activity)} no pudo pagarse; se registró una obligación pendiente.");
        }
        catch (InvalidOperationException exception)
        {
            SetStatus(exception.Message);
        }
    }

    public void ShowElectionResult(CampaignElectionResult result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));

        ElectionOutcome outcome = result.Outcome;
        SetScreen(outcome.RequiresRunoff ? "Balotaje" : "Victoria en primera vuelta", outcome.RequiresRunoff
            ? $"Clasifican {GetCandidateName(outcome.RunoffFirstId)} y {GetCandidateName(outcome.RunoffSecondId)}."
            : $"Gana {GetCandidateName(outcome.WinnerId)} con {result.Tally.GetValidVoteShare(outcome.WinnerId):0.0}% de votos válidos.");
        AddContextCard("Escrutinio", "El resultado se calculó a partir del estado electoral acumulado, no mediante un resultado prefijado.");
        _newsHeadline.text = "El escrutinio fue calculado a partir de la campaña acumulada.";
        SetStatus("Resultado electoral disponible.");
        _nextDayButton.SetEnabled(false);
    }

    private void AdvanceDay()
    {
        try
        {
            CampaignDayAdvanceResult result = _advanceDay();
            Refresh();
            if (result.ElectionUnavailable)
            {
                SetStatus("No hay catálogo territorial asignado para resolver la elección.");
            }
            else if (result.ElectionResult != null)
            {
                ShowElectionResult(result.ElectionResult);
            }
            else if (result.CompletedTaskCauses.Count > 0)
            {
                SetStatus("La jornada se resolvió y una tarea delegada produjo nuevas consecuencias.");
            }
            else if (result.MonthlyClose != null)
            {
                SetStatus("Cierre mensual procesado.");
            }
            else
            {
                SetStatus("La jornada fue resuelta.");
            }
        }
        catch (InvalidOperationException exception)
        {
            SetStatus(exception.Message);
        }
    }

    private void SelectLocality(LocalityDefinition locality)
    {
        _selectedLocalityId = locality.Id;
        Refresh();
        SetStatus($"Foco territorial: {locality.DisplayName}. Las próximas actividades usarán esta localidad.");
    }

    private void AssignDefaultTask(CampaignTeamMember member)
    {
        string targetId = string.IsNullOrWhiteSpace(_selectedLocalityId) ? "nacional" : _selectedLocalityId;
        try
        {
            _assignTeamTask(member.Id, GetDefaultTask(member.RoleId), targetId);
            ShowTeam();
            SetStatus($"Tarea de {GetRoleName(member.RoleId)} asignada sobre {targetId}.");
        }
        catch (InvalidOperationException exception)
        {
            SetStatus(exception.Message);
        }
    }

    private void Refresh()
    {
        CampaignCalendar calendar = _runtime.State.Calendar;
        _dayLabel.text = $"Día {calendar.CurrentDay} · Semana {calendar.CurrentWeek}";
        _timeLabel.text = "Mañana";
        _fundsLabel.text = $"Fondos: ${_runtime.Economy.Funds:0}";
        _fogLabel.text = calendar.IsElectoralFogActive ? "Niebla Electoral" : "Información abierta";
        _phaseLabel.text = GetPhaseName(_runtime.PhaseMachine.Current);
        _selectedTerritoryLabel.text = string.IsNullOrWhiteSpace(_selectedLocalityId) ? "Foco: nacional" : $"Foco: {_selectedLocalityId}";
        _nextDayButton.SetEnabled(!_runtime.State.Calendar.IsElectionDay);
        _trustLabel.text = calendar.IsElectoralFogActive
            ? "Confianza nacional: tendencia reservada"
            : $"Confianza nacional: {GetNationalTrust():0.0}";
    }

    private decimal GetNationalTrust()
    {
        decimal totalWeight = 0m;
        decimal totalTrust = 0m;
        foreach (MicroElector elector in _electorate)
        {
            if (elector.Candidates.TryGetValue(CampaignCandidateIds.Player, out CandidateElectoralState playerState))
            {
                totalWeight += elector.ElectoralWeight;
                totalTrust += playerState.Trust * elector.ElectoralWeight;
            }
        }

        return totalWeight == 0m ? 0m : totalTrust / totalWeight;
    }

    private void SetScreen(string title, string description)
    {
        _screenTitle.text = title;
        _screenDescription.text = description;
        _contextContent.Clear();
    }

    private void AddContextCard(string title, string description)
    {
        VisualElement card = CreateContextCard(title);
        card.Add(CreateCopyLabel(description));
    }

    private Label CreateCopyLabel(string text)
    {
        var label = new Label(text);
        label.AddToClassList("context-card-copy");
        return label;
    }
    private VisualElement CreateContextCard(string title)
    {
        var card = new VisualElement();
        card.AddToClassList("context-card");
        var titleLabel = new Label(title);
        titleLabel.AddToClassList("context-card-title");
        card.Add(titleLabel);
        _contextContent.Add(card);
        return card;
    }

    private string GetNextCalendarMilestone()
    {
        CampaignCalendar calendar = _runtime.State.Calendar;
        if (calendar.CurrentDay < 30)
        {
            return "El primer cierre mensual llegará al día 30.";
        }

        if (!calendar.IsElectoralFogActive)
        {
            return "La Niebla Electoral comienza el día 31.";
        }

        return "La elección se resuelve al cierre del día 60.";
    }

    private void SetStatus(string message)
    {
        _statusLabel.text = message;
    }

    private static T RequireElement<T>(VisualElement root, string name) where T : VisualElement
    {
        T element = root.Q<T>(name);
        if (element == null)
        {
            throw new InvalidOperationException($"CampaignShell is missing required element '{name}'.");
        }

        return element;
    }

    private static string GetNewsTitle(NewsItem item)
    {
        switch (item.TopicId)
        {
            case "activity-Rally": return "El acto de campaña gana visibilidad.";
            case "activity-Interview": return "La entrevista instala un nuevo mensaje.";
            case "activity-Negotiation": return "Una negociación reordena vínculos políticos.";
            case "delegated-task": return "El equipo completó una tarea delegada.";
            case "monthly-close": return item.Framing < 0m ? "Las cuentas de campaña generan presión." : "El cierre mensual mantiene en marcha la estructura.";
            default: return "Nueva consecuencia de campaña.";
        }
    }

    private static string GetEvidenceName(EvidenceQuality evidence)
    {
        switch (evidence)
        {
            case EvidenceQuality.Rumor: return "rumor";
            case EvidenceQuality.Indication: return "indicio";
            case EvidenceQuality.Proof: return "prueba";
            default: return evidence.ToString();
        }
    }
    private static string GetActivityName(CampaignActivity activity)
    {
        switch (activity)
        {
            case CampaignActivity.Rally: return "Acto";
            case CampaignActivity.Interview: return "Entrevista";
            case CampaignActivity.Negotiation: return "Negociación";
            default: return activity.ToString();
        }
    }

    private static DelegatedTaskType GetDefaultTask(string roleId)
    {
        switch (roleId)
        {
            case "vicepresidencia": return DelegatedTaskType.PoliticalContact;
            case "jefatura-campana": return DelegatedTaskType.CrisisAnalysis;
            case "jefatura-prensa":
            case "voceria": return DelegatedTaskType.MediaStatement;
            case "coordinacion-territorial": return DelegatedTaskType.TerritorialCampaign;
            case "legal-contable": return DelegatedTaskType.Fundraising;
            case "consultoria-politica": return DelegatedTaskType.CrisisAnalysis;
            case "jefatura-operaciones": return DelegatedTaskType.Investigation;
            default: throw new ArgumentOutOfRangeException(nameof(roleId));
        }
    }

    private static string GetRoleName(string roleId)
    {
        switch (roleId)
        {
            case "vicepresidencia": return "Vicepresidencia";
            case "jefatura-campana": return "Jefe de campaña";
            case "jefatura-prensa": return "Jefe de prensa";
            case "voceria": return "Vocería";
            case "coordinacion-territorial": return "Coordinación territorial";
            case "legal-contable": return "Responsable legal / contable";
            case "consultoria-politica": return "Consultoría política";
            case "jefatura-operaciones": return "Jefe de Operaciones";
            default: return roleId;
        }
    }

    private static string GetPhaseName(CampaignPhase phase)
    {
        switch (phase)
        {
            case CampaignPhase.WeeklyMeeting: return "Mesa semanal";
            case CampaignPhase.Planning: return "Planificación";
            case CampaignPhase.DailyResolution: return "Resolviendo jornada";
            case CampaignPhase.ElectoralFog: return "Niebla Electoral";
            case CampaignPhase.ElectoralBan: return "Veda electoral";
            case CampaignPhase.ElectionDay: return "Día de elección";
            default: return phase.ToString();
        }
    }

    private static string GetCandidateName(string candidateId)
    {
        switch (candidateId)
        {
            case CampaignCandidateIds.Player: return "Tu partido";
            case CampaignCandidateIds.Liberales: return "Liberales";
            case CampaignCandidateIds.Contr: return "CONTR";
            case CampaignCandidateIds.Zurditos: return "Zurditos";
            case CampaignCandidateIds.Federales: return "Federales";
            default: return candidateId;
        }
    }
}
}