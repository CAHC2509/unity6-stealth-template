public class ConsoleTabController : DevToolsTabControllerBase<ConsoleTabView>
{
    public override DevToolsTab Tab => DevToolsTab.Console;

    protected override void AddListeners()
    {
        base.AddListeners();

        DevToolsManager.SessionLog.CountersChanged += OnCountersChanged;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        DevToolsManager.SessionLog.CountersChanged -= OnCountersChanged;
    }

    protected override void OnViewOpened()
    {
        base.OnViewOpened();

        View.ClearStatus();
        View.SetDebugConsoleEnabled(DevToolsManager.Preferences.DebugConsoleEnabled);
        RefreshCounters();
    }

    public void OnDebugConsoleToggled(bool enabled)
    {
        DevToolsManager.SetDebugConsoleEnabled(enabled);
    }

    public void OnExportLogsClicked()
    {
        DevToolsManager.SessionLog.TryExport(out string path);
        View.ShowLogsExported(path);
    }

    public void OnOpenFolderClicked()
    {
        DevToolsManager.OpenFolder(FileWriteUtility.GetPersistentFolder(Constants.DEVTOOLS_LOGS_FOLDER));
    }

    private void OnCountersChanged()
    {
        if (!IsOpen) return;

        RefreshCounters();
    }

    private void RefreshCounters()
    {
        SessionLogController sessionLog = DevToolsManager.SessionLog;
        View.SetCounters(sessionLog.ErrorCount, sessionLog.WarningCount, sessionLog.MessageCount);
    }
}
