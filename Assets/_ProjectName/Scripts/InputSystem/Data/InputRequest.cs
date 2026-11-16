using System;
using UnityEngine.InputSystem;

public class InputRequest
{
    public int SubscriptionId { get; internal set; }

    public PlayerInput PlayerOwner { get; }
    public InputActionMapId MapId { get; }
    public InputActionId ActionId { get; }
    public InputListenPhase Phase { get; }
    public Action<InputData> OnInput { get; }

    public InputRequest(
        PlayerInput owner,
        InputActionMapId mapId,
        InputActionId actionId,
        InputListenPhase phase,
        Action<InputData> onInput)
    {
        PlayerOwner = owner;
        MapId = mapId;
        ActionId = actionId;
        Phase = phase;
        OnInput = onInput;
    }
}