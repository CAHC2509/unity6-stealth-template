using NUnit.Framework;
using StealthController.Core;
using UnityEngine;

public class PlayerLogicTests
{
    private const float FIXED_DELTA = 0.02f;
    private const float SPEED_TOLERANCE = 0.001f;
    private const float MEASURED_SPEED_TOLERANCE = 0.05f;
    private const float POSITION_TOLERANCE = 0.01f;
    private const float YAW_TOLERANCE = 0.5f;
    private const float SETTLE_DURATION = 0.1f;
    private const float MOVE_DURATION = 1f;
    private const float SHORT_MOVE_DURATION = 0.5f;
    private const float HOLD_MARGIN = 0.1f;
    private const float EXTRA_HOLD_MARGIN = 0.3f;
    private const int MAX_TRANSITION_STEPS = 50;
    private const int TURN_STEPS = 50;
    private const float ROTATED_REFERENCE_YAW = 90f;
    private const float MIN_CROUCH_WALK_DISTANCE = 0.8f;
    private const float MIN_ROTATED_WALK_DISTANCE = 1f;

    private PlayerMovementConfig config;

    [SetUp]
    public void SetUp()
    {
        config = new PlayerMovementConfig();
    }

    [Test]
    public void CrouchPressedWhileStanding_ChangesToCrouched()
    {
        PlayerLogic player = CreateOnFloor(new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero));

        TapCrouch(player);

        Assert.AreEqual(PlayerPoseState.Crouched, player.PoseState);
        Assert.AreEqual(config.CrouchedHeight, player.Motor.Height);
    }

    [Test]
    public void TappingCrouchWhileCrouched_StandsUp()
    {
        PlayerLogic player = CreateOnFloor(new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero));

        TapCrouch(player);
        WaitForTransition(player);
        TapCrouch(player);

        Assert.AreEqual(PlayerPoseState.Standing, player.PoseState);
        Assert.AreEqual(config.StandingHeight, player.Motor.Height);
    }

    [Test]
    public void HoldingCrouch_DropsToProne()
    {
        PlayerLogic player = CreateOnFloor(new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero));

        HoldCrouch(player, config.CrouchHoldToProneDuration + HOLD_MARGIN);

        Assert.AreEqual(PlayerPoseState.Crawling, player.PoseState);
        Assert.AreEqual(config.CrawlingHeight, player.Motor.Height);
    }

    [Test]
    public void PostureTransitionProgress_RunsFromStartToEnd()
    {
        PlayerLogic player = CreateOnFloor(new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero));

        Assert.AreEqual(1f, player.Posture.TransitionProgress);

        TapCrouch(player);

        Assert.IsTrue(player.Posture.IsInTransition);
        Assert.Less(player.Posture.TransitionProgress, 1f);
        Assert.AreEqual(player.Posture.TransitionProgress, player.Pose.PostureTransitionProgress);

        WaitForTransition(player);

        Assert.AreEqual(1f, player.Posture.TransitionProgress);
        Assert.IsFalse(player.Pose.IsInPostureTransition);
    }

    [Test]
    public void DroppingToProne_LastsProneTransitionDuration()
    {
        PlayerLogic player = CreateOnFloor(new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero));

        HoldCrouch(player, config.CrouchHoldToProneDuration + HOLD_MARGIN);
        Simulate(player, Idle(), config.PostureTransitionDuration);

        Assert.AreEqual(PlayerPoseState.Crawling, player.PoseState);
        Assert.IsTrue(player.Posture.IsInTransition);

        Simulate(player, Idle(), config.ProneTransitionDuration);

        Assert.IsFalse(player.Posture.IsInTransition);
    }

    [Test]
    public void SprintingWhileCrouched_StandsUpAndRuns()
    {
        PlayerLogic player = CreateOnFloor(new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero));

        TapCrouch(player);
        WaitForTransition(player);
        Simulate(player, Sprint(Vector2.up), MOVE_DURATION);

        Assert.AreEqual(PlayerPoseState.Standing, player.PoseState);
        Assert.AreEqual(config.RunSpeed, player.Locomotion.MeasuredSpeed, MEASURED_SPEED_TOLERANCE);
    }

    [Test]
    public void CrouchingWhileSprinting_StaysCrouched()
    {
        PlayerLogic player = CreateOnFloor(new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero));

        Simulate(player, Sprint(Vector2.up), SHORT_MOVE_DURATION);
        player.Tick(FIXED_DELTA, new PlayerInputFrame(Vector2.up, false, true, true, true, false), 0f);
        player.Tick(FIXED_DELTA, new PlayerInputFrame(Vector2.up, false, true, false, false, true), 0f);
        Simulate(player, Sprint(Vector2.up), SHORT_MOVE_DURATION);

        Assert.AreEqual(PlayerPoseState.Crouched, player.PoseState);
    }

    [Test]
    public void StandPressedWhileProne_StandsUpDirectly()
    {
        PlayerLogic player = CreateProne(new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero));

        player.Tick(FIXED_DELTA, new PlayerInputFrame(Vector2.zero, false, false, false, false, false, true), 0f);
        WaitForTransition(player);
        player.Tick(FIXED_DELTA, Idle(), 0f);
        WaitForTransition(player);

        Assert.AreEqual(PlayerPoseState.Standing, player.PoseState);
        Assert.AreEqual(config.StandingHeight, player.Motor.Height);
    }

    [Test]
    public void SprintingWhileProne_GetsUpBeforeMoving()
    {
        PlayerLogic player = CreateProne(new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero));
        Vector3 start = player.Motor.Position;

        Simulate(player, Sprint(Vector2.up), config.ProneTransitionDuration - FIXED_DELTA);

        Assert.AreEqual(PlayerPoseState.Crouched, player.PoseState);
        Assert.AreEqual(start.z, player.Motor.Position.z, POSITION_TOLERANCE);

        Simulate(player, Sprint(Vector2.up), MOVE_DURATION);

        Assert.AreEqual(PlayerPoseState.Standing, player.PoseState);
        Assert.AreEqual(config.RunSpeed, player.Locomotion.MeasuredSpeed, MEASURED_SPEED_TOLERANCE);
    }

    [Test]
    public void MovementDuringPostureTransition_IsNotBlocked()
    {
        PlayerLogic player = CreateOnFloor(new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero));
        Vector3 start = player.Motor.Position;

        player.Tick(FIXED_DELTA, new PlayerInputFrame(Vector2.up, false, false, true, true, false), 0f);
        player.Tick(FIXED_DELTA, new PlayerInputFrame(Vector2.up, false, false, false, false, true), 0f);
        Simulate(player, Move(Vector2.up, false), config.PostureTransitionDuration * 0.5f);

        Assert.IsTrue(player.Posture.IsInTransition);
        Assert.Greater(player.Motor.Position.z, start.z);
    }

    [Test]
    public void MoveInputWhileCrouched_WalksCrouched()
    {
        PlayerLogic player = CreateOnFloor(new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero));

        TapCrouch(player);
        WaitForTransition(player);
        Simulate(player, Move(Vector2.up, false), MOVE_DURATION);

        Assert.AreEqual(PlayerPoseState.Crouched, player.PoseState);
        Assert.AreEqual(config.CrouchWalkSpeed, player.Locomotion.MeasuredSpeed, MEASURED_SPEED_TOLERANCE);
        Assert.Greater(player.Motor.Position.z, MIN_CROUCH_WALK_DISTANCE);
    }

    [Test]
    public void CrouchedTargetSpeed_FollowsStickTilt()
    {
        PlayerLogic player = CreateOnFloor(new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero));
        Vector2 belowDeadZone = Vector2.up * (config.StickDeadZone * 0.5f);

        Assert.AreEqual(0f, player.MoveInput.ComputeCrouchedTargetSpeed(Move(belowDeadZone, true)), SPEED_TOLERANCE);
        Assert.AreEqual(config.CrouchWalkSpeed, player.MoveInput.ComputeCrouchedTargetSpeed(Move(Vector2.up, true)), SPEED_TOLERANCE);
        Assert.AreEqual(config.CrouchWalkSpeed, player.MoveInput.ComputeCrouchedTargetSpeed(Move(Vector2.up, false)), SPEED_TOLERANCE);
    }

    [Test]
    public void ReleasingInputWhileCrawling_StaysProne()
    {
        PlayerLogic player = CreateProne(new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero));

        Simulate(player, Move(Vector2.up, true), SHORT_MOVE_DURATION);
        Simulate(player, Idle(), SHORT_MOVE_DURATION);

        Assert.AreEqual(PlayerPoseState.Crawling, player.PoseState);
    }

    [Test]
    public void CrouchPressedWhileProneUnderLowCeiling_StaysProne()
    {
        PlaneCollisionWorldFake world = new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero);
        PlayerLogic player = CreateProne(world);

        world.AddPlane(Vector3.down, new Vector3(0f, 0.6f, 0f));
        TapCrouch(player);

        Assert.AreEqual(PlayerPoseState.Crawling, player.PoseState);
        Assert.AreEqual(config.CrawlingHeight, player.Motor.Height);
    }

    [Test]
    public void CrouchPressedWhileProneInOpenSpace_ReturnsToCrouched()
    {
        PlayerLogic player = CreateProne(new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero));

        TapCrouch(player);

        Assert.AreEqual(PlayerPoseState.Crouched, player.PoseState);
        Assert.AreEqual(config.CrouchedHeight, player.Motor.Height);
    }

    [Test]
    public void HoldingCrouchFromProne_DoesNotDropBackToProne()
    {
        PlayerLogic player = CreateProne(new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero));

        HoldCrouch(player, config.CrouchHoldToProneDuration + EXTRA_HOLD_MARGIN);

        Assert.AreEqual(PlayerPoseState.Crouched, player.PoseState);
    }

    [Test]
    public void StandingTargetSpeed_FollowsStickTiltAndSprint()
    {
        PlayerLogic player = CreateOnFloor(new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero));
        Vector2 belowDeadZone = Vector2.up * (config.StickDeadZone * 0.5f);
        float halfwayTilt = Mathf.Lerp(config.StickDeadZone, config.RunInputThreshold, 0.5f);
        float halfwayWalkSpeed = Mathf.Lerp(config.MinWalkSpeed, config.MaxWalkSpeed, 0.5f);

        Assert.AreEqual(0f, player.MoveInput.ComputeStandingTargetSpeed(Move(belowDeadZone, true)), SPEED_TOLERANCE);
        Assert.AreEqual(halfwayWalkSpeed, player.MoveInput.ComputeStandingTargetSpeed(Move(Vector2.up * halfwayTilt, true)), SPEED_TOLERANCE);
        Assert.AreEqual(config.RunSpeed, player.MoveInput.ComputeStandingTargetSpeed(Move(Vector2.up, true)), SPEED_TOLERANCE);
        Assert.AreEqual(config.MaxWalkSpeed, player.MoveInput.ComputeStandingTargetSpeed(Move(Vector2.up, false)), SPEED_TOLERANCE);
        Assert.AreEqual(config.RunSpeed, player.MoveInput.ComputeStandingTargetSpeed(new PlayerInputFrame(Vector2.up, false, true, false)), SPEED_TOLERANCE);
    }

    [Test]
    public void StandingMovement_FollowsReferenceYaw()
    {
        PlayerLogic player = CreateOnFloor(new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero));

        for (int i = 0; i < TURN_STEPS; i++)
            player.Tick(FIXED_DELTA, Move(Vector2.up, false), ROTATED_REFERENCE_YAW);

        Assert.Greater(player.Motor.Position.x, MIN_ROTATED_WALK_DISTANCE);
        Assert.AreEqual(0f, player.Motor.Position.z, POSITION_TOLERANCE);
        Assert.AreEqual(ROTATED_REFERENCE_YAW, Mathf.Repeat(player.Facing.Yaw, StealthConstants.HALF_TURN_DEGREES * 2f), YAW_TOLERANCE);
    }

    private PlayerLogic CreateOnFloor(PlaneCollisionWorldFake world)
    {
        PlayerLogic player = new PlayerLogic(config, world);
        player.Teleport(Vector3.zero, 0f);
        Simulate(player, Idle(), SETTLE_DURATION);
        return player;
    }

    private PlayerLogic CreateProne(PlaneCollisionWorldFake world)
    {
        PlayerLogic player = CreateOnFloor(world);

        HoldCrouch(player, config.CrouchHoldToProneDuration + HOLD_MARGIN);
        WaitForTransition(player);
        return player;
    }

    private void TapCrouch(PlayerLogic player)
    {
        player.Tick(FIXED_DELTA, new PlayerInputFrame(Vector2.zero, false, false, true, true, false), 0f);
        player.Tick(FIXED_DELTA, new PlayerInputFrame(Vector2.zero, false, false, false, false, true), 0f);
    }

    private void HoldCrouch(PlayerLogic player, float seconds)
    {
        player.Tick(FIXED_DELTA, new PlayerInputFrame(Vector2.zero, false, false, true, true, false), 0f);
        Simulate(player, new PlayerInputFrame(Vector2.zero, false, false, false, true, false), seconds);
        player.Tick(FIXED_DELTA, new PlayerInputFrame(Vector2.zero, false, false, false, false, true), 0f);
    }

    private void WaitForTransition(PlayerLogic player)
    {
        for (int i = 0; i < MAX_TRANSITION_STEPS && player.Posture.IsInTransition; i++)
            player.Tick(FIXED_DELTA, Idle(), 0f);
    }

    private void Simulate(PlayerLogic player, PlayerInputFrame input, float seconds)
    {
        int steps = Mathf.RoundToInt(seconds / FIXED_DELTA);

        for (int i = 0; i < steps; i++)
            player.Tick(FIXED_DELTA, input, 0f);
    }

    private PlayerInputFrame Idle() => new PlayerInputFrame(Vector2.zero, false, false, false);
    private PlayerInputFrame Move(Vector2 move, bool isAnalog) => new PlayerInputFrame(move, isAnalog, false, false);
    private PlayerInputFrame Sprint(Vector2 move) => new PlayerInputFrame(move, false, true, false);
}
