using System;

namespace Poliyo.Simulation
{
public enum CauseCategory
{
    Activity,
    DelegatedTask,
    Economy,
    Media,
    Event,
    RivalAction,
    Election,
}

/// <summary>
/// An auditable explanation for a campaign-state change.
/// </summary>
public sealed class CauseRecord
{
    public CauseRecord(
        int day,
        CauseCategory category,
        string sourceId,
        string targetId,
        string effectId,
        decimal magnitude)
    {
        if (day < 1 || day > CampaignCalendar.TotalCampaignDays)
        {
            throw new ArgumentOutOfRangeException(nameof(day));
        }

        if (string.IsNullOrWhiteSpace(sourceId))
        {
            throw new ArgumentException("A cause requires a source.", nameof(sourceId));
        }

        if (string.IsNullOrWhiteSpace(targetId))
        {
            throw new ArgumentException("A cause requires a target.", nameof(targetId));
        }

        if (string.IsNullOrWhiteSpace(effectId))
        {
            throw new ArgumentException("A cause requires an effect.", nameof(effectId));
        }

        Day = day;
        Category = category;
        SourceId = sourceId;
        TargetId = targetId;
        EffectId = effectId;
        Magnitude = magnitude;
    }

    public int Day { get; }

    public CauseCategory Category { get; }

    public string SourceId { get; }

    public string TargetId { get; }

    public string EffectId { get; }

    public decimal Magnitude { get; }
}

}
