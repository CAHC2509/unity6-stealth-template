using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;

public class DevToolsTests
{
    private const float FAST_FRAME = 0.01f;
    private const float SLOW_FRAME = 0.1f;
    private const float TOLERANCE = 0.0001f;

    [Test]
    public void FrameTimeStatistics_ComputesAverageExtremesAndOnePercentLow()
    {
        FrameTimeStatisticsData statistics = new FrameTimeStatisticsData(100);

        for (int i = 0; i < 99; i++)
            statistics.AddSample(FAST_FRAME);

        statistics.AddSample(SLOW_FRAME);
        statistics.Recalculate();

        Assert.AreEqual(100, statistics.SampleCount);
        Assert.AreEqual((FAST_FRAME * 99 + SLOW_FRAME) / 100f, statistics.AverageFrameTime, TOLERANCE);
        Assert.AreEqual(FAST_FRAME, statistics.MinFrameTime, TOLERANCE);
        Assert.AreEqual(SLOW_FRAME, statistics.MaxFrameTime, TOLERANCE);
        Assert.AreEqual(SLOW_FRAME, statistics.OnePercentLowFrameTime, TOLERANCE);
        Assert.AreEqual(10f, statistics.MinFps, TOLERANCE);
        Assert.AreEqual(100f, statistics.MaxFps, TOLERANCE);
    }

    [Test]
    public void FrameTimeStatistics_KeepsOnlyTheLatestWindow()
    {
        FrameTimeStatisticsData statistics = new FrameTimeStatisticsData(4);

        for (int i = 0; i < 4; i++)
            statistics.AddSample(SLOW_FRAME);

        for (int i = 0; i < 4; i++)
            statistics.AddSample(FAST_FRAME);

        statistics.Recalculate();

        Assert.AreEqual(4, statistics.SampleCount);
        Assert.AreEqual(FAST_FRAME, statistics.MaxFrameTime, TOLERANCE);
    }

    [Test]
    public void FrameTimeStatistics_IgnoresNonPositiveSamplesAndHandlesEmpty()
    {
        FrameTimeStatisticsData statistics = new FrameTimeStatisticsData(10);

        statistics.AddSample(0f);
        statistics.AddSample(-1f);
        statistics.Recalculate();

        Assert.AreEqual(0, statistics.SampleCount);
        Assert.AreEqual(0f, statistics.AverageFps);
    }

    [Test]
    public void BenchmarkCsv_HasHeaderOneRowPerSampleAndSummary()
    {
        List<PerformanceSampleData> samples = new List<PerformanceSampleData>
        {
            new PerformanceSampleData { Time = 1f, FrameTimeMs = 16.67f, DrawCalls = 120 },
            new PerformanceSampleData { Time = 1.016f, FrameTimeMs = 16.5f, DrawCalls = 118 }
        };

        FrameTimeStatisticsData summary = new FrameTimeStatisticsData(2);
        summary.AddSample(FAST_FRAME);
        summary.AddSample(FAST_FRAME);
        summary.Recalculate();

        string[] lines = DevToolsReportUtility.BuildBenchmarkCsv(samples, summary).Replace("\r", string.Empty).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        Assert.AreEqual(5, lines.Length);
        Assert.AreEqual(Constants.DEVTOOLS_BENCHMARK_CSV_HEADER, lines[0]);
        StringAssert.StartsWith("1.00,16.67,", lines[1]);
        StringAssert.Contains(",120,", lines[1]);
        Assert.AreEqual(Constants.DEVTOOLS_BENCHMARK_SUMMARY_HEADER, lines[3]);
        StringAssert.StartsWith("100.00,", lines[4]);
    }

    [Test]
    public void SessionLog_IncludesStackTracesOnlyForErrorsAndReportsDroppedEntries()
    {
        List<DevToolsLogEntryData> entries = new List<DevToolsLogEntryData>
        {
            new DevToolsLogEntryData(1.5f, 10, LogType.Log, "Loaded", "LogStack"),
            new DevToolsLogEntryData(2f, 20, LogType.Error, "Broken", "ErrorStack")
        };

        string log = DevToolsReportUtility.BuildSessionLog(entries, "HEADER", 3);

        StringAssert.StartsWith("HEADER", log);
        StringAssert.Contains(string.Format(Constants.DEVTOOLS_LOG_DROPPED_FORMAT, 3), log);
        StringAssert.Contains("[1.50s f10] [Log] Loaded", log);
        StringAssert.Contains("[2.00s f20] [Error] Broken", log);
        StringAssert.Contains("ErrorStack", log);
        StringAssert.DoesNotContain("LogStack", log);
    }

    [Test]
    public void StateSnapshot_WritesSectionsAsTextAndJson()
    {
        DevToolsStateSnapshotData snapshot = new DevToolsStateSnapshotData();
        snapshot.Begin(new DateTime(2026, 1, 2, 3, 4, 5), 42, 1f);

        DevToolsStateSectionData section = snapshot.BeginSection("Player");
        section.Add("health", 3);
        section.Add("alive", true);
        section.Add("position", new Vector3(1f, 2f, 3f));

        StringBuilder builder = new StringBuilder();
        snapshot.WriteText(builder);
        string text = builder.ToString();
        string json = snapshot.ToJson();

        StringAssert.Contains("[Player]", text);
        StringAssert.Contains("health: 3", text);
        StringAssert.Contains("alive: true", text);
        StringAssert.Contains("position: (1.00, 2.00, 3.00)", text);
        StringAssert.Contains("\"capturedAt\": \"2026-01-02 03:04:05\"", json);
        StringAssert.Contains("\"frame\": 42", json);
        StringAssert.Contains("\"key\": \"health\"", json);
    }

    [Test]
    public void StateSnapshot_ReusedForASmallerCapture_DropsStaleEntries()
    {
        DevToolsStateSnapshotData snapshot = new DevToolsStateSnapshotData();

        snapshot.Begin(DateTime.Now, 1, 0f);
        DevToolsStateSectionData first = snapshot.BeginSection("A");
        first.Add("one", 1);
        first.Add("two", 2);
        snapshot.BeginSection("B").Add("three", 3);

        snapshot.Begin(DateTime.Now, 2, 0f);
        snapshot.BeginSection("A").Add("one", 1);

        StringBuilder builder = new StringBuilder();
        snapshot.WriteText(builder);
        string json = snapshot.ToJson();

        Assert.AreEqual(1, snapshot.SectionCount);
        Assert.IsNull(snapshot.FindSection("B"));
        StringAssert.DoesNotContain("two", builder.ToString());
        StringAssert.DoesNotContain("three", json);
    }

    [Test]
    public void ToggleCommand_RunFlipsTheValue()
    {
        bool value = false;
        DevToolsCommandData command = DevToolsCommandData.CreateToggle("invisible", "label", () => value, newValue => value = newValue);

        command.Run();
        Assert.IsTrue(value);

        command.Run();
        Assert.IsFalse(value);
        Assert.IsTrue(command.IsToggle);
    }

    [Test]
    public void ActionCommand_RunInvokesTheActionAndKeepsArguments()
    {
        int calls = 0;
        DevToolsCommandData command = DevToolsCommandData.CreateAction("checkpoint2", "label", () => calls++, 2);

        command.Run();

        Assert.AreEqual(1, calls);
        Assert.IsFalse(command.IsToggle);
        Assert.AreEqual(2, command.LabelArguments[0]);
    }

    [Test]
    public void TimestampedPath_UsesFolderPrefixAndTimestamp()
    {
        string path = FileWriteUtility.BuildTimestampedPath("root", "DevTools/Logs", "session", "txt", new DateTime(2026, 11, 16, 9, 8, 7));

        Assert.AreEqual(Path.GetFullPath(Path.Combine("root", "DevTools", "Logs", "session_20261116_090807.txt")), path);
    }

    [Test]
    public void WriteAtomic_CreatesMissingFoldersAndOverwrites()
    {
        string root = Path.Combine(Path.GetTempPath(), "DevToolsTests_" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(root, "nested", "file.txt");

        try
        {
            FileWriteUtility.WriteAtomic(path, "first");
            FileWriteUtility.WriteAtomic(path, "second");

            Assert.AreEqual("second", File.ReadAllText(path));
            Assert.IsFalse(File.Exists(path + Constants.TEMPORARY_FILE_SUFFIX));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }
}
