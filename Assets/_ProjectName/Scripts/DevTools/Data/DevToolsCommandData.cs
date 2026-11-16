using System;

public class DevToolsCommandData
{
    public string ConsoleName { get; }
    public string LabelKey { get; }
    public object[] LabelArguments { get; }
    public Action Execute { get; }
    public Func<bool> ReadToggle { get; }
    public Action<bool> WriteToggle { get; }

    public bool IsToggle => ReadToggle != null && WriteToggle != null;

    private DevToolsCommandData(string consoleName, string labelKey, Action execute, Func<bool> readToggle, Action<bool> writeToggle, object[] labelArguments)
    {
        ConsoleName = consoleName;
        LabelKey = labelKey;
        Execute = execute;
        ReadToggle = readToggle;
        WriteToggle = writeToggle;
        LabelArguments = labelArguments ?? Array.Empty<object>();
    }

    public static DevToolsCommandData CreateAction(string consoleName, string labelKey, Action execute, params object[] labelArguments)
    {
        return new DevToolsCommandData(consoleName, labelKey, execute, null, null, labelArguments);
    }

    public static DevToolsCommandData CreateToggle(string consoleName, string labelKey, Func<bool> readToggle, Action<bool> writeToggle, params object[] labelArguments)
    {
        return new DevToolsCommandData(consoleName, labelKey, null, readToggle, writeToggle, labelArguments);
    }

    public void Run()
    {
        if (IsToggle)
        {
            WriteToggle(!ReadToggle());
            return;
        }

        Execute?.Invoke();
    }
}
