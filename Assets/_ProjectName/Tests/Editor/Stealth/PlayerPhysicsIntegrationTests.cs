using System.Collections.Generic;
using NUnit.Framework;
using StealthController.Core;
using StealthController.Unity;
using UnityEngine;

public class PlayerPhysicsIntegrationTests
{
    private const float FIXED_DELTA = 0.02f;
    private const int ENVIRONMENT_LAYER = 31;
    private const int PROBE_LAYER = 2;
    private const float SETTLE_DURATION = 0.2f;
    private const float POSTURE_SETTLE_DURATION = 0.3f;
    private const float HOLD_MARGIN = 0.1f;
    private const float WALK_DURATION = 2.5f;
    private const float BEAM_WALK_DURATION = 3f;
    private const float BEAM_CRAWL_DURATION = 5f;
    private const float UNDER_BEAM_CRAWL_DURATION = 3.3f;
    private const float WALL_APPROACH_DURATION = 1f;
    private const float CRAWL_APPROACH_DURATION = 2f;
    private const float CROUCH_APPROACH_DURATION = 1.5f;
    private const float WALL_PRESS_DURATION = 0.5f;
    private const float SHUFFLE_DURATION = 2f;
    private const float APPROACH_EDGE_DURATION = 0.5f;
    private const float HEIGHT_TOLERANCE = 0.05f;
    private const float POSITION_TOLERANCE = 0.05f;
    private const float MIN_NORMAL_ALIGNMENT = 0.9f;
    private const float MIN_TRAVEL_PAST_CORNER = 0.5f;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    private UnityCollisionWorldAdapter collisionWorld;
    private PlayerMovementConfig config;

    [SetUp]
    public void SetUp()
    {
        config = new PlayerMovementConfig();

        SpawnBox(new Vector3(0f, -0.1f, 0f), new Vector3(20f, 0.2f, 20f));

        GameObject probeObject = new GameObject("TestCollisionProbe");
        probeObject.layer = PROBE_LAYER;
        CapsuleCollider probe = probeObject.AddComponent<CapsuleCollider>();
        probe.isTrigger = true;
        spawnedObjects.Add(probeObject);

        collisionWorld = new UnityCollisionWorldAdapter(1 << ENVIRONMENT_LAYER, probe);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject spawned in spawnedObjects)
            Object.DestroyImmediate(spawned);

        spawnedObjects.Clear();
    }

    [Test]
    public void Standing_ClimbsStepLowerThanStepOffset()
    {
        SpawnBox(new Vector3(0f, 0.125f, 3f), new Vector3(2f, 0.25f, 2f));
        PlayerLogic player = CreatePlayer(new Vector3(0f, 0f, 0f));

        Simulate(player, Walk(), WALK_DURATION);

        Assert.IsTrue(player.Motor.IsGrounded);
        Assert.AreEqual(0.25f, player.Motor.Position.y, HEIGHT_TOLERANCE);
        Assert.Greater(player.Motor.Position.z, 2.5f);
    }

    [Test]
    public void Standing_IsBlockedByObstacleTallerThanStepOffset()
    {
        SpawnBox(new Vector3(0f, 0.5f, 3f), new Vector3(2f, 1f, 2f));
        PlayerLogic player = CreatePlayer(new Vector3(0f, 0f, 0f));

        Simulate(player, Walk(), WALK_DURATION);

        Assert.AreEqual(0f, player.Motor.Position.y, HEIGHT_TOLERANCE);
        Assert.AreEqual(2f - config.StandingRadius, player.Motor.Position.z, POSITION_TOLERANCE);
    }

    [Test]
    public void Crawling_PassesUnderLowBeamThatBlocksStanding()
    {
        SpawnBox(new Vector3(0f, 0.75f, 3f), new Vector3(4f, 0.3f, 2f));

        PlayerLogic standing = CreatePlayer(new Vector3(0f, 0f, 0f));
        Simulate(standing, Walk(), BEAM_WALK_DURATION);
        Assert.Less(standing.Motor.Position.z, 2f);

        PlayerLogic crawling = CreatePlayer(new Vector3(0f, 0f, 0f));
        EnterCrawl(crawling);
        Simulate(crawling, Walk(), BEAM_CRAWL_DURATION);
        Assert.Greater(crawling.Motor.Position.z, 4f);
    }

    [Test]
    public void Crawling_CannotStandUpUnderLowBeam()
    {
        SpawnBox(new Vector3(0f, 0.75f, 3f), new Vector3(4f, 0.3f, 2f));
        PlayerLogic player = CreatePlayer(new Vector3(0f, 0f, 0f));

        EnterCrawl(player);
        Simulate(player, Walk(), UNDER_BEAM_CRAWL_DURATION);
        player.Tick(FIXED_DELTA, CrouchPress(), 0f);

        Assert.Greater(player.Motor.Position.z, 2f);
        Assert.Less(player.Motor.Position.z, 4f);
        Assert.AreEqual(PlayerPoseState.Crawling, player.PoseState);
    }

    [Test]
    public void LowCover_NeverAttaches()
    {
        SpawnBox(new Vector3(0f, 0.5f, 1.3f), new Vector3(4f, 1f, 0.6f));

        PlayerLogic standing = CreatePlayer(new Vector3(0f, 0f, 0.5f));
        Simulate(standing, Walk(), WALL_APPROACH_DURATION);
        Assert.AreEqual(PlayerPoseState.Standing, standing.PoseState);

        PlayerLogic crawling = CreatePlayer(new Vector3(0f, 0f, 0f));
        EnterCrawl(crawling);
        Simulate(crawling, Walk(), CRAWL_APPROACH_DURATION);
        Assert.AreEqual(PlayerPoseState.Crawling, crawling.PoseState);

        PlayerLogic crouched = CreatePlayer(new Vector3(0f, 0f, 0f));
        EnterCrouch(crouched);
        Simulate(crouched, Walk(), CROUCH_APPROACH_DURATION);
        Assert.AreEqual(PlayerPoseState.Crouched, crouched.PoseState);
    }

    [Test]
    public void PressingAlongNextFaceNearOuterCorner_TurnsTheCorner()
    {
        SpawnBox(new Vector3(0f, 1.5f, 2f), new Vector3(4f, 3f, 2f));
        PlayerLogic player = CreatePlayer(new Vector3(1f, 0f, 0.5f));

        Simulate(player, Walk(), WALL_PRESS_DURATION);
        Simulate(player, Move(Vector2.right), APPROACH_EDGE_DURATION);
        Assert.AreEqual(PlayerPoseState.WallStanding, player.PoseState);

        Simulate(player, Move(Vector2.up), SHUFFLE_DURATION);

        Assert.AreEqual(PlayerPoseState.WallStanding, player.PoseState);
        Assert.Greater(Vector3.Dot(player.WallCover.Normal, Vector3.right), MIN_NORMAL_ALIGNMENT);
        Assert.AreEqual(2f + config.StandingRadius + config.SkinWidth, player.Motor.Position.x, POSITION_TOLERANCE);
        Assert.Greater(player.Motor.Position.z, 1f + MIN_TRAVEL_PAST_CORNER);
    }

    [Test]
    public void KeepPushingPastOuterEdge_LeavesTheWall()
    {
        SpawnBox(new Vector3(0f, 1.5f, 2f), new Vector3(4f, 3f, 2f));
        PlayerLogic player = CreatePlayer(new Vector3(1f, 0f, 0.5f));

        Simulate(player, Walk(), WALL_PRESS_DURATION);
        Simulate(player, Move(Vector2.right), SHUFFLE_DURATION);

        Assert.AreEqual(PlayerPoseState.Standing, player.PoseState);
        Assert.Greater(player.Motor.Position.x, 2f + config.StandingRadius);
    }

    [Test]
    public void PushingIntoInnerCorner_TransfersToSideWallAndFollowsInput()
    {
        SpawnBox(new Vector3(0f, 1.5f, 1.5f), new Vector3(6f, 3f, 1f));
        SpawnBox(new Vector3(2f, 1.5f, 0f), new Vector3(1f, 3f, 4f));
        PlayerLogic player = CreatePlayer(new Vector3(0f, 0f, 0.5f));

        Simulate(player, Walk(), WALL_PRESS_DURATION);
        Simulate(player, Move(Vector2.right), SHUFFLE_DURATION);

        Assert.AreEqual(PlayerPoseState.WallStanding, player.PoseState);
        Assert.Greater(Vector3.Dot(player.WallCover.Normal, Vector3.left), MIN_NORMAL_ALIGNMENT);
        Assert.AreEqual(1.5f - config.StandingRadius - config.SkinWidth, player.Motor.Position.x, POSITION_TOLERANCE);
        float cornerZ = player.Motor.Position.z;

        Simulate(player, Move(Vector2.down), SHUFFLE_DURATION);

        Assert.AreEqual(PlayerPoseState.WallStanding, player.PoseState);
        Assert.Less(player.Motor.Position.z, cornerZ - MIN_TRAVEL_PAST_CORNER);
    }

    private void SpawnBox(Vector3 center, Vector3 size)
    {
        GameObject box = new GameObject("TestBox");
        box.layer = ENVIRONMENT_LAYER;
        box.transform.position = center;
        box.AddComponent<BoxCollider>().size = size;
        spawnedObjects.Add(box);
        Physics.SyncTransforms();
    }

    private PlayerLogic CreatePlayer(Vector3 start)
    {
        PlayerLogic player = new PlayerLogic(config, collisionWorld);
        player.Teleport(start, 0f);
        Simulate(player, Idle(), SETTLE_DURATION);
        return player;
    }

    private void EnterCrawl(PlayerLogic player)
    {
        player.Tick(FIXED_DELTA, CrouchPress(), 0f);
        Simulate(player, CrouchHold(), config.CrouchHoldToProneDuration + HOLD_MARGIN);
        player.Tick(FIXED_DELTA, CrouchRelease(), 0f);
        Simulate(player, Idle(), POSTURE_SETTLE_DURATION);
    }

    private void EnterCrouch(PlayerLogic player)
    {
        player.Tick(FIXED_DELTA, CrouchPress(), 0f);
        player.Tick(FIXED_DELTA, CrouchRelease(), 0f);
        Simulate(player, Idle(), POSTURE_SETTLE_DURATION);
    }

    private void Simulate(PlayerLogic player, PlayerInputFrame input, float seconds)
    {
        int steps = Mathf.RoundToInt(seconds / FIXED_DELTA);

        for (int i = 0; i < steps; i++)
            player.Tick(FIXED_DELTA, input, 0f);
    }

    private PlayerInputFrame Idle() => new PlayerInputFrame(Vector2.zero, false, false, false);
    private PlayerInputFrame CrouchPress() => new PlayerInputFrame(Vector2.zero, false, false, true, true, false);
    private PlayerInputFrame CrouchHold() => new PlayerInputFrame(Vector2.zero, false, false, false, true, false);
    private PlayerInputFrame CrouchRelease() => new PlayerInputFrame(Vector2.zero, false, false, false, false, true);
    private PlayerInputFrame Walk() => new PlayerInputFrame(Vector2.up, false, false, false);
    private PlayerInputFrame Move(Vector2 move) => new PlayerInputFrame(move, false, false, false);
}
