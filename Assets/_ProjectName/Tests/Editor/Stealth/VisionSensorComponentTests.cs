using System.Collections.Generic;
using NUnit.Framework;
using StealthController.Core;
using StealthController.Unity;
using UnityEngine;

public class VisionSensorComponentTests
{
    private const float FIXED_DELTA = 0.02f;
    private const int ENVIRONMENT_LAYER = 31;
    private const float EYE_HEIGHT = 1.6f;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    private PlayerMovementConfig playerConfig;
    private VisionConfig visionConfig;

    [SetUp]
    public void SetUp()
    {
        playerConfig = new PlayerMovementConfig();
        visionConfig = new VisionConfig();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject spawned in spawnedObjects)
            Object.DestroyImmediate(spawned);

        spawnedObjects.Clear();
    }

    [Test]
    public void PlayerInClearView_IsDetectedOnlyAfterConfirmDuration()
    {
        VisionSensorComponent sensor = new VisionSensorComponent(visionConfig, new PlaneCollisionWorldFake());
        PlayerPose player = Standing(new Vector3(0f, 0f, 5f));

        sensor.Tick(FIXED_DELTA, Eye(), Vector3.forward, player);
        Assert.IsTrue(sensor.CanSeePlayer);
        Assert.IsFalse(sensor.HasDetectedPlayer);

        Simulate(sensor, player, visionConfig.ConfirmDuration);
        Assert.IsTrue(sensor.HasDetectedPlayer);
        Assert.AreEqual(player.Position, sensor.LastSeenPosition);
    }

    [Test]
    public void PlayerOutsideConeAngle_IsNotSeen()
    {
        VisionSensorComponent sensor = new VisionSensorComponent(visionConfig, new PlaneCollisionWorldFake());

        sensor.Tick(FIXED_DELTA, Eye(), Vector3.forward, Standing(new Vector3(5f, 0f, 1f)));

        Assert.IsFalse(sensor.CanSeePlayer);
    }

    [Test]
    public void PlayerBeyondRange_IsNotSeen()
    {
        VisionSensorComponent sensor = new VisionSensorComponent(visionConfig, new PlaneCollisionWorldFake());

        sensor.Tick(FIXED_DELTA, Eye(), Vector3.forward, Standing(Vector3.forward * (visionConfig.Range + 1f)));

        Assert.IsFalse(sensor.CanSeePlayer);
    }

    [Test]
    public void WallBetween_BlocksSight()
    {
        PlaneCollisionWorldFake world = new PlaneCollisionWorldFake().AddPlane(Vector3.back, new Vector3(0f, 0f, 3f));
        VisionSensorComponent sensor = new VisionSensorComponent(visionConfig, world);

        sensor.Tick(FIXED_DELTA, Eye(), Vector3.forward, Standing(new Vector3(0f, 0f, 5f)));

        Assert.IsFalse(sensor.CanSeePlayer);
    }

    [Test]
    public void LosingSight_ResetsDetection()
    {
        VisionSensorComponent sensor = new VisionSensorComponent(visionConfig, new PlaneCollisionWorldFake());

        Simulate(sensor, Standing(new Vector3(0f, 0f, 5f)), visionConfig.ConfirmDuration * 2f);
        sensor.Tick(FIXED_DELTA, Eye(), Vector3.forward, Standing(new Vector3(0f, 0f, -5f)));

        Assert.IsFalse(sensor.CanSeePlayer);
        Assert.IsFalse(sensor.HasDetectedPlayer);
    }

    [Test]
    public void LowCover_HidesCrouchedPlayerButNotStandingPlayer()
    {
        GameObject cover = new GameObject("TestLowCover");
        cover.layer = ENVIRONMENT_LAYER;
        cover.transform.position = new Vector3(0f, 0.6f, 4f);
        cover.AddComponent<BoxCollider>().size = new Vector3(2f, 1.2f, 0.6f);
        spawnedObjects.Add(cover);
        Physics.SyncTransforms();

        UnityCollisionWorldAdapter world = new UnityCollisionWorldAdapter(1 << ENVIRONMENT_LAYER, null);
        VisionSensorComponent sensor = new VisionSensorComponent(visionConfig, world);
        Vector3 behindCover = new Vector3(0f, 0f, 4.6f);

        sensor.Tick(FIXED_DELTA, Eye(), Vector3.forward, Crouched(behindCover));
        Assert.IsFalse(sensor.CanSeePlayer);

        sensor.Tick(FIXED_DELTA, Eye(), Vector3.forward, Standing(behindCover));
        Assert.IsTrue(sensor.CanSeePlayer);
    }

    private Vector3 Eye() => Vector3.up * EYE_HEIGHT;

    private void Simulate(VisionSensorComponent sensor, PlayerPose player, float seconds)
    {
        int steps = Mathf.CeilToInt(seconds / FIXED_DELTA);

        for (int i = 0; i < steps; i++)
            sensor.Tick(FIXED_DELTA, Eye(), Vector3.forward, player);
    }

    private PlayerPose Standing(Vector3 position) => Pose(position, PlayerPoseState.Standing, playerConfig.StandingHeight);

    private PlayerPose Crouched(Vector3 position) => Pose(position, PlayerPoseState.Crouched, playerConfig.CrouchedHeight);

    private PlayerPose Pose(Vector3 position, PlayerPoseState state, float height)
    {
        return new PlayerPose(position, Quaternion.identity, state, 0f, 1f, playerConfig.StandingRadius, height, Vector3.zero);
    }
}
