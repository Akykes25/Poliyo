using NUnit.Framework;
using Poliyo.Core;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class CampaignActionResolverTests
{
    [Test]
    public void Resolve_WhenAffordablePaysAndAppliesTraceableImpact()
    {
        var state = new CampaignState(new CampaignSeed(1UL));
        var economy = new CampaignEconomy(100m);
        var elector = new MicroElector("elector", "locality", 10m, 70m, new[]
        {
            new CandidateElectoralState("player", 50m, 40m, 10m),
        });
        var impact = new ElectoralImpact("rally", "player", ElectoralMetric.Trust, 4m, 1m, 1m, 1m, 1m, 1m, 1m);
        var action = new CampaignActionDefinition("rally", CampaignActivity.Rally, 25m, impact);

        CampaignActionResolution result = CampaignActionResolver.Resolve(state, economy, action, new[] { elector });

        Assert.That(result.WasPaid, Is.True);
        Assert.That(economy.Funds, Is.EqualTo(75m));
        Assert.That(elector.GetCandidate("player").Trust, Is.EqualTo(54m));
        Assert.That(result.Causes, Has.Count.EqualTo(2));
    }

    [Test]
    public void Resolve_WhenUnaffordableRecordsUnpaidCauseWithoutImpact()
    {
        var state = new CampaignState(new CampaignSeed(1UL));
        var economy = new CampaignEconomy(10m);
        var elector = new MicroElector("elector", "locality", 10m, 70m, new[]
        {
            new CandidateElectoralState("player", 50m, 40m, 10m),
        });
        var impact = new ElectoralImpact("rally", "player", ElectoralMetric.Trust, 4m, 1m, 1m, 1m, 1m, 1m, 1m);
        var action = new CampaignActionDefinition("rally", CampaignActivity.Rally, 25m, impact);

        CampaignActionResolution result = CampaignActionResolver.Resolve(state, economy, action, new[] { elector });

        Assert.That(result.WasPaid, Is.False);
        Assert.That(economy.Funds, Is.EqualTo(10m));
        Assert.That(elector.GetCandidate("player").Trust, Is.EqualTo(50m));
        Assert.That(state.CauseRecords[0].EffectId, Is.EqualTo("unpaid-expense"));
    }
}
}
