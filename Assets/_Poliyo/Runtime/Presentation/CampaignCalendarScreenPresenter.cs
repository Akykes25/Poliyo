using System;
using Poliyo.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Poliyo.Presentation
{
/// <summary>Projects one persistent campaign session into the authored calendar screen.</summary>
public sealed class CampaignCalendarScreenPresenter : MonoBehaviour
{
    [SerializeField] private TMP_Text _dayLabel;
    [SerializeField] private TMP_Text _statusLabel;
    [SerializeField] private Button _rallyButton;
    [SerializeField] private Button _nextDayButton;

    private CampaignGameSessionHost _host;

    public void Configure(TMP_Text dayLabel, TMP_Text statusLabel, Button rallyButton, Button nextDayButton)
    {
        _dayLabel = dayLabel;
        _statusLabel = statusLabel;
        _rallyButton = rallyButton;
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
        ResolveAction(CampaignActivity.Rally, "Acto");
    }

    public void ResolveInterview()
    {
        ResolveAction(CampaignActivity.Interview, "Entrevista");
    }

    public void ResolveNegotiation()
    {
        ResolveAction(CampaignActivity.Negotiation, "Negociación");
    }

    public void AdvanceDay()
    {
        try
        {
            _host.AdvanceDay();
            _statusLabel.text = $"Jornada resuelta. Fondos: ${_host.Session.Runtime.Economy.Funds:0}";
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
        _statusLabel.text = $"Fondos: ${_host.Session.Runtime.Economy.Funds:0} · {GetTerritoryStatus()}";
        _nextDayButton.interactable = calendar.CanAdvance;
        _rallyButton.interactable = calendar.CanAdvance;
    }
    private string GetTerritoryStatus()
    {
        return string.IsNullOrWhiteSpace(_host.SelectedJurisdictionId)
            ? "alcance nacional"
            : "prioridad territorial: " + _host.SelectedJurisdictionId;
    }
}
}