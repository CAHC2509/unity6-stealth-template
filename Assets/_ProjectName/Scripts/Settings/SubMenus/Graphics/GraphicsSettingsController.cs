using System.Collections.Generic;
using UnityEngine;

public class GraphicsSettingsController : SettingsSubMenuControllerBase<GraphicsSettingsView>
{
    [SerializeField] private GraphicsSettingsData graphicsSettingsData;
    [SerializeField, Tooltip("Only list 16:9 resolutions. The native resolution is always listed.")] private bool filterByTargetAspectRatio = true;

    private readonly List<Resolution> availableResolutions = new();
    private readonly List<string> resolutionOptions = new();
    private int defaultResolutionIndex;
    private int defaultQualityIndex;

    public override void Initialize(params object[] parameters)
    {
        base.Initialize(parameters);

        SetupResolutions();
        SetupQualitySettings();
        View.SetOptionalSettingsAvailable(PlatformCapabilityUtility.SupportsResolutionSelection, PlatformCapabilityUtility.SupportsFullscreenToggle, PlatformCapabilityUtility.SupportsVSyncControl);

        LoadSettings();
        ApplySettings();

        RefreshView();
    }

    public override void SaveSettings()
    {
        SaveSystem.Save(Constants.GRAPHICS_SETTINGS_SAVE_KEY, graphicsSettingsData);
    }

    public override void LoadSettings()
    {
        if (!SaveSystem.TryLoad(Constants.GRAPHICS_SETTINGS_SAVE_KEY, out graphicsSettingsData))
            graphicsSettingsData = CreateDefaultData();
    }

    public override void RestoreDefaultSettings()
    {
        graphicsSettingsData = CreateDefaultData();
        RefreshView();

        ApplySettings();
        SaveSettings();
    }

    public void UpdateResolution(int index)
    {
        if (index < 0 || index >= availableResolutions.Count) return;

        Resolution resolution = availableResolutions[index];
        graphicsSettingsData.SetResolution(resolution.width, resolution.height);

        ApplyResolution();
    }

    public void UpdateQuality(int index)
    {
        graphicsSettingsData.QualityIndex = index;

        ApplyQuality();
    }

    public void UpdateFullscreen(bool isFullscreen)
    {
        graphicsSettingsData.IsFullscreen = isFullscreen;

        ApplyFullscreen();
    }

    public void UpdateVSync(bool isVSyncEnabled)
    {
        graphicsSettingsData.IsVSyncEnabled = isVSyncEnabled;

        ApplyVSync();
    }

    private void SetupResolutions()
    {
        Resolution nativeResolution = Screen.currentResolution;

        availableResolutions.Clear();
        resolutionOptions.Clear();

        foreach (Resolution resolution in Screen.resolutions)
        {
            if (!IsResolutionAllowed(resolution, nativeResolution)) continue;
            if (FindResolutionIndex(resolution.width, resolution.height) >= 0) continue;

            availableResolutions.Add(resolution);
        }

        if (FindResolutionIndex(nativeResolution.width, nativeResolution.height) < 0)
            availableResolutions.Add(nativeResolution);

        availableResolutions.Sort((a, b) => (b.width * b.height).CompareTo(a.width * a.height));

        foreach (Resolution resolution in availableResolutions)
            resolutionOptions.Add($"{resolution.width} x {resolution.height}");

        defaultResolutionIndex = FindResolutionIndex(nativeResolution.width, nativeResolution.height);

        View.SetResolutionOptions(resolutionOptions);
    }

    private bool IsResolutionAllowed(Resolution resolution, Resolution nativeResolution)
    {
        if (!filterByTargetAspectRatio) return true;
        if (resolution.width == nativeResolution.width && resolution.height == nativeResolution.height) return true;

        float aspectRatio = (float)resolution.width / resolution.height;
        return Mathf.Abs(aspectRatio - Constants.GRAPHICS_TARGET_ASPECT_RATIO) <= Constants.GRAPHICS_ASPECT_RATIO_TOLERANCE;
    }

    private int FindResolutionIndex(int width, int height)
    {
        for (int i = 0; i < availableResolutions.Count; i++)
        {
            if (availableResolutions[i].width == width && availableResolutions[i].height == height)
                return i;
        }

        return -1;
    }

    private int GetSelectedResolutionIndex()
    {
        if (!graphicsSettingsData.HasResolution) return defaultResolutionIndex;

        int index = FindResolutionIndex(graphicsSettingsData.ResolutionWidth, graphicsSettingsData.ResolutionHeight);
        return index >= 0 ? index : defaultResolutionIndex;
    }

    private void SetupQualitySettings()
    {
        defaultQualityIndex = QualitySettings.GetQualityLevel();
        View.SetQualityOptions(QualitySettings.names);
    }

    private GraphicsSettingsData CreateDefaultData()
    {
        GraphicsSettingsData defaultData = new GraphicsSettingsData();
        Resolution defaultResolution = availableResolutions[defaultResolutionIndex];

        defaultData.SetResolution(defaultResolution.width, defaultResolution.height);
        defaultData.QualityIndex = defaultQualityIndex;
        return defaultData;
    }

    private void RefreshView()
    {
        View.SetData(graphicsSettingsData, GetSelectedResolutionIndex());
    }

    private void ApplySettings()
    {
        ApplyResolution();
        ApplyQuality();
        ApplyFullscreen();
    }

    private void ApplyQuality()
    {
        if (graphicsSettingsData.QualityIndex < 0 || graphicsSettingsData.QualityIndex >= QualitySettings.names.Length)
            graphicsSettingsData.QualityIndex = defaultQualityIndex;

        QualitySettings.SetQualityLevel(graphicsSettingsData.QualityIndex);
        ApplyVSync();
    }

    private void ApplyVSync()
    {
        if (!PlatformCapabilityUtility.SupportsVSyncControl) return;

        QualitySettings.vSyncCount = graphicsSettingsData.IsVSyncEnabled
            ? Constants.GRAPHICS_VSYNC_ENABLED_COUNT
            : Constants.GRAPHICS_VSYNC_DISABLED_COUNT;
    }

    private void ApplyFullscreen()
    {
        if (!PlatformCapabilityUtility.SupportsFullscreenToggle) return;

        Screen.fullScreen = graphicsSettingsData.IsFullscreen;
    }

    private void ApplyResolution()
    {
        if (!PlatformCapabilityUtility.SupportsResolutionSelection) return;

        Resolution resolution = availableResolutions[GetSelectedResolutionIndex()];
        bool isFullscreen = PlatformCapabilityUtility.SupportsFullscreenToggle ? graphicsSettingsData.IsFullscreen : Screen.fullScreen;

        Screen.SetResolution(resolution.width, resolution.height, isFullscreen);
    }
}
