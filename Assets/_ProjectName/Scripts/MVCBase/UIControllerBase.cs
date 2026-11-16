using UnityEngine;

public abstract class UIControllerBase<TView> : ControllerBase where TView : UIViewBase
{
    [Header("View")]
    [SerializeField] private TView view;

    public TView View => view;

    public override void Initialize(params object[] parameters)
    {
        base.Initialize(parameters);

        if (!RequireDependency(view, nameof(view))) return;

        view.ViewOpened += OnViewOpened;
        view.ViewClosed += OnViewClosed;

        view.Initialize(this);
    }

    public override void Conclude()
    {
        if (view != null)
        {
            view.ViewOpened -= OnViewOpened;
            view.ViewClosed -= OnViewClosed;

            view.Conclude();
        }

        base.Conclude();
    }

    protected virtual void OnViewOpened() { }

    protected virtual void OnViewClosed() { }
}
