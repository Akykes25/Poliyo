using System;
using System.Collections.Generic;
using Poliyo.Core;
using Poliyo.Simulation;

namespace Poliyo.Application
{
/// <summary>Coordinates the campaign's explicit time flow without placing rules in Unity callbacks.</summary>
public sealed class CampaignRuntime
{
    private readonly IReadOnlyList<MonthlyCommitment> _monthlyCommitments;

    public CampaignRuntime(CampaignSeed seed, decimal initialFunds, IReadOnlyList<MonthlyCommitment> monthlyCommitments)
        : this(
            new CampaignState(seed),
            new CampaignEconomy(initialFunds),
            new CampaignPhaseMachine(),
            monthlyCommitments)
    {
    }

    private CampaignRuntime(
        CampaignState state,
        CampaignEconomy economy,
        CampaignPhaseMachine phaseMachine,
        IReadOnlyList<MonthlyCommitment> monthlyCommitments)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (economy == null) throw new ArgumentNullException(nameof(economy));
        if (phaseMachine == null) throw new ArgumentNullException(nameof(phaseMachine));
        if (monthlyCommitments == null) throw new ArgumentNullException(nameof(monthlyCommitments));

        State = state;
        Economy = economy;
        PhaseMachine = phaseMachine;
        _monthlyCommitments = monthlyCommitments;
    }

    public CampaignState State { get; }
    public CampaignEconomy Economy { get; }
    public CampaignPhaseMachine PhaseMachine { get; }

    public static CampaignRuntime Restore(CampaignSaveData saveData, IReadOnlyList<MonthlyCommitment> monthlyCommitments)
    {
        CampaignSaveMapper.Validate(saveData);
        CampaignPhase phase = CampaignSaveMapper.GetPhase(saveData);
        return new CampaignRuntime(
            new CampaignState(CampaignSaveMapper.GetSeed(saveData), saveData.CurrentDay),
            new CampaignEconomy(saveData.Funds, saveData.UnpaidObligations),
            CampaignPhaseMachine.Restore(phase),
            monthlyCommitments);
    }

    public void StartCampaign()
    {
        PhaseMachine.MoveTo(CampaignPhase.WeeklyMeeting);
        PhaseMachine.MoveTo(CampaignPhase.Planning);
    }

    public MonthlyCloseResult AdvanceDay()
    {
        if (PhaseMachine.Current != CampaignPhase.Planning && PhaseMachine.Current != CampaignPhase.DailyResolution && PhaseMachine.Current != CampaignPhase.ElectoralFog)
        {
            throw new InvalidOperationException("A campaign day can only advance from planning or daily resolution.");
        }

        CampaignCalendar calendar = State.Calendar;
        if (calendar.IsElectionDay)
        {
            PhaseMachine.MoveTo(CampaignPhase.ElectoralBan);
            return null;
        }

        PhaseMachine.MoveTo(CampaignPhase.DailyResolution);
        calendar.AdvanceDay();

        MonthlyCloseResult close = null;
        if (calendar.CurrentDay == 30 || calendar.CurrentDay == 60)
        {
            close = MonthlyCloseProcessor.Process(calendar.CurrentDay, Economy, _monthlyCommitments);
        }

        if (calendar.IsElectionDay)
        {
            PhaseMachine.MoveTo(CampaignPhase.ElectoralBan);
            PhaseMachine.MoveTo(CampaignPhase.ElectionDay);
        }
        else if (calendar.IsElectoralFogActive)
        {
            PhaseMachine.MoveTo(CampaignPhase.ElectoralFog);
        }
        else if (calendar.IsCampaignMeetingDay)
        {
            PhaseMachine.MoveTo(CampaignPhase.WeeklyMeeting);
            PhaseMachine.MoveTo(CampaignPhase.Planning);
        }
        else
        {
            PhaseMachine.MoveTo(CampaignPhase.Planning);
        }

        return close;
    }
}
}