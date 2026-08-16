using System.Collections.Generic;
using NUnit.Framework;
using Poliyo.Application;
using Poliyo.Core;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class CampaignNewsPersistenceTests
{
    [Test]
    public void CreateSaveDataAndRestoreNews_RetainsNewsMemory()
    {
        var commitments = new List<MonthlyCommitment>();
        var runtime = new CampaignRuntime(new CampaignSeed(11UL), 100m, commitments);
        runtime.StartCampaign();
        var sourceNews = new NewsMemory();
        sourceNews.Publish(new NewsItem(
            "activity-rally-1",
            1,
            "activity-Rally",
            "rally",
            CampaignCandidateIds.Player,
            EvidenceQuality.Indication,
            0.65m,
            0.25m,
            0.90m));
        var source = new CampaignSimulationSession(
            runtime,
            new List<MicroElector>(),
            new CampaignTeam(new CampaignTeamMember[0]),
            sourceNews);

        CampaignSaveData saveData = source.CreateSaveData();
        var restoredNews = new NewsMemory();
        var restored = new CampaignSimulationSession(
            CampaignRuntime.Restore(saveData, commitments),
            new List<MicroElector>(),
            new CampaignTeam(new CampaignTeamMember[0]),
            restoredNews);

        restored.RestoreNews(saveData);

        Assert.That(restoredNews.Items, Has.Count.EqualTo(1));
        Assert.That(restoredNews.Items[0].TopicId, Is.EqualTo("activity-Rally"));
        Assert.That(restoredNews.Items[0].CurrentIntensity, Is.EqualTo(0.90m));
    }

    [Test]
    public void CreateForActivity_WhenExpenseCannotBePaid_CreatesNegativeRumor()
    {
        var action = new CampaignActionDefinition(
            "rally",
            CampaignActivity.Rally,
            10m,
            new ElectoralImpact("rally", CampaignCandidateIds.Player, ElectoralMetric.Trust, 1m, 1m, 1m, 1m, 1m, 1m, 1m));
        var resolution = new CampaignActionResolution(false, new List<CauseRecord>());

        NewsItem item = CampaignNewsFactory.CreateForActivity(1, action, resolution);

        Assert.That(item.Evidence, Is.EqualTo(EvidenceQuality.Rumor));
        Assert.That(item.Framing, Is.LessThan(0m));
    }
}
}
