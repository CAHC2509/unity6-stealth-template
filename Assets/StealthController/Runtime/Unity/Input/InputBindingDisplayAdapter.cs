using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace StealthController.Unity
{
    public class InputBindingDisplayAdapter
    {
        private static readonly Dictionary<string, string> ShortControlLabels = new Dictionary<string, string>
        {
            { StealthUnityConstants.INPUT_CONTROL_LEFT_SHIFT, StealthUnityConstants.INPUT_KEY_LABEL_SHIFT },
            { StealthUnityConstants.INPUT_CONTROL_RIGHT_SHIFT, StealthUnityConstants.INPUT_KEY_LABEL_SHIFT },
            { StealthUnityConstants.INPUT_CONTROL_LEFT_CTRL, StealthUnityConstants.INPUT_KEY_LABEL_CTRL },
            { StealthUnityConstants.INPUT_CONTROL_RIGHT_CTRL, StealthUnityConstants.INPUT_KEY_LABEL_CTRL },
            { StealthUnityConstants.INPUT_CONTROL_LEFT_ALT, StealthUnityConstants.INPUT_KEY_LABEL_ALT },
            { StealthUnityConstants.INPUT_CONTROL_LEFT_STICK_PRESS, StealthUnityConstants.INPUT_KEY_LABEL_LEFT_STICK },
            { StealthUnityConstants.INPUT_CONTROL_RIGHT_STICK_PRESS, StealthUnityConstants.INPUT_KEY_LABEL_RIGHT_STICK },
            { StealthUnityConstants.INPUT_CONTROL_LEFT_TRIGGER, StealthUnityConstants.INPUT_KEY_LABEL_LEFT_TRIGGER },
            { StealthUnityConstants.INPUT_CONTROL_RIGHT_TRIGGER, StealthUnityConstants.INPUT_KEY_LABEL_RIGHT_TRIGGER }
        };

        private readonly InputActionsAdapter input;
        private StealthDeviceType deviceType;

        public event Action DeviceChanged;

        public StealthDeviceType DeviceType => deviceType;

        public InputBindingDisplayAdapter(InputActionsAdapter input)
        {
            this.input = input;
            deviceType = Keyboard.current == null && Gamepad.current != null ? StealthDeviceType.Gamepad : StealthDeviceType.KeyboardMouse;
        }

        public void AddListeners()
        {
            InputSystem.onActionChange += OnActionChange;
        }

        public void RemoveListeners()
        {
            InputSystem.onActionChange -= OnActionChange;
        }

        public string ShortLabelFor(StealthInputAction actionId)
        {
            InputAction action = input.Find(actionId);
            int bindingIndex = FindBindingIndex(action, deviceType);
            if (bindingIndex < 0) return string.Empty;

            string displayString = action.GetBindingDisplayString(bindingIndex, out string _, out string controlPath);
            return controlPath != null && ShortControlLabels.TryGetValue(controlPath, out string shortLabel) ? shortLabel : displayString;
        }

        private void OnActionChange(object changed, InputActionChange change)
        {
            if (change != InputActionChange.ActionPerformed) return;
            if (!(changed is InputAction action) || action.activeControl == null) return;

            StealthDeviceType usedDeviceType = action.activeControl.device is Gamepad ? StealthDeviceType.Gamepad : StealthDeviceType.KeyboardMouse;
            if (usedDeviceType == deviceType) return;

            deviceType = usedDeviceType;
            DeviceChanged?.Invoke();
        }

        private static int FindBindingIndex(InputAction action, StealthDeviceType deviceType)
        {
            if (action == null) return -1;

            int fallbackIndex = -1;

            for (int i = 0; i < action.bindings.Count; ++i)
            {
                InputBinding binding = action.bindings[i];
                if (binding.isComposite || binding.isPartOfComposite) continue;

                if (MatchesBinding(binding, deviceType)) return i;

                if (fallbackIndex < 0)
                    fallbackIndex = i;
            }

            return fallbackIndex;
        }

        private static bool MatchesBinding(InputBinding binding, StealthDeviceType deviceType)
        {
            if (deviceType == StealthDeviceType.KeyboardMouse)
                return HasGroup(binding, StealthUnityConstants.INPUT_GROUP_KEYBOARD) || HasGroup(binding, StealthUnityConstants.INPUT_GROUP_MOUSE);

            return HasGroup(binding, StealthUnityConstants.INPUT_GROUP_GAMEPAD)
                   || (!string.IsNullOrEmpty(binding.path) && binding.path.StartsWith(StealthUnityConstants.INPUT_GAMEPAD_PATH_PREFIX, StringComparison.OrdinalIgnoreCase));
        }

        private static bool HasGroup(InputBinding binding, string group)
        {
            return !string.IsNullOrEmpty(binding.groups) && binding.groups.Contains(group);
        }
    }
}
