using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LoadingEffectsView : UIViewBase
{
    [Header("Transition settings")]
    [SerializeField] private UIFadeTransitionData fadeTransitionData;

    [Header("Fill Animation")]
    [SerializeField] private Image loadingFillImage;
    [SerializeField, Range(0f, 1f)] private float minFill = 0.15f;
    [SerializeField, Range(0f, 1f)] private float maxFill = 0.85f;
    [SerializeField, Min(0f)] private float fillDuration = 0.6f;

    [Header("Rotation Animation")]
    [SerializeField, Min(0f)] private float rotationDuration = 1.2f;

    private Tween rotationTween;
    private Tween fillTween;

    protected override bool RequiresSelection => false;

    protected override UITransitionBase CreateTransition() => new UIFadeTransition(fadeTransitionData);

    public override void Initialize(params object[] parameters)
    {
        RequireDependency(loadingFillImage, nameof(loadingFillImage));

        base.Initialize(parameters);
    }

    protected override void OnViewShowStarted()
    {
        base.OnViewShowStarted();

        StartLoadingAnimation();
    }

    protected override void OnViewClosed()
    {
        base.OnViewClosed();

        StopLoadingAnimation();
    }

    private void StartLoadingAnimation()
    {
        StopLoadingAnimation();

        loadingFillImage.fillAmount = minFill;
        loadingFillImage.rectTransform.rotation = Quaternion.identity;

        fillTween = DOTween.To(
                () => loadingFillImage.fillAmount,
                value => loadingFillImage.fillAmount = value,
                maxFill,
                fillDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        rotationTween = loadingFillImage.rectTransform
            .DORotate(
                new Vector3(0f, 0f, -Constants.FULL_ROTATION_DEGREES),
                rotationDuration,
                RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    private void StopLoadingAnimation()
    {
        fillTween?.Kill();
        rotationTween?.Kill();

        fillTween = null;
        rotationTween = null;
    }
}