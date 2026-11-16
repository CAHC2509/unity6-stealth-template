using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ControlRebindSession
{
    public event Action<string> PartStarted;
    public event Action InputAccepted;
    public event Action<RebindErrorType> ErrorDetected;
    public event Action<bool> Finished;
    public event Action<string> Logged;

    private readonly List<CompositePartBinding> compositeParts = new List<CompositePartBinding>();
    private readonly List<int> singleBindingIndices = new List<int>();

    private InputActionRebindingExtensions.RebindingOperation currentOperation;
    private InputAction action;
    private InputDevice candidateDevice;
    private DeviceType deviceType;
    private int nextPartIndex;
    private bool actionMapWasEnabled;
    private bool isAwaitingErrorCleanup;

    public bool IsActive => action != null;
    public bool IsShowingError => isAwaitingErrorCleanup;

    public void Start(InputAction targetAction, int bindingIndex, DeviceType expectedDeviceType)
    {
        if (IsActive || targetAction == null) return;

        action = targetAction;
        deviceType = expectedDeviceType;
        actionMapWasEnabled = action.enabled;

        InputSystem.onDeviceChange += OnDeviceChange;

        if (actionMapWasEnabled)
        {
            Log($"Disabling action map '{action.actionMap.name}' for rebind.");
            action.actionMap.Disable();
        }

        if (action.bindings[bindingIndex].isComposite)
        {
            RebindOperationUtility.CollectCompositeParts(action, bindingIndex, compositeParts);
            nextPartIndex = 0;
            StartNextCompositePart();
            return;
        }

        singleBindingIndices.Clear();
        singleBindingIndices.Add(bindingIndex);
        StartPart(string.Empty, singleBindingIndices, false);
    }

    public void Cancel()
    {
        if (isAwaitingErrorCleanup) return;

        currentOperation?.Cancel();
    }

    public void CompleteErrorCleanup()
    {
        if (!isAwaitingErrorCleanup) return;

        isAwaitingErrorCleanup = false;
        Finish(false);
    }

    private void StartNextCompositePart()
    {
        if (nextPartIndex >= compositeParts.Count)
        {
            Log($"Composite sequence finished for action '{action.name}'.");
            Finish(true);
            return;
        }

        CompositePartBinding part = compositeParts[nextPartIndex++];
        StartPart(part.PartName, part.BindingIndices, true);
    }

    private void StartPart(string partName, List<int> bindingIndices, bool isCompositePart)
    {
        candidateDevice = null;
        PartStarted?.Invoke(partName);

        currentOperation = RebindOperationUtility.CreateOperation(action, bindingIndices[0], deviceType)
            .OnPotentialMatch(operation => ValidatePotentialMatch(operation, bindingIndices))
            .OnCancel(_ => OnOperationCanceled())
            .OnComplete(operation => OnOperationCompleted(operation, bindingIndices, isCompositePart));

        currentOperation.Start();
        Log(isCompositePart ? $"Composite part rebind started for '{partName}'." : "Rebind operation started.");
    }

    private void ValidatePotentialMatch(InputActionRebindingExtensions.RebindingOperation operation, List<int> bindingIndices)
    {
        InputControl control = operation.selectedControl;
        candidateDevice = control?.device;
        Log($"Potential match: {control?.path ?? "null"} (device: {control?.device?.displayName})");

        if (control == null || !DeviceTypeUtility.Matches(control.device, deviceType))
        {
            RaiseError(RebindErrorType.InvalidDevice);
            return;
        }

        if (RebindOperationUtility.IsControlUsedInMap(control, action, bindingIndices))
        {
            RaiseError(RebindErrorType.KeyAlreadyUsed);
            return;
        }

        InputAccepted?.Invoke();
    }

    private void OnOperationCanceled()
    {
        if (isAwaitingErrorCleanup)
        {
            Log("Rebind canceled due to an error, waiting for the error feedback to finish.");
            return;
        }

        Log("Rebind canceled.");
        Finish(false);
    }

    private void OnOperationCompleted(InputActionRebindingExtensions.RebindingOperation operation, List<int> bindingIndices, bool isCompositePart)
    {
        Log($"Rebind completed: {operation.selectedControl?.path}");

        if (!isCompositePart)
        {
            Finish(true);
            return;
        }

        string generalizedPath = action.bindings[bindingIndices[0]].effectivePath;

        for (int i = 1; i < bindingIndices.Count; i++)
            action.ApplyBindingOverride(bindingIndices[i], generalizedPath);

        DisposeCurrentOperation();
        StartNextCompositePart();
    }

    private void RaiseError(RebindErrorType errorType)
    {
        if (isAwaitingErrorCleanup) return;

        Log($"Rebind rejected: {errorType}.");
        isAwaitingErrorCleanup = true;
        currentOperation?.Cancel();
        ErrorDetected?.Invoke(errorType);
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change != InputDeviceChange.Removed && change != InputDeviceChange.Disconnected) return;
        if (currentOperation == null || candidateDevice != device) return;

        Log($"Device '{device.displayName}' disconnected during rebind, cancelling.");
        currentOperation.Cancel();
    }

    private void Finish(bool hasChanges)
    {
        InputSystem.onDeviceChange -= OnDeviceChange;

        DisposeCurrentOperation();

        if (actionMapWasEnabled && action != null && !action.enabled)
            action.actionMap.Enable();

        action = null;
        candidateDevice = null;
        compositeParts.Clear();

        Finished?.Invoke(hasChanges);
    }

    private void DisposeCurrentOperation()
    {
        if (currentOperation == null) return;

        currentOperation.Dispose();
        currentOperation = null;
    }

    private void Log(string message)
    {
        Logged?.Invoke(message);
    }
}
