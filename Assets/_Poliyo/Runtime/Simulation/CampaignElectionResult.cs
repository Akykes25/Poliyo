using System;

namespace Poliyo.Simulation
{
public sealed class CampaignElectionResult
{
    public CampaignElectionResult(ElectionTally tally, ElectionOutcome outcome)
    {
        Tally = tally ?? throw new ArgumentNullException(nameof(tally));
        Outcome = outcome ?? throw new ArgumentNullException(nameof(outcome));
    }

    public ElectionTally Tally { get; }
    public ElectionOutcome Outcome { get; }
}
}
