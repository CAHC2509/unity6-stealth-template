using UnityEngine;

public static class TextFormatUtility
{
    public static string ToPercentText(float normalizedValue)
    {
        return Mathf.RoundToInt(normalizedValue * Constants.PERCENT_MULTIPLIER).ToString();
    }
}
