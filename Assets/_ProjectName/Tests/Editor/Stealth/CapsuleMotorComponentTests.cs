using NUnit.Framework;
using StealthController.Core;
using UnityEngine;

public class CapsuleMotorComponentTests
{
    private const float FIXED_DELTA = 0.02f;
    private const float POSITION_TOLERANCE = 0.01f;
    private const float WALL_CONTACT_TOLERANCE = 0.05f;
    private const int SETTLE_STEPS = 10;
    private const int FALL_STEPS = 100;
    private const int PUSH_STEPS = 60;
    private const int WALK_STEPS = 50;
    private const float PUSH_STEP_DISTANCE = 0.05f;
    private const float WALK_STEP_DISTANCE = 0.08f;
    private const float MIN_DISTANCE_SLID_ALONG_WALL = 2.5f;

    private PlayerMovementConfig config;

    [SetUp]
    public void SetUp()
    {
        config = new PlayerMovementConfig();
    }

    [Test]
    public void Motor_FallsAndLandsOnFloor()
    {
        PlaneCollisionWorldFake world = new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero);
        CapsuleMotorComponent motor = new CapsuleMotorComponent(world, config);

        motor.Teleport(Vector3.up);
        Step(motor, Vector3.zero, FALL_STEPS);

        Assert.IsTrue(motor.IsGrounded);
        Assert.AreEqual(config.SkinWidth, motor.Position.y, POSITION_TOLERANCE);
    }

    [Test]
    public void Motor_StopsAtWallWithoutPassingThrough()
    {
        PlaneCollisionWorldFake world = new PlaneCollisionWorldFake()
            .AddPlane(Vector3.up, Vector3.zero)
            .AddPlane(Vector3.left, new Vector3(1f, 0f, 0f));
        CapsuleMotorComponent motor = new CapsuleMotorComponent(world, config);

        motor.Teleport(Vector3.zero);
        Step(motor, Vector3.right * PUSH_STEP_DISTANCE, PUSH_STEPS);

        float contactX = 1f - config.StandingRadius;
        Assert.LessOrEqual(motor.Position.x, contactX + POSITION_TOLERANCE);
        Assert.Greater(motor.Position.x, contactX - WALL_CONTACT_TOLERANCE);
    }

    [Test]
    public void Motor_SlidesAlongWallWhenMovingDiagonally()
    {
        PlaneCollisionWorldFake world = new PlaneCollisionWorldFake()
            .AddPlane(Vector3.up, Vector3.zero)
            .AddPlane(Vector3.left, new Vector3(1f, 0f, 0f));
        CapsuleMotorComponent motor = new CapsuleMotorComponent(world, config);

        motor.Teleport(Vector3.zero);
        Step(motor, (Vector3.right + Vector3.forward) * PUSH_STEP_DISTANCE, PUSH_STEPS);

        Assert.LessOrEqual(motor.Position.x, 1f - config.StandingRadius + POSITION_TOLERANCE);
        Assert.Greater(motor.Position.z, MIN_DISTANCE_SLID_ALONG_WALL);
    }

    [Test]
    public void Motor_CannotFitTallerShapeUnderLowCeiling()
    {
        PlaneCollisionWorldFake world = new PlaneCollisionWorldFake()
            .AddPlane(Vector3.up, Vector3.zero)
            .AddPlane(Vector3.down, new Vector3(0f, 0.6f, 0f));
        CapsuleMotorComponent motor = new CapsuleMotorComponent(world, config);

        motor.SetShape(config.CrawlingRadius, config.CrawlingHeight);
        motor.Teleport(Vector3.zero);
        Step(motor, Vector3.zero, SETTLE_STEPS);

        Assert.IsTrue(motor.CanFitShape(config.CrawlingRadius, config.CrawlingHeight));
        Assert.IsFalse(motor.CanFitShape(config.StandingRadius, config.CrouchedHeight));
        Assert.IsFalse(motor.CanFitShape(config.StandingRadius, config.StandingHeight));
    }

    [Test]
    public void Motor_StaysGroundedWhileWalkingOnFlatFloor()
    {
        PlaneCollisionWorldFake world = new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero);
        CapsuleMotorComponent motor = new CapsuleMotorComponent(world, config);

        motor.Teleport(Vector3.zero);
        Step(motor, Vector3.zero, SETTLE_STEPS);
        Step(motor, Vector3.forward * WALK_STEP_DISTANCE, WALK_STEPS);

        Assert.IsTrue(motor.IsGrounded);
        Assert.AreEqual(config.SkinWidth, motor.Position.y, POSITION_TOLERANCE);
        Assert.AreEqual(WALK_STEPS * WALK_STEP_DISTANCE, motor.Position.z, POSITION_TOLERANCE);
    }

    private void Step(CapsuleMotorComponent motor, Vector3 displacementPerStep, int steps)
    {
        for (int i = 0; i < steps; i++)
            motor.Move(displacementPerStep, FIXED_DELTA);
    }
}
