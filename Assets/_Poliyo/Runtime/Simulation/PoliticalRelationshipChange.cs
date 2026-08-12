using System;

namespace Poliyo.Simulation
{
/// <summary>Authored relationship consequences produced by one political decision.</summary>
public sealed class PoliticalRelationshipChange
{
    public PoliticalRelationshipChange(
        string actorId,
        decimal trustDelta,
        decimal affinityDelta,
        decimal obligationDelta,
        decimal grievanceDelta)
    {
        if (string.IsNullOrWhiteSpace(actorId)) throw new ArgumentException("A relationship change requires an actor.", nameof(actorId));

        ActorId = actorId;
        TrustDelta = trustDelta;
        AffinityDelta = affinityDelta;
        ObligationDelta = obligationDelta;
        GrievanceDelta = grievanceDelta;
    }

    public string ActorId { get; }
    public decimal TrustDelta { get; }
    public decimal AffinityDelta { get; }
    public decimal ObligationDelta { get; }
    public decimal GrievanceDelta { get; }
}
}
