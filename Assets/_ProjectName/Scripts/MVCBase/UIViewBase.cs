using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using MEC;

public abstract class UIViewBase : ViewBase
{
    public event Action ViewOpened;
    public event Action ViewClosed;

    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected RectTransform rectTransform;

    protected UITransitionDataBase transitionData;
    protected Selectable defaultSelection;
    protected Selectable currentSelection;
    protected virtual bool RequiresSelection => true;
    protected virtual bool StartsHidden => true;

    public bool IsVisible => isVisible;

    private CoroutineHandle selectionHandle;
    private UITransitionHandle activeHandle;
    private UITransitionHandle pendingHandle;
    private bool pendingShow;
    private bool isTransitioning;
    private bool isVisible;
    private UITransitionBase transition;
    private int activeSubViews;

    protected abstract UITransitionBase CreateTransition();
    protected virtual void AddInputListeners() { }
    protected virtual void RemoveInputListeners() { }
    protected virtual void OnViewShowStarted() { }
    protected virtual void OnViewHideStarted() { }

    protected virtual void Awake()
    {
        if (transitionData == null)
            transitionData = new UITransitionDataBase();

        transition = CreateTransition();

        if (transition == null)
        {
            transition = new UIFadeTransition(transitionData);
            Debug.LogWarning($"{GetType().Name} must provide a valid transition, using {transition.GetType().Name} by default.", this);
        }

        currentSelection = defaultSelection;
    }

    public override void Initialize(params object[] parameters)
    {
        RequireDependency(canvasGroup, nameof(canvasGroup));
        RequireDependency(rectTransform, nameof(rectTransform));

        base.Initialize(parameters);

        if (StartsHidden)
            HideInstant();
        else
            isVisible = gameObject.activeSelf;
    }

    public override void Conclude()
    {
        base.Conclude();

        if (StartsHidden)
            HideInstant();
    }

    public UITransitionHandle ShowView()
    {
        UITransitionHandle handle = new UITransitionHandle();
        RequestTransition(true, handle);
        return handle;
    }

    public UITransitionHandle HideView()
    {
        UITransitionHandle handle = new UITransitionHandle();
        RequestTransition(false, handle);
        return handle;
    }

    public void ShowInstant(Action onComplete = null)
    {
        StopTransitions();

        gameObject.SetActive(true);
        isVisible = true;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        OnViewShowStarted();
        OnViewOpened();
        ViewOpened?.Invoke();

        onComplete?.Invoke();
    }

    public void HideInstant(Action onComplete = null)
    {
        StopTransitions();

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        OnViewHideStarted();
        OnViewClosed();
        ViewClosed?.Invoke();

        gameObject.SetActive(false);
        isVisible = false;

        onComplete?.Invoke();
    }

    private void RequestTransition(bool show, UITransitionHandle handle)
    {
        if (isTransitioning)
        {
            QueueTransition(show, handle);
            return;
        }

        if (show)
            PlayShowTransition(handle);
        else
            PlayHideTransition(handle);
    }

    private void QueueTransition(bool show, UITransitionHandle handle)
    {
        if (pendingHandle != null && pendingShow == show)
        {
            pendingHandle.OnComplete(handle.InvokeComplete);
            pendingHandle.OnCancel(handle.Cancel);
            return;
        }

        pendingHandle?.Cancel();

        pendingShow = show;
        pendingHandle = handle;
    }

    private void PlayShowTransition(UITransitionHandle handle)
    {
        if (isVisible)
        {
            handle.InvokeComplete();
            return;
        }

        isTransitioning = true;
        isVisible = true;
        activeHandle = handle;

        gameObject.SetActive(true);
        OnViewShowStarted();
        handle.InvokeStart();

        transition.PlayShow(canvasGroup, rectTransform, () =>
        {
            if (activeHandle != handle) return;

            FinishTransition();

            OnViewOpened();
            ViewOpened?.Invoke();

            handle.InvokeComplete();
            ProcessPendingTransition();
        });
    }

    private void PlayHideTransition(UITransitionHandle handle)
    {
        if (!CanHideView())
        {
            handle.Cancel();
            return;
        }

        if (!isVisible)
        {
            handle.InvokeComplete();
            return;
        }

        isTransitioning = true;
        isVisible = false;
        activeHandle = handle;

        OnViewHideStarted();
        handle.InvokeStart();

        transition.PlayHide(canvasGroup, rectTransform, () =>
        {
            if (activeHandle != handle) return;

            FinishTransition();

            OnViewClosed();
            ViewClosed?.Invoke();

            gameObject.SetActive(false);

            handle.InvokeComplete();
            ProcessPendingTransition();
        });
    }

    private void FinishTransition()
    {
        isTransitioning = false;
        activeHandle = null;
    }

    private void ProcessPendingTransition()
    {
        if (pendingHandle == null) return;

        UITransitionHandle handle = pendingHandle;
        pendingHandle = null;

        RequestTransition(pendingShow, handle);
    }

    private void StopTransitions()
    {
        canvasGroup.DOKill();
        rectTransform.DOKill();

        isTransitioning = false;

        activeHandle?.Cancel();
        activeHandle = null;

        pendingHandle?.Cancel();
        pendingHandle = null;
    }

    public void SelectCurrentItem()
    {
        if (currentSelection == null)
        {
            Debug.LogWarning($"[{name}] Trying to select a null item.", this);
            return;
        }

        ClearSelection();
        EventSystem.current.SetSelectedGameObject(currentSelection.gameObject);
    }

    public void SetCurrentSelection(Selectable selectable)
    {
        currentSelection = selectable;
    }

    public void SelectDefaultItem()
    {
        if (defaultSelection == null)
        {
            Debug.LogWarning($"[{name}] Default selection shouldn't be null.", this);
            return;
        }

        currentSelection = defaultSelection;
        SelectCurrentItem();
    }

    protected void ClearSelection()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    protected void SelectCurrentItemDelayed()
    {
        if (selectionHandle.IsRunning)
            Timing.KillCoroutines(selectionHandle);

        selectionHandle = Timing.RunCoroutine(SelectNextFrame().CancelWith(gameObject));
    }

    private IEnumerator<float> SelectNextFrame()
    {
        yield return Timing.WaitForOneFrame;
        SelectCurrentItem();
    }

    protected virtual void OnViewOpened()
    {
        if (currentSelection == null)
            currentSelection = defaultSelection;

        AddInputListeners();

        if (RequiresSelection)
            SelectCurrentItemDelayed();
    }

    protected virtual void OnViewClosed()
    {
        RemoveInputListeners();
    }

    public void RegisterSubViewOpen()
    {
        activeSubViews++;
    }

    public void RegisterSubViewClose()
    {
        activeSubViews = Mathf.Max(0, activeSubViews - 1);
    }

    public bool CanHideView()
    {
        return activeSubViews <= 0;
    }

    public bool HasActiveSubViews()
    {
        return activeSubViews > 0;
    }
}