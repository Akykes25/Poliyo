using System;
using System.Collections.Generic;

namespace Poliyo.Simulation
{
/// <summary>Resolves campaign actions against explicit targets and explains both economic and electoral consequences.</summary>
public static class CampaignActionResolver
{
    public static CampaignActionResolution Resolve(
        CampaignState campaign,
        CampaignEconomy economy,
        CampaignActionDefinition action,
        IEnumerable<MicroElector> targets)
    {
        if (campaign == null) throw new ArgumentNullException(nameof(campaign));
        if (economy == null) throw new ArgumentNullException(nameof(economy));
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (targets == null) throw new ArgumentNullException(nameof(targets));

        int day = campaign.Calendar.CurrentDay;
        var targetList = new List<MicroElector>();
        foreach (MicroElector target in targets)
        {
            if (target == null) throw new ArgumentException("An electoral target is required.", nameof(targets));
            targetList.Add(target);
        }

        // Validate every candidate reference before charging or mutating any target. A bad
        // content row must fail atomically instead of leaving a partial action behind.
        foreach (ElectoralImpact impact in action.Impacts)
        {
            if (impact.Metric == ElectoralMetric.Participation) continue;
            foreach (MicroElector target in targetList)
            {
                target.GetCandidate(impact.CandidateId);
            }
        }

        var causes = new List<CauseRecord>();
        if (action.Cost > 0m && !economy.CanAfford(action.Cost))
        {
            return new CampaignActionResolution(false, causes);
        }

        if (action.Cost > 0m && !economy.TryPayExpense(day, action.Id, action.Cost))
        {
            // The preflight above keeps this path unreachable in the single-threaded
            // campaign loop, but retain the guard so a future economy implementation
            // cannot apply electoral effects without recording its transaction.
            return new CampaignActionResolution(false, causes);
        }

        if (action.Cost > 0m)
        {
            var expenseCause = new CauseRecord(day, CauseCategory.Economy, action.Id, "campaign", "expense", -action.Cost);
            campaign.RecordCause(expenseCause);
            causes.Add(expenseCause);
        }

        foreach (ElectoralImpact impact in action.Impacts)
        {
            IReadOnlyList<CauseRecord> impactCauses = ElectoralImpactApplier.Apply(campaign, impact, targetList, CauseCategory.Activity);
            foreach (CauseRecord impactCause in impactCauses)
            {
                causes.Add(impactCause);
            }
        }

        return new CampaignActionResolution(true, causes);
    }
}
}
