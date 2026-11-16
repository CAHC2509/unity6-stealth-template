using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Globals
{
    public static PlayerInput GlobalPlayerInput { get; set; }
    public static DeviceType GlobalDeviceType { get; set; }
    public static List<PlayerInput> ActivePlayers { get; set; }

    public static SensitivitySettingsData SensitivitySettings { get; set; }
}
