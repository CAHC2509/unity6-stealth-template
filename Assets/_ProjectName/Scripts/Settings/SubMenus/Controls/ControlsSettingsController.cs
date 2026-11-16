using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MEC;

public class ControlsSettingsController : SettingsSubMenuControllerBase<ControlSettingsView>
{
    [SerializeField] private ControlsRebindOverlayView rebindOverlayView;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private RectTransform rebindControllersParent;

    [Header("Debug")]
    [SerializeField] private bool debugRebinds;
    [SerializeField] private DebugReport debugReport = new DebugReport();

    private readonly ControlRebindSession rebindSession = new ControlRebindSession();

    private ControlBindingController[] bindingControllers = Array.Empty<ControlBindingController>();
    private ControlBindingsData controlBindingsData;
    private ControlBindingController currentBindingController;
    private CoroutineHandle startRebindCoroutine;
    private CoroutineHandle initializeControllersCoroutine;
    private bool controllersInitialized;

    public InputActionAsset ActionAsset => PlayerOwner != null && PlayerOwner.actions != null ? PlayerOwner.actions : InputSystem.actions;

    private PlayerInput PlayerOwner => playerInput != null ? playerInput : Globals.GlobalPlayerInput;

    public override void Initialize(params object[] parameters)
    {
        RequireDependency(rebindOverlayView, nameof(rebindOverlayView));

        if (RequireDependency(rebindControllersParent, nameof(rebindControllersParent)))
            bindingControllers = rebindControllersParent.GetComponentsInChildren<ControlBindingController>(true);

        base.Initialize(parameters);

        rebindOverlayView.Initialize();

        ClearDebugReport();
        LoadSettings();
        ApplyBindingOverrides();

        if (HasPlayerActions())
            InitializeBindingControllers();
        else
            initializeControllersCoroutine = Timing.RunCoroutine(WaitForPlayerInputAndInitialize().CancelWith(gameObject));

        LogRebind("Controls settings initialized.");
    }

    public override void Conclude()
    {
        CancelCurrentRebind();
        KillCoroutine(initializeControllersCoroutine);
        ConcludeBindingControllers();

        rebindOverlayView.Conclude();

        base.Conclude();

        LogRebind("Controls settings concluded.");
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        rebindSession.PartStarted += OnRebindPartStarted;
        rebindSession.InputAccepted += OnRebindInputAccepted;
        rebindSession.ErrorDetected += OnRebindErrorDetected;
        rebindSession.Finished += OnRebindFinished;
        rebindSession.Logged += LogRebind;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        rebindSession.PartStarted -= OnRebindPartStarted;
        rebindSession.InputAccepted -= OnRebindInputAccepted;
        rebindSession.ErrorDetected -= OnRebindErrorDetected;
        rebindSession.Finished -= OnRebindFinished;
        rebindSession.Logged -= LogRebind;
    }

    public override void OpenSubMenu()
    {
        LogRebind("OpenSubMenu");

        if (!controllersInitialized && HasPlayerActions())
            InitializeBindingControllers();

        base.OpenSubMenu();
    }

    public override void CloseSubMenu()
    {
        LogRebind("CloseSubMenu");
        CancelCurrentRebind();

        base.CloseSubMenu();
    }

    public override void SaveSettings()
    {
        CaptureBindingOverrides();
        SaveSystem.Save(Constants.CONTROLS_SETTINGS_SAVE_KEY, controlBindingsData);
        LogRebind("SaveSettings completed.");
    }

    public override void LoadSettings()
    {
        if (!SaveSystem.TryLoad(Constants.CONTROLS_SETTINGS_SAVE_KEY, out controlBindingsData) || controlBindingsData == null)
            controlBindingsData = new ControlBindingsData();

        LogRebind($"LoadSettings -> json length: {controlBindingsData.BindingsJson?.Length ?? 0}");
    }

    public override void RestoreDefaultSettings()
    {
        LogRebind("RestoreDefaultSettings");
        CancelCurrentRebind();

        InputActionAsset asset = ActionAsset;
        if (asset == null) return;

        asset.RemoveAllBindingOverrides();
        controlBindingsData = new ControlBindingsData();
        RefreshBindingDisplays();
        SaveSettings();
    }

    public void RequestRebind(ControlBindingController bindingController)
    {
        if (bindingController == null) return;
        if (rebindSession.IsActive || rebindSession.IsShowingError || startRebindCoroutine.IsRunning) return;

        if (!bindingController.ResolveActionAndBinding(out InputAction action, out int bindingIndex))
        {
            LogRebind($"RequestRebind failed to resolve binding for '{bindingController.name}'.");
            return;
        }

        currentBindingController = bindingController;
        bindingController.SetInteractable(false);

        View.SetCurrentSelection(bindingController.RebindButton);

        rebindOverlayView.ShowView();
        rebindOverlayView.SetExpectedDevice(bindingController.RebindDeviceType);

        startRebindCoroutine = Timing.RunCoroutine(StartRebindDelayed(bindingController, action, bindingIndex).CancelWith(gameObject));
    }

    public void RequestReset(ControlBindingController bindingController)
    {
        CancelCurrentRebind();
        bindingController.ResetToDefault();
        RefreshBindingDisplays();
        SaveSettings();
    }

    public void CancelCurrentRebind()
    {
        if (rebindSession.IsShowingError) return;

        if (startRebindCoroutine.IsRunning)
        {
            Timing.KillCoroutines(startRebindCoroutine);
            OnRebindFinished(false);
            return;
        }

        rebindSession.Cancel();
    }

    public void AppendDebugReport(string message)
    {
        if (debugReport == null) debugReport = new DebugReport();
        debugReport.AppendLine(message);
    }

    public void ClearDebugReport() => debugReport?.Clear();

    private IEnumerator<float> WaitForPlayerInputAndInitialize()
    {
        yield return Timing.WaitForOneFrame;

        if (HasPlayerActions())
            InitializeBindingControllers();
    }

    private IEnumerator<float> StartRebindDelayed(ControlBindingController bindingController, InputAction action, int bindingIndex)
    {
        yield return Timing.WaitForOneFrame;

        rebindSession.Start(action, bindingIndex, bindingController.RebindDeviceType);
    }

    private void OnRebindPartStarted(string partName)
    {
        if (currentBindingController == null) return;

        rebindOverlayView.ShowWaitingForRebind(currentBindingController.ActionLocalizedString, partName);
        rebindOverlayView.SetExpectedDevice(currentBindingController.RebindDeviceType);
        rebindOverlayView.StartTimeout(rebindOverlayView.TimeoutDuration, HandleRebindTimeout);
    }

    private void OnRebindInputAccepted()
    {
        rebindOverlayView.ClearInfo();
    }

    private void OnRebindErrorDetected(RebindErrorType errorType)
    {
        rebindOverlayView.PauseTimeout();

        switch (errorType)
        {
            case RebindErrorType.InvalidDevice:
                rebindOverlayView.ShowInvalidDevice();
                break;

            case RebindErrorType.KeyAlreadyUsed:
                rebindOverlayView.ShowKeyAlreadyUsed();
                break;
        }

        rebindOverlayView.PlayErrorAnimation(rebindSession.CompleteErrorCleanup);
    }

    private void OnRebindFinished(bool hasChanges)
    {
        rebindOverlayView.StopTimeout();

        if (currentBindingController != null)
        {
            currentBindingController.SetInteractable(true);
            currentBindingController = null;
        }

        rebindOverlayView.HideView().OnComplete(View.SelectCurrentItem);

        if (hasChanges)
            SaveSettings();

        RefreshBindingDisplays();
    }

    private void HandleRebindTimeout()
    {
        LogRebind("Timeout reached, cancelling rebind.");
        CancelCurrentRebind();
    }

    private void KillCoroutine(CoroutineHandle handle)
    {
        if (handle.IsRunning)
            Timing.KillCoroutines(handle);
    }

    private bool HasPlayerActions()
    {
        return PlayerOwner != null && PlayerOwner.actions != null;
    }

    private void InitializeBindingControllers()
    {
        if (controllersInitialized) return;

        foreach (ControlBindingController bindingController in bindingControllers)
            bindingController.Initialize(this);

        RefreshBindingDisplays();
        controllersInitialized = true;
    }

    private void ConcludeBindingControllers()
    {
        if (!controllersInitialized) return;

        foreach (ControlBindingController bindingController in bindingControllers)
            bindingController.Conclude();

        controllersInitialized = false;
    }

    private void RefreshBindingDisplays()
    {
        foreach (ControlBindingController bindingController in bindingControllers)
            bindingController.UpdateBindingDisplay();
    }

    private void ApplyBindingOverrides()
    {
        InputActionAsset asset = ActionAsset;
        if (asset == null) return;

        asset.RemoveAllBindingOverrides();

        if (string.IsNullOrEmpty(controlBindingsData.BindingsJson))
        {
            LogRebind("No binding override JSON found.");
            return;
        }

        LogRebind("Applying binding overrides from JSON.");
        asset.LoadBindingOverridesFromJson(controlBindingsData.BindingsJson);
    }

    private void CaptureBindingOverrides()
    {
        InputActionAsset asset = ActionAsset;
        if (asset == null) return;

        controlBindingsData.BindingsJson = asset.SaveBindingOverridesAsJson();
        LogRebind($"Captured binding overrides. Json length: {controlBindingsData.BindingsJson?.Length ?? 0}");
    }

    private void LogRebind(string message)
    {
        AppendDebugReport($"[Controls Rebind] {message}");
#if UNITY_EDITOR
        if (debugRebinds) Debug.Log($"[Controls Rebind] {message}", this);
#endif
    }
}
