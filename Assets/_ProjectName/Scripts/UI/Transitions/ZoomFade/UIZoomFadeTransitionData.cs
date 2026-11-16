using System;
using UnityEngine;
using DG.Tweening;

[Serializable]
public class UIZoomFadeTransitionData : UITransitionDataBase
{
    [SerializeField] private RectTransform bodyTransform;
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InBack;
    [SerializeField, Range(0f, 1f)] private float fadeDurationFraction = 0.35f;

    public RectTransform BodyTransform => bodyTransform;
    public Ease ShowEase => showEase;
    public Ease HideEase => hideEase;
    public float FadeDurationFraction => fadeDurationFraction;
}
