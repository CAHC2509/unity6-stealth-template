using UnityEngine;

namespace StealthController.Core
{
    public class SurveillanceCameraLogic
    {
        private readonly SurveillanceCameraConfig config;
        private readonly VisionSensorComponent vision;
        private readonly SweepComponent sweep;
        private readonly AlertEmitterComponent alertEmitter;
        private readonly Vector3 eyePosition;
        private readonly Vector3 sightOrigin;
        private readonly float baseYaw;

        public SurveillanceCameraConfig Config => config;
        public VisionSensorComponent Vision => vision;
        public SweepComponent Sweep => sweep;
        public AlertEmitterComponent AlertEmitter => alertEmitter;
        public Vector3 EyePosition => eyePosition;
        public Vector3 SightOrigin => sightOrigin;
        public float Yaw => baseYaw + sweep.Offset;
        public Vector3 Forward => Quaternion.Euler(0f, Yaw, 0f) * Vector3.forward;
        public bool IsAlerting => vision.HasDetectedPlayer;

        public SurveillanceCameraLogic(SurveillanceCameraConfig config, Vector3 eyePosition, Vector3 floorPosition, float baseYaw, ICollisionWorld collisionWorld)
        {
            this.config = config;
            this.eyePosition = eyePosition;
            sightOrigin = new Vector3(eyePosition.x, floorPosition.y + config.SightHeight, eyePosition.z);
            this.baseYaw = baseYaw;

            vision = new VisionSensorComponent(config.Vision, collisionWorld);
            sweep = new SweepComponent(config.SweepHalfAngle, config.SweepSpeed, config.SweepPauseDuration);
            alertEmitter = new AlertEmitterComponent(config.AlertCooldown);

            Reset();
        }

        public void Reset()
        {
            sweep.Reset();
            alertEmitter.Reset();
            vision.Reset();
        }

        public void Tick(float deltaTime, PlayerPose player)
        {
            alertEmitter.Tick(deltaTime);
            vision.Tick(deltaTime, sightOrigin, Forward, player);

            if (vision.HasDetectedPlayer)
            {
                alertEmitter.Raise(vision.LastSeenPosition);
                return;
            }

            if (vision.CanSeePlayer) return;

            sweep.Tick(deltaTime);
        }

        public bool TryConsumeAlert(out Vector3 knownPosition)
        {
            return alertEmitter.TryConsume(out knownPosition);
        }
    }
}
