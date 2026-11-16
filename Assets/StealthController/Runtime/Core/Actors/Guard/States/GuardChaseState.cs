using UnityEngine;

namespace StealthController.Core
{
    public class GuardChaseState : GuardStateBase
    {
        private Vector3 targetPosition;
        private float repathRemaining;
        private bool isSearching;
        private float searchElapsed;
        private float searchBaseYaw;

        public bool IsSearching => isSearching;

        public override GuardAlertState AlertState => GuardAlertState.Chase;

        public GuardChaseState(GuardLogic guard) : base(guard) { }

        public void SetTarget(Vector3 knownPosition)
        {
            targetPosition = knownPosition;
            isSearching = false;
            repathRemaining = 0f;
        }

        public override void Tick(float deltaTime, PlayerPose player)
        {
            if (guard.Vision.CanSeePlayer && TrackVisiblePlayer(player)) return;

            if (isSearching)
            {
                TickSearch(deltaTime);
                return;
            }

            repathRemaining -= deltaTime;

            if (repathRemaining <= 0f)
            {
                repathRemaining = Config.RepathInterval;

                if (!guard.PathFollower.TrySetDestination(targetPosition))
                {
                    BeginSearch();
                    return;
                }
            }

            if (!guard.PathFollower.FollowPath(Config.ChaseSpeed, deltaTime)) return;
            if (guard.Vision.CanSeePlayer) return;

            BeginSearch();
        }

        private bool TrackVisiblePlayer(PlayerPose player)
        {
            targetPosition = player.Position;
            repathRemaining = 0f;

            if (isSearching)
            {
                isSearching = false;
                repathRemaining = 0f;
            }

            return guard.Capture.TryCapture(guard.PathFollower.HorizontalDistanceTo(player.Position));
        }

        private void BeginSearch()
        {
            guard.PathFollower.Halt();
            isSearching = true;
            searchElapsed = 0f;
            searchBaseYaw = guard.Yaw;
        }

        private void TickSearch(float deltaTime)
        {
            searchElapsed += deltaTime;

            float sweepAngle = Config.SearchSweepAngle;
            float sweepOffset = sweepAngle > 0f ? sweepAngle * Mathf.Sin(searchElapsed * Config.SearchSweepSpeed / sweepAngle) : 0f;
            guard.TurnTowardsYaw(searchBaseYaw + sweepOffset, deltaTime);

            if (searchElapsed >= Config.SearchDuration)
                guard.ReturnToPatrol();
        }
    }
}
