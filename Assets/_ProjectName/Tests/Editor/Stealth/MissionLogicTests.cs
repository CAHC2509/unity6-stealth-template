using System.Collections.Generic;
using NUnit.Framework;
using StealthController.Core;
using StealthController.Unity;
using UnityEngine;

public class MissionLogicTests
{
    private const int START_ORDER = 0;
    private const int MIDDLE_ORDER = 1;
    private const int LATE_ORDER = 2;
    private const float START_YAW = 90f;

    private PlayerMovementConfig config;

    [SetUp]
    public void SetUp()
    {
        config = new PlayerMovementConfig();
    }

    [Test]
    public void Restart_ActivatesLowestOrderCheckpoint()
    {
        MissionLogic mission = CreateMission();

        Assert.IsTrue(mission.HasActiveCheckpoint);
        Assert.AreEqual(START_ORDER, mission.ActiveCheckpoint.Order);
        Assert.AreEqual(START_YAW, mission.ActiveCheckpoint.SpawnYaw);
    }

    [Test]
    public void EnteringHigherOrderCheckpoint_ActivatesIt()
    {
        MissionLogic mission = CreateMission();

        mission.Tick(PoseAt(new Vector3(20f, 0f, 0f)));

        Assert.AreEqual(LATE_ORDER, mission.ActiveCheckpoint.Order);
    }

    [Test]
    public void ReturningToLowerOrderCheckpoint_KeepsActiveCheckpoint()
    {
        MissionLogic mission = CreateMission();

        mission.Tick(PoseAt(new Vector3(20f, 0f, 0f)));
        mission.Tick(PoseAt(new Vector3(10f, 0f, 0f)));
        mission.Tick(PoseAt(Vector3.zero));

        Assert.AreEqual(LATE_ORDER, mission.ActiveCheckpoint.Order);
    }

    [Test]
    public void EnteringExit_ReportsExitUntilRestart()
    {
        MissionLogic mission = CreateMission();

        mission.Tick(PoseAt(new Vector3(20f, 0f, 0f)));
        mission.Tick(PoseAt(new Vector3(30f, 0f, 0f)));
        Assert.IsTrue(mission.HasReachedExit);

        mission.Restart();

        Assert.IsFalse(mission.HasReachedExit);
        Assert.AreEqual(START_ORDER, mission.ActiveCheckpoint.Order);
    }

    [Test]
    public void MissionWithoutCheckpoints_HasNoActiveCheckpoint()
    {
        MissionLogic mission = new MissionLogic();

        mission.SetCheckpoints(new List<CheckpointData>());

        Assert.IsFalse(mission.HasActiveCheckpoint);
    }

    private MissionLogic CreateMission()
    {
        MissionLogic mission = new MissionLogic();

        mission.SetCheckpoints(new List<CheckpointData>
        {
            Checkpoint(new Vector3(10f, 0f, 0f), MIDDLE_ORDER, 0f),
            Checkpoint(new Vector3(20f, 0f, 0f), LATE_ORDER, 0f),
            Checkpoint(Vector3.zero, START_ORDER, START_YAW)
        });
        mission.SetExit(Volume(new Vector3(30f, 0f, 0f)));
        return mission;
    }

    private CheckpointData Checkpoint(Vector3 spawn, int order, float yaw) => new CheckpointData(order, Volume(spawn), spawn, yaw);

    private Bounds Volume(Vector3 floorCenter)
    {
        Vector3 size = new Vector3(
            StealthUnityConstants.MISSION_VOLUME_DEFAULT_WIDTH,
            StealthUnityConstants.MISSION_VOLUME_DEFAULT_HEIGHT,
            StealthUnityConstants.MISSION_VOLUME_DEFAULT_DEPTH);

        return new Bounds(floorCenter + Vector3.up * (size.y * 0.5f), size);
    }

    private PlayerPose PoseAt(Vector3 position)
    {
        return new PlayerPose(
            position,
            Quaternion.identity,
            PlayerPoseState.Standing,
            0f,
            1f,
            config.StandingRadius,
            config.StandingHeight,
            Vector3.zero);
    }
}
