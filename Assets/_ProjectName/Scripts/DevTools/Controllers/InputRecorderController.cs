using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class InputRecorderController : ControllerBase
{
    public event Action StateChanged;

    [SerializeField, Min(1)] private int bufferSizeKilobytes = Constants.DEVTOOLS_INPUT_BUFFER_KILOBYTES;
    [SerializeField, Min(1)] private int maxBufferSizeKilobytes = Constants.DEVTOOLS_INPUT_MAX_BUFFER_KILOBYTES;

    private InputEventTrace trace;
    private InputEventTrace.ReplayController replay;
    private Func<bool> canRecordEvent;
    private InputRecorderState state;
    private string lastRecordingPath = string.Empty;

    public InputRecorderState State => state;
    public bool IsRecording => state == InputRecorderState.Recording;
    public bool IsReplaying => state == InputRecorderState.Replaying;
    public bool HasRecording => trace != null && trace.eventCount > 0;
    public long RecordedEventCount => trace != null ? trace.eventCount : 0L;
    public string LastRecordingPath => lastRecordingPath;

    public override void Initialize(params object[] parameters)
    {
        canRecordEvent = parameters.Length > 0 ? parameters[0] as Func<bool> : null;

        if (!RequireService(canRecordEvent, nameof(canRecordEvent))) return;

        base.Initialize(parameters);

        state = InputRecorderState.Idle;
    }

    public override void Conclude()
    {
        base.Conclude();

        StopReplay();
        StopRecording();
        DisposeTrace();
    }

    public void StartRecording()
    {
        if (state != InputRecorderState.Idle) return;

        DisposeTrace();

        long bufferSize = (long)bufferSizeKilobytes * Constants.BYTES_PER_KILOBYTE;
        long maxBufferSize = (long)maxBufferSizeKilobytes * Constants.BYTES_PER_KILOBYTE;

        trace = new InputEventTrace(bufferSize, true, maxBufferSize, bufferSize);
        trace.onFilterEvent = FilterEvent;
        trace.Enable();

        SetState(InputRecorderState.Recording);
    }

    public void StopRecording()
    {
        if (state != InputRecorderState.Recording) return;

        trace.Disable();
        SetState(InputRecorderState.Idle);

        SaveRecording();
    }

    public void ToggleRecording()
    {
        if (IsRecording)
            StopRecording();
        else
            StartRecording();
    }

    public bool StartReplay()
    {
        if (state != InputRecorderState.Idle || !HasRecording) return false;

        replay = trace.Replay();
        MapRecordedDevices(replay);

        replay.OnFinished(OnReplayFinished).PlayAllEventsAccordingToTimestamps();

        SetState(InputRecorderState.Replaying);
        return true;
    }

    public void StopReplay()
    {
        if (replay != null)
        {
            replay.Dispose();
            replay = null;
        }

        if (state == InputRecorderState.Replaying)
            SetState(InputRecorderState.Idle);
    }

    public bool LoadRecording(string path)
    {
        if (state != InputRecorderState.Idle || !File.Exists(path)) return false;

        try
        {
            InputEventTrace loadedTrace = InputEventTrace.LoadFrom(path);

            DisposeTrace();
            trace = loadedTrace;
            lastRecordingPath = path;

            StateChanged?.Invoke();
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"{nameof(InputRecorderController)}: could not load the recording '{path}'. {exception.Message}", this);
            return false;
        }
    }

    private bool FilterEvent(InputEventPtr eventPtr, InputDevice device)
    {
        return canRecordEvent();
    }

    private void SaveRecording()
    {
        if (!HasRecording) return;

        string path = FileWriteUtility.BuildTimestampedPath(Application.persistentDataPath, Constants.DEVTOOLS_RECORDINGS_FOLDER, Constants.DEVTOOLS_RECORDING_FILE_PREFIX, Constants.DEVTOOLS_RECORDING_EXTENSION, DateTime.Now);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            trace.WriteTo(path);
            lastRecordingPath = path;
        }
        catch (Exception exception)
        {
            Debug.LogError($"{nameof(InputRecorderController)}: could not save the recording to '{path}'. {exception.Message}", this);
            lastRecordingPath = string.Empty;
        }

        StateChanged?.Invoke();
    }

    private void MapRecordedDevices(InputEventTrace.ReplayController controller)
    {
        foreach (InputEventTrace.DeviceInfo deviceInfo in trace.deviceInfos)
        {
            if (InputSystem.GetDeviceById(deviceInfo.deviceId) != null) continue;

            InputDevice playbackDevice = FindDeviceWithLayout(deviceInfo.layout);

            if (playbackDevice != null)
                controller.WithDeviceMappedFromTo(deviceInfo.deviceId, playbackDevice.deviceId);
        }
    }

    private static InputDevice FindDeviceWithLayout(string layout)
    {
        foreach (InputDevice device in InputSystem.devices)
        {
            if (device.layout == layout)
                return device;
        }

        return null;
    }

    private void OnReplayFinished()
    {
        replay?.Dispose();
        replay = null;
        SetState(InputRecorderState.Idle);
    }

    private void DisposeTrace()
    {
        if (trace == null) return;

        trace.Dispose();
        trace = null;
    }

    private void SetState(InputRecorderState newState)
    {
        state = newState;
        StateChanged?.Invoke();
    }
}
