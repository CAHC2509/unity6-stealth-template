using System;
using UnityEngine;

namespace StealthController.Core
{
    [Serializable]
    public class GuardConfig
    {
        [Header("Movement")]
        [SerializeField, BasicSetting, Min(0f), Tooltip("Walking speed while patrolling, in metres per second.")] private float patrolSpeed = StealthConstants.GUARD_PATROL_SPEED;
        [SerializeField, BasicSetting, Min(0f), Tooltip("Running speed while chasing the player, in metres per second.")] private float chaseSpeed = StealthConstants.GUARD_CHASE_SPEED;
        [SerializeField, BasicSetting, Min(0f), Tooltip("Fastest the guard can turn, in degrees per second.")] private float turnSpeed = StealthConstants.GUARD_TURN_SPEED;
        [SerializeField, Min(0f), Tooltip("Approximate time the guard takes to face a new direction.")] private float turnSmoothTime = StealthConstants.GUARD_TURN_SMOOTH_TIME;
        [SerializeField, Min(0f), Tooltip("How quickly the guard speeds up and slows down, in metres per second squared.")] private float acceleration = StealthConstants.GUARD_ACCELERATION;
        [SerializeField, Min(0f), Tooltip("How far a destination may be from the walkable area and still be reached.")] private float walkableSnapDistance = StealthConstants.GUARD_WALKABLE_SNAP_DISTANCE;

        [Header("Vision")]
        [SerializeField, Min(0f)] private float eyeHeight = StealthConstants.GUARD_EYE_HEIGHT;
        [SerializeField] private VisionConfig vision = new VisionConfig();

        [Header("Chase")]
        [SerializeField, BasicSetting, Min(0f), Tooltip("The player is caught when a chasing guard gets this close, in metres.")] private float captureDistance = StealthConstants.GUARD_CAPTURE_DISTANCE;
        [SerializeField, Min(0f)] private float repathInterval = StealthConstants.GUARD_REPATH_INTERVAL;

        [Header("Search")]
        [SerializeField, BasicSetting, Min(0f), Tooltip("Seconds the guard searches the place where it lost the player before going back to its route.")] private float searchDuration = StealthConstants.GUARD_SEARCH_DURATION;
        [SerializeField, Range(0f, StealthConstants.HALF_TURN_DEGREES)] private float searchSweepAngle = StealthConstants.GUARD_SEARCH_SWEEP_ANGLE;
        [SerializeField, Min(0f)] private float searchSweepSpeed = StealthConstants.GUARD_SEARCH_SWEEP_SPEED;

        public float PatrolSpeed => patrolSpeed;
        public float ChaseSpeed => chaseSpeed;
        public float TurnSpeed => turnSpeed;
        public float TurnSmoothTime => turnSmoothTime;
        public float Acceleration => acceleration;
        public float WalkableSnapDistance => walkableSnapDistance;

        public float EyeHeight => eyeHeight;
        public VisionConfig Vision => vision;

        public float CaptureDistance => captureDistance;
        public float RepathInterval => repathInterval;

        public float SearchDuration => searchDuration;
        public float SearchSweepAngle => searchSweepAngle;
        public float SearchSweepSpeed => searchSweepSpeed;
    }
}
