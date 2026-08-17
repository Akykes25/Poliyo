using System;
using System.Collections.Generic;

namespace Poliyo.Simulation
{
/// <summary>Deterministic, non-final electoral readout for the fourteen-day vertical slice.</summary>
public sealed class CampaignSliceClosureResult
{
    public CampaignSliceClosureResult(
        int day,
        ElectionTally tally,
        ElectionOutcome outcome,
        IEnumerable<CampaignCausalFactor> decisiveFactors,
        int decisionCount)
    {
        if (day < 1 || day > CampaignCalendar.TotalCampaignDays) throw new ArgumentOutOfRangeException(nameof(day));
        if (tally == null) throw new ArgumentNullException(nameof(tally));
        if (outcome == null) throw new ArgumentNullException(nameof(outcome));
        if (decisiveFactors == null) throw new ArgumentNullException(nameof(decisiveFactors));
        if (decisionCount < 1) throw new ArgumentOutOfRangeException(nameof(decisionCount));

        var copiedFactors = new List<CampaignCausalFactor>();
        foreach (CampaignCausalFactor factor in decisiveFactors)
        {
            if (factor == null) throw new ArgumentException("A decisive factor is required.", nameof(decisiveFactors));
            copiedFactors.Add(factor);
        }

        Day = day;
        Tally = tally;
        Outcome = outcome;
        DecisiveFactors = copiedFactors.AsReadOnly();
        DecisionCount = decisionCount;
    }

    public int Day { get; }
    public ElectionTally Tally { get; }
    public ElectionOutcome Outcome { get; }
    public IReadOnlyList<CampaignCausalFactor> DecisiveFactors { get; }
    public int DecisionCount { get; }
}
}
