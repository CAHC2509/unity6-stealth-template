using UnityEngine;
using UnityEngine.EventSystems;

public class ClickButtonOnSubmitController : ControllerBase
{
    private InputRequest submitInputRequest;

    public override void Initialize(params object[] parameters)
    {
        submitInputRequest = new InputRequest(Globals.GlobalPlayerInput, InputActionMapId.UI, InputActionId.AuxiliarySubmit, InputListenPhase.Started, OnSubmit);

        base.Initialize(parameters);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        InputManager.AddInputListener?.Invoke(submitInputRequest);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        InputManager.RemoveInputListener?.Invoke(submitInputRequest.SubscriptionId);
    }

    private void OnSubmit(InputData inputData)
    {
        GameObject current = EventSystem.current.currentSelectedGameObject;

        if (current == null) return;
        if (!current.activeInHierarchy) return;

        ExecuteEvents.Execute(current, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
    }
}
