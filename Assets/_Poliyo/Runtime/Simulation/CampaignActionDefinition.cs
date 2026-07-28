using System;

namespace Poliyo.Simulation
{
/// <summary>
/// Data required to resolve one player or rival campaign action. Costs and impacts are authored content, not view logic.
/// </summary>
public sealed class CampaignActionDefinition
{
    public CampaignActionDefinition(string id, CampaignActivity activity, decimal cost, ElectoralImpact impact)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("An action id is required.", nameof(id));
        if (cost < 0m) throw new ArgumentOutOfRangeException(nameof(cost));
        if (impact == null) throw new ArgumentNullException(nameof(impact));

        Id = id;
        Activity = activity;
        Cost = cost;
        Impact = impact;
    }

    public string Id { get; }
    public CampaignActivity Activity { get; }
    public decimal Cost { get; }
    public ElectoralImpact Impact { get; }
}
}
