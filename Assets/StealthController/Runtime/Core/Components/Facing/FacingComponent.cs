using UnityEngine;

namespace StealthController.Core
{
    public class FacingComponent
    {
        private float yaw;
        private float yawVelocity;

        public float Yaw => yaw;
        public Quaternion Rotation => Quaternion.Euler(0f, yaw, 0f);
        public Vector3 Forward => Rotation * Vector3.forward;

        public void Reset(float startYaw)
        {
            yaw = startYaw;
            yawVelocity = 0f;
        }

        public void RotateTowards(Vector3 direction, float turnSmoothTime, float maxTurnSpeed, float deltaTime)
        {
            Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z);
            if (flatDirection.sqrMagnitude < StealthConstants.MIN_MOVE_DISTANCE) return;

            RotateTowardsYaw(Mathf.Atan2(flatDirection.x, flatDirection.z) * Mathf.Rad2Deg, turnSmoothTime, maxTurnSpeed, deltaTime);
        }

        public void RotateTowardsYaw(float targetYaw, float turnSmoothTime, float maxTurnSpeed, float deltaTime)
        {
            yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref yawVelocity, turnSmoothTime, maxTurnSpeed, deltaTime);
        }
    }
}
