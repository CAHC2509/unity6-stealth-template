using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputDisplayController : ControllerBase
{
    private readonly List<ButtonControl> buttons = new();
    private readonly List<StickControl> sticks = new();

    private bool isDirty = true;

    protected override void AddListeners()
    {
        base.AddListeners();

        InputSystem.onDeviceChange += OnDeviceChange;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    public void WriteActiveControls(StringBuilder builder)
    {
        if (isDirty)
            CacheControls();

        int written = 0;

        for (int i = 0; i < buttons.Count && written < Constants.DEVTOOLS_MAX_DISPLAYED_CONTROLS; i++)
        {
            ButtonControl button = buttons[i];
            if (!button.isPressed) continue;

            AppendSeparator(builder, written);
            builder.Append(GetLabel(button));
            written++;
        }

        for (int i = 0; i < sticks.Count && written < Constants.DEVTOOLS_MAX_DISPLAYED_CONTROLS; i++)
        {
            StickControl stick = sticks[i];
            Vector2 value = stick.ReadValue();

            if (value.sqrMagnitude < Constants.DEVTOOLS_STICK_DISPLAY_DEADZONE * Constants.DEVTOOLS_STICK_DISPLAY_DEADZONE) continue;

            AppendSeparator(builder, written);
            builder.Append(GetLabel(stick)).Append('(')
                .Append(value.x.ToString(Constants.DEVTOOLS_DECIMAL_FORMAT, CultureInfo.InvariantCulture)).Append(", ")
                .Append(value.y.ToString(Constants.DEVTOOLS_DECIMAL_FORMAT, CultureInfo.InvariantCulture)).Append(')');
            written++;
        }
    }

    private void CacheControls()
    {
        isDirty = false;
        buttons.Clear();
        sticks.Clear();

        foreach (InputDevice device in InputSystem.devices)
        {
            if (!device.added || !IsDisplayedDevice(device)) continue;

            foreach (InputControl control in device.allControls)
            {
                if (control.synthetic || control.noisy) continue;

                if (control is StickControl stick)
                {
                    sticks.Add(stick);
                    continue;
                }

                if (control is ButtonControl button && control.parent is InputDevice)
                    buttons.Add(button);
            }
        }
    }

    private static bool IsDisplayedDevice(InputDevice device)
    {
        return device is Keyboard || device is Mouse || device is Gamepad;
    }

    private static string GetLabel(InputControl control)
    {
        return string.IsNullOrEmpty(control.shortDisplayName) ? control.displayName : control.shortDisplayName;
    }

    private static void AppendSeparator(StringBuilder builder, int written)
    {
        if (written > 0)
            builder.Append(Constants.DEVTOOLS_CONTROLS_SEPARATOR);
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        isDirty = true;
    }
}
