using System;
using UnityEngine;

namespace StealthController.Unity.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class FadeViewBase : StealthBehaviourBase
    {
        [Header("Fade")]
        [SerializeField, Min(0f)] private float showDuration = StealthUnityConstants.UI_SHOW_DURATION;
        [SerializeField, Min(0f)] private float hideDuration = StealthUnityConstants.UI_HIDE_DURATION;
        [SerializeField, Tooltip("Lets the view receive clicks and selection while visible.")] private bool isInteractable;

        private CanvasGroup canvasGroup;
        private float targetAlpha;
        private Action pendingCompletion;

        public bool IsVisible => targetAlpha > 0f;

        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Show(Action onComplete = null)
        {
            FadeTo(1f, onComplete);
        }

        public void Hide(Action onComplete = null)
        {
            FadeTo(0f, onComplete);
        }

        public void ShowInstant()
        {
            SetAlphaInstant(1f);
        }

        public void HideInstant()
        {
            SetAlphaInstant(0f);
        }

        private void Update()
        {
            if (Mathf.Approximately(canvasGroup.alpha, targetAlpha)) return;

            float duration = targetAlpha > canvasGroup.alpha ? showDuration : hideDuration;
            float step = duration > 0f ? Time.unscaledDeltaTime / duration : 1f;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, step);

            if (Mathf.Approximately(canvasGroup.alpha, targetAlpha))
                Complete();
        }

        private void FadeTo(float alpha, Action onComplete)
        {
            pendingCompletion = onComplete;
            targetAlpha = alpha;
            ApplyInteraction();

            if (Mathf.Approximately(canvasGroup.alpha, targetAlpha))
                Complete();
        }

        private void SetAlphaInstant(float alpha)
        {
            pendingCompletion = null;
            targetAlpha = alpha;
            canvasGroup.alpha = alpha;
            ApplyInteraction();
        }

        private void ApplyInteraction()
        {
            bool canInteract = isInteractable && targetAlpha > 0f;
            canvasGroup.interactable = canInteract;
            canvasGroup.blocksRaycasts = canInteract;
        }

        private void Complete()
        {
            canvasGroup.alpha = targetAlpha;

            Action completion = pendingCompletion;
            pendingCompletion = null;
            completion?.Invoke();
        }
    }
}
