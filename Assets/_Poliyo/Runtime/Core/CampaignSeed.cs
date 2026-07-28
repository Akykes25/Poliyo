using System;

namespace Poliyo.Core
{
/// <summary>
/// Identifies one reproducible campaign and derives isolated random streams for its systems.
/// </summary>
public readonly struct CampaignSeed : IEquatable<CampaignSeed>
{
    public CampaignSeed(ulong value)
    {
        Value = value;
    }

    public ulong Value { get; }

    public CampaignSeed Derive(string streamName)
    {
        if (string.IsNullOrWhiteSpace(streamName))
        {
            throw new ArgumentException("A random stream must have a name.", nameof(streamName));
        }

        return new CampaignSeed(DeterministicHash.Combine(Value, streamName));
    }

    public DeterministicRandom CreateRandom(string streamName)
    {
        return new DeterministicRandom(Derive(streamName).Value);
    }

    public bool Equals(CampaignSeed other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        return obj is CampaignSeed other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

}
