using System;

namespace Poliyo.Core
{
/// <summary>
/// Small deterministic generator for simulation decisions. Use named streams derived from a campaign seed.
/// </summary>
public sealed class DeterministicRandom
{
    private ulong _state;

    public DeterministicRandom(ulong seed)
    {
        _state = DeterministicHash.Mix(seed);
    }

    public ulong NextUInt64()
    {
        _state += 0x9E3779B97F4A7C15UL;
        var value = _state;
        value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
        value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
        return value ^ (value >> 31);
    }

    public int NextInt(int minInclusive, int maxExclusive)
    {
        if (minInclusive >= maxExclusive)
        {
            throw new ArgumentOutOfRangeException(nameof(maxExclusive), "The maximum must be greater than the minimum.");
        }

        var range = (ulong)((long)maxExclusive - minInclusive);
        return minInclusive + (int)(NextUInt64() % range);
    }

    public double NextDouble()
    {
        return (NextUInt64() >> 11) * (1.0 / (1UL << 53));
    }
}

}
