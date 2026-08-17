using System;

namespace Poliyo.Simulation
{
/// <summary>Aggregated explanation used by the simulated slice closure.</summary>
public sealed class CampaignCausalFactor
{
    public CampaignCausalFactor(CauseCategory category, string sourceId, string effectId, decimal magnitude, int occurrences)
    {
        if (string.IsNullOrWhiteSpace(sourceId)) throw new ArgumentException("A causal factor source is required.", nameof(sourceId));
        if (string.IsNullOrWhiteSpace(effectId)) throw new ArgumentException("A causal factor effect is required.", nameof(effectId));
        if (occurrences < 1) throw new ArgumentOutOfRangeException(nameof(occurrences));

        Category = category;
        SourceId = sourceId;
        EffectId = effectId;
        Magnitude = magnitude;
        Occurrences = occurrences;
    }

    public CauseCategory Category { get; }
    public string SourceId { get; }
    public string EffectId { get; }
    public decimal Magnitude { get; }
    public int Occurrences { get; }
}
}
