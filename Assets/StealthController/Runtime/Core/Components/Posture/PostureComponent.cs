using UnityEngine;

namespace StealthController.Core
{
    public class PostureComponent
    {
        private readonly CapsuleMotorComponent motor;

        private float transitionRemaining;
        private float transitionDuration;
        private bool isTransitionBlockingMovement;
        private bool hasQueuedStandUp;

        public bool IsInTransition => transitionRemaining > 0f;
        public float TransitionProgress => IsInTransition ? 1f - transitionRemaining / transitionDuration : 1f;
        public float MovementScale => !IsInTransition ? 1f : isTransitionBlockingMovement ? 0f : TransitionProgress;
        public bool HasQueuedStandUp => hasQueuedStandUp;

        public PostureComponent(CapsuleMotorComponent motor)
        {
            this.motor = motor;
        }

        public void Reset()
        {
            transitionRemaining = 0f;
            isTransitionBlockingMovement = false;
            hasQueuedStandUp = false;
        }

        public void Tick(float deltaTime)
        {
            transitionRemaining = Mathf.Max(0f, transitionRemaining - deltaTime);
        }

        public bool TryChange(float radius, float height, float duration)
        {
            return TryChange(radius, height, duration, false);
        }

        public bool TryChange(float radius, float height, float duration, bool blocksMovement)
        {
            if (!motor.CanFitShape(radius, height)) return false;

            motor.SetShape(radius, height);
            isTransitionBlockingMovement = blocksMovement;
            transitionDuration = duration;
            transitionRemaining = duration;
            return true;
        }

        public void QueueStandUp()
        {
            hasQueuedStandUp = true;
        }

        public void ClearQueuedStandUp()
        {
            hasQueuedStandUp = false;
        }
    }
}
