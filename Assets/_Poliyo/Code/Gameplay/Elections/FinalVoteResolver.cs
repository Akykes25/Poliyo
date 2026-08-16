using System;
using System.Collections.Generic;
using Poliyo.Core;

namespace Poliyo.Simulation
{
/// <summary>
/// Resolves a microelector's undecided share from accumulated electoral state and a stable campaign seed.
/// It does not mutate the electorate, so the same save always reproduces the same scrutiny.
/// </summary>
public static class FinalVoteResolver
{
    public static FinalVoteDistribution Resolve(
        MicroElector elector,
        IReadOnlyList<string> candidateIds,
        CampaignSeed campaignSeed,
        FinalVoteResolutionParameters parameters)
    {
        if (elector == null) throw new ArgumentNullException(nameof(elector));
        if (candidateIds == null || candidateIds.Count < 2)
        {
            throw new ArgumentException("At least two candidates are required.", nameof(candidateIds));
        }

        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        var uniqueCandidateIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (string candidateId in candidateIds)
        {
            if (string.IsNullOrWhiteSpace(candidateId) || !uniqueCandidateIds.Add(candidateId))
            {
                throw new ArgumentException("Candidate ids must be non-empty and unique.", nameof(candidateIds));
            }
        }

        if (elector.Candidates.Count != uniqueCandidateIds.Count)
        {
            throw new ArgumentException("The elector and election must contain the same candidates.", nameof(candidateIds));
        }

        foreach (string electorCandidateId in elector.Candidates.Keys)
        {
            if (!uniqueCandidateIds.Contains(electorCandidateId))
            {
                throw new ArgumentException("The elector and election must contain the same candidates.", nameof(candidateIds));
            }
        }

        decimal undecidedShare = elector.UndecidedIntention + elector.GetResidualUndecidedIntention();
        var appealByCandidate = new decimal[candidateIds.Count];
        decimal totalAppeal = 0m;

        for (var index = 0; index < candidateIds.Count; index++)
        {
            string candidateId = candidateIds[index];
            CandidateElectoralState state = elector.GetCandidate(candidateId);
            decimal appeal = CalculateAppeal(elector.Id, state, campaignSeed, parameters);
            appealByCandidate[index] = appeal;
            totalAppeal += appeal;
        }

        decimal blankAppeal = CalculateBlankAppeal(elector, campaignSeed, parameters);
        totalAppeal += blankAppeal;

        var candidateShares = new Dictionary<string, decimal>(candidateIds.Count);
        decimal assignedUndecided = 0m;
        for (var index = 0; index < candidateIds.Count; index++)
        {
            string candidateId = candidateIds[index];
            decimal undecidedAllocation = undecidedShare * appealByCandidate[index] / totalAppeal;
            assignedUndecided += undecidedAllocation;
            candidateShares.Add(candidateId, elector.GetCandidate(candidateId).VotingIntention + undecidedAllocation);
        }

        decimal blankAllocation = undecidedShare - assignedUndecided;
        return new FinalVoteDistribution(candidateShares, elector.BlankVoteIntention + blankAllocation);
    }

    private static decimal CalculateAppeal(
        string electorId,
        CandidateElectoralState state,
        CampaignSeed campaignSeed,
        FinalVoteResolutionParameters parameters)
    {
        decimal baseAppeal = Math.Max(parameters.MinimumAppeal, state.VotingIntention);
        decimal trustFactor = 1m + (state.Trust / 100m * parameters.TrustInfluence);
        decimal rejectionFactor = 1m + ((100m - state.Rejection) / 100m * parameters.RejectionInfluence);
        decimal seededOffset = GetSeededOffset(campaignSeed, electorId, state.CandidateId);
        decimal variationFactor = 1m + (seededOffset * parameters.SeededVariation);
        return baseAppeal * trustFactor * rejectionFactor * variationFactor;
    }

    private static decimal GetSeededOffset(CampaignSeed campaignSeed, string electorId, string candidateId)
    {
        DeterministicRandom random = campaignSeed.CreateRandom("first-round-vote:" + electorId + ":" + candidateId);
        return ((decimal)random.NextDouble() * 2m) - 1m;
    }

    private static decimal CalculateBlankAppeal(
        MicroElector elector,
        CampaignSeed campaignSeed,
        FinalVoteResolutionParameters parameters)
    {
        decimal baseAppeal = Math.Max(parameters.MinimumAppeal, elector.BlankVoteIntention);
        decimal seededOffset = GetSeededOffset(campaignSeed, elector.Id, "blank-vote");
        decimal variationFactor = 1m + (seededOffset * parameters.SeededVariation);
        return baseAppeal * variationFactor;
    }
}
}
