using System.Collections.Generic;
using StealthController.Core;
using UnityEngine;

public class StraightLinePathfinderFake : IPathfinder
{
    private bool isBlocked;

    public StraightLinePathfinderFake Blocked()
    {
        isBlocked = true;
        return this;
    }

    public bool TryFindPath(Vector3 start, Vector3 destination, List<Vector3> corners)
    {
        corners.Clear();

        if (isBlocked) return false;

        corners.Add(start);
        corners.Add(destination);
        return true;
    }

    public bool TrySnapToWalkable(Vector3 point, float maxDistance, out Vector3 snappedPoint)
    {
        snappedPoint = point;
        return !isBlocked;
    }
}
