using UnityEngine;

namespace StealthController.Unity.UI
{
    public class TakedownPromptView : FadeViewBase
    {
        [Header("Elements")]
        [SerializeField] private ActionPromptView keyPrompt;

        private RectTransform rectTransform;
        private RectTransform parentRect;

        protected override void Awake()
        {
            base.Awake();

            rectTransform = (RectTransform)transform;
            parentRect = rectTransform.parent as RectTransform;
        }

        public override void Initialize(params object[] parameters)
        {
            if (!RequireDependency(keyPrompt, nameof(keyPrompt))) return;

            base.Initialize(parameters);

            keyPrompt.Initialize(parameters);
        }

        public override void Conclude()
        {
            if (keyPrompt != null)
                keyPrompt.Conclude();

            base.Conclude();
        }

        public void SetKey(string keyLabel)
        {
            keyPrompt.Show(keyLabel);
        }

        public void PlaceAtScreenPoint(Vector2 screenPoint)
        {
            if (parentRect == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, null, out Vector2 localPoint))
                rectTransform.anchoredPosition = localPoint;
        }
    }
}
