using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public static class RebindOperationUtility
{
    private static readonly string[] alwaysExcludedControls =
    {
        "<Mouse>/position",
        "<Mouse>/delta",
        "<Mouse>/press",
        "<Pointer>/position",
        "<Pointer>/delta",
        "<Pointer>/press",
        "<Keyboard>/escape",
        "<Gamepad>/select",
        "<Gamepad>/start"
    };

    private static readonly string[] keyboardMouseExcludedDevices = { "<Gamepad>", "<Joystick>", "<Touchscreen>", "<Pen>" };
    private static readonly string[] gamepadExcludedDevices = { "<Keyboard>", "<Mouse>", "<Touchscreen>", "<Pen>", "<Joystick>" };

    public static InputActionRebindingExtensions.RebindingOperation CreateOperation(InputAction action, int bindingIndex, DeviceType deviceType)
    {
        InputActionRebindingExtensions.RebindingOperation operation = action.PerformInteractiveRebinding(bindingIndex)
            .WithActionEventNotificationsBeingSuppressed()
            .WithCancelingThrough(Constants.INPUT_REBIND_CANCEL_CONTROL);

        ExcludeControls(operation, alwaysExcludedControls);

        switch (deviceType)
        {
            case DeviceType.KeyboardMouse:
                ExcludeControls(operation, keyboardMouseExcludedDevices);
                if (action.bindings[bindingIndex].isPartOfComposite)
                    operation.WithExpectedControlType(Constants.INPUT_REBIND_COMPOSITE_PART_CONTROL_TYPE);
                break;

            case DeviceType.Gamepad:
                ExcludeControls(operation, gamepadExcludedDevices);
                break;
        }

        return operation;
    }

    public static bool IsControlUsedInMap(InputControl control, InputAction currentAction, ICollection<int> excludedIndices)
    {
        if (control == null || currentAction?.actionMap == null) return false;

        foreach (InputAction action in currentAction.actionMap.actions)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (action == currentAction && excludedIndices.Contains(i))
                    continue;

                string effectivePath = action.bindings[i].effectivePath;
                if (string.IsNullOrEmpty(effectivePath))
                    continue;

                if (PathMatchesControl(effectivePath, control))
                    return true;
            }
        }

        return false;
    }

    public static bool PathMatchesControl(string bindingPath, InputControl control)
    {
        if (string.IsNullOrEmpty(bindingPath) || control == null) return false;

        if (InputControlPath.Matches(bindingPath, control))
            return true;

        if (string.Equals(bindingPath, control.path, StringComparison.OrdinalIgnoreCase))
            return true;

        return MatchesDeviceSpecificPathOnSameLayout(bindingPath, control);
    }

    private static bool MatchesDeviceSpecificPathOnSameLayout(string bindingPath, InputControl control)
    {
        if (bindingPath[0] != InputControlPath.Separator) return false;

        int controlSeparatorIndex = bindingPath.IndexOf(InputControlPath.Separator, 1);
        if (controlSeparatorIndex < 0) return false;

        string deviceName = bindingPath.Substring(1, controlSeparatorIndex - 1);
        string relativeBindingPath = bindingPath.Substring(controlSeparatorIndex);
        string relativeControlPath = control.path.Substring(control.device.path.Length);

        if (!string.Equals(relativeBindingPath, relativeControlPath, StringComparison.OrdinalIgnoreCase))
            return false;

        InputDevice boundDevice = InputSystem.GetDevice(deviceName);
        string boundLayout = boundDevice != null ? boundDevice.layout : deviceName;

        return string.Equals(boundLayout, control.device.layout, StringComparison.OrdinalIgnoreCase);
    }

    public static void CollectCompositeParts(InputAction action, int compositeRootIndex, List<CompositePartBinding> parts)
    {
        parts.Clear();

        for (int i = compositeRootIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++)
        {
            string partName = action.bindings[i].name.ToLowerInvariant();
            CompositePartBinding part = FindPart(parts, partName);

            if (part == null)
            {
                part = new CompositePartBinding(partName);
                parts.Add(part);
            }

            part.BindingIndices.Add(i);
        }
    }

    private static CompositePartBinding FindPart(List<CompositePartBinding> parts, string partName)
    {
        foreach (CompositePartBinding part in parts)
        {
            if (part.PartName == partName)
                return part;
        }

        return null;
    }

    private static void ExcludeControls(InputActionRebindingExtensions.RebindingOperation operation, string[] controlPaths)
    {
        foreach (string controlPath in controlPaths)
            operation.WithControlsExcluding(controlPath);
    }
}

public class CompositePartBinding
{
    public string PartName { get; }
    public List<int> BindingIndices { get; } = new List<int>();

    public CompositePartBinding(string partName)
    {
        PartName = partName;
    }
}
