using System;
using UnityEngine;

namespace StealthController.Core
{
    [Serializable]
    public class TakedownConfig
    {
        [Header("Takedown Reach")]
        [SerializeField, BasicSetting, Min(0f), Tooltip("Furthest horizontal distance from which the player can start a takedown.")] private float maxDistance = StealthConstants.TAKEDOWN_MAX_DISTANCE;
        [SerializeField, Min(0f)] private float maxHeightDifference = StealthConstants.TAKEDOWN_MAX_HEIGHT_DIFFERENCE;
        [SerializeField, BasicSetting, Range(0f, StealthConstants.HALF_TURN_DEGREES), Tooltip("Half angle of the cone behind the guard where the player must stand.")] private float backHalfAngle = StealthConstants.TAKEDOWN_BACK_HALF_ANGLE;

        [Header("Paired Animation")]
        [SerializeField, Range(0f, StealthConstants.TAKEDOWN_MAX_ATTACKER_OFFSET), Tooltip("Distance behind the guard where the attacker clip starts.")] private float attackerOffset = StealthConstants.TAKEDOWN_ATTACKER_OFFSET;
        [SerializeField, Min(StealthConstants.MIN_MOVE_DISTANCE), Tooltip("Approach speed of the attacker clip, used to skip the approach the player already walked.")] private float approachSpeed = StealthConstants.TAKEDOWN_APPROACH_SPEED;
        [SerializeField, Min(0f)] private float maxStartTime = StealthConstants.TAKEDOWN_MAX_START_TIME;
        [SerializeField, Range(StealthConstants.MIN_MOVE_DISTANCE, StealthConstants.TAKEDOWN_MAX_CLIP_LENGTH)] private float clipLength = StealthConstants.TAKEDOWN_CLIP_LENGTH;
        [SerializeField, Range(0f, StealthConstants.TAKEDOWN_MAX_CLIP_LENGTH), Tooltip("Clip time at which the player gets control back and the guard stays down.")] private float endTime = StealthConstants.TAKEDOWN_END_TIME;

        [Header("Exit")]
        [SerializeField, Tooltip("Where the attacker body ends at the end time, relative to the start point and the guard's facing.")] private float exitOffsetSide = StealthConstants.TAKEDOWN_EXIT_OFFSET_SIDE;
        [SerializeField] private float exitOffsetForward = StealthConstants.TAKEDOWN_EXIT_OFFSET_FORWARD;
        [SerializeField, Range(-StealthConstants.HALF_TURN_DEGREES, StealthConstants.HALF_TURN_DEGREES)] private float exitYawOffset = StealthConstants.TAKEDOWN_EXIT_YAW_OFFSET;

        public float MaxDistance => maxDistance;
        public float MaxHeightDifference => maxHeightDifference;
        public float BackHalfAngle => backHalfAngle;
        public float AttackerOffset => attackerOffset;
        public float ApproachSpeed => approachSpeed;
        public float MaxStartTime => maxStartTime;
        public float ClipLength => clipLength;
        public float EndTime => Mathf.Min(endTime, clipLength);
        public float ExitOffsetSide => exitOffsetSide;
        public float ExitOffsetForward => exitOffsetForward;
        public float ExitYawOffset => exitYawOffset;
    }
}
