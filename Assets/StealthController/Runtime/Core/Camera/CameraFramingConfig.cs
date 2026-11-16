using System;
using UnityEngine;

namespace StealthController.Core
{
    [Serializable]
    public class CameraFramingConfig
    {
        [SerializeField, BasicSetting, Range(0f, StealthConstants.NEAR_VERTICAL_DEGREES), Tooltip("How much the camera looks down, in degrees. Higher looks more straight down.")] private float pitch = StealthConstants.CAMERA_DEFAULT_PITCH;
        [SerializeField, BasicSetting, Range(-StealthConstants.HALF_TURN_DEGREES, StealthConstants.HALF_TURN_DEGREES), Tooltip("Direction the camera looks from, around the player, in degrees.")] private float yaw = StealthConstants.CAMERA_DEFAULT_YAW;
        [SerializeField, BasicSetting, Min(StealthConstants.CAMERA_MIN_DISTANCE), Tooltip("Distance from the camera to the player, in metres.")] private float distance = StealthConstants.CAMERA_DEFAULT_DISTANCE;
        [SerializeField, BasicSetting, Range(StealthConstants.CAMERA_MIN_FIELD_OF_VIEW, StealthConstants.CAMERA_MAX_FIELD_OF_VIEW), Tooltip("Vertical field of view, in degrees. Lower zooms in.")] private float fieldOfView = StealthConstants.CAMERA_DEFAULT_FIELD_OF_VIEW;

        public float Pitch => pitch;
        public float Yaw => yaw;
        public float Distance => distance;
        public float FieldOfView => fieldOfView;

        public CameraFramingConfig() { }

        public CameraFramingConfig(float pitch, float yaw, float distance, float fieldOfView)
        {
            this.pitch = pitch;
            this.yaw = yaw;
            this.distance = distance;
            this.fieldOfView = fieldOfView;
        }
    }
}
