using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class GraphicsSettingsView : CancelableUIViewBase
{
    [Header("Transition settings")]
    [SerializeField] private UIFadeTransitionData fadeTransitionData;

    [Header("Graphics settings")]
    [SerializeField] private ObservableDropdown resolutionDropdown;
    [SerializeField] private ObservableDropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private Button restoreDefaultSettingsButton;

    [Header("Optional Rows")]
    [SerializeField, Tooltip("Row that contains the resolution dropdown, hidden on platforms without windowed resolutions.")] private GameObject resolutionItem;
    [SerializeField, Tooltip("Row that contains the fullscreen toggle, hidden on platforms without windowed mode.")] private GameObject fullscreenItem;
    [SerializeField, Tooltip("Row that contains the V-Sync toggle, hidden on platforms that manage V-Sync themselves.")] private GameObject vSyncItem;

    [Header("Quality Localization")]
    [SerializeField] private QualityLevelLocalizationMapping[] qualityLevelLocalizations;

    private readonly List<string> qualityOptions = new List<string>();
    private readonly List<Selectable> navigationChain = new List<Selectable>();
    private readonly Dictionary<string, string> localizedQualityNames = new Dictionary<string, string>();
    private LocalizedString.ChangeHandler[] qualityNameHandlers = Array.Empty<LocalizedString.ChangeHandler>();
    private string[] qualityFallbackNames = Array.Empty<string>();
    private GraphicsSettingsController graphicsSettingsController;

    protected override UITransitionBase CreateTransition() => new UIFadeTransition(fadeTransitionData);

    protected override void Awake()
    {
        defaultSelection = resolutionDropdown;

        base.Awake();
    }

    public override void Initialize(params object[] parameters)
    {
        graphicsSettingsController = parameters[0] as GraphicsSettingsController;

        RequireDependency(graphicsSettingsController, nameof(graphicsSettingsController));
        RequireDependency(resolutionDropdown, nameof(resolutionDropdown));
        RequireDependency(qualityDropdown, nameof(qualityDropdown));
        RequireDependency(fullscreenToggle, nameof(fullscreenToggle));
        RequireDependency(vSyncToggle, nameof(vSyncToggle));
        RequireDependency(restoreDefaultSettingsButton, nameof(restoreDefaultSettingsButton));
        RequireDependency(resolutionItem, nameof(resolutionItem));
        RequireDependency(fullscreenItem, nameof(fullscreenItem));
        RequireDependency(vSyncItem, nameof(vSyncItem));

        CreateQualityNameHandlers();

        base.Initialize(parameters);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        resolutionDropdown.Opened += RegisterSubViewOpen;
        resolutionDropdown.Closed += RegisterSubViewClose;

        qualityDropdown.Opened += RegisterSubViewOpen;
        qualityDropdown.Closed += RegisterSubViewClose;

        resolutionDropdown.onValueChanged.AddListener(graphicsSettingsController.UpdateResolution);
        qualityDropdown.onValueChanged.AddListener(graphicsSettingsController.UpdateQuality);
        fullscreenToggle.onValueChanged.AddListener(graphicsSettingsController.UpdateFullscreen);
        vSyncToggle.onValueChanged.AddListener(graphicsSettingsController.UpdateVSync);
        restoreDefaultSettingsButton.onClick.AddListener(graphicsSettingsController.RestoreDefaultSettings);

        for (int i = 0; i < qualityLevelLocalizations.Length; i++)
        {
            if (qualityLevelLocalizations[i].LocalizedName.IsEmpty) continue;

            qualityLevelLocalizations[i].LocalizedName.StringChanged += qualityNameHandlers[i];
        }
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        resolutionDropdown.Opened -= RegisterSubViewOpen;
        resolutionDropdown.Closed -= RegisterSubViewClose;

        qualityDropdown.Opened -= RegisterSubViewOpen;
        qualityDropdown.Closed -= RegisterSubViewClose;

        resolutionDropdown.onValueChanged.RemoveListener(graphicsSettingsController.UpdateResolution);
        qualityDropdown.onValueChanged.RemoveListener(graphicsSettingsController.UpdateQuality);
        fullscreenToggle.onValueChanged.RemoveListener(graphicsSettingsController.UpdateFullscreen);
        vSyncToggle.onValueChanged.RemoveListener(graphicsSettingsController.UpdateVSync);
        restoreDefaultSettingsButton.onClick.RemoveListener(graphicsSettingsController.RestoreDefaultSettings);

        for (int i = 0; i < qualityLevelLocalizations.Length; i++)
            qualityLevelLocalizations[i].LocalizedName.StringChanged -= qualityNameHandlers[i];
    }

    public void SetOptionalSettingsAvailable(bool isResolutionAvailable, bool isFullscreenAvailable, bool isVSyncAvailable)
    {
        resolutionItem.SetActive(isResolutionAvailable);
        fullscreenItem.SetActive(isFullscreenAvailable);
        vSyncItem.SetActive(isVSyncAvailable);

        defaultSelection = isResolutionAvailable ? resolutionDropdown : qualityDropdown;
        currentSelection = defaultSelection;

        RebuildNavigation();
    }

    public void SetResolutionOptions(List<string> options)
    {
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
    }

    public void SetQualityOptions(string[] fallbackNames)
    {
        qualityFallbackNames = fallbackNames;

        WarnAboutMissingQualityLocalizations();
        RefreshQualityOptions();
    }

    public void SetData(GraphicsSettingsData data, int resolutionIndex)
    {
        resolutionDropdown.SetValueWithoutNotify(resolutionIndex);
        qualityDropdown.SetValueWithoutNotify(data.QualityIndex);
        fullscreenToggle.SetIsOnWithoutNotify(data.IsFullscreen);
        vSyncToggle.SetIsOnWithoutNotify(data.IsVSyncEnabled);
    }

    protected override void OnCancelRequested()
    {
        graphicsSettingsController.CloseSubMenu();
    }

    private void RebuildNavigation()
    {
        navigationChain.Clear();

        if (resolutionItem.activeSelf)
            navigationChain.Add(resolutionDropdown);

        navigationChain.Add(qualityDropdown);

        if (fullscreenItem.activeSelf)
            navigationChain.Add(fullscreenToggle);

        if (vSyncItem.activeSelf)
            navigationChain.Add(vSyncToggle);

        navigationChain.Add(restoreDefaultSettingsButton);

        for (int i = 0; i < navigationChain.Count - 1; i++)
            SelectableNavigationUtility.LinkVertically(navigationChain[i], navigationChain[i + 1]);
    }

    private void CreateQualityNameHandlers()
    {
        qualityNameHandlers = new LocalizedString.ChangeHandler[qualityLevelLocalizations.Length];

        for (int i = 0; i < qualityLevelLocalizations.Length; i++)
        {
            string qualityName = qualityLevelLocalizations[i].QualityName;
            qualityNameHandlers[i] = localizedName => OnQualityNameLocalized(qualityName, localizedName);
        }
    }

    private void OnQualityNameLocalized(string qualityName, string localizedName)
    {
        localizedQualityNames[qualityName] = localizedName;
        RefreshQualityOptions();
    }

    private void RefreshQualityOptions()
    {
        if (qualityFallbackNames.Length == 0) return;

        int selectedIndex = qualityDropdown.value;

        qualityOptions.Clear();
        foreach (string qualityName in qualityFallbackNames)
            qualityOptions.Add(localizedQualityNames.TryGetValue(qualityName, out string localizedName) ? localizedName : qualityName);

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(qualityOptions);
        qualityDropdown.SetValueWithoutNotify(selectedIndex);
    }

    private void WarnAboutMissingQualityLocalizations()
    {
        foreach (string qualityName in qualityFallbackNames)
        {
            if (HasQualityLocalization(qualityName)) continue;

            Debug.LogWarning($"[{name}] Missing localized name for quality level '{qualityName}'.", this);
        }
    }

    private bool HasQualityLocalization(string qualityName)
    {
        foreach (QualityLevelLocalizationMapping mapping in qualityLevelLocalizations)
        {
            if (mapping.QualityName == qualityName && !mapping.LocalizedName.IsEmpty)
                return true;
        }

        return false;
    }
}

[Serializable]
public struct QualityLevelLocalizationMapping
{
    [SerializeField] private string qualityName;
    [SerializeField] private LocalizedString localizedName;

    public string QualityName => qualityName;
    public LocalizedString LocalizedName => localizedName;
}
