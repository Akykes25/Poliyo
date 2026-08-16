using NUnit.Framework;
using Poliyo.Core;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class VerticalSliceElectorateFactoryTests
{
    [Test]
    public void Create_WithSameSeedCreatesSameElectoralState()
    {
        var localities = new[]
        {
            new LocalityElectorateSeed("one", 100),
            new LocalityElectorateSeed("two", 50),
        };

        var first = VerticalSliceElectorateFactory.Create(new CampaignSeed(42UL), localities);
        var second = VerticalSliceElectorateFactory.Create(new CampaignSeed(42UL), localities);

        Assert.That(first, Has.Count.EqualTo(2));
        Assert.That(first[0].Participation, Is.EqualTo(second[0].Participation));
        Assert.That(first[0].GetCandidate(CampaignCandidateIds.Player).VotingIntention,
            Is.EqualTo(second[0].GetCandidate(CampaignCandidateIds.Player).VotingIntention));
    }
}
}
