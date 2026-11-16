using UnityEngine;
using UnityEngine.UI;

public class ControlSettingsView : CancelableUIViewBase
{
    [Header("Transition settings")]
    [SerializeField] private UIFadeTransitionData fadeTransitionData;

    [Header("Controls settings")]
    [SerializeField] private Button openKeyboardPanelButton;
    [SerializeField] private Button openGamepadPanelButton;
    [SerializeField] private Button resetAllButton;
    [SerializeField] private GameObject keyboardPanel;
    [SerializeField] private GameObject gamepadPanel;

    private ControlsSettingsController controlsSettingsController;
    private Selectable keyboardPanelDefaultSelection;
    private Selectable gamepadPanelDefaultSelection;

    protected override UITransitionBase CreateTransition() => new UIFadeTransition(fadeTransitionData);

    public override void Initialize(params object[] parameters)
    {
        controlsSettingsController = parameters[0] as ControlsSettingsController;

        RequireDependency(controlsSettingsController, nameof(controlsSettingsController));
        RequireDependency(openKeyboardPanelButton, nameof(openKeyboardPanelButton));
        RequireDependency(openGamepadPanelButton, nameof(openGamepadPanelButton));
        RequireDependency(resetAllButton, nameof(resetAllButton));
        RequireDependency(keyboardPanel, nameof(keyboardPanel));
        RequireDependency(gamepadPanel, nameof(gamepadPanel));

        keyboardPanelDefaultSelection = keyboardPanel.GetComponentInChildren<Button>(true);
        gamepadPanelDefaultSelection = gamepadPanel.GetComponentInChildren<Button>(true);

        base.Initialize(parameters);

        gamepadPanel.SetActive(false);
        keyboardPanel.SetActive(false);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        resetAllButton.onClick.AddListener(OnResetAllClicked);
        openKeyboardPanelButton.onClick.AddListener(OnOpenKeyboardPanelClicked);
        openGamepadPanelButton.onClick.AddListener(OnOpenGamepadPanelClicked);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        resetAllButton.onClick.RemoveListener(OnResetAllClicked);
        openKeyboardPanelButton.onClick.RemoveListener(OnOpenKeyboardPanelClicked);
        openGamepadPanelButton.onClick.RemoveListener(OnOpenGamepadPanelClicked);
    }

    protected override void OnViewShowStarted()
    {
        base.OnViewShowStarted();

        if (Globals.GlobalDeviceType == DeviceType.Gamepad)
            openGamepadPanelButton.onClick.Invoke();
        else
            openKeyboardPanelButton.onClick.Invoke();
    }

    protected override void OnViewClosed()
    {
        base.OnViewClosed();

        gamepadPanel.SetActive(false);
        keyboardPanel.SetActive(false);
    }

    protected override void OnCancelRequested()
    {
        controlsSettingsController.CloseSubMenu();
    }

    private void OnOpenKeyboardPanelClicked()
    {
        ShowPanel(keyboardPanel, gamepadPanel, keyboardPanelDefaultSelection);
    }

    private void OnOpenGamepadPanelClicked()
    {
        ShowPanel(gamepadPanel, keyboardPanel, gamepadPanelDefaultSelection);
    }

    private void ShowPanel(GameObject panelToShow, GameObject panelToHide, Selectable panelDefaultSelection)
    {
        panelToHide.SetActive(false);
        panelToShow.SetActive(true);

        defaultSelection = panelDefaultSelection;
        SelectDefaultItem();
    }

    private void OnResetAllClicked()
    {
        controlsSettingsController.RestoreDefaultSettings();
    }
}
