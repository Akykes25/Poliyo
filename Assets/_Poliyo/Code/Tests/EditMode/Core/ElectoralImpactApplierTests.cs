using NUnit.Framework;
using Poliyo.Core;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class ElectoralImpactApplierTests
{
    [Test]
    public void Apply_MutatesExplicitTargetsAndRecordsExplainableCauses()
    {
        var state = new CampaignState(new CampaignSeed(42UL));
        var elector = new MicroElector("elector", "locality", 10m, 60m, new[]
        {
            new CandidateElectoralState("player", 50m, 40m, 10m),
        });
        var impact = new ElectoralImpact("rally", "player", ElectoralMetric.Trust, 4m, 1m, 0.5m, 1m, 1m, 1m, 1m);

        var causes = ElectoralImpactApplier.Apply(state, impact, new[] { elector }, CauseCategory.Activity);

        Assert.That(elector.GetCandidate("player").Trust, Is.EqualTo(52m));
        Assert.That(causes, Has.Count.EqualTo(1));
        Assert.That(state.CauseRecords, Has.Count.EqualTo(1));
        Assert.That(state.CauseRecords[0].SourceId, Is.EqualTo("rally"));
    }
}
}
