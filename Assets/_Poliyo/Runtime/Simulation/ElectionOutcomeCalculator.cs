using System;
using System.Collections.Generic;

namespace Poliyo.Simulation
{
public static class ElectionOutcomeCalculator
{
    public static ElectionOutcome CalculateFirstRound(ElectionTally tally, IReadOnlyList<string> candidateIds)
    {
        if (tally == null) throw new ArgumentNullException(nameof(tally));
        if (candidateIds == null || candidateIds.Count < 2) throw new ArgumentException("At least two candidates are required.", nameof(candidateIds));

        var first = candidateIds[0];
        var second = candidateIds[1];
        for (var index = 1; index < candidateIds.Count; index++)
        {
            var candidate = candidateIds[index];
            if (tally.GetCandidateVotes(candidate) > tally.GetCandidateVotes(first))
            {
                second = first;
                first = candidate;
            }
            else if (candidate != first && tally.GetCandidateVotes(candidate) > tally.GetCandidateVotes(second))
            {
                second = candidate;
            }
        }

        var firstShare = tally.GetValidVoteShare(first);
        var secondShare = tally.GetValidVoteShare(second);
        var winsFirstRound = firstShare > 45m || (firstShare >= 40m && firstShare - secondShare >= 10m);

        return winsFirstRound
            ? new ElectionOutcome(first, null, null)
            : new ElectionOutcome(null, first, second);
    }
}

}
