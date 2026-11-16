using System.Collections.Generic;
using System.Globalization;
using System.Text;
using IngameDebugConsole;
using UnityEngine;

public class DevToolsConsoleController : ControllerBase
{
    [SerializeField] private GameObject debugConsolePrefab;

    private readonly StringBuilder commandListBuilder = new();

    private DevToolsManager devToolsManager;
    private GameObject debugConsoleInstance;
    private bool areCommandsRegistered;

    public bool IsDebugConsoleActive => debugConsoleInstance != null && debugConsoleInstance.activeSelf;

    public override void Initialize(params object[] parameters)
    {
        devToolsManager = parameters[0] as DevToolsManager;

        bool hasDependencies = RequireDependency(devToolsManager, nameof(devToolsManager))
            & RequireDependency(debugConsolePrefab, nameof(debugConsolePrefab));

        if (!hasDependencies) return;

        base.Initialize(parameters);
    }

    public override void Conclude()
    {
        UnregisterCommands();

        base.Conclude();
    }

    public void SetDebugConsoleActive(bool active)
    {
        if (active && debugConsoleInstance == null)
            debugConsoleInstance = Instantiate(debugConsolePrefab);

        if (debugConsoleInstance != null)
            debugConsoleInstance.SetActive(active);
    }

    public void RegisterCommands()
    {
        if (areCommandsRegistered) return;

        DebugLogConsole.AddCommand(Constants.DEVTOOLS_COMMAND_TOGGLE, Constants.DEVTOOLS_COMMAND_TOGGLE_DESCRIPTION, TogglePanel);
        DebugLogConsole.AddCommand<float>(Constants.DEVTOOLS_COMMAND_TIME_SCALE, Constants.DEVTOOLS_COMMAND_TIME_SCALE_DESCRIPTION, SetTimeScale);
        DebugLogConsole.AddCommand(Constants.DEVTOOLS_COMMAND_PAUSE, Constants.DEVTOOLS_COMMAND_PAUSE_DESCRIPTION, TogglePause);
        DebugLogConsole.AddCommand(Constants.DEVTOOLS_COMMAND_STEP, Constants.DEVTOOLS_COMMAND_STEP_DESCRIPTION, StepFrame);
        DebugLogConsole.AddCommand(Constants.DEVTOOLS_COMMAND_STATE, Constants.DEVTOOLS_COMMAND_STATE_DESCRIPTION, LogState);
        DebugLogConsole.AddCommand(Constants.DEVTOOLS_COMMAND_SCREENSHOT, Constants.DEVTOOLS_COMMAND_SCREENSHOT_DESCRIPTION, CaptureScreenshot);
        DebugLogConsole.AddCommand(Constants.DEVTOOLS_COMMAND_EXPORT_LOGS, Constants.DEVTOOLS_COMMAND_EXPORT_LOGS_DESCRIPTION, ExportLogs);
        DebugLogConsole.AddCommand(Constants.DEVTOOLS_COMMAND_BENCHMARK, Constants.DEVTOOLS_COMMAND_BENCHMARK_DESCRIPTION, StartBenchmark);
        DebugLogConsole.AddCommand(Constants.DEVTOOLS_COMMAND_RECORD, Constants.DEVTOOLS_COMMAND_RECORD_DESCRIPTION, ToggleRecording);
        DebugLogConsole.AddCommand(Constants.DEVTOOLS_COMMAND_REPLAY, Constants.DEVTOOLS_COMMAND_REPLAY_DESCRIPTION, Replay);
        DebugLogConsole.AddCommand<string>(Constants.DEVTOOLS_COMMAND_LOAD_RECORDING, Constants.DEVTOOLS_COMMAND_LOAD_RECORDING_DESCRIPTION, LoadRecording);
        DebugLogConsole.AddCommand(Constants.DEVTOOLS_COMMAND_LIST, Constants.DEVTOOLS_COMMAND_LIST_DESCRIPTION, ListGameplayCommands);
        DebugLogConsole.AddCommand<int>(Constants.DEVTOOLS_COMMAND_RUN, Constants.DEVTOOLS_COMMAND_RUN_DESCRIPTION, RunGameplayCommand);

        areCommandsRegistered = true;
    }

    public void UnregisterCommands()
    {
        if (!areCommandsRegistered) return;

        DebugLogConsole.RemoveCommand(Constants.DEVTOOLS_COMMAND_TOGGLE);
        DebugLogConsole.RemoveCommand(Constants.DEVTOOLS_COMMAND_TIME_SCALE);
        DebugLogConsole.RemoveCommand(Constants.DEVTOOLS_COMMAND_PAUSE);
        DebugLogConsole.RemoveCommand(Constants.DEVTOOLS_COMMAND_STEP);
        DebugLogConsole.RemoveCommand(Constants.DEVTOOLS_COMMAND_STATE);
        DebugLogConsole.RemoveCommand(Constants.DEVTOOLS_COMMAND_SCREENSHOT);
        DebugLogConsole.RemoveCommand(Constants.DEVTOOLS_COMMAND_EXPORT_LOGS);
        DebugLogConsole.RemoveCommand(Constants.DEVTOOLS_COMMAND_BENCHMARK);
        DebugLogConsole.RemoveCommand(Constants.DEVTOOLS_COMMAND_RECORD);
        DebugLogConsole.RemoveCommand(Constants.DEVTOOLS_COMMAND_REPLAY);
        DebugLogConsole.RemoveCommand(Constants.DEVTOOLS_COMMAND_LOAD_RECORDING);
        DebugLogConsole.RemoveCommand(Constants.DEVTOOLS_COMMAND_LIST);
        DebugLogConsole.RemoveCommand(Constants.DEVTOOLS_COMMAND_RUN);

        areCommandsRegistered = false;
    }

    private void TogglePanel() => devToolsManager.TogglePanel();

    private void SetTimeScale(float value) => devToolsManager.TimeControl.SetTimeScale(value);

    private void TogglePause() => devToolsManager.TimeControl.TogglePause();

    private void StepFrame() => devToolsManager.TimeControl.StepFrame();

    private void LogState() => Debug.Log(devToolsManager.State.CaptureText());

    private void CaptureScreenshot() => devToolsManager.ScreenCapture.CaptureScreenshot(devToolsManager.State.CaptureJson);

    private void StartBenchmark() => devToolsManager.PerformanceMonitor.StartBenchmark();

    private void ToggleRecording() => devToolsManager.InputRecorder.ToggleRecording();

    private void Replay() => devToolsManager.StartInputReplay();

    private void ExportLogs()
    {
        if (devToolsManager.SessionLog.TryExport(out string path))
            Debug.Log($"DevTools: session log exported to '{path}'.");
    }

    private void LoadRecording(string path)
    {
        if (!devToolsManager.InputRecorder.LoadRecording(path))
            Debug.LogWarning($"DevTools: could not load the input recording '{path}'.");
    }

    private void ListGameplayCommands()
    {
        IReadOnlyList<DevToolsCommandData> commands = devToolsManager.State.CollectCommands();
        commandListBuilder.Clear();

        for (int i = 0; i < commands.Count; i++)
        {
            commandListBuilder.Append(i.ToString(CultureInfo.InvariantCulture))
                .Append(Constants.DEVTOOLS_KEY_VALUE_SEPARATOR)
                .AppendLine(commands[i].ConsoleName);
        }

        Debug.Log(commandListBuilder.ToString());
    }

    private void RunGameplayCommand(int index)
    {
        IReadOnlyList<DevToolsCommandData> commands = devToolsManager.State.CollectCommands();

        if (index < 0 || index >= commands.Count)
        {
            Debug.LogWarning($"DevTools: there is no gameplay command with index {index}. Use '{Constants.DEVTOOLS_COMMAND_LIST}' to list them.");
            return;
        }

        commands[index].Run();
    }
}
