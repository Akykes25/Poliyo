using System;

namespace Poliyo.Simulation
{
/// <summary>
/// Balance inputs for resolving undecided voters. The defaults are provisional vertical-slice values,
/// not locked campaign balance.
/// </summary>
public sealed class FinalVoteResolutionParameters
{
    public static FinalVoteResolutionParameters VerticalSlicePrototype { get; } =
        new FinalVoteResolutionParameters(0.35m, 0.50m, 0.08m, 0.10m);

    public FinalVoteResolutionParameters(
        decimal trustInfluence,
        decimal rejectionInfluence,
        decimal seededVariation,
        decimal minimumAppeal)
    {
        if (trustInfluence < 0m) throw new ArgumentOutOfRangeException(nameof(trustInfluence));
        if (rejectionInfluence < 0m) throw new ArgumentOutOfRangeException(nameof(rejectionInfluence));
        if (seededVariation < 0m || seededVariation > 0.5m) throw new ArgumentOutOfRangeException(nameof(seededVariation));
        if (minimumAppeal <= 0m) throw new ArgumentOutOfRangeException(nameof(minimumAppeal));

        TrustInfluence = trustInfluence;
        RejectionInfluence = rejectionInfluence;
        SeededVariation = seededVariation;
        MinimumAppeal = minimumAppeal;
    }

    public decimal TrustInfluence { get; }
    public decimal RejectionInfluence { get; }
    public decimal SeededVariation { get; }
    public decimal MinimumAppeal { get; }
}
}
