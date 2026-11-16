using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "UITheme", menuName = "UI/Theme")]
public class UIThemeConfig : ScriptableObject
{
    [Header("Fonts")]
    [SerializeField] private TMP_FontAsset titleFont;
    [SerializeField] private TMP_FontAsset headingFont;
    [SerializeField] private TMP_FontAsset bodyFont;

    [Header("Surfaces")]
    [SerializeField] private Color background;
    [SerializeField] private Color surface;
    [SerializeField] private Color surfaceAlternate;
    [SerializeField] private Color veil;

    [Header("Controls")]
    [SerializeField] private Color control;
    [SerializeField] private Color controlHighlighted;
    [SerializeField] private Color accent;
    [SerializeField] private Color accentPressed;
    [SerializeField] private Color disabled;

    [Header("Text")]
    [SerializeField] private Color textPrimary;
    [SerializeField] private Color textSecondary;

    [Header("Font sizes")]
    [SerializeField, Min(1f)] private float titleSize;
    [SerializeField, Min(1f)] private float subtitleSize;
    [SerializeField, Min(1f)] private float bodySize;
    [SerializeField, Min(1f)] private float labelSize;
    [SerializeField, Min(1f)] private float buttonSize;
    [SerializeField, Min(1f)] private float smallSize;
    [SerializeField, Range(Constants.UI_THEME_MIN_AUTO_SIZE_RATIO, 1f), Tooltip("Smallest size auto-sized text may shrink to, as a ratio of its normal size.")] private float autoSizeMinRatio;

    [Header("Metrics")]
    [SerializeField, Min(0f)] private float itemHeight;
    [SerializeField, Min(0f)] private float buttonHeight;
    [SerializeField, Min(0f)] private float controlSize;
    [SerializeField, Min(0f)] private float cardPadding;
    [SerializeField, Min(0f)] private float itemPadding;
    [SerializeField, Min(0f)] private float spacing;
    [SerializeField, Min(0f)] private float sectionSpacing;
    [SerializeField, Min(1f)] private float controlCornerRadius;
    [SerializeField, Min(1f)] private float cardCornerRadius;

    public TMP_FontAsset TitleFont => titleFont;
    public TMP_FontAsset HeadingFont => headingFont;
    public TMP_FontAsset BodyFont => bodyFont;

    public Color Background => background;
    public Color Surface => surface;
    public Color SurfaceAlternate => surfaceAlternate;
    public Color Veil => veil;
    public Color Control => control;
    public Color ControlHighlighted => controlHighlighted;
    public Color Accent => accent;
    public Color AccentPressed => accentPressed;
    public Color Disabled => disabled;
    public Color TextPrimary => textPrimary;
    public Color TextSecondary => textSecondary;

    public float TitleSize => titleSize;
    public float SubtitleSize => subtitleSize;
    public float BodySize => bodySize;
    public float LabelSize => labelSize;
    public float ButtonSize => buttonSize;
    public float SmallSize => smallSize;
    public float AutoSizeMinRatio => autoSizeMinRatio;

    public float ItemHeight => itemHeight;
    public float ButtonHeight => buttonHeight;
    public float ControlSize => controlSize;
    public float CardPadding => cardPadding;
    public float ItemPadding => itemPadding;
    public float Spacing => spacing;
    public float SectionSpacing => sectionSpacing;
    public float ControlCornerRadius => controlCornerRadius;
    public float CardCornerRadius => cardCornerRadius;

    public ColorBlock CreateControlColors()
    {
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = control;
        colors.highlightedColor = controlHighlighted;
        colors.pressedColor = accentPressed;
        colors.selectedColor = accent;
        colors.disabledColor = disabled;
        return colors;
    }

    public ColorBlock CreateHandleColors()
    {
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = textPrimary;
        colors.highlightedColor = textPrimary;
        colors.pressedColor = accentPressed;
        colors.selectedColor = accent;
        colors.disabledColor = disabled;
        return colors;
    }
}
