using System;

namespace Poliyo.Simulation
{
/// <summary>Pure simulation input extracted from authored locality content.</summary>
public readonly struct LocalityElectorateSeed
{
    public LocalityElectorateSeed(string localityId, int electoralWeight)
    {
        if (string.IsNullOrWhiteSpace(localityId)) throw new ArgumentException("A locality id is required.", nameof(localityId));
        if (electoralWeight < 1) throw new ArgumentOutOfRangeException(nameof(electoralWeight));

        LocalityId = localityId;
        ElectoralWeight = electoralWeight;
    }

    public string LocalityId { get; }
    public int ElectoralWeight { get; }
}
}
