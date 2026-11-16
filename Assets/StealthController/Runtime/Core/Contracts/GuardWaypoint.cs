using UnityEngine;

namespace StealthController.Core
{
    public readonly struct GuardWaypoint
    {
        public Vector3 Position { get; }
        public float WaitDuration { get; }
        public bool HasLookYaw { get; }
        public float LookYaw { get; }

        public GuardWaypoint(Vector3 position, float waitDuration, bool hasLookYaw, float lookYaw)
        {
            Position = position;
            WaitDuration = waitDuration;
            HasLookYaw = hasLookYaw;
            LookYaw = lookYaw;
        }
    }
}
