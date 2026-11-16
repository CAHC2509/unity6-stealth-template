using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectSelectionFollower : MonoBehaviour
{
    [Header("Focus Settings")]
    [SerializeField, Min(0f)] private float focusDuration = 0.2f;
    [SerializeField] private Ease focusEase = Ease.OutCubic;

    private ScrollRectSelectableItemView[] items;
    private ScrollRect scrollRect;
    private RectTransform viewport;
    private RectTransform content;

    private Tween focusTween;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;
        viewport = scrollRect.viewport;
    }

    private void Start()
    {
        items = GetComponentsInChildren<ScrollRectSelectableItemView>();

        foreach (ScrollRectSelectableItemView selectableItem in items)
            selectableItem.Initialize(this);
    }

    public void Focus(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        if (contentHeight <= viewportHeight)
            return;

        Vector2 targetPosition = content.InverseTransformPoint(target.position);
        float targetCenterY = Mathf.Abs(targetPosition.y);

        targetCenterY -= viewportHeight * 0.5f;
        targetCenterY += target.rect.height * 0.5f;

        float normalizedPosition = 1f - Mathf.Clamp01(
            targetCenterY / (contentHeight - viewportHeight));

        focusTween?.Kill();

        focusTween = DOTween.To(
                () => scrollRect.verticalNormalizedPosition,
                value => scrollRect.verticalNormalizedPosition = value,
                normalizedPosition,
                focusDuration)
            .SetEase(focusEase)
            .SetUpdate(true);
    }

    private void OnDestroy()
    {
        focusTween?.Kill();
    }
}