using System;

namespace Poliyo.Simulation
{
public sealed class DelegatedTaskAssignment
{
    public DelegatedTaskAssignment(int day, string memberId, DelegatedTaskType taskType, string targetId)
    {
        if (day < 1 || day > CampaignCalendar.TotalCampaignDays) throw new ArgumentOutOfRangeException(nameof(day));
        if (string.IsNullOrWhiteSpace(memberId) || string.IsNullOrWhiteSpace(targetId)) throw new ArgumentException("A task requires a member and target.");
        Day = day;
        MemberId = memberId;
        TaskType = taskType;
        TargetId = targetId;
    }

    public int Day { get; }
    public string MemberId { get; }
    public DelegatedTaskType TaskType { get; }
    public string TargetId { get; }
}

}
