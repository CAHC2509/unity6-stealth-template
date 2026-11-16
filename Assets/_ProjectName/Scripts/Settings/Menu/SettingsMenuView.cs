using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SettingsMenuView : CancelableUIViewBase
{
    [Header("Transition settings")]
    [SerializeField] private UIFadeTransitionData fadeTransitionData;

    [Header("Settings menu buttons")]
    [SerializeField] private Button graphicsButton;
    [SerializeField] private Button audioButton;
    [SerializeField] private Button controlsButton;
    [SerializeField, FormerlySerializedAs("sensibilityButton")] private Button sensitivityButton;
    [SerializeField] private Button languageButton;
    [SerializeField] private Button developerButton;

    private SettingsMenuController settingsMenuController;

    protected override UITransitionBase CreateTransition() => new UIFadeTransition(fadeTransitionData);

    protected override void Awake()
    {
        defaultSelection = graphicsButton;

        base.Awake();
    }

    public override void Initialize(params object[] parameters)
    {
        settingsMenuController = parameters[0] as SettingsMenuController;

        RequireDependency(settingsMenuController, nameof(settingsMenuController));
        RequireDependency(graphicsButton, nameof(graphicsButton));
        RequireDependency(audioButton, nameof(audioButton));
        RequireDependency(controlsButton, nameof(controlsButton));
        RequireDependency(sensitivityButton, nameof(sensitivityButton));
        RequireDependency(languageButton, nameof(languageButton));
        RequireDependency(developerButton, nameof(developerButton));

        base.Initialize(parameters);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        audioButton.onClick.AddListener(OnAudioButtonClicked);
        graphicsButton.onClick.AddListener(OnGraphicsButtonClicked);
        controlsButton.onClick.AddListener(OnControlsButtonClicked);
        sensitivityButton.onClick.AddListener(OnSensitivityButtonClicked);
        languageButton.onClick.AddListener(OnLanguageButtonClicked);
        developerButton.onClick.AddListener(OnDeveloperButtonClicked);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        audioButton.onClick.RemoveListener(OnAudioButtonClicked);
        graphicsButton.onClick.RemoveListener(OnGraphicsButtonClicked);
        controlsButton.onClick.RemoveListener(OnControlsButtonClicked);
        sensitivityButton.onClick.RemoveListener(OnSensitivityButtonClicked);
        languageButton.onClick.RemoveListener(OnLanguageButtonClicked);
        developerButton.onClick.RemoveListener(OnDeveloperButtonClicked);
    }

    public void SetDeveloperAvailable(bool available)
    {
        developerButton.gameObject.SetActive(available);
    }

    protected override void OnCancelRequested()
    {
        settingsMenuController.CloseSettingsMenu();
    }

    private void OnGraphicsButtonClicked()
    {
        currentSelection = graphicsButton;
        settingsMenuController.OpenGraphicsMenu();
    }

    private void OnAudioButtonClicked()
    {
        currentSelection = audioButton;
        settingsMenuController.OpenAudioMenu();
    }

    private void OnControlsButtonClicked()
    {
        currentSelection = controlsButton;
        settingsMenuController.OpenControlsMenu();
    }

    private void OnSensitivityButtonClicked()
    {
        currentSelection = sensitivityButton;
        settingsMenuController.OpenSensitivityMenu();
    }

    private void OnLanguageButtonClicked()
    {
        currentSelection = languageButton;
        settingsMenuController.OpenLanguageMenu();
    }

    private void OnDeveloperButtonClicked()
    {
        currentSelection = developerButton;
        settingsMenuController.OpenDeveloperMenu();
    }
}
