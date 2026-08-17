using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Poliyo.Presentation
{
/// <summary>Owns navigation between the authored UGUI prototype scenes.</summary>
public sealed class UiSceneNavigation : MonoBehaviour
{
    public void OpenScene(string sceneName)
    {
        OpenSceneInternal(sceneName, false);
    }

    /// <summary>Returns to the campaign dashboard even when the campaign already has a terminal readout.</summary>
    public void OpenCampaignDashboard()
    {
        OpenSceneInternal("CampaignSlice", true);
    }

    private static void OpenSceneInternal(string sceneName, bool allowFinishedCampaignDashboard)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            throw new ArgumentException("A destination scene name is required.", nameof(sceneName));
        }

        if (sceneName == "CampaignSlice" && CampaignGameSessionHost.Current != null &&
            !CampaignGameSessionHost.Current.IsInitialTeamSelectionComplete)
        {
            SceneManager.LoadScene("TeamSelection");
            return;
        }

        if (sceneName == "CampaignSlice" && !allowFinishedCampaignDashboard && CampaignGameSessionHost.Current != null &&
            (CampaignGameSessionHost.Current.Session.IsPlayerCampaignFinished || CampaignGameSessionHost.Current.Session.SliceClosureResult != null))
        {
            SceneManager.LoadScene("ElectionResult");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        UnityEngine.Application.Quit();
    }
}

}
