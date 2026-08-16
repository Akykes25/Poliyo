using System;
using System.Collections.Generic;

namespace Poliyo.Simulation
{
/// <summary>Resolved decision plus the persistent political memory it created.</summary>
public sealed class CampaignDecisionResolution
{
    public CampaignDecisionResolution(
        CampaignActionResolution actionResolution,
        CampaignDecisionRecord record,
        IReadOnlyList<PoliticalRelationshipChange> relationshipChanges,
        IReadOnlyList<PoliticalPromise> createdPromises)
    {
        ActionResolution = actionResolution ?? throw new ArgumentNullException(nameof(actionResolution));
        Record = record;
        RelationshipChanges = relationshipChanges ?? throw new ArgumentNullException(nameof(relationshipChanges));
        CreatedPromises = createdPromises ?? throw new ArgumentNullException(nameof(createdPromises));
    }

    public CampaignActionResolution ActionResolution { get; }
    public bool WasResolved => ActionResolution.WasPaid;
    public CampaignDecisionRecord Record { get; }
    public IReadOnlyList<PoliticalRelationshipChange> RelationshipChanges { get; }
    public IReadOnlyList<PoliticalPromise> CreatedPromises { get; }
}
}
