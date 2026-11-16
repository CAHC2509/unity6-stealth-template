using System.Collections.Generic;
using StealthController.Core;
using UnityEngine.AI;
using UnityEngine;

namespace StealthController.Unity
{
    public class NavMeshPathfinderAdapter : IPathfinder
    {
        private readonly NavMeshPath path = new NavMeshPath();
        private readonly Vector3[] cornerBuffer = new Vector3[StealthUnityConstants.NAV_PATH_CORNER_BUFFER_SIZE];
        private readonly int areaMask;

        public NavMeshPathfinderAdapter(int areaMask)
        {
            this.areaMask = areaMask;
        }

        public bool TryFindPath(Vector3 start, Vector3 destination, List<Vector3> corners)
        {
            corners.Clear();

            if (!NavMesh.CalculatePath(start, destination, areaMask, path)) return false;
            if (path.status != NavMeshPathStatus.PathComplete) return false;

            int cornerCount = path.GetCornersNonAlloc(cornerBuffer);

            for (int i = 0; i < cornerCount; i++)
                corners.Add(cornerBuffer[i]);

            return cornerCount > 0;
        }

        public bool TrySnapToWalkable(Vector3 point, float maxDistance, out Vector3 snappedPoint)
        {
            bool found = NavMesh.SamplePosition(point, out NavMeshHit hit, maxDistance, areaMask);
            snappedPoint = found ? hit.position : point;
            return found;
        }
    }
}
