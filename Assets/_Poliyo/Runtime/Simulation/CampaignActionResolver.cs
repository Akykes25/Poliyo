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
        var causes = new List<CauseRecord>();
        if (action.Cost > 0m && !economy.TryPayExpense(day, action.Id, action.Cost))
        {
            var unpaidCause = new CauseRecord(day, CauseCategory.Economy, action.Id, "campaign", "unpaid-expense", -action.Cost);
            campaign.RecordCause(unpaidCause);
            causes.Add(unpaidCause);
            return new CampaignActionResolution(false, causes);
        }

        if (action.Cost > 0m)
        {
            var expenseCause = new CauseRecord(day, CauseCategory.Economy, action.Id, "campaign", "expense", -action.Cost);
            campaign.RecordCause(expenseCause);
            causes.Add(expenseCause);
        }

        IReadOnlyList<CauseRecord> impactCauses = ElectoralImpactApplier.Apply(campaign, action.Impact, targets, CauseCategory.Activity);
        foreach (CauseRecord impactCause in impactCauses)
        {
            causes.Add(impactCause);
        }

        return new CampaignActionResolution(true, causes);
    }
}
}
