using NUnit.Framework;
using StealthController.Core;
using UnityEngine;

public class PlayerWallTests
{
    private const float FIXED_DELTA = 0.02f;
    private const float WALL_Z = 1f;
    private const float PUSH_MARGIN = 0.1f;
    private const float SETTLE_DURATION = 0.5f;
    private const float SHORT_SETTLE_DURATION = 0.1f;
    private const float LONG_PUSH_DURATION = 1f;
    private const float SHUFFLE_DURATION = 1f;
    private const float WALL_IDLE_DURATION = 1f;
    private const float POSITION_TOLERANCE = 0.02f;
    private const float SHUFFLE_DISTANCE_TOLERANCE = 0.1f;
    private const float YAW_TOLERANCE = 1f;
    private const float RUN_SPEED_TOLERANCE = 0.05f;

    private PlayerMovementConfig config;

    [SetUp]
    public void SetUp()
    {
        config = new PlayerMovementConfig();
    }

    [Test]
    public void PushingIntoWall_EntersWallAfterPushDuration()
    {
        PlayerLogic player = CreateFacingWall();

        Simulate(player, Move(Vector2.up), config.WallPushDuration + PUSH_MARGIN);
        Simulate(player, Idle(), SETTLE_DURATION);

        Assert.AreEqual(PlayerPoseState.WallStanding, player.PoseState);
        Assert.AreEqual(StealthConstants.HALF_TURN_DEGREES, Mathf.Repeat(player.Facing.Yaw, StealthConstants.HALF_TURN_DEGREES * 2f), YAW_TOLERANCE);
        Assert.AreEqual(ContactZ(), player.Motor.Position.z, POSITION_TOLERANCE);
        Assert.AreEqual(Vector3.back, player.WallCover.Normal);
    }

    [Test]
    public void BriefPush_DoesNotEnterWall()
    {
        PlayerLogic player = CreateFacingWall();

        Simulate(player, Move(Vector2.up), config.WallPushDuration * 0.5f);

        Assert.AreEqual(PlayerPoseState.Standing, player.PoseState);
    }

    [Test]
    public void DiagonalPush_DoesNotEnterWall()
    {
        PlayerLogic player = CreateFacingWall();

        Simulate(player, Move((Vector2.up + Vector2.right).normalized), LONG_PUSH_DURATION);

        Assert.AreEqual(PlayerPoseState.Standing, player.PoseState);
    }

    [Test]
    public void LateralInputOnWall_ShufflesAlongWall()
    {
        PlayerLogic player = CreateOnWall();
        float startX = player.Motor.Position.x;

        Simulate(player, Move(Vector2.right), SHUFFLE_DURATION);

        Assert.AreEqual(PlayerPoseState.WallStanding, player.PoseState);
        Assert.AreEqual(startX + config.WallShuffleStandingSpeed * SHUFFLE_DURATION, player.Motor.Position.x, SHUFFLE_DISTANCE_TOLERANCE);
        Assert.AreEqual(ContactZ(), player.Motor.Position.z, POSITION_TOLERANCE);
    }

    [Test]
    public void ReleasingInputOnWall_StaysOnWall()
    {
        PlayerLogic player = CreateOnWall();

        Simulate(player, Idle(), WALL_IDLE_DURATION);

        Assert.AreEqual(PlayerPoseState.WallStanding, player.PoseState);
    }

    [Test]
    public void InputAwayFromWall_LeavesWall()
    {
        PlayerLogic player = CreateOnWall();

        player.Tick(FIXED_DELTA, Move(Vector2.down), 0f);

        Assert.AreEqual(PlayerPoseState.Standing, player.PoseState);
        Assert.AreEqual(Vector3.zero, player.WallCover.Normal);
    }

    [Test]
    public void SprintAlongWall_DetachesAndRuns()
    {
        PlayerLogic player = CreateOnWall();

        Simulate(player, Sprint(Vector2.right), SHUFFLE_DURATION);

        Assert.AreEqual(PlayerPoseState.Standing, player.PoseState);
        Assert.AreEqual(config.RunSpeed, player.Locomotion.MeasuredSpeed, RUN_SPEED_TOLERANCE);
    }

    [Test]
    public void SprintIntoWall_StaysOnWall()
    {
        PlayerLogic player = CreateOnWall();

        Simulate(player, Sprint(Vector2.up), SHUFFLE_DURATION);

        Assert.AreEqual(PlayerPoseState.WallStanding, player.PoseState);
    }

    [Test]
    public void CrouchOnWall_DetachesIntoCrouch()
    {
        PlayerLogic player = CreateOnWall();

        TapCrouch(player);
        Simulate(player, Idle(), SETTLE_DURATION);

        Assert.AreEqual(PlayerPoseState.Crouched, player.PoseState);
        Assert.AreEqual(config.CrouchedHeight, player.Motor.Height);
    }

    private float ContactZ() => WALL_Z - config.StandingRadius - config.SkinWidth;

    private PlayerLogic CreateFacingWall()
    {
        PlaneCollisionWorldFake world = new PlaneCollisionWorldFake()
            .AddPlane(Vector3.up, Vector3.zero)
            .AddPlane(Vector3.back, new Vector3(0f, 0f, WALL_Z));

        PlayerLogic player = new PlayerLogic(config, world);
        player.Teleport(new Vector3(0f, 0f, 0.5f), 0f);
        Simulate(player, Idle(), SHORT_SETTLE_DURATION);
        return player;
    }

    private PlayerLogic CreateOnWall()
    {
        PlayerLogic player = CreateFacingWall();

        Simulate(player, Move(Vector2.up), config.WallPushDuration + PUSH_MARGIN);
        Simulate(player, Idle(), SETTLE_DURATION);
        return player;
    }

    private void TapCrouch(PlayerLogic player)
    {
        player.Tick(FIXED_DELTA, new PlayerInputFrame(Vector2.zero, false, false, true, true, false), 0f);
        player.Tick(FIXED_DELTA, new PlayerInputFrame(Vector2.zero, false, false, false, false, true), 0f);
    }

    private void Simulate(PlayerLogic player, PlayerInputFrame input, float seconds)
    {
        int steps = Mathf.RoundToInt(seconds / FIXED_DELTA);

        for (int i = 0; i < steps; i++)
            player.Tick(FIXED_DELTA, input, 0f);
    }

    private PlayerInputFrame Idle() => new PlayerInputFrame(Vector2.zero, false, false, false);
    private PlayerInputFrame Move(Vector2 move) => new PlayerInputFrame(move, false, false, false);
    private PlayerInputFrame Sprint(Vector2 move) => new PlayerInputFrame(move, false, true, false);
}
