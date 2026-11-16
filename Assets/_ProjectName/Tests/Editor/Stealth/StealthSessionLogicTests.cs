using System.Collections.Generic;
using NUnit.Framework;
using StealthController.Core;
using UnityEngine;

public class StealthSessionLogicTests
{
    private const float FIXED_DELTA = 0.02f;
    private const float ALERT_RADIUS = 10f;
    private const float POSITION_TOLERANCE = 0.05f;
    private const int START_ORDER = 0;
    private const int LATE_ORDER = 1;

    private PlaneCollisionWorldFake world;
    private PlayerLogic player;
    private MissionLogic mission;
    private StealthSessionLogic session;

    [SetUp]
    public void SetUp()
    {
        world = new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero);
        player = new PlayerLogic(new PlayerMovementConfig(), world);
        mission = new MissionLogic();
        mission.SetCheckpoints(new List<CheckpointData>
        {
            new CheckpointData(START_ORDER, new Bounds(new Vector3(0f, 1f, 0f), new Vector3(3f, 2f, 3f)), Vector3.zero, 0f),
            new CheckpointData(LATE_ORDER, new Bounds(new Vector3(10f, 1f, 0f), new Vector3(3f, 2f, 3f)), new Vector3(10f, 0f, 0f), 90f)
        });
        mission.SetExit(new Bounds(new Vector3(30f, 1f, 0f), new Vector3(3f, 2f, 3f)));

        session = new StealthSessionLogic(player, new CameraRigLogic(new CameraRigConfig()), new EnemyCoordinatorLogic(ALERT_RADIUS), mission, new TakedownLogic(new TakedownConfig(), world));
        session.Start();
    }

    [Test]
    public void RestartMission_UnpausesAndRespawnsAtTheFirstCheckpoint()
    {
        player.Teleport(new Vector3(10f, 0f, 0f), 0f);
        session.Tick(FIXED_DELTA, new PlayerInputFrame(Vector2.zero, false, false, false));
        Assert.AreEqual(LATE_ORDER, mission.ActiveCheckpoint.Order);

        session.SetPaused(true);
        session.RestartMission();

        Assert.IsFalse(session.IsPaused);
        Assert.IsFalse(session.IsFrozen);
        Assert.AreEqual(START_ORDER, mission.ActiveCheckpoint.Order);
        Assert.AreEqual(0f, player.Motor.Position.x, POSITION_TOLERANCE);
        Assert.AreEqual(0f, player.Motor.Position.z, POSITION_TOLERANCE);
    }
}
