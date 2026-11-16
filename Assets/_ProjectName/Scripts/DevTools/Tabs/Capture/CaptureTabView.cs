using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class CaptureTabView : DevToolsTabViewBase
{
    [Header("Controls")]
    [SerializeField] private Button screenshotButton;
    [SerializeField] private Button exportStateButton;
    [SerializeField] private Button openFolderButton;

    [Header("Status")]
    [SerializeField] private LocalizeStringEvent statusText;
    [SerializeField] private LocalizedString captureSavedMessage;
    [SerializeField] private LocalizedString stateSavedMessage;
    [SerializeField] private LocalizedString saveFailedMessage;

    private CaptureTabController captureTabController;

    public override void Initialize(params object[] parameters)
    {
        captureTabController = parameters[0] as CaptureTabController;

        bool hasDependencies = RequireDependency(captureTabController, nameof(captureTabController))
            & RequireDependency(screenshotButton, nameof(screenshotButton))
            & RequireDependency(exportStateButton, nameof(exportStateButton))
            & RequireDependency(openFolderButton, nameof(openFolderButton))
            & RequireDependency(statusText, nameof(statusText));

        if (!hasDependencies) return;

        openFolderButton.gameObject.SetActive(PlatformCapabilityUtility.SupportsFileBrowsing);

        base.Initialize(parameters);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        screenshotButton.onClick.AddListener(captureTabController.OnScreenshotClicked);
        exportStateButton.onClick.AddListener(captureTabController.OnExportStateClicked);
        openFolderButton.onClick.AddListener(captureTabController.OnOpenFolderClicked);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        screenshotButton.onClick.RemoveListener(captureTabController.OnScreenshotClicked);
        exportStateButton.onClick.RemoveListener(captureTabController.OnExportStateClicked);
        openFolderButton.onClick.RemoveListener(captureTabController.OnOpenFolderClicked);
    }

    public void ClearStatus() => ClearMessage(statusText);

    public void ShowCaptureSaved(string path) => ShowPathMessage(captureSavedMessage, path);

    public void ShowStateExported(string path) => ShowPathMessage(stateSavedMessage, path);

    private void ShowPathMessage(LocalizedString message, string path)
    {
        if (string.IsNullOrEmpty(path))
            ShowMessage(statusText, saveFailedMessage);
        else
            ShowMessage(statusText, message, path);
    }
}
