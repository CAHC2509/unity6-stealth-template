using UnityEngine;

namespace StealthController.Core
{
    public class CameraOcclusionLogic
    {
        private readonly ICollisionWorld occluders;
        private readonly float targetMinHeight;
        private readonly float cutoutRadius;
        private readonly float openSpeed;
        private readonly float closeSpeed;

        private float openness;
        private Vector3 cutoutTarget;

        public Vector3 CutoutTarget => cutoutTarget;
        public float CutoutRadius => cutoutRadius * Mathf.SmoothStep(0f, 1f, openness);

        public CameraOcclusionLogic(ICollisionWorld occluders, float targetMinHeight, float cutoutRadius, float openSpeed, float closeSpeed)
        {
            this.occluders = occluders;
            this.targetMinHeight = targetMinHeight;
            this.cutoutRadius = cutoutRadius;
            this.openSpeed = openSpeed;
            this.closeSpeed = closeSpeed;
        }

        public void Tick(float deltaTime, Vector3 cameraPosition, PlayerPose pose)
        {
            Vector3 lowerTarget = pose.Position + Vector3.up * Mathf.Max(pose.CapsuleHeight * 0.5f, targetMinHeight);
            Vector3 upperTarget = pose.Position + Vector3.up * Mathf.Max(pose.CapsuleHeight, targetMinHeight);

            bool isOccluded = IsBlocked(cameraPosition, lowerTarget) || IsBlocked(cameraPosition, upperTarget);
            float targetOpenness = isOccluded ? 1f : 0f;
            float speed = isOccluded ? openSpeed : closeSpeed;

            openness = deltaTime > 0f ? Mathf.MoveTowards(openness, targetOpenness, speed * deltaTime) : targetOpenness;
            cutoutTarget = lowerTarget;
        }

        public void Reset()
        {
            openness = 0f;
        }

        private bool IsBlocked(Vector3 origin, Vector3 target)
        {
            Vector3 offset = target - origin;
            float distance = offset.magnitude;
            if (distance < StealthConstants.MIN_MOVE_DISTANCE) return false;

            return occluders.Raycast(origin, offset / distance, distance, out CollisionHit _);
        }
    }
}
