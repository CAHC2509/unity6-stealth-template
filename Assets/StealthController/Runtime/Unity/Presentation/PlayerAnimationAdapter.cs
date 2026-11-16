using StealthController.Core;
using UnityEngine;

namespace StealthController.Unity
{
    public class PlayerAnimationAdapter : CharacterAnimationAdapterBase
    {
        private static readonly int WallLateralId = Animator.StringToHash(StealthUnityConstants.ANIMATOR_WALL_LATERAL_PARAMETER);
        private static readonly int StandToCrouchProgressId = Animator.StringToHash(StealthUnityConstants.ANIMATOR_STAND_TO_CROUCH_PROGRESS_PARAMETER);
        private static readonly int CrouchToStandProgressId = Animator.StringToHash(StealthUnityConstants.ANIMATOR_CROUCH_TO_STAND_PROGRESS_PARAMETER);
        private static readonly int CrouchToProneProgressId = Animator.StringToHash(StealthUnityConstants.ANIMATOR_CROUCH_TO_PRONE_PROGRESS_PARAMETER);
        private static readonly int ProneToCrouchProgressId = Animator.StringToHash(StealthUnityConstants.ANIMATOR_PRONE_TO_CROUCH_PROGRESS_PARAMETER);
        private static readonly int StandingState = Animator.StringToHash(StealthUnityConstants.ANIMATOR_STANDING_STATE);
        private static readonly int CrouchedState = Animator.StringToHash(StealthUnityConstants.ANIMATOR_CROUCHED_STATE);
        private static readonly int ProneState = Animator.StringToHash(StealthUnityConstants.ANIMATOR_PRONE_STATE);
        private static readonly int WallStandingState = Animator.StringToHash(StealthUnityConstants.ANIMATOR_WALL_STANDING_STATE);
        private static readonly int StandToCrouchState = Animator.StringToHash(StealthUnityConstants.ANIMATOR_STAND_TO_CROUCH_STATE);
        private static readonly int CrouchToStandState = Animator.StringToHash(StealthUnityConstants.ANIMATOR_CROUCH_TO_STAND_STATE);
        private static readonly int CrouchToProneState = Animator.StringToHash(StealthUnityConstants.ANIMATOR_CROUCH_TO_PRONE_STATE);
        private static readonly int ProneToCrouchState = Animator.StringToHash(StealthUnityConstants.ANIMATOR_PRONE_TO_CROUCH_STATE);
        private static readonly int TakedownState = Animator.StringToHash(StealthUnityConstants.ANIMATOR_TAKEDOWN_STATE);
        private static readonly int TakedownProgressId = Animator.StringToHash(StealthUnityConstants.ANIMATOR_TAKEDOWN_PROGRESS_PARAMETER);

        private PlayerPoseState currentPoseState;
        private bool hasPoseState;
        private bool isPlayingPostureTransition;
        private int activeTransitionProgressId;

        public void Snap(PlayerPose pose)
        {
            currentPoseState = pose.PoseState;
            hasPoseState = true;
            isPlayingPostureTransition = false;

            PlayImmediately(LoopStateFor(pose.PoseState));
            ResetSpeed();
            SetImmediate(WallLateralId, 0f);
        }

        public void Apply(PlayerPose pose, Vector3 horizontalVelocity, float measuredSpeed, float wallShuffleSpeed, float deltaTime)
        {
            if (!hasPoseState)
            {
                Snap(pose);
                return;
            }

            SetImmediate(TakedownProgressId, pose.ActionProgress);

            if (pose.PoseState != currentPoseState)
                EnterPoseState(pose.PoseState);

            if (isPlayingPostureTransition)
                SetImmediate(activeTransitionProgressId, pose.PostureTransitionProgress);

            if (isPlayingPostureTransition && !pose.IsInPostureTransition)
                FinishPostureTransition();

            SetSpeed(measuredSpeed, deltaTime);
            SetDamped(WallLateralId, ComputeWallLateral(pose, horizontalVelocity, wallShuffleSpeed), deltaTime);
        }

        private void EnterPoseState(PlayerPoseState nextPoseState)
        {
            bool isTeleporting = currentPoseState == PlayerPoseState.Takedown || nextPoseState == PlayerPoseState.Takedown;
            int transitionState = TransitionStateFor(currentPoseState, nextPoseState);
            currentPoseState = nextPoseState;
            isPlayingPostureTransition = transitionState != LoopStateFor(nextPoseState);
            activeTransitionProgressId = ProgressParameterFor(transitionState);

            if (isTeleporting)
            {
                PlayImmediately(transitionState);
                return;
            }

            CrossFadeTo(transitionState);
        }

        private void FinishPostureTransition()
        {
            isPlayingPostureTransition = false;
            CrossFadeTo(LoopStateFor(currentPoseState));
        }

        private static float ComputeWallLateral(PlayerPose pose, Vector3 horizontalVelocity, float wallShuffleSpeed)
        {
            if (!pose.IsOnWall || wallShuffleSpeed <= 0f) return 0f;

            Vector3 right = pose.Rotation * Vector3.right;
            return Mathf.Clamp(Vector3.Dot(horizontalVelocity, right) / wallShuffleSpeed, -1f, 1f);
        }

        private static int TransitionStateFor(PlayerPoseState from, PlayerPoseState to)
        {
            if (from == PlayerPoseState.Standing && to == PlayerPoseState.Crouched) return StandToCrouchState;
            if (from == PlayerPoseState.Crouched && to == PlayerPoseState.Standing) return CrouchToStandState;
            if (from == PlayerPoseState.Crouched && to == PlayerPoseState.Crawling) return CrouchToProneState;
            if (from == PlayerPoseState.Crawling && to != PlayerPoseState.Crawling) return ProneToCrouchState;

            return LoopStateFor(to);
        }

        private static int ProgressParameterFor(int transitionState)
        {
            if (transitionState == StandToCrouchState) return StandToCrouchProgressId;
            if (transitionState == CrouchToStandState) return CrouchToStandProgressId;
            if (transitionState == CrouchToProneState) return CrouchToProneProgressId;
            if (transitionState == ProneToCrouchState) return ProneToCrouchProgressId;

            return 0;
        }

        private static int LoopStateFor(PlayerPoseState poseState)
        {
            switch (poseState)
            {
                case PlayerPoseState.Crouched: return CrouchedState;
                case PlayerPoseState.Crawling: return ProneState;
                case PlayerPoseState.WallStanding: return WallStandingState;
                case PlayerPoseState.Takedown: return TakedownState;
                default: return StandingState;
            }
        }
    }
}
