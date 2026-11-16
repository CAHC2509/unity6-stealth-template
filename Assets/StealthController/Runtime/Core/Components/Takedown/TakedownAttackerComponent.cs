using UnityEngine;

namespace StealthController.Core
{
    public class TakedownAttackerComponent
    {
        private float clipLength = 1f;
        private float endTime;

        private float clipTime;
        private Vector3 exitPosition;
        private float exitYaw;

        public bool IsFinished => clipTime >= endTime;
        public float Progress => clipTime / clipLength;
        public Vector3 ExitPosition => exitPosition;
        public float ExitYaw => exitYaw;

        public void Begin(float startTime, float clipDuration, float finishTime, Vector3 exitPoint, float exitFacingYaw)
        {
            clipLength = clipDuration;
            endTime = finishTime;
            clipTime = startTime;
            exitPosition = exitPoint;
            exitYaw = exitFacingYaw;
        }

        public void Tick(float deltaTime)
        {
            clipTime = Mathf.Min(clipTime + deltaTime, endTime);
        }

        public void Reset()
        {
            clipTime = 0f;
        }
    }
}
