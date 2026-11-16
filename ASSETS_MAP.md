# UI Prefab and Asset Map

This document describes the hierarchy and inventory of the base UI prefabs (`Assets/_ProjectName/Prefabs/UI/BaseElements/`). It is updated as the project grows.

---

## Base UI Library (`Prefabs/UI/BaseElements/`)

```
ScreenCanvas.prefab
└── DefaultBackground.prefab

NormalButton.prefab (Button + Image)
└── NormalButtonText.prefab (TMP + LocalizeStringEvent)

NormalText.prefab / NormalTitle.prefab (TMP + LocalizeStringEvent)
├── SmallText.prefab = NormalText variant (secondary text)
└── Subtitle.prefab = NormalTitle variant (subtitles)

Card.prefab (Background with LayoutElement ignoreLayout + VerticalLayoutGroup + ContentSizeFitter)

ConfirmationWindow(Base).prefab (ConfirmationWindowController + CanvasGroup)
├── DefaultBackground.prefab (nested, veil)
├── NormalText.prefab (nested, message)
└── NormalButton.prefab (nested, SubmitButton)
    ├── ConfirmationWindow(Single).prefab = Base variant + SingleButtonConfirmationWindowView
    └── ConfirmationWindow(Dual).prefab = Base variant + TwoButtonConfirmationWindowView

Toggle.prefab + ToggleItem.prefab
NormalSlider.prefab + SliderItem.prefab
Dropdown.prefab + DropdownItem.prefab
ScrollView.prefab
ProgressBar.prefab
InputRebinder_.prefab (ControlBindingController + ControlBindingView + CanvasGroup)
LoadingBanner.prefab (LoadingEffectsController)
```

## DevTools (`Prefabs/DevTools/`)

```
DevTools.prefab (DevToolsManager + logic, UI and view controllers)
└── DevToolsCanvas = ScreenCanvas.prefab (sortingOrder 1000)
    ├── Overlay (CanvasGroup, does not block raycasts) with non-localized NormalText
    └── Panel (CanvasGroup) with NormalTitle, NormalButton (close and tabs) and one root per tab
        that nests NormalText, NormalButton, ToggleItem, SliderItem and ScrollView
```
The Gameplay tab commands are instantiated at runtime from `NormalButton.prefab` and `ToggleItem.prefab`.
