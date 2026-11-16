public abstract class DevToolsTabControllerBase<TView> : UIControllerBase<TView>, IDevToolsTabController where TView : DevToolsTabViewBase
{
    protected DevToolsManager DevToolsManager { get; private set; }

    public abstract DevToolsTab Tab { get; }
    public bool IsOpen => View.IsVisible;

    public override void Initialize(params object[] parameters)
    {
        DevToolsManager = parameters[0] as DevToolsManager;

        if (!RequireDependency(DevToolsManager, nameof(DevToolsManager))) return;

        base.Initialize(parameters);
    }

    public void Open()
    {
        View.ShowInstant();
    }

    public void Close()
    {
        View.HideInstant();
    }

    public void SelectFirstItem()
    {
        View.SelectDefaultItem();
    }
}
