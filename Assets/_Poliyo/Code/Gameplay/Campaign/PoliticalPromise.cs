using System;

namespace Poliyo.Simulation
{
/// <summary>A political commitment kept in campaign memory for future checks and consequences.</summary>
public sealed class PoliticalPromise
{
    public PoliticalPromise(string id, string counterpartId, string description, int createdDay)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A promise id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(counterpartId)) throw new ArgumentException("A promise counterpart is required.", nameof(counterpartId));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("A promise description is required.", nameof(description));
        if (createdDay < 1 || createdDay > CampaignCalendar.TotalCampaignDays) throw new ArgumentOutOfRangeException(nameof(createdDay));

        Id = id;
        CounterpartId = counterpartId;
        Description = description;
        CreatedDay = createdDay;
    }

    public string Id { get; }
    public string CounterpartId { get; }
    public string Description { get; }
    public int CreatedDay { get; }
}
}
