using NUnit.Framework;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class ElectoralImpactTests
{
    [Test]
    public void CalculateDelta_WithFullFactors_ReturnsBaseMagnitude()
    {
        var impact = new ElectoralImpact("acto", "player", ElectoralMetric.Trust, 4m, 1m, 1m, 1m, 1m, 1m, 1m);

        Assert.That(impact.CalculateDelta(), Is.EqualTo(4m));
    }

    [Test]
    public void Apply_RejectionImpact_DoesNotChangeTrustOrIntent()
    {
        var candidate = new CandidateElectoralState("player", 45m, 20m, 10m);
        var elector = new MicroElector("e1", "puerto-alba", 10m, 80m, new[] { candidate });

        elector.Apply("player", ElectoralMetric.Rejection, 15m);

        Assert.That(candidate.Rejection, Is.EqualTo(25m));
        Assert.That(candidate.Trust, Is.EqualTo(45m));
        Assert.That(candidate.VotingIntention, Is.EqualTo(20m));
    }
}

}
