using StealthController.Core;
using UnityEngine;

namespace StealthController.Unity.UI
{
    public class PostureHudController : StealthBehaviourBase
    {
        [SerializeField] private PostureHudView view;

        private StealthSessionManager session;
        private InputBindingDisplayAdapter bindingDisplay;
        private PlayerPoseState displayedPosture;

        public override void Initialize(params object[] parameters)
        {
            session = parameters.Length > 0 ? parameters[0] as StealthSessionManager : null;
            bindingDisplay = parameters.Length > 1 ? parameters[1] as InputBindingDisplayAdapter : null;
            LocalizationAdapter localization = parameters.Length > 2 ? parameters[2] as LocalizationAdapter : null;

            bool hasDependencies = RequireDependency(view, nameof(view))
                & RequireDependency(session, nameof(session))
                & RequireService(bindingDisplay, nameof(bindingDisplay))
                & RequireService(localization, nameof(localization));

            if (!hasDependencies) return;

            displayedPosture = DisplayedPostureFor(session.PlayerPoseState);
            view.Initialize(localization);

            base.Initialize(parameters);

            Refresh();
            view.ShowInstant();
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

            session.PlayerPoseStateChanged += OnPlayerPoseStateChanged;
            bindingDisplay.DeviceChanged += Refresh;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();

            bindingDisplay.DeviceChanged -= Refresh;
            session.PlayerPoseStateChanged -= OnPlayerPoseStateChanged;
        }

        private void OnPlayerPoseStateChanged(PlayerPoseState poseState)
        {
            PlayerPoseState posture = DisplayedPostureFor(poseState);
            if (posture == displayedPosture) return;

            displayedPosture = posture;
            Refresh();
        }

        private void Refresh()
        {
            string sprintKey = bindingDisplay.ShortLabelFor(StealthInputAction.Sprint);
            string standKey = bindingDisplay.ShortLabelFor(StealthInputAction.Stand);
            string crouchKey = bindingDisplay.ShortLabelFor(StealthInputAction.Crouch);

            view.ShowPosture(displayedPosture, sprintKey, standKey, crouchKey);
        }

        private static PlayerPoseState DisplayedPostureFor(PlayerPoseState poseState)
        {
            return poseState == PlayerPoseState.WallStanding || poseState == PlayerPoseState.Takedown ? PlayerPoseState.Standing : poseState;
        }
    }
}
