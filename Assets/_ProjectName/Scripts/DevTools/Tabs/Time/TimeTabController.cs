public class TimeTabController : DevToolsTabControllerBase<TimeTabView>
{
    public override DevToolsTab Tab => DevToolsTab.Time;

    protected override void AddListeners()
    {
        base.AddListeners();

        DevToolsManager.TimeControl.TimeChanged += OnTimeChanged;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        DevToolsManager.TimeControl.TimeChanged -= OnTimeChanged;
    }

    protected override void OnViewOpened()
    {
        base.OnViewOpened();

        RefreshView();
    }

    public void OnTimeScaleChanged(float value) => DevToolsManager.TimeControl.SetTimeScale(value);

    public void OnPauseToggled(bool paused) => DevToolsManager.TimeControl.SetPaused(paused);

    public void OnStepClicked() => DevToolsManager.TimeControl.StepFrame();

    public void OnResetClicked() => DevToolsManager.TimeControl.ResetTime();

    private void OnTimeChanged()
    {
        if (!IsOpen) return;

        RefreshView();
    }

    private void RefreshView()
    {
        TimeControlController timeControl = DevToolsManager.TimeControl;
        View.SetData(timeControl.TimeScale, timeControl.IsPaused);
    }
}
