using NUnit.Framework;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class MicroElectorVotingIntentionTests
{
    [Test]
    public void ApplyVotingIntention_RedistributesCandidateSupportWithoutBreakingDistribution()
    {
        var elector = new MicroElector("elector", "locality", 1m, 70m, new[]
        {
            new CandidateElectoralState("player", 50m, 40m, 10m),
            new CandidateElectoralState("rival", 50m, 30m, 10m),
        }, blankVoteIntention: 10m, undecidedIntention: 20m);

        elector.Apply("player", ElectoralMetric.VotingIntention, 10m);

        decimal total = elector.GetCandidate("player").VotingIntention + elector.GetCandidate("rival").VotingIntention;
        Assert.That(total, Is.EqualTo(70m));
        Assert.That(elector.GetCandidate("player").VotingIntention, Is.GreaterThan(40m));
        Assert.That(elector.GetCandidate("rival").VotingIntention, Is.LessThan(30m));
    }
}
}
