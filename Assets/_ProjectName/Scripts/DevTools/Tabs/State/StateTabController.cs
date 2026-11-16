using UnityEngine;

public class StateTabController : DevToolsTabControllerBase<StateTabView>
{
    [SerializeField, Min(Constants.DEVTOOLS_MIN_REFRESH_INTERVAL)] private float refreshInterval = Constants.DEVTOOLS_REFRESH_INTERVAL;

    public override DevToolsTab Tab => DevToolsTab.State;

    private float refreshElapsed;
    private string lastStateText = string.Empty;

    protected override void OnViewOpened()
    {
        base.OnViewOpened();

        Refresh();
    }

    public void OnCopyClicked()
    {
        GUIUtility.systemCopyBuffer = lastStateText;
        View.ShowCopied();
    }

    public void OnExportClicked()
    {
        DevToolsManager.ScreenCapture.TryExportState(DevToolsManager.State.CaptureJson(), out string path);
        View.ShowExported(path);
    }

    private void Update()
    {
        if (!IsOpen) return;

        refreshElapsed += Time.unscaledDeltaTime;
        if (refreshElapsed < refreshInterval) return;

        refreshElapsed = 0f;
        Refresh();
    }

    private void Refresh()
    {
        lastStateText = DevToolsManager.State.CaptureText();
        View.SetStateText(lastStateText);
    }
}
