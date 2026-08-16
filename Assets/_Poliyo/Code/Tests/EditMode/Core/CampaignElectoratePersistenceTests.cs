using System.Collections.Generic;
using NUnit.Framework;
using Poliyo.Application;
using Poliyo.Core;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class CampaignElectoratePersistenceTests
{
    [Test]
    public void CreateAndRestoreElectorate_RetainsAccumulatedElectoralMetrics()
    {
        var runtime = new CampaignRuntime(new CampaignSeed(18UL), 500m, new List<MonthlyCommitment>());
        runtime.StartCampaign();
        var elector = new MicroElector("elector", "locality", 20m, 70m, new[]
        {
            new CandidateElectoralState("player", 55m, 45m, 12m),
            new CandidateElectoralState("rival", 42m, 35m, 20m),
        }, blankVoteIntention: 8m, undecidedIntention: 12m);
        elector.Apply("player", ElectoralMetric.Trust, 5m);

        CampaignSaveData save = CampaignSaveMapper.Create(runtime, new[] { elector });
        IReadOnlyList<MicroElector> restored = CampaignSaveMapper.RestoreElectorate(save);

        Assert.That(restored, Has.Count.EqualTo(1));
        Assert.That(restored[0].LocalityId, Is.EqualTo("locality"));
        Assert.That(restored[0].Participation, Is.EqualTo(70m));
        Assert.That(restored[0].GetCandidate("player").Trust, Is.EqualTo(60m));
        Assert.That(restored[0].BlankVoteIntention, Is.EqualTo(8m));
    }
}
}
