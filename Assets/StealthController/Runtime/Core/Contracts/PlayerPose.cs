using UnityEngine;

namespace StealthController.Core
{
    public readonly struct PlayerPose
    {
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public PlayerPoseState PoseState { get; }
        public float NormalizedSpeed { get; }
        public float PostureTransitionProgress { get; }
        public float CapsuleRadius { get; }
        public float CapsuleHeight { get; }
        public Vector3 WallNormal { get; }
        public float ActionProgress { get; }

        public bool IsInPostureTransition => PostureTransitionProgress < 1f;
        public bool IsOnWall => PoseState == PlayerPoseState.WallStanding;

        public PlayerPose(
            Vector3 position,
            Quaternion rotation,
            PlayerPoseState poseState,
            float normalizedSpeed,
            float postureTransitionProgress,
            float capsuleRadius,
            float capsuleHeight,
            Vector3 wallNormal)
            : this(position, rotation, poseState, normalizedSpeed, postureTransitionProgress, capsuleRadius, capsuleHeight, wallNormal, 0f) { }

        public PlayerPose(
            Vector3 position,
            Quaternion rotation,
            PlayerPoseState poseState,
            float normalizedSpeed,
            float postureTransitionProgress,
            float capsuleRadius,
            float capsuleHeight,
            Vector3 wallNormal,
            float actionProgress)
        {
            Position = position;
            Rotation = rotation;
            PoseState = poseState;
            NormalizedSpeed = normalizedSpeed;
            PostureTransitionProgress = postureTransitionProgress;
            CapsuleRadius = capsuleRadius;
            CapsuleHeight = capsuleHeight;
            WallNormal = wallNormal;
            ActionProgress = actionProgress;
        }
    }
}
