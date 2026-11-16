using System;
using System.Collections.Generic;
using UnityEngine;

public class SessionLogController : ControllerBase
{
    public event Action CountersChanged;

    [SerializeField, Min(1)] private int maxEntries = Constants.DEVTOOLS_MAX_LOG_ENTRIES;

    private readonly List<DevToolsLogEntryData> entries = new();

    private string sessionStart;
    private int droppedEntries;
    private int errorCount;
    private int warningCount;
    private int messageCount;

    public int ErrorCount => errorCount;
    public int WarningCount => warningCount;
    public int MessageCount => messageCount;
    public IReadOnlyList<DevToolsLogEntryData> Entries => entries;

    public override void Initialize(params object[] parameters)
    {
        sessionStart = DateTime.Now.ToString(Constants.DEVTOOLS_ISO_TIMESTAMP_FORMAT);
        entries.Capacity = maxEntries;

        base.Initialize(parameters);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        Application.logMessageReceived += OnLogMessageReceived;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        Application.logMessageReceived -= OnLogMessageReceived;
    }

    public bool TryExport(out string path)
    {
        path = FileWriteUtility.BuildTimestampedPath(Application.persistentDataPath, Constants.DEVTOOLS_LOGS_FOLDER, Constants.DEVTOOLS_LOG_FILE_PREFIX, Constants.DEVTOOLS_TEXT_EXTENSION, DateTime.Now);

        string header = DevToolsReportUtility.BuildSessionHeader(sessionStart, DateTime.Now.ToString(Constants.DEVTOOLS_ISO_TIMESTAMP_FORMAT));
        string content = DevToolsReportUtility.BuildSessionLog(entries, header, droppedEntries);

        try
        {
            FileWriteUtility.WriteAtomic(path, content);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"{nameof(SessionLogController)}: could not export the session log to '{path}'. {exception.Message}", this);
            path = string.Empty;
            return false;
        }
    }

    private void OnLogMessageReceived(string message, string stackTrace, LogType type)
    {
        if (entries.Count >= maxEntries)
        {
            entries.RemoveAt(0);
            droppedEntries++;
        }

        entries.Add(new DevToolsLogEntryData(Time.realtimeSinceStartup, Time.frameCount, type, message, stackTrace));
        CountEntry(type);

        CountersChanged?.Invoke();
    }

    private void CountEntry(LogType type)
    {
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert:
                errorCount++;
                break;

            case LogType.Warning:
                warningCount++;
                break;

            default:
                messageCount++;
                break;
        }
    }
}
