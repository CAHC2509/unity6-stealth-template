using UnityEngine;

namespace StealthController.Core
{
    public class CameraRigLogic
    {
        private readonly CameraRigConfig config;

        private Vector3 followPivot;
        private Vector3 followVelocity;

        public CameraFraming Framing => new CameraFraming(
            followPivot,
            config.Framing.Pitch,
            config.Framing.Yaw,
            config.Framing.Distance,
            config.Framing.FieldOfView);

        public float InputReferenceYaw => config.Framing.Yaw;

        public CameraRigLogic(CameraRigConfig config)
        {
            this.config = config;
        }

        public void Snap(PlayerPose pose)
        {
            followPivot = ComputeTargetPivot(pose);
            followVelocity = Vector3.zero;
        }

        public void Tick(float deltaTime, PlayerPose pose)
        {
            if (deltaTime <= 0f) return;

            float followSmoothTime = Mathf.Max(config.FollowSmoothTime, StealthConstants.CAMERA_MIN_SMOOTH_TIME);
            followPivot = Vector3.SmoothDamp(followPivot, ComputeTargetPivot(pose), ref followVelocity, followSmoothTime, Mathf.Infinity, deltaTime);
        }

        private Vector3 ComputeTargetPivot(PlayerPose pose)
        {
            float lookAhead = pose.IsOnWall ? 0f : config.LookAheadDistance * Mathf.Clamp01(pose.NormalizedSpeed);
            Vector3 forward = pose.Rotation * Vector3.forward;
            return pose.Position + Vector3.up * config.PivotHeight + forward * lookAhead;
        }
    }
}
