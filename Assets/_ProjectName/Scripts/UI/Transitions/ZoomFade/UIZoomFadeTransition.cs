using System;
using UnityEngine;
using DG.Tweening;

[Serializable]
public class UIZoomFadeTransition : UITransitionBase
{
    private readonly RectTransform bodyTransform;
    private readonly Ease showEase;
    private readonly Ease hideEase;
    private readonly float fadeDurationFraction;

    public UIZoomFadeTransition(UIZoomFadeTransitionData data) : base(data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        if (data.BodyTransform == null)
            throw new ArgumentException("UIZoomFadeTransitionData requires a bodyTransform.");

        bodyTransform = data.BodyTransform;
        showEase = data.ShowEase;
        hideEase = data.HideEase;
        fadeDurationFraction = data.FadeDurationFraction;
    }

    public override void PlayShow(CanvasGroup canvasGroup, RectTransform rectTransform, Action onComplete = null)
    {
        float fadeDuration = transitionData.ShowDuration * fadeDurationFraction;
        float scaleDuration = transitionData.ShowDuration - fadeDuration;

        canvasGroup.DOKill();
        bodyTransform.DOKill();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        bodyTransform.localScale = Vector3.zero;

        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Append(canvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad));
        sequence.Append(bodyTransform.DOScale(Vector3.one, scaleDuration).SetEase(showEase));

        sequence.OnComplete(() =>
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            onComplete?.Invoke();
        });
    }

    public override void PlayHide(CanvasGroup canvasGroup, RectTransform rectTransform, Action onComplete = null)
    {
        float fadeDuration = transitionData.HideDuration * fadeDurationFraction;
        float scaleDuration = transitionData.HideDuration - fadeDuration;

        canvasGroup.DOKill();
        bodyTransform.DOKill();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Append(bodyTransform.DOScale(Vector3.zero, scaleDuration).SetEase(hideEase));
        sequence.Append(canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InQuad));

        sequence.OnComplete(() => onComplete?.Invoke());
    }
}