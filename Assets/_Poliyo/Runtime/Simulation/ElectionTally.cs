using System;
using System.Collections.Generic;

namespace Poliyo.Simulation
{
/// <summary>
/// Immutable-by-observation weighted election result. Valid votes exclude blank and undecided intentions.
/// </summary>
public sealed class ElectionTally
{
    private readonly Dictionary<string, decimal> _candidateVotes = new Dictionary<string, decimal>();

    public IReadOnlyDictionary<string, decimal> CandidateVotes => _candidateVotes;
    public decimal ParticipatingWeight { get; private set; }
    public decimal ValidVotes { get; private set; }
    public decimal BlankVotes { get; private set; }
    public decimal UndecidedVotes { get; private set; }

    public void AddCandidateVotes(string candidateId, decimal votes)
    {
        if (string.IsNullOrWhiteSpace(candidateId)) throw new ArgumentException("A candidate id is required.", nameof(candidateId));
        if (votes < 0m) throw new ArgumentOutOfRangeException(nameof(votes));

        _candidateVotes[candidateId] = GetCandidateVotes(candidateId) + votes;
        ValidVotes += votes;
    }

    public void AddBlankVotes(decimal votes)
    {
        if (votes < 0m) throw new ArgumentOutOfRangeException(nameof(votes));
        BlankVotes += votes;
    }

    public void AddUndecidedVotes(decimal votes)
    {
        if (votes < 0m) throw new ArgumentOutOfRangeException(nameof(votes));
        UndecidedVotes += votes;
    }

    public void AddParticipation(decimal weight)
    {
        if (weight < 0m) throw new ArgumentOutOfRangeException(nameof(weight));
        ParticipatingWeight += weight;
    }

    public decimal GetCandidateVotes(string candidateId)
    {
        return _candidateVotes.TryGetValue(candidateId, out decimal votes) ? votes : 0m;
    }

    public decimal GetValidVoteShare(string candidateId)
    {
        return ValidVotes == 0m ? 0m : GetCandidateVotes(candidateId) / ValidVotes * 100m;
    }
}
}