using System;
using UnityEngine;

public abstract class GameStateBase : ManagerBase
{
    public event Action<GameStates> StateFinished;

    public GameStates NextState { get; protected set; }

    private void Start()
    {
        GameManager.SetNewGameStateRequest?.Invoke(this);
    }

    public void EnterState()
    {
        Initialize();
    }

    public void ExitState()
    {
        StateFinished?.Invoke(NextState);
        Conclude();
    }
}
