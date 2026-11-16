using UnityEngine;

public class MainState : GameStateBase
{
    [SerializeField] private MainController mainController;

    private SettingsManager settingsManager;

    public void Dependencies(SettingsManager settingsManager)
    {
        this.settingsManager = settingsManager;
    }

    public override void Initialize(params object[] parameters)
    {
        bool hasDependencies = RequireDependency(mainController, nameof(mainController))
            & RequireDependency(settingsManager, nameof(settingsManager));

        if (!hasDependencies) return;

        base.Initialize(parameters);

        mainController.Initialize(this, settingsManager);
    }

    public override void Conclude()
    {
        base.Conclude();

        mainController.Conclude();
    }

    public void GoToGameplay()
    {
        NextState = GameStates.Gameplay;
        ExitState();
    }
}
