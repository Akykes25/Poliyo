using System;
using System.Collections.Generic;

namespace Poliyo.Simulation
{
/// <summary>Weighted unit of electoral simulation; it may represent one person or a small population block.</summary>
public sealed class MicroElector
{
    private readonly Dictionary<string, CandidateElectoralState> _candidates;

    public MicroElector(
        string id,
        string localityId,
        decimal electoralWeight,
        decimal participation,
        IEnumerable<CandidateElectoralState> candidates,
        decimal blankVoteIntention = 0m,
        decimal undecidedIntention = 0m)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(localityId))
        {
            throw new ArgumentException("A microelector requires an id and locality.");
        }

        if (electoralWeight <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(electoralWeight));
        }

        if (candidates == null)
        {
            throw new ArgumentNullException(nameof(candidates));
        }

        Id = id;
        LocalityId = localityId;
        ElectoralWeight = electoralWeight;
        Participation = Clamp(participation);
        BlankVoteIntention = Clamp(blankVoteIntention);
        UndecidedIntention = Clamp(undecidedIntention);
        _candidates = new Dictionary<string, CandidateElectoralState>();

        foreach (CandidateElectoralState candidate in candidates)
        {
            if (candidate == null)
            {
                throw new ArgumentException("A candidate electoral state is required.", nameof(candidates));
            }

            _candidates.Add(candidate.CandidateId, candidate);
        }

        if (GetDeclaredDistribution() > 100m)
        {
            throw new ArgumentException("Candidate, blank and undecided intentions cannot exceed 100.", nameof(candidates));
        }
    }

    public string Id { get; }
    public string LocalityId { get; }
    public decimal ElectoralWeight { get; }
    public decimal Participation { get; private set; }
    public decimal BlankVoteIntention { get; }
    public decimal UndecidedIntention { get; }
    public IReadOnlyDictionary<string, CandidateElectoralState> Candidates => _candidates;

    public CandidateElectoralState GetCandidate(string candidateId) => _candidates[candidateId];

    public decimal GetResidualUndecidedIntention() => 100m - GetDeclaredDistribution();

    public void Apply(string candidateId, ElectoralMetric metric, decimal delta)
    {
        if (metric == ElectoralMetric.Participation)
        {
            Participation = Clamp(Participation + delta);
            return;
        }

        GetCandidate(candidateId).Apply(metric, delta);
        if (metric == ElectoralMetric.VotingIntention)
        {
            NormalizeCandidateVotingIntentions();
        }
    }

    private void NormalizeCandidateVotingIntentions()
    {
        decimal availableIntention = 100m - BlankVoteIntention - UndecidedIntention;
        decimal currentTotal = 0m;
        foreach (CandidateElectoralState candidate in _candidates.Values)
        {
            currentTotal += candidate.VotingIntention;
        }

        if (currentTotal <= 0m)
        {
            return;
        }

        decimal assigned = 0m;
        var index = 0;
        foreach (CandidateElectoralState candidate in _candidates.Values)
        {
            decimal normalized = index == _candidates.Count - 1
                ? availableIntention - assigned
                : decimal.Round(candidate.VotingIntention / currentTotal * availableIntention, 4);
            candidate.SetVotingIntention(normalized);
            assigned += normalized;
            index++;
        }
    }

    private decimal GetDeclaredDistribution()
    {
        decimal declared = BlankVoteIntention + UndecidedIntention;
        foreach (CandidateElectoralState candidate in _candidates.Values)
        {
            declared += candidate.VotingIntention;
        }

        return declared;
    }

    private static decimal Clamp(decimal value) => Math.Min(100m, Math.Max(0m, value));
}
}