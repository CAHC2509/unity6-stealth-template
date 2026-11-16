using StealthController.Core;
using UnityEngine;

namespace StealthController.Unity
{
    public class GuardAnimationAdapter : CharacterAnimationAdapterBase
    {
        private static readonly int LocomotionState = Animator.StringToHash(StealthUnityConstants.ANIMATOR_LOCOMOTION_STATE);
        private static readonly int SearchState = Animator.StringToHash(StealthUnityConstants.ANIMATOR_SEARCH_STATE);
        private static readonly int TakenDownState = Animator.StringToHash(StealthUnityConstants.ANIMATOR_TAKEN_DOWN_STATE);
        private static readonly int TakedownProgressId = Animator.StringToHash(StealthUnityConstants.ANIMATOR_TAKEDOWN_PROGRESS_PARAMETER);

        private bool isSearching;
        private bool isTakenDown;
        private bool hasPose;

        public void Snap(GuardPose pose)
        {
            isSearching = pose.IsSearching;
            isTakenDown = pose.IsTakenDown;
            hasPose = true;

            SetImmediate(TakedownProgressId, pose.ActionProgress);
            PlayImmediately(StateFor(pose));
            ResetSpeed();
        }

        public void Apply(GuardPose pose, float deltaTime)
        {
            if (!hasPose)
            {
                Snap(pose);
                return;
            }

            SetImmediate(TakedownProgressId, pose.ActionProgress);

            if (pose.IsSearching != isSearching || pose.IsTakenDown != isTakenDown)
            {
                isSearching = pose.IsSearching;
                isTakenDown = pose.IsTakenDown;
                CrossFadeTo(StateFor(pose));
            }

            SetSpeed(pose.Speed, deltaTime);
        }

        private static int StateFor(GuardPose pose)
        {
            if (pose.IsTakenDown) return TakenDownState;

            return pose.IsSearching ? SearchState : LocomotionState;
        }
    }
}
