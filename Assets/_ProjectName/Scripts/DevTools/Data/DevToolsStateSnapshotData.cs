using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class DevToolsStateSnapshotData
{
    [SerializeField] private string capturedAt;
    [SerializeField] private int frame;
    [SerializeField] private float realtimeSinceStartup;
    [SerializeField] private List<DevToolsStateSectionData> sections = new();

    private int usedSections;

    public int SectionCount => usedSections;
    public string CapturedAt => capturedAt;
    public int Frame => frame;

    public DevToolsStateSectionData GetSection(int index) => sections[index];

    public void Begin(DateTime timestamp, int frameCount, float realtime)
    {
        capturedAt = timestamp.ToString(Constants.DEVTOOLS_ISO_TIMESTAMP_FORMAT);
        frame = frameCount;
        realtimeSinceStartup = realtime;
        usedSections = 0;
    }

    public DevToolsStateSectionData BeginSection(string sectionName)
    {
        if (usedSections == sections.Count)
            sections.Add(new DevToolsStateSectionData());

        DevToolsStateSectionData section = sections[usedSections];
        section.Begin(sectionName);
        usedSections++;

        return section;
    }

    public DevToolsStateSectionData FindSection(string sectionName)
    {
        for (int i = 0; i < usedSections; i++)
        {
            if (sections[i].Name == sectionName)
                return sections[i];
        }

        return null;
    }

    public void WriteText(StringBuilder builder)
    {
        builder.Clear();

        for (int i = 0; i < usedSections; i++)
        {
            DevToolsStateSectionData section = sections[i];

            if (i > 0)
                builder.AppendLine();

            builder.Append(Constants.DEVTOOLS_SECTION_PREFIX).Append(section.Name).AppendLine(Constants.DEVTOOLS_SECTION_SUFFIX);

            for (int j = 0; j < section.EntryCount; j++)
            {
                DevToolsStateEntryData entry = section.GetEntry(j);
                builder.Append(entry.Key).Append(Constants.DEVTOOLS_KEY_VALUE_SEPARATOR).AppendLine(entry.Value);
            }
        }
    }

    public string ToJson()
    {
        if (sections.Count > usedSections)
            sections.RemoveRange(usedSections, sections.Count - usedSections);

        for (int i = 0; i < usedSections; i++)
            sections[i].TrimUnusedEntries();

        return JsonUtility.ToJson(this, true);
    }
}
