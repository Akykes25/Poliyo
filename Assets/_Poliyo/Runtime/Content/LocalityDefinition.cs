using System;
using UnityEngine;

namespace Poliyo.Content
{
[CreateAssetMenu(menuName = "Poliyo/Content/Locality", fileName = "LocalityDefinition")]
public sealed class LocalityDefinition : ScriptableObject
{
    [SerializeField] private string _id;
    [SerializeField] private string _jurisdictionId;
    [SerializeField] private string _displayName;
    [SerializeField, Min(1)] private int _populationWeight = 1;

    public string Id => _id;
    public string JurisdictionId => _jurisdictionId;
    public string DisplayName => _displayName;
    public int PopulationWeight => _populationWeight;

    public void Configure(string id, string jurisdictionId, string displayName, int populationWeight)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A locality id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(jurisdictionId)) throw new ArgumentException("A jurisdiction id is required.", nameof(jurisdictionId));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("A locality display name is required.", nameof(displayName));
        if (populationWeight < 1) throw new ArgumentOutOfRangeException(nameof(populationWeight));

        _id = id;
        _jurisdictionId = jurisdictionId;
        _displayName = displayName;
        _populationWeight = populationWeight;
    }
}
}