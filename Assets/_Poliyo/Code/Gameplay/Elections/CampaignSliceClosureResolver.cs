using System;
using System.Collections.Generic;
using Poliyo.Core;

namespace Poliyo.Simulation
{
/// <summary>Builds the slice readout from the accumulated campaign state without changing the real election rules.</summary>
public static class CampaignSliceClosureResolver
{
    public const int RequiredDecisionDay = 14;

    public static CampaignSliceClosureResult Resolve(
        CampaignState campaign,
        IEnumerable<MicroElector> electorate,
        IEnumerable<CampaignDecisionRecord> decisions)
    {
        if (campaign == null) throw new ArgumentNullException(nameof(campaign));
        if (electorate == null) throw new ArgumentNullException(nameof(electorate));
        if (decisions == null) throw new ArgumentNullException(nameof(decisions));
        if (campaign.Calendar.CurrentDay < RequiredDecisionDay)
        {
            throw new InvalidOperationException("The fourteen-day slice cannot close before day 14.");
        }

        var decisionList = new List<CampaignDecisionRecord>();
        foreach (CampaignDecisionRecord decision in decisions)
        {
            if (decision != null) decisionList.Add(decision);
        }

        if (decisionList.Count == 0)
        {
            throw new InvalidOperationException("The slice closure requires at least one recorded decision.");
        }

        ElectionTally tally = ElectionTallyCalculator.Calculate(electorate, CampaignCandidateIds.All);
        ElectionOutcome outcome = ElectionOutcomeCalculator.CalculateFirstRound(tally, CampaignCandidateIds.All);
        var factors = AggregateFactors(campaign.CauseRecords, 5);
        return new CampaignSliceClosureResult(campaign.Calendar.CurrentDay, tally, outcome, factors, decisionList.Count);
    }

    private static IReadOnlyList<CampaignCausalFactor> AggregateFactors(IEnumerable<CauseRecord> causes, int limit)
    {
        var aggregates = new Dictionary<string, FactorAccumulator>(StringComparer.Ordinal);
        foreach (CauseRecord cause in causes)
        {
            if (cause == null || cause.Category == CauseCategory.Election)
            {
                continue;
            }

            string key = cause.Category + "|" + cause.SourceId + "|" + cause.EffectId;
            if (!aggregates.TryGetValue(key, out FactorAccumulator accumulator))
            {
                accumulator = new FactorAccumulator(cause.Category, cause.SourceId, cause.EffectId);
                aggregates.Add(key, accumulator);
            }

            accumulator.Magnitude += cause.Magnitude;
            accumulator.Occurrences++;
        }

        var factors = new List<CampaignCausalFactor>();
        foreach (FactorAccumulator accumulator in aggregates.Values)
        {
            factors.Add(new CampaignCausalFactor(
                accumulator.Category,
                accumulator.SourceId,
                accumulator.EffectId,
                accumulator.Magnitude,
                accumulator.Occurrences));
        }

        factors.Sort(CompareFactors);
        if (factors.Count > limit)
        {
            factors.RemoveRange(limit, factors.Count - limit);
        }

        return factors.AsReadOnly();
    }

    private static int CompareFactors(CampaignCausalFactor left, CampaignCausalFactor right)
    {
        int magnitudeComparison = Math.Abs(right.Magnitude).CompareTo(Math.Abs(left.Magnitude));
        if (magnitudeComparison != 0) return magnitudeComparison;

        int sourceComparison = string.Compare(left.SourceId, right.SourceId, StringComparison.Ordinal);
        if (sourceComparison != 0) return sourceComparison;
        return string.Compare(left.EffectId, right.EffectId, StringComparison.Ordinal);
    }

    private sealed class FactorAccumulator
    {
        public FactorAccumulator(CauseCategory category, string sourceId, string effectId)
        {
            Category = category;
            SourceId = sourceId;
            EffectId = effectId;
        }

        public CauseCategory Category { get; }
        public string SourceId { get; }
        public string EffectId { get; }
        public decimal Magnitude { get; set; }
        public int Occurrences { get; set; }
    }
}
}
