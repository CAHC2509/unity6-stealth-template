using UnityEngine;

namespace StealthController.Core
{
    public abstract class PlayerWallStateBase : PlayerStateBase
    {
        private readonly WallCornerTraversalLogic corner = new WallCornerTraversalLogic();

        private float lastLateralSide = 1f;

        protected abstract float MinWallHeight { get; }
        protected abstract float ShuffleSpeed { get; }
        protected abstract PlayerStateBase DetachedState { get; }

        protected PlayerWallStateBase(PlayerLogic player) : base(player) { }

        protected abstract bool TryHandlePostureInput(PlayerInputFrame input);

        public override void Enter()
        {
            corner.End();
        }

        public override void Exit()
        {
            corner.End();
        }

        public override void Tick(float deltaTime, PlayerInputFrame input, float referenceYaw)
        {
            player.Locomotion.Stop();
            player.Facing.RotateTowards(player.WallCover.Normal, Config.StandingTurnSmoothTime, Config.StandingTurnSpeed, deltaTime);

            if (player.Posture.IsInTransition) return;

            Vector3 direction = player.MoveInput.HasMoveInput(input) ? player.MoveInput.ComputeWorldDirection(input.Move, referenceYaw) : Vector3.zero;

            if (TryBreakIntoRun(input, direction)) return;

            if (corner.IsActive)
            {
                TickCorner(direction, deltaTime);
                return;
            }

            if (TryHandlePostureInput(input)) return;

            float probeHeight = player.WallCover.ProbeHeightFor(MinWallHeight);
            if (!player.WallCover.Sensor.TryReacquire(player.Motor.Position, player.Motor.Radius, player.WallCover.Normal, probeHeight, out WallContact contact))
            {
                Detach();
                return;
            }

            player.WallCover.UpdateContact(contact);

            if (direction == Vector3.zero) return;

            float stepDistance = ShuffleSpeed * deltaTime;

            if (TryTurnCornerToward(direction, contact, stepDistance, probeHeight))
            {
                if (corner.IsActive)
                    TickCorner(direction, deltaTime);

                return;
            }

            Vector3 wallNormal = player.WallCover.Normal;

            if (Vector3.Dot(direction, wallNormal) >= Config.WallLeaveInputThreshold)
            {
                Detach();
                return;
            }

            Vector3 tangent = Vector3.Cross(Vector3.up, wallNormal);
            float lateralInput = Vector3.Dot(direction, tangent);
            if (Mathf.Abs(lateralInput) < Config.WallLateralInputThreshold) return;

            lastLateralSide = Mathf.Sign(lateralInput);
            Vector3 lateralDirection = tangent * lastLateralSide;
            Vector3 nextPosition = player.Motor.Position + lateralDirection * stepDistance;

            bool wallContinues = player.WallCover.Sensor.HasWallAt(nextPosition, player.Motor.Radius, wallNormal, probeHeight)
                && player.WallCover.Sensor.ReachesHeight(nextPosition, player.Motor.Radius, wallNormal, MinWallHeight);

            if (!wallContinues)
            {
                Detach();
                return;
            }

            player.Locomotion.SetHorizontalVelocity(lateralDirection * ShuffleSpeed);
        }

        protected void Detach()
        {
            player.WallCover.Detach();
            player.ChangeState(DetachedState);
        }

        private bool TryBreakIntoRun(PlayerInputFrame input, Vector3 direction)
        {
            if (!input.SprintHeld || direction == Vector3.zero) return false;
            if (Vector3.Dot(direction, player.WallCover.Normal) <= -Config.WallLateralInputThreshold) return false;

            corner.End();
            player.WallCover.Detach();
            player.ChangeState(player.StandingState);
            player.Locomotion.SetHorizontalVelocity(direction * ShuffleSpeed);
            return true;
        }

        private bool TryTurnCornerToward(Vector3 direction, WallContact contact, float stepDistance, float probeHeight)
        {
            Vector3 tangent = Vector3.Cross(Vector3.up, player.WallCover.Normal);
            Vector3 preferredSide = tangent * lastLateralSide;

            return TryEnterInnerCorner(direction, contact, preferredSide, stepDistance, probeHeight)
                || TryEnterInnerCorner(direction, contact, -preferredSide, stepDistance, probeHeight)
                || TryBeginOuterCorner(direction, contact, preferredSide, probeHeight)
                || TryBeginOuterCorner(direction, contact, -preferredSide, probeHeight);
        }

        private bool TryEnterInnerCorner(Vector3 direction, WallContact contact, Vector3 side, float stepDistance, float probeHeight)
        {
            Vector3 position = player.Motor.Position;
            float reach = contact.Distance + stepDistance;

            if (!player.WallCover.Sensor.TryFindInnerCornerWall(position, player.Motor.Radius, player.WallCover.Normal, side, reach, probeHeight, out WallContact sideWall)) return false;

            Vector3 travel = Vector3.ProjectOnPlane(player.WallCover.Normal, sideWall.Normal).normalized;
            bool wantsSideWall = Vector3.Dot(direction, side) >= Config.WallLateralInputThreshold
                || Vector3.Dot(direction, travel) >= Config.WallLateralInputThreshold;

            if (!wantsSideWall) return false;
            if (!player.WallCover.Sensor.ReachesHeight(position, player.Motor.Radius, sideWall.Normal, MinWallHeight)) return false;

            player.WallCover.UpdateContact(sideWall);
            return true;
        }

        private bool TryBeginOuterCorner(Vector3 direction, WallContact contact, Vector3 side, float probeHeight)
        {
            Vector3 position = player.Motor.Position;
            float anticipation = Config.CornerAnticipationDistance;

            if (player.WallCover.Sensor.HasWallAt(position + side * anticipation, player.Motor.Radius, player.WallCover.Normal, probeHeight)) return false;
            if (!player.WallCover.Sensor.TryFindOuterCornerFace(position, contact.Distance, player.WallCover.Normal, side, anticipation + player.Motor.Radius, probeHeight, out WallContact nextFace)) return false;

            Vector3 travel = Vector3.ProjectOnPlane(-player.WallCover.Normal, nextFace.Normal).normalized;
            if (Vector3.Dot(direction, travel) < Config.WallLateralInputThreshold) return false;
            if (!WallSensorLogic.TryIntersectFaces(contact, nextFace, out Vector3 edge)) return false;

            Vector3 flatEdge = new Vector3(edge.x, position.y, edge.z);
            Vector3 cornerExit = flatEdge + nextFace.Normal * contact.Distance + travel * Config.CornerDepthProbe;
            if (!player.WallCover.Sensor.ReachesHeight(cornerExit, player.Motor.Radius, nextFace.Normal, MinWallHeight)) return false;

            corner.Begin(position, flatEdge, player.WallCover.Normal, nextFace.Normal, Config.CornerDepthProbe);
            return true;
        }

        private void TickCorner(Vector3 direction, float deltaTime)
        {
            float progress = ResolveCornerProgress(direction);

            if (progress == 0f)
            {
                if (direction != Vector3.zero && Vector3.Dot(direction, corner.CurrentNormal) >= Config.WallLeaveInputThreshold)
                {
                    corner.End();
                    Detach();
                }

                return;
            }

            corner.Advance(progress * ShuffleSpeed * deltaTime);
            player.WallCover.SetNormal(corner.CurrentNormal);

            Vector3 offset = corner.CurrentPoint - player.Motor.Position;
            offset.y = 0f;
            player.Locomotion.SetHorizontalVelocity(offset / deltaTime);

            if (corner.HasReachedEnd)
            {
                player.WallCover.SetNormal(corner.ToNormal);
                corner.End();
                return;
            }

            if (progress < 0f && corner.HasReturnedToStart)
                corner.End();
        }

        private float ResolveCornerProgress(Vector3 direction)
        {
            if (direction == Vector3.zero) return 0f;

            float threshold = Config.WallLateralInputThreshold;
            bool wantsForward = Vector3.Dot(direction, corner.ForwardDirection) >= threshold
                || Vector3.Dot(direction, corner.ExitDirection) >= threshold;

            if (wantsForward) return 1f;

            bool wantsBack = Vector3.Dot(direction, -corner.ForwardDirection) >= threshold
                || Vector3.Dot(direction, -corner.StartDirection) >= threshold;

            return wantsBack ? -1f : 0f;
        }
    }
}
