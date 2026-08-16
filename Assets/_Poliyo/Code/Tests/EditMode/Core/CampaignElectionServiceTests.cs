using NUnit.Framework;
using Poliyo.Core;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class CampaignElectionServiceTests
{
    [Test]
    public void ResolveFirstRound_OnElectionDayCreatesAuditableResult()
    {
        var campaign = new CampaignState(new CampaignSeed(7UL), CampaignCalendar.TotalCampaignDays);
        var elector = new MicroElector("elector", "locality", 100m, 100m, new[]
        {
            new CandidateElectoralState("player", 50m, 50m, 0m),
            new CandidateElectoralState("rival", 50m, 50m, 0m),
        });
        var service = new CampaignElectionService(new[] { "player", "rival" });

        CampaignElectionResult result = service.ResolveFirstRound(campaign, new[] { elector });

        Assert.That(result.Outcome.RequiresRunoff, Is.True);
        Assert.That(campaign.CauseRecords, Has.Count.EqualTo(2));
        Assert.That(campaign.CauseRecords[0].Category, Is.EqualTo(CauseCategory.Election));
    }
}
}
