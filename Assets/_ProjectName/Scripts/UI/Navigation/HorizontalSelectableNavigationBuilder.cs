using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HorizontalSelectableNavigationBuilder : SelectableNavigationBuilderBase
{
    protected override void BuildNavigation(List<Selectable> selectables)
    {
        selectables.Sort((a, b) =>
        {
            Vector2 posA = GetAnchoredPosition(a);
            Vector2 posB = GetAnchoredPosition(b);

            int xCompare = posA.x.CompareTo(posB.x);
            if (xCompare != 0)
                return xCompare;

            return posB.y.CompareTo(posA.y);
        });

        for (int i = 0; i < selectables.Count; ++i)
        {
            Selectable current = selectables[i];

            GetLinearNeighbours(selectables, i, out Selectable selectLeft, out Selectable selectRight);

            ApplyNavigation(current, CreateNavigation(current, null, null, selectLeft, selectRight));
        }
    }
}
