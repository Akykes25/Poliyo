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
        bool hasStrictLead = firstShare > secondShare;
        var winsFirstRound = hasStrictLead &&
                             (firstShare > 45m || (firstShare >= 40m && firstShare - secondShare >= 10m));

        return winsFirstRound
            ? new ElectionOutcome(first, null, null)
            : new ElectionOutcome(null, first, second);
    }

    /// <summary>
    /// Resolves the two-candidate runoff. A tied weighted tally uses the stable
    /// finalist order as its deterministic tie-breaker until an explicit tie rule
    /// is introduced by the campaign design.
    /// </summary>
    public static ElectionOutcome CalculateRunoff(ElectionTally tally, IReadOnlyList<string> finalistIds)
    {
        if (tally == null) throw new ArgumentNullException(nameof(tally));
        if (finalistIds == null || finalistIds.Count != 2)
        {
            throw new ArgumentException("A runoff requires exactly two finalists.", nameof(finalistIds));
        }

        if (string.IsNullOrWhiteSpace(finalistIds[0]) || string.IsNullOrWhiteSpace(finalistIds[1]) ||
            finalistIds[0] == finalistIds[1])
        {
            throw new ArgumentException("Runoff finalists must be non-empty and distinct.", nameof(finalistIds));
        }

        string winnerId = tally.GetCandidateVotes(finalistIds[0]) >= tally.GetCandidateVotes(finalistIds[1])
            ? finalistIds[0]
            : finalistIds[1];
        return new ElectionOutcome(winnerId, null, null);
    }
}

}
