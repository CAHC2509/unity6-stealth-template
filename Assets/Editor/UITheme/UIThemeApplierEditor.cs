using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class UIThemeApplierEditor
{
    private const string MENU_PATH = "Tools/UI/Apply Theme";
    private const string BASE_FOLDER = Constants.UI_BASE_ELEMENTS_FOLDER;
    private const string SQUARE_SPRITE_PATH = "Assets/_ProjectName/Art2D/UI/UI_SquareFill.png";
    private const string CIRCLE_SPRITE_PATH = "Assets/_ProjectName/Art2D/UI/UI_CircleFill.png";
    private const string CHECK_SPRITE_PATH = "Assets/_ProjectName/Art2D/UI/UI_Check.png";
    private const string CHEVRON_SPRITE_PATH = "Assets/_ProjectName/Art2D/UI/UI_Chevron.png";
    private const string SCENES_FOLDER = "Assets/_ProjectName/Scenes";
    private const string PREFABS_FOLDER = "Assets/_ProjectName/Prefabs";

    private const string BUTTON = BASE_FOLDER + "Buttons/NormalButton.prefab";
    private const string BUTTON_TEXT = BASE_FOLDER + "Buttons/NormalButtonText.prefab";
    private const string TEXT = BASE_FOLDER + "Texts/NormalText.prefab";
    private const string SMALL_TEXT = BASE_FOLDER + "Texts/SmallText.prefab";
    private const string TITLE = BASE_FOLDER + "Texts/NormalTitle.prefab";
    private const string SUBTITLE = BASE_FOLDER + "Texts/Subtitle.prefab";
    private const string TOGGLE = BASE_FOLDER + "Toggles/Toggle.prefab";
    private const string TOGGLE_ITEM = BASE_FOLDER + "Toggles/ToggleItem.prefab";
    private const string SLIDER = BASE_FOLDER + "Sliders/NormalSlider.prefab";
    private const string SLIDER_ITEM = BASE_FOLDER + "Sliders/SliderItem.prefab";
    private const string DROPDOWN = BASE_FOLDER + "Dropdowns/Dropdown.prefab";
    private const string DROPDOWN_ITEM = BASE_FOLDER + "Dropdowns/DropdownItem.prefab";
    private const string SCROLL_VIEW = BASE_FOLDER + "Scrolls/ScrollView.prefab";
    private const string BACKGROUND = BASE_FOLDER + "Miscellaneous/DefaultBackground.prefab";
    private const string SCREEN_CANVAS = BASE_FOLDER + "Miscellaneous/ScreenCanvas.prefab";
    private const string PROGRESS_BAR = BASE_FOLDER + "ProgressBars/ProgressBar.prefab";
    private const string LOADING_BANNER = BASE_FOLDER + "LoadingBanner/LoadingBanner.prefab";
    private const string INPUT_REBINDER = BASE_FOLDER + "InputRebinders/InputRebinder_.prefab";
    private const string CONFIRMATION_WINDOW = BASE_FOLDER + "ConfirmationWindows/ConfirmationWindow(Base).prefab";
    private const string CARD = BASE_FOLDER + "Miscellaneous/Card.prefab";
    private const string CONFIRMATION_WINDOW_DUAL = BASE_FOLDER + "ConfirmationWindows/ConfirmationWindow(Dual).prefab";
    private const string CONFIRMATION_WINDOW_SINGLE = BASE_FOLDER + "ConfirmationWindows/ConfirmationWindow(Single).prefab";
    private const string CARD_BACKGROUND_NAME = "Background";

    private const string PACKAGE_FOLDER = "Assets/StealthController/";
    private const string PACKAGE_PAUSE_MENU = PACKAGE_FOLDER + "Prefabs/UI/PauseMenu.prefab";
    private const string PACKAGE_OUTCOME_BANNER = PACKAGE_FOLDER + "Prefabs/UI/OutcomeBanner.prefab";
    private const string PACKAGE_GAME = PACKAGE_FOLDER + "Prefabs/Game/StealthGame.prefab";
    private const string PACKAGE_SQUARE_SPRITE = PACKAGE_FOLDER + "Art/UI/UI_SquareFill.png";
    private const string PACKAGE_BOLD_FONT = PACKAGE_FOLDER + "Art/Fonts/Lexend/F_Lexend-Bold SDF.asset";
    private const string PACKAGE_MEDIUM_FONT = PACKAGE_FOLDER + "Art/Fonts/Lexend/F_Lexend-Medium SDF.asset";
    private const float PACKAGE_MENU_WIDTH = 480f;

    private static readonly string[] StylePropertyPrefixes =
    {
        "m_fontAsset", "m_sharedMaterial", "m_fontSize", "m_fontSizeBase", "m_fontSizeMin", "m_fontSizeMax", "m_enableAutoSizing",
        "m_fontColor", "m_fontColor32", "m_fontStyle", "m_fontWeight",
        "m_Color", "m_Sprite", "m_Type", "m_PixelsPerUnitMultiplier", "m_Colors", "m_Transition",
        "m_UiScaleMode", "m_ReferenceResolution", "m_ScreenMatchMode", "m_MatchWidthOrHeight",
        "m_Padding", "m_Spacing"
    };

    private static UIThemeConfig theme;
    private static Sprite squareSprite;
    private static Sprite circleSprite;
    private static Sprite checkSprite;
    private static Sprite chevronSprite;

    [MenuItem(MENU_PATH)]
    public static void ApplyTheme()
    {
        theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(Constants.UI_THEME_ASSET_PATH);
        squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SQUARE_SPRITE_PATH);
        circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CIRCLE_SPRITE_PATH);
        checkSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CHECK_SPRITE_PATH);
        chevronSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CHEVRON_SPRITE_PATH);

        if (theme == null)
        {
            Debug.LogError($"UIThemeApplierEditor: no theme found at '{Constants.UI_THEME_ASSET_PATH}'.");
            return;
        }

        StyleBasePrefabs();
        StylePackagePrefabs();
        int reverted = RevertStyleOverridesInPrefabs() + RevertStyleOverridesInScenes();

        AssetDatabase.SaveAssets();
        Debug.Log($"UIThemeApplierEditor: theme applied to the base elements; {reverted} style overrides reverted.");
    }

    private static void StyleBasePrefabs()
    {
        Edit(SCREEN_CANVAS, StyleScreenCanvas);
        Edit(BUTTON_TEXT, StyleButtonText);
        Edit(TEXT, root => StyleText(root.GetComponent<TMP_Text>(), theme.BodyFont, theme.BodySize, theme.TextPrimary));
        Edit(SMALL_TEXT, root => StyleText(root.GetComponent<TMP_Text>(), theme.BodyFont, theme.SmallSize, theme.TextSecondary));
        Edit(TITLE, root => StyleText(root.GetComponent<TMP_Text>(), theme.TitleFont, theme.TitleSize, theme.TextPrimary));
        Edit(SUBTITLE, root => StyleText(root.GetComponent<TMP_Text>(), theme.HeadingFont, theme.SubtitleSize, theme.TextSecondary));
        Edit(BUTTON, StyleButton);
        Edit(TOGGLE, root => StyleToggle(root.GetComponent<Toggle>()));
        Edit(SLIDER, root => StyleSlider(root.GetComponent<Slider>()));
        Edit(TOGGLE_ITEM, StyleToggleItem);
        Edit(SLIDER_ITEM, StyleSliderItem);
        Edit(DROPDOWN, root => StyleDropdown(root.GetComponent<TMP_Dropdown>()));
        Edit(DROPDOWN_ITEM, StyleDropdownItem);
        Edit(SCROLL_VIEW, StyleScrollView);
        Edit(BACKGROUND, root => StyleImage(root.GetComponent<Image>(), theme.Background));
        Edit(PROGRESS_BAR, StyleProgressBar);
        Edit(LOADING_BANNER, StyleLoadingBanner);
        Edit(INPUT_REBINDER, StyleInputRebinder);
        Edit(CONFIRMATION_WINDOW, StyleConfirmationWindow);
        Edit(CARD, StyleCard);
        Edit(CONFIRMATION_WINDOW_DUAL, EnableConfirmationBody);
        Edit(CONFIRMATION_WINDOW_SINGLE, EnableConfirmationBody);
    }

    private static void StylePackagePrefabs()
    {
        Sprite packageSquare = AssetDatabase.LoadAssetAtPath<Sprite>(PACKAGE_SQUARE_SPRITE);
        TMP_FontAsset bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PACKAGE_BOLD_FONT);
        TMP_FontAsset medium = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PACKAGE_MEDIUM_FONT);

        Edit(PACKAGE_GAME, root =>
        {
            foreach (CanvasScaler scaler in root.GetComponentsInChildren<CanvasScaler>(true))
                StyleScreenCanvas(scaler.gameObject);
        });

        Edit(PACKAGE_OUTCOME_BANNER, root =>
        {
            root.transform.Find("DefaultBackground").GetComponent<Image>().color = theme.Veil;
            StyleText(root.transform.Find("OutcomeMessage").GetComponent<TMP_Text>(), bold, theme.TitleSize, theme.TextPrimary);
        });

        Edit(PACKAGE_PAUSE_MENU, root =>
        {
            root.transform.Find("DefaultBackground").GetComponent<Image>().color = theme.Veil;
            StyleText(root.transform.Find("PauseTitle").GetComponent<TMP_Text>(), bold, theme.TitleSize, theme.TextPrimary);

            Transform panel = root.transform.Find("ButtonsPanel");
            StyleCardLayout(panel.gameObject, packageSquare);

            RectTransform panelRect = (RectTransform)panel;
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, theme.TitleSize * 2f);
            panelRect.sizeDelta = new Vector2(PACKAGE_MENU_WIDTH, 0f);

            foreach (Button button in panel.GetComponentsInChildren<Button>(true))
            {
                Image image = button.GetComponent<Image>();
                StyleImage(image, Color.white, theme.ControlCornerRadius, packageSquare);
                button.targetGraphic = image;
                button.transition = Selectable.Transition.ColorTint;
                button.colors = theme.CreateControlColors();
                SetLayoutHeight(button.gameObject, theme.ButtonHeight);

                TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
                StyleText(label, medium, theme.ButtonSize, theme.TextPrimary);
                label.textWrappingMode = TextWrappingModes.NoWrap;
            }
        });
    }

    private static void StyleCardLayout(GameObject root, Sprite sprite)
    {
        Image rootImage = root.GetComponent<Image>();
        if (rootImage != null)
            UnityEngine.Object.DestroyImmediate(rootImage);

        Transform background = root.transform.Find(CARD_BACKGROUND_NAME);
        if (background == null)
        {
            GameObject backgroundObject = new GameObject(CARD_BACKGROUND_NAME, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            background = backgroundObject.transform;
            background.SetParent(root.transform, false);
        }

        background.SetAsFirstSibling();
        RectTransform backgroundRect = (RectTransform)background;
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        background.GetComponent<LayoutElement>().ignoreLayout = true;

        Image backgroundImage = background.GetComponent<Image>();
        StyleImage(backgroundImage, theme.Surface, theme.CardCornerRadius, sprite);
        backgroundImage.raycastTarget = false;

        VerticalLayoutGroup layout = GetOrAdd<VerticalLayoutGroup>(root);
        int padding = Mathf.RoundToInt(theme.CardPadding);
        layout.padding = new RectOffset(padding, padding, padding, padding);
        layout.spacing = theme.Spacing;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = GetOrAdd<ContentSizeFitter>(root);
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private static void Edit(string path, Action<GameObject> style)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
        {
            Debug.LogWarning($"UIThemeApplierEditor: '{path}' not found, skipped.");
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(path);

        try
        {
            style(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void StyleScreenCanvas(GameObject root)
    {
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
    }

    private static void StyleButton(GameObject root)
    {
        StyleSelectable(root.GetComponent<Button>(), root.GetComponent<Image>(), theme.CreateControlColors());

        TMP_Text label = root.GetComponentInChildren<TMP_Text>(true);
        StyleText(label, theme.HeadingFont, theme.ButtonSize, theme.TextPrimary);
        label.textWrappingMode = TextWrappingModes.NoWrap;
        SetLayoutHeight(root, theme.ButtonHeight);
    }

    private static void StyleButtonText(GameObject root)
    {
        TMP_Text label = root.GetComponent<TMP_Text>();
        StyleText(label, theme.HeadingFont, theme.ButtonSize, theme.TextPrimary);
        label.textWrappingMode = TextWrappingModes.NoWrap;
    }

    private static void StyleCard(GameObject root)
    {
        Image rootImage = root.GetComponent<Image>();
        if (rootImage != null)
            UnityEngine.Object.DestroyImmediate(rootImage);

        Transform background = root.transform.Find(CARD_BACKGROUND_NAME);
        if (background == null)
        {
            GameObject backgroundObject = new GameObject(CARD_BACKGROUND_NAME, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            background = backgroundObject.transform;
            background.SetParent(root.transform, false);
        }

        background.SetAsFirstSibling();
        RectTransform backgroundRect = (RectTransform)background;
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        background.GetComponent<LayoutElement>().ignoreLayout = true;

        Image backgroundImage = background.GetComponent<Image>();
        StyleImage(backgroundImage, theme.Surface, theme.CardCornerRadius);
        backgroundImage.raycastTarget = false;

        VerticalLayoutGroup layout = root.GetComponent<VerticalLayoutGroup>();
        int padding = Mathf.RoundToInt(theme.CardPadding);
        layout.padding = new RectOffset(padding, padding, padding, padding);
        layout.spacing = theme.Spacing;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
    }

    private static void SetLayoutHeight(GameObject root, float height)
    {
        LayoutElement element = root.GetComponent<LayoutElement>();
        if (element == null)
            element = root.AddComponent<LayoutElement>();

        element.minHeight = height;
        element.preferredHeight = height;
        element.flexibleHeight = -1f;
    }

    private static void StyleToggle(Toggle toggle)
    {
        Image background = (Image)toggle.targetGraphic;
        StyleSelectable(toggle, background, theme.CreateControlColors());
        SetSize(toggle.transform, theme.ControlSize, theme.ControlSize);

        if (toggle.graphic is Image checkmark)
            StyleIcon(checkmark, checkSprite, theme.TextPrimary);
    }

    private static void StyleSlider(Slider slider)
    {
        Transform root = slider.transform;
        StyleImage(root.Find("Background").GetComponent<Image>(), theme.Control);
        StyleImage(slider.fillRect.GetComponent<Image>(), theme.Accent);

        Image handle = slider.handleRect.GetComponent<Image>();
        handle.sprite = circleSprite;
        handle.type = Image.Type.Simple;
        handle.color = Color.white;
        handle.preserveAspect = true;
        slider.targetGraphic = handle;
        slider.transition = Selectable.Transition.ColorTint;
        slider.colors = theme.CreateHandleColors();
    }

    private static void StyleItemRoot(GameObject root)
    {
        StyleImage(root.GetComponent<Image>(), theme.SurfaceAlternate);
        SetHeight(root.transform, theme.ItemHeight);
        SetLayoutHeight(root, theme.ItemHeight);
    }

    private static void StyleItemLabel(TMP_Text label)
    {
        StyleText(label, theme.BodyFont, theme.LabelSize, theme.TextPrimary);
        label.alignment = TextAlignmentOptions.MidlineLeft;
        SetHorizontalInset((RectTransform)label.transform, theme.ItemPadding);
    }

    private static void StyleToggleItem(GameObject root)
    {
        StyleItemRoot(root);
        StyleItemLabel(FindText(root, "NormalText_DropdownTitle"));

        Toggle toggle = root.GetComponentInChildren<Toggle>(true);
        StyleToggle(toggle);
        AnchorRight((RectTransform)toggle.transform, theme.ItemPadding);
    }

    private static void StyleSliderItem(GameObject root)
    {
        StyleItemRoot(root);
        StyleItemLabel(FindText(root, "NormalText_SliderTitle"));

        TMP_Text value = FindText(root, "NormalText_SliderValue");
        StyleText(value, theme.HeadingFont, theme.LabelSize, theme.TextSecondary);
        value.alignment = TextAlignmentOptions.MidlineRight;
        AnchorRightStretched((RectTransform)value.transform, theme.ItemPadding);

        StyleSlider(root.GetComponentInChildren<Slider>(true));
    }

    private static void StyleDropdown(TMP_Dropdown dropdown)
    {
        StyleSelectable(dropdown, dropdown.GetComponent<Image>(), theme.CreateControlColors());
        SetHeight(dropdown.transform, theme.ControlSize);

        if (dropdown.captionText != null)
            StyleText(dropdown.captionText, theme.BodyFont, theme.LabelSize, theme.TextPrimary);

        Transform arrow = dropdown.transform.Find("Arrow");
        if (arrow != null)
        {
            StyleIcon(arrow.GetComponent<Image>(), chevronSprite, theme.TextPrimary);
            SetSize(arrow, theme.ControlSize, theme.ControlSize);
        }

        RectTransform template = dropdown.template;
        StyleImage(template.GetComponent<Image>(), theme.Surface);

        Toggle item = template.GetComponentInChildren<Toggle>(true);
        StyleSelectable(item, (Image)item.targetGraphic, theme.CreateControlColors());
        if (item.graphic is Image itemCheckmark)
            StyleIcon(itemCheckmark, checkSprite, theme.Accent);

        if (dropdown.itemText != null)
            StyleText(dropdown.itemText, theme.BodyFont, theme.LabelSize, theme.TextPrimary);

        Scrollbar scrollbar = template.GetComponentInChildren<Scrollbar>(true);
        if (scrollbar != null)
            StyleScrollbar(scrollbar);
    }

    private static void StyleDropdownItem(GameObject root)
    {
        StyleItemRoot(root);
        StyleItemLabel(FindText(root, "NormalText_DropdownTitle"));

        TMP_Dropdown dropdown = root.GetComponentInChildren<TMP_Dropdown>(true);
        StyleDropdown(dropdown);
        AnchorRight((RectTransform)dropdown.transform, theme.ItemPadding);
    }

    private static void StyleScrollView(GameObject root)
    {
        StyleImage(root.GetComponent<Image>(), theme.SurfaceAlternate);

        ScrollRect scrollRect = root.GetComponent<ScrollRect>();
        float scrollbarWidth = 0f;

        foreach (Scrollbar scrollbar in root.GetComponentsInChildren<Scrollbar>(true))
        {
            StyleScrollbar(scrollbar);
            scrollbarWidth = ((RectTransform)scrollbar.transform).sizeDelta.x;
        }

        RectTransform viewport = scrollRect.viewport;
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.pivot = new Vector2(0f, 1f);
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = new Vector2(-scrollbarWidth, 0f);

        RectTransform content = scrollRect.content;
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        int padding = Mathf.RoundToInt(theme.Spacing);
        layout.padding = new RectOffset(padding, padding, padding, padding);
        layout.spacing = theme.Spacing;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        LayoutElement contentElement = content.GetComponent<LayoutElement>();
        if (contentElement != null)
            UnityEngine.Object.DestroyImmediate(contentElement);
    }

    private static void StyleScrollbar(Scrollbar scrollbar)
    {
        StyleImage(scrollbar.GetComponent<Image>(), theme.Surface);
        StyleSelectable(scrollbar, scrollbar.handleRect.GetComponent<Image>(), theme.CreateControlColors());
    }

    private static void StyleProgressBar(GameObject root)
    {
        StyleImage(root.transform.Find("Background").GetComponent<Image>(), theme.Control);

        Image fill = root.transform.Find("Background/ProgressBarFill").GetComponent<Image>();
        fill.sprite = squareSprite;
        fill.color = theme.Accent;

        root.transform.Find("Border").GetComponent<Image>().color = Color.clear;
    }

    private static void StyleLoadingBanner(GameObject root)
    {
        StyleImage(root.GetComponent<Image>(), theme.Surface);
        StyleText(root.GetComponentInChildren<TMP_Text>(true), theme.HeadingFont, theme.LabelSize, theme.TextSecondary);
        root.transform.Find("LoadingIcon/Background").GetComponent<Image>().color = theme.Control;
        root.transform.Find("LoadingIcon/LoadingFill").GetComponent<Image>().color = theme.Accent;
    }

    private static void StyleInputRebinder(GameObject root)
    {
        ColorBlock rowColors = theme.CreateControlColors();
        rowColors.normalColor = theme.SurfaceAlternate;
        StyleSelectable(root.GetComponent<Button>(), root.GetComponent<Image>(), rowColors);
        SetHeight(root.transform, theme.ItemHeight);
        SetLayoutHeight(root, theme.ItemHeight);

        List<TMP_Text> texts = new List<TMP_Text>(root.GetComponentsInChildren<TMP_Text>(true));
        if (texts.Count > 0)
        {
            StyleText(texts[0], theme.BodyFont, theme.LabelSize, theme.TextPrimary);
            texts[0].alignment = TextAlignmentOptions.MidlineLeft;
        }

        if (texts.Count > 1)
        {
            StyleText(texts[1], theme.HeadingFont, theme.LabelSize, theme.TextPrimary);
            texts[1].alignment = TextAlignmentOptions.Center;
        }

        foreach (Transform child in root.transform)
        {
            if (child.name == "Separator")
                child.GetComponent<Image>().color = theme.Control;
        }

        Transform reset = root.transform.Find("ResetToDefaultButton");
        if (reset != null)
        {
            StyleSelectable(reset.GetComponent<Button>(), reset.GetComponent<Image>(), theme.CreateControlColors());
            reset.Find("Icon").GetComponent<Image>().color = theme.TextPrimary;
        }
    }

    private static void StyleConfirmationWindow(GameObject root)
    {
        StyleImage(root.transform.Find("DefaultBackground").GetComponent<Image>(), theme.Veil);

        Transform body = root.transform.Find("Body");
        StyleImage(body.GetComponent<Image>(), theme.Surface, theme.CardCornerRadius);

        foreach (string sectionName in new[] { "InfoSection", "ButtonsSection" })
        {
            Image section = body.Find(sectionName).GetComponent<Image>();
            section.color = Color.clear;
        }

        TMP_Text message = body.Find("InfoSection").GetComponentInChildren<TMP_Text>(true);
        StyleText(message, theme.BodyFont, theme.BodySize, theme.TextPrimary);

        foreach (Button button in body.GetComponentsInChildren<Button>(true))
            StyleButton(button.gameObject);

        LayoutConfirmationWindow(body);
    }

    private static void EnableConfirmationBody(GameObject root)
    {
        root.transform.Find("Body").GetComponent<Image>().enabled = true;
    }

    private static void LayoutConfirmationWindow(Transform body)
    {
        RectTransform bodyRect = (RectTransform)body;
        bodyRect.sizeDelta = new Vector2(bodyRect.sizeDelta.x, 0f);

        VerticalLayoutGroup bodyLayout = GetOrAdd<VerticalLayoutGroup>(body.gameObject);
        int padding = Mathf.RoundToInt(theme.CardPadding);
        bodyLayout.padding = new RectOffset(padding, padding, padding, padding);
        bodyLayout.spacing = theme.SectionSpacing;
        bodyLayout.childAlignment = TextAnchor.UpperCenter;
        bodyLayout.childControlWidth = true;
        bodyLayout.childControlHeight = true;
        bodyLayout.childForceExpandWidth = true;
        bodyLayout.childForceExpandHeight = false;

        ContentSizeFitter bodyFitter = GetOrAdd<ContentSizeFitter>(body.gameObject);
        bodyFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        bodyFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Transform info = body.Find("InfoSection");
        LayoutElement infoElement = GetOrAdd<LayoutElement>(info.gameObject);
        infoElement.minHeight = theme.ButtonHeight * 2f;
        infoElement.preferredHeight = theme.ButtonHeight * 2f;

        RectTransform messageRect = (RectTransform)info.GetComponentInChildren<TMP_Text>(true).transform;
        messageRect.anchorMin = Vector2.zero;
        messageRect.anchorMax = Vector2.one;
        messageRect.offsetMin = Vector2.zero;
        messageRect.offsetMax = Vector2.zero;

        Transform buttons = body.Find("ButtonsSection");
        HorizontalLayoutGroup buttonsLayout = buttons.GetComponent<HorizontalLayoutGroup>();
        buttonsLayout.spacing = theme.Spacing;
        buttonsLayout.childAlignment = TextAnchor.MiddleCenter;
        buttonsLayout.childControlWidth = true;
        buttonsLayout.childControlHeight = true;
        buttonsLayout.childForceExpandWidth = true;
        buttonsLayout.childForceExpandHeight = false;
        SetLayoutHeight(buttons.gameObject, theme.ButtonHeight);
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component != null ? component : target.AddComponent<T>();
    }

    private static void StyleIcon(Image image, Sprite sprite, Color color)
    {
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        image.color = color;
    }

    private static void StyleSelectable(Selectable selectable, Image target, ColorBlock colors)
    {
        StyleImage(target, Color.white);
        selectable.targetGraphic = target;
        selectable.transition = Selectable.Transition.ColorTint;
        selectable.colors = colors;
    }

    private static void StyleImage(Image image, Color color)
    {
        StyleImage(image, color, theme.ControlCornerRadius);
    }

    private static void StyleImage(Image image, Color color, float cornerRadius)
    {
        StyleImage(image, color, cornerRadius, squareSprite);
    }

    private static void StyleImage(Image image, Color color, float cornerRadius, Sprite sprite)
    {
        if (image == null) return;

        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = color;
        image.pixelsPerUnitMultiplier = sprite.border.x > 0f ? sprite.border.x / cornerRadius : 1f;
    }

    private static void StyleText(TMP_Text text, TMP_FontAsset font, float size, Color color)
    {
        if (text == null) return;

        text.font = font;
        text.fontSharedMaterial = font.material;
        text.fontStyle = FontStyles.Normal;
        text.color = color;
        text.enableAutoSizing = true;
        text.fontSizeMax = size;
        text.fontSizeMin = Mathf.Round(size * theme.AutoSizeMinRatio);
        text.fontSize = size;
    }

    private static TMP_Text FindText(GameObject root, string name)
    {
        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.name == name)
                return text;
        }

        return null;
    }

    private static void SetSize(Transform target, float width, float height)
    {
        ((RectTransform)target).sizeDelta = new Vector2(width, height);
    }

    private static void SetHeight(Transform target, float height)
    {
        RectTransform rect = (RectTransform)target;
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
    }

    private static void SetHorizontalInset(RectTransform rect, float inset)
    {
        rect.anchoredPosition = new Vector2(inset, rect.anchoredPosition.y);
    }

    private static void AnchorRightStretched(RectTransform rect, float inset)
    {
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.anchoredPosition = new Vector2(-inset, 0f);
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, 0f);
    }

    private static void AnchorRight(RectTransform rect, float inset)
    {
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.anchoredPosition = new Vector2(-inset, 0f);
    }

    private static int RevertStyleOverridesInPrefabs()
    {
        int reverted = 0;

        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PREFABS_FOLDER }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            bool isVariant = PrefabUtility.GetPrefabAssetType(AssetDatabase.LoadAssetAtPath<GameObject>(path)) == PrefabAssetType.Variant;
            if (path.StartsWith(BASE_FOLDER) && !isVariant) continue;
            if (path == SMALL_TEXT || path == SUBTITLE) continue;

            GameObject root = PrefabUtility.LoadPrefabContents(path);

            try
            {
                int count = RevertStyleOverrides(root);
                if (count > 0)
                    PrefabUtility.SaveAsPrefabAsset(root, path);

                reverted += count;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        return reverted;
    }

    private static int RevertStyleOverridesInScenes()
    {
        int reverted = 0;
        string activeScene = SceneManager.GetActiveScene().path;

        foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { SCENES_FOLDER }))
        {
            Scene scene = EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guid), OpenSceneMode.Single);
            int count = 0;

            foreach (GameObject root in scene.GetRootGameObjects())
                count += RevertStyleOverrides(root);

            if (count > 0)
                EditorSceneManager.SaveScene(scene);

            reverted += count;
        }

        if (!string.IsNullOrEmpty(activeScene))
            EditorSceneManager.OpenScene(activeScene, OpenSceneMode.Single);

        return reverted;
    }

    private static int RevertStyleOverrides(GameObject root)
    {
        int reverted = 0;
        HashSet<GameObject> visited = new HashSet<GameObject>();

        foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
        {
            GameObject instanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(transform.gameObject);
            if (instanceRoot == null || !visited.Add(instanceRoot)) continue;

            PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(instanceRoot);
            if (modifications == null) continue;

            List<PropertyModification> kept = new List<PropertyModification>(modifications.Length);

            foreach (PropertyModification modification in modifications)
            {
                if (IsBaseElementStyle(modification))
                {
                    reverted++;
                    continue;
                }

                kept.Add(modification);
            }

            if (kept.Count != modifications.Length)
                PrefabUtility.SetPropertyModifications(instanceRoot, kept.ToArray());
        }

        return reverted;
    }

    private static bool IsBaseElementStyle(PropertyModification modification)
    {
        if (modification.target == null) return false;
        if (!AssetDatabase.GetAssetPath(modification.target).StartsWith(BASE_FOLDER)) return false;

        foreach (string prefix in StylePropertyPrefixes)
        {
            if (modification.propertyPath.StartsWith(prefix))
                return true;
        }

        return false;
    }
}
