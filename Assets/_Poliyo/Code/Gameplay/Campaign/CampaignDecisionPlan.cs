using System;
using System.Collections.Generic;

namespace Poliyo.Simulation
{
/// <summary>Complete authored consequences for a multi-step campaign decision.</summary>
public sealed class CampaignDecisionPlan
{
    public CampaignDecisionPlan(
        string id,
        CampaignActivity activity,
        string actorId,
        decimal cost,
        IEnumerable<ElectoralImpact> impacts,
        IEnumerable<string> selectedOptionIds,
        IEnumerable<PoliticalRelationshipChange> relationshipChanges = null,
        IEnumerable<CampaignPromiseDefinition> promises = null,
        IEnumerable<CampaignDeferredConsequenceDefinition> deferredConsequences = null,
        decimal rivalImpactMagnitude = 0m,
        string rivalResponseId = null,
        string contextId = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A decision id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(actorId)) throw new ArgumentException("A decision actor is required.", nameof(actorId));
        if (cost < 0m) throw new ArgumentOutOfRangeException(nameof(cost));
        if (impacts == null) throw new ArgumentNullException(nameof(impacts));
        if (selectedOptionIds == null) throw new ArgumentNullException(nameof(selectedOptionIds));
        if (rivalImpactMagnitude < -100m || rivalImpactMagnitude > 100m) throw new ArgumentOutOfRangeException(nameof(rivalImpactMagnitude));

        Id = id;
        Activity = activity;
        ActorId = actorId;
        Cost = cost;
        Impacts = CopyItems(impacts, nameof(impacts));
        SelectedOptionIds = CopyRequiredStrings(selectedOptionIds, nameof(selectedOptionIds));
        RelationshipChanges = relationshipChanges == null
            ? Array.Empty<PoliticalRelationshipChange>()
            : CopyItems(relationshipChanges, nameof(relationshipChanges));
        Promises = promises == null
            ? Array.Empty<CampaignPromiseDefinition>()
            : CopyItems(promises, nameof(promises));
        DeferredConsequences = deferredConsequences == null
            ? Array.Empty<CampaignDeferredConsequenceDefinition>()
            : CopyItems(deferredConsequences, nameof(deferredConsequences));
        RivalImpactMagnitude = rivalImpactMagnitude;
        RivalResponseId = string.IsNullOrWhiteSpace(rivalResponseId) ? null : rivalResponseId;
        ContextId = string.IsNullOrWhiteSpace(contextId) ? "nacional" : contextId;

        if (SelectedOptionIds.Count == 0)
        {
            throw new ArgumentException("A decision requires at least one selected option.", nameof(selectedOptionIds));
        }
    }

    public string Id { get; }
    public CampaignActivity Activity { get; }
    public string ActorId { get; }
    public decimal Cost { get; }
    public IReadOnlyList<ElectoralImpact> Impacts { get; }
    public IReadOnlyList<string> SelectedOptionIds { get; }
    public IReadOnlyList<PoliticalRelationshipChange> RelationshipChanges { get; }
    public IReadOnlyList<CampaignPromiseDefinition> Promises { get; }
    public IReadOnlyList<CampaignDeferredConsequenceDefinition> DeferredConsequences { get; }
    public decimal RivalImpactMagnitude { get; }
    public string RivalResponseId { get; }
    public string ContextId { get; }

    public CampaignActionDefinition CreateActionDefinition()
    {
        return new CampaignActionDefinition(Id, Activity, Cost, Impacts);
    }

    private static IReadOnlyList<T> CopyItems<T>(IEnumerable<T> items, string parameterName)
        where T : class
    {
        var copiedItems = new List<T>();
        foreach (T item in items)
        {
            if (item == null) throw new ArgumentException("A decision item is required.", parameterName);
            copiedItems.Add(item);
        }

        return copiedItems.AsReadOnly();
    }

    private static IReadOnlyList<string> CopyRequiredStrings(IEnumerable<string> values, string parameterName)
    {
        var copiedValues = new List<string>();
        foreach (string value in values)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("A decision option id is required.", parameterName);
            copiedValues.Add(value);
        }

        return copiedValues.AsReadOnly();
    }
}
}
