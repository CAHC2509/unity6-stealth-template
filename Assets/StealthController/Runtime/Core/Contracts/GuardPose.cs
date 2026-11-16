using UnityEngine;

namespace StealthController.Core
{
    public readonly struct GuardPose
    {
        public Vector3 Position { get; }
        public float Yaw { get; }
        public GuardAlertState AlertState { get; }
        public bool IsSearching { get; }
        public bool CanSeePlayer { get; }
        public float Speed { get; }
        public float ActionProgress { get; }

        public bool IsTakenDown => AlertState == GuardAlertState.TakenDown;

        public Quaternion Rotation => Quaternion.Euler(0f, Yaw, 0f);

        public GuardPose(Vector3 position, float yaw, GuardAlertState alertState, bool isSearching, bool canSeePlayer, float speed)
            : this(position, yaw, alertState, isSearching, canSeePlayer, speed, 0f) { }

        public GuardPose(Vector3 position, float yaw, GuardAlertState alertState, bool isSearching, bool canSeePlayer, float speed, float actionProgress)
        {
            Position = position;
            Yaw = yaw;
            AlertState = alertState;
            IsSearching = isSearching;
            CanSeePlayer = canSeePlayer;
            Speed = speed;
            ActionProgress = actionProgress;
        }
    }
}
