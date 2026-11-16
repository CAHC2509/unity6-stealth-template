using UnityEngine;

namespace StealthController.Unity.UI
{
    public class TakedownPromptController : StealthBehaviourBase
    {
        [SerializeField] private TakedownPromptView view;

        [Header("Placement")]
        [SerializeField] private Camera worldCamera;
        [SerializeField, Min(0f), Tooltip("Height above the guard's eyes where the key floats.")] private float heightAboveHead = StealthUnityConstants.TAKEDOWN_PROMPT_HEIGHT;

        private StealthSessionManager session;
        private InputBindingDisplayAdapter bindingDisplay;

        public override void Initialize(params object[] parameters)
        {
            session = parameters.Length > 0 ? parameters[0] as StealthSessionManager : null;
            bindingDisplay = parameters.Length > 1 ? parameters[1] as InputBindingDisplayAdapter : null;
            LocalizationAdapter localization = parameters.Length > 2 ? parameters[2] as LocalizationAdapter : null;

            bool hasDependencies = RequireDependency(view, nameof(view))
                & RequireDependency(worldCamera, nameof(worldCamera))
                & RequireDependency(session, nameof(session))
                & RequireService(bindingDisplay, nameof(bindingDisplay))
                & RequireService(localization, nameof(localization));

            if (!hasDependencies) return;

            view.Initialize(localization);

            base.Initialize(parameters);

            RefreshKey();
            view.HideInstant();
        }

        public override void Conclude()
        {
            base.Conclude();

            if (view != null)
                view.Conclude();
        }

        protected override void AddListeners()
        {
            base.AddListeners();

            session.TakedownAvailabilityChanged += OnTakedownAvailabilityChanged;
            bindingDisplay.DeviceChanged += RefreshKey;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();

            bindingDisplay.DeviceChanged -= RefreshKey;
            session.TakedownAvailabilityChanged -= OnTakedownAvailabilityChanged;
        }

        private void LateUpdate()
        {
            FollowTarget();
        }

        private void FollowTarget()
        {
            if (!IsInitialized || !session.HasTakedownTarget) return;

            Vector3 anchor = session.TakedownTargetHeadPosition + Vector3.up * heightAboveHead;
            view.PlaceAtScreenPoint(worldCamera.WorldToScreenPoint(anchor));
        }

        private void OnTakedownAvailabilityChanged(bool isAvailable)
        {
            if (isAvailable)
            {
                FollowTarget();
                view.Show();
                return;
            }

            view.Hide();
        }

        private void RefreshKey()
        {
            view.SetKey(bindingDisplay.ShortLabelFor(StealthInputAction.Takedown));
        }
    }
}
