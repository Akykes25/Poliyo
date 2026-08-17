using System;
using Poliyo.Content;
using Poliyo.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Poliyo.Presentation
{
/// <summary>
/// Owns the playable press-room screen. Media selection is content-driven and
/// the selected scenario is handed to the existing interview decision scene.
/// </summary>
public sealed class CampaignPressScreenPresenter : MonoBehaviour
{
    [SerializeField] private TMP_Text _selectedMediaLabel;
    [SerializeField] private TMP_Text _selectedJournalistLabel;
    [SerializeField] private TMP_Text _selectedContextLabel;
    [SerializeField] private TMP_Text _newsLabel;
    [SerializeField] private TMP_Text _statusLabel;
    [SerializeField] private Button[] _mediaButtons = Array.Empty<Button>();
    [SerializeField] private Button _openInterviewButton;
    [SerializeField] private Button _returnButton;
    [SerializeField] private UiSceneNavigation _navigator;

    private CampaignGameSessionHost _host;
    private CampaignDecisionScenarioDefinition[] _scenarios = Array.Empty<CampaignDecisionScenarioDefinition>();
    private int _selectedScenarioIndex = -1;

    public void Configure(
        TMP_Text selectedMediaLabel,
        TMP_Text selectedJournalistLabel,
        TMP_Text selectedContextLabel,
        TMP_Text newsLabel,
        TMP_Text statusLabel,
        Button[] mediaButtons,
        Button openInterviewButton,
        Button returnButton,
        UiSceneNavigation navigator)
    {
        _selectedMediaLabel = selectedMediaLabel;
        _selectedJournalistLabel = selectedJournalistLabel;
        _selectedContextLabel = selectedContextLabel;
        _newsLabel = newsLabel;
        _statusLabel = statusLabel;
        _mediaButtons = mediaButtons ?? Array.Empty<Button>();
        _openInterviewButton = openInterviewButton;
        _returnButton = returnButton;
        _navigator = navigator;
    }

    private void Start()
    {
        _host = CampaignGameSessionHost.Current ?? throw new InvalidOperationException("Prensa requiere una campaña activa.");
        _host.StateChanged += Refresh;
        Refresh();
        if (_scenarios.Length > 0)
        {
            SelectScenarioAtIndex(0);
        }
        else
        {
            SetText(_statusLabel, "No hay entrevistas cargadas en el catálogo de campaña.");
        }
    }

    private void OnDestroy()
    {
        if (_host != null)
        {
            _host.StateChanged -= Refresh;
        }
    }

    public void SelectScenarioAtIndex(int index)
    {
        if (index < 0 || index >= _scenarios.Length || _scenarios[index] == null)
        {
            return;
        }

        _selectedScenarioIndex = index;
        CampaignDecisionScenarioDefinition scenario = _scenarios[index];
        string[] actorParts = SplitActorDisplayName(scenario.ActorDisplayName);
        SetText(_selectedMediaLabel, actorParts[0]);
        SetText(_selectedJournalistLabel, "Periodista · " + actorParts[1]);
        SetText(_selectedContextLabel, scenario.Context + "\n\nSeñal disponible · " + scenario.KnownSignal);
        SetText(_statusLabel, "Medio seleccionado. Abrí la entrevista para leer la primera pregunta y responder.");
        SetInteractable(_openInterviewButton, true);
        FocusButton(index < _mediaButtons.Length ? _mediaButtons[index] : null);
    }

    public void OpenSelectedInterview()
    {
        if (_selectedScenarioIndex < 0 || _selectedScenarioIndex >= _scenarios.Length)
        {
            SetText(_statusLabel, "Seleccioná un medio antes de abrir la entrevista.");
            return;
        }

        if (_host == null || !_host.Session.TeamSelectionCompleted)
        {
            SetText(_statusLabel, "Confirmá primero el equipo inicial de campaña.");
            return;
        }

        CampaignDecisionScenarioDefinition scenario = _scenarios[_selectedScenarioIndex];
        CampaignGameSessionHost.SetPendingDecisionScenario(CampaignActivity.Interview, scenario.Id);
        SceneManager.LoadScene("Interview");
    }

    public void ReturnToCampaign()
    {
        if (_navigator != null)
        {
            _navigator.OpenCampaignDashboard();
        }
        else
        {
            SceneManager.LoadScene("CampaignSlice");
        }
    }

    private void Refresh()
    {
        if (_host == null) return;

        _scenarios = _host.GetDecisionScenarios(CampaignActivity.Interview);
        for (var index = 0; index < _mediaButtons.Length; index++)
        {
            bool available = index < _scenarios.Length && _scenarios[index] != null;
            _mediaButtons[index].interactable = available;
            if (!available) continue;

            TMP_Text label = _mediaButtons[index].GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = SplitActorDisplayName(_scenarios[index].ActorDisplayName)[0];
            }
        }

        if (_host.Session.News.Items.Count == 0)
        {
            SetText(_newsLabel, "Todavía no hay cobertura registrada. Las entrevistas generan memoria de campaña cuando se resuelven.");
        }
        else
        {
            NewsItem latest = _host.Session.News.Items[_host.Session.News.Items.Count - 1];
            SetText(_newsLabel, $"Último parte · día {latest.Day} · {FormatIdentifier(latest.SourceId)} · evidencia {latest.Evidence}");
        }

        bool hasSelection = _selectedScenarioIndex >= 0 && _selectedScenarioIndex < _scenarios.Length;
        SetInteractable(_openInterviewButton, hasSelection);
    }

    private static string[] SplitActorDisplayName(string actorDisplayName)
    {
        if (string.IsNullOrWhiteSpace(actorDisplayName))
        {
            return new[] { "Medio sin nombre", "Periodismo sin firma" };
        }

        int separator = actorDisplayName.IndexOf(" · ", StringComparison.Ordinal);
        if (separator < 0)
        {
            return new[] { actorDisplayName, "Periodista sin firma" };
        }

        return new[]
        {
            actorDisplayName.Substring(0, separator),
            actorDisplayName.Substring(separator + 3),
        };
    }

    private static string FormatIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier)) return "fuente desconocida";
        string value = identifier.Replace('-', ' ');
        return char.ToUpperInvariant(value[0]) + value.Substring(1);
    }

    private static void SetInteractable(Selectable selectable, bool value)
    {
        if (selectable != null) selectable.interactable = value;
    }

    private static void SetText(TMP_Text label, string value)
    {
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
