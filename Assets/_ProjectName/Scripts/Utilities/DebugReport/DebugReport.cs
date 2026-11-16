using System;
using UnityEngine;

[Serializable]
public class DebugReport
{
    [TextArea(12, 40)]
    [SerializeField] private string reportBody;
    [SerializeField] private int lineCount;

    public string ReportBody => reportBody;

    public void Clear()
    {
        reportBody = string.Empty;
        lineCount = 0;
    }

    public void AppendLine(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        lineCount++;

        if (string.IsNullOrEmpty(reportBody))
            reportBody = $"{lineCount:000}: {message}";
        else
            reportBody += $"\n{lineCount:000}: {message}";
    }

    public void AppendSection(string title)
    {
        AppendLine($"--- {title} ---");
    }
}