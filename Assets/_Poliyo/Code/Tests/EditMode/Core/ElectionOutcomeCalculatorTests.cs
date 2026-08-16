using NUnit.Framework;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class ElectionOutcomeCalculatorTests
{
    [Test]
    public void CalculateFirstRound_WithMoreThanFortyFivePercent_ReturnsWinner()
    {
        var tally = Tally(48m, 32m, 20m);

        var outcome = ElectionOutcomeCalculator.CalculateFirstRound(tally, new[] { "a", "b", "c" });

        Assert.That(outcome.WinnerId, Is.EqualTo("a"));
        Assert.That(outcome.RequiresRunoff, Is.False);
    }

    [Test]
    public void CalculateFirstRound_WithFortyPercentAndTenPointLead_ReturnsWinner()
    {
        var tally = Tally(40m, 30m, 30m);

        var outcome = ElectionOutcomeCalculator.CalculateFirstRound(tally, new[] { "a", "b", "c" });

        Assert.That(outcome.WinnerId, Is.EqualTo("a"));
    }

    [Test]
    public void CalculateFirstRound_WithoutVictoryRule_ReturnsTwoFinalists()
    {
        var tally = Tally(39m, 33m, 28m);

        var outcome = ElectionOutcomeCalculator.CalculateFirstRound(tally, new[] { "a", "b", "c" });

        Assert.That(outcome.RequiresRunoff, Is.True);
        Assert.That(outcome.RunoffFirstId, Is.EqualTo("a"));
        Assert.That(outcome.RunoffSecondId, Is.EqualTo("b"));
    }

    private static ElectionTally Tally(decimal a, decimal b, decimal c)
    {
        var tally = new ElectionTally();
        tally.AddCandidateVotes("a", a);
        tally.AddCandidateVotes("b", b);
        tally.AddCandidateVotes("c", c);
        return tally;
    }
}

}
