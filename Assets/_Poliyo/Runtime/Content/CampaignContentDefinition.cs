using System;
using UnityEngine;

namespace Poliyo.Content
{
[CreateAssetMenu(menuName = "Poliyo/Content/Campaign Catalog", fileName = "CampaignContentDefinition")]
public sealed class CampaignContentDefinition : ScriptableObject
{
    [SerializeField] private LocalityDefinition[] _localities = Array.Empty<LocalityDefinition>();

    public LocalityDefinition[] Localities => _localities;

    public void Configure(LocalityDefinition[] localities)
    {
        if (localities == null) throw new ArgumentNullException(nameof(localities));
        _localities = (LocalityDefinition[])localities.Clone();
    }
}
}