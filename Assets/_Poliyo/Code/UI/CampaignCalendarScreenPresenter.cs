using System;
using Poliyo.Application;
using Poliyo.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    [SerializeField] private Button _weeklyMeetingButton;
    [SerializeField] private Button _nextDayButton;

    private CampaignGameSessionHost _host;
    private bool _weeklyMeetingListenerBound;
    private bool _interviewListenerBound;

    public void Configure(TMP_Text dayLabel, TMP_Text statusLabel, Button rallyButton, Button nextDayButton)
    {
        Configure(dayLabel, statusLabel, null, null, rallyButton, null, null, null, nextDayButton);
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
        Configure(dayLabel, statusLabel, phaseLabel, actionCapacityLabel, rallyButton, interviewButton, negotiationButton, null, nextDayButton);
    }

    public void Configure(
        TMP_Text dayLabel,
        TMP_Text statusLabel,
        TMP_Text phaseLabel,
        TMP_Text actionCapacityLabel,
        Button rallyButton,
        Button interviewButton,
        Button negotiationButton,
        Button weeklyMeetingButton,
        Button nextDayButton)
    {
        _dayLabel = dayLabel;
        _statusLabel = statusLabel;
        _phaseLabel = phaseLabel;
        _actionCapacityLabel = actionCapacityLabel;
        _rallyButton = rallyButton;
        _interviewButton = interviewButton;
        _negotiationButton = negotiationButton;
        _weeklyMeetingButton = weeklyMeetingButton;
        _nextDayButton = nextDayButton;
    }

    private void Start()
    {
        _host = CampaignGameSessionHost.Current ?? throw new InvalidOperationException("Campaign calendar requires a CampaignGameSessionHost.");
        FindAndBindWeeklyMeetingButton();
        FindAndBindInterviewButton();
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
        OpenDecisionScene(CampaignActivity.Rally, "PoliticalRally");
    }

    public void ResolveInterview()
    {
        OpenDecisionScene(CampaignActivity.Interview, "Interview");
    }

    public void ResolveNegotiation()
    {
        OpenDecisionScene(CampaignActivity.Negotiation, "PoliticalNegotiation");
    }

    public void ResolveWeeklyMeetingOrCrisis()
    {
        if (_host == null) return;
        if (_host.CanResolveWeeklyMeeting)
        {
            OpenDecisionScene(CampaignActivity.WeeklyMeeting, "PoliticalRally");
            return;
        }

        if (_host.CanResolveCrisis)
        {
            OpenDecisionScene(CampaignActivity.Crisis, "PoliticalRally");
            return;
        }

        SetTextIfPresent(_statusLabel, "No hay una mesa semanal ni una crisis disponible en esta fase.");
    }

    private void OpenDecisionScene(CampaignActivity activity, string sceneName)
    {
        if (_host != null && !_host.Session.TeamSelectionCompleted)
        {
            _statusLabel.text = "Confirmá la elección inicial del equipo antes de abrir una escena política.";
            return;
        }

        CampaignGameSessionHost.SetPendingDecisionActivity(activity);
        SceneManager.LoadScene(sceneName);
    }

    public void AdvanceDay()
    {
        try
        {
            if (_host.CanResolveSliceClosure)
            {
                CampaignSliceClosureResult closure = _host.ResolveSliceClosure();
                _statusLabel.text = FormatSliceClosure(closure);
                SceneManager.LoadScene("ElectionResult");
                return;
            }

            CampaignDayAdvanceResult result = _host.AdvanceDay();
            if (result.ElectionResult != null)
            {
                SceneManager.LoadScene("ElectionResult");
                return;
            }

            if (result.ElectionUnavailable)
            {
                _statusLabel.text = "No se pudo abrir el escrutinio: falta el catálogo territorial de la campaña.";
                return;
            }

            _statusLabel.text = result.ElectionResult == null
                ? $"Jornada resuelta. Fondos: ${_host.Session.Runtime.Economy.Funds:0} · {GetAdvanceFeedback(result)}"
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
            _host.CanResolveWeeklyMeeting ? "MESA SEMANAL PENDIENTE" :
            _host.CanResolveCrisis ? "CRISIS DISPONIBLE" :
            _host.CanResolveSliceClosure ? "CIERRE DEL SLICE DISPONIBLE" :
            _host.CanResolveRunoff ? "BALOTAJE DISPONIBLE" :
            _host.Session.CanResolvePublicAction ? "1 ACCIÓN DISPONIBLE" : "ACTIVIDAD PÚBLICA CERRADA");

        bool canResolveAction = _host.Session.CanResolvePublicAction;
        SetInteractableIfPresent(_rallyButton, canResolveAction);
        SetInteractableIfPresent(_interviewButton, canResolveAction);
        SetInteractableIfPresent(_negotiationButton, canResolveAction);
        SetWeeklyMeetingButtonState();
        if (_host.CanResolveWeeklyMeeting)
        {
            SetTextIfPresent(_statusLabel, "La semana terminó. Resolvé la Mesa semanal para desbloquear el siguiente día.");
        }
        else if (_host.CanResolveRunoff)
        {
            SetTextIfPresent(_statusLabel, "La primera vuelta terminó. La mesa quedó reducida a tu partido y su rival: resolvé el balotaje.");
        }
        else if (_host.CanResolveSliceClosure)
        {
            SetTextIfPresent(_statusLabel, "El slice ya tiene sus decisiones mínimas. Abrí el cierre para leer el escrutinio y sus causas.");
        }
        bool closureReadoutAvailable = _host.CanResolveSliceClosure;
        SetInteractableIfPresent(
            _nextDayButton,
            (_host.CanResolveRunoff || calendar.CanAdvance || (calendar.IsElectionDay && _host.Session.ElectionResult == null)) &&
            _host.Session.ElectionResult == null &&
            _host.Session.SliceClosureResult == null &&
            (!_host.CanResolveWeeklyMeeting || closureReadoutAvailable));
        if (_host.CanResolveRunoff)
        {
            SetInteractableIfPresent(_nextDayButton, true);
        }
        if (_host.CanResolveSliceClosure)
        {
            SetInteractableIfPresent(_nextDayButton, true);
        }
        SetButtonLabel(
            _nextDayButton,
            _host.CanResolveWeeklyMeeting ? "Resolver mesa" :
            _host.CanResolveCrisis ? "Resolver crisis" :
            _host.CanResolveSliceClosure ? "Abrir cierre" :
            _host.CanResolveRunoff ? "Resolver balotaje" :
            calendar.IsElectionDay ? "Abrir escrutinio" : "Cerrar día y continuar");
    }

    private void FindAndBindWeeklyMeetingButton()
    {
        if (_weeklyMeetingButton == null)
        {
            foreach (Button button in GetComponentsInChildren<Button>(true))
            {
                if (button != null && button.name == "TeamMeetingButton")
                {
                    _weeklyMeetingButton = button;
                    break;
                }
            }
        }

        if (_weeklyMeetingButton != null && !_weeklyMeetingListenerBound)
        {
            _weeklyMeetingListenerBound = HasPersistentWeeklyMeetingListener();
            if (!_weeklyMeetingListenerBound)
            {
                _weeklyMeetingButton.onClick.AddListener(ResolveWeeklyMeetingOrCrisis);
            }

            _weeklyMeetingListenerBound = true;
        }
    }

    private void FindAndBindInterviewButton()
    {
        if (_interviewButton == null || _interviewListenerBound) return;

        int listenerCount = _interviewButton.onClick.GetPersistentEventCount();
        for (var index = 0; index < listenerCount; index++)
        {
            if (_interviewButton.onClick.GetPersistentTarget(index) == this &&
                _interviewButton.onClick.GetPersistentMethodName(index) == nameof(ResolveInterview))
            {
                _interviewListenerBound = true;
                return;
            }
        }

        _interviewButton.onClick.AddListener(ResolveInterview);
        _interviewListenerBound = true;
    }

    private bool HasPersistentWeeklyMeetingListener()
    {
        int listenerCount = _weeklyMeetingButton.onClick.GetPersistentEventCount();
        for (var index = 0; index < listenerCount; index++)
        {
            if (_weeklyMeetingButton.onClick.GetPersistentTarget(index) == this &&
                _weeklyMeetingButton.onClick.GetPersistentMethodName(index) == nameof(ResolveWeeklyMeetingOrCrisis))
            {
                return true;
            }
        }

        return false;
    }

    private void SetWeeklyMeetingButtonState()
    {
        if (_weeklyMeetingButton == null) return;

        bool canResolveMeeting = _host.CanResolveWeeklyMeeting;
        bool canResolveCrisis = _host.CanResolveCrisis;
        _weeklyMeetingButton.interactable = canResolveMeeting || canResolveCrisis;
        TMP_Text label = _weeklyMeetingButton.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.text = canResolveMeeting ? "Resolver mesa" : canResolveCrisis ? "Resolver crisis" : "Mesa / crisis";
        }

        if (canResolveMeeting)
        {
            FocusButton(_weeklyMeetingButton);
        }
    }

    private static string GetAdvanceFeedback(CampaignDayAdvanceResult result)
    {
        if (result.DeferredConsequenceCauses != null && result.DeferredConsequenceCauses.Count > 0)
        {
            return "una consecuencia diferida acaba de hacerse visible";
        }

        if (result.CompletedTaskCauses != null && result.CompletedTaskCauses.Count > 0)
        {
            return "el equipo devolvió una señal al tablero";
        }

        return "sin novedades inmediatas";
    }

    private static string FormatSliceClosure(CampaignSliceClosureResult closure)
    {
        decimal playerShare = closure.Tally.GetValidVoteShare(CampaignCandidateIds.Player);
        string outcome = closure.Outcome.RequiresRunoff
            ? $"balotaje entre {GetCandidateName(closure.Outcome.RunoffFirstId)} y {GetCandidateName(closure.Outcome.RunoffSecondId)}"
            : $"ganó {GetCandidateName(closure.Outcome.WinnerId)}";
        return $"Cierre del slice · día {closure.Day}: {outcome}. Tu candidatura: {playerShare:0.0}% · {closure.DecisiveFactors.Count} factores trazables.";
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
            case Poliyo.Application.CampaignPhase.SliceClosure: return "Cierre del slice";
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

    private static void SetButtonLabel(Button button, string value)
    {
        if (button == null) return;
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null) label.text = value;
    }

    private static void FocusButton(Button button)
    {
        if (button != null && button.interactable && button.gameObject.activeInHierarchy && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }
}
}
