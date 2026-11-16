using System;
using System.Collections.Generic;
using System.IO;
using MEC;
using UnityEngine;

public class ScreenCaptureController : ControllerBase
{
    public event Action CaptureStarted;
    public event Action<string> CaptureCompleted;
    public event Action<string> StateExported;

    private CoroutineHandle captureHandle;

    public bool IsCapturing => captureHandle.IsRunning;
    public string CapturesFolder => FileWriteUtility.GetPersistentFolder(Constants.DEVTOOLS_CAPTURES_FOLDER);

    public override void Conclude()
    {
        base.Conclude();

        if (captureHandle.IsRunning)
            Timing.KillCoroutines(captureHandle);
    }

    public void CaptureScreenshot(Func<string> buildStateJson)
    {
        if (IsCapturing) return;

        captureHandle = Timing.RunCoroutine(CaptureRoutine(buildStateJson).CancelWith(gameObject));
    }

    public bool TryExportState(string stateJson, out string path)
    {
        path = BuildCapturePath(DateTime.Now, Constants.DEVTOOLS_JSON_EXTENSION);
        bool written = TryWrite(path, stateJson);

        if (!written)
            path = string.Empty;

        StateExported?.Invoke(path);
        return written;
    }

    private IEnumerator<float> CaptureRoutine(Func<string> buildStateJson)
    {
        CaptureStarted?.Invoke();

        yield return Timing.WaitForOneFrame;

        DateTime timestamp = DateTime.Now;
        string imagePath = BuildCapturePath(timestamp, Constants.DEVTOOLS_PNG_EXTENSION);
        string statePath = BuildCapturePath(timestamp, Constants.DEVTOOLS_JSON_EXTENSION);

        Directory.CreateDirectory(CapturesFolder);
        ScreenCapture.CaptureScreenshot(imagePath);
        TryWrite(statePath, buildStateJson());

        for (int i = 0; i < Constants.DEVTOOLS_CAPTURE_SETTLE_FRAMES; i++)
            yield return Timing.WaitForOneFrame;

        CaptureCompleted?.Invoke(imagePath);
    }

    private string BuildCapturePath(DateTime timestamp, string extension)
    {
        return FileWriteUtility.BuildTimestampedPath(Application.persistentDataPath, Constants.DEVTOOLS_CAPTURES_FOLDER, Constants.DEVTOOLS_CAPTURE_FILE_PREFIX, extension, timestamp);
    }

    private bool TryWrite(string path, string content)
    {
        try
        {
            FileWriteUtility.WriteAtomic(path, content);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"{nameof(ScreenCaptureController)}: could not write '{path}'. {exception.Message}", this);
            return false;
        }
    }
}
