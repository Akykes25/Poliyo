using System;
using System.Collections.Generic;
using Poliyo.Simulation;

namespace Poliyo.Application
{
/// <summary>Read model returned by one deterministic campaign-day transition.</summary>
public sealed class CampaignDayAdvanceResult
{
    public CampaignDayAdvanceResult(
        MonthlyCloseResult monthlyClose,
        IReadOnlyList<CauseRecord> completedTaskCauses,
        CampaignElectionResult electionResult,
        bool electionUnavailable)
        : this(monthlyClose, completedTaskCauses, Array.Empty<CauseRecord>(), electionResult, electionUnavailable)
    {
    }

    public CampaignDayAdvanceResult(
        MonthlyCloseResult monthlyClose,
        IReadOnlyList<CauseRecord> completedTaskCauses,
        IReadOnlyList<CauseRecord> deferredConsequenceCauses,
        CampaignElectionResult electionResult,
        bool electionUnavailable)
    {
        MonthlyClose = monthlyClose;
        CompletedTaskCauses = completedTaskCauses;
        DeferredConsequenceCauses = deferredConsequenceCauses;
        ElectionResult = electionResult;
        ElectionUnavailable = electionUnavailable;
    }

    public MonthlyCloseResult MonthlyClose { get; }
    public IReadOnlyList<CauseRecord> CompletedTaskCauses { get; }
    public IReadOnlyList<CauseRecord> DeferredConsequenceCauses { get; }
    public CampaignElectionResult ElectionResult { get; }
    public bool ElectionUnavailable { get; }
}
}
