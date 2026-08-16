using System;
using System.Text;
using TMPro;
using UnityEngine;

namespace Poliyo.Presentation
{
/// <summary>Projects selected jurisdiction content into the map's details drawer.</summary>
public sealed class CampaignMapScreenPresenter : MonoBehaviour
{
    [SerializeField] private TMP_Text _zoneName;
    [SerializeField] private TMP_Text _zoneDescription;

    private CampaignGameSessionHost _host;

    public void Configure(TMP_Text zoneName, TMP_Text zoneDescription)
    {
        _zoneName = zoneName;
        _zoneDescription = zoneDescription;
    }

    private void Start()
    {
        _host = CampaignGameSessionHost.Current ?? throw new InvalidOperationException("Campaign map requires a CampaignGameSessionHost.");
        _host.StateChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (_host != null)
        {
            _host.StateChanged -= Refresh;
        }
    }

    public void SelectJurisdiction(string jurisdictionId)
    {
        _host.SelectJurisdiction(jurisdictionId);
    }

    private void Refresh()
    {
        if (string.IsNullOrWhiteSpace(_host.SelectedJurisdictionId))
        {
            _zoneName.text = "Seleccioná una jurisdicción";
            _zoneDescription.text = "Elegí una zona para consultar sus localidades y concentrar allí las próximas acciones.";
            return;
        }

        _zoneName.text = FormatJurisdictionName(_host.SelectedJurisdictionId);
        var text = new StringBuilder("Localidades: ");
        var localities = _host.GetSelectedJurisdictionLocalities();
        for (var index = 0; index < localities.Count; index++)
        {
            if (index > 0)
            {
                text.Append(" · ");
            }

            text.Append(localities[index].DisplayName);
        }

        text.Append("\n\nLas actividades del calendario impactarán esta jurisdicción hasta que selecciones otra.");
        _zoneDescription.text = text.ToString();
    }

    private static string FormatJurisdictionName(string jurisdictionId)
    {
        string[] words = jurisdictionId.Split('-');
        for (var index = 0; index < words.Length; index++)
        {
            if (words[index].Length > 0)
            {
                words[index] = char.ToUpperInvariant(words[index][0]) + words[index].Substring(1);
            }
        }

        return string.Join(" ", words);
    }
}
}
