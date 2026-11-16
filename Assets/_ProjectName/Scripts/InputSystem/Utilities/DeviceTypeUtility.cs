using System;
using UnityEngine.InputSystem;

public static class DeviceTypeUtility
{
    public static DeviceType FromDevice(InputDevice device)
    {
        return device is Gamepad ? DeviceType.Gamepad : DeviceType.KeyboardMouse;
    }

    public static bool Matches(InputDevice device, DeviceType deviceType)
    {
        switch (deviceType)
        {
            case DeviceType.Any:
                return true;

            case DeviceType.KeyboardMouse:
                return device is Keyboard || device is Mouse;

            case DeviceType.Gamepad:
                return device is Gamepad;
        }

        return false;
    }

    public static bool MatchesBinding(InputBinding binding, DeviceType deviceType)
    {
        switch (deviceType)
        {
            case DeviceType.Any:
                return true;

            case DeviceType.KeyboardMouse:
                return HasGroup(binding, Constants.INPUT_GROUP_KEYBOARD) || HasGroup(binding, Constants.INPUT_GROUP_MOUSE);

            case DeviceType.Gamepad:
                return HasGroup(binding, Constants.INPUT_GROUP_GAMEPAD)
                       || (!string.IsNullOrEmpty(binding.path) && binding.path.StartsWith(Constants.INPUT_GAMEPAD_PATH_PREFIX, StringComparison.OrdinalIgnoreCase));
        }

        return false;
    }

    private static bool HasGroup(InputBinding binding, string group)
    {
        return !string.IsNullOrEmpty(binding.groups) && binding.groups.Contains(group);
    }
}
