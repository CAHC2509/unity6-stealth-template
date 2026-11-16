using System.Collections.Generic;
using NUnit.Framework;
using StealthController.Core;
using UnityEngine;

public class StealthComponentTests
{
    private const float FIXED_DELTA = 0.02f;
    private const float TURN_DURATION = 1f;
    private const float TURN_SMOOTH_TIME = 0.1f;
    private const float TURN_SPEED = 720f;
    private const float TARGET_YAW = 90f;
    private const float YAW_TOLERANCE = 0.5f;
    private const float ALERT_COOLDOWN = 1f;
    private const float CAPTURE_DISTANCE = 0.8f;
    private const float SWEEP_HALF_ANGLE = 45f;
    private const float SWEEP_SPEED = 90f;
    private const float SWEEP_PAUSE = 0.5f;
    private const float ANGLE_TOLERANCE = 0.01f;

    [Test]
    public void Facing_TurnsTowardsDirectionOverTime()
    {
        FacingComponent facing = new FacingComponent();
        facing.Reset(0f);

        for (float elapsed = 0f; elapsed < TURN_DURATION; elapsed += FIXED_DELTA)
            facing.RotateTowards(Vector3.right, TURN_SMOOTH_TIME, TURN_SPEED, FIXED_DELTA);

        Assert.AreEqual(TARGET_YAW, facing.Yaw, YAW_TOLERANCE);
        Assert.AreEqual(1f, Vector3.Dot(facing.Forward, Vector3.right), ANGLE_TOLERANCE);
    }

    [Test]
    public void PatrolRoute_StartsAtNearestWaypointAndWraps()
    {
        PatrolRouteComponent route = new PatrolRouteComponent(new List<GuardWaypoint>
        {
            new GuardWaypoint(new Vector3(0f, 0f, 0f), 0f, false, 0f),
            new GuardWaypoint(new Vector3(10f, 0f, 0f), 0f, false, 0f),
            new GuardWaypoint(new Vector3(20f, 0f, 0f), 0f, false, 0f)
        });

        route.StartFromNearest(new Vector3(19f, 0f, 1f));
        Assert.AreEqual(20f, route.Current.Position.x);

        route.AdvanceToNext();
        Assert.AreEqual(0f, route.Current.Position.x);
    }

    [Test]
    public void AlertEmitter_IgnoresAlertsDuringCooldown()
    {
        AlertEmitterComponent emitter = new AlertEmitterComponent(ALERT_COOLDOWN);
        emitter.Reset();

        emitter.Raise(Vector3.one);
        Assert.IsTrue(emitter.TryConsume(out Vector3 firstPosition));
        Assert.AreEqual(Vector3.one, firstPosition);

        emitter.Raise(Vector3.zero);
        Assert.IsFalse(emitter.TryConsume(out Vector3 _));

        emitter.Tick(ALERT_COOLDOWN);
        emitter.Raise(Vector3.zero);
        Assert.IsTrue(emitter.TryConsume(out Vector3 _));
    }

    [Test]
    public void PlayerCapture_OnlyCapturesWithinDistance()
    {
        PlayerCaptureComponent capture = new PlayerCaptureComponent(CAPTURE_DISTANCE);

        Assert.IsFalse(capture.TryCapture(CAPTURE_DISTANCE * 2f));
        Assert.IsFalse(capture.HasCapturedPlayer);

        Assert.IsTrue(capture.TryCapture(CAPTURE_DISTANCE));
        Assert.IsTrue(capture.HasCapturedPlayer);

        capture.Reset();
        Assert.IsFalse(capture.HasCapturedPlayer);
    }

    [Test]
    public void Sweep_ClampsAtTheLimitAndReverses()
    {
        SweepComponent sweep = new SweepComponent(SWEEP_HALF_ANGLE, SWEEP_SPEED, SWEEP_PAUSE);
        sweep.Reset();

        sweep.Tick(SWEEP_HALF_ANGLE / SWEEP_SPEED * 2f);
        Assert.AreEqual(SWEEP_HALF_ANGLE, sweep.Offset, ANGLE_TOLERANCE);

        sweep.Tick(SWEEP_PAUSE);
        sweep.Tick(FIXED_DELTA);
        Assert.Less(sweep.Offset, SWEEP_HALF_ANGLE);
    }
}
