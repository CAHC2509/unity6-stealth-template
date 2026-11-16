using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[Serializable]
public class DevToolsStateSectionData
{
    [SerializeField] private string name;
    [SerializeField] private List<DevToolsStateEntryData> entries = new();

    private int usedEntries;

    public string Name => name;
    public int EntryCount => usedEntries;

    public DevToolsStateEntryData GetEntry(int index) => entries[index];

    public void Begin(string sectionName)
    {
        name = sectionName;
        usedEntries = 0;
    }

    public void Add(string key, string value)
    {
        if (usedEntries == entries.Count)
            entries.Add(new DevToolsStateEntryData());

        entries[usedEntries].Set(key, value);
        usedEntries++;
    }

    public void Add(string key, int value) => Add(key, value.ToString(CultureInfo.InvariantCulture));

    public void Add(string key, bool value) => Add(key, value ? Constants.DEVTOOLS_TRUE_TEXT : Constants.DEVTOOLS_FALSE_TEXT);

    public void Add(string key, float value) => Add(key, value.ToString(Constants.DEVTOOLS_DECIMAL_FORMAT, CultureInfo.InvariantCulture));

    public void Add(string key, Vector3 value)
    {
        Add(key, string.Format(CultureInfo.InvariantCulture, Constants.DEVTOOLS_VECTOR_FORMAT, value.x, value.y, value.z));
    }

    public void TrimUnusedEntries()
    {
        if (entries.Count > usedEntries)
            entries.RemoveRange(usedEntries, entries.Count - usedEntries);
    }
}
