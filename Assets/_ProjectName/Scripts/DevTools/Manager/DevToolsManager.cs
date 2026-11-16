using System;
using System.Collections.Generic;
using UnityEngine;

public class DevToolsManager : ManagerBase
{
    public static Action<bool> SetDevToolsEnabledRequest;
    public static Action<IDevToolsProvider> RegisterProviderRequest;
    public static Action<IDevToolsProvider> UnregisterProviderRequest;

    public event Action EnabledChanged;
    public event Action PreferencesChanged;
    public event Action ProvidersChanged;

    [Header("Logic")]
    [SerializeField] private PerformanceMonitorController performanceMonitor;
    [SerializeField] private TimeControlController timeControl;
    [SerializeField] private InputRecorderController inputRecorder;
    [SerializeField] private InputDisplayController inputDisplay;
    [SerializeField] private ScreenCaptureController screenCapture;
    [SerializeField] private SessionLogController sessionLog;
    [SerializeField] private DevToolsStateController stateController;
    [SerializeField] private DevToolsConsoleController consoleController;

    [Header("UI")]
    [SerializeField] private DevToolsPanelController panelController;
    [SerializeField] private DevToolsOverlayController overlayController;

    private readonly List<IDevToolsProvider> providers = new();

    private DevToolsPreferencesData preferences;
    private InputRequest toggleRequest;
    private bool isInitialized;
    private bool isEnabled;
    private bool isToggleListening;

    public bool IsEnabled => isEnabled;
    public bool IsPanelVisible => isInitialized && panelController.View.IsVisible;
    public DevToolsPreferencesData Preferences => preferences;
    public IReadOnlyList<IDevToolsProvider> Providers => providers;

    public PerformanceMonitorController PerformanceMonitor => performanceMonitor;
    public TimeControlController TimeControl => timeControl;
    public InputRecorderController InputRecorder => inputRecorder;
    public InputDisplayController InputDisplay => inputDisplay;
    public ScreenCaptureController ScreenCapture => screenCapture;
    public SessionLogController SessionLog => sessionLog;
    public DevToolsStateController State => stateController;
    public DevToolsConsoleController Console => consoleController;

    public override void Initialize(params object[] parameters)
    {
        if (!PlatformCapabilityUtility.SupportsDevTools)
        {
            gameObject.SetActive(false);
            return;
        }

        GameManager gameManager = parameters.Length > 0 ? parameters[0] as GameManager : null;

        bool hasDependencies = RequireDependency(gameManager, nameof(gameManager))
            & RequireDependency(performanceMonitor, nameof(performanceMonitor))
            & RequireDependency(timeControl, nameof(timeControl))
            & RequireDependency(inputRecorder, nameof(inputRecorder))
            & RequireDependency(inputDisplay, nameof(inputDisplay))
            & RequireDependency(screenCapture, nameof(screenCapture))
            & RequireDependency(sessionLog, nameof(sessionLog))
            & RequireDependency(stateController, nameof(stateController))
            & RequireDependency(consoleController, nameof(consoleController))
            & RequireDependency(panelController, nameof(panelController))
            & RequireDependency(overlayController, nameof(overlayController));

        if (!hasDependencies) return;

        preferences = SaveSystem.Load<DevToolsPreferencesData>(Constants.DEVTOOLS_PREFERENCES_SAVE_KEY);
        toggleRequest = new InputRequest(Globals.GlobalPlayerInput, InputActionMapId.Debug, InputActionId.ToggleDevTools, InputListenPhase.Started, OnToggleInput);

        base.Initialize(parameters);

        sessionLog.Initialize();
        performanceMonitor.Initialize();
        timeControl.Initialize();
        inputRecorder.Initialize(new Func<bool>(CanRecordInput));
        inputDisplay.Initialize();
        screenCapture.Initialize();
        stateController.Initialize(gameManager, timeControl, providers);
        consoleController.Initialize(this);
        panelController.Initialize(this);
        overlayController.Initialize(this);

        isInitialized = true;
    }

    public override void Conclude()
    {
        if (!isInitialized) return;

        SetEnabled(false);

        overlayController.Conclude();
        panelController.Conclude();
        consoleController.Conclude();
        stateController.Conclude();
        screenCapture.Conclude();
        inputDisplay.Conclude();
        inputRecorder.Conclude();
        timeControl.Conclude();
        performanceMonitor.Conclude();
        sessionLog.Conclude();

        base.Conclude();

        isInitialized = false;
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        SetDevToolsEnabledRequest += SetEnabled;
        RegisterProviderRequest += OnRegisterProviderRequest;
        UnregisterProviderRequest += OnUnregisterProviderRequest;
        screenCapture.CaptureStarted += OnCaptureStarted;
        screenCapture.CaptureCompleted += OnCaptureCompleted;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        SetDevToolsEnabledRequest -= SetEnabled;
        RegisterProviderRequest -= OnRegisterProviderRequest;
        UnregisterProviderRequest -= OnUnregisterProviderRequest;
        screenCapture.CaptureStarted -= OnCaptureStarted;
        screenCapture.CaptureCompleted -= OnCaptureCompleted;
    }

    public void SetEnabled(bool enabled)
    {
        if (!isInitialized || isEnabled == enabled) return;

        isEnabled = enabled;

        if (isEnabled)
            Activate();
        else
            Deactivate();

        EnabledChanged?.Invoke();
    }

    public void TogglePanel()
    {
        if (!isEnabled) return;

        if (IsPanelVisible)
            HidePanel();
        else
            ShowPanel();
    }

    public void ShowPanel()
    {
        if (!isEnabled || IsPanelVisible) return;

        panelController.ShowPanel();
        NotifyPanelVisibility(true);
    }

    public void HidePanel()
    {
        if (!IsPanelVisible) return;

        panelController.HidePanel();
        NotifyPanelVisibility(false);
    }

    public void SetOverlayVisible(bool visible)
    {
        preferences.OverlayVisible = visible;
        SavePreferences();

        overlayController.SetVisible(isEnabled && visible);
    }

    public void SetInputDisplayVisible(bool visible)
    {
        preferences.InputDisplayVisible = visible;
        SavePreferences();
    }

    public void SetDebugConsoleEnabled(bool enabled)
    {
        preferences.DebugConsoleEnabled = enabled;
        SavePreferences();

        consoleController.SetDebugConsoleActive(isEnabled && enabled);
    }

    public bool StartInputReplay()
    {
        if (!inputRecorder.HasRecording) return false;

        HidePanel();
        return inputRecorder.StartReplay();
    }

    public void OpenFolder(string folderPath)
    {
        if (!PlatformCapabilityUtility.SupportsFileBrowsing || string.IsNullOrEmpty(folderPath)) return;

        System.IO.Directory.CreateDirectory(folderPath);
        Application.OpenURL(Constants.DEVTOOLS_FILE_URL_PREFIX + folderPath.Replace('\\', '/'));
    }

    private void Activate()
    {
        performanceMonitor.SetSampling(true);
        overlayController.SetVisible(preferences.OverlayVisible);
        consoleController.SetDebugConsoleActive(preferences.DebugConsoleEnabled);
        consoleController.RegisterCommands();

        StartListeningToggle();
    }

    private void Deactivate()
    {
        StopListeningToggle();

        HidePanel();
        overlayController.SetVisible(false);
        consoleController.UnregisterCommands();
        consoleController.SetDebugConsoleActive(false);
        inputRecorder.StopReplay();
        inputRecorder.StopRecording();
        timeControl.ResetTime();
        performanceMonitor.SetSampling(false);
    }

    private void StartListeningToggle()
    {
        if (isToggleListening) return;

        InputManager.AddInputListener?.Invoke(toggleRequest);
        isToggleListening = true;
    }

    private void StopListeningToggle()
    {
        if (!isToggleListening) return;

        InputManager.RemoveInputListener?.Invoke(toggleRequest.SubscriptionId);
        isToggleListening = false;
    }

    private void SavePreferences()
    {
        SaveSystem.Save(Constants.DEVTOOLS_PREFERENCES_SAVE_KEY, preferences);
        PreferencesChanged?.Invoke();
    }

    private bool CanRecordInput() => !IsPanelVisible;

    private void NotifyPanelVisibility(bool visible)
    {
        for (int i = 0; i < providers.Count; i++)
            providers[i].OnDevToolsPanelVisibilityChanged(visible);
    }

    private void OnToggleInput(InputData data)
    {
        if (inputRecorder.IsReplaying) return;

        TogglePanel();
    }

    private void OnRegisterProviderRequest(IDevToolsProvider provider)
    {
        if (provider == null || providers.Contains(provider)) return;

        providers.Add(provider);

        if (IsPanelVisible)
            provider.OnDevToolsPanelVisibilityChanged(true);

        ProvidersChanged?.Invoke();
    }

    private void OnUnregisterProviderRequest(IDevToolsProvider provider)
    {
        if (!providers.Remove(provider)) return;

        provider.OnDevToolsPanelVisibilityChanged(false);
        ProvidersChanged?.Invoke();
    }

    private void OnCaptureStarted()
    {
        panelController.SetCaptureHidden(true);
        overlayController.SetCaptureHidden(true);
    }

    private void OnCaptureCompleted(string path)
    {
        panelController.SetCaptureHidden(false);
        overlayController.SetCaptureHidden(false);
    }
}
