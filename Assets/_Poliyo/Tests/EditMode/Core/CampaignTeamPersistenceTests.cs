using System.Collections.Generic;
using NUnit.Framework;
using Poliyo.Application;
using Poliyo.Core;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class CampaignTeamPersistenceTests
{
    [Test]
    public void CreateSaveDataAndRestoreTeam_RetainsPendingAssignment()
    {
        var commitments = new List<MonthlyCommitment>();
        var sourceTeam = new CampaignTeam(new[] { new CampaignTeamMember("press", "jefatura-prensa") });
        var sourceRuntime = new CampaignRuntime(new CampaignSeed(7UL), 100m, commitments);
        sourceRuntime.StartCampaign();
        var source = new CampaignSimulationSession(
            sourceRuntime,
            new List<MicroElector>(),
            sourceTeam,
            new NewsMemory());
        source.AssignTeamTask("press", DelegatedTaskType.MediaStatement, "nacional");

        CampaignSaveData saveData = source.CreateSaveData();
        var restoredTeam = new CampaignTeam(new[] { new CampaignTeamMember("press", "jefatura-prensa") });
        CampaignRuntime restoredRuntime = CampaignRuntime.Restore(saveData, commitments);
        var restored = new CampaignSimulationSession(
            restoredRuntime,
            new List<MicroElector>(),
            restoredTeam,
            new NewsMemory());

        restored.RestoreActivityLimits(saveData);
        restored.RestoreTeam(saveData);

        DelegatedTaskAssignment assignment = restoredTeam.Members["press"].CurrentAssignment;
        Assert.That(assignment, Is.Not.Null);
        Assert.That(assignment.TaskType, Is.EqualTo(DelegatedTaskType.MediaStatement));
        Assert.That(assignment.TargetId, Is.EqualTo("nacional"));
    }
}
}
