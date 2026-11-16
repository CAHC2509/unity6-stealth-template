using System;
using UnityEngine;

public class FrameTimeStatisticsData
{
    private readonly float[] frameTimes;
    private readonly float[] sortedFrameTimes;

    private int nextIndex;
    private int sampleCount;

    public int Capacity => frameTimes.Length;
    public int SampleCount => sampleCount;
    public float AverageFrameTime { get; private set; }
    public float MinFrameTime { get; private set; }
    public float MaxFrameTime { get; private set; }
    public float OnePercentLowFrameTime { get; private set; }

    public float AverageFps => ToFps(AverageFrameTime);
    public float MinFps => ToFps(MaxFrameTime);
    public float MaxFps => ToFps(MinFrameTime);
    public float OnePercentLowFps => ToFps(OnePercentLowFrameTime);

    public FrameTimeStatisticsData(int capacity)
    {
        int safeCapacity = Mathf.Max(1, capacity);

        frameTimes = new float[safeCapacity];
        sortedFrameTimes = new float[safeCapacity];
    }

    public void Clear()
    {
        nextIndex = 0;
        sampleCount = 0;
        AverageFrameTime = 0f;
        MinFrameTime = 0f;
        MaxFrameTime = 0f;
        OnePercentLowFrameTime = 0f;
    }

    public void AddSample(float frameTime)
    {
        if (frameTime <= 0f) return;

        frameTimes[nextIndex] = frameTime;
        nextIndex = (nextIndex + 1) % frameTimes.Length;
        sampleCount = Mathf.Min(sampleCount + 1, frameTimes.Length);
    }

    public void Recalculate()
    {
        if (sampleCount == 0)
        {
            Clear();
            return;
        }

        float total = 0f;
        float min = float.MaxValue;
        float max = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float frameTime = frameTimes[i];

            total += frameTime;
            min = Mathf.Min(min, frameTime);
            max = Mathf.Max(max, frameTime);
            sortedFrameTimes[i] = frameTime;
        }

        Array.Sort(sortedFrameTimes, 0, sampleCount);

        int slowestCount = Mathf.Max(1, Mathf.CeilToInt(sampleCount * Constants.DEVTOOLS_ONE_PERCENT_RATIO));
        float slowestTotal = 0f;

        for (int i = sampleCount - slowestCount; i < sampleCount; i++)
            slowestTotal += sortedFrameTimes[i];

        AverageFrameTime = total / sampleCount;
        MinFrameTime = min;
        MaxFrameTime = max;
        OnePercentLowFrameTime = slowestTotal / slowestCount;
    }

    private static float ToFps(float frameTime) => frameTime > 0f ? 1f / frameTime : 0f;
}
