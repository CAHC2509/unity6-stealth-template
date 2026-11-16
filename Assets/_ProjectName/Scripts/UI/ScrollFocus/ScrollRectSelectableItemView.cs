using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollRectSelectableItemView : ViewBase, ISelectHandler
{
    private ScrollRectSelectionFollower selectionFollower;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
    }

    public override void Initialize(params object[] parameters)
    {
        selectionFollower = parameters[0] as ScrollRectSelectionFollower;

        base.Initialize(parameters);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (selectionFollower == null) return;

        selectionFollower.Focus(rectTransform);
    }
}
