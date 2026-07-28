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

    public CampaignElectionService(IReadOnlyList<string> candidateIds)
    {
        if (candidateIds == null || candidateIds.Count < 2)
        {
            throw new ArgumentException("At least two candidates are required.", nameof(candidateIds));
        }

        _candidateIds = candidateIds;
    }

    public CampaignElectionResult ResolveFirstRound(CampaignState campaign, IEnumerable<MicroElector> microElectors)
    {
        if (campaign == null) throw new ArgumentNullException(nameof(campaign));
        if (microElectors == null) throw new ArgumentNullException(nameof(microElectors));
        if (!campaign.Calendar.IsElectionDay)
        {
            throw new InvalidOperationException("First-round results can only be resolved on election day.");
        }

        ElectionTally tally = ElectionTallyCalculator.Calculate(microElectors, _candidateIds);
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
