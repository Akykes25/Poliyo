using System;
using Poliyo.Simulation;
using TMPro;
using UnityEngine;
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
    [SerializeField] private GameObject _newsPanel;
    [SerializeField] private Button _nextDayButton;

    private CampaignGameSessionHost _host;

    public void Configure(
        TMP_Text dayLabel,
        TMP_Text budgetLabel,
        GameObject fogOverlay,
        TMP_Text trustLabel,
        TMP_Text votingIntentionLabel,
        GameObject newsPanel,
        Button nextDayButton)
    {
        _dayLabel = dayLabel;
        _budgetLabel = budgetLabel;
        _fogOverlay = fogOverlay;
        _trustLabel = trustLabel;
        _votingIntentionLabel = votingIntentionLabel;
        _newsPanel = newsPanel;
        _nextDayButton = nextDayButton;
    }

    private void Start()
    {
        _host = CampaignGameSessionHost.Current ?? throw new InvalidOperationException("CampaignSlice requires a CampaignGameSessionHost.");
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

    public void AdvanceDay()
    {
        _host.AdvanceDay();
    }

    public void TogglePressPanel()
    {
        if (_newsPanel != null)
        {
            _newsPanel.SetActive(!_newsPanel.activeSelf);
        }
    }

    private void Refresh()
    {
        CampaignCalendar calendar = _host.Session.Runtime.State.Calendar;
        _dayLabel.text = $"Día {calendar.CurrentDay} · Semana {calendar.CurrentWeek}";
        _budgetLabel.text = $"Presupuesto: ${_host.Session.Runtime.Economy.Funds:0}";
        _fogOverlay.SetActive(calendar.IsElectoralFogActive);
        _trustLabel.text = $"Confianza: {GetNationalMetric(ElectoralMetric.Trust):0.0}";
        _votingIntentionLabel.text = calendar.IsElectoralFogActive
            ? "Intención de voto: tendencia reservada"
            : $"Intención de voto: {GetNationalMetric(ElectoralMetric.VotingIntention):0.0}";
        _nextDayButton.interactable = calendar.CanAdvance;
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
            }
        }

        return totalWeight == 0m ? 0m : total / totalWeight;
    }
}
}
