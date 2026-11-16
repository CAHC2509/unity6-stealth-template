using UnityEngine;

namespace StealthController.Core
{
    public class TakedownVictimComponent
    {
        private float clipLength = 1f;
        private float endTime;

        private float clipTime;
        private bool isDown;

        public bool IsDown => isDown;
        public float Progress => clipTime / clipLength;

        public void Begin(float startTime, float clipDuration, float finishTime)
        {
            clipLength = clipDuration;
            endTime = finishTime;
            clipTime = startTime;
            isDown = true;
        }

        public void Tick(float deltaTime)
        {
            clipTime = Mathf.Min(clipTime + deltaTime, endTime);
        }

        public void Reset()
        {
            clipTime = 0f;
            isDown = false;
        }
    }
}
