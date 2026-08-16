using System;

namespace Poliyo.Simulation
{
/// <summary>Authored promise data before the campaign assigns its creation day.</summary>
public sealed class CampaignPromiseDefinition
{
    public CampaignPromiseDefinition(string id, string counterpartId, string description)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A promise id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(counterpartId)) throw new ArgumentException("A promise counterpart is required.", nameof(counterpartId));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("A promise description is required.", nameof(description));

        Id = id;
        CounterpartId = counterpartId;
        Description = description;
    }

    public string Id { get; }
    public string CounterpartId { get; }
    public string Description { get; }
}
}
