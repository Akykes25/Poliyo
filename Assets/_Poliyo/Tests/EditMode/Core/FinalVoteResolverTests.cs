using NUnit.Framework;
using Poliyo.Core;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class FinalVoteResolverTests
{
    private static readonly string[] CandidateIds = { "player", "rival" };
    private static readonly FinalVoteResolutionParameters Parameters =
        new FinalVoteResolutionParameters(0.35m, 0.50m, 0.08m, 0.10m);

    [Test]
    public void CalculateResolved_WithSameStateAndSeed_ReplaysExactly()
    {
        MicroElector elector = CreateElector(50m, 30m);

        ElectionTally first = ElectionTallyCalculator.CalculateResolved(
            new[] { elector }, CandidateIds, new CampaignSeed(42UL), Parameters);
        ElectionTally second = ElectionTallyCalculator.CalculateResolved(
            new[] { elector }, CandidateIds, new CampaignSeed(42UL), Parameters);

        Assert.That(second.GetCandidateVotes("player"), Is.EqualTo(first.GetCandidateVotes("player")));
        Assert.That(second.GetCandidateVotes("rival"), Is.EqualTo(first.GetCandidateVotes("rival")));
        Assert.That(second.BlankVotes, Is.EqualTo(first.BlankVotes));
    }

    [Test]
    public void CalculateResolved_AllocatesAllUndecidedVotes()
    {
        ElectionTally tally = ElectionTallyCalculator.CalculateResolved(
            new[] { CreateElector(50m, 30m) }, CandidateIds, new CampaignSeed(7UL), Parameters);

        Assert.That(tally.UndecidedVotes, Is.Zero);
        Assert.That(tally.ValidVotes + tally.BlankVotes, Is.EqualTo(tally.ParticipatingWeight));
    }

    [Test]
    public void Resolve_AllowsBlankVoteToCompeteForUndecidedShare()
    {
        MicroElector elector = CreateElector(50m, 30m);

        FinalVoteDistribution result = FinalVoteResolver.Resolve(
            elector,
            CandidateIds,
            new CampaignSeed(7UL),
            Parameters);

        Assert.That(result.BlankShare, Is.GreaterThan(elector.BlankVoteIntention));
    }

    [Test]
    public void Resolve_WhenCandidateSetsDiffer_RejectsTheTally()
    {
        MicroElector elector = CreateElector(50m, 30m);

        Assert.That(
            () => FinalVoteResolver.Resolve(elector, new[] { "player", "rival", "extra" }, new CampaignSeed(7UL), Parameters),
            Throws.ArgumentException);
    }

    [Test]
    public void Resolve_IncreasingTrustNeverReducesCandidateShare()
    {
        MicroElector baseline = CreateElector(45m, 30m);
        MicroElector improved = CreateElector(70m, 30m);

        FinalVoteDistribution baselineResult = FinalVoteResolver.Resolve(baseline, CandidateIds, new CampaignSeed(9UL), Parameters);
        FinalVoteDistribution improvedResult = FinalVoteResolver.Resolve(improved, CandidateIds, new CampaignSeed(9UL), Parameters);

        Assert.That(improvedResult.GetCandidateShare("player"), Is.GreaterThan(baselineResult.GetCandidateShare("player")));
    }

    [Test]
    public void Resolve_ReducingRejectionNeverReducesCandidateShare()
    {
        MicroElector baseline = CreateElector(50m, 45m);
        MicroElector improved = CreateElector(50m, 20m);

        FinalVoteDistribution baselineResult = FinalVoteResolver.Resolve(baseline, CandidateIds, new CampaignSeed(9UL), Parameters);
        FinalVoteDistribution improvedResult = FinalVoteResolver.Resolve(improved, CandidateIds, new CampaignSeed(9UL), Parameters);

        Assert.That(improvedResult.GetCandidateShare("player"), Is.GreaterThan(baselineResult.GetCandidateShare("player")));
    }

    private static MicroElector CreateElector(decimal playerTrust, decimal playerRejection)
    {
        return new MicroElector("elector", "capital", 100m, 100m, new[]
        {
            new CandidateElectoralState("player", playerTrust, 42m, playerRejection),
            new CandidateElectoralState("rival", 50m, 38m, 30m),
        }, blankVoteIntention: 8m, undecidedIntention: 12m);
    }
}
}
