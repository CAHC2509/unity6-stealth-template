using System.Collections.Generic;
using NUnit.Framework;
using StealthController.Core;
using UnityEngine;

public class VisionConeShapeLogicTests
{
    private const float EYE_HEIGHT = 1.6f;
    private const float VIEW_RANGE = 9f;
    private const float FULL_ANGLE = 60f;
    private const float BLIND_RANGE = 1.8f;
    private const float POSITION_TOLERANCE = 0.01f;

    private VisionConeShapeSettings settings;

    [SetUp]
    public void SetUp()
    {
        settings = new VisionConeShapeSettings(
            StealthConstants.VISION_CONE_DEFAULT_SEGMENTS,
            StealthConstants.VISION_CONE_DEFAULT_RINGS,
            StealthConstants.VISION_CONE_FLOOR_OFFSET,
            StealthConstants.VISION_CONE_EDGE_ITERATIONS,
            StealthConstants.VISION_CONE_EDGE_DISTANCE_THRESHOLD,
            StealthConstants.VISION_CONE_GROUND_PROBE_HEIGHT,
            StealthConstants.VISION_CONE_GROUND_PROBE_DISTANCE,
            StealthConstants.VISION_CONE_GROUND_CELL_SIZE,
            StealthConstants.VISION_CONE_MAX_GROUND_RISE);
    }

    [Test]
    public void OpenFloor_LiesOnTheGroundUpToFullRange()
    {
        PlaneCollisionWorldFake world = new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero);
        VisionConeShapeLogic shape = new VisionConeShapeLogic(settings, world, world);

        shape.Build(new Vector3(0f, EYE_HEIGHT, 0f), 0f, VIEW_RANGE, 0f, FULL_ANGLE, 0f);

        float farthest = 0f;
        foreach (Vector3 vertex in shape.Vertices)
        {
            Assert.AreEqual(StealthConstants.VISION_CONE_FLOOR_OFFSET, vertex.y, POSITION_TOLERANCE);
            farthest = Mathf.Max(farthest, new Vector2(vertex.x, vertex.z).magnitude);
        }

        Assert.AreEqual(VIEW_RANGE, farthest, POSITION_TOLERANCE);
    }

    [Test]
    public void WallInFront_CutsTheConeAtTheWall()
    {
        PlaneCollisionWorldFake world = new PlaneCollisionWorldFake()
            .AddPlane(Vector3.up, Vector3.zero)
            .AddPlane(Vector3.back, new Vector3(0f, 0f, 4f));
        VisionConeShapeLogic shape = new VisionConeShapeLogic(settings, world, world);

        shape.Build(new Vector3(0f, EYE_HEIGHT, 0f), 0f, VIEW_RANGE, 0f, FULL_ANGLE, 0f);

        foreach (Vector3 vertex in shape.Vertices)
            Assert.LessOrEqual(vertex.z, 4f + POSITION_TOLERANCE);
    }

    [Test]
    public void BlindRange_StartsTheConeAwayFromTheEye()
    {
        PlaneCollisionWorldFake world = new PlaneCollisionWorldFake().AddPlane(Vector3.up, Vector3.zero);
        VisionConeShapeLogic shape = new VisionConeShapeLogic(settings, world, world);

        shape.Build(new Vector3(0f, EYE_HEIGHT, 0f), 0f, VIEW_RANGE, BLIND_RANGE, FULL_ANGLE, 0f);

        IReadOnlyList<Vector3> vertices = shape.Vertices;
        foreach (Vector3 vertex in vertices)
            Assert.GreaterOrEqual(new Vector2(vertex.x, vertex.z).magnitude, BLIND_RANGE - POSITION_TOLERANCE);
    }
}
