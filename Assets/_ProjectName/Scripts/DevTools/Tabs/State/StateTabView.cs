using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class StateTabView : DevToolsTabViewBase
{
    [Header("State")]
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private Button copyButton;
    [SerializeField] private Button exportButton;

    [Header("Status")]
    [SerializeField] private LocalizeStringEvent statusText;
    [SerializeField] private LocalizedString copiedMessage;
    [SerializeField] private LocalizedString exportedMessage;
    [SerializeField] private LocalizedString exportFailedMessage;

    private StateTabController stateTabController;

    public override void Initialize(params object[] parameters)
    {
        stateTabController = parameters[0] as StateTabController;

        bool hasDependencies = RequireDependency(stateTabController, nameof(stateTabController))
            & RequireDependency(stateText, nameof(stateText))
            & RequireDependency(copyButton, nameof(copyButton))
            & RequireDependency(exportButton, nameof(exportButton))
            & RequireDependency(statusText, nameof(statusText));

        if (!hasDependencies) return;

        base.Initialize(parameters);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        copyButton.onClick.AddListener(stateTabController.OnCopyClicked);
        exportButton.onClick.AddListener(stateTabController.OnExportClicked);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        copyButton.onClick.RemoveListener(stateTabController.OnCopyClicked);
        exportButton.onClick.RemoveListener(stateTabController.OnExportClicked);
    }

    protected override void OnViewOpened()
    {
        base.OnViewOpened();

        ClearMessage(statusText);
    }

    public void SetStateText(string text) => stateText.text = text;

    public void ShowCopied() => ShowMessage(statusText, copiedMessage);

    public void ShowExported(string path)
    {
        if (string.IsNullOrEmpty(path))
            ShowMessage(statusText, exportFailedMessage);
        else
            ShowMessage(statusText, exportedMessage, path);
    }
}
