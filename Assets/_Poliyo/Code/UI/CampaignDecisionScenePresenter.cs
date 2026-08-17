using System;
using System.Collections.Generic;
using Poliyo.Content;
using Poliyo.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Poliyo.Presentation
{
/// <summary>Shared decision-scene presenter for authored public, meeting and crisis decisions.</summary>
public sealed class CampaignDecisionScenePresenter : MonoBehaviour
{
    private enum TimerMode
    {
        Normal,
        Extended,
        VeryExtended,
        Disabled,
    }

    [SerializeField] private CampaignActivity _activity;
    [SerializeField] private TMP_Text _sceneTitle;
    [SerializeField] private TMP_Text _contextLabel;
    [SerializeField] private TMP_Text _actorLabel;
    [SerializeField] private TMP_Text _locationLabel;
    [SerializeField] private TMP_Text _stageLabel;
    [SerializeField] private TMP_Text _promptLabel;
    [SerializeField] private TMP_Text _timerLabel;
    [SerializeField] private TMP_Text _timerModeLabel;
    [SerializeField] private TMP_Text _costLabel;
    [SerializeField] private TMP_Text _riskLabel;
    [SerializeField] private TMP_Text _reactionLabel;
    [SerializeField] private TMP_Text _statusLabel;
    [SerializeField] private Button[] _variantButtons = Array.Empty<Button>();
    [SerializeField] private Button[] _responseButtons = Array.Empty<Button>();
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _timerModeButton;
    [SerializeField] private Button _returnButton;
    [SerializeField] private UiSceneNavigation _navigator;
    [SerializeField, Min(5f)] private float _normalTimerSeconds = 45f;

    private CampaignGameSessionHost _host;
    private CampaignDecisionScenarioDefinition _scenario;
    private readonly List<string> _selectedOptionIds = new List<string>();
    private IReadOnlyList<string> _variantIds = Array.Empty<string>();
    private string _selectedLocalityId;
    private int _stepIndex;
    private int _pendingOptionIndex = -1;
    private float _remainingSeconds;
    private TimerMode _timerMode;
    private bool _resolved;

    public void Configure(
        CampaignActivity activity,
        TMP_Text sceneTitle,
        TMP_Text contextLabel,
        TMP_Text actorLabel,
        TMP_Text locationLabel,
        TMP_Text stageLabel,
        TMP_Text promptLabel,
        TMP_Text timerLabel,
        TMP_Text timerModeLabel,
        TMP_Text costLabel,
        TMP_Text riskLabel,
        TMP_Text reactionLabel,
        TMP_Text statusLabel,
        Button[] variantButtons,
        Button[] responseButtons,
        Button confirmButton,
        Button timerModeButton,
        Button returnButton,
        UiSceneNavigation navigator)
    {
        _activity = activity;
        _sceneTitle = sceneTitle;
        _contextLabel = contextLabel;
        _actorLabel = actorLabel;
        _locationLabel = locationLabel;
        _stageLabel = stageLabel;
        _promptLabel = promptLabel;
        _timerLabel = timerLabel;
        _timerModeLabel = timerModeLabel;
        _costLabel = costLabel;
        _riskLabel = riskLabel;
        _reactionLabel = reactionLabel;
        _statusLabel = statusLabel;
        _variantButtons = variantButtons ?? Array.Empty<Button>();
        _responseButtons = responseButtons ?? Array.Empty<Button>();
        _confirmButton = confirmButton;
        _timerModeButton = timerModeButton;
        _returnButton = returnButton;
        _navigator = navigator;
    }

    private void Start()
    {
        _host = CampaignGameSessionHost.Current ?? throw new InvalidOperationException("A decision scene requires a campaign session host.");
        CampaignActivity? pendingActivity = CampaignGameSessionHost.ConsumePendingDecisionActivity();
        if (pendingActivity.HasValue)
        {
            _activity = pendingActivity.Value;
        }

        if (!_host.Session.TeamSelectionCompleted)
        {
            _statusLabel.text = "Confirmá la elección inicial del equipo antes de resolver una escena política.";
            SetInteractable(_confirmButton, false);
            if (_navigator != null)
            {
                _navigator.OpenScene("TeamSelection");
            }

            return;
        }

        _sceneTitle.text = GetActivityTitle(_activity);
        _timerMode = TimerMode.Normal;
        string pendingScenarioId = CampaignGameSessionHost.ConsumePendingDecisionScenarioId();
        LoadVariants();
        RefreshVariantButtons();
        if (_variantIds.Count > 0)
        {
            int selectedVariantIndex = FindVariantIndex(pendingScenarioId);
            SelectVariant(_variantIds[selectedVariantIndex]);
            FocusButton(selectedVariantIndex < _variantButtons.Length ? _variantButtons[selectedVariantIndex] : null);
        }
        else
        {
            _statusLabel.text = "No hay contenido disponible para esta escena.";
            SetInteractable(_confirmButton, false);
        }
        RefreshTimerModeLabel();
    }

    private void Update()
    {
        if (_resolved || _scenario == null || _timerMode == TimerMode.Disabled)
        {
            return;
        }

        _remainingSeconds -= Time.unscaledDeltaTime;
        if (_remainingSeconds <= 0f)
        {
            _remainingSeconds = 0f;
            _statusLabel.text = "Se agotó el tiempo: la campaña toma la salida prudente disponible.";
            _pendingOptionIndex = _pendingOptionIndex < 0 ? 0 : _pendingOptionIndex;
            ConfirmDecision();
        }

        RefreshTimerLabel();
    }

    public void SelectVariant(string variantId)
    {
        if (_host == null || string.IsNullOrWhiteSpace(variantId)) return;
        _selectedLocalityId = _activity == CampaignActivity.Rally ? variantId : null;
        _scenario = _activity == CampaignActivity.Rally
            ? _host.GetDecisionScenario(_activity)
            : _host.GetDecisionScenario(_activity, variantId);
        if (_scenario == null)
        {
            _statusLabel.text = "La escena no encontró un escenario válido.";
            SetInteractable(_confirmButton, false);
            return;
        }

        if (_scenario.Steps == null || _scenario.Steps.Length == 0)
        {
            SetInteractable(_confirmButton, false);
            return;
        }

        _selectedOptionIds.Clear();
        _stepIndex = 0;
        _pendingOptionIndex = -1;
        _resolved = false;
        _contextLabel.text = _scenario.Context + "\n\nSeñal de campaña · " + _scenario.KnownSignal;
        _actorLabel.text = "CONTRAPARTE · " + _scenario.ActorDisplayName;
        _locationLabel.text = _activity == CampaignActivity.Rally ? "LOCALIDAD · " + GetLocalityName(variantId) : "ALCANCE · NACIONAL";
        _statusLabel.text = "Leé la situación. Elegí una respuesta y confirmá para avanzar.";
        _reactionLabel.text = "La reacción se mostrará después de tu decisión; el resultado exacto queda en la simulación.";
        ResetTimer();
        RefreshStep();
    }

    public void SelectVariantAtIndex(int variantIndex)
    {
        if (variantIndex < 0 || variantIndex >= _variantIds.Count) return;
        SelectVariant(_variantIds[variantIndex]);
        FocusButton(variantIndex < _variantButtons.Length ? _variantButtons[variantIndex] : null);
    }

    public void SelectResponseAtIndex(int optionIndex)
    {
        SelectOption(optionIndex);
        FocusButton(optionIndex >= 0 && optionIndex < _responseButtons.Length ? _responseButtons[optionIndex] : null);
    }

    public void SelectOption(int optionIndex)
    {
        if (_scenario == null || _resolved || _stepIndex >= _scenario.Steps.Length) return;
        CampaignDecisionOptionDefinition[] options = _scenario.Steps[_stepIndex].Options;
        if (options == null || optionIndex < 0 || optionIndex >= options.Length || options[optionIndex] == null) return;
        _pendingOptionIndex = optionIndex;
        CampaignDecisionOptionDefinition option = options[optionIndex];
        _riskLabel.text = option.RiskLabel + " · " + option.Description;
        _costLabel.text = FormatCost(GetPreviewCost(optionIndex));
        _reactionLabel.text = FormatPreviewReaction(option);
        _statusLabel.text = "Respuesta seleccionada. Confirmá para fijarla en la memoria de campaña.";
        SetInteractable(_confirmButton, true);
    }

    public void ConfirmDecision()
    {
        if (_scenario == null || _resolved || _pendingOptionIndex < 0) return;
        CampaignDecisionOptionDefinition option = _scenario.Steps[_stepIndex].Options[_pendingOptionIndex];
        _selectedOptionIds.Add(option.Id);
        if (_stepIndex < _scenario.Steps.Length - 1)
        {
            _stepIndex++;
            _pendingOptionIndex = -1;
            ResetTimer();
            RefreshStep();
            FocusButton(_responseButtons.Length > 0 ? _responseButtons[0] : null);
            return;
        }

        try
        {
            CampaignDecisionResolution resolution = _host.ResolveDecision(_activity, _scenario.Id, _selectedOptionIds, _selectedLocalityId);
            if (!resolution.WasResolved)
            {
                _statusLabel.text = "Fondos insuficientes: la escena no consumió el día ni publicó una noticia.";
                _costLabel.text = "COSTO · no confirmado";
                _selectedOptionIds.Clear();
                _stepIndex = 0;
                _pendingOptionIndex = -1;
                RefreshStep();
                return;
            }

            _resolved = true;
            _stageLabel.text = "REACCIÓN · MEMORIA REGISTRADA";
            _promptLabel.text = "La decisión ya forma parte de la campaña.";
            _reactionLabel.text = BuildReactionSummary();
            _statusLabel.text = "Decisión registrada. Sus efectos inmediatos y diferidos quedan trazables en el resumen sistémico.";
            SetInteractable(_confirmButton, false);
            SetInteractable(_returnButton, true);
        }
        catch (InvalidOperationException exception)
        {
            _statusLabel.text = exception.Message;
        }
        catch (ArgumentException exception)
        {
            _statusLabel.text = exception.Message;
        }
    }

    public void CycleTimerMode()
    {
        _timerMode = (TimerMode)(((int)_timerMode + 1) % 4);
        ResetTimer();
        RefreshTimerModeLabel();
    }

    public void ReturnToCalendar()
    {
        if (_navigator != null) _navigator.OpenScene("CampaignCalendar");
    }

    private void RefreshStep()
    {
        if (_scenario == null || _scenario.Steps == null || _scenario.Steps.Length == 0) return;
        CampaignDecisionStepDefinition step = _scenario.Steps[_stepIndex];
        _stageLabel.text = "DECISIÓN " + (_stepIndex + 1) + "/" + _scenario.Steps.Length;
        _promptLabel.text = step.Prompt;
        _costLabel.text = FormatCost(GetPreviewCost(-1));
        _riskLabel.text = "Elegí una opción para conocer su señal de riesgo.";
        _pendingOptionIndex = -1;
        for (var index = 0; index < _responseButtons.Length; index++)
        {
            bool available = step.Options != null && index < step.Options.Length && step.Options[index] != null;
            _responseButtons[index].interactable = available;
            if (available)
            {
                TMP_Text label = _responseButtons[index].GetComponentInChildren<TMP_Text>(true);
                if (label != null) label.text = step.Options[index].Label;
            }
        }

        SetInteractable(_confirmButton, false);
        RefreshTimerLabel();
    }

    private void LoadVariants()
    {
        var ids = new List<string>();
        if (_activity == CampaignActivity.Rally)
        {
            foreach (LocalityDefinition locality in _host.GetSelectedJurisdictionLocalities())
            {
                if (locality != null) ids.Add(locality.Id);
            }

            if (ids.Count == 0 && _host.ContentCatalog != null)
            {
                foreach (LocalityDefinition locality in _host.ContentCatalog.Localities)
                {
                    if (locality != null) ids.Add(locality.Id);
                }
            }
        }
        else
        {
            foreach (CampaignDecisionScenarioDefinition scenario in _host.GetDecisionScenarios(_activity))
            {
                if (scenario != null) ids.Add(scenario.Id);
            }
        }

        _variantIds = ids;
    }

    private void RefreshVariantButtons()
    {
        for (var index = 0; index < _variantButtons.Length; index++)
        {
            bool available = index < _variantIds.Count;
            _variantButtons[index].interactable = available;
            if (!available) continue;
            string id = _variantIds[index];
            TMP_Text label = _variantButtons[index].GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = _activity == CampaignActivity.Rally ? GetLocalityName(id) : GetScenarioName(id);
        }
    }

    private int FindVariantIndex(string scenarioId)
    {
        if (!string.IsNullOrWhiteSpace(scenarioId))
        {
            for (var index = 0; index < _variantIds.Count; index++)
            {
                if (string.Equals(_variantIds[index], scenarioId, StringComparison.Ordinal))
                {
                    return index;
                }
            }
        }

        return 0;
    }

    private decimal GetPreviewCost(int optionIndex)
    {
        decimal cost = _scenario == null ? 0m : ToSimulationDecimal(_scenario.BaseCost);
        if (_scenario == null || _scenario.Steps == null)
        {
            return Math.Max(0m, cost);
        }

        for (var stepIndex = 0; stepIndex < _selectedOptionIds.Count && stepIndex < _scenario.Steps.Length; stepIndex++)
        {
            CampaignDecisionOptionDefinition selectedOption = FindOption(_scenario.Steps[stepIndex], _selectedOptionIds[stepIndex]);
            if (selectedOption != null)
            {
                cost += ToSimulationDecimal(selectedOption.CostModifier);
            }
        }

        if (optionIndex >= 0 && _stepIndex < _scenario.Steps.Length && _scenario.Steps[_stepIndex] != null &&
            _scenario.Steps[_stepIndex].Options != null && optionIndex < _scenario.Steps[_stepIndex].Options.Length &&
            _scenario.Steps[_stepIndex].Options[optionIndex] != null)
        {
            cost += ToSimulationDecimal(_scenario.Steps[_stepIndex].Options[optionIndex].CostModifier);
        }

        return Math.Max(0m, cost);
    }

    private static CampaignDecisionOptionDefinition FindOption(CampaignDecisionStepDefinition step, string optionId)
    {
        if (step == null || step.Options == null) return null;
        foreach (CampaignDecisionOptionDefinition option in step.Options)
        {
            if (option != null && option.Id == optionId) return option;
        }

        return null;
    }

    private string BuildReactionSummary()
    {
        var reactions = new List<string>();
        foreach (string optionId in _selectedOptionIds)
        {
            foreach (CampaignDecisionStepDefinition step in _scenario.Steps)
            {
                if (step == null || step.Options == null) continue;
                foreach (CampaignDecisionOptionDefinition option in step.Options)
                {
                    if (option == null || option.Id != optionId) continue;
                    if (!string.IsNullOrWhiteSpace(option.ImmediateReaction)) reactions.Add("Reacción inmediata · " + option.ImmediateReaction);
                    if (!string.IsNullOrWhiteSpace(option.RivalResponse)) reactions.Add("Rival · " + option.RivalResponse);
                    if (option.DeferredImpactDelta != 0f)
                    {
                        string metric = string.IsNullOrWhiteSpace(option.DeferredMetricId) ? "tablero" : option.DeferredMetricId;
                        reactions.Add("Consecuencia diferida · " + metric + " · día +" + Math.Max(1, option.DeferredDayOffset));
                    }
                }
            }
        }

        return reactions.Count == 0 ? "La decisión quedó registrada y su efecto se explicará en el tablero." : string.Join("\n", reactions.ToArray());
    }

    private static string FormatPreviewReaction(CampaignDecisionOptionDefinition option)
    {
        if (option == null)
        {
            return "La reacción se mostrará después de confirmar.";
        }

        string immediateReaction = string.IsNullOrWhiteSpace(option.ImmediateReaction)
            ? "La campaña conserva esta señal para el cierre."
            : option.ImmediateReaction;
        string rivalResponse = string.IsNullOrWhiteSpace(option.RivalResponse)
            ? string.Empty
            : "\nRival · " + option.RivalResponse;
        return "Reacción prevista · " + immediateReaction + rivalResponse;
    }

    private void ResetTimer()
    {
        _remainingSeconds = GetTimerDuration();
        RefreshTimerLabel();
    }

    private float GetTimerDuration()
    {
        switch (_timerMode)
        {
            case TimerMode.Extended: return _normalTimerSeconds * 1.75f;
            case TimerMode.VeryExtended: return _normalTimerSeconds * 3f;
            case TimerMode.Disabled: return 0f;
            default: return _normalTimerSeconds;
        }
    }

    private void RefreshTimerLabel()
    {
        _timerLabel.text = _timerMode == TimerMode.Disabled ? "TIEMPO · DESACTIVADO" : "TIEMPO · " + Mathf.CeilToInt(_remainingSeconds) + " s";
    }

    private void RefreshTimerModeLabel()
    {
        _timerModeLabel.text = "TIEMPO: " + GetTimerModeName();
    }

    private string GetTimerModeName()
    {
        switch (_timerMode)
        {
            case TimerMode.Extended: return "extendido";
            case TimerMode.VeryExtended: return "muy extendido";
            case TimerMode.Disabled: return "desactivado";
            default: return "normal";
        }
    }

    private string GetLocalityName(string localityId)
    {
        if (_host.ContentCatalog != null)
        {
            foreach (LocalityDefinition locality in _host.ContentCatalog.Localities)
            {
                if (locality != null && locality.Id == localityId) return locality.DisplayName;
            }
        }

        return localityId.Replace('-', ' ');
    }

    private string GetScenarioName(string scenarioId)
    {
        CampaignDecisionScenarioDefinition scenario = _host.GetDecisionScenario(_activity, scenarioId);
        return scenario == null ? scenarioId : scenario.ActorDisplayName;
    }

    private static string GetActivityTitle(CampaignActivity activity)
    {
        switch (activity)
        {
            case CampaignActivity.Rally: return "Acto político";
            case CampaignActivity.Interview: return "Entrevista periodística";
            case CampaignActivity.Negotiation: return "Negociación política";
            case CampaignActivity.WeeklyMeeting: return "Mesa semanal de campaña";
            case CampaignActivity.Crisis: return "Crisis del día";
            default: return activity.ToString();
        }
    }

    private static string FormatCost(decimal cost) => "COSTO PREVISTO · $" + cost.ToString("0");

    private static void SetInteractable(Selectable selectable, bool interactable)
    {
        if (selectable != null) selectable.interactable = interactable;
    }

    private static decimal ToSimulationDecimal(float value)
    {
        return decimal.Round((decimal)value, 4, MidpointRounding.AwayFromZero);
    }

    private static void FocusButton(Button button)
    {
        if (button != null && button.gameObject.activeInHierarchy && button.interactable && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }
}
}
