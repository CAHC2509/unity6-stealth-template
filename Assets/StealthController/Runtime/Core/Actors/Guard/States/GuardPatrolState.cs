namespace StealthController.Core
{
    public class GuardPatrolState : GuardStateBase
    {
        private float waitRemaining;
        private bool isWaiting;

        public override GuardAlertState AlertState => GuardAlertState.Patrol;

        public GuardPatrolState(GuardLogic guard) : base(guard) { }

        public override void Enter()
        {
            isWaiting = false;

            if (!guard.Route.HasRoute) return;

            guard.Route.StartFromNearest(guard.Position);
            guard.PathFollower.TrySetDestination(guard.Route.Current.Position);
        }

        public override void Tick(float deltaTime, PlayerPose player)
        {
            if (guard.Vision.HasDetectedPlayer)
            {
                guard.Pursue(guard.Vision.LastSeenPosition);
                return;
            }

            if (!guard.Route.HasRoute) return;

            if (isWaiting)
            {
                TickWait(deltaTime);
                return;
            }

            if (!guard.PathFollower.FollowPath(Config.PatrolSpeed, deltaTime)) return;

            isWaiting = true;
            waitRemaining = guard.Route.Current.WaitDuration;
        }

        private void TickWait(float deltaTime)
        {
            GuardWaypoint waypoint = guard.Route.Current;

            if (waypoint.HasLookYaw)
                guard.TurnTowardsYaw(waypoint.LookYaw, deltaTime);

            waitRemaining -= deltaTime;
            if (waitRemaining > 0f) return;

            isWaiting = false;
            guard.Route.AdvanceToNext();
            guard.PathFollower.TrySetDestination(guard.Route.Current.Position);
        }
    }
}
