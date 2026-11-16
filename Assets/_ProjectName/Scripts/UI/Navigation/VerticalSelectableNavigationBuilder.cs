using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VerticalSelectableNavigationBuilder : SelectableNavigationBuilderBase
{
    protected override void BuildNavigation(List<Selectable> selectables)
    {
        selectables.Sort((a, b) =>
        {
            Vector2 posA = GetAnchoredPosition(a);
            Vector2 posB = GetAnchoredPosition(b);

            int yCompare = posB.y.CompareTo(posA.y);
            if (yCompare != 0)
                return yCompare;

            return posA.x.CompareTo(posB.x);
        });

        for (int i = 0; i < selectables.Count; ++i)
        {
            Selectable current = selectables[i];

            GetLinearNeighbours(selectables, i, out Selectable selectUp, out Selectable selectDown);

            ApplyNavigation(current, CreateNavigation(current, selectUp, selectDown, null, null));
        }
    }
}
