using System;
using System.Collections.Generic;

namespace Poliyo.Simulation
{
/// <summary>Resolved share of participating votes for one weighted microelector.</summary>
public sealed class FinalVoteDistribution
{
    private readonly IReadOnlyDictionary<string, decimal> _candidateShares;

    public FinalVoteDistribution(IReadOnlyDictionary<string, decimal> candidateShares, decimal blankShare)
    {
        _candidateShares = candidateShares ?? throw new ArgumentNullException(nameof(candidateShares));
        BlankShare = blankShare;
    }

    public IReadOnlyDictionary<string, decimal> CandidateShares => _candidateShares;
    public decimal BlankShare { get; }
    public decimal UndecidedShare => 0m;

    public decimal GetCandidateShare(string candidateId)
    {
        return _candidateShares.TryGetValue(candidateId, out decimal share) ? share : 0m;
    }
}
}
