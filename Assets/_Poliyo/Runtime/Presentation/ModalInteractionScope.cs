using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Poliyo.Presentation
{
/// <summary>
/// Keeps keyboard/gamepad focus inside a visible modal without overwriting
/// domain-driven changes to the underlying controls while the modal is open.
/// </summary>
internal sealed class ModalInteractionScope
{
    private readonly GameObject _modalRoot;
    private readonly Selectable _initialSelection;
    private readonly List<CanvasGroup> _interactionBlockers = new List<CanvasGroup>();
    private Transform _interactionRoot;
    private GameObject _previousSelection;

    public ModalInteractionScope(GameObject modalRoot, Selectable initialSelection)
    {
        _modalRoot = modalRoot;
        _initialSelection = initialSelection;
    }

    public bool IsOpen { get; private set; }

    public void Open()
    {
        if (IsOpen || _modalRoot == null)
        {
            return;
        }

        EventSystem eventSystem = EventSystem.current;
        _previousSelection = eventSystem != null ? eventSystem.currentSelectedGameObject : null;
        _interactionBlockers.Clear();

        Transform modalTransform = _modalRoot.transform;
        Canvas rootCanvas = _modalRoot.GetComponentInParent<Canvas>();
        _interactionRoot = rootCanvas != null ? rootCanvas.rootCanvas.transform : modalTransform.root;
        var blockedObjects = new HashSet<GameObject>();
        foreach (Selectable selectable in _interactionRoot.GetComponentsInChildren<Selectable>(includeInactive: true))
        {
            if (selectable == null ||
                selectable.transform.IsChildOf(modalTransform) ||
                !blockedObjects.Add(selectable.gameObject))
            {
                continue;
            }

            CanvasGroup blocker = selectable.gameObject.AddComponent<CanvasGroup>();
            blocker.interactable = false;
            _interactionBlockers.Add(blocker);
        }

        IsOpen = true;
        Selectable target = IsFocusable(_initialSelection)
            ? _initialSelection
            : _modalRoot.GetComponentsInChildren<Selectable>(includeInactive: false).FirstOrDefault(IsFocusable);
        eventSystem?.SetSelectedGameObject(target != null ? target.gameObject : null);
    }

    public void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        foreach (CanvasGroup blocker in _interactionBlockers)
        {
            if (blocker != null)
            {
                blocker.interactable = true;
                DestroyBlocker(blocker);
            }
        }

        _interactionBlockers.Clear();
        IsOpen = false;

        EventSystem eventSystem = EventSystem.current;
        GameObject restoreTarget = IsFocusable(_previousSelection)
            ? _previousSelection
            : FindFallbackSelection();
        eventSystem?.SetSelectedGameObject(restoreTarget);
        _previousSelection = null;
        _interactionRoot = null;
    }

    private static bool IsFocusable(Selectable selectable)
    {
        return selectable != null && selectable.IsInteractable() && selectable.gameObject.activeInHierarchy;
    }

    private static bool IsFocusable(GameObject target)
    {
        return target != null && IsFocusable(target.GetComponent<Selectable>());
    }

    private GameObject FindFallbackSelection()
    {
        if (_interactionRoot == null)
        {
            return null;
        }

        Transform modalTransform = _modalRoot != null ? _modalRoot.transform : null;
        Selectable fallback = _interactionRoot
            .GetComponentsInChildren<Selectable>(includeInactive: false)
            .FirstOrDefault(selectable =>
                (modalTransform == null || !selectable.transform.IsChildOf(modalTransform)) &&
                IsFocusable(selectable));
        return fallback != null ? fallback.gameObject : null;
    }

    private static void DestroyBlocker(CanvasGroup blocker)
    {
        if (UnityEngine.Application.isPlaying)
        {
            Object.Destroy(blocker);
            return;
        }

        Object.DestroyImmediate(blocker);
    }
}
}
