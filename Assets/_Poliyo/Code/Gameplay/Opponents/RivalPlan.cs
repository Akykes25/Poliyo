using Poliyo.Simulation;

namespace Poliyo.AI
{
public sealed class RivalPlan
{
    public RivalPlan(CampaignActivity primaryActivity, DelegatedTaskType delegatedTask)
    {
        PrimaryActivity = primaryActivity;
        DelegatedTask = delegatedTask;
    }

    public CampaignActivity PrimaryActivity { get; }
    public DelegatedTaskType DelegatedTask { get; }
}

}
