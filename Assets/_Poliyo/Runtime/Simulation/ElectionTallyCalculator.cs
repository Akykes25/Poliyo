using System;
using System.Collections.Generic;

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
}
}