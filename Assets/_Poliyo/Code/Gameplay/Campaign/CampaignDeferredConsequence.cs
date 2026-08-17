using System;

namespace Poliyo.Simulation
{
/// <summary>Persistable runtime consequence waiting for its deterministic resolution day.</summary>
public sealed class CampaignDeferredConsequence
{
    public CampaignDeferredConsequence(
        string id,
        int dueDay,
        string sourceId,
        string targetId,
        string effectId,
        string candidateId,
        ElectoralMetric metric,
        decimal magnitude,
        CauseCategory category)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A deferred consequence id is required.", nameof(id));
        if (dueDay < 1 || dueDay > CampaignCalendar.TotalCampaignDays) throw new ArgumentOutOfRangeException(nameof(dueDay));
        if (string.IsNullOrWhiteSpace(sourceId)) throw new ArgumentException("A deferred consequence source is required.", nameof(sourceId));
        if (string.IsNullOrWhiteSpace(targetId)) throw new ArgumentException("A deferred consequence target is required.", nameof(targetId));
        if (string.IsNullOrWhiteSpace(effectId)) throw new ArgumentException("A deferred consequence effect is required.", nameof(effectId));
        if (string.IsNullOrWhiteSpace(candidateId)) throw new ArgumentException("A deferred consequence candidate is required.", nameof(candidateId));

        Id = id;
        DueDay = dueDay;
        SourceId = sourceId;
        TargetId = targetId;
        EffectId = effectId;
        CandidateId = candidateId;
        Metric = metric;
        Magnitude = magnitude;
        Category = category;
    }

    public string Id { get; }
    public int DueDay { get; }
    public string SourceId { get; }
    public string TargetId { get; }
    public string EffectId { get; }
    public string CandidateId { get; }
    public ElectoralMetric Metric { get; }
    public decimal Magnitude { get; }
    public CauseCategory Category { get; }
}
}
