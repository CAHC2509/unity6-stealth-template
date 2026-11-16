using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class ConsoleTabView : DevToolsTabViewBase
{
    [Header("Controls")]
    [SerializeField] private Toggle debugConsoleToggle;
    [SerializeField] private Button exportLogsButton;
    [SerializeField] private Button openFolderButton;

    [Header("Counters")]
    [SerializeField] private TMP_Text errorsValue;
    [SerializeField] private TMP_Text warningsValue;
    [SerializeField] private TMP_Text messagesValue;

    [Header("Status")]
    [SerializeField] private LocalizeStringEvent statusText;
    [SerializeField] private LocalizedString logsSavedMessage;
    [SerializeField] private LocalizedString saveFailedMessage;

    private ConsoleTabController consoleTabController;

    public override void Initialize(params object[] parameters)
    {
        consoleTabController = parameters[0] as ConsoleTabController;

        bool hasDependencies = RequireDependency(consoleTabController, nameof(consoleTabController))
            & RequireDependency(debugConsoleToggle, nameof(debugConsoleToggle))
            & RequireDependency(exportLogsButton, nameof(exportLogsButton))
            & RequireDependency(openFolderButton, nameof(openFolderButton))
            & RequireDependency(errorsValue, nameof(errorsValue))
            & RequireDependency(warningsValue, nameof(warningsValue))
            & RequireDependency(messagesValue, nameof(messagesValue))
            & RequireDependency(statusText, nameof(statusText));

        if (!hasDependencies) return;

        openFolderButton.gameObject.SetActive(PlatformCapabilityUtility.SupportsFileBrowsing);

        base.Initialize(parameters);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        debugConsoleToggle.onValueChanged.AddListener(consoleTabController.OnDebugConsoleToggled);
        exportLogsButton.onClick.AddListener(consoleTabController.OnExportLogsClicked);
        openFolderButton.onClick.AddListener(consoleTabController.OnOpenFolderClicked);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        debugConsoleToggle.onValueChanged.RemoveListener(consoleTabController.OnDebugConsoleToggled);
        exportLogsButton.onClick.RemoveListener(consoleTabController.OnExportLogsClicked);
        openFolderButton.onClick.RemoveListener(consoleTabController.OnOpenFolderClicked);
    }

    public void SetDebugConsoleEnabled(bool enabled) => debugConsoleToggle.SetIsOnWithoutNotify(enabled);

    public void SetCounters(int errors, int warnings, int messages)
    {
        errorsValue.text = errors.ToString(CultureInfo.InvariantCulture);
        warningsValue.text = warnings.ToString(CultureInfo.InvariantCulture);
        messagesValue.text = messages.ToString(CultureInfo.InvariantCulture);
    }

    public void ClearStatus() => ClearMessage(statusText);

    public void ShowLogsExported(string path)
    {
        if (string.IsNullOrEmpty(path))
            ShowMessage(statusText, saveFailedMessage);
        else
            ShowMessage(statusText, logsSavedMessage, path);
    }
}
