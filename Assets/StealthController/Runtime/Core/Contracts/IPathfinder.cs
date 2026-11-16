using System.Collections.Generic;
using UnityEngine;

namespace StealthController.Core
{
    public interface IPathfinder
    {
        bool TryFindPath(Vector3 start, Vector3 destination, List<Vector3> corners);
        bool TrySnapToWalkable(Vector3 point, float maxDistance, out Vector3 snappedPoint);
    }
}
