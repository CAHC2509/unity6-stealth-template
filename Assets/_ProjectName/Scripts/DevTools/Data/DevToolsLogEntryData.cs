using UnityEngine;

public readonly struct DevToolsLogEntryData
{
    public float Time { get; }
    public int Frame { get; }
    public LogType Type { get; }
    public string Message { get; }
    public string StackTrace { get; }

    public DevToolsLogEntryData(float time, int frame, LogType type, string message, string stackTrace)
    {
        Time = time;
        Frame = frame;
        Type = type;
        Message = message;
        StackTrace = stackTrace;
    }
}
