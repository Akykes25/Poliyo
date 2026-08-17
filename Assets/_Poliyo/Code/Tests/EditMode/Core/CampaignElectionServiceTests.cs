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

    [Test]
    public void ResolveRunoff_OnElectionDayKeepsOnlyTheTwoFinalists()
    {
        var campaign = new CampaignState(new CampaignSeed(7UL), CampaignCalendar.TotalCampaignDays);
        var elector = new MicroElector("elector", "locality", 100m, 100m, new[]
        {
            new CandidateElectoralState("player", 60m, 40m, 10m),
            new CandidateElectoralState("rival", 30m, 25m, 20m),
            new CandidateElectoralState("eliminated", 20m, 25m, 35m),
        }, blankVoteIntention: 5m, undecidedIntention: 5m);
        var service = new CampaignElectionService(new[] { "player", "rival", "eliminated" });

        CampaignElectionResult result = service.ResolveRunoff(
            campaign,
            new[] { elector },
            new[] { "player", "rival" });

        Assert.That(result.Outcome.WinnerId, Is.Not.Null);
        Assert.That(result.Outcome.RequiresRunoff, Is.False);
        Assert.That(result.Tally.CandidateVotes.Keys, Is.EquivalentTo(new[] { "player", "rival" }));
        Assert.That(campaign.CauseRecords, Has.Count.EqualTo(2));
    }
}
}
