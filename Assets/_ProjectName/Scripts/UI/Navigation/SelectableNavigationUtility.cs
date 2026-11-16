using UnityEngine.UI;

public static class SelectableNavigationUtility
{
    public static void LinkVertically(Selectable upper, Selectable lower)
    {
        Navigation upperNavigation = upper.navigation;
        upperNavigation.mode = Navigation.Mode.Explicit;
        upperNavigation.selectOnDown = lower;
        upper.navigation = upperNavigation;

        Navigation lowerNavigation = lower.navigation;
        lowerNavigation.mode = Navigation.Mode.Explicit;
        lowerNavigation.selectOnUp = upper;
        lower.navigation = lowerNavigation;
    }
}
