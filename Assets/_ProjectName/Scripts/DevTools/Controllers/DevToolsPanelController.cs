using UnityEngine;

public class DevToolsPanelController : UIControllerBase<DevToolsPanelView>
{
    [Header("Tabs")]
    [SerializeField] private PerformanceTabController performanceTab;
    [SerializeField] private TimeTabController timeTab;
    [SerializeField] private StateTabController stateTab;
    [SerializeField] private GameplayTabController gameplayTab;
    [SerializeField] private InputTabController inputTab;
    [SerializeField] private CaptureTabController captureTab;
    [SerializeField] private ConsoleTabController consoleTab;

    private IDevToolsTabController[] tabs;
    private DevToolsManager devToolsManager;
    private DevToolsTab currentTab = DevToolsTab.Performance;

    public DevToolsTab CurrentTab => currentTab;

    public override void Initialize(params object[] parameters)
    {
        devToolsManager = parameters[0] as DevToolsManager;

        bool hasDependencies = RequireDependency(devToolsManager, nameof(devToolsManager))
            & RequireDependency(performanceTab, nameof(performanceTab))
            & RequireDependency(timeTab, nameof(timeTab))
            & RequireDependency(stateTab, nameof(stateTab))
            & RequireDependency(gameplayTab, nameof(gameplayTab))
            & RequireDependency(inputTab, nameof(inputTab))
            & RequireDependency(captureTab, nameof(captureTab))
            & RequireDependency(consoleTab, nameof(consoleTab));

        if (!hasDependencies) return;

        tabs = new IDevToolsTabController[] { performanceTab, timeTab, stateTab, gameplayTab, inputTab, captureTab, consoleTab };

        base.Initialize(parameters);

        foreach (IDevToolsTabController tab in tabs)
            tab.Initialize(devToolsManager);
    }

    public override void Conclude()
    {
        if (tabs != null)
        {
            foreach (IDevToolsTabController tab in tabs)
                tab.Conclude();
        }

        base.Conclude();
    }

    public void ShowPanel()
    {
        View.ShowInstant();
        OpenTab(currentTab, true);
    }

    public void HidePanel()
    {
        foreach (IDevToolsTabController tab in tabs)
            tab.Close();

        View.HideInstant();
    }

    public void SetCaptureHidden(bool hidden)
    {
        View.SetCaptureHidden(hidden);
    }

    public void OnTabClicked(DevToolsTab tab)
    {
        OpenTab(tab, false);
    }

    public void OnCloseClicked()
    {
        devToolsManager.HidePanel();
    }

    private void OpenTab(DevToolsTab tab, bool selectContent)
    {
        currentTab = tab;

        foreach (IDevToolsTabController tabController in tabs)
        {
            if (tabController.Tab != tab)
                tabController.Close();
        }

        View.HighlightTab(tab);
        GetTab(tab).Open();

        if (selectContent)
            GetTab(tab).SelectFirstItem();
        else
            View.SelectTabButton(tab);
    }

    private IDevToolsTabController GetTab(DevToolsTab tab)
    {
        foreach (IDevToolsTabController tabController in tabs)
        {
            if (tabController.Tab == tab)
                return tabController;
        }

        return tabs[0];
    }
}
