using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;

public class TimeControlController : ControllerBase
{
    public event Action TimeChanged;

    [SerializeField, Range(Constants.DEVTOOLS_MIN_TIME_SCALE, Constants.DEVTOOLS_MAX_TIME_SCALE)] private float defaultTimeScale = Constants.DEVTOOLS_DEFAULT_TIME_SCALE;

    private CoroutineHandle stepHandle;
    private float timeScale;
    private bool isPaused;

    public float TimeScale => timeScale;
    public bool IsPaused => isPaused;
    public bool IsModified => isPaused || !Mathf.Approximately(timeScale, defaultTimeScale);

    public override void Initialize(params object[] parameters)
    {
        base.Initialize(parameters);

        timeScale = defaultTimeScale;
        isPaused = false;
    }

    public override void Conclude()
    {
        base.Conclude();

        ResetTime();
    }

    public void SetTimeScale(float value)
    {
        timeScale = Mathf.Clamp(value, Constants.DEVTOOLS_MIN_TIME_SCALE, Constants.DEVTOOLS_MAX_TIME_SCALE);
        Apply();
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
        Apply();
    }

    public void TogglePause() => SetPaused(!isPaused);

    public void StepFrame()
    {
        if (!isPaused || stepHandle.IsRunning) return;

        stepHandle = Timing.RunCoroutine(StepOneFrame().CancelWith(gameObject));
    }

    public void ResetTime()
    {
        if (stepHandle.IsRunning)
            Timing.KillCoroutines(stepHandle);

        timeScale = defaultTimeScale;
        isPaused = false;
        Apply();
    }

    private void Apply()
    {
        Time.timeScale = isPaused ? 0f : timeScale;
        TimeChanged?.Invoke();
    }

    private IEnumerator<float> StepOneFrame()
    {
        Time.timeScale = timeScale;

        yield return Timing.WaitForOneFrame;

        if (isPaused)
            Time.timeScale = 0f;

        TimeChanged?.Invoke();
    }
}
