public class InputTabController : DevToolsTabControllerBase<InputTabView>
{
    public override DevToolsTab Tab => DevToolsTab.Input;

    protected override void AddListeners()
    {
        base.AddListeners();

        DevToolsManager.InputRecorder.StateChanged += OnRecorderStateChanged;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        DevToolsManager.InputRecorder.StateChanged -= OnRecorderStateChanged;
    }

    protected override void OnViewOpened()
    {
        base.OnViewOpened();

        RefreshView();
    }

    public void OnRecordClicked()
    {
        DevToolsManager.InputRecorder.ToggleRecording();
    }

    public void OnReplayClicked()
    {
        DevToolsManager.StartInputReplay();
    }

    public void OnInputDisplayToggled(bool visible)
    {
        DevToolsManager.SetInputDisplayVisible(visible);
    }

    public void OnOpenFolderClicked()
    {
        DevToolsManager.OpenFolder(FileWriteUtility.GetPersistentFolder(Constants.DEVTOOLS_RECORDINGS_FOLDER));
    }

    private void OnRecorderStateChanged()
    {
        if (!IsOpen) return;

        RefreshView();
    }

    private void RefreshView()
    {
        InputRecorderController recorder = DevToolsManager.InputRecorder;

        View.SetData(recorder.State, recorder.HasRecording, recorder.RecordedEventCount, recorder.LastRecordingPath, DevToolsManager.Preferences.InputDisplayVisible);
    }
}
