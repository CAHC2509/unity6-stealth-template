using System;
using UnityEngine;

namespace StealthController.Core
{
    [Serializable]
    public class PlayerMovementConfig
    {
        [Header("Input")]
        [SerializeField, Range(0f, StealthConstants.PLAYER_MAX_STICK_DEAD_ZONE)] private float stickDeadZone = StealthConstants.PLAYER_STICK_DEAD_ZONE;
        [SerializeField, Range(StealthConstants.PLAYER_MIN_RUN_INPUT_THRESHOLD, StealthConstants.FULL_STICK_TILT)] private float runInputThreshold = StealthConstants.PLAYER_RUN_INPUT_THRESHOLD;

        [Header("Speeds")]
        [SerializeField, Min(0f)] private float minWalkSpeed = StealthConstants.PLAYER_MIN_WALK_SPEED;
        [SerializeField, BasicSetting, Min(0f), Tooltip("Walking speed with the stick fully tilted or a movement key held, in metres per second.")] private float maxWalkSpeed = StealthConstants.PLAYER_MAX_WALK_SPEED;
        [SerializeField, BasicSetting, Min(0f), Tooltip("Speed while holding Sprint, in metres per second.")] private float runSpeed = StealthConstants.PLAYER_RUN_SPEED;
        [SerializeField, Min(0f)] private float crouchWalkMinSpeed = StealthConstants.PLAYER_CROUCH_WALK_MIN_SPEED;
        [SerializeField, BasicSetting, Min(0f), Tooltip("Top speed while crouched, in metres per second.")] private float crouchWalkSpeed = StealthConstants.PLAYER_CROUCH_WALK_SPEED;
        [SerializeField, BasicSetting, Min(0f), Tooltip("Speed while prone (crawling), in metres per second.")] private float crawlSpeed = StealthConstants.PLAYER_CRAWL_SPEED;
        [SerializeField, BasicSetting, Min(0f), Tooltip("How quickly the player reaches its speed, in metres per second squared. Higher feels snappier.")] private float acceleration = StealthConstants.PLAYER_ACCELERATION;
        [SerializeField, BasicSetting, Min(0f), Tooltip("How quickly the player stops after letting go of the input, in metres per second squared.")] private float deceleration = StealthConstants.PLAYER_DECELERATION;

        [Header("Turning")]
        [SerializeField, BasicSetting, Min(0f), Tooltip("Fastest the player can turn while standing or crouched, in degrees per second.")] private float standingTurnSpeed = StealthConstants.PLAYER_STANDING_TURN_SPEED;
        [SerializeField, BasicSetting, Min(0f), Tooltip("Fastest the player can turn while prone, in degrees per second.")] private float crawlingTurnSpeed = StealthConstants.PLAYER_CRAWLING_TURN_SPEED;
        [SerializeField, BasicSetting, Min(0f), Tooltip("Approximate time the body takes to face a new direction while standing or crouched.")]
        private float standingTurnSmoothTime = StealthConstants.PLAYER_STANDING_TURN_SMOOTH_TIME;
        [SerializeField, BasicSetting, Min(0f), Tooltip("Approximate time the body takes to face a new direction while crawling.")]
        private float crawlingTurnSmoothTime = StealthConstants.PLAYER_CRAWLING_TURN_SMOOTH_TIME;

        [Header("Posture")]
        [SerializeField, BasicSetting, Min(0f), Tooltip("Seconds it takes to crouch down or to stand up.")] private float postureTransitionDuration = StealthConstants.PLAYER_POSTURE_TRANSITION_DURATION;
        [SerializeField, BasicSetting, Min(0f), Tooltip("Duration of the drop from crouched to prone and of getting back up from prone.")]
        private float proneTransitionDuration = StealthConstants.PLAYER_PRONE_TRANSITION_DURATION;
        [SerializeField, BasicSetting, Min(0f), Tooltip("Time the crouch button must be held to drop from crouched to prone.")]
        private float crouchHoldToProneDuration = StealthConstants.PLAYER_CROUCH_HOLD_TO_PRONE_DURATION;
        [SerializeField, Min(StealthConstants.PLAYER_MIN_CAPSULE_RADIUS)] private float standingRadius = StealthConstants.PLAYER_STANDING_RADIUS;
        [SerializeField, Min(StealthConstants.PLAYER_MIN_CAPSULE_RADIUS)] private float crawlingRadius = StealthConstants.PLAYER_CRAWLING_RADIUS;
        [SerializeField, Min(StealthConstants.PLAYER_MIN_CAPSULE_HEIGHT)] private float standingHeight = StealthConstants.PLAYER_STANDING_HEIGHT;
        [SerializeField, Min(StealthConstants.PLAYER_MIN_CAPSULE_HEIGHT)] private float crouchedHeight = StealthConstants.PLAYER_CROUCHED_HEIGHT;
        [SerializeField, Min(StealthConstants.PLAYER_MIN_CAPSULE_HEIGHT)] private float crawlingHeight = StealthConstants.PLAYER_CRAWLING_HEIGHT;

        [Header("Wall")]
        [SerializeField, Min(0f)] private float wallDetectDistance = StealthConstants.WALL_DETECT_DISTANCE;
        [SerializeField, Range(0f, StealthConstants.RIGHT_ANGLE_DEGREES)] private float wallAngleTolerance = StealthConstants.WALL_ANGLE_TOLERANCE;
        [SerializeField, Min(0f)] private float wallPushDuration = StealthConstants.WALL_PUSH_DURATION;
        [SerializeField, Min(StealthConstants.WALL_MIN_HEIGHT_LIMIT)] private float standingWallMinHeight = StealthConstants.WALL_STANDING_MIN_HEIGHT;
        [SerializeField, BasicSetting, Min(0f), Tooltip("Speed while moving sideways with the back against a wall, in metres per second.")] private float wallShuffleStandingSpeed = StealthConstants.WALL_SHUFFLE_STANDING_SPEED;
        [SerializeField, Range(0f, StealthConstants.FULL_STICK_TILT)] private float wallLateralInputThreshold = StealthConstants.WALL_LATERAL_INPUT_THRESHOLD;
        [SerializeField, Range(0f, StealthConstants.FULL_STICK_TILT)] private float wallLeaveInputThreshold = StealthConstants.WALL_LEAVE_INPUT_THRESHOLD;
        [SerializeField, Min(0f), Tooltip("How close to an outer corner the player must be for input along the next face to turn the corner.")]
        private float cornerAnticipationDistance = StealthConstants.WALL_CORNER_ANTICIPATION_DISTANCE;
        [SerializeField, Min(0f), Tooltip("How far behind the wall plane the corner probe looks for the next face.")]
        private float cornerDepthProbe = StealthConstants.WALL_CORNER_DEPTH_PROBE;

        [Header("Collision")]
        [SerializeField, Min(StealthConstants.COLLISION_MIN_SKIN_WIDTH)] private float skinWidth = StealthConstants.COLLISION_SKIN_WIDTH;
        [SerializeField, Range(StealthConstants.COLLISION_MIN_SLIDE_ITERATIONS, StealthConstants.COLLISION_MAX_SLIDE_ITERATIONS)] private int maxSlideIterations = StealthConstants.COLLISION_SLIDE_ITERATIONS;
        [SerializeField, Min(0f)] private float stepOffset = StealthConstants.COLLISION_STEP_OFFSET;
        [SerializeField, Range(0f, StealthConstants.NEAR_VERTICAL_DEGREES)] private float slopeLimit = StealthConstants.COLLISION_SLOPE_LIMIT;
        [SerializeField, Min(0f)] private float gravity = StealthConstants.COLLISION_GRAVITY;
        [SerializeField, Min(0f)] private float maxFallSpeed = StealthConstants.COLLISION_MAX_FALL_SPEED;
        [SerializeField, Min(0f)] private float groundSnapDistance = StealthConstants.COLLISION_GROUND_SNAP_DISTANCE;

        public float StickDeadZone => stickDeadZone;
        public float RunInputThreshold => runInputThreshold;

        public float MinWalkSpeed => minWalkSpeed;
        public float MaxWalkSpeed => maxWalkSpeed;
        public float RunSpeed => runSpeed;
        public float CrouchWalkMinSpeed => crouchWalkMinSpeed;
        public float CrouchWalkSpeed => crouchWalkSpeed;
        public float CrawlSpeed => crawlSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;

        public float StandingTurnSpeed => standingTurnSpeed;
        public float CrawlingTurnSpeed => crawlingTurnSpeed;
        public float StandingTurnSmoothTime => standingTurnSmoothTime;
        public float CrawlingTurnSmoothTime => crawlingTurnSmoothTime;

        public float PostureTransitionDuration => postureTransitionDuration;
        public float ProneTransitionDuration => proneTransitionDuration;
        public float CrouchHoldToProneDuration => crouchHoldToProneDuration;
        public float StandingRadius => standingRadius;
        public float CrawlingRadius => crawlingRadius;
        public float StandingHeight => standingHeight;
        public float CrouchedHeight => crouchedHeight;
        public float CrawlingHeight => crawlingHeight;

        public float WallDetectDistance => wallDetectDistance;
        public float WallAngleTolerance => wallAngleTolerance;
        public float WallPushDuration => wallPushDuration;
        public float StandingWallMinHeight => standingWallMinHeight;
        public float WallShuffleStandingSpeed => wallShuffleStandingSpeed;
        public float WallLateralInputThreshold => wallLateralInputThreshold;
        public float WallLeaveInputThreshold => wallLeaveInputThreshold;
        public float CornerAnticipationDistance => cornerAnticipationDistance;
        public float CornerDepthProbe => cornerDepthProbe;

        public float SkinWidth => skinWidth;
        public int MaxSlideIterations => maxSlideIterations;
        public float StepOffset => stepOffset;
        public float SlopeLimit => slopeLimit;
        public float Gravity => gravity;
        public float MaxFallSpeed => maxFallSpeed;
        public float GroundSnapDistance => groundSnapDistance;
    }
}
