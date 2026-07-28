using System.Collections.Generic;
using NUnit.Framework;
using Poliyo.Application;
using Poliyo.Core;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class CampaignSaveMapperTests
{
    [Test]
    public void Restore_RetainsSeedDayEconomyAndPhase()
    {
        var commitments = new List<MonthlyCommitment>();
        var runtime = new CampaignRuntime(new CampaignSeed(55UL), 1000m, commitments);
        runtime.StartCampaign();
        runtime.AdvanceDay();
        runtime.Economy.TryPayExpense(2, "activity", 125m);

        CampaignSaveData save = CampaignSaveMapper.Create(runtime);
        CampaignRuntime restored = CampaignRuntime.Restore(save, commitments);

        Assert.That(restored.State.Seed, Is.EqualTo(new CampaignSeed(55UL)));
        Assert.That(restored.State.Calendar.CurrentDay, Is.EqualTo(2));
        Assert.That(restored.Economy.Funds, Is.EqualTo(875m));
        Assert.That(restored.PhaseMachine.Current, Is.EqualTo(CampaignPhase.Planning));
    }

    [Test]
    public void Restore_RejectsUnsupportedSchema()
    {
        var save = new CampaignSaveData
        {
            SchemaVersion = 99,
            CurrentDay = 1,
            Funds = 0m,
            UnpaidObligations = 0m,
            Phase = "Planning",
        };

        Assert.That(() => CampaignRuntime.Restore(save, new List<MonthlyCommitment>()), Throws.TypeOf<System.NotSupportedException>());
    }
}
}
