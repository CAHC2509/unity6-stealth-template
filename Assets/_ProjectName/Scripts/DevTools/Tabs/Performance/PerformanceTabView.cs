using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class PerformanceTabView : DevToolsTabViewBase
{
    [Header("Values")]
    [SerializeField] private TMP_Text fpsValue;
    [SerializeField] private TMP_Text frameTimeValue;
    [SerializeField] private TMP_Text mainThreadValue;
    [SerializeField] private TMP_Text renderThreadValue;
    [SerializeField] private TMP_Text gcAllocatedValue;
    [SerializeField] private TMP_Text usedMemoryValue;
    [SerializeField] private TMP_Text gcReservedValue;
    [SerializeField] private TMP_Text drawCallsValue;
    [SerializeField] private TMP_Text setPassCallsValue;
    [SerializeField] private TMP_Text trianglesValue;
    [SerializeField] private TMP_Text verticesValue;

    [Header("Controls")]
    [SerializeField] private Toggle overlayToggle;
    [SerializeField] private Button benchmarkButton;

    [Header("Status")]
    [SerializeField] private LocalizeStringEvent statusText;
    [SerializeField] private LocalizedString benchmarkRunningMessage;
    [SerializeField] private LocalizedString benchmarkSavedMessage;
    [SerializeField] private LocalizedString benchmarkFailedMessage;

    private PerformanceTabController performanceTabController;

    public override void Initialize(params object[] parameters)
    {
        performanceTabController = parameters[0] as PerformanceTabController;

        bool hasDependencies = RequireDependency(performanceTabController, nameof(performanceTabController))
            & RequireDependency(fpsValue, nameof(fpsValue))
            & RequireDependency(frameTimeValue, nameof(frameTimeValue))
            & RequireDependency(mainThreadValue, nameof(mainThreadValue))
            & RequireDependency(renderThreadValue, nameof(renderThreadValue))
            & RequireDependency(gcAllocatedValue, nameof(gcAllocatedValue))
            & RequireDependency(usedMemoryValue, nameof(usedMemoryValue))
            & RequireDependency(gcReservedValue, nameof(gcReservedValue))
            & RequireDependency(drawCallsValue, nameof(drawCallsValue))
            & RequireDependency(setPassCallsValue, nameof(setPassCallsValue))
            & RequireDependency(trianglesValue, nameof(trianglesValue))
            & RequireDependency(verticesValue, nameof(verticesValue))
            & RequireDependency(overlayToggle, nameof(overlayToggle))
            & RequireDependency(benchmarkButton, nameof(benchmarkButton))
            & RequireDependency(statusText, nameof(statusText));

        if (!hasDependencies) return;

        base.Initialize(parameters);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        overlayToggle.onValueChanged.AddListener(performanceTabController.OnOverlayToggled);
        benchmarkButton.onClick.AddListener(performanceTabController.OnBenchmarkClicked);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        overlayToggle.onValueChanged.RemoveListener(performanceTabController.OnOverlayToggled);
        benchmarkButton.onClick.RemoveListener(performanceTabController.OnBenchmarkClicked);
    }

    public void ShowStatistics(PerformanceMonitorController monitor)
    {
        FrameTimeStatisticsData statistics = monitor.Statistics;
        PerformanceSampleData sample = monitor.CurrentSample;

        fpsValue.text = string.Format(CultureInfo.InvariantCulture, Constants.DEVTOOLS_FPS_SUMMARY_FORMAT, statistics.AverageFps, statistics.MinFps, statistics.MaxFps, statistics.OnePercentLowFps);
        frameTimeValue.text = string.Format(CultureInfo.InvariantCulture, Constants.DEVTOOLS_FRAME_TIME_FORMAT, statistics.AverageFrameTime * Constants.MILLISECONDS_PER_SECOND, statistics.MaxFrameTime * Constants.MILLISECONDS_PER_SECOND);
        mainThreadValue.text = monitor.HasMainThreadTiming ? DevToolsReportUtility.FormatDecimal(sample.MainThreadMs) : Constants.DEVTOOLS_NOT_AVAILABLE_TEXT;
        renderThreadValue.text = monitor.HasRenderThreadTiming ? DevToolsReportUtility.FormatDecimal(sample.RenderThreadMs) : Constants.DEVTOOLS_NOT_AVAILABLE_TEXT;
        gcAllocatedValue.text = DevToolsReportUtility.FormatKilobytes(sample.GcAllocatedBytes);
        usedMemoryValue.text = DevToolsReportUtility.FormatMegabytes(sample.TotalUsedMemoryBytes);
        gcReservedValue.text = DevToolsReportUtility.FormatMegabytes(sample.GcReservedMemoryBytes);

        if (!monitor.HasRenderStatistics)
        {
            drawCallsValue.text = Constants.DEVTOOLS_NOT_AVAILABLE_TEXT;
            setPassCallsValue.text = Constants.DEVTOOLS_NOT_AVAILABLE_TEXT;
            trianglesValue.text = Constants.DEVTOOLS_NOT_AVAILABLE_TEXT;
            verticesValue.text = Constants.DEVTOOLS_NOT_AVAILABLE_TEXT;
            return;
        }

        drawCallsValue.text = sample.DrawCalls.ToString(CultureInfo.InvariantCulture);
        setPassCallsValue.text = sample.SetPassCalls.ToString(CultureInfo.InvariantCulture);
        trianglesValue.text = sample.Triangles.ToString(CultureInfo.InvariantCulture);
        verticesValue.text = sample.Vertices.ToString(CultureInfo.InvariantCulture);
    }

    public void SetOverlayVisible(bool visible) => overlayToggle.SetIsOnWithoutNotify(visible);

    public void SetBenchmarkRunning(bool running)
    {
        benchmarkButton.interactable = !running;

        if (!running)
            ClearMessage(statusText);
    }

    public void ShowBenchmarkProgress(float remainingSeconds)
    {
        ShowMessage(statusText, benchmarkRunningMessage, Mathf.CeilToInt(remainingSeconds));
    }

    public void ShowBenchmarkResult(string path)
    {
        if (string.IsNullOrEmpty(path))
            ShowMessage(statusText, benchmarkFailedMessage);
        else
            ShowMessage(statusText, benchmarkSavedMessage, path);
    }
}
