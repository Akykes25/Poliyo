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
    {
        MonthlyClose = monthlyClose;
        CompletedTaskCauses = completedTaskCauses;
        ElectionResult = electionResult;
        ElectionUnavailable = electionUnavailable;
    }

    public MonthlyCloseResult MonthlyClose { get; }
    public IReadOnlyList<CauseRecord> CompletedTaskCauses { get; }
    public CampaignElectionResult ElectionResult { get; }
    public bool ElectionUnavailable { get; }
}
}
