using System;
using UnityEngine;
using DG.Tweening;

[Serializable]
public class UIFadeTransition : UITransitionBase
{
    private readonly Ease showEase = Ease.OutQuad;
    private readonly Ease hideEase = Ease.InQuad;

    public UIFadeTransition(UITransitionDataBase data) : base(data) { }

    public override void PlayShow(CanvasGroup canvasGroup, RectTransform rectTransform, Action onComplete = null)
    {
        canvasGroup.DOKill();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        canvasGroup.DOFade(1f, transitionData.ShowDuration)
            .SetEase(showEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                onComplete?.Invoke();
            });
    }

    public override void PlayHide(CanvasGroup canvasGroup, RectTransform rectTransform, Action onComplete = null)
    {
        canvasGroup.DOKill();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        canvasGroup.DOFade(0f, transitionData.HideDuration)
            .SetEase(hideEase)
            .SetUpdate(true)
            .OnComplete(() => onComplete?.Invoke());
    }
}