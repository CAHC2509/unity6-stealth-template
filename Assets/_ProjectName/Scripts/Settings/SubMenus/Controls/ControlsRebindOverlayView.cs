using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using DG.Tweening;

public class ControlsRebindOverlayView : UIViewBase
{
    [Header("Transition settings")]
    [SerializeField] private UIZoomFadeTransitionData zoomFadeTransitionData;

    [Header("Texts")]
    [SerializeField] private LocalizeStringEvent promptText;
    [SerializeField] private LocalizeStringEvent infoText;
    [SerializeField] private LocalizeStringEvent expectedDeviceText;
    [SerializeField] private LocalizedString waitingForRebindText;
    [SerializeField] private LocalizedString keyAlreadyUsedText;
    [SerializeField] private LocalizedString invalidDeviceText;

    [Header("Device Localizations")]
    [SerializeField] private DeviceLocalizationMapping[] deviceLocalizations;

    [Header("Composite Localizations")]
    [SerializeField] private CompositePartLocalizationMapping[] compositePartLocalizations;

    [Header("Timeout")]
    [SerializeField] private Image timeoutFillImage;
    [SerializeField, Min(0f)] private float timeoutDuration = 10f;

    [Header("Error Animation Settings")]
    [SerializeField, Min(1)] private int errorAnimationLoops = 2;
    [SerializeField, Min(0f)] private float errorAnimationFadeDuration = 0.15f;
    [SerializeField, Range(0f, 1f)] private float errorAnimationMinAlpha = 0.35f;
    [SerializeField] private Ease errorAnimationEase = Ease.Linear;

    public float TimeoutDuration => timeoutDuration;

    private Tween timeoutTween;
    private Sequence errorSequence;
    private CanvasGroup infoTextCanvasGroup;

    protected override bool RequiresSelection => false;

    protected override UITransitionBase CreateTransition() => new UIZoomFadeTransition(zoomFadeTransitionData);

    public override void Initialize(params object[] parameters)
    {
        RequireDependency(promptText, nameof(promptText));
        RequireDependency(infoText, nameof(infoText));
        RequireDependency(expectedDeviceText, nameof(expectedDeviceText));

        if (infoText != null && !infoText.TryGetComponent(out infoTextCanvasGroup))
            infoTextCanvasGroup = infoText.gameObject.AddComponent<CanvasGroup>();

        base.Initialize(parameters);
    }

    public override void Conclude()
    {
        StopTimeout();
        errorSequence?.Kill();

        base.Conclude();
    }

    public void ShowWaitingForRebind(LocalizedString actionName, string compositePart = "")
    {
        promptText.StringReference = waitingForRebindText;
        string actionText = actionName.GetLocalizedString();

        if (!string.IsNullOrEmpty(compositePart))
        {
            LocalizedString localizedPart = GetLocalizedCompositePartName(compositePart);
            if (localizedPart != null)
                actionText = $"{actionText} - {localizedPart.GetLocalizedString()}";
        }

        promptText.StringReference.Arguments = new object[] { actionText };
        promptText.RefreshString();
        ClearInfo();
    }

    public void SetExpectedDevice(DeviceType deviceType)
    {
        LocalizedString localizedDeviceName = GetLocalizedDeviceName(deviceType);
        expectedDeviceText.StringReference.Arguments = localizedDeviceName != null
            ? new object[] { localizedDeviceName.GetLocalizedString() }
            : new object[] { deviceType.ToString() };

        expectedDeviceText.RefreshString();
    }

    public void ShowKeyAlreadyUsed()
    {
        infoText.gameObject.SetActive(true);
        infoText.StringReference = keyAlreadyUsedText;
        infoText.RefreshString();
    }

    public void ShowInvalidDevice()
    {
        infoText.gameObject.SetActive(true);
        infoText.StringReference = invalidDeviceText;
        infoText.RefreshString();
    }

    public void ClearInfo()
    {
        infoText.StringReference.Clear();
        infoText.RefreshString();
        infoText.gameObject.SetActive(false);
    }

    public void StartTimeout(float duration, Action onTimeout)
    {
        StopTimeout();

        if (timeoutFillImage == null)
            return;

        timeoutFillImage.fillAmount = 1f;
        timeoutTween = DOTween.To(
            () => timeoutFillImage.fillAmount,
            value => timeoutFillImage.fillAmount = value,
            0f,
            duration
        ).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() => onTimeout?.Invoke());
    }

    public void PauseTimeout()
    {
        if (timeoutTween != null && timeoutTween.IsPlaying())
            timeoutTween.Pause();
    }

    public void StopTimeout()
    {
        if (timeoutTween != null)
        {
            timeoutTween.Kill();
            timeoutTween = null;
        }
    }

    public void PlayErrorAnimation(Action onComplete)
    {
        if (infoTextCanvasGroup == null)
        {
            onComplete?.Invoke();
            return;
        }

        errorSequence?.Kill();
        infoTextCanvasGroup.alpha = 1f;

        errorSequence = DOTween.Sequence().SetUpdate(true);
        for (int i = 0; i < errorAnimationLoops; i++)
        {
            errorSequence.Append(infoTextCanvasGroup.DOFade(errorAnimationMinAlpha, errorAnimationFadeDuration).SetEase(errorAnimationEase));
            errorSequence.Append(infoTextCanvasGroup.DOFade(1f, errorAnimationFadeDuration).SetEase(errorAnimationEase));
        }

        errorSequence.OnComplete(() =>
        {
            infoTextCanvasGroup.alpha = 1f;
            errorSequence = null;
            onComplete?.Invoke();
        });
    }

    private LocalizedString GetLocalizedDeviceName(DeviceType deviceType)
    {
        foreach (DeviceLocalizationMapping mapping in deviceLocalizations)
            if (mapping.DeviceType == deviceType)
                return mapping.LocalizedName;

        Debug.LogWarning($"Can't find a name for device type: {deviceType}", this);
        return null;
    }

    private LocalizedString GetLocalizedCompositePartName(string partName)
    {
        foreach (CompositePartLocalizationMapping mapping in compositePartLocalizations)
            if (mapping.PartName.Equals(partName, StringComparison.OrdinalIgnoreCase))
                return mapping.LocalizedName;

        Debug.LogWarning($"Can't find a name for composite part: {partName}", this);
        return null;
    }
}

[Serializable]
public struct DeviceLocalizationMapping
{
    [SerializeField] private DeviceType deviceType;
    [SerializeField] private LocalizedString localizedName;

    public DeviceType DeviceType => deviceType;
    public LocalizedString LocalizedName => localizedName;
}

[Serializable]
public struct CompositePartLocalizationMapping
{
    [SerializeField] private string partName;
    [SerializeField] private LocalizedString localizedName;

    public string PartName => partName;
    public LocalizedString LocalizedName => localizedName;
}