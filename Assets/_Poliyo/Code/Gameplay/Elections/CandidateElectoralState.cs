using System;

namespace Poliyo.Simulation
{
public sealed class CandidateElectoralState
{
    public CandidateElectoralState(string candidateId, decimal trust, decimal votingIntention, decimal rejection)
    {
        if (string.IsNullOrWhiteSpace(candidateId))
        {
            throw new ArgumentException("A candidate requires an id.", nameof(candidateId));
        }

        CandidateId = candidateId;
        Trust = Clamp(trust);
        VotingIntention = Clamp(votingIntention);
        Rejection = Clamp(rejection);
    }

    public string CandidateId { get; }
    public decimal Trust { get; private set; }
    public decimal VotingIntention { get; private set; }
    public decimal Rejection { get; private set; }

    public void Apply(ElectoralMetric metric, decimal delta)
    {
        switch (metric)
        {
            case ElectoralMetric.Trust:
                Trust = Clamp(Trust + delta);
                break;
            case ElectoralMetric.VotingIntention:
                VotingIntention = Clamp(VotingIntention + delta);
                break;
            case ElectoralMetric.Rejection:
                Rejection = Clamp(Rejection + delta);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(metric), metric, "Candidate metrics do not own participation.");
        }
    }

    internal void SetVotingIntention(decimal votingIntention)
    {
        VotingIntention = Clamp(votingIntention);
    }
    private static decimal Clamp(decimal value) => Math.Min(100m, Math.Max(0m, value));
}

}
