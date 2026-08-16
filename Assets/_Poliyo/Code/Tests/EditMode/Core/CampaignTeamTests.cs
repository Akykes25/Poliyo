using NUnit.Framework;
using Poliyo.Core;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class CampaignTeamTests
{
    [Test]
    public void ResolveAssignmentsForDay_ReleasesMembersAndRecordsCause()
    {
        var member = new CampaignTeamMember("press", "press-chief");
        var team = new CampaignTeam(new[] { member });
        var campaign = new CampaignState(new CampaignSeed(9UL));
        team.Assign(1, "press", DelegatedTaskType.MediaStatement, "canal-7");

        var causes = team.ResolveAssignmentsForDay(campaign);

        Assert.That(causes, Has.Count.EqualTo(1));
        Assert.That(member.IsAvailable, Is.True);
        Assert.That(causes[0].Category, Is.EqualTo(CauseCategory.DelegatedTask));
        Assert.That(causes[0].TargetId, Is.EqualTo("canal-7"));
    }

    [Test]
    public void ResolveAssignmentsForDay_DoesNotResolveFutureWork()
    {
        var member = new CampaignTeamMember("territory", "territorial-coordinator");
        var team = new CampaignTeam(new[] { member });
        var campaign = new CampaignState(new CampaignSeed(9UL));
        team.Assign(2, "territory", DelegatedTaskType.TerritorialCampaign, "puerto-alba");

        var causes = team.ResolveAssignmentsForDay(campaign);

        Assert.That(causes, Is.Empty);
        Assert.That(member.IsAvailable, Is.False);
    }
}
}
