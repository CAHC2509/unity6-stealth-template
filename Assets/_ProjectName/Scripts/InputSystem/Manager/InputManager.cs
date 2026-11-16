using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputManager : ManagerBase
{
    public static Action<InputRequest> AddInputListener;
    public static Action<int> RemoveInputListener;

    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private PlayerInput globalPlayerInput;
    [SerializeField] private ClickButtonOnSubmitController clickButtonOnSubmit;

    private List<PlayerInput> activePlayers = new();
    private Dictionary<PlayerInput, Dictionary<(InputActionMapId, InputActionId), InputAction>> cachedActions;
    private readonly Dictionary<int, InputSubscription> subscriptions = new();
    private int nextSubscriptionId = 1;

    public override void Initialize(params object[] parameters)
    {
        bool hasDependencies = RequireDependency(playerInputManager, nameof(playerInputManager))
            & RequireDependency(globalPlayerInput, nameof(globalPlayerInput))
            & RequireDependency(clickButtonOnSubmit, nameof(clickButtonOnSubmit));

        if (!hasDependencies) return;

        Globals.GlobalPlayerInput = globalPlayerInput;
        Globals.GlobalDeviceType = GetDeviceType(globalPlayerInput);
        Globals.ActivePlayers = new List<PlayerInput>();

        CacheGlobalPlayer();
        EnablePersistentMaps(globalPlayerInput);

        base.Initialize(parameters);

        clickButtonOnSubmit.Initialize();
    }

    public override void Conclude()
    {
        base.Conclude();

        clickButtonOnSubmit.Conclude();
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        AddInputListener += OnAddInputListener;
        RemoveInputListener += OnRemoveInputListener;

        playerInputManager.onPlayerJoined += OnPlayerJoined;
        playerInputManager.onPlayerLeft += OnPlayerLeft;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        AddInputListener -= OnAddInputListener;
        RemoveInputListener -= OnRemoveInputListener;

        playerInputManager.onPlayerJoined -= OnPlayerJoined;
        playerInputManager.onPlayerLeft -= OnPlayerLeft;
    }

    private void CacheGlobalPlayer()
    {
        if (cachedActions == null)
            cachedActions = new();

        activePlayers.Add(globalPlayerInput);
        Globals.ActivePlayers.Add(globalPlayerInput);
        cachedActions[globalPlayerInput] = BuildCache(globalPlayerInput);
    }

    private void OnAddInputListener(InputRequest request)
    {
        PlayerInput player = request.PlayerOwner != null ? request.PlayerOwner : globalPlayerInput;

        if (!cachedActions.TryGetValue(player, out Dictionary<(InputActionMapId, InputActionId), InputAction> cache))
        {
            Debug.LogError("InputManager: Player not registered in cache", this);
            return;
        }

        if (!cache.TryGetValue((request.MapId, request.ActionId), out InputAction action))
        {
            Debug.LogError($"InputManager: action not found {request.ActionId} in {request.MapId}", this);
            return;
        }

        Action<InputAction.CallbackContext> handler = context =>
        {
            if (!IsPersistentMap(request.MapId) && context.action.actionMap != player.currentActionMap) return;

            InputData inputData = ReadInputData(context);
            request.OnInput?.Invoke(inputData);
        };

        Attach(action, request.Phase, handler);

        int subscriptionId = nextSubscriptionId++;
        request.SubscriptionId = subscriptionId;

        subscriptions.Add(subscriptionId, new InputSubscription(player, action, request.Phase, handler));
    }

    private void OnRemoveInputListener(int subscriptionId)
    {
        if (!subscriptions.TryGetValue(subscriptionId, out InputSubscription subscription)) return;

        Detach(subscription.Action, subscription.Phase, subscription.Handler);
        subscriptions.Remove(subscriptionId);
    }

    private void OnPlayerJoined(PlayerInput player)
    {
        if (cachedActions == null)
            cachedActions = new();

        if (activePlayers.Contains(player)) return;

        activePlayers.Add(player);
        Globals.ActivePlayers.Add(player);

        Dictionary<(InputActionMapId, InputActionId), InputAction> cache = BuildCache(player);
        cachedActions[player] = cache;
        EnablePersistentMaps(player);

        Debug.Log($"<color=yellow>New device paired: {player.devices[0].displayName}</color>");
    }

    private void OnPlayerLeft(PlayerInput player)
    {
        if (!activePlayers.Contains(player)) return;

        activePlayers.Remove(player);
        cachedActions.Remove(player);
        Globals.ActivePlayers.Remove(player);

        List<int> toRemove = new();

        foreach (KeyValuePair<int, InputSubscription> pair in subscriptions)
        {
            if (pair.Value.Player == player)
            {
                Detach(pair.Value.Action, pair.Value.Phase, pair.Value.Handler);
                toRemove.Add(pair.Key);
            }
        }

        foreach (int id in toRemove)
            subscriptions.Remove(id);

        Debug.Log($"<color=yellow>Device unpaired: {player.devices[0].displayName}</color>");
    }

    private static bool IsPersistentMap(InputActionMapId mapId) => mapId == InputActionMapId.Debug;

    private void EnablePersistentMaps(PlayerInput player)
    {
        foreach (InputActionMapId mapId in Enum.GetValues(typeof(InputActionMapId)))
        {
            if (!IsPersistentMap(mapId)) continue;

            InputActionMap map = player.actions.FindActionMap(mapId.ToString(), false);

            if (map != null)
                map.Enable();
        }
    }

    private Dictionary<(InputActionMapId, InputActionId), InputAction> BuildCache(PlayerInput player)
    {
        Dictionary<(InputActionMapId, InputActionId), InputAction> cache = new();

        foreach (InputActionMapId mapId in Enum.GetValues(typeof(InputActionMapId)))
        {
            InputActionMap map = player.actions.FindActionMap(mapId.ToString(), false);
            if (map == null) continue;

            foreach (InputActionId actionId in Enum.GetValues(typeof(InputActionId)))
            {
                InputAction action = map.FindAction(actionId.ToString(), false);
                
                if (action != null)
                    cache[(mapId, actionId)] = action;
            }
        }

        return cache;
    }

    private void Attach(InputAction action, InputListenPhase phase, Action<InputAction.CallbackContext> handler)
    {
        switch (phase)
        {
            case InputListenPhase.Started:
                action.started += handler;
                break;

            case InputListenPhase.Performed:
                action.performed += handler;
                break;

            case InputListenPhase.Canceled:
                action.canceled += handler;
                break;
        }
    }

    private void Detach(InputAction action, InputListenPhase phase, Action<InputAction.CallbackContext> handler)
    {
        switch (phase)
        {
            case InputListenPhase.Started:
                action.started -= handler;
                break;

            case InputListenPhase.Performed:
                action.performed -= handler;
                break;

            case InputListenPhase.Canceled:
                action.canceled -= handler;
                break;
        }
    }

    private InputData ReadInputData(InputAction.CallbackContext context)
    {
        Type valueType = context.valueType;

        if (valueType == typeof(Vector2))
            return new InputData(context.ReadValue<Vector2>(), context.control is StickControl);

        if (valueType == typeof(float))
            return new InputData(context.ReadValue<float>());

        if (valueType == typeof(bool))
            return new InputData(context.ReadValueAsButton());

        return new InputData(context.ReadValue<float>());
    }

    private DeviceType GetDeviceType(PlayerInput player)
    {
        if (player == null || player.devices.Count == 0)
            return DeviceType.KeyboardMouse;

        return DeviceTypeUtility.FromDevice(player.devices[0]);
    }
}