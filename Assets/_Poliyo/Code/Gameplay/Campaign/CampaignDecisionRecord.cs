using System;
using System.Collections.Generic;

namespace Poliyo.Simulation
{
/// <summary>Immutable campaign memory of the path chosen in a special decision scene.</summary>
public sealed class CampaignDecisionRecord
{
    public CampaignDecisionRecord(
        string id,
        string decisionId,
        int day,
        CampaignActivity activity,
        string actorId,
        decimal cost,
        IEnumerable<string> selectedOptionIds,
        string contextId = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A decision record id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(decisionId)) throw new ArgumentException("A decision id is required.", nameof(decisionId));
        if (day < 1 || day > CampaignCalendar.TotalCampaignDays) throw new ArgumentOutOfRangeException(nameof(day));
        if (string.IsNullOrWhiteSpace(actorId)) throw new ArgumentException("A decision actor is required.", nameof(actorId));
        if (cost < 0m) throw new ArgumentOutOfRangeException(nameof(cost));
        if (selectedOptionIds == null) throw new ArgumentNullException(nameof(selectedOptionIds));

        var copiedOptionIds = new List<string>();
        foreach (string optionId in selectedOptionIds)
        {
            if (string.IsNullOrWhiteSpace(optionId)) throw new ArgumentException("A decision option id is required.", nameof(selectedOptionIds));
            copiedOptionIds.Add(optionId);
        }

        if (copiedOptionIds.Count == 0) throw new ArgumentException("A decision record requires at least one selected option.", nameof(selectedOptionIds));

        Id = id;
        DecisionId = decisionId;
        Day = day;
        Activity = activity;
        ActorId = actorId;
        Cost = cost;
        SelectedOptionIds = copiedOptionIds.AsReadOnly();
        ContextId = string.IsNullOrWhiteSpace(contextId) ? "nacional" : contextId;
    }

    public string Id { get; }
    public string DecisionId { get; }
    public int Day { get; }
    public CampaignActivity Activity { get; }
    public string ActorId { get; }
    public decimal Cost { get; }
    public IReadOnlyList<string> SelectedOptionIds { get; }
    public string ContextId { get; }
}
}
