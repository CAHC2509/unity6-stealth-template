using UnityEngine;

public abstract class ConfirmationWindowViewBase : CancelableUIViewBase
{
    [Header("Transition settings")]
    [SerializeField] private UIZoomFadeTransitionData zoomFadeTransitionData;

    protected ConfirmationWindowController controller;

    protected override UITransitionBase CreateTransition() => new UIZoomFadeTransition(zoomFadeTransitionData);

    protected override void Awake()
    {
        if (zoomFadeTransitionData != null)
            transitionData = zoomFadeTransitionData;
        else
            zoomFadeTransitionData = new UIZoomFadeTransitionData();

        base.Awake();
    }

    public override void Initialize(params object[] parameters)
    {
        controller = parameters[0] as ConfirmationWindowController;

        RequireDependency(controller, nameof(controller));

        base.Initialize(parameters);
    }

    protected override void OnCancelRequested()
    {
        controller.Cancel();
    }
}
