using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class InputTabView : DevToolsTabViewBase
{
    [Header("Controls")]
    [SerializeField] private Button recordButton;
    [SerializeField] private LocalizeStringEvent recordButtonLabel;
    [SerializeField] private LocalizedString recordLabel;
    [SerializeField] private LocalizedString stopLabel;
    [SerializeField] private Button replayButton;
    [SerializeField] private Button openFolderButton;
    [SerializeField] private Toggle inputDisplayToggle;

    [Header("Status")]
    [SerializeField] private LocalizeStringEvent statusText;
    [SerializeField] private LocalizedString idleMessage;
    [SerializeField] private LocalizedString recordingMessage;
    [SerializeField] private LocalizedString replayingMessage;
    [SerializeField] private LocalizedString savedMessage;

    private InputTabController inputTabController;

    public override void Initialize(params object[] parameters)
    {
        inputTabController = parameters[0] as InputTabController;

        bool hasDependencies = RequireDependency(inputTabController, nameof(inputTabController))
            & RequireDependency(recordButton, nameof(recordButton))
            & RequireDependency(recordButtonLabel, nameof(recordButtonLabel))
            & RequireDependency(replayButton, nameof(replayButton))
            & RequireDependency(openFolderButton, nameof(openFolderButton))
            & RequireDependency(inputDisplayToggle, nameof(inputDisplayToggle))
            & RequireDependency(statusText, nameof(statusText));

        if (!hasDependencies) return;

        openFolderButton.gameObject.SetActive(PlatformCapabilityUtility.SupportsFileBrowsing);

        base.Initialize(parameters);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        recordButton.onClick.AddListener(inputTabController.OnRecordClicked);
        replayButton.onClick.AddListener(inputTabController.OnReplayClicked);
        openFolderButton.onClick.AddListener(inputTabController.OnOpenFolderClicked);
        inputDisplayToggle.onValueChanged.AddListener(inputTabController.OnInputDisplayToggled);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        recordButton.onClick.RemoveListener(inputTabController.OnRecordClicked);
        replayButton.onClick.RemoveListener(inputTabController.OnReplayClicked);
        openFolderButton.onClick.RemoveListener(inputTabController.OnOpenFolderClicked);
        inputDisplayToggle.onValueChanged.RemoveListener(inputTabController.OnInputDisplayToggled);
    }

    public void SetData(InputRecorderState state, bool hasRecording, long eventCount, string lastRecordingPath, bool inputDisplayVisible)
    {
        bool isRecording = state == InputRecorderState.Recording;

        ShowMessage(recordButtonLabel, isRecording ? stopLabel : recordLabel);
        recordButton.interactable = state != InputRecorderState.Replaying;
        replayButton.interactable = state == InputRecorderState.Idle && hasRecording;
        inputDisplayToggle.SetIsOnWithoutNotify(inputDisplayVisible);

        switch (state)
        {
            case InputRecorderState.Recording:
                ShowMessage(statusText, recordingMessage);
                break;

            case InputRecorderState.Replaying:
                ShowMessage(statusText, replayingMessage, eventCount);
                break;

            default:
                if (hasRecording && !string.IsNullOrEmpty(lastRecordingPath))
                    ShowMessage(statusText, savedMessage, eventCount, lastRecordingPath);
                else
                    ShowMessage(statusText, idleMessage);
                break;
        }
    }
}
