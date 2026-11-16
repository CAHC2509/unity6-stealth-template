using System;
using UnityEngine;

namespace StealthController.Core
{
    [Serializable]
    public class VisionConfig
    {
        [SerializeField, BasicSetting, Min(0f), Tooltip("How far it can see, in metres.")] private float range = StealthConstants.GUARD_VISION_RANGE;
        [SerializeField, Min(0f), Tooltip("Blind distance around the eye, measured on the ground. The area below a wall camera stays safe.")] private float minRange;
        [SerializeField, BasicSetting, Range(0f, StealthConstants.FULL_TURN_DEGREES), Tooltip("Width of the vision cone, in degrees.")] private float fullAngle = StealthConstants.GUARD_VISION_ANGLE;
        [SerializeField, BasicSetting, Min(0f), Tooltip("Seconds of continuous line of sight before the player counts as detected.")] private float confirmDuration = StealthConstants.VISION_CONFIRM_DURATION;

        public float Range => range;
        public float MinRange => minRange;
        public float FullAngle => fullAngle;
        public float HalfAngle => fullAngle * 0.5f;
        public float ConfirmDuration => confirmDuration;

        public VisionConfig() { }

        public VisionConfig(float range, float minRange, float fullAngle, float confirmDuration)
        {
            this.range = range;
            this.minRange = minRange;
            this.fullAngle = fullAngle;
            this.confirmDuration = confirmDuration;
        }
    }
}
