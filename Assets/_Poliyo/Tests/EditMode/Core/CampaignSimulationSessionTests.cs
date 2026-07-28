using System.Collections.Generic;
using NUnit.Framework;
using Poliyo.Application;
using Poliyo.Core;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class CampaignSimulationSessionTests
{
    [Test]
    public void ResolveAction_WhenActivityWasAlreadyUsedToday_RejectsSecondAction()
    {
        var runtime = new CampaignRuntime(new CampaignSeed(3UL), 100m, new List<MonthlyCommitment>());
        runtime.StartCampaign();
        var electorate = new List<MicroElector>
        {
            new MicroElector("elector", "locality", 100m, 100m, new[]
            {
                new CandidateElectoralState(CampaignCandidateIds.Player, 50m, 50m, 0m),
                new CandidateElectoralState(CampaignCandidateIds.Liberales, 50m, 50m, 0m),
            }),
        };
        var session = new CampaignSimulationSession(runtime, electorate, new CampaignTeam(new CampaignTeamMember[0]), new NewsMemory());
        var action = new CampaignActionDefinition(
            "rally",
            CampaignActivity.Rally,
            0m,
            new ElectoralImpact("rally", CampaignCandidateIds.Player, ElectoralMetric.Trust, 1m, 1m, 1m, 1m, 1m, 1m, 1m));

        session.ResolveAction(action, electorate);

        Assert.That(() => session.ResolveAction(action, electorate), Throws.TypeOf<System.InvalidOperationException>());
    }

    [Test]
    public void AdvanceDay_OnElectionDayResolvesElectionExactlyOnce()
    {
        var runtime = new CampaignRuntime(new CampaignSeed(3UL), 100m, new List<MonthlyCommitment>());
        runtime.StartCampaign();
        while (runtime.State.Calendar.CurrentDay < CampaignCalendar.TotalCampaignDays - 1)
        {
            runtime.AdvanceDay();
        }

        var electorate = new List<MicroElector>
        {
            new MicroElector("elector", "locality", 100m, 100m, new[]
            {
                new CandidateElectoralState(CampaignCandidateIds.Player, 50m, 50m, 0m),
                new CandidateElectoralState(CampaignCandidateIds.Liberales, 50m, 20m, 0m),
                new CandidateElectoralState(CampaignCandidateIds.Contr, 50m, 10m, 0m),
                new CandidateElectoralState(CampaignCandidateIds.Zurditos, 50m, 10m, 0m),
                new CandidateElectoralState(CampaignCandidateIds.Federales, 50m, 10m, 0m),
            }),
        };
        var session = new CampaignSimulationSession(runtime, electorate, new CampaignTeam(new CampaignTeamMember[0]), new NewsMemory());

        CampaignDayAdvanceResult result = session.AdvanceDay();

        Assert.That(result.ElectionResult, Is.Not.Null);
        Assert.That(session.ElectionResult, Is.SameAs(result.ElectionResult));
        Assert.That(runtime.State.CauseRecords, Has.Count.EqualTo(CampaignCandidateIds.All.Count));
    }
}
}