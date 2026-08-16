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

        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        UnityEngine.Application.Quit();
    }
}

}
