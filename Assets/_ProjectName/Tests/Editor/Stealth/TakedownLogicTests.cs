using System.Collections.Generic;
using NUnit.Framework;
using StealthController.Core;
using UnityEngine;

public class TakedownLogicTests
{
    private const float FIXED_DELTA = 0.02f;
    private const float SETTLE_DURATION = 0.2f;
    private const float END_MARGIN = 0.1f;
    private const float WATCH_DURATION = 3f;
    private const float POSITION_TOLERANCE = 0.05f;
    private const float YAW_TOLERANCE = 0.5f;

    private PlaneCollisionWorldFake world;
    private PlayerMovementConfig playerConfig;
    private TakedownConfig takedownConfig;
    private GuardLogic guard;
    private List<GuardLogic> guards;

    [SetUp]
    public void SetUp()
    {
        world = new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero);
        playerConfig = new PlayerMovementConfig();
        takedownConfig = new TakedownConfig();

        List<GuardWaypoint> route = new List<GuardWaypoint> { new GuardWaypoint(Vector3.zero, WATCH_DURATION, false, 0f) };
        guard = new GuardLogic(new GuardConfig(), route, Vector3.zero, 0f, world, new StraightLinePathfinderFake());
        guards = new List<GuardLogic> { guard };
    }

    [Test]
    public void PlayerBehindPatrollingGuard_IsATarget()
    {
        PlayerLogic player = CreatePlayer(new Vector3(0f, 0f, -1.5f));
        TakedownLogic takedown = new TakedownLogic(takedownConfig, world);

        takedown.UpdateTarget(player, guards);

        Assert.AreSame(guard, takedown.Target);
    }

    [Test]
    public void PlayerInFrontOrTooFar_IsNotATarget()
    {
        TakedownLogic takedown = new TakedownLogic(takedownConfig, world);

        takedown.UpdateTarget(CreatePlayer(new Vector3(0f, 0f, 1.5f)), guards);
        Assert.IsFalse(takedown.HasTarget);

        takedown.UpdateTarget(CreatePlayer(new Vector3(0f, 0f, -takedownConfig.MaxDistance - 1f)), guards);
        Assert.IsFalse(takedown.HasTarget);
    }

    [Test]
    public void ChasingGuard_IsNotATarget()
    {
        PlayerLogic player = CreatePlayer(new Vector3(0f, 0f, -1.5f));
        TakedownLogic takedown = new TakedownLogic(takedownConfig, world);

        guard.Pursue(new Vector3(0f, 0f, 5f));
        takedown.UpdateTarget(player, guards);

        Assert.IsFalse(takedown.HasTarget);
    }

    [Test]
    public void Takedown_LocksBothAndReturnsControlAtTheExit()
    {
        PlayerLogic player = CreatePlayer(new Vector3(0f, 0f, -1.5f));
        TakedownLogic takedown = new TakedownLogic(takedownConfig, world);

        takedown.UpdateTarget(player, guards);
        Assert.IsTrue(takedown.TryStart(player));
        Assert.AreEqual(PlayerPoseState.Takedown, player.PoseState);
        Assert.AreEqual(GuardAlertState.TakenDown, guard.AlertState);

        Simulate(player, takedownConfig.EndTime + END_MARGIN);

        Vector3 expectedExit = new Vector3(takedownConfig.ExitOffsetSide, 0f, takedownConfig.ExitOffsetForward - takedownConfig.AttackerOffset);
        Assert.AreEqual(PlayerPoseState.Standing, player.PoseState);
        Assert.AreEqual(expectedExit.x, player.Motor.Position.x, POSITION_TOLERANCE);
        Assert.AreEqual(expectedExit.z, player.Motor.Position.z, POSITION_TOLERANCE);
        Assert.AreEqual(Mathf.Repeat(takedownConfig.ExitYawOffset, StealthConstants.FULL_TURN_DEGREES), Mathf.Repeat(player.Facing.Yaw, StealthConstants.FULL_TURN_DEGREES), YAW_TOLERANCE);

        Simulate(player, WATCH_DURATION);
        Assert.AreEqual(GuardAlertState.TakenDown, guard.AlertState);
        Assert.IsFalse(guard.Vision.CanSeePlayer);
        Assert.IsFalse(guard.HasCapturedPlayer);
    }

    [Test]
    public void GuardReset_BringsTheGuardBackToPatrol()
    {
        PlayerLogic player = CreatePlayer(new Vector3(0f, 0f, -1.5f));
        TakedownLogic takedown = new TakedownLogic(takedownConfig, world);

        takedown.UpdateTarget(player, guards);
        takedown.TryStart(player);
        guard.Reset();

        Assert.AreEqual(GuardAlertState.Patrol, guard.AlertState);
        Assert.AreEqual(0f, guard.Pose.ActionProgress);
    }

    private PlayerLogic CreatePlayer(Vector3 position)
    {
        PlayerLogic player = new PlayerLogic(playerConfig, world);
        player.Teleport(position, 0f);
        Simulate(player, SETTLE_DURATION);
        return player;
    }

    private void Simulate(PlayerLogic player, float seconds)
    {
        int steps = Mathf.CeilToInt(seconds / FIXED_DELTA);

        for (int i = 0; i < steps; i++)
        {
            player.Tick(FIXED_DELTA, new PlayerInputFrame(Vector2.zero, false, false, false), 0f);
            guard.Tick(FIXED_DELTA, player.Pose);
        }
    }
}
