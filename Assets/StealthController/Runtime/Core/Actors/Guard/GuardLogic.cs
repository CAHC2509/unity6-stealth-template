using System.Collections.Generic;
using UnityEngine;

namespace StealthController.Core
{
    public class GuardLogic
    {
        private readonly GuardConfig config;
        private readonly FacingComponent facing;
        private readonly PathFollowerComponent pathFollower;
        private readonly VisionSensorComponent vision;
        private readonly PatrolRouteComponent route;
        private readonly PlayerCaptureComponent capture;
        private readonly TakedownVictimComponent takedownVictim;
        private readonly GuardPatrolState patrolState;
        private readonly GuardChaseState chaseState;
        private readonly GuardTakenDownState takenDownState;
        private readonly Vector3 spawnPosition;
        private readonly float spawnYaw;

        private GuardStateBase currentState;
        private float measuredSpeed;

        public GuardConfig Config => config;
        public FacingComponent Facing => facing;
        public PathFollowerComponent PathFollower => pathFollower;
        public VisionSensorComponent Vision => vision;
        public PatrolRouteComponent Route => route;
        public PlayerCaptureComponent Capture => capture;
        public TakedownVictimComponent TakedownVictim => takedownVictim;

        public Vector3 Position => pathFollower.Position;
        public float Yaw => facing.Yaw;
        public Vector3 EyePosition => Position + Vector3.up * config.EyeHeight;
        public bool HasCapturedPlayer => capture.HasCapturedPlayer;
        public GuardAlertState AlertState => currentState.AlertState;
        public bool IsSearching => currentState == chaseState && chaseState.IsSearching;
        public bool IsTakenDown => currentState == takenDownState;
        public float MeasuredSpeed => measuredSpeed;

        public GuardPose Pose => new GuardPose(Position, Yaw, AlertState, IsSearching, vision.CanSeePlayer, measuredSpeed, IsTakenDown ? takedownVictim.Progress : 0f);

        public GuardLogic(GuardConfig config, IReadOnlyList<GuardWaypoint> waypoints, Vector3 spawnPosition, float spawnYaw, ICollisionWorld collisionWorld, IPathfinder pathfinder)
        {
            this.config = config;
            this.spawnPosition = spawnPosition;
            this.spawnYaw = spawnYaw;

            facing = new FacingComponent();
            pathFollower = new PathFollowerComponent(pathfinder, facing, config.Acceleration, config.WalkableSnapDistance, config.TurnSmoothTime, config.TurnSpeed);
            vision = new VisionSensorComponent(config.Vision, collisionWorld);
            route = new PatrolRouteComponent(waypoints);
            capture = new PlayerCaptureComponent(config.CaptureDistance);
            takedownVictim = new TakedownVictimComponent();
            patrolState = new GuardPatrolState(this);
            chaseState = new GuardChaseState(this);
            takenDownState = new GuardTakenDownState(this);

            Reset();
        }

        public void Reset()
        {
            pathFollower.Reset(spawnPosition);
            facing.Reset(spawnYaw);
            capture.Reset();
            takedownVictim.Reset();
            measuredSpeed = 0f;
            vision.Reset();

            ChangeState(patrolState);
        }

        public void Tick(float deltaTime, PlayerPose player)
        {
            if (IsTakenDown)
            {
                measuredSpeed = 0f;
                currentState.Tick(deltaTime, player);
                return;
            }

            Vector3 previousPosition = Position;

            vision.Tick(deltaTime, EyePosition, facing.Forward, player);
            currentState.Tick(deltaTime, player);

            measuredSpeed = deltaTime > 0f ? pathFollower.HorizontalDistanceTo(previousPosition) / deltaTime : 0f;
        }

        public void Pursue(Vector3 knownPosition)
        {
            if (IsTakenDown) return;

            chaseState.SetTarget(knownPosition);

            if (currentState != chaseState)
                ChangeState(chaseState);
        }

        public void BeginTakenDown(float startTime, float clipLength, float endTime)
        {
            takedownVictim.Begin(startTime, clipLength, endTime);
            ChangeState(takenDownState);
        }

        public void ReturnToPatrol()
        {
            ChangeState(patrolState);
        }

        public void TurnTowardsYaw(float targetYaw, float deltaTime)
        {
            facing.RotateTowardsYaw(targetYaw, config.TurnSmoothTime, config.TurnSpeed, deltaTime);
        }

        private void ChangeState(GuardStateBase nextState)
        {
            if (currentState != null)
                currentState.Exit();

            currentState = nextState;
            currentState.Enter();
        }
    }
}
