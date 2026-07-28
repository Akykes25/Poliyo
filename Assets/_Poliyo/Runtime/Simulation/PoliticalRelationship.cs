using System;

namespace Poliyo.Simulation
{
/// <summary>Relationship is a multi-factor memory, not a single friendship score.</summary>
public sealed class PoliticalRelationship
{
    public PoliticalRelationship(string actorId)
    {
        if (string.IsNullOrWhiteSpace(actorId)) throw new ArgumentException("An actor id is required.", nameof(actorId));
        ActorId = actorId;
    }

    public string ActorId { get; }
    public decimal Trust { get; private set; }
    public decimal Affinity { get; private set; }
    public decimal Obligation { get; private set; }
    public decimal Grievance { get; private set; }

    public void Apply(decimal trustDelta, decimal affinityDelta, decimal obligationDelta, decimal grievanceDelta)
    {
        Trust = Clamp(Trust + trustDelta);
        Affinity = Clamp(Affinity + affinityDelta);
        Obligation = Clamp(Obligation + obligationDelta);
        Grievance = Clamp(Grievance + grievanceDelta);
    }

    private static decimal Clamp(decimal value) => Math.Min(100m, Math.Max(-100m, value));
}

}
