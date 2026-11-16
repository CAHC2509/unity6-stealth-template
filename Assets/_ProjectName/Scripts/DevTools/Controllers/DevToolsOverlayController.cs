using System.Globalization;
using System.Text;
using UnityEngine;

public class DevToolsOverlayController : UIControllerBase<DevToolsOverlayView>
{
    [SerializeField, Min(Constants.DEVTOOLS_MIN_INPUT_REFRESH_INTERVAL)] private float inputRefreshInterval = Constants.DEVTOOLS_INPUT_REFRESH_INTERVAL;

    private readonly StringBuilder performanceBuilder = new();
    private readonly StringBuilder stateBuilder = new();
    private readonly StringBuilder inputBuilder = new();

    private DevToolsManager devToolsManager;
    private float inputRefreshElapsed;

    public override void Initialize(params object[] parameters)
    {
        devToolsManager = parameters[0] as DevToolsManager;

        if (!RequireDependency(devToolsManager, nameof(devToolsManager))) return;

        base.Initialize(parameters);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        devToolsManager.PerformanceMonitor.StatisticsUpdated += OnStatisticsUpdated;
        devToolsManager.PreferencesChanged += OnPreferencesChanged;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        devToolsManager.PerformanceMonitor.StatisticsUpdated -= OnStatisticsUpdated;
        devToolsManager.PreferencesChanged -= OnPreferencesChanged;
    }

    public void SetVisible(bool visible)
    {
        if (visible)
        {
            View.ShowInstant();
            RefreshPerformance();
            RefreshInput();
            return;
        }

        View.HideInstant();
    }

    public void SetCaptureHidden(bool hidden)
    {
        View.SetCaptureHidden(hidden);
    }

    private void Update()
    {
        if (!View.IsVisible || !devToolsManager.Preferences.InputDisplayVisible) return;

        inputRefreshElapsed += Time.unscaledDeltaTime;
        if (inputRefreshElapsed < inputRefreshInterval) return;

        inputRefreshElapsed = 0f;
        RefreshInput();
    }

    private void RefreshPerformance()
    {
        FrameTimeStatisticsData statistics = devToolsManager.PerformanceMonitor.Statistics;
        TimeControlController timeControl = devToolsManager.TimeControl;
        InputRecorderController inputRecorder = devToolsManager.InputRecorder;

        performanceBuilder.Clear()
            .Append(Mathf.RoundToInt(statistics.AverageFps).ToString(CultureInfo.InvariantCulture)).Append(Constants.DEVTOOLS_FPS_UNIT)
            .Append(Constants.DEVTOOLS_OVERLAY_SEPARATOR)
            .Append(DevToolsReportUtility.FormatDecimal(statistics.AverageFrameTime * Constants.MILLISECONDS_PER_SECOND)).Append(Constants.DEVTOOLS_MILLISECONDS_UNIT)
            .Append(Constants.DEVTOOLS_OVERLAY_SEPARATOR)
            .Append(Constants.DEVTOOLS_ONE_PERCENT_LOW_LABEL)
            .Append(Mathf.RoundToInt(statistics.OnePercentLowFps).ToString(CultureInfo.InvariantCulture));

        if (timeControl.IsPaused)
            performanceBuilder.Append(Constants.DEVTOOLS_OVERLAY_SEPARATOR).Append(Constants.DEVTOOLS_PAUSED_LABEL);
        else if (timeControl.IsModified)
            performanceBuilder.Append(Constants.DEVTOOLS_OVERLAY_SEPARATOR).Append(Constants.DEVTOOLS_TIME_SCALE_PREFIX).Append(DevToolsReportUtility.FormatDecimal(timeControl.TimeScale));

        if (inputRecorder.IsRecording)
            performanceBuilder.Append(Constants.DEVTOOLS_OVERLAY_SEPARATOR).Append(Constants.DEVTOOLS_RECORDING_LABEL);
        else if (inputRecorder.IsReplaying)
            performanceBuilder.Append(Constants.DEVTOOLS_OVERLAY_SEPARATOR).Append(Constants.DEVTOOLS_REPLAYING_LABEL);

        stateBuilder.Clear();
        devToolsManager.State.WriteOverlaySummary(stateBuilder);

        View.SetPerformanceText(performanceBuilder);
        View.SetStateText(stateBuilder);
    }

    private void RefreshInput()
    {
        bool showInput = devToolsManager.Preferences.InputDisplayVisible;

        View.SetInputVisible(showInput);

        if (!showInput) return;

        inputBuilder.Clear();
        devToolsManager.InputDisplay.WriteActiveControls(inputBuilder);
        View.SetInputText(inputBuilder);
    }

    private void OnStatisticsUpdated()
    {
        if (!View.IsVisible) return;

        RefreshPerformance();
    }

    private void OnPreferencesChanged()
    {
        if (!View.IsVisible) return;

        RefreshInput();
    }
}
