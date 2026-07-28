using System;

namespace Poliyo.Simulation
{
/// <summary>
/// Owns campaign time. The vertical slice always runs for sixty days.
/// </summary>
public sealed class CampaignCalendar
{
    public const int TotalCampaignDays = 60;
    public const int FogStartDay = 31;

    public CampaignCalendar()
        : this(1)
    {
    }

    public CampaignCalendar(int currentDay)
    {
        if (currentDay < 1 || currentDay > TotalCampaignDays)
        {
            throw new ArgumentOutOfRangeException(nameof(currentDay));
        }

        CurrentDay = currentDay;
    }

    public int CurrentDay { get; private set; }

    public int CurrentWeek => ((CurrentDay - 1) / 7) + 1;

    public bool IsCampaignMeetingDay => (CurrentDay - 1) % 7 == 0;

    public bool IsElectoralFogActive => CurrentDay >= FogStartDay;

    public bool IsElectionDay => CurrentDay == TotalCampaignDays;

    public bool CanAdvance => !IsElectionDay;

    public void AdvanceDay()
    {
        if (!CanAdvance)
        {
            throw new InvalidOperationException("The campaign cannot advance beyond election day.");
        }

        CurrentDay++;
    }
}

}
