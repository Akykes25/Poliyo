using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Poliyo.Simulation
{
/// <summary>
/// Whole-vote read model for player-facing election screens.
/// The simulation keeps weighted decimal precision; this snapshot only normalizes
/// the values shown as people/votes and preserves the rounded participation total.
/// </summary>
public sealed class ElectionVoteCounts
{
    private readonly IReadOnlyDictionary<string, long> _candidateVotes;

    internal ElectionVoteCounts(
        IReadOnlyDictionary<string, long> candidateVotes,
        long participatingVotes,
        long validVotes,
        long blankVotes,
        long undecidedVotes)
    {
        _candidateVotes = candidateVotes ?? throw new ArgumentNullException(nameof(candidateVotes));
        ParticipatingVotes = participatingVotes;
        ValidVotes = validVotes;
        BlankVotes = blankVotes;
        UndecidedVotes = undecidedVotes;
    }

    public IReadOnlyDictionary<string, long> CandidateVotes => _candidateVotes;
    public long ParticipatingVotes { get; }
    public long ValidVotes { get; }
    public long BlankVotes { get; }
    public long UndecidedVotes { get; }

    public long GetCandidateVotes(string candidateId)
    {
        if (string.IsNullOrWhiteSpace(candidateId)) return 0L;
        return _candidateVotes.TryGetValue(candidateId, out long votes) ? votes : 0L;
    }
}

/// <summary>
/// Converts weighted election totals into a coherent whole-vote display without
/// mutating the exact tally used by the election rules.
/// </summary>
public static class ElectionVoteCountNormalizer
{
    private const string BlankBucketId = "__blank";
    private const string UndecidedBucketId = "__undecided";

    public static ElectionVoteCounts Normalize(ElectionTally tally, IReadOnlyList<string> candidateIds)
    {
        if (tally == null) throw new ArgumentNullException(nameof(tally));
        if (candidateIds == null || candidateIds.Count == 0)
        {
            throw new ArgumentException("At least one candidate is required.", nameof(candidateIds));
        }

        var candidateSet = new HashSet<string>(StringComparer.Ordinal);
        var buckets = new List<Bucket>(candidateIds.Count + 2);
        foreach (string candidateId in candidateIds)
        {
            if (string.IsNullOrWhiteSpace(candidateId) || !candidateSet.Add(candidateId))
            {
                throw new ArgumentException("Candidate ids must be non-empty and unique.", nameof(candidateIds));
            }

            buckets.Add(new Bucket(candidateId, tally.GetCandidateVotes(candidateId), buckets.Count));
        }

        foreach (string candidateId in tally.CandidateVotes.Keys)
        {
            if (!candidateSet.Contains(candidateId))
            {
                throw new ArgumentException("The display candidate list does not contain every tally candidate.", nameof(candidateIds));
            }
        }

        buckets.Add(new Bucket(BlankBucketId, tally.BlankVotes, buckets.Count));
        buckets.Add(new Bucket(UndecidedBucketId, tally.UndecidedVotes, buckets.Count));

        long targetTotal = ToWholeCount(tally.ParticipatingWeight);
        long assignedTotal = 0L;
        foreach (Bucket bucket in buckets)
        {
            bucket.WholeValue = ToWholeCountFloor(bucket.ExactValue);
            assignedTotal = checked(assignedTotal + bucket.WholeValue);
            bucket.Fraction = bucket.ExactValue - bucket.WholeValue;
        }

        long remaining = targetTotal - assignedTotal;
        if (remaining > 0L)
        {
            DistributeRemainder(buckets, remaining);
        }
        else if (remaining < 0L)
        {
            throw new InvalidOperationException(
                "The election tally cannot be normalized because its participation total is smaller than its vote buckets.");
        }

        var candidateVotes = new Dictionary<string, long>(candidateIds.Count, StringComparer.Ordinal);
        long validVotes = 0L;
        foreach (Bucket bucket in buckets)
        {
            if (bucket.Id == BlankBucketId || bucket.Id == UndecidedBucketId) continue;

            candidateVotes.Add(bucket.Id, bucket.WholeValue);
            validVotes = checked(validVotes + bucket.WholeValue);
        }

        long blankVotes = FindBucket(buckets, BlankBucketId).WholeValue;
        long undecidedVotes = FindBucket(buckets, UndecidedBucketId).WholeValue;
        return new ElectionVoteCounts(
            new ReadOnlyDictionary<string, long>(candidateVotes),
            targetTotal,
            validVotes,
            blankVotes,
            undecidedVotes);
    }

    private static void DistributeRemainder(List<Bucket> buckets, long amount)
    {
        var ordered = new List<Bucket>(buckets);
        ordered.Sort((left, right) =>
        {
            int comparison = right.Fraction.CompareTo(left.Fraction);
            return comparison != 0 ? comparison : left.Order.CompareTo(right.Order);
        });

        for (long index = 0L; index < amount; index++)
        {
            Bucket bucket = ordered[(int)(index % ordered.Count)];
            bucket.WholeValue = checked(bucket.WholeValue + 1L);
        }
    }

    private static Bucket FindBucket(IEnumerable<Bucket> buckets, string id)
    {
        foreach (Bucket bucket in buckets)
        {
            if (bucket.Id == id) return bucket;
        }

        throw new InvalidOperationException("The requested vote bucket does not exist.");
    }

    private static long ToWholeCount(decimal value)
    {
        if (value < 0m) throw new ArgumentOutOfRangeException(nameof(value));
        return checked((long)decimal.Round(value, 0, MidpointRounding.AwayFromZero));
    }

    private static long ToWholeCountFloor(decimal value)
    {
        if (value < 0m) throw new ArgumentOutOfRangeException(nameof(value));
        return checked((long)decimal.Truncate(value));
    }

    private sealed class Bucket
    {
        public Bucket(string id, decimal exactValue, int order)
        {
            Id = id;
            ExactValue = exactValue;
            Order = order;
        }

        public string Id { get; }
        public decimal ExactValue { get; }
        public int Order { get; }
        public decimal Fraction { get; set; }
        public long WholeValue { get; set; }
    }
}
}
