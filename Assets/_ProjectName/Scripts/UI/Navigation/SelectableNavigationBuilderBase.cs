using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public abstract class SelectableNavigationBuilderBase : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private RectTransform navigationRoot;
    [SerializeField] [Min(1)] private int collectionDepth = 1;

    [Header("Behaviour")]
    [SerializeField] private SelectableNavigationLoopMode loopMode = SelectableNavigationLoopMode.Linear;
    [SerializeField] private bool includeInactive = true;

    [Header("Overriding")]
    [SerializeField] private NavigationDirections directionsToOverride = NavigationDirections.All;

    protected SelectableNavigationLoopMode LoopMode => loopMode;

    protected abstract void BuildNavigation(List<Selectable> selectables);

    public void RebuildNavigation(RectTransform overrideRoot = null)
    {
        RectTransform targetRoot = overrideRoot != null ? overrideRoot : navigationRoot;
        List<Selectable> items = CollectSelectables(targetRoot);

        if (items.Count == 1)
        {
            Selectable singleItem = items[0];

            Navigation disabledNavigation = singleItem.navigation;
            disabledNavigation.mode = Navigation.Mode.None;
            ApplyNavigation(singleItem, disabledNavigation);
            return;
        }

        BuildNavigation(items);
    }

    protected void ApplyNavigation(Selectable selectable, Navigation navigation)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            Undo.RecordObject(selectable, "Update Navigation");
#endif

        selectable.navigation = navigation;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            EditorUtility.SetDirty(selectable);
#endif
    }

    protected void GetLinearNeighbours(List<Selectable> selectables, int index, out Selectable previous, out Selectable next)
    {
        previous = index > 0 ? selectables[index - 1] : null;
        next = index < selectables.Count - 1 ? selectables[index + 1] : null;

        if (LoopMode != SelectableNavigationLoopMode.ClosedLoop || selectables.Count <= 1) return;

        if (previous == null)
            previous = selectables[selectables.Count - 1];

        if (next == null)
            next = selectables[0];
    }

    protected List<Selectable> CollectSelectables(RectTransform root)
    {
        List<Selectable> result = new();
        CollectSelectablesRecursive(root, 1, result);
        return result;
    }

    private void CollectSelectablesRecursive(Transform current, int currentDepth, List<Selectable> list)
    {
        if (currentDepth > collectionDepth) return;

        for (int i = 0; i < current.childCount; i++)
        {
            Transform child = current.GetChild(i);

            if (!includeInactive && !child.gameObject.activeSelf)
                continue;

            if (child.TryGetComponent(out Selectable selectable))
                list.Add(selectable);

            if (currentDepth < collectionDepth)
                CollectSelectablesRecursive(child, currentDepth + 1, list);
        }
    }

    protected Navigation CreateNavigation(Selectable current, Selectable selectOnUp, Selectable selectOnDown, Selectable selectOnLeft, Selectable selectOnRight)
    {
        Navigation navigation = current.navigation;
        navigation.mode = Navigation.Mode.Explicit;

        if (directionsToOverride.HasFlag(NavigationDirections.Up))
            navigation.selectOnUp = selectOnUp;

        if (directionsToOverride.HasFlag(NavigationDirections.Down))
            navigation.selectOnDown = selectOnDown;

        if (directionsToOverride.HasFlag(NavigationDirections.Left))
            navigation.selectOnLeft = selectOnLeft;

        if (directionsToOverride.HasFlag(NavigationDirections.Right))
            navigation.selectOnRight = selectOnRight;

        return navigation;
    }

    protected static Vector2 GetAnchoredPosition(Selectable selectable)
    {
        RectTransform rectTransform = selectable.transform as RectTransform;
        return rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
    }
}