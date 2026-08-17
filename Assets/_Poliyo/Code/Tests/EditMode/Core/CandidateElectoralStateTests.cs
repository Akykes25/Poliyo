using NUnit.Framework;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class CandidateElectoralStateTests
{
    [Test]
    public void ConstructorAndMutations_NeverAllowTrustAboveVotingIntention()
    {
        var candidate = new CandidateElectoralState("player", 80m, 25m, 10m);

        Assert.That(candidate.Trust, Is.EqualTo(25m));

        candidate.Apply(ElectoralMetric.Trust, 20m);
        Assert.That(candidate.Trust, Is.EqualTo(25m));

        candidate.Apply(ElectoralMetric.VotingIntention, -20m);
        Assert.That(candidate.Trust, Is.EqualTo(5m));
        Assert.That(candidate.Trust, Is.LessThanOrEqualTo(candidate.VotingIntention));
    }
}
}
