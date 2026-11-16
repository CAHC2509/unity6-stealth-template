using UnityEngine;
using UnityEngine.UI;

public class DeveloperSettingsView : CancelableUIViewBase
{
    [Header("Transition settings")]
    [SerializeField] private UIFadeTransitionData fadeTransitionData;

    [Header("Developer Controls")]
    [SerializeField] private Toggle devToolsToggle;
    [SerializeField] private Button backButton;

    private DeveloperSettingsController developerSettingsController;

    protected override UITransitionBase CreateTransition() => new UIFadeTransition(fadeTransitionData);

    protected override void Awake()
    {
        defaultSelection = devToolsToggle;

        base.Awake();
    }

    public override void Initialize(params object[] parameters)
    {
        developerSettingsController = parameters[0] as DeveloperSettingsController;

        bool hasDependencies = RequireDependency(developerSettingsController, nameof(developerSettingsController))
            & RequireDependency(devToolsToggle, nameof(devToolsToggle))
            & RequireDependency(backButton, nameof(backButton));

        if (!hasDependencies) return;

        base.Initialize(parameters);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        devToolsToggle.onValueChanged.AddListener(developerSettingsController.UpdateDevToolsEnabled);
        backButton.onClick.AddListener(developerSettingsController.CloseSubMenu);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        devToolsToggle.onValueChanged.RemoveListener(developerSettingsController.UpdateDevToolsEnabled);
        backButton.onClick.RemoveListener(developerSettingsController.CloseSubMenu);
    }

    public void SetData(DeveloperSettingsData data)
    {
        devToolsToggle.SetIsOnWithoutNotify(data.DevToolsEnabled);
    }

    protected override void OnCancelRequested()
    {
        developerSettingsController.CloseSubMenu();
    }
}
