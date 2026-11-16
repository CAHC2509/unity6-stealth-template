using UnityEngine;

namespace StealthController.Core
{
    public class LocomotionComponent
    {
        private readonly CapsuleMotorComponent motor;
        private readonly float acceleration;
        private readonly float deceleration;

        private Vector3 horizontalVelocity;
        private Vector3 pendingDisplacement;
        private float measuredSpeed;

        public Vector3 HorizontalVelocity => horizontalVelocity;
        public float MeasuredSpeed => measuredSpeed;

        public LocomotionComponent(CapsuleMotorComponent motor, float acceleration, float deceleration)
        {
            this.motor = motor;
            this.acceleration = acceleration;
            this.deceleration = deceleration;
        }

        public void Reset()
        {
            horizontalVelocity = Vector3.zero;
            pendingDisplacement = Vector3.zero;
            measuredSpeed = 0f;
        }

        public void AccelerateTowards(Vector3 targetVelocity, float deltaTime)
        {
            float rate = targetVelocity.sqrMagnitude >= horizontalVelocity.sqrMagnitude ? acceleration : deceleration;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, rate * deltaTime);
        }

        public void SetHorizontalVelocity(Vector3 velocity)
        {
            horizontalVelocity = velocity;
        }

        public void Stop()
        {
            horizontalVelocity = Vector3.zero;
        }

        public void AddDisplacement(Vector3 displacement)
        {
            pendingDisplacement += displacement;
        }

        public void Apply(float deltaTime)
        {
            Vector3 previousPosition = motor.Position;
            motor.Move(horizontalVelocity * deltaTime + pendingDisplacement, deltaTime);
            pendingDisplacement = Vector3.zero;

            Vector3 displacement = motor.Position - previousPosition;
            displacement.y = 0f;
            measuredSpeed = displacement.magnitude / deltaTime;
        }
    }
}
