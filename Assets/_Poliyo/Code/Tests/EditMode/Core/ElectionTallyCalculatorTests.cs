using NUnit.Framework;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class ElectionTallyCalculatorTests
{
    [Test]
    public void Calculate_WeightsVotesByPopulationInsteadOfMicroelectorCount()
    {
        var highWeight = new MicroElector("large", "a", 100m, 100m, new[]
        {
            new CandidateElectoralState("player", 50m, 60m, 0m),
            new CandidateElectoralState("rival", 40m, 40m, 0m),
        });
        var lowWeight = new MicroElector("small", "b", 10m, 100m, new[]
        {
            new CandidateElectoralState("player", 0m, 0m, 0m),
            new CandidateElectoralState("rival", 50m, 100m, 0m),
        });

        var tally = ElectionTallyCalculator.Calculate(new[] { highWeight, lowWeight }, new[] { "player", "rival" });

        Assert.That(tally.GetCandidateVotes("player"), Is.EqualTo(60m));
        Assert.That(tally.GetCandidateVotes("rival"), Is.EqualTo(50m));
        Assert.That(tally.GetValidVoteShare("player"), Is.EqualTo(60m / 110m * 100m));
    }

    [Test]
    public void Calculate_SeparatesBlankAndUndecidedVotesFromValidVotes()
    {
        var elector = new MicroElector("elector", "locality", 100m, 80m, new[]
        {
            new CandidateElectoralState("player", 45m, 45m, 10m),
            new CandidateElectoralState("rival", 35m, 35m, 10m),
        }, blankVoteIntention: 10m, undecidedIntention: 10m);

        var tally = ElectionTallyCalculator.Calculate(new[] { elector }, new[] { "player", "rival" });

        Assert.That(tally.ParticipatingWeight, Is.EqualTo(80m));
        Assert.That(tally.ValidVotes, Is.EqualTo(64m));
        Assert.That(tally.BlankVotes, Is.EqualTo(8m));
        Assert.That(tally.UndecidedVotes, Is.EqualTo(8m));
        Assert.That(tally.GetValidVoteShare("player"), Is.EqualTo(45m / 80m * 100m));
    }

    [Test]
    public void NormalizeDisplayCounts_RoundsWeightedVotesWithoutFractionsAndKeepsTotal()
    {
        var tally = new ElectionTally();
        tally.AddCandidateVotes("player", 203.5m);
        tally.AddCandidateVotes("rival", 102.5m);
        tally.AddBlankVotes(50.5m);
        tally.AddUndecidedVotes(50.5m);
        tally.AddParticipation(407m);

        ElectionVoteCounts normalized = ElectionVoteCountNormalizer.Normalize(tally, new[] { "player", "rival" });

        Assert.That(normalized.GetCandidateVotes("player"), Is.EqualTo(204L));
        Assert.That(normalized.GetCandidateVotes("rival"), Is.EqualTo(103L));
        Assert.That(normalized.ValidVotes, Is.EqualTo(307L));
        Assert.That(normalized.BlankVotes, Is.EqualTo(50L));
        Assert.That(normalized.UndecidedVotes, Is.EqualTo(50L));
        Assert.That(normalized.ParticipatingVotes, Is.EqualTo(407L));
        Assert.That(normalized.ValidVotes + normalized.BlankVotes + normalized.UndecidedVotes,
            Is.EqualTo(normalized.ParticipatingVotes));
    }
}
}
