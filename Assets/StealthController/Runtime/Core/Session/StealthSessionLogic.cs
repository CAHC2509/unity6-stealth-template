using UnityEngine;

namespace StealthController.Core
{
    public class StealthSessionLogic
    {
        private readonly PlayerLogic player;
        private readonly CameraRigLogic cameraRig;
        private readonly EnemyCoordinatorLogic enemies;
        private readonly MissionLogic mission;
        private readonly TakedownLogic takedown;

        private bool isPaused;
        private StealthSessionOutcome pendingOutcome;

        public PlayerLogic Player => player;
        public CameraRigLogic CameraRig => cameraRig;
        public EnemyCoordinatorLogic Enemies => enemies;
        public MissionLogic Mission => mission;
        public TakedownLogic Takedown => takedown;
        public bool IsPaused => isPaused;
        public StealthSessionOutcome PendingOutcome => pendingOutcome;
        public bool IsFrozen => isPaused || pendingOutcome != StealthSessionOutcome.None;
        public bool CanPause => !IsFrozen;

        public StealthSessionLogic(PlayerLogic player, CameraRigLogic cameraRig, EnemyCoordinatorLogic enemies, MissionLogic mission, TakedownLogic takedown)
        {
            this.player = player;
            this.cameraRig = cameraRig;
            this.enemies = enemies;
            this.mission = mission;
            this.takedown = takedown;
        }

        public void Start()
        {
            isPaused = false;
            pendingOutcome = StealthSessionOutcome.None;
            RespawnAtActiveCheckpoint();
        }

        public bool SetPaused(bool paused)
        {
            if (isPaused == paused) return false;

            isPaused = paused;
            return true;
        }

        public StealthSessionOutcome Tick(float deltaTime, PlayerInputFrame input)
        {
            if (IsFrozen) return StealthSessionOutcome.None;

            if (input.TakedownPressed)
                takedown.TryStart(player);

            player.Tick(deltaTime, input, cameraRig.InputReferenceYaw);
            PlayerPose playerPose = player.Pose;

            enemies.Tick(deltaTime, playerPose);

            if (enemies.HasCapturedPlayer)
                return ReachOutcome(StealthSessionOutcome.Captured);

            mission.Tick(playerPose);

            if (mission.HasReachedExit)
                return ReachOutcome(StealthSessionOutcome.MissionCompleted);

            cameraRig.Tick(deltaTime, playerPose);
            takedown.UpdateTarget(player, enemies.Guards);
            return StealthSessionOutcome.None;
        }

        public bool ResolvePendingOutcome()
        {
            StealthSessionOutcome outcome = pendingOutcome;
            if (outcome == StealthSessionOutcome.None) return false;

            pendingOutcome = StealthSessionOutcome.None;

            if (outcome == StealthSessionOutcome.MissionCompleted)
                mission.Restart();

            enemies.ResetAll();
            RespawnAtActiveCheckpoint();
            return true;
        }

        public void RestartMission()
        {
            isPaused = false;
            pendingOutcome = StealthSessionOutcome.None;
            takedown.Clear();
            mission.Restart();
            enemies.ResetAll();
            RespawnAtActiveCheckpoint();
        }

        public bool RespawnAtCheckpoint(int rank)
        {
            if (!mission.TryActivateCheckpointByRank(rank)) return false;

            pendingOutcome = StealthSessionOutcome.None;
            takedown.Clear();
            enemies.ResetAll();
            RespawnAtActiveCheckpoint();
            return true;
        }

        public bool TeleportPlayerToExit()
        {
            if (!mission.HasExit || IsFrozen) return false;

            Bounds exitBounds = mission.ExitBounds;
            Vector3 exitFloor = new Vector3(exitBounds.center.x, exitBounds.min.y, exitBounds.center.z);

            takedown.Clear();
            player.Teleport(exitFloor, player.Facing.Yaw);
            cameraRig.Snap(player.Pose);
            return true;
        }

        public void SetPlayerDetectable(bool detectable)
        {
            enemies.SetPlayerDetectable(detectable);
        }

        private StealthSessionOutcome ReachOutcome(StealthSessionOutcome outcome)
        {
            takedown.Clear();
            pendingOutcome = outcome;
            return outcome;
        }

        private void RespawnAtActiveCheckpoint()
        {
            if (mission.HasActiveCheckpoint)
            {
                CheckpointData checkpoint = mission.ActiveCheckpoint;
                player.Teleport(checkpoint.SpawnPosition, checkpoint.SpawnYaw);
            }

            cameraRig.Snap(player.Pose);
        }
    }
}
