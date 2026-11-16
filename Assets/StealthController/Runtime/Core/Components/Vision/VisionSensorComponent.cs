using UnityEngine;

namespace StealthController.Core
{
    public class VisionSensorComponent
    {
        private static readonly float[] BodySampleHeightRatios =
        {
            StealthConstants.VISION_HEAD_HEIGHT_RATIO,
            StealthConstants.VISION_CHEST_HEIGHT_RATIO,
            StealthConstants.VISION_FEET_HEIGHT_RATIO
        };

        private readonly VisionConfig config;
        private readonly ICollisionWorld collisionWorld;

        private bool canSeePlayer;
        private bool ignoresPlayer;
        private bool hasDetectedPlayer;
        private float sightElapsed;
        private Vector3 lastSeenPosition;

        public VisionConfig Config => config;
        public bool CanSeePlayer => canSeePlayer;
        public bool HasDetectedPlayer => hasDetectedPlayer;
        public Vector3 LastSeenPosition => lastSeenPosition;
        public bool IgnoresPlayer => ignoresPlayer;

        public VisionSensorComponent(VisionConfig config, ICollisionWorld collisionWorld)
        {
            this.config = config;
            this.collisionWorld = collisionWorld;
        }

        public void Reset()
        {
            canSeePlayer = false;
            hasDetectedPlayer = false;
            sightElapsed = 0f;
        }

        public void Tick(float deltaTime, Vector3 eyePosition, Vector3 forward, PlayerPose player)
        {
            canSeePlayer = !ignoresPlayer && HasLineOfSight(eyePosition, forward, player);

            if (!canSeePlayer)
            {
                sightElapsed = 0f;
                hasDetectedPlayer = false;
                return;
            }

            lastSeenPosition = player.Position;
            sightElapsed += deltaTime;
            hasDetectedPlayer = sightElapsed >= config.ConfirmDuration;
        }

        public void SetIgnoresPlayer(bool value)
        {
            ignoresPlayer = value;

            if (ignoresPlayer)
                Reset();
        }

        public bool IsInsideCone(Vector3 eyePosition, Vector3 forward, Vector3 point)
        {
            Vector3 toPoint = point - eyePosition;
            Vector3 flatToPoint = new Vector3(toPoint.x, 0f, toPoint.z);

            if (flatToPoint.sqrMagnitude > config.Range * config.Range) return false;
            if (flatToPoint.sqrMagnitude < config.MinRange * config.MinRange) return false;
            if (flatToPoint.sqrMagnitude < StealthConstants.MIN_MOVE_DISTANCE) return true;

            Vector3 flatForward = new Vector3(forward.x, 0f, forward.z);
            return Vector3.Angle(flatForward, flatToPoint) <= config.HalfAngle;
        }

        private bool HasLineOfSight(Vector3 eyePosition, Vector3 forward, PlayerPose player)
        {
            for (int i = 0; i < BodySampleHeightRatios.Length; i++)
            {
                Vector3 samplePoint = player.Position + Vector3.up * (player.CapsuleHeight * BodySampleHeightRatios[i]);

                if (!IsInsideCone(eyePosition, forward, samplePoint)) continue;
                if (IsUnobstructed(eyePosition, samplePoint)) return true;
            }

            return false;
        }

        private bool IsUnobstructed(Vector3 eyePosition, Vector3 samplePoint)
        {
            Vector3 toPoint = samplePoint - eyePosition;
            float distance = toPoint.magnitude;

            if (distance < StealthConstants.MIN_MOVE_DISTANCE) return true;

            float rayLength = distance - StealthConstants.VISION_LINE_OF_SIGHT_TOLERANCE;
            return !collisionWorld.Raycast(eyePosition, toPoint / distance, rayLength, out CollisionHit hit);
        }
    }
}
