using UnityEngine;
using UnityEngine.UI;

public class MainView : UIViewBase
{
    [Header("Transition settings")]
    [SerializeField] private UIFadeTransitionData fadeTransitionData;

    [Header("Main menu buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    private MainController mainController;

    protected override UITransitionBase CreateTransition() => new UIFadeTransition(fadeTransitionData);

    protected override void Awake()
    {
        defaultSelection = playButton;

        base.Awake();
    }

    public override void Initialize(params object[] parameters)
    {
        mainController = parameters[0] as MainController;

        RequireDependency(mainController, nameof(mainController));
        RequireDependency(playButton, nameof(playButton));
        RequireDependency(settingsButton, nameof(settingsButton));
        RequireDependency(exitButton, nameof(exitButton));

        base.Initialize(parameters);
    }

    public void SetExitAvailable(bool isAvailable)
    {
        exitButton.gameObject.SetActive(isAvailable);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        playButton.onClick.AddListener(mainController.OnPlayClicked);
        settingsButton.onClick.AddListener(mainController.OnSettingsClicked);
        exitButton.onClick.AddListener(mainController.OnExitClicked);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        playButton.onClick.RemoveListener(mainController.OnPlayClicked);
        settingsButton.onClick.RemoveListener(mainController.OnSettingsClicked);
        exitButton.onClick.RemoveListener(mainController.OnExitClicked);
    }
}
