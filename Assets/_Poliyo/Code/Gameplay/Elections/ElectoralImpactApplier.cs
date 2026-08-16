using System;
using System.Collections.Generic;

namespace Poliyo.Simulation
{
/// <summary>
/// Applies a resolved electoral impact to its explicit targets and records every mutation for player-facing explanations.
/// </summary>
public static class ElectoralImpactApplier
{
    public static IReadOnlyList<CauseRecord> Apply(
        CampaignState campaign,
        ElectoralImpact impact,
        IEnumerable<MicroElector> targets,
        CauseCategory category)
    {
        if (campaign == null) throw new ArgumentNullException(nameof(campaign));
        if (impact == null) throw new ArgumentNullException(nameof(impact));
        if (targets == null) throw new ArgumentNullException(nameof(targets));

        decimal delta = impact.CalculateDelta();
        var causes = new List<CauseRecord>();
        foreach (MicroElector target in targets)
        {
            if (target == null) throw new ArgumentException("An electoral target is required.", nameof(targets));

            target.Apply(impact.CandidateId, impact.Metric, delta);
            var cause = new CauseRecord(
                campaign.Calendar.CurrentDay,
                category,
                impact.SourceId,
                target.Id,
                impact.Metric.ToString(),
                delta);
            campaign.RecordCause(cause);
            causes.Add(cause);
        }

        return causes;
    }
}
}
