using System;
using UnityEngine.Serialization;
using UnityEngine;

namespace StealthController.Core
{
    [Serializable]
    public class CameraRigConfig
    {
        [Header("Framing")]
        [SerializeField, FormerlySerializedAs("defaultFraming")] private CameraFramingConfig framing = new CameraFramingConfig();

        [Header("Follow")]
        [SerializeField, Min(0f)] private float pivotHeight = StealthConstants.CAMERA_PIVOT_HEIGHT;
        [SerializeField, BasicSetting, Min(0f), Tooltip("How far ahead of a moving player the camera aims, in metres.")] private float lookAheadDistance = StealthConstants.CAMERA_LOOK_AHEAD_DISTANCE;
        [SerializeField, BasicSetting, Min(0f), Tooltip("Seconds the camera takes to catch up with the player. Lower follows more tightly.")] private float followSmoothTime = StealthConstants.CAMERA_FOLLOW_SMOOTH_TIME;

        public CameraFramingConfig Framing => framing;

        public float PivotHeight => pivotHeight;
        public float LookAheadDistance => lookAheadDistance;
        public float FollowSmoothTime => followSmoothTime;
    }
}
