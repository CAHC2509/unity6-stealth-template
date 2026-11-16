using System.Collections.Generic;
using UnityEngine;

namespace StealthController.Core
{
    public class TakedownLogic
    {
        private readonly TakedownConfig config;
        private readonly ICollisionWorld collisionWorld;

        private GuardLogic target;
        private Vector3 targetStartPosition;
        private Vector3 targetExitPosition;
        private float targetDistance;

        public TakedownConfig Config => config;
        public GuardLogic Target => target;
        public bool HasTarget => target != null;

        public TakedownLogic(TakedownConfig config, ICollisionWorld collisionWorld)
        {
            this.config = config;
            this.collisionWorld = collisionWorld;
        }

        public void Clear()
        {
            target = null;
        }

        public void UpdateTarget(PlayerLogic player, IReadOnlyList<GuardLogic> guards)
        {
            target = null;

            if (!CanAttack(player.PoseState)) return;

            PlayerPose pose = player.Pose;
            PlayerMovementConfig playerConfig = player.Config;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < guards.Count; i++)
            {
                GuardLogic guard = guards[i];
                if (!IsVulnerable(guard, pose, playerConfig, out float distance, out Vector3 startPosition, out Vector3 exitPosition)) continue;
                if (distance >= bestDistance) continue;

                bestDistance = distance;
                target = guard;
                targetDistance = distance;
                targetStartPosition = startPosition;
                targetExitPosition = exitPosition;
            }
        }

        public bool TryStart(PlayerLogic player)
        {
            if (target == null) return false;

            float startTime = Mathf.Clamp((config.AttackerOffset - targetDistance) / config.ApproachSpeed, 0f, config.MaxStartTime);
            float yaw = target.Yaw;

            player.BeginTakedown(targetStartPosition, yaw, startTime, config.ClipLength, config.EndTime, targetExitPosition, yaw + config.ExitYawOffset);
            target.BeginTakenDown(startTime, config.ClipLength, config.EndTime);
            target = null;
            return true;
        }

        private static bool CanAttack(PlayerPoseState poseState)
        {
            return poseState == PlayerPoseState.Standing || poseState == PlayerPoseState.Crouched;
        }

        private bool IsVulnerable(GuardLogic guard, PlayerPose player, PlayerMovementConfig playerConfig, out float distance, out Vector3 startPosition, out Vector3 exitPosition)
        {
            distance = 0f;
            startPosition = Vector3.zero;
            exitPosition = Vector3.zero;

            if (guard.AlertState != GuardAlertState.Patrol || guard.Vision.CanSeePlayer) return false;

            Vector3 guardPosition = guard.Position;
            Vector3 toPlayer = player.Position - guardPosition;
            if (Mathf.Abs(toPlayer.y) > config.MaxHeightDifference) return false;

            Vector3 flatToPlayer = new Vector3(toPlayer.x, 0f, toPlayer.z);
            distance = flatToPlayer.magnitude;
            if (distance > config.MaxDistance) return false;

            Quaternion facing = Quaternion.Euler(0f, guard.Yaw, 0f);
            Vector3 forward = facing * Vector3.forward;
            if (distance > StealthConstants.MIN_MOVE_DISTANCE && Vector3.Angle(-forward, flatToPlayer) > config.BackHalfAngle) return false;

            float chestHeight = playerConfig.StandingHeight * StealthConstants.TAKEDOWN_CHEST_HEIGHT_RATIO;
            Vector3 guardChest = guardPosition + Vector3.up * chestHeight;
            if (!IsLineClear(player.Position + Vector3.up * chestHeight, guardChest)) return false;

            startPosition = guardPosition - forward * config.AttackerOffset;
            exitPosition = startPosition + facing * new Vector3(config.ExitOffsetSide, 0f, config.ExitOffsetForward);

            return FitsStanding(startPosition, playerConfig)
                && FitsStanding(exitPosition, playerConfig)
                && IsLineClear(startPosition + Vector3.up * chestHeight, guardChest);
        }

        private bool FitsStanding(Vector3 position, PlayerMovementConfig playerConfig)
        {
            float radius = playerConfig.StandingRadius;
            float floorClearance = playerConfig.SkinWidth * StealthConstants.SHAPE_CHECK_FLOOR_CLEARANCE_SKINS;
            Vector3 bottom = position + Vector3.up * (radius + floorClearance);
            Vector3 top = position + Vector3.up * (playerConfig.StandingHeight - radius);

            return !collisionWorld.CheckCapsule(bottom, top, radius);
        }

        private bool IsLineClear(Vector3 from, Vector3 to)
        {
            Vector3 offset = to - from;
            float length = offset.magnitude;
            if (length < StealthConstants.MIN_MOVE_DISTANCE) return true;

            return !collisionWorld.Raycast(from, offset / length, length, out CollisionHit _);
        }
    }
}
