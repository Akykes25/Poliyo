using NUnit.Framework;

namespace Poliyo.Core.EditModeTests
{
public sealed class CampaignSeedTests
{
    [Test]
    public void Derive_WithSameSeedAndStream_ReturnsSameSeed()
    {
        var campaignSeed = new CampaignSeed(20260725UL);

        var first = campaignSeed.Derive("electorate/locality/puerto-alba");
        var second = campaignSeed.Derive("electorate/locality/puerto-alba");

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public void Derive_WithDifferentStreams_ReturnsDifferentSeeds()
    {
        var campaignSeed = new CampaignSeed(20260725UL);

        var electorate = campaignSeed.Derive("electorate");
        var rivals = campaignSeed.Derive("rivals");

        Assert.That(electorate, Is.Not.EqualTo(rivals));
    }

    [Test]
    public void CreateRandom_WithSameStream_ReplaysSequence()
    {
        var campaignSeed = new CampaignSeed(42UL);
        var first = campaignSeed.CreateRandom("events/day/1");
        var second = campaignSeed.CreateRandom("events/day/1");

        for (var index = 0; index < 16; index++)
        {
            Assert.That(first.NextUInt64(), Is.EqualTo(second.NextUInt64()));
        }
    }
}

}
