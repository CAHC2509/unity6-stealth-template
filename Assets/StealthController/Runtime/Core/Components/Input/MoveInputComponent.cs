using UnityEngine;

namespace StealthController.Core
{
    public class MoveInputComponent
    {
        private readonly PlayerMovementConfig config;

        private bool wasRunRequested;
        private bool hasRunRequestStarted;

        public MoveInputComponent(PlayerMovementConfig config)
        {
            this.config = config;
        }

        public void Reset()
        {
            wasRunRequested = false;
            hasRunRequestStarted = false;
        }

        public void Update(PlayerInputFrame input)
        {
            bool isRunRequested = input.SprintHeld && HasMoveInput(input);
            hasRunRequestStarted = isRunRequested && !wasRunRequested;
            wasRunRequested = isRunRequested;
        }

        public bool HasMoveInput(PlayerInputFrame input) => ComputeInputMagnitude(input) > 0f;
        public bool WantsToStandUp(PlayerInputFrame input) => input.StandPressed || hasRunRequestStarted;

        public float ComputeInputMagnitude(PlayerInputFrame input)
        {
            float magnitude = Mathf.Min(input.Move.magnitude, StealthConstants.FULL_STICK_TILT);
            return magnitude < config.StickDeadZone ? 0f : magnitude;
        }

        public float ComputeStandingTargetSpeed(PlayerInputFrame input)
        {
            float magnitude = ComputeInputMagnitude(input);

            if (magnitude <= 0f) return 0f;
            if (input.SprintHeld) return config.RunSpeed;
            if (!input.IsMoveAnalog) return config.MaxWalkSpeed;
            if (magnitude >= config.RunInputThreshold) return config.RunSpeed;

            float walkBlend = Mathf.InverseLerp(config.StickDeadZone, config.RunInputThreshold, magnitude);
            return Mathf.Lerp(config.MinWalkSpeed, config.MaxWalkSpeed, walkBlend);
        }

        public float ComputeCrouchedTargetSpeed(PlayerInputFrame input)
        {
            float magnitude = ComputeInputMagnitude(input);

            if (magnitude <= 0f) return 0f;
            if (!input.IsMoveAnalog) return config.CrouchWalkSpeed;

            float tiltBlend = Mathf.InverseLerp(config.StickDeadZone, StealthConstants.FULL_STICK_TILT, magnitude);
            return Mathf.Lerp(config.CrouchWalkMinSpeed, config.CrouchWalkSpeed, tiltBlend);
        }

        public Vector3 ComputeWorldDirection(Vector2 move, float referenceYaw)
        {
            Vector3 localDirection = new Vector3(move.x, 0f, move.y);
            if (localDirection.sqrMagnitude < StealthConstants.MIN_MOVE_DISTANCE) return Vector3.zero;

            return Quaternion.Euler(0f, referenceYaw, 0f) * localDirection.normalized;
        }
    }
}
