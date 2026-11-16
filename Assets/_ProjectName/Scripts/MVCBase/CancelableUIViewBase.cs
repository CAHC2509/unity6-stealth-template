public abstract class CancelableUIViewBase : UIViewBase
{
    private InputRequest cancelRequest;

    protected abstract void OnCancelRequested();

    public override void Initialize(params object[] parameters)
    {
        cancelRequest = new InputRequest(Globals.GlobalPlayerInput, InputActionMapId.UI, InputActionId.Cancel, InputListenPhase.Started, OnCancelInput);

        base.Initialize(parameters);
    }

    protected override void AddInputListeners()
    {
        base.AddInputListeners();

        InputManager.AddInputListener?.Invoke(cancelRequest);
    }

    protected override void RemoveInputListeners()
    {
        base.RemoveInputListeners();

        if (cancelRequest == null) return;

        InputManager.RemoveInputListener?.Invoke(cancelRequest.SubscriptionId);
    }

    private void OnCancelInput(InputData data)
    {
        if (HasActiveSubViews()) return;

        OnCancelRequested();
    }
}
