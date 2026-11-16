using UnityEngine;

namespace StealthController.Core
{
    public class SweepComponent
    {
        private readonly float halfAngle;
        private readonly float speed;
        private readonly float pauseDuration;

        private float offset;
        private float direction;
        private float pauseRemaining;

        public float Offset => offset;

        public SweepComponent(float halfAngle, float speed, float pauseDuration)
        {
            this.halfAngle = halfAngle;
            this.speed = speed;
            this.pauseDuration = pauseDuration;
        }

        public void Reset()
        {
            offset = 0f;
            direction = 1f;
            pauseRemaining = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (pauseRemaining > 0f)
            {
                pauseRemaining -= deltaTime;
                return;
            }

            offset += direction * speed * deltaTime;

            if (Mathf.Abs(offset) < halfAngle) return;

            offset = Mathf.Clamp(offset, -halfAngle, halfAngle);
            direction = -direction;
            pauseRemaining = pauseDuration;
        }
    }
}
