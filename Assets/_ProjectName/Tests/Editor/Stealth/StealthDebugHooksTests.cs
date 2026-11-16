using System.Collections.Generic;
using NUnit.Framework;
using StealthController.Core;
using UnityEngine;

public class StealthDebugHooksTests
{
    private const float FIXED_DELTA = 0.02f;
    private const float ALERT_RADIUS = 10f;
    private const float POSITION_TOLERANCE = 0.05f;
    private const int FIRST_ORDER = 0;
    private const int SECOND_ORDER = 5;
    private const int THIRD_ORDER = 9;
    private const int SIGHT_STEPS = 10;

    private static readonly Vector3 SecondSpawn = new Vector3(10f, 0f, 0f);
    private static readonly Vector3 ThirdSpawn = new Vector3(20f, 0f, 0f);
    private static readonly Vector3 ExitCenter = new Vector3(30f, 0f, 0f);
    private static readonly Vector3 VolumeSize = new Vector3(3f, 2f, 3f);

    private PlaneCollisionWorldFake world;
    private PlayerLogic player;
    private MissionLogic mission;
    private EnemyCoordinatorLogic enemies;
    private StealthSessionLogic session;

    [SetUp]
    public void SetUp()
    {
        world = new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero);
        player = new PlayerLogic(new PlayerMovementConfig(), world);
        mission = new MissionLogic();
        mission.SetCheckpoints(new List<CheckpointData>
        {
            Checkpoint(THIRD_ORDER, ThirdSpawn),
            Checkpoint(FIRST_ORDER, Vector3.zero),
            Checkpoint(SECOND_ORDER, SecondSpawn)
        });
        mission.SetExit(new Bounds(ExitCenter + Vector3.up * (VolumeSize.y * 0.5f), VolumeSize));

        enemies = new EnemyCoordinatorLogic(ALERT_RADIUS);
        session = new StealthSessionLogic(player, new CameraRigLogic(new CameraRigConfig()), enemies, mission, new TakedownLogic(new TakedownConfig(), world));
        session.Start();
    }

    [Test]
    public void TryActivateCheckpointByRank_FollowsCheckpointOrderNotListOrder()
    {
        Assert.IsTrue(mission.TryActivateCheckpointByRank(1));
        Assert.AreEqual(SECOND_ORDER, mission.ActiveCheckpoint.Order);
        Assert.AreEqual(1, mission.ActiveCheckpointRank);

        Assert.IsTrue(mission.TryActivateCheckpointByRank(2));
        Assert.AreEqual(THIRD_ORDER, mission.ActiveCheckpoint.Order);
    }

    [Test]
    public void TryActivateCheckpointByRank_OutOfRange_KeepsActiveCheckpoint()
    {
        Assert.IsFalse(mission.TryActivateCheckpointByRank(-1));
        Assert.IsFalse(mission.TryActivateCheckpointByRank(mission.CheckpointCount));
        Assert.AreEqual(FIRST_ORDER, mission.ActiveCheckpoint.Order);
    }

    [Test]
    public void RespawnAtCheckpoint_MovesPlayerToThatCheckpoint()
    {
        Assert.IsTrue(session.RespawnAtCheckpoint(2));

        Assert.AreEqual(ThirdSpawn.x, player.Motor.Position.x, POSITION_TOLERANCE);
        Assert.AreEqual(ThirdSpawn.z, player.Motor.Position.z, POSITION_TOLERANCE);
        Assert.IsFalse(session.IsFrozen);
    }

    [Test]
    public void TeleportPlayerToExit_CompletesMissionOnNextTick()
    {
        Assert.IsTrue(session.TeleportPlayerToExit());

        StealthSessionOutcome outcome = session.Tick(FIXED_DELTA, new PlayerInputFrame(Vector2.zero, false, false, false));

        Assert.AreEqual(StealthSessionOutcome.MissionCompleted, outcome);
    }

    [Test]
    public void SetPlayerDetectable_False_GuardsStopSeeingThePlayer()
    {
        GuardLogic guard = CreateGuardFacingPlayer();
        enemies.AddGuard(guard);

        PlayerPose visiblePose = PoseInFrontOfGuard();
        TickEnemies(visiblePose);
        Assert.IsTrue(guard.Vision.CanSeePlayer);

        session.SetPlayerDetectable(false);
        TickEnemies(visiblePose);

        Assert.IsFalse(enemies.IsPlayerDetectable);
        Assert.IsFalse(guard.Vision.CanSeePlayer);
        Assert.IsFalse(enemies.HasCapturedPlayer);

        session.SetPlayerDetectable(true);
        TickEnemies(visiblePose);

        Assert.IsTrue(guard.Vision.CanSeePlayer);
    }

    [Test]
    public void GuardAddedWhileUndetectable_IgnoresThePlayer()
    {
        session.SetPlayerDetectable(false);

        GuardLogic guard = CreateGuardFacingPlayer();
        enemies.AddGuard(guard);
        TickEnemies(PoseInFrontOfGuard());

        Assert.IsTrue(guard.Vision.IgnoresPlayer);
        Assert.IsFalse(guard.Vision.CanSeePlayer);
    }

    private void TickEnemies(PlayerPose pose)
    {
        for (int i = 0; i < SIGHT_STEPS; i++)
            enemies.Tick(FIXED_DELTA, pose);
    }

    private GuardLogic CreateGuardFacingPlayer()
    {
        List<GuardWaypoint> route = new List<GuardWaypoint> { new GuardWaypoint(Vector3.zero, 0f, false, 0f) };
        return new GuardLogic(new GuardConfig(), route, Vector3.zero, 0f, new PlaneCollisionWorldFake(), new StraightLinePathfinderFake());
    }

    private PlayerPose PoseInFrontOfGuard()
    {
        PlayerMovementConfig config = new PlayerMovementConfig();

        return new PlayerPose(
            new Vector3(0f, 0f, 4f),
            Quaternion.identity,
            PlayerPoseState.Standing,
            0f,
            1f,
            config.StandingRadius,
            config.StandingHeight,
            Vector3.zero);
    }

    private static CheckpointData Checkpoint(int order, Vector3 spawn)
    {
        return new CheckpointData(order, new Bounds(spawn + Vector3.up * (VolumeSize.y * 0.5f), VolumeSize), spawn, 0f);
    }
}
