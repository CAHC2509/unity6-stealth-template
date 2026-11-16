using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class PerformanceMonitorController : ControllerBase
{
    public event Action StatisticsUpdated;
    public event Action BenchmarkStarted;
    public event Action<string> BenchmarkCompleted;

    [Header("Sampling")]
    [SerializeField, Min(1)] private int statisticsWindowFrames = Constants.DEVTOOLS_STATISTICS_WINDOW_FRAMES;
    [SerializeField, Min(Constants.DEVTOOLS_MIN_REFRESH_INTERVAL)] private float refreshInterval = Constants.DEVTOOLS_REFRESH_INTERVAL;

    [Header("Benchmark")]
    [SerializeField, Min(1f)] private float benchmarkDuration = Constants.DEVTOOLS_BENCHMARK_DURATION;
    [SerializeField, Min(1)] private int benchmarkMaxSamples = Constants.DEVTOOLS_BENCHMARK_MAX_SAMPLES;

    private readonly List<PerformanceSampleData> benchmarkSamples = new();

    private FrameTimeStatisticsData statistics;
    private FrameTimeStatisticsData benchmarkStatistics;
    private PerformanceSampleData currentSample;

    private ProfilerRecorder mainThreadRecorder;
    private ProfilerRecorder renderThreadRecorder;
    private ProfilerRecorder gcAllocatedRecorder;
    private ProfilerRecorder totalUsedMemoryRecorder;
    private ProfilerRecorder gcReservedMemoryRecorder;
    private ProfilerRecorder[] drawCallRecorders;
    private ProfilerRecorder setPassCallsRecorder;
    private ProfilerRecorder trianglesRecorder;
    private ProfilerRecorder verticesRecorder;

    private bool isSampling;
    private bool isBenchmarking;
    private float refreshElapsed;
    private float benchmarkElapsed;

    public FrameTimeStatisticsData Statistics => statistics;
    public PerformanceSampleData CurrentSample => currentSample;
    public bool IsBenchmarking => isBenchmarking;
    public float BenchmarkDuration => benchmarkDuration;
    public float BenchmarkRemaining => isBenchmarking ? Mathf.Max(0f, benchmarkDuration - benchmarkElapsed) : 0f;
    public bool HasMainThreadTiming => mainThreadRecorder.Valid;
    public bool HasRenderThreadTiming => renderThreadRecorder.Valid;
    public bool HasRenderStatistics => setPassCallsRecorder.Valid;

    public override void Initialize(params object[] parameters)
    {
        base.Initialize(parameters);

        statistics = new FrameTimeStatisticsData(statisticsWindowFrames);
        benchmarkStatistics = new FrameTimeStatisticsData(benchmarkMaxSamples);
        benchmarkSamples.Capacity = benchmarkMaxSamples;

        mainThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, Constants.PROFILER_MAIN_THREAD);
        renderThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, Constants.PROFILER_RENDER_THREAD);
        gcAllocatedRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, Constants.PROFILER_GC_ALLOCATED_IN_FRAME);
        totalUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, Constants.PROFILER_TOTAL_USED_MEMORY);
        gcReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, Constants.PROFILER_GC_RESERVED_MEMORY);
        setPassCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, Constants.PROFILER_SET_PASS_CALLS);
        trianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, Constants.PROFILER_TRIANGLES);
        verticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, Constants.PROFILER_VERTICES);

        drawCallRecorders = new ProfilerRecorder[Constants.PROFILER_DRAW_CALL_COUNTERS.Length];

        for (int i = 0; i < drawCallRecorders.Length; i++)
            drawCallRecorders[i] = ProfilerRecorder.StartNew(ProfilerCategory.Render, Constants.PROFILER_DRAW_CALL_COUNTERS[i]);
    }

    public override void Conclude()
    {
        base.Conclude();

        isSampling = false;
        isBenchmarking = false;

        mainThreadRecorder.Dispose();
        renderThreadRecorder.Dispose();
        gcAllocatedRecorder.Dispose();
        totalUsedMemoryRecorder.Dispose();
        gcReservedMemoryRecorder.Dispose();
        setPassCallsRecorder.Dispose();
        trianglesRecorder.Dispose();
        verticesRecorder.Dispose();

        if (drawCallRecorders == null) return;

        for (int i = 0; i < drawCallRecorders.Length; i++)
            drawCallRecorders[i].Dispose();
    }

    public void SetSampling(bool sampling)
    {
        isSampling = sampling;
        refreshElapsed = 0f;

        if (statistics != null)
            statistics.Clear();

        if (!sampling)
            isBenchmarking = false;
    }

    public void StartBenchmark()
    {
        if (!isSampling || isBenchmarking) return;

        benchmarkSamples.Clear();
        benchmarkStatistics.Clear();
        benchmarkElapsed = 0f;
        isBenchmarking = true;

        BenchmarkStarted?.Invoke();
    }

    private void Update()
    {
        if (!isSampling) return;

        float frameTime = Time.unscaledDeltaTime;

        currentSample = ReadSample(frameTime);
        statistics.AddSample(frameTime);

        if (isBenchmarking)
            RecordBenchmarkFrame(frameTime);

        refreshElapsed += frameTime;
        if (refreshElapsed < refreshInterval) return;

        refreshElapsed = 0f;
        statistics.Recalculate();
        StatisticsUpdated?.Invoke();
    }

    private PerformanceSampleData ReadSample(float frameTime)
    {
        return new PerformanceSampleData
        {
            Time = Time.realtimeSinceStartup,
            FrameTimeMs = frameTime * Constants.MILLISECONDS_PER_SECOND,
            MainThreadMs = ReadMilliseconds(mainThreadRecorder),
            RenderThreadMs = ReadMilliseconds(renderThreadRecorder),
            GcAllocatedBytes = ReadValue(gcAllocatedRecorder),
            TotalUsedMemoryBytes = ReadValue(totalUsedMemoryRecorder),
            GcReservedMemoryBytes = ReadValue(gcReservedMemoryRecorder),
            DrawCalls = ReadDrawCalls(),
            SetPassCalls = ReadValue(setPassCallsRecorder),
            Triangles = ReadValue(trianglesRecorder),
            Vertices = ReadValue(verticesRecorder)
        };
    }

    private void RecordBenchmarkFrame(float frameTime)
    {
        if (benchmarkSamples.Count < benchmarkMaxSamples)
        {
            benchmarkSamples.Add(currentSample);
            benchmarkStatistics.AddSample(frameTime);
        }

        benchmarkElapsed += frameTime;
        if (benchmarkElapsed < benchmarkDuration) return;

        CompleteBenchmark();
    }

    private void CompleteBenchmark()
    {
        isBenchmarking = false;
        benchmarkStatistics.Recalculate();

        string path = FileWriteUtility.BuildTimestampedPath(Application.persistentDataPath, Constants.DEVTOOLS_BENCHMARKS_FOLDER, Constants.DEVTOOLS_BENCHMARK_FILE_PREFIX, Constants.DEVTOOLS_CSV_EXTENSION, DateTime.Now);

        try
        {
            FileWriteUtility.WriteAtomic(path, DevToolsReportUtility.BuildBenchmarkCsv(benchmarkSamples, benchmarkStatistics));
        }
        catch (Exception exception)
        {
            Debug.LogError($"{nameof(PerformanceMonitorController)}: could not write benchmark to '{path}'. {exception.Message}", this);
            path = string.Empty;
        }

        BenchmarkCompleted?.Invoke(path);
    }

    private long ReadDrawCalls()
    {
        long total = 0L;

        for (int i = 0; i < drawCallRecorders.Length; i++)
            total += ReadValue(drawCallRecorders[i]);

        return total;
    }

    private static float ReadMilliseconds(ProfilerRecorder recorder)
    {
        return recorder.Valid ? recorder.LastValue / Constants.NANOSECONDS_PER_MILLISECOND : 0f;
    }

    private static long ReadValue(ProfilerRecorder recorder)
    {
        return recorder.Valid ? recorder.LastValue : 0L;
    }
}
