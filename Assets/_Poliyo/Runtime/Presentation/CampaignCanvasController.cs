using System;
using System.Collections.Generic;
using System.Text;
using Poliyo.Application;
using Poliyo.Content;
using Poliyo.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Poliyo.Presentation
{
/// <summary>UGUI presentation controller with explicit scene references and persistent button events.</summary>
public sealed class CampaignCanvasController : MonoBehaviour
{
    [SerializeField, Tooltip("Accepts either legacy UGUI Text or TextMeshPro text.")] private Component _dayLabel;
    [SerializeField, Tooltip("Accepts either legacy UGUI Text or TextMeshPro text.")] private Component _fundsLabel;
    [SerializeField, Tooltip("Accepts either legacy UGUI Text or TextMeshPro text.")] private Component _fogLabel;
    [SerializeField, Tooltip("Accepts either legacy UGUI Text or TextMeshPro text.")] private Component _phaseLabel;
    [SerializeField, Tooltip("Accepts either legacy UGUI Text or TextMeshPro text.")] private Component _screenTitle;
    [SerializeField, Tooltip("Accepts either legacy UGUI Text or TextMeshPro text.")] private Component _screenDescription;
    [SerializeField, Tooltip("Accepts either legacy UGUI Text or TextMeshPro text.")] private Component _statusLabel;
    [SerializeField, Tooltip("Accepts either legacy UGUI Text or TextMeshPro text.")] private Component _summaryLabel;
    [SerializeField, Tooltip("Accepts either legacy UGUI Text or TextMeshPro text.")] private Component _newsLabel;
    [SerializeField] private Button _nextDayButton;
    [SerializeField] private bool _logUiInteractions = true;

    private CampaignRuntime _runtime;
    private CampaignTeam _team;
    private CampaignContentDefinition _contentCatalog;
    private IReadOnlyList<MicroElector> _electorate;
    private NewsMemory _news;
    private Func<CampaignDayAdvanceResult> _advanceDay;
    private Func<CampaignActivity, string, CampaignActionResolution> _resolveAction;
    private Action<string, DelegatedTaskType, string> _assignTeamTask;

    public void Configure(
        Component dayLabel,
        Component fundsLabel,
        Component fogLabel,
        Component phaseLabel,
        Component screenTitle,
        Component screenDescription,
        Component statusLabel,
        Component summaryLabel,
        Component newsLabel,
        Button nextDayButton)
    {
        _dayLabel = dayLabel;
        _fundsLabel = fundsLabel;
        _fogLabel = fogLabel;
        _phaseLabel = phaseLabel;
        _screenTitle = screenTitle;
        _screenDescription = screenDescription;
        _statusLabel = statusLabel;
        _summaryLabel = summaryLabel;
        _newsLabel = newsLabel;
        _nextDayButton = nextDayButton;
    }

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
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        _team = team ?? throw new ArgumentNullException(nameof(team));
        _contentCatalog = contentCatalog;
        _electorate = electorate ?? throw new ArgumentNullException(nameof(electorate));
        _news = news ?? throw new ArgumentNullException(nameof(news));
        _advanceDay = advanceDay ?? throw new ArgumentNullException(nameof(advanceDay));
        _resolveAction = resolveAction ?? throw new ArgumentNullException(nameof(resolveAction));
        _assignTeamTask = assignTeamTask ?? throw new ArgumentNullException(nameof(assignTeamTask));
        ValidateReferences();
        Refresh();
        ShowCalendar();
    }

    public void ShowCalendar()
    {
        TraceInteraction("Calendario");
        SetScreen(
            "Calendario",
            $"Día {_runtime.State.Calendar.CurrentDay} de {CampaignCalendar.TotalCampaignDays}. " +
            "Una actividad pública por día y hasta tres por semana. Las tareas delegadas se resuelven al avanzar.");
        SetStatus("Mesa de campaña lista para decidir la jornada.");
    }

    public void ShowMap()
    {
        TraceInteraction("Mapa");
        var text = new StringBuilder("Elegí una prioridad territorial. En esta primera vista, las actividades mantienen alcance nacional.\n\n");
        if (_contentCatalog != null)
        {
            foreach (LocalityDefinition locality in _contentCatalog.Localities)
            {
                if (locality != null)
                {
                    text.Append(locality.JurisdictionId).Append(": ").Append(locality.DisplayName).Append('\n');
                }
            }
        }

        SetScreen("Mapa de Roscalia", text.ToString());
        SetStatus("El mapa contiene las 24 localidades del vertical slice.");
    }

    public void ShowTeam()
    {
        TraceInteraction("Equipo");
        var text = new StringBuilder();
        foreach (CampaignTeamMember member in _team.Members.Values)
        {
            text.Append(GetRoleName(member.RoleId)).Append(": ")
                .Append(member.IsAvailable ? "disponible" : "ocupado")
                .Append('\n');
        }

        SetScreen("Equipo", text.ToString());
        SetStatus("Las tareas se asignan a través de los botones de acción de esta primera interfaz.");
    }

    public void ShowPress()
    {
        TraceInteraction("Prensa");
        var text = new StringBuilder();
        for (var index = _news.Items.Count - 1; index >= 0; index--)
        {
            NewsItem item = _news.Items[index];
            text.Append("Día ").Append(item.Day).Append(" · ").Append(item.TopicId)
                .Append(" · intensidad ").Append(item.CurrentIntensity.ToString("0.00"))
                .Append('\n');
        }

        SetScreen("Prensa", text.Length == 0 ? "Todavía no hay noticias. Las actividades y tareas generarán titulares." : text.ToString());
        SetStatus("Las consecuencias de campaña se conservan en la memoria de noticias.");
    }

    public void ResolveRally()
    {
        TraceInteraction("Acto");
        ResolveAction(CampaignActivity.Rally);
    }

    public void ResolveInterview()
    {
        TraceInteraction("Entrevista");
        ResolveAction(CampaignActivity.Interview);
    }

    public void ResolveNegotiation()
    {
        TraceInteraction("Negociación");
        ResolveAction(CampaignActivity.Negotiation);
    }

    public void AdvanceDay()
    {
        TraceInteraction("Siguiente día");
        try
        {
            CampaignDayAdvanceResult result = _advanceDay();
            Refresh();
            if (result.ElectionResult != null)
            {
                ShowElectionResult(result.ElectionResult);
            }
            else
            {
                SetStatus(result.CompletedTaskCauses.Count > 0 ? "Jornada resuelta: una tarea delegada produjo consecuencias." : "Jornada resuelta.");
            }
        }
        catch (InvalidOperationException exception)
        {
            SetStatus(exception.Message);
        }
    }

    public void AssignTerritorialTask()
    {
        TraceInteraction("Asignar tarea territorial");
        foreach (CampaignTeamMember member in _team.Members.Values)
        {
            if (!member.IsAvailable)
            {
                continue;
            }

            _assignTeamTask(member.Id, DelegatedTaskType.TerritorialCampaign, "nacional");
            SetStatus($"Tarea territorial asignada a {GetRoleName(member.RoleId)}. Se resolverá al finalizar la jornada.");
            return;
        }

        SetStatus("No hay integrantes disponibles para una tarea territorial.");
    }

    private void ResolveAction(CampaignActivity activity)
    {
        try
        {
            CampaignActionResolution result = _resolveAction(activity, null);
            Refresh();
            SetStatus(result.WasPaid ? $"{GetActivityName(activity)} resuelto." : $"{GetActivityName(activity)} no pudo pagarse.");
        }
        catch (InvalidOperationException exception)
        {
            SetStatus(exception.Message);
        }
    }

    private void ShowElectionResult(CampaignElectionResult result)
    {
        SetScreen("Elección", result.Outcome.RequiresRunoff ? "La campaña continúa hacia el balotaje." : $"Ganador: {result.Outcome.WinnerId}.");
        _nextDayButton.interactable = false;
    }

    private void Refresh()
    {
        CampaignCalendar calendar = _runtime.State.Calendar;
        SetText(_dayLabel, $"Día {calendar.CurrentDay} · Semana {calendar.CurrentWeek}");
        SetText(_fundsLabel, $"Fondos: ${_runtime.Economy.Funds:0}");
        SetText(_fogLabel, calendar.IsElectoralFogActive ? "Niebla Electoral" : "Información abierta");
        SetText(_phaseLabel, _runtime.PhaseMachine.Current.ToString());
        SetText(_summaryLabel, calendar.IsElectoralFogActive ? "Confianza nacional: tendencia reservada" : $"Confianza nacional: {GetNationalTrust():0.0}");
        SetText(_newsLabel, _news.Items.Count == 0 ? "Noticias: sin novedades" : $"Noticias: {_news.Items[_news.Items.Count - 1].TopicId}");
        _nextDayButton.interactable = !calendar.IsElectionDay;
    }

    private decimal GetNationalTrust()
    {
        decimal weight = 0m;
        decimal trust = 0m;
        foreach (MicroElector elector in _electorate)
        {
            if (elector.Candidates.TryGetValue(CampaignCandidateIds.Player, out CandidateElectoralState state))
            {
                weight += elector.ElectoralWeight;
                trust += state.Trust * elector.ElectoralWeight;
            }
        }

        return weight == 0m ? 0m : trust / weight;
    }

    private void SetScreen(string title, string description)
    {
        SetText(_screenTitle, title);
        SetText(_screenDescription, description);
    }

    private void SetStatus(string message)
    {
        SetText(_statusLabel, message);
    }

    private static void SetText(Component textComponent, string value)
    {
        switch (textComponent)
        {
            case Text legacyText:
                legacyText.text = value;
                return;
            case TMP_Text textMeshPro:
                textMeshPro.text = value;
                return;
            default:
                throw new InvalidOperationException($"Unsupported text component '{textComponent.name}'.");
        }
    }

    private void TraceInteraction(string interaction)
    {
        if (_logUiInteractions)
        {
            Debug.Log($"Poliyo UI: se recibió la interacción '{interaction}'.", this);
        }
    }

    private void ValidateReferences()
    {
        ValidateTextReference(_dayLabel, nameof(_dayLabel));
        ValidateTextReference(_fundsLabel, nameof(_fundsLabel));
        ValidateTextReference(_fogLabel, nameof(_fogLabel));
        ValidateTextReference(_phaseLabel, nameof(_phaseLabel));
        ValidateTextReference(_screenTitle, nameof(_screenTitle));
        ValidateTextReference(_screenDescription, nameof(_screenDescription));
        ValidateTextReference(_statusLabel, nameof(_statusLabel));
        ValidateTextReference(_summaryLabel, nameof(_summaryLabel));
        ValidateTextReference(_newsLabel, nameof(_newsLabel));

        if (_nextDayButton == null)
        {
            throw new InvalidOperationException("CampaignCanvasController is missing the Next Day Button reference.");
        }
    }

    private static void ValidateTextReference(Component textComponent, string fieldName)
    {
        if (textComponent == null)
        {
            throw new InvalidOperationException($"CampaignCanvasController is missing the {fieldName} reference.");
        }

        if (textComponent is not Text && textComponent is not TMP_Text)
        {
            throw new InvalidOperationException($"CampaignCanvasController field {fieldName} must reference UGUI Text or TMP_Text.");
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
}
}
