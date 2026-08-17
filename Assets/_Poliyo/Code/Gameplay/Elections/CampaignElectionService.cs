using System;
using System.Collections.Generic;

namespace Poliyo.Simulation
{
/// <summary>
/// Resolves an election from accumulated, weighted elector state. It never rerolls a result during presentation.
/// </summary>
public sealed class CampaignElectionService
{
    private readonly IReadOnlyList<string> _candidateIds;
    private readonly FinalVoteResolutionParameters _voteResolutionParameters;

    public CampaignElectionService(
        IReadOnlyList<string> candidateIds,
        FinalVoteResolutionParameters voteResolutionParameters = null)
    {
        if (candidateIds == null || candidateIds.Count < 2)
        {
            throw new ArgumentException("At least two candidates are required.", nameof(candidateIds));
        }

        var stableCandidateIds = new List<string>(candidateIds.Count);
        var uniqueCandidateIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (string candidateId in candidateIds)
        {
            if (string.IsNullOrWhiteSpace(candidateId) || !uniqueCandidateIds.Add(candidateId))
            {
                throw new ArgumentException("Candidate ids must be non-empty and unique.", nameof(candidateIds));
            }

            stableCandidateIds.Add(candidateId);
        }

        _candidateIds = stableCandidateIds.AsReadOnly();
        _voteResolutionParameters = voteResolutionParameters ?? FinalVoteResolutionParameters.VerticalSlicePrototype;
    }

    public CampaignElectionResult ResolveFirstRound(CampaignState campaign, IEnumerable<MicroElector> microElectors)
    {
        return ResolveFirstRound(campaign, microElectors, resolveUndecided: true);
    }

    /// <summary>Reproduces the pre-schema-5 tally for saves created before final vote resolution existed.</summary>
    public CampaignElectionResult ResolveFirstRoundLegacy(CampaignState campaign, IEnumerable<MicroElector> microElectors)
    {
        return ResolveFirstRound(campaign, microElectors, resolveUndecided: false);
    }

    /// <summary>Resolves the persistent second round for the two first-round finalists.</summary>
    public CampaignElectionResult ResolveRunoff(
        CampaignState campaign,
        IEnumerable<MicroElector> microElectors,
        IReadOnlyList<string> finalistIds)
    {
        if (campaign == null) throw new ArgumentNullException(nameof(campaign));
        if (microElectors == null) throw new ArgumentNullException(nameof(microElectors));
        ValidateFinalists(finalistIds);
        if (!campaign.Calendar.IsElectionDay)
        {
            throw new InvalidOperationException("Runoff results can only be resolved after the first election day.");
        }

        ElectionTally tally = ElectionTallyCalculator.CalculateRunoffResolved(
            microElectors,
            finalistIds,
            campaign.Seed,
            _voteResolutionParameters);
        ElectionOutcome outcome = ElectionOutcomeCalculator.CalculateRunoff(tally, finalistIds);
        foreach (string finalistId in finalistIds)
        {
            campaign.RecordCause(new CauseRecord(
                campaign.Calendar.CurrentDay,
                CauseCategory.Election,
                "runoff",
                finalistId,
                "valid-vote-share",
                tally.GetValidVoteShare(finalistId)));
        }

        return new CampaignElectionResult(tally, outcome);
    }

    private CampaignElectionResult ResolveFirstRound(
        CampaignState campaign,
        IEnumerable<MicroElector> microElectors,
        bool resolveUndecided)
    {
        if (campaign == null) throw new ArgumentNullException(nameof(campaign));
        if (microElectors == null) throw new ArgumentNullException(nameof(microElectors));
        if (!campaign.Calendar.IsElectionDay)
        {
            throw new InvalidOperationException("First-round results can only be resolved on election day.");
        }

        ElectionTally tally = resolveUndecided
            ? ElectionTallyCalculator.CalculateResolved(
                microElectors,
                _candidateIds,
                campaign.Seed,
                _voteResolutionParameters)
            : ElectionTallyCalculator.Calculate(microElectors, _candidateIds);
        ElectionOutcome outcome = ElectionOutcomeCalculator.CalculateFirstRound(tally, _candidateIds);
        foreach (string candidateId in _candidateIds)
        {
            campaign.RecordCause(new CauseRecord(
                campaign.Calendar.CurrentDay,
                CauseCategory.Election,
                "first-round",
                candidateId,
                "valid-vote-share",
                tally.GetValidVoteShare(candidateId)));
        }

        return new CampaignElectionResult(tally, outcome);
    }

    private void ValidateFinalists(IReadOnlyList<string> finalistIds)
    {
        if (finalistIds == null || finalistIds.Count != 2)
        {
            throw new ArgumentException("A runoff requires exactly two finalists.", nameof(finalistIds));
        }

        var finalistSet = new HashSet<string>(StringComparer.Ordinal);
        foreach (string finalistId in finalistIds)
        {
            if (string.IsNullOrWhiteSpace(finalistId) || !finalistSet.Add(finalistId) || !ContainsCandidate(finalistId))
            {
                throw new ArgumentException("Runoff finalists must be known, non-empty and distinct.", nameof(finalistIds));
            }
        }
    }

    private bool ContainsCandidate(string candidateId)
    {
        foreach (string knownCandidateId in _candidateIds)
        {
            if (knownCandidateId == candidateId) return true;
        }

        return false;
    }
}
}
