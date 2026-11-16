using NUnit.Framework;
using StealthController.Core;
using UnityEngine;

public class CameraRigLogicTests
{
    private const float FIXED_DELTA = 0.02f;
    private const float SETTLE_DURATION = 3f;
    private const float POSITION_TOLERANCE = 0.05f;
    private const float ANGLE_TOLERANCE = 0.5f;
    private const float DISTANCE_TOLERANCE = 0.05f;

    private CameraRigConfig config;
    private PlayerMovementConfig playerConfig;

    [SetUp]
    public void SetUp()
    {
        config = new CameraRigConfig();
        playerConfig = new PlayerMovementConfig();
    }

    [Test]
    public void Snap_PlacesPivotAbovePlayerWithFixedFraming()
    {
        CameraRigLogic rig = new CameraRigLogic(config);
        Vector3 playerPosition = new Vector3(1f, 0f, 2f);

        rig.Snap(StandingPose(playerPosition));
        CameraFraming framing = rig.Framing;

        Assert.AreEqual(playerPosition + Vector3.up * config.PivotHeight, framing.Pivot);
        Assert.AreEqual(config.Framing.Pitch, framing.Pitch, ANGLE_TOLERANCE);
        Assert.AreEqual(config.Framing.Distance, framing.Distance, DISTANCE_TOLERANCE);
        Assert.AreEqual(config.Framing.Distance, Vector3.Distance(framing.Position, framing.Pivot), DISTANCE_TOLERANCE);
    }

    [Test]
    public void Tick_FollowsPlayerOverTime()
    {
        CameraRigLogic rig = new CameraRigLogic(config);
        Vector3 target = new Vector3(5f, 0f, 0f);

        rig.Snap(StandingPose(Vector3.zero));
        Simulate(rig, StandingPose(target), SETTLE_DURATION);

        Assert.AreEqual(target.x, rig.Framing.Pivot.x, POSITION_TOLERANCE);
        Assert.AreEqual(config.PivotHeight, rig.Framing.Pivot.y, POSITION_TOLERANCE);
    }

    [Test]
    public void Framing_StaysFixedWhileFollowing()
    {
        CameraRigLogic rig = new CameraRigLogic(config);

        rig.Snap(StandingPose(Vector3.zero));
        Simulate(rig, StandingPose(new Vector3(-8f, 0f, 12f)), SETTLE_DURATION);

        Assert.AreEqual(config.Framing.Pitch, rig.Framing.Pitch, ANGLE_TOLERANCE);
        Assert.AreEqual(config.Framing.Yaw, rig.Framing.Yaw, ANGLE_TOLERANCE);
        Assert.AreEqual(config.Framing.Distance, rig.Framing.Distance, DISTANCE_TOLERANCE);
        Assert.AreEqual(config.Framing.Yaw, rig.InputReferenceYaw, ANGLE_TOLERANCE);
    }

    private void Simulate(CameraRigLogic rig, PlayerPose pose, float seconds)
    {
        int steps = Mathf.RoundToInt(seconds / FIXED_DELTA);

        for (int i = 0; i < steps; i++)
            rig.Tick(FIXED_DELTA, pose);
    }

    private PlayerPose StandingPose(Vector3 position)
    {
        return new PlayerPose(
            position,
            Quaternion.identity,
            PlayerPoseState.Standing,
            0f,
            1f,
            playerConfig.StandingRadius,
            playerConfig.StandingHeight,
            Vector3.zero);
    }
}
