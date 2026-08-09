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
}
}
