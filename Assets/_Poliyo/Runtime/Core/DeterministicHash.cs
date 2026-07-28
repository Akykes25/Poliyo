using System;

namespace Poliyo.Core
{
internal static class DeterministicHash
{
    private const ulong FnvOffsetBasis = 14695981039346656037UL;
    private const ulong FnvPrime = 1099511628211UL;

    public static ulong Combine(ulong seed, string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var hash = Mix(seed ^ FnvOffsetBasis);

        for (var index = 0; index < value.Length; index++)
        {
            hash ^= value[index];
            hash *= FnvPrime;
        }

        return Mix(hash);
    }

    public static ulong Mix(ulong value)
    {
        value += 0x9E3779B97F4A7C15UL;
        value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
        value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
        return value ^ (value >> 31);
    }
}

}
