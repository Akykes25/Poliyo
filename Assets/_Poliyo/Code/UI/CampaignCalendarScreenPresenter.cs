using System;
using Poliyo.Application;
using Poliyo.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Poliyo.Presentation
{
/// <summary>Projects one persistent campaign session into the authored calendar screen.</summary>
public sealed class CampaignCalendarScreenPresenter : MonoBehaviour
{
    [SerializeField] private TMP_Text _dayLabel;
    [SerializeField] private TMP_Text _statusLabel;
    [SerializeField] private TMP_Text _phaseLabel;
    [SerializeField] private TMP_Text _actionCapacityLabel;
    [SerializeField] private Button _rallyButton;
    [SerializeField] private Button _interviewButton;
    [SerializeField] private Button _negotiationButton;
    [SerializeField] private Button _nextDayButton;

    private CampaignGameSessionHost _host;

    public void Configure(TMP_Text dayLabel, TMP_Text statusLabel, Button rallyButton, Button nextDayButton)
    {
        Configure(dayLabel, statusLabel, null, null, rallyButton, null, null, nextDayButton);
    }

    public void Configure(
        TMP_Text dayLabel,
        TMP_Text statusLabel,
        TMP_Text phaseLabel,
        TMP_Text actionCapacityLabel,
        Button rallyButton,
        Button interviewButton,
        Button negotiationButton,
        Button nextDayButton)
    {
        _dayLabel = dayLabel;
        _statusLabel = statusLabel;
        _phaseLabel = phaseLabel;
        _actionCapacityLabel = actionCapacityLabel;
        _rallyButton = rallyButton;
        _interviewButton = interviewButton;
        _negotiationButton = negotiationButton;
        _nextDayButton = nextDayButton;
    }

    private void Start()
    {
        _host = CampaignGameSessionHost.Current ?? throw new InvalidOperationException("Campaign calendar requires a CampaignGameSessionHost.");
        _host.StateChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (_host != null)
        {
            _host.StateChanged -= Refresh;
        }
    }

    public void ResolveRally()
    {
        OpenDecisionScene("PoliticalRally");
    }

    public void ResolveInterview()
    {
        OpenDecisionScene("Interview");
    }

    public void ResolveNegotiation()
    {
        OpenDecisionScene("PoliticalNegotiation");
    }

    private void OpenDecisionScene(string sceneName)
    {
        if (_host != null && !_host.Session.TeamSelectionCompleted)
        {
            _statusLabel.text = "Confirmá la elección inicial del equipo antes de abrir una escena política.";
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    public void AdvanceDay()
    {
        try
        {
            CampaignDayAdvanceResult result = _host.AdvanceDay();
            _statusLabel.text = result.ElectionResult == null
                ? $"Jornada resuelta. Fondos: ${_host.Session.Runtime.Economy.Funds:0}"
                : FormatElectionResult(result.ElectionResult);
        }
        catch (InvalidOperationException exception)
        {
            _statusLabel.text = exception.Message;
        }
    }

    private void ResolveAction(CampaignActivity activity, string displayName)
    {
        try
        {
            _host.ResolveAction(activity);
            _statusLabel.text = $"{displayName} resuelto. Fondos: ${_host.Session.Runtime.Economy.Funds:0}";
        }
        catch (InvalidOperationException exception)
        {
            _statusLabel.text = exception.Message;
        }
    }

    private void Refresh()
    {
        CampaignCalendar calendar = _host.Session.Runtime.State.Calendar;
        _dayLabel.text = $"Semana {calendar.CurrentWeek} · Día {calendar.CurrentDay}";
        _statusLabel.text = _host.Session.ElectionResult == null
            ? $"Fondos: ${_host.Session.Runtime.Economy.Funds:0} · {GetTerritoryStatus()}"
            : FormatElectionResult(_host.Session.ElectionResult);
        SetTextIfPresent(_phaseLabel, "FASE: " + GetPhaseName(_host.Session.Runtime.PhaseMachine.Current).ToUpperInvariant());
        SetTextIfPresent(
            _actionCapacityLabel,
            _host.Session.CanResolvePublicAction ? "1 ACCIÓN DISPONIBLE" : "ACTIVIDAD PÚBLICA CERRADA");

        bool canResolveAction = _host.Session.CanResolvePublicAction;
        SetInteractableIfPresent(_rallyButton, canResolveAction);
        SetInteractableIfPresent(_interviewButton, canResolveAction);
        SetInteractableIfPresent(_negotiationButton, canResolveAction);
        SetInteractableIfPresent(_nextDayButton, calendar.CanAdvance && _host.Session.ElectionResult == null);
    }

    private string GetTerritoryStatus()
    {
        return string.IsNullOrWhiteSpace(_host.SelectedJurisdictionId)
            ? "alcance nacional"
            : "prioridad territorial: " + _host.SelectedJurisdictionId;
    }

    private static string FormatElectionResult(CampaignElectionResult result)
    {
        decimal playerShare = result.Tally.GetValidVoteShare(CampaignCandidateIds.Player);
        if (!result.Outcome.RequiresRunoff)
        {
            return result.Outcome.WinnerId == CampaignCandidateIds.Player
                ? $"Primera vuelta cerrada: ganaste con {playerShare:0.0}% de los votos válidos."
                : $"Primera vuelta cerrada: ganó {GetCandidateName(result.Outcome.WinnerId)}. Tu candidatura obtuvo {playerShare:0.0}%.";
        }

        return $"Habrá balotaje: {GetCandidateName(result.Outcome.RunoffFirstId)} vs. {GetCandidateName(result.Outcome.RunoffSecondId)}. Tu candidatura: {playerShare:0.0}%.";
    }

    private static string GetCandidateName(string candidateId)
    {
        switch (candidateId)
        {
            case CampaignCandidateIds.Player: return "tu candidatura";
            case CampaignCandidateIds.Liberales: return "Liberales";
            case CampaignCandidateIds.Contr: return "CONTR";
            case CampaignCandidateIds.Zurditos: return "Zurditos";
            case CampaignCandidateIds.Federales: return "Federales";
            default: return "una candidatura rival";
        }
    }

    private static string GetPhaseName(Poliyo.Application.CampaignPhase phase)
    {
        switch (phase)
        {
            case Poliyo.Application.CampaignPhase.WeeklyMeeting: return "Mesa semanal";
            case Poliyo.Application.CampaignPhase.Planning: return "Planificación";
            case Poliyo.Application.CampaignPhase.DailyResolution: return "Resolución diaria";
            case Poliyo.Application.CampaignPhase.ElectoralFog: return "Niebla Electoral";
            case Poliyo.Application.CampaignPhase.ElectoralBan: return "Veda electoral";
            case Poliyo.Application.CampaignPhase.ElectionDay: return "Día de Elección";
            case Poliyo.Application.CampaignPhase.Scrutiny: return "Escrutinio";
            case Poliyo.Application.CampaignPhase.Runoff: return "Balotaje";
            case Poliyo.Application.CampaignPhase.Finished: return "Campaña finalizada";
            default: return phase.ToString();
        }
    }

    private static void SetInteractableIfPresent(Selectable selectable, bool interactable)
    {
        if (selectable != null)
        {
            selectable.interactable = interactable;
        }
    }

    private static void SetTextIfPresent(TMP_Text label, string value)
    {
        if (label != null)
        {
            label.text = value;
        }
    }
}
}
