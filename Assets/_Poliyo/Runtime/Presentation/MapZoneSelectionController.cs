using System;
using TMPro;
using UnityEngine;

namespace Poliyo.Presentation
{
/// <summary>Displays the selected jurisdiction in the map detail drawer. Territorial simulation is connected later.</summary>
public sealed class MapZoneSelectionController : MonoBehaviour
{
    [SerializeField] private TMP_Text _zoneName;
    [SerializeField] private TMP_Text _zoneDescription;

    public void Configure(TMP_Text zoneName, TMP_Text zoneDescription)
    {
        _zoneName = zoneName;
        _zoneDescription = zoneDescription;
        SelectZone("Seleccioná una jurisdicción");
    }

    public void SelectZone(string zoneName)
    {
        if (_zoneName == null || _zoneDescription == null)
        {
            throw new InvalidOperationException("MapZoneSelectionController requires both detail labels.");
        }

        _zoneName.text = zoneName;
        _zoneDescription.text = zoneName == "Seleccioná una jurisdicción"
            ? "Elegí una zona del mapa para consultar su información territorial."
            : "Datos territoriales y acciones disponibles: se conectarán con la simulación de campaña en el siguiente paso.";
    }
}
}
