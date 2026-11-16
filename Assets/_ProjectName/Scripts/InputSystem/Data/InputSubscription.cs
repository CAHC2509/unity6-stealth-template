using System;
using UnityEngine.InputSystem;

public class InputSubscription
{
    public PlayerInput Player { get; }
    public InputAction Action { get; }
    public InputListenPhase Phase { get; }
    public Action<InputAction.CallbackContext> Handler { get; }

    public InputSubscription(
        PlayerInput player,
        InputAction action,
        InputListenPhase phase,
        Action<InputAction.CallbackContext> handler)
    {
        Player = player;
        Action = action;
        Phase = phase;
        Handler = handler;
    }
}