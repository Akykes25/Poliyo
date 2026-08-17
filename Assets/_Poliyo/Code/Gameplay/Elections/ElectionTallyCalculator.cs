using System;
using System.Collections.Generic;
using Poliyo.Core;

namespace Poliyo.Simulation
{
/// <summary>Aggregates weighted microelectors without allowing record count to distort territorial value.</summary>
public static class ElectionTallyCalculator
{
    public static ElectionTally Calculate(IEnumerable<MicroElector> microElectors, IEnumerable<string> candidateIds)
    {
        if (microElectors == null) throw new ArgumentNullException(nameof(microElectors));
        if (candidateIds == null) throw new ArgumentNullException(nameof(candidateIds));

        var candidates = new List<string>(candidateIds);
        if (candidates.Count == 0)
        {
            throw new ArgumentException("At least one candidate is required.", nameof(candidateIds));
        }

        var tally = new ElectionTally();
        foreach (MicroElector elector in microElectors)
        {
            if (elector == null) throw new ArgumentException("A microelector is required.", nameof(microElectors));

            decimal participatingWeight = elector.ElectoralWeight * elector.Participation / 100m;
            tally.AddParticipation(participatingWeight);

            decimal candidateDistribution = 0m;
            foreach (string candidateId in candidates)
            {
                decimal intention = elector.GetCandidate(candidateId).VotingIntention;
                candidateDistribution += intention;
                tally.AddCandidateVotes(candidateId, participatingWeight * intention / 100m);
            }

            tally.AddBlankVotes(participatingWeight * elector.BlankVoteIntention / 100m);
            decimal unresolvedIntention = 100m - candidateDistribution - elector.BlankVoteIntention;
            tally.AddUndecidedVotes(participatingWeight * unresolvedIntention / 100m);
        }

        return tally;
    }

    /// <summary>Calculates the final scrutiny after resolving every undecided share deterministically.</summary>
    public static ElectionTally CalculateResolved(
        IEnumerable<MicroElector> microElectors,
        IEnumerable<string> candidateIds,
        CampaignSeed campaignSeed,
        FinalVoteResolutionParameters parameters)
    {
        if (microElectors == null) throw new ArgumentNullException(nameof(microElectors));
        if (candidateIds == null) throw new ArgumentNullException(nameof(candidateIds));
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        var candidates = new List<string>(candidateIds);
        if (candidates.Count < 2)
        {
            throw new ArgumentException("At least two candidates are required.", nameof(candidateIds));
        }

        var tally = new ElectionTally();
        foreach (MicroElector elector in microElectors)
        {
            if (elector == null) throw new ArgumentException("A microelector is required.", nameof(microElectors));

            decimal participatingWeight = elector.ElectoralWeight * elector.Participation / 100m;
            FinalVoteDistribution distribution = FinalVoteResolver.Resolve(elector, candidates, campaignSeed, parameters);
            tally.AddParticipation(participatingWeight);
            foreach (string candidateId in candidates)
            {
                tally.AddCandidateVotes(candidateId, participatingWeight * distribution.GetCandidateShare(candidateId) / 100m);
            }

            tally.AddBlankVotes(participatingWeight * distribution.BlankShare / 100m);
        }

        return tally;
    }

    /// <summary>Calculates a deterministic two-candidate scrutiny for the runoff.</summary>
    public static ElectionTally CalculateRunoffResolved(
        IEnumerable<MicroElector> microElectors,
        IReadOnlyList<string> finalistIds,
        CampaignSeed campaignSeed,
        FinalVoteResolutionParameters parameters)
    {
        if (microElectors == null) throw new ArgumentNullException(nameof(microElectors));
        if (finalistIds == null || finalistIds.Count != 2)
        {
            throw new ArgumentException("A runoff requires exactly two finalists.", nameof(finalistIds));
        }

        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        var tally = new ElectionTally();
        foreach (MicroElector elector in microElectors)
        {
            if (elector == null) throw new ArgumentException("A microelector is required.", nameof(microElectors));

            decimal participatingWeight = elector.ElectoralWeight * elector.Participation / 100m;
            FinalVoteDistribution distribution = FinalVoteResolver.ResolveRunoff(elector, finalistIds, campaignSeed, parameters);
            tally.AddParticipation(participatingWeight);
            foreach (string finalistId in finalistIds)
            {
                tally.AddCandidateVotes(finalistId, participatingWeight * distribution.GetCandidateShare(finalistId) / 100m);
            }

            tally.AddBlankVotes(participatingWeight * distribution.BlankShare / 100m);
        }

        return tally;
    }
}
}
