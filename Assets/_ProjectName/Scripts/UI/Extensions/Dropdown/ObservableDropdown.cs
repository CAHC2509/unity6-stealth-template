using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using MEC;

[RequireComponent(typeof(VerticalSelectableNavigationBuilder))]
public class ObservableDropdown : TMP_Dropdown
{
    public event Action Opened;
    public event Action Closed;

    private CoroutineHandle monitorHandle;
    private VerticalSelectableNavigationBuilder navigationBuilder;
    private RectTransform currentDropdownList;

    protected override void Awake()
    {
        base.Awake();
        navigationBuilder = GetComponent<VerticalSelectableNavigationBuilder>();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        bool wasExpanded = IsExpanded;

        base.OnPointerClick(eventData);

        if (!wasExpanded && IsExpanded)
            NotifyOpened();
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        bool wasExpanded = IsExpanded;

        base.OnSubmit(eventData);

        if (!wasExpanded && IsExpanded)
            NotifyOpened();
    }

    protected override GameObject CreateDropdownList(GameObject template)
    {
        GameObject dropdownList = base.CreateDropdownList(template);
        currentDropdownList = dropdownList.transform as RectTransform;
        return dropdownList;
    }

    private void NotifyOpened()
    {
        if (monitorHandle.IsRunning) return;

        Opened?.Invoke();

        monitorHandle = Timing.RunCoroutine(MonitorDropdown().CancelWith(gameObject));
    }

    private IEnumerator<float> MonitorDropdown()
    {
        yield return Timing.WaitForOneFrame;

        if (currentDropdownList != null)
        {
            Canvas.ForceUpdateCanvases();
            navigationBuilder.RebuildNavigation(currentDropdownList);

            if (currentDropdownList.TryGetComponent(out ScrollRectSelectionFollower follower))
            {
                ScrollRectSelectableItemView[] items = currentDropdownList.GetComponentsInChildren<ScrollRectSelectableItemView>();

                foreach (ScrollRectSelectableItemView item in items)
                    item.Initialize(follower);

                if (value >= 0 && value < items.Length)
                    follower.Focus(items[value].transform as RectTransform);
            }
        }

        while (IsExpanded)
            yield return Timing.WaitForOneFrame;

        Closed?.Invoke();
        currentDropdownList = null;
    }

    protected override void OnDisable()
    {
        if (monitorHandle.IsRunning)
        {
            Timing.KillCoroutines(monitorHandle);
            Closed?.Invoke();
        }

        currentDropdownList = null;

        base.OnDisable();
    }
}