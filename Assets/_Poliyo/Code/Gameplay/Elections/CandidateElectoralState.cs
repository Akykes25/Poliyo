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
        VotingIntention = Clamp(votingIntention);
        // Trust is a quality signal for an already-existing electoral preference
        // in the vertical slice. It can never exceed that preference. Keeping the
        // invariant in the domain object also protects restored saves and future
        // content from reintroducing the reversed presentation seen in the UI.
        Trust = Math.Min(Clamp(trust), VotingIntention);
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
                Trust = Math.Min(Clamp(Trust + delta), VotingIntention);
                break;
            case ElectoralMetric.VotingIntention:
                SetVotingIntention(VotingIntention + delta);
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
        Trust = Math.Min(Trust, VotingIntention);
    }
    private static decimal Clamp(decimal value) => Math.Min(100m, Math.Max(0m, value));
}

}
