using System.Collections.Generic;
using UnityEngine.InputSystem;

public static class InputBindingDisplayUtility
{
    private static readonly Dictionary<string, string> ShortControlLabels = new Dictionary<string, string>
    {
        { Constants.INPUT_CONTROL_LEFT_SHIFT, Constants.INPUT_KEY_LABEL_SHIFT },
        { Constants.INPUT_CONTROL_RIGHT_SHIFT, Constants.INPUT_KEY_LABEL_SHIFT },
        { Constants.INPUT_CONTROL_LEFT_CTRL, Constants.INPUT_KEY_LABEL_CTRL },
        { Constants.INPUT_CONTROL_RIGHT_CTRL, Constants.INPUT_KEY_LABEL_CTRL },
        { Constants.INPUT_CONTROL_LEFT_ALT, Constants.INPUT_KEY_LABEL_ALT },
        { Constants.INPUT_CONTROL_LEFT_STICK_PRESS, Constants.INPUT_KEY_LABEL_LEFT_STICK },
        { Constants.INPUT_CONTROL_RIGHT_STICK_PRESS, Constants.INPUT_KEY_LABEL_RIGHT_STICK },
        { Constants.INPUT_CONTROL_LEFT_TRIGGER, Constants.INPUT_KEY_LABEL_LEFT_TRIGGER },
        { Constants.INPUT_CONTROL_RIGHT_TRIGGER, Constants.INPUT_KEY_LABEL_RIGHT_TRIGGER }
    };

    public static int FindBindingIndex(InputAction action, DeviceType deviceType)
    {
        if (action == null) return -1;

        int fallbackIndex = -1;

        for (int i = 0; i < action.bindings.Count; ++i)
        {
            InputBinding binding = action.bindings[i];
            if (binding.isComposite || binding.isPartOfComposite) continue;

            if (DeviceTypeUtility.MatchesBinding(binding, deviceType)) return i;

            if (fallbackIndex < 0)
                fallbackIndex = i;
        }

        return fallbackIndex;
    }

    public static string GetShortDisplayString(InputAction action, DeviceType deviceType)
    {
        int bindingIndex = FindBindingIndex(action, deviceType);
        if (bindingIndex < 0) return string.Empty;

        string displayString = action.GetBindingDisplayString(bindingIndex, out string _, out string controlPath);
        return controlPath != null && ShortControlLabels.TryGetValue(controlPath, out string shortLabel) ? shortLabel : displayString;
    }

    public static bool TryGetChangedDeviceType(object changed, InputActionChange change, DeviceType currentDeviceType, out DeviceType usedDeviceType)
    {
        usedDeviceType = currentDeviceType;

        if (change != InputActionChange.ActionPerformed) return false;
        if (!(changed is InputAction action) || action.activeControl == null) return false;

        usedDeviceType = DeviceTypeUtility.FromDevice(action.activeControl.device);
        return usedDeviceType != currentDeviceType;
    }

    public static DeviceType CurrentDeviceType(PlayerInput playerInput)
    {
        if (playerInput == null || playerInput.devices.Count == 0) return Globals.GlobalDeviceType;

        return DeviceTypeUtility.FromDevice(playerInput.devices[0]);
    }
}
