using System;
using UnityEngine;

namespace StealthController.Core
{
    [Serializable]
    public class SurveillanceCameraConfig
    {
        [Header("Sweep")]
        [SerializeField, BasicSetting, Range(0f, StealthConstants.HALF_TURN_DEGREES), Tooltip("How far the camera turns to each side of the direction it was placed in, in degrees.")] private float sweepHalfAngle = StealthConstants.SURVEILLANCE_SWEEP_HALF_ANGLE;
        [SerializeField, BasicSetting, Min(0f), Tooltip("Turning speed of the sweep, in degrees per second.")] private float sweepSpeed = StealthConstants.SURVEILLANCE_SWEEP_SPEED;
        [SerializeField, BasicSetting, Min(0f), Tooltip("Seconds the camera waits at each end of the sweep.")] private float sweepPauseDuration = StealthConstants.SURVEILLANCE_SWEEP_PAUSE_DURATION;

        [Header("Alert")]
        [SerializeField, BasicSetting, Min(0f), Tooltip("Seconds after an alert before the camera can raise another one.")] private float alertCooldown = StealthConstants.SURVEILLANCE_ALERT_COOLDOWN;

        [Header("Vision")]
        [SerializeField, Min(0f), Tooltip("Height above the floor below the camera from which line of sight is traced, so walls and cover hide the player as they do from a guard.")] private float sightHeight = StealthConstants.SURVEILLANCE_SIGHT_HEIGHT;
        [SerializeField] private VisionConfig vision = new VisionConfig(
            StealthConstants.SURVEILLANCE_VISION_RANGE,
            StealthConstants.SURVEILLANCE_VISION_MIN_RANGE,
            StealthConstants.SURVEILLANCE_VISION_ANGLE,
            StealthConstants.VISION_CONFIRM_DURATION);

        public float SweepHalfAngle => sweepHalfAngle;
        public float SweepSpeed => sweepSpeed;
        public float SweepPauseDuration => sweepPauseDuration;
        public float AlertCooldown => alertCooldown;
        public float SightHeight => sightHeight;
        public VisionConfig Vision => vision;
    }
}
