using System;
using NUnit.Framework;
using Poliyo.Simulation;

namespace Poliyo.Core.EditModeTests
{
public sealed class CampaignCalendarTests
{
    [Test]
    public void Constructor_OnFirstDay_StartsCampaignMeeting()
    {
        var calendar = new CampaignCalendar();

        Assert.That(calendar.CurrentDay, Is.EqualTo(1));
        Assert.That(calendar.CurrentWeek, Is.EqualTo(1));
        Assert.That(calendar.IsCampaignMeetingDay, Is.True);
    }

    [Test]
    public void AdvanceDay_OnDayThirtyOne_ActivatesElectoralFog()
    {
        var calendar = new CampaignCalendar(30);

        calendar.AdvanceDay();

        Assert.That(calendar.CurrentDay, Is.EqualTo(31));
        Assert.That(calendar.IsElectoralFogActive, Is.True);
    }

    [Test]
    public void AdvanceDay_OnElectionDay_ThrowsInvalidOperationException()
    {
        var calendar = new CampaignCalendar(CampaignCalendar.TotalCampaignDays);

        Assert.That(calendar.CanAdvance, Is.False);
        Assert.That(() => calendar.AdvanceDay(), Throws.TypeOf<InvalidOperationException>());
    }
}

}
