using System.Collections.Generic;
using System;
using StealthController.Core;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine;

namespace StealthController.Unity
{
    public class StealthSessionManager : StealthBehaviourBase
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;

        [Header("Actors")]
        [SerializeField] private PlayerAdapter playerAdapter;
        [SerializeField] private CameraRigAdapter cameraRigAdapter;

        [Header("Mission")]
        [SerializeField, BasicSetting, Tooltip("Checkpoints of the mission. The player comes back at the last one reached.")] private CheckpointAdapter[] checkpoints = Array.Empty<CheckpointAdapter>();
        [SerializeField, BasicSetting, Tooltip("Area that completes the mission.")] private MissionExitAdapter missionExit;

        [Header("Enemies")]
        [SerializeField, BasicSetting, Tooltip("Guards that take part in the mission.")] private GuardAdapter[] guards = Array.Empty<GuardAdapter>();
        [SerializeField, BasicSetting, Tooltip("Surveillance cameras that take part in the mission.")] private SurveillanceCameraAdapter[] surveillanceCameras = Array.Empty<SurveillanceCameraAdapter>();
        [SerializeField] private LayerMask visionOccluderMask = StealthUnityConstants.DEFAULT_ENVIRONMENT_MASK;
        [SerializeField, BasicSetting, Min(0f), Tooltip("Guards within this distance of a surveillance camera answer its alert.")] private float alertRadius = StealthUnityConstants.SURVEILLANCE_ALERT_RADIUS;

        [Header("Takedown")]
        [SerializeField] private TakedownConfig takedownConfig = new TakedownConfig();
        [SerializeField, Tooltip("Geometry that blocks a takedown: the reach line and the space the paired animation needs.")] private LayerMask takedownEnvironmentMask = StealthUnityConstants.DEFAULT_ENVIRONMENT_MASK;

        private readonly List<CheckpointData> checkpointData = new List<CheckpointData>();

        private InputActionsAdapter input;
        private StealthSessionLogic session;
        private PlayerPoseState playerPoseState;
        private GuardLogic takedownTarget;
        private bool isInputSuspended;

        public event Action<StealthSessionOutcome> OutcomeReached;
        public event Action<PlayerPoseState> PlayerPoseStateChanged;
        public event Action<bool> TakedownAvailabilityChanged;

        public PlayerPoseState PlayerPoseState => playerPoseState;
        public InputActionsAdapter Input => input;
        public StealthSessionLogic Logic => session;
        public IReadOnlyList<GuardAdapter> Guards => guards;
        public IReadOnlyList<SurveillanceCameraAdapter> SurveillanceCameras => surveillanceCameras;
        public bool IsInputSuspended => isInputSuspended;

        public bool IsRunning => session != null;
        public bool CanPause => session != null && session.CanPause;
        public bool HasTakedownTarget => takedownTarget != null;
        public Vector3 TakedownTargetHeadPosition => takedownTarget != null ? takedownTarget.EyePosition : Vector3.zero;

        public override void Initialize(params object[] parameters)
        {
            bool hasDependencies = RequireDependency(playerAdapter, nameof(playerAdapter))
                & RequireDependency(cameraRigAdapter, nameof(cameraRigAdapter))
                & RequireDependency(missionExit, nameof(missionExit))
                & RequireDependency(inputActions, nameof(inputActions));

            if (!hasDependencies) return;

            input = new InputActionsAdapter(inputActions);

            if (!input.IsValid)
            {
                Debug.LogError($"{nameof(StealthSessionManager)} on '{name}': '{inputActions.name}' needs a '{StealthInputMap.Player}' and a '{StealthInputMap.UI}' action map.", this);
                input.Dispose();
                input = null;
                return;
            }

            base.Initialize(parameters);

            input.SwitchTo(StealthInputMap.Player);

            playerAdapter.Initialize(input);
            cameraRigAdapter.Initialize();
            MissionLogic missionLogic = BuildMission();
            EnemyCoordinatorLogic enemyCoordinator = BuildEnemies();

            if (!playerAdapter.IsReady || !cameraRigAdapter.IsReady) return;

            TakedownLogic takedownLogic = new TakedownLogic(takedownConfig, new UnityCollisionWorldAdapter(takedownEnvironmentMask, null));
            session = new StealthSessionLogic(playerAdapter.Logic, cameraRigAdapter.Logic, enemyCoordinator, missionLogic, takedownLogic);
            session.Start();
            SnapAfterRespawn();
        }

        public override void Conclude()
        {
            session = null;
            isInputSuspended = false;
            PublishTakedownTarget(null);

            base.Conclude();

            foreach (GuardAdapter guard in guards)
            {
                if (guard != null)
                    guard.Conclude();
            }

            foreach (SurveillanceCameraAdapter surveillanceCamera in surveillanceCameras)
            {
                if (surveillanceCamera != null)
                    surveillanceCamera.Conclude();
            }

            if (cameraRigAdapter != null)
                cameraRigAdapter.Conclude();

            if (playerAdapter != null)
                playerAdapter.Conclude();

            if (input == null) return;

            input.Dispose();
            input = null;
        }

        public void SetPaused(bool paused)
        {
            if (session == null || !session.SetPaused(paused)) return;

            PublishTakedownTarget(paused ? null : session.Takedown.Target);
            ApplyInputMap();
        }

        public void SetInputSuspended(bool suspended)
        {
            isInputSuspended = suspended;
            ApplyInputMap();
        }

        public bool RespawnAtCheckpoint(int rank)
        {
            if (session == null || !session.RespawnAtCheckpoint(rank)) return false;

            PublishTakedownTarget(null);
            SnapEnemyPoses();
            SnapAfterRespawn();
            return true;
        }

        public bool TeleportPlayerToExit()
        {
            if (session == null || !session.TeleportPlayerToExit()) return false;

            PublishTakedownTarget(null);
            SnapAfterRespawn();
            return true;
        }

        public void SetPlayerDetectable(bool detectable)
        {
            if (session == null) return;

            session.SetPlayerDetectable(detectable);
        }

        public void RestartMission()
        {
            if (session == null) return;

            session.RestartMission();
            ApplyInputMap();
            PublishTakedownTarget(null);
            SnapEnemyPoses();
            SnapAfterRespawn();
        }

        public void ResolvePendingOutcome()
        {
            if (session == null || !session.ResolvePendingOutcome()) return;

            SnapEnemyPoses();
            SnapAfterRespawn();
        }

        private void Update()
        {
            if (session == null || session.IsFrozen) return;

            float deltaTime = Time.deltaTime;
            StealthSessionOutcome outcome = session.Tick(deltaTime, playerAdapter.ConsumeInput());

            playerAdapter.ApplyPose(deltaTime);
            PublishPlayerPoseState();
            ApplyEnemyPoses(deltaTime);
            PublishTakedownTarget(session.Takedown.Target);

            if (outcome != StealthSessionOutcome.None)
            {
                ReachOutcome(outcome);
                return;
            }

            cameraRigAdapter.ApplyFraming(session.Player.Pose, deltaTime);
        }

        private void ApplyInputMap()
        {
            if (input == null) return;

            if (isInputSuspended)
            {
                input.DisableAll();
                return;
            }

            input.SwitchTo(session != null && session.IsPaused ? StealthInputMap.UI : StealthInputMap.Player);
        }

        private void ReachOutcome(StealthSessionOutcome outcome)
        {
            if (OutcomeReached == null)
            {
                ResolvePendingOutcome();
                return;
            }

            OutcomeReached.Invoke(outcome);
        }

        private MissionLogic BuildMission()
        {
            checkpointData.Clear();

            foreach (CheckpointAdapter checkpoint in checkpoints)
            {
                if (checkpoint != null)
                    checkpointData.Add(checkpoint.ToCheckpointData());
            }

            MissionLogic missionLogic = new MissionLogic();
            missionLogic.SetCheckpoints(checkpointData);
            missionLogic.SetExit(missionExit.ToBounds());

            if (!missionLogic.HasActiveCheckpoint)
                Debug.LogError("StealthSessionManager has no checkpoints; the player keeps its scene position.", this);

            return missionLogic;
        }

        private EnemyCoordinatorLogic BuildEnemies()
        {
            UnityCollisionWorldAdapter visionWorld = new UnityCollisionWorldAdapter(visionOccluderMask, null);
            NavMeshPathfinderAdapter pathfinder = new NavMeshPathfinderAdapter(NavMesh.AllAreas);
            EnemyCoordinatorLogic enemyCoordinator = new EnemyCoordinatorLogic(alertRadius);

            foreach (GuardAdapter guard in guards)
            {
                if (guard == null) continue;

                guard.Initialize(visionWorld, pathfinder);

                if (guard.IsReady)
                    enemyCoordinator.AddGuard(guard.Logic);
            }

            foreach (SurveillanceCameraAdapter surveillanceCamera in surveillanceCameras)
            {
                if (surveillanceCamera == null) continue;

                surveillanceCamera.Initialize(visionWorld);

                if (surveillanceCamera.IsReady)
                    enemyCoordinator.AddSurveillanceCamera(surveillanceCamera.Logic);
            }

            return enemyCoordinator;
        }

        private void ApplyEnemyPoses(float deltaTime)
        {
            foreach (GuardAdapter guard in guards)
            {
                if (guard != null)
                    guard.ApplyPose(deltaTime);
            }

            ApplySurveillanceCameraPoses();
        }

        private void SnapEnemyPoses()
        {
            foreach (GuardAdapter guard in guards)
            {
                if (guard != null)
                    guard.SnapPose();
            }

            ApplySurveillanceCameraPoses();
        }

        private void ApplySurveillanceCameraPoses()
        {
            foreach (SurveillanceCameraAdapter surveillanceCamera in surveillanceCameras)
            {
                if (surveillanceCamera != null)
                    surveillanceCamera.ApplyPose();
            }
        }

        private void SnapAfterRespawn()
        {
            playerAdapter.SnapPose();
            cameraRigAdapter.ApplyFraming(session.Player.Pose, 0f);
            PublishPlayerPoseState();
        }

        private void PublishTakedownTarget(GuardLogic target)
        {
            bool wasAvailable = takedownTarget != null;
            takedownTarget = target;

            if (wasAvailable != (target != null))
                TakedownAvailabilityChanged?.Invoke(target != null);
        }

        private void PublishPlayerPoseState()
        {
            PlayerPoseState currentPoseState = session.Player.PoseState;
            if (currentPoseState == playerPoseState) return;

            playerPoseState = currentPoseState;
            PlayerPoseStateChanged?.Invoke(playerPoseState);
        }
    }
}
