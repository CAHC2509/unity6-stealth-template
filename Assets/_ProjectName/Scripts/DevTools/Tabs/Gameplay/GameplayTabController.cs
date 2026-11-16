using System.Collections.Generic;

public class GameplayTabController : DevToolsTabControllerBase<GameplayTabView>
{
    public override DevToolsTab Tab => DevToolsTab.Gameplay;

    private IReadOnlyList<DevToolsCommandData> commands;

    protected override void AddListeners()
    {
        base.AddListeners();

        DevToolsManager.ProvidersChanged += OnProvidersChanged;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        DevToolsManager.ProvidersChanged -= OnProvidersChanged;
    }

    protected override void OnViewOpened()
    {
        RebuildCommands();

        base.OnViewOpened();
    }

    public void OnCommandClicked(int index)
    {
        if (commands == null || index < 0 || index >= commands.Count) return;

        commands[index].Run();
        View.RefreshToggles(commands);
    }

    public void OnRefreshClicked()
    {
        RebuildCommands();
        View.SelectDefaultItem();
    }

    private void RebuildCommands()
    {
        commands = DevToolsManager.State.CollectCommands();
        View.ShowCommands(commands);
    }

    private void OnProvidersChanged()
    {
        if (!IsOpen) return;

        RebuildCommands();
        View.SelectDefaultItem();
    }
}
