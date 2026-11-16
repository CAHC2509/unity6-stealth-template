using System.Collections.Generic;
using UnityEngine;

namespace StealthController.Core
{
    public class PathFollowerComponent
    {
        private readonly IPathfinder pathfinder;
        private readonly FacingComponent facing;
        private readonly List<Vector3> pathCorners = new List<Vector3>();
        private readonly float acceleration;
        private readonly float walkableSnapDistance;
        private readonly float turnSmoothTime;
        private readonly float turnSpeed;

        private Vector3 position;
        private float travelSpeed;
        private int cornerIndex;

        public Vector3 Position => position;

        public PathFollowerComponent(IPathfinder pathfinder, FacingComponent facing, float acceleration, float walkableSnapDistance, float turnSmoothTime, float turnSpeed)
        {
            this.pathfinder = pathfinder;
            this.facing = facing;
            this.acceleration = acceleration;
            this.walkableSnapDistance = walkableSnapDistance;
            this.turnSmoothTime = turnSmoothTime;
            this.turnSpeed = turnSpeed;
        }

        public void Reset(Vector3 startPosition)
        {
            position = startPosition;
            travelSpeed = 0f;
            pathCorners.Clear();
            cornerIndex = 0;
        }

        public bool TrySetDestination(Vector3 destination)
        {
            pathCorners.Clear();
            cornerIndex = 0;

            if (!pathfinder.TrySnapToWalkable(destination, walkableSnapDistance, out Vector3 walkableDestination)) return false;
            if (!pathfinder.TryFindPath(position, walkableDestination, pathCorners)) return false;

            cornerIndex = pathCorners.Count > 1 ? 1 : 0;
            return true;
        }

        public bool FollowPath(float speed, float deltaTime)
        {
            float arrivalSpeed = Mathf.Sqrt(2f * acceleration * RemainingPathDistance());
            travelSpeed = Mathf.MoveTowards(travelSpeed, Mathf.Min(speed, arrivalSpeed), acceleration * deltaTime);
            float remainingTravel = travelSpeed * deltaTime;

            while (cornerIndex < pathCorners.Count)
            {
                Vector3 corner = pathCorners[cornerIndex];
                float distance = Vector3.Distance(position, corner);

                if (distance > remainingTravel)
                {
                    facing.RotateTowards(corner - position, turnSmoothTime, turnSpeed, deltaTime);
                    position = Vector3.MoveTowards(position, corner, remainingTravel);
                    return false;
                }

                position = corner;
                remainingTravel -= distance;
                cornerIndex++;
            }

            travelSpeed = 0f;
            return true;
        }

        public void Halt()
        {
            travelSpeed = 0f;
        }

        public float HorizontalDistanceTo(Vector3 point)
        {
            Vector3 offset = point - position;
            return new Vector2(offset.x, offset.z).magnitude;
        }

        private float RemainingPathDistance()
        {
            float distance = 0f;
            Vector3 from = position;

            for (int i = cornerIndex; i < pathCorners.Count; i++)
            {
                distance += Vector3.Distance(from, pathCorners[i]);
                from = pathCorners[i];
            }

            return distance;
        }
    }
}
