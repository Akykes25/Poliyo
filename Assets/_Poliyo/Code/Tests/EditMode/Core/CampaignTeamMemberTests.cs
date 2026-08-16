using System;
using NUnit.Framework;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class CampaignTeamMemberTests
{
    [Test]
    public void Assign_WhenMemberIsAlreadyBusy_ThrowsInvalidOperationException()
    {
        var member = new CampaignTeamMember("press", "press-chief");
        member.Assign(new DelegatedTaskAssignment(1, "press", DelegatedTaskType.MediaStatement, "canal-7"));

        Assert.That(
            () => member.Assign(new DelegatedTaskAssignment(1, "press", DelegatedTaskType.CrisisAnalysis, "crisis-1")),
            Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void ReleaseAssignment_MakesMemberAvailableAgain()
    {
        var member = new CampaignTeamMember("territory", "territorial-coordinator");
        member.Assign(new DelegatedTaskAssignment(1, "territory", DelegatedTaskType.TerritorialCampaign, "puerto-alba"));

        member.ReleaseAssignment();

        Assert.That(member.IsAvailable, Is.True);
    }
}

}
