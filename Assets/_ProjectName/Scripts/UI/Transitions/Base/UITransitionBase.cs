using System;
using UnityEngine;

[Serializable]
public abstract class UITransitionBase
{
    protected readonly UITransitionDataBase transitionData;

    protected UITransitionBase(UITransitionDataBase transitionData)
    {
        this.transitionData = transitionData;
    }

    public abstract void PlayShow(CanvasGroup canvasGroup, RectTransform rectTransform, Action onComplete = null);
    public abstract void PlayHide(CanvasGroup canvasGroup, RectTransform rectTransform, Action onComplete = null);
}