using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public static class DevToolsReportUtility
{
    public static string BuildBenchmarkCsv(IReadOnlyList<PerformanceSampleData> samples, FrameTimeStatisticsData summary)
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine(Constants.DEVTOOLS_BENCHMARK_CSV_HEADER);

        for (int i = 0; i < samples.Count; i++)
        {
            PerformanceSampleData sample = samples[i];

            builder.Append(FormatDecimal(sample.Time)).Append(Constants.DEVTOOLS_CSV_SEPARATOR)
                .Append(FormatDecimal(sample.FrameTimeMs)).Append(Constants.DEVTOOLS_CSV_SEPARATOR)
                .Append(FormatDecimal(sample.MainThreadMs)).Append(Constants.DEVTOOLS_CSV_SEPARATOR)
                .Append(FormatDecimal(sample.RenderThreadMs)).Append(Constants.DEVTOOLS_CSV_SEPARATOR)
                .Append(sample.GcAllocatedBytes.ToString(CultureInfo.InvariantCulture)).Append(Constants.DEVTOOLS_CSV_SEPARATOR)
                .Append(sample.TotalUsedMemoryBytes.ToString(CultureInfo.InvariantCulture)).Append(Constants.DEVTOOLS_CSV_SEPARATOR)
                .Append(sample.GcReservedMemoryBytes.ToString(CultureInfo.InvariantCulture)).Append(Constants.DEVTOOLS_CSV_SEPARATOR)
                .Append(sample.DrawCalls.ToString(CultureInfo.InvariantCulture)).Append(Constants.DEVTOOLS_CSV_SEPARATOR)
                .Append(sample.SetPassCalls.ToString(CultureInfo.InvariantCulture)).Append(Constants.DEVTOOLS_CSV_SEPARATOR)
                .Append(sample.Triangles.ToString(CultureInfo.InvariantCulture)).Append(Constants.DEVTOOLS_CSV_SEPARATOR)
                .AppendLine(sample.Vertices.ToString(CultureInfo.InvariantCulture));
        }

        if (summary == null || summary.SampleCount == 0) return builder.ToString();

        builder.AppendLine()
            .AppendLine(Constants.DEVTOOLS_BENCHMARK_SUMMARY_HEADER)
            .Append(FormatDecimal(summary.AverageFps)).Append(Constants.DEVTOOLS_CSV_SEPARATOR)
            .Append(FormatDecimal(summary.MinFps)).Append(Constants.DEVTOOLS_CSV_SEPARATOR)
            .Append(FormatDecimal(summary.MaxFps)).Append(Constants.DEVTOOLS_CSV_SEPARATOR)
            .Append(FormatDecimal(summary.OnePercentLowFps)).Append(Constants.DEVTOOLS_CSV_SEPARATOR)
            .AppendLine(FormatDecimal(summary.AverageFrameTime * Constants.MILLISECONDS_PER_SECOND));

        return builder.ToString();
    }

    public static string BuildSessionLog(IReadOnlyList<DevToolsLogEntryData> entries, string sessionHeader, int droppedEntries)
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine(sessionHeader);

        if (droppedEntries > 0)
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, Constants.DEVTOOLS_LOG_DROPPED_FORMAT, droppedEntries));

        builder.AppendLine();

        for (int i = 0; i < entries.Count; i++)
            AppendLogEntry(builder, entries[i]);

        return builder.ToString();
    }

    public static string BuildSessionHeader(string sessionStart, string exportTime)
    {
        StringBuilder builder = new StringBuilder();

        builder.Append(Constants.DEVTOOLS_LOG_HEADER_PRODUCT).AppendLine($"{Application.productName} {Application.version}")
            .Append(Constants.DEVTOOLS_LOG_HEADER_ENGINE).AppendLine(Application.unityVersion)
            .Append(Constants.DEVTOOLS_LOG_HEADER_PLATFORM).AppendLine(Application.platform.ToString())
            .Append(Constants.DEVTOOLS_LOG_HEADER_DEVICE).AppendLine($"{SystemInfo.operatingSystem} | {SystemInfo.processorType} | {SystemInfo.graphicsDeviceName}")
            .Append(Constants.DEVTOOLS_LOG_HEADER_SESSION_START).AppendLine(sessionStart)
            .Append(Constants.DEVTOOLS_LOG_HEADER_EXPORTED).Append(exportTime);

        return builder.ToString();
    }

    public static void AppendLogEntry(StringBuilder builder, DevToolsLogEntryData entry)
    {
        builder.Append('[').Append(FormatDecimal(entry.Time)).Append(Constants.DEVTOOLS_LOG_TIME_SUFFIX)
            .Append(Constants.DEVTOOLS_LOG_FRAME_PREFIX).Append(entry.Frame.ToString(CultureInfo.InvariantCulture)).Append("] ")
            .Append('[').Append(entry.Type.ToString()).Append("] ")
            .AppendLine(entry.Message);

        if (!ShouldIncludeStackTrace(entry.Type) || string.IsNullOrEmpty(entry.StackTrace)) return;

        builder.AppendLine(entry.StackTrace.TrimEnd());
    }

    public static bool ShouldIncludeStackTrace(LogType type)
    {
        return type == LogType.Error || type == LogType.Exception || type == LogType.Assert;
    }

    public static string FormatDecimal(float value) => value.ToString(Constants.DEVTOOLS_DECIMAL_FORMAT, CultureInfo.InvariantCulture);

    public static string FormatMegabytes(long bytes)
    {
        return FormatDecimal(bytes / (float)Constants.BYTES_PER_MEGABYTE);
    }

    public static string FormatKilobytes(long bytes)
    {
        return FormatDecimal(bytes / (float)Constants.BYTES_PER_KILOBYTE);
    }
}
