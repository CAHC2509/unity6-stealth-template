using UnityEngine;

namespace StealthController.Core
{
    public class AlertEmitterComponent
    {
        private readonly float cooldown;

        private float cooldownRemaining;
        private bool hasPendingAlert;
        private Vector3 alertPosition;

        public AlertEmitterComponent(float cooldown)
        {
            this.cooldown = cooldown;
        }

        public void Reset()
        {
            cooldownRemaining = 0f;
            hasPendingAlert = false;
        }

        public void Tick(float deltaTime)
        {
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - deltaTime);
        }

        public void Raise(Vector3 knownPosition)
        {
            if (cooldownRemaining > 0f) return;

            hasPendingAlert = true;
            alertPosition = knownPosition;
            cooldownRemaining = cooldown;
        }

        public bool TryConsume(out Vector3 knownPosition)
        {
            knownPosition = alertPosition;

            if (!hasPendingAlert) return false;

            hasPendingAlert = false;
            return true;
        }
    }
}
