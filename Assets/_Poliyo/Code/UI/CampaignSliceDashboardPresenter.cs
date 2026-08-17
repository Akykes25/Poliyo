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
/// <summary>Projects the persistent campaign session into the existing CampaignSlice dashboard.</summary>
public sealed class CampaignSliceDashboardPresenter : MonoBehaviour
{
    [SerializeField] private TMP_Text _dayLabel;
    [SerializeField] private TMP_Text _budgetLabel;
    [SerializeField] private GameObject _fogOverlay;
    [SerializeField] private TMP_Text _trustLabel;
    [SerializeField] private TMP_Text _votingIntentionLabel;
    [SerializeField] private TMP_Text _rejectionLabel;
    [SerializeField] private TMP_Text _participationLabel;
    [SerializeField] private TMP_Text _priorityTitle;
    [SerializeField] private TMP_Text _priorityCopy;
    [SerializeField] private TMP_Text _territoryTitle;
    [SerializeField] private TMP_Text _territoryCopy;
    [SerializeField] private TMP_Text _newsTicker;
    [SerializeField] private TMP_Text _nextDayNote;
    [SerializeField] private GameObject _newsPanel;
    [SerializeField] private Button _pressCloseButton;
    [SerializeField] private Button _nextDayButton;

    private CampaignGameSessionHost _host;
    private ModalInteractionScope _pressInteractionScope;

    public void Configure(
        TMP_Text dayLabel,
        TMP_Text budgetLabel,
        GameObject fogOverlay,
        TMP_Text trustLabel,
        TMP_Text votingIntentionLabel,
        GameObject newsPanel,
        Button nextDayButton)
    {
        Configure(
            dayLabel,
            budgetLabel,
            fogOverlay,
            trustLabel,
            votingIntentionLabel,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            newsPanel,
            null,
            nextDayButton);
    }

    public void Configure(
        TMP_Text dayLabel,
        TMP_Text budgetLabel,
        GameObject fogOverlay,
        TMP_Text trustLabel,
        TMP_Text votingIntentionLabel,
        TMP_Text rejectionLabel,
        TMP_Text participationLabel,
        TMP_Text priorityTitle,
        TMP_Text priorityCopy,
        TMP_Text territoryTitle,
        TMP_Text territoryCopy,
        TMP_Text newsTicker,
        TMP_Text nextDayNote,
        GameObject newsPanel,
        Button pressCloseButton,
        Button nextDayButton)
    {
        _dayLabel = dayLabel;
        _budgetLabel = budgetLabel;
        _fogOverlay = fogOverlay;
        _trustLabel = trustLabel;
        _votingIntentionLabel = votingIntentionLabel;
        _rejectionLabel = rejectionLabel;
        _participationLabel = participationLabel;
        _priorityTitle = priorityTitle;
        _priorityCopy = priorityCopy;
        _territoryTitle = territoryTitle;
        _territoryCopy = territoryCopy;
        _newsTicker = newsTicker;
        _nextDayNote = nextDayNote;
        _newsPanel = newsPanel;
        _pressCloseButton = pressCloseButton;
        _nextDayButton = nextDayButton;
        _pressInteractionScope = null;
    }

    private void Start()
    {
        _host = CampaignGameSessionHost.Current ?? throw new InvalidOperationException("CampaignSlice requires a CampaignGameSessionHost.");
        _host.StateChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        _pressInteractionScope?.Close();
        if (_host != null)
        {
            _host.StateChanged -= Refresh;
        }
    }

    public void AdvanceDay()
    {
        try
        {
            if (_host.CanResolveWeeklyMeeting)
            {
                OpenDecisionScene(CampaignActivity.WeeklyMeeting);
                return;
            }

            if (_host.CanResolveCrisis)
            {
                OpenDecisionScene(CampaignActivity.Crisis);
                return;
            }

            if (_host.CanResolveSliceClosure)
            {
                CampaignSliceClosureResult closure = _host.ResolveSliceClosure();
                SetTextIfPresent(_nextDayNote, FormatSliceClosure(closure));
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
                SetTextIfPresent(_nextDayNote, "No se pudo abrir el escrutinio: falta el catálogo territorial de la campaña.");
            }
        }
        catch (InvalidOperationException exception)
        {
            SetTextIfPresent(_nextDayNote, exception.Message);
        }
    }

    public void TogglePressPanel()
    {
        if (_newsPanel != null && _newsPanel.activeSelf)
        {
            _pressInteractionScope?.Close();
            _newsPanel.SetActive(false);
            return;
        }

        // Prensa is a navigable screen, not a modal interpretation of the
        // latest headline. Keep the old method name for scene compatibility,
        // but route every new activation to the authored Press scene.
        SceneManager.LoadScene("Press");
    }

    private void Refresh()
    {
        CampaignCalendar calendar = _host.Session.Runtime.State.Calendar;
        _dayLabel.text = $"Día {calendar.CurrentDay} · Semana {calendar.CurrentWeek}";
        _budgetLabel.text = $"Presupuesto: ${_host.Session.Runtime.Economy.Funds:0}";
        bool fogHidesEstimates = ElectoralMetricDisplay.ShouldHideEstimates(calendar);
        _fogOverlay.SetActive(fogHidesEstimates);
        _trustLabel.text = ElectoralMetricDisplay.FormatNational(
            ElectoralMetric.Trust,
            GetNationalMetric(ElectoralMetric.Trust),
            fogHidesEstimates);
        _votingIntentionLabel.text = ElectoralMetricDisplay.FormatNational(
            ElectoralMetric.VotingIntention,
            GetNationalMetric(ElectoralMetric.VotingIntention),
            fogHidesEstimates);
        SetTextIfPresent(
            _rejectionLabel,
            ElectoralMetricDisplay.FormatNational(
                ElectoralMetric.Rejection,
                GetNationalMetric(ElectoralMetric.Rejection),
                fogHidesEstimates));
        SetTextIfPresent(
            _participationLabel,
            ElectoralMetricDisplay.FormatNational(
                ElectoralMetric.Participation,
                GetNationalMetric(ElectoralMetric.Participation),
                fogHidesEstimates));
        RefreshTerritory();
        RefreshNewsTicker();
        RefreshPriority();
        bool requiresPriorityResolution = _host.CanResolveWeeklyMeeting || _host.CanResolveCrisis;
        bool closureReadoutAvailable = _host.CanResolveSliceClosure;
        bool electionReadoutAvailable = calendar.IsElectionDay && _host.Session.ElectionResult == null;
        bool runoffReadoutAvailable = _host.CanResolveRunoff;
        if (_nextDayButton != null)
        {
            _nextDayButton.interactable = requiresPriorityResolution || closureReadoutAvailable || electionReadoutAvailable || runoffReadoutAvailable || (calendar.CanAdvance &&
                _host.Session.ElectionResult == null &&
                _host.Session.SliceClosureResult == null &&
                !requiresPriorityResolution);
        }
        SetButtonLabel(
            _nextDayButton,
            _host.CanResolveWeeklyMeeting ? "Resolver mesa semanal" :
            _host.CanResolveCrisis ? "Resolver crisis" :
            _host.CanResolveSliceClosure ? "Abrir cierre" :
            runoffReadoutAvailable ? "Resolver balotaje" :
            electionReadoutAvailable ? "Abrir escrutinio" : "Cerrar día y continuar");
    }

    private decimal GetNationalMetric(ElectoralMetric metric)
    {
        decimal totalWeight = 0m;
        decimal total = 0m;
        foreach (MicroElector elector in _host.Session.Electorate)
        {
            if (!elector.Candidates.TryGetValue(CampaignCandidateIds.Player, out CandidateElectoralState state))
            {
                continue;
            }

            totalWeight += elector.ElectoralWeight;
            switch (metric)
            {
                case ElectoralMetric.Trust:
                    total += state.Trust * elector.ElectoralWeight;
                    break;
                case ElectoralMetric.VotingIntention:
                    total += state.VotingIntention * elector.ElectoralWeight;
                    break;
                case ElectoralMetric.Rejection:
                    total += state.Rejection * elector.ElectoralWeight;
                    break;
                case ElectoralMetric.Participation:
                    total += elector.Participation * elector.ElectoralWeight;
                    break;
            }
        }

        return totalWeight == 0m ? 0m : total / totalWeight;
    }

    private ModalInteractionScope GetPressInteractionScope()
    {
        return _pressInteractionScope ??= new ModalInteractionScope(_newsPanel, _pressCloseButton);
    }

    private void RefreshPriority()
    {
        CampaignElectionResult electionResult = _host.Session.ElectionResult;
        CampaignSliceClosureResult closure = _host.Session.SliceClosureResult;
        if (_host.CanResolveRunoff)
        {
            SetTextIfPresent(_priorityTitle, "Balotaje: quedan dos partidos");
            SetTextIfPresent(
                _priorityCopy,
                $"La primera vuelta dejó a {GetCandidateName(_host.Session.ActiveElectionCandidateIds[0])} y " +
                $"{GetCandidateName(_host.Session.ActiveElectionCandidateIds[1])}. La mesa vuelve a enfocarse sólo en ellos.");
            SetTextIfPresent(_nextDayNote, "Acción requerida · resolvé el balotaje para cerrar la campaña.");
            return;
        }

        if (electionResult != null)
        {
            decimal playerShare = electionResult.Tally.GetValidVoteShare(CampaignCandidateIds.Player);
            SetTextIfPresent(_priorityTitle, electionResult.Outcome.RequiresRunoff ? "Balotaje confirmado" : "Primera vuelta resuelta");
            SetTextIfPresent(_priorityCopy, FormatElectionResult(electionResult, playerShare));
            SetTextIfPresent(_nextDayNote, "El resultado quedó guardado y puede reproducirse desde el autosave.");
            return;
        }

        if (closure != null)
        {
            decimal playerShare = closure.Tally.GetValidVoteShare(CampaignCandidateIds.Player);
            SetTextIfPresent(_priorityTitle, "Cierre del slice");
            SetTextIfPresent(_priorityCopy, FormatSliceClosure(closure) + $" Tu candidatura: {playerShare:0.0}%.");
            SetTextIfPresent(_nextDayNote, "El cierre queda persistido; esta prueba de 14 días termina aquí.");
            return;
        }

        if (_host.CanResolveWeeklyMeeting)
        {
            SetTextIfPresent(_priorityTitle, "Resolver la mesa semanal");
            SetTextIfPresent(_priorityCopy, "La semana terminó. Abrí la mesa para ver el diagnóstico, las preguntas y las respuestas antes de continuar.");
            SetTextIfPresent(_nextDayNote, "Acción requerida · el botón principal abre la Mesa semanal.");
            return;
        }

        if (_host.CanResolveCrisis)
        {
            SetTextIfPresent(_priorityTitle, "Resolver la crisis abierta");
            SetTextIfPresent(_priorityCopy, "La información es incompleta: cada respuesta cambia el tablero ahora y deja una consecuencia diferida.");
            SetTextIfPresent(_nextDayNote, "La crisis se registra como una decisión, no como un botón de impacto fijo.");
            return;
        }

        if (_host.CanResolveSliceClosure)
        {
            SetTextIfPresent(_priorityTitle, "Abrir cierre del slice");
            SetTextIfPresent(_priorityCopy, "Ya están reunidas las decisiones mínimas. Abrí el cierre para ver el escrutinio simulado y los factores que explican el resultado.");
            SetTextIfPresent(_nextDayNote, "Acción requerida · el botón principal abre el cierre causal.");
            return;
        }

        if (_host.Session.CanResolvePublicAction)
        {
            SetTextIfPresent(_priorityTitle, "Definir una acción pública");
            SetTextIfPresent(_priorityCopy, "Elegí territorio, mensaje y exposición desde Calendario. El cierre mostrará el costo y la causa.");
        }
        else
        {
            SetTextIfPresent(_priorityTitle, "Actividad pública ya utilizada");
            SetTextIfPresent(_priorityCopy, "Revisá equipo, territorio y noticias antes de cerrar la jornada.");
        }

        SetTextIfPresent(_nextDayNote, "Se guardará el estado al resolver la jornada.");
    }

    private void OpenDecisionScene(CampaignActivity activity)
    {
        CampaignGameSessionHost.SetPendingDecisionActivity(activity);
        SceneManager.LoadScene("PoliticalRally");
    }

    private void RefreshTerritory()
    {
        if (string.IsNullOrWhiteSpace(_host.SelectedJurisdictionId))
        {
            SetTextIfPresent(_territoryTitle, "Sin prioridad seleccionada");
            SetTextIfPresent(_territoryCopy, "El mapa muestra localidades y alcance conocido. No convierte una señal en una certeza.");
            return;
        }

        string territoryName = FormatIdentifier(_host.SelectedJurisdictionId);
        SetTextIfPresent(_territoryTitle, "Prioridad: " + territoryName);
        SetTextIfPresent(_territoryCopy, "Las próximas acciones públicas concentrarán su impacto en esta jurisdicción hasta que elijas otra.");
    }

    private void RefreshNewsTicker()
    {
        if (_host.Session.News.Items.Count == 0)
        {
            SetTextIfPresent(_newsTicker, "ÚLTIMO PARTE  ·  Todavía no hay noticias registradas.");
            return;
        }

        NewsItem item = _host.Session.News.Items[_host.Session.News.Items.Count - 1];
        SetTextIfPresent(
            _newsTicker,
            $"ÚLTIMO PARTE  ·  DÍA {item.Day}  ·  {FormatIdentifier(item.SourceId)}  ·  {FormatIdentifier(item.TopicId)}  ·  {GetEvidenceName(item.Evidence)}");
    }

    private static string FormatElectionResult(CampaignElectionResult result, decimal playerShare)
    {
        if (!result.Outcome.RequiresRunoff)
        {
            return result.Outcome.WinnerId == CampaignCandidateIds.Player
                ? $"Ganaste con {playerShare:0.0}% de los votos válidos. El escrutinio se resolvió una sola vez y quedó persistido."
                : $"Ganó {GetCandidateName(result.Outcome.WinnerId)}. Tu candidatura obtuvo {playerShare:0.0}% de los votos válidos.";
        }

        return $"{GetCandidateName(result.Outcome.RunoffFirstId)} y {GetCandidateName(result.Outcome.RunoffSecondId)} pasan al balotaje. Tu candidatura obtuvo {playerShare:0.0}%.";
    }

    private static string FormatSliceClosure(CampaignSliceClosureResult closure)
    {
        string outcome = closure.Outcome.RequiresRunoff
            ? $"balotaje: {GetCandidateName(closure.Outcome.RunoffFirstId)} vs. {GetCandidateName(closure.Outcome.RunoffSecondId)}"
            : "ganador: " + GetCandidateName(closure.Outcome.WinnerId);
        return $"Día {closure.Day} · {outcome} · {closure.DecisionCount} decisiones · {closure.DecisiveFactors.Count} factores causales.";
    }

    private static string GetCandidateName(string candidateId)
    {
        switch (candidateId)
        {
            case CampaignCandidateIds.Player: return "Tu candidatura";
            case CampaignCandidateIds.Liberales: return "Liberales";
            case CampaignCandidateIds.Contr: return "CONTR";
            case CampaignCandidateIds.Zurditos: return "Zurditos";
            case CampaignCandidateIds.Federales: return "Federales";
            default: return "Una candidatura rival";
        }
    }

    private static string GetEvidenceName(EvidenceQuality evidence)
    {
        switch (evidence)
        {
            case EvidenceQuality.Proof: return "EVIDENCIA CONFIRMADA";
            case EvidenceQuality.Indication: return "CALIDAD MEDIA";
            case EvidenceQuality.Rumor: return "CALIDAD BAJA";
            default: return "FUENTE SIN CLASIFICAR";
        }
    }

    private static string FormatIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return "fuente desconocida";
        }

        string value = identifier.Replace('-', ' ');
        return char.ToUpperInvariant(value[0]) + value.Substring(1);
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
}
}
