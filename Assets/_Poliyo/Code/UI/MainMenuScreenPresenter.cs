using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Poliyo.Presentation
{
/// <summary>Reflects autosave availability in the main menu without making the view read persistence files.</summary>
public sealed class MainMenuScreenPresenter : MonoBehaviour
{
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _loadButton;
    [SerializeField] private Button _newCampaignButton;
    [SerializeField] private TMP_Text _saveStatusLabel;

    public void Configure(
        Button continueButton,
        Button loadButton,
        Button newCampaignButton,
        TMP_Text saveStatusLabel)
    {
        _continueButton = continueButton;
        _loadButton = loadButton;
        _newCampaignButton = newCampaignButton;
        _saveStatusLabel = saveStatusLabel;
    }

    private void Start()
    {
        if (_continueButton == null || _loadButton == null || _newCampaignButton == null || _saveStatusLabel == null)
        {
            throw new InvalidOperationException("MainMenuScreenPresenter is missing required UI references.");
        }

        CampaignGameSessionHost host = GetHost();
        bool hasAutosave = host.HasAutosave;
        _continueButton.interactable = hasAutosave;
        _loadButton.interactable = hasAutosave;
        _saveStatusLabel.text = hasAutosave
            ? "AUTOSAVE DISPONIBLE · CONTINUÁ O CARGALO"
            : "SIN AUTOSAVE · INICIÁ UNA NUEVA CAMPAÑA";

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(
                hasAutosave ? _continueButton.gameObject : _newCampaignButton.gameObject);
        }
    }

    public void StartNewCampaign()
    {
        GetHost().StartNewCampaign();
    }

    public void LoadAutosave()
    {
        GetHost().LoadAutosave();
    }

    private static CampaignGameSessionHost GetHost()
    {
        return CampaignGameSessionHost.Current ??
               throw new InvalidOperationException("Main menu requires a CampaignGameSessionHost.");
    }
}
}
