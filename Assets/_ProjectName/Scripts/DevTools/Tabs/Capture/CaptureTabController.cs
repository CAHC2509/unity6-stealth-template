public class CaptureTabController : DevToolsTabControllerBase<CaptureTabView>
{
    public override DevToolsTab Tab => DevToolsTab.Capture;

    protected override void AddListeners()
    {
        base.AddListeners();

        DevToolsManager.ScreenCapture.CaptureCompleted += OnCaptureCompleted;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        DevToolsManager.ScreenCapture.CaptureCompleted -= OnCaptureCompleted;
    }

    protected override void OnViewOpened()
    {
        base.OnViewOpened();

        View.ClearStatus();
    }

    public void OnScreenshotClicked()
    {
        DevToolsManager.ScreenCapture.CaptureScreenshot(DevToolsManager.State.CaptureJson);
    }

    public void OnExportStateClicked()
    {
        DevToolsManager.ScreenCapture.TryExportState(DevToolsManager.State.CaptureJson(), out string path);
        View.ShowStateExported(path);
    }

    public void OnOpenFolderClicked()
    {
        DevToolsManager.OpenFolder(DevToolsManager.ScreenCapture.CapturesFolder);
    }

    private void OnCaptureCompleted(string path)
    {
        if (!IsOpen) return;

        View.ShowCaptureSaved(path);
    }
}
