using System;

namespace Poliyo.Simulation
{
/// <summary>Authored description of a consequence that is deliberately resolved after the decision.</summary>
public sealed class CampaignDeferredConsequenceDefinition
{
    public CampaignDeferredConsequenceDefinition(
        string id,
        string sourceId,
        string targetId,
        string effectId,
        string candidateId,
        ElectoralMetric metric,
        decimal magnitude,
        int daysAfterDecision,
        CauseCategory category = CauseCategory.Event)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A deferred consequence id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(sourceId)) throw new ArgumentException("A deferred consequence source is required.", nameof(sourceId));
        if (string.IsNullOrWhiteSpace(targetId)) throw new ArgumentException("A deferred consequence target is required.", nameof(targetId));
        if (string.IsNullOrWhiteSpace(effectId)) throw new ArgumentException("A deferred consequence effect is required.", nameof(effectId));
        if (string.IsNullOrWhiteSpace(candidateId)) throw new ArgumentException("A deferred consequence candidate is required.", nameof(candidateId));
        if (daysAfterDecision < 1) throw new ArgumentOutOfRangeException(nameof(daysAfterDecision));

        Id = id;
        SourceId = sourceId;
        TargetId = targetId;
        EffectId = effectId;
        CandidateId = candidateId;
        Metric = metric;
        Magnitude = magnitude;
        DaysAfterDecision = daysAfterDecision;
        Category = category;
    }

    public string Id { get; }
    public string SourceId { get; }
    public string TargetId { get; }
    public string EffectId { get; }
    public string CandidateId { get; }
    public ElectoralMetric Metric { get; }
    public decimal Magnitude { get; }
    public int DaysAfterDecision { get; }
    public CauseCategory Category { get; }
}
}
