using NUnit.Framework;
using StealthController.Core;
using UnityEngine;

public class CameraOcclusionLogicTests
{
    private const float FIXED_DELTA = 0.02f;
    private const float RADIUS_TOLERANCE = 0.001f;

    private static readonly Vector3 CameraPosition = new Vector3(0f, 10f, -10f);

    private PlayerMovementConfig playerConfig;

    [SetUp]
    public void SetUp()
    {
        playerConfig = new PlayerMovementConfig();
    }

    [Test]
    public void ClearView_KeepsTheCutoutClosed()
    {
        CameraOcclusionLogic occlusion = CreateOcclusion(new PlaneCollisionWorldFake());

        Simulate(occlusion, 1f / StealthConstants.OCCLUSION_CUTOUT_OPEN_SPEED);

        Assert.AreEqual(0f, occlusion.CutoutRadius, RADIUS_TOLERANCE);
    }

    [Test]
    public void WallBetweenCameraAndPlayer_OpensTheCutoutOverTime()
    {
        PlaneCollisionWorldFake world = new PlaneCollisionWorldFake().AddPlane(Vector3.back, new Vector3(0f, 0f, -2f));
        CameraOcclusionLogic occlusion = CreateOcclusion(world);

        Simulate(occlusion, 0.5f / StealthConstants.OCCLUSION_CUTOUT_OPEN_SPEED);
        Assert.Greater(occlusion.CutoutRadius, 0f);
        Assert.Less(occlusion.CutoutRadius, StealthConstants.OCCLUSION_CUTOUT_RADIUS);

        Simulate(occlusion, 1f / StealthConstants.OCCLUSION_CUTOUT_OPEN_SPEED);
        Assert.AreEqual(StealthConstants.OCCLUSION_CUTOUT_RADIUS, occlusion.CutoutRadius, RADIUS_TOLERANCE);
    }

    [Test]
    public void CutoutTarget_FollowsThePlayerMidHeight()
    {
        CameraOcclusionLogic occlusion = CreateOcclusion(new PlaneCollisionWorldFake());
        Vector3 playerPosition = new Vector3(2f, 0f, 3f);

        occlusion.Tick(FIXED_DELTA, CameraPosition, Standing(playerPosition));

        Assert.AreEqual(playerPosition + Vector3.up * (playerConfig.StandingHeight * 0.5f), occlusion.CutoutTarget);
    }

    private CameraOcclusionLogic CreateOcclusion(ICollisionWorld world)
    {
        return new CameraOcclusionLogic(world, StealthConstants.CAMERA_OCCLUSION_MIN_TARGET_HEIGHT, StealthConstants.OCCLUSION_CUTOUT_RADIUS, StealthConstants.OCCLUSION_CUTOUT_OPEN_SPEED, StealthConstants.OCCLUSION_CUTOUT_CLOSE_SPEED);
    }

    private void Simulate(CameraOcclusionLogic occlusion, float seconds)
    {
        int steps = Mathf.CeilToInt(seconds / FIXED_DELTA);

        for (int i = 0; i < steps; i++)
            occlusion.Tick(FIXED_DELTA, CameraPosition, Standing(Vector3.zero));
    }

    private PlayerPose Standing(Vector3 position)
    {
        return new PlayerPose(position, Quaternion.identity, PlayerPoseState.Standing, 0f, 1f, playerConfig.StandingRadius, playerConfig.StandingHeight, Vector3.zero);
    }
}
