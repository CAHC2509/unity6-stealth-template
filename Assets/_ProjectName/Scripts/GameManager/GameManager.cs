using System;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class GameManager : ManagerBase
{
    public static Action<GameStateBase> SetNewGameStateRequest;

    [SerializeField] private SceneLoadingManager sceneLoadingManager;
    [SerializeField] private LoadingEffectsController loadingEffectsController;
    [SerializeField] private SettingsManager settingsManager;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private DevToolsManager devToolsManager;
    [SerializeField] private GameStates currentState;
    [SerializeField] private UserData userData;

    private GameStateBase currentGameState;
    private GameStates pendingState;
    private CoroutineHandle reenterStateCoroutine;

    public GameStates CurrentState => currentState;

    private void Start() => Initialize();
    private void OnApplicationQuit() => Conclude();

    public override void Initialize(object[] parameters)
    {
        bool hasDependencies = RequireDependency(sceneLoadingManager, nameof(sceneLoadingManager))
            & RequireDependency(loadingEffectsController, nameof(loadingEffectsController))
            & RequireDependency(settingsManager, nameof(settingsManager))
            & RequireDependency(inputManager, nameof(inputManager))
            & RequireDependency(devToolsManager, nameof(devToolsManager));

        if (!hasDependencies) return;

        base.Initialize(parameters);

        SaveSystem.CreateImplementation(SaveSystemType.JSON);

        inputManager.Initialize();
        sceneLoadingManager.Initialize();
        loadingEffectsController.Initialize();
        devToolsManager.Initialize(this);
        settingsManager.Initialize();

        pendingState = currentState;
        sceneLoadingManager.LoadScene(currentState.ToString());
    }

    public override void Conclude()
    {
        base.Conclude();

        if (reenterStateCoroutine.IsRunning)
            Timing.KillCoroutines(reenterStateCoroutine);

        sceneLoadingManager.Conclude();
        loadingEffectsController.Conclude();
        settingsManager.Conclude();
        devToolsManager.Conclude();
        inputManager.Conclude();
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        SetNewGameStateRequest += OnSetNewGameStateRequest;
        sceneLoadingManager.SwitchScenesCompleted += OnSwitchScenesCompleted;
        sceneLoadingManager.SwitchScenesFailed += OnSwitchScenesFailed;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        SetNewGameStateRequest -= OnSetNewGameStateRequest;
        sceneLoadingManager.SwitchScenesCompleted -= OnSwitchScenesCompleted;
        sceneLoadingManager.SwitchScenesFailed -= OnSwitchScenesFailed;

        if (currentGameState != null)
            currentGameState.StateFinished -= OnCurrentStateFinished;
    }

    private void OnSetNewGameStateRequest(GameStateBase newGameState)
    {
        if (currentGameState != null)
            currentGameState.StateFinished -= OnCurrentStateFinished;

        currentGameState = newGameState;

        switch (currentGameState)
        {
            case MainState main:
                main.Dependencies(settingsManager);
                break;
        }
    }

    private void OnSwitchScenesCompleted()
    {
        currentState = pendingState;

        loadingEffectsController.HideLoadingBanner();

        currentGameState.EnterState();
        currentGameState.StateFinished += OnCurrentStateFinished;
    }

    private void OnCurrentStateFinished(GameStates nextState)
    {
        pendingState = nextState;

        loadingEffectsController.ShowLoadingBanner();
        sceneLoadingManager.SwitchScenes(currentState.ToString(), nextState.ToString());
    }

    private void OnSwitchScenesFailed()
    {
        pendingState = currentState;
        reenterStateCoroutine = Timing.RunCoroutine(ReenterCurrentStateNextFrame().CancelWith(gameObject));
    }

    private IEnumerator<float> ReenterCurrentStateNextFrame()
    {
        yield return Timing.WaitForOneFrame;

        loadingEffectsController.HideLoadingBanner();
        currentGameState.EnterState();
    }
}
