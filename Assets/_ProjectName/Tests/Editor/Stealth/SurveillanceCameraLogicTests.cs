using System.Collections.Generic;
using NUnit.Framework;
using StealthController.Core;
using StealthController.Unity;
using UnityEngine;

public class SurveillanceCameraLogicTests
{
    private const float FIXED_DELTA = 0.02f;
    private const float ANGLE_TOLERANCE = 0.5f;
    private const float DETECTION_MARGIN = 0.1f;
    private const float MOUNT_HEIGHT = 2.7f;

    private static readonly Vector3 FarAway = new Vector3(0f, 0f, -100f);

    private SurveillanceCameraConfig config;
    private PlayerMovementConfig playerConfig;

    [SetUp]
    public void SetUp()
    {
        config = new SurveillanceCameraConfig();
        playerConfig = new PlayerMovementConfig();
    }

    [Test]
    public void Sweep_StopsAtTheLimitAndPausesBeforeReversing()
    {
        SurveillanceCameraLogic surveillanceCamera = CreateCamera(Vector3.zero);
        float timeToLimit = config.SweepHalfAngle / config.SweepSpeed;

        Simulate(surveillanceCamera, Standing(FarAway), timeToLimit + config.SweepPauseDuration * 0.5f);
        Assert.AreEqual(config.SweepHalfAngle, surveillanceCamera.Yaw, ANGLE_TOLERANCE);

        Simulate(surveillanceCamera, Standing(FarAway), config.SweepPauseDuration + timeToLimit);
        Assert.Less(surveillanceCamera.Yaw, config.SweepHalfAngle - ANGLE_TOLERANCE);
    }

    [Test]
    public void DetectingThePlayer_RaisesOneAlertPerCooldown()
    {
        SurveillanceCameraLogic surveillanceCamera = CreateCamera(Vector3.zero);
        PlayerPose player = Standing(new Vector3(0f, 0f, 5f));

        Simulate(surveillanceCamera, player, config.Vision.ConfirmDuration + DETECTION_MARGIN);
        Assert.IsTrue(surveillanceCamera.TryConsumeAlert(out Vector3 knownPosition));
        Assert.AreEqual(player.Position, knownPosition);

        Simulate(surveillanceCamera, player, config.AlertCooldown * 0.5f);
        Assert.IsFalse(surveillanceCamera.TryConsumeAlert(out knownPosition));

        Simulate(surveillanceCamera, player, config.AlertCooldown);
        Assert.IsTrue(surveillanceCamera.TryConsumeAlert(out knownPosition));
    }

    [Test]
    public void PlayerInsideBlindDistance_IsNotSeen()
    {
        SurveillanceCameraLogic surveillanceCamera = CreateCamera(Vector3.zero);
        PlayerPose player = Standing(new Vector3(0f, 0f, config.Vision.MinRange * 0.5f));

        Simulate(surveillanceCamera, player, config.Vision.ConfirmDuration + DETECTION_MARGIN);

        Assert.IsFalse(surveillanceCamera.Vision.CanSeePlayer);
        Assert.IsFalse(surveillanceCamera.TryConsumeAlert(out Vector3 _));
    }

    [Test]
    public void CameraAlert_OnlyReachesGuardsWithinRadius()
    {
        float alertRadius = StealthUnityConstants.SURVEILLANCE_ALERT_RADIUS;
        EnemyCoordinatorLogic coordinator = new EnemyCoordinatorLogic(alertRadius);
        GuardLogic nearGuard = CreateGuard(new Vector3(-alertRadius * 0.5f, 0f, 0f));
        GuardLogic farGuard = CreateGuard(new Vector3(-alertRadius * 2f, 0f, 0f));

        coordinator.AddGuard(nearGuard);
        coordinator.AddGuard(farGuard);
        coordinator.AddSurveillanceCamera(CreateCamera(Vector3.zero));

        PlayerPose player = Standing(new Vector3(0f, 0f, 5f));
        int steps = Mathf.CeilToInt((config.Vision.ConfirmDuration + DETECTION_MARGIN) / FIXED_DELTA);

        for (int i = 0; i < steps; i++)
            coordinator.Tick(FIXED_DELTA, player);

        Assert.AreEqual(GuardAlertState.Chase, nearGuard.AlertState);
        Assert.AreEqual(GuardAlertState.Patrol, farGuard.AlertState);
    }

    private SurveillanceCameraLogic CreateCamera(Vector3 floorPosition)
    {
        return new SurveillanceCameraLogic(config, floorPosition + Vector3.up * MOUNT_HEIGHT, floorPosition, 0f, new PlaneCollisionWorldFake());
    }

    private GuardLogic CreateGuard(Vector3 position)
    {
        return new GuardLogic(new GuardConfig(), new List<GuardWaypoint>(), position, StealthConstants.HALF_TURN_DEGREES, new PlaneCollisionWorldFake(), new StraightLinePathfinderFake());
    }

    private void Simulate(SurveillanceCameraLogic surveillanceCamera, PlayerPose player, float seconds)
    {
        int steps = Mathf.CeilToInt(seconds / FIXED_DELTA);

        for (int i = 0; i < steps; i++)
            surveillanceCamera.Tick(FIXED_DELTA, player);
    }

    private PlayerPose Standing(Vector3 position)
    {
        return new PlayerPose(position, Quaternion.identity, PlayerPoseState.Standing, 0f, 1f, playerConfig.StandingRadius, playerConfig.StandingHeight, Vector3.zero);
    }
}
