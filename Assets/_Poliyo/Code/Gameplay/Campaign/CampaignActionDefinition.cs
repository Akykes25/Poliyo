using System;
using System.Collections.Generic;

namespace Poliyo.Simulation
{
/// <summary>
/// Data required to resolve one player or rival campaign action. Costs and impacts are authored content, not view logic.
/// </summary>
public sealed class CampaignActionDefinition
{
    public CampaignActionDefinition(string id, CampaignActivity activity, decimal cost, ElectoralImpact impact)
        : this(id, activity, cost, new[] { impact ?? throw new ArgumentNullException(nameof(impact)) })
    {
    }

    public CampaignActionDefinition(string id, CampaignActivity activity, decimal cost, IEnumerable<ElectoralImpact> impacts)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("An action id is required.", nameof(id));
        if (cost < 0m) throw new ArgumentOutOfRangeException(nameof(cost));
        if (impacts == null) throw new ArgumentNullException(nameof(impacts));

        Id = id;
        Activity = activity;
        Cost = cost;
        var copiedImpacts = new List<ElectoralImpact>();
        foreach (ElectoralImpact impact in impacts)
        {
            if (impact == null) throw new ArgumentException("An action impact is required.", nameof(impacts));
            copiedImpacts.Add(impact);
        }

        Impacts = copiedImpacts.AsReadOnly();
    }

    public string Id { get; }
    public CampaignActivity Activity { get; }
    public decimal Cost { get; }
    /// <summary>The first impact kept for compatibility with single-impact callers; null for relationship-only actions.</summary>
    public ElectoralImpact Impact => Impacts.Count == 0 ? null : Impacts[0];
    public IReadOnlyList<ElectoralImpact> Impacts { get; }
}
}
