using System;
using NUnit.Framework;
using Poliyo.Core;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class CampaignStateTests
{
    [Test]
    public void RecordCause_OnCurrentDay_AddsTraceableRecord()
    {
        var state = new CampaignState(new CampaignSeed(7UL));
        var cause = new CauseRecord(1, CauseCategory.Activity, "acto", "puerto-alba", "confidence", 2.5m);

        state.RecordCause(cause);

        Assert.That(state.CauseRecords, Has.Count.EqualTo(1));
        Assert.That(state.CauseRecords[0], Is.SameAs(cause));
    }

    [Test]
    public void RecordCause_OnDifferentDay_ThrowsInvalidOperationException()
    {
        var state = new CampaignState(new CampaignSeed(7UL));
        var cause = new CauseRecord(2, CauseCategory.Activity, "acto", "puerto-alba", "confidence", 2.5m);

        Assert.That(() => state.RecordCause(cause), Throws.TypeOf<InvalidOperationException>());
    }
}

}
