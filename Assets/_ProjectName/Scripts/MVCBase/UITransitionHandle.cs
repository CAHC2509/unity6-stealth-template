using System;

public class UITransitionHandle
{
    private Action onStart;
    private Action onComplete;
    private Action onCancel;

    private bool isStarted;
    private bool isCompleted;
    private bool isCanceled;

    public bool IsPlaying => isStarted && !isCompleted && !isCanceled;
    public bool IsCompleted => isCompleted;
    public bool IsCanceled => isCanceled;

    public UITransitionHandle OnStart(Action callback)
    {
        if (isStarted)
        {
            callback?.Invoke();
            return this;
        }

        onStart += callback;
        return this;
    }

    public UITransitionHandle OnComplete(Action callback)
    {
        if (isCompleted)
        {
            callback?.Invoke();
            return this;
        }

        onComplete += callback;
        return this;
    }

    public UITransitionHandle OnCancel(Action callback)
    {
        if (isCanceled)
        {
            callback?.Invoke();
            return this;
        }

        onCancel += callback;
        return this;
    }

    public void InvokeStart()
    {
        if (isStarted) return;

        isStarted = true;
        onStart?.Invoke();
    }

    public void InvokeComplete()
    {
        if (isCompleted || isCanceled) return;

        isCompleted = true;
        onComplete?.Invoke();
        Clear();
    }

    public void Cancel()
    {
        if (isCompleted || isCanceled) return;

        isCanceled = true;
        onCancel?.Invoke();
        Clear();
    }

    private void Clear()
    {
        onStart = null;
        onComplete = null;
        onCancel = null;
    }
}