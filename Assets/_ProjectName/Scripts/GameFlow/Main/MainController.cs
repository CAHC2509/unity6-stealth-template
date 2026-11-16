using UnityEngine;

public class MainController : UIControllerBase<MainView>
{
    [SerializeField] private ConfirmationWindowController exitConfirmationWindow;

    private MainState mainState;
    private SettingsManager settingsManager;

    public override void Initialize(params object[] parameters)
    {
        mainState = parameters[0] as MainState;
        settingsManager = parameters[1] as SettingsManager;

        bool hasDependencies = RequireDependency(mainState, nameof(mainState))
            & RequireDependency(settingsManager, nameof(settingsManager))
            & RequireDependency(exitConfirmationWindow, nameof(exitConfirmationWindow));

        if (!hasDependencies) return;

        base.Initialize(parameters);

        exitConfirmationWindow.Initialize();

        View.SetExitAvailable(PlatformCapabilityUtility.SupportsApplicationQuit);
        View.ShowInstant();
    }

    public override void Conclude()
    {
        exitConfirmationWindow.Conclude();

        base.Conclude();
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        settingsManager.SettingsClosed += OnSettingsClosed;
        exitConfirmationWindow.SubmitSelected += OnExitConfirmed;
        exitConfirmationWindow.CancelSelected += OnExitCancelled;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        settingsManager.SettingsClosed -= OnSettingsClosed;
        exitConfirmationWindow.SubmitSelected -= OnExitConfirmed;
        exitConfirmationWindow.CancelSelected -= OnExitCancelled;
    }

    public void OnPlayClicked()
    {
        mainState.GoToGameplay();
    }

    public void OnSettingsClicked()
    {
        View.HideView()
            .OnComplete(settingsManager.OpenSettingsMenu);
    }

    public void OnExitClicked()
    {
        exitConfirmationWindow.ShowWindow();
    }

    private void OnExitConfirmed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnExitCancelled()
    {
        View.SelectDefaultItem();
    }

    private void OnSettingsClosed()
    {
        View.ShowView();
    }
}
