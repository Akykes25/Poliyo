using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Poliyo.Presentation
{
/// <summary>Provides the runtime EventSystem required for UGUI pointer and navigation input.</summary>
internal static class RuntimeUiInputBootstrap
{
    public static void Ensure()
    {
        EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            var eventSystemObject = new GameObject("PoliyoEventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        if (inputModule.actionsAsset == null)
        {
            inputModule.AssignDefaultActions();
        }
    }
}
}
