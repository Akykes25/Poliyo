using System;
using UnityEngine;
using UnityEngine.UI;

namespace Poliyo.Presentation
{
/// <summary>Controls the calendar interview drawer and owns its modal input boundary.</summary>
public sealed class CalendarInterviewDrawer : MonoBehaviour
{
    [SerializeField] private CanvasGroup _drawer;
    [SerializeField] private Selectable _initialSelection;

    private ModalInteractionScope _interactionScope;

    public void Configure(CanvasGroup drawer, Selectable initialSelection)
    {
        _drawer = drawer;
        _initialSelection = initialSelection;
        _interactionScope = null;
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

    private void OnDisable()
    {
        _interactionScope?.Close();
    }

    private void SetVisible(bool isVisible)
    {
        if (_drawer == null)
        {
            throw new InvalidOperationException("CalendarInterviewDrawer requires a drawer CanvasGroup.");
        }

        if (isVisible)
        {
            _drawer.alpha = 1f;
            _drawer.interactable = true;
            _drawer.blocksRaycasts = true;
            GetInteractionScope().Open();
            return;
        }

        _interactionScope?.Close();
        _drawer.alpha = 0f;
        _drawer.interactable = false;
        _drawer.blocksRaycasts = false;
    }

    private ModalInteractionScope GetInteractionScope()
    {
        return _interactionScope ??= new ModalInteractionScope(_drawer.gameObject, _initialSelection);
    }
}
}
