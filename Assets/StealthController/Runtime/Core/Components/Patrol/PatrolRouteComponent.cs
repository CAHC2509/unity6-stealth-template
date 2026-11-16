using System.Collections.Generic;
using UnityEngine;

namespace StealthController.Core
{
    public class PatrolRouteComponent
    {
        private readonly List<GuardWaypoint> waypoints = new List<GuardWaypoint>();

        private int currentIndex;

        public IReadOnlyList<GuardWaypoint> Waypoints => waypoints;
        public bool HasRoute => waypoints.Count > 0;
        public GuardWaypoint Current => waypoints[currentIndex];

        public PatrolRouteComponent(IReadOnlyList<GuardWaypoint> route)
        {
            for (int i = 0; i < route.Count; i++)
                waypoints.Add(route[i]);
        }

        public void StartFromNearest(Vector3 position)
        {
            int nearestIndex = 0;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < waypoints.Count; i++)
            {
                Vector3 offset = waypoints[i].Position - position;
                float distance = new Vector2(offset.x, offset.z).magnitude;
                if (distance >= nearestDistance) continue;

                nearestDistance = distance;
                nearestIndex = i;
            }

            currentIndex = nearestIndex;
        }

        public void AdvanceToNext()
        {
            currentIndex = (currentIndex + 1) % waypoints.Count;
        }
    }
}
