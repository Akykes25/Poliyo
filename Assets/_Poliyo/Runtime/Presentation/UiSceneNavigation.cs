using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Poliyo.Presentation
{
/// <summary>Owns navigation between the authored UGUI prototype scenes.</summary>
public sealed class UiSceneNavigation : MonoBehaviour
{
    public void OpenScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            throw new ArgumentException("A destination scene name is required.", nameof(sceneName));
        }

        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        UnityEngine.Application.Quit();
    }
}

/// <summary>Controls the calendar's press-interview drawer without coupling it to campaign simulation.</summary>
public sealed class CalendarInterviewDrawer : MonoBehaviour
{
    [SerializeField] private CanvasGroup _drawer;

    public void Configure(CanvasGroup drawer)
    {
        _drawer = drawer;
        SetVisible(false);
    }

    public void Toggle()
    {
        SetVisible(!IsVisible);
    }

    public void Close()
    {
        SetVisible(false);
    }

    private bool IsVisible => _drawer != null && _drawer.alpha > 0.5f;

    private void SetVisible(bool isVisible)
    {
        if (_drawer == null)
        {
            throw new InvalidOperationException("CalendarInterviewDrawer requires a drawer CanvasGroup.");
        }

        _drawer.alpha = isVisible ? 1f : 0f;
        _drawer.interactable = isVisible;
        _drawer.blocksRaycasts = isVisible;
    }
}

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
