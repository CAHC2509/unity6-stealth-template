using System;
using UnityEngine;

[Serializable]
public class DevToolsStateEntryData
{
    [SerializeField] private string key;
    [SerializeField] private string value;

    public string Key => key;
    public string Value => value;

    public void Set(string entryKey, string entryValue)
    {
        key = entryKey;
        value = entryValue;
    }
}
