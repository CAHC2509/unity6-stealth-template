using System;
using UnityEngine;

[Serializable]
public class DeveloperSettingsData
{
    [SerializeField] private bool devToolsEnabled;

    public bool DevToolsEnabled { get => devToolsEnabled; set => devToolsEnabled = value; }
}
