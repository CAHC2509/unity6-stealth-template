using UnityEngine;

public class PerformanceTabController : DevToolsTabControllerBase<PerformanceTabView>
{
    public override DevToolsTab Tab => DevToolsTab.Performance;

    private PerformanceMonitorController monitor;
    private float benchmarkRefreshElapsed;

    public override void Initialize(params object[] parameters)
    {
        base.Initialize(parameters);

        if (DevToolsManager == null) return;

        monitor = DevToolsManager.PerformanceMonitor;
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        DevToolsManager.PerformanceMonitor.StatisticsUpdated += OnStatisticsUpdated;
        DevToolsManager.PerformanceMonitor.BenchmarkStarted += OnBenchmarkStarted;
        DevToolsManager.PerformanceMonitor.BenchmarkCompleted += OnBenchmarkCompleted;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        DevToolsManager.PerformanceMonitor.StatisticsUpdated -= OnStatisticsUpdated;
        DevToolsManager.PerformanceMonitor.BenchmarkStarted -= OnBenchmarkStarted;
        DevToolsManager.PerformanceMonitor.BenchmarkCompleted -= OnBenchmarkCompleted;
    }

    protected override void OnViewOpened()
    {
        base.OnViewOpened();

        View.SetOverlayVisible(DevToolsManager.Preferences.OverlayVisible);
        View.SetBenchmarkRunning(monitor.IsBenchmarking);
        View.ShowStatistics(monitor);
    }

    public void OnOverlayToggled(bool visible)
    {
        DevToolsManager.SetOverlayVisible(visible);
    }

    public void OnBenchmarkClicked()
    {
        monitor.StartBenchmark();
    }

    private void Update()
    {
        if (!IsOpen || !monitor.IsBenchmarking) return;

        benchmarkRefreshElapsed += Time.unscaledDeltaTime;
        if (benchmarkRefreshElapsed < Constants.DEVTOOLS_REFRESH_INTERVAL) return;

        benchmarkRefreshElapsed = 0f;
        View.ShowBenchmarkProgress(monitor.BenchmarkRemaining);
    }

    private void OnStatisticsUpdated()
    {
        if (!IsOpen) return;

        View.ShowStatistics(monitor);
    }

    private void OnBenchmarkStarted()
    {
        benchmarkRefreshElapsed = 0f;

        if (!IsOpen) return;

        View.SetBenchmarkRunning(true);
        View.ShowBenchmarkProgress(monitor.BenchmarkRemaining);
    }

    private void OnBenchmarkCompleted(string path)
    {
        if (!IsOpen) return;

        View.SetBenchmarkRunning(false);
        View.ShowBenchmarkResult(path);
    }
}
