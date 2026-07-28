using System;

namespace Poliyo.Simulation
{
public sealed class ElectoralImpact
{
    public ElectoralImpact(string sourceId, string candidateId, ElectoralMetric metric, decimal baseMagnitude, decimal reach, decimal relevance, decimal compatibility, decimal credibility, decimal mediaFraming, decimal novelty)
    {
        SourceId = sourceId ?? throw new ArgumentNullException(nameof(sourceId));
        CandidateId = candidateId ?? throw new ArgumentNullException(nameof(candidateId));
        Metric = metric;
        BaseMagnitude = baseMagnitude;
        Reach = ValidateFactor(reach, nameof(reach));
        Relevance = ValidateFactor(relevance, nameof(relevance));
        Compatibility = ValidateFactor(compatibility, nameof(compatibility));
        Credibility = ValidateFactor(credibility, nameof(credibility));
        MediaFraming = ValidateFactor(mediaFraming, nameof(mediaFraming));
        Novelty = ValidateFactor(novelty, nameof(novelty));
    }

    public string SourceId { get; }
    public string CandidateId { get; }
    public ElectoralMetric Metric { get; }
    public decimal BaseMagnitude { get; }
    public decimal Reach { get; }
    public decimal Relevance { get; }
    public decimal Compatibility { get; }
    public decimal Credibility { get; }
    public decimal MediaFraming { get; }
    public decimal Novelty { get; }

    public decimal CalculateDelta() => BaseMagnitude * Reach * Relevance * Compatibility * Credibility * MediaFraming * Novelty;

    private static decimal ValidateFactor(decimal value, string parameterName)
    {
        if (value < 0m || value > 1m) throw new ArgumentOutOfRangeException(parameterName);
        return value;
    }
}

}
