using System;
using UnityEngine;

[Serializable]
public class DevToolsPreferencesData
{
    [SerializeField] private bool overlayVisible = true;
    [SerializeField] private bool inputDisplayVisible;
    [SerializeField] private bool debugConsoleEnabled;

    public bool OverlayVisible { get => overlayVisible; set => overlayVisible = value; }
    public bool InputDisplayVisible { get => inputDisplayVisible; set => inputDisplayVisible = value; }
    public bool DebugConsoleEnabled { get => debugConsoleEnabled; set => debugConsoleEnabled = value; }
}
