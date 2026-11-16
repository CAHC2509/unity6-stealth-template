using UnityEngine;

namespace StealthController.Core
{
    public class WallSensorLogic
    {
        private readonly ICollisionWorld collisionWorld;
        private readonly PlayerMovementConfig config;

        public WallSensorLogic(ICollisionWorld collisionWorld, PlayerMovementConfig config)
        {
            this.collisionWorld = collisionWorld;
            this.config = config;
        }

        public bool TryDetectWall(Vector3 position, float radius, Vector3 direction, float probeHeight, out WallContact contact)
        {
            contact = default;

            if (direction.sqrMagnitude < StealthConstants.MIN_MOVE_DISTANCE) return false;

            Vector3 origin = position + Vector3.up * probeHeight;
            if (!collisionWorld.Raycast(origin, direction, radius + config.WallDetectDistance, out CollisionHit hit)) return false;

            return TryCreateContact(hit, out contact);
        }

        public bool TryReacquire(Vector3 position, float radius, Vector3 wallNormal, float probeHeight, out WallContact contact)
        {
            if (!TryDetectWall(position, radius, -wallNormal, probeHeight, out contact)) return false;

            return Vector3.Angle(contact.Normal, wallNormal) <= StealthConstants.WALL_REACQUIRE_MAX_ANGLE;
        }

        public bool HasWallAt(Vector3 position, float radius, Vector3 wallNormal, float probeHeight)
        {
            return TryReacquire(position, radius, wallNormal, probeHeight, out WallContact contact);
        }

        public bool TryFindInnerCornerWall(Vector3 position, float radius, Vector3 wallNormal, Vector3 lateralDirection, float reach, float probeHeight, out WallContact contact)
        {
            if (!TryDetectWall(position, radius, lateralDirection, probeHeight, out contact)) return false;
            if (contact.Distance > reach) return false;

            return Vector3.Angle(contact.Normal, wallNormal) >= StealthConstants.WALL_CORNER_MIN_TURN_ANGLE;
        }

        public bool TryFindOuterCornerFace(Vector3 position, float wallDistance, Vector3 wallNormal, Vector3 lateralDirection, float reach, float probeHeight, out WallContact contact)
        {
            contact = default;

            Vector3 origin = position + Vector3.up * probeHeight + lateralDirection * reach - wallNormal * (wallDistance + config.CornerDepthProbe);
            if (!collisionWorld.Raycast(origin, -lateralDirection, reach, out CollisionHit hit)) return false;
            if (!TryCreateContact(hit, out contact)) return false;

            return Vector3.Angle(contact.Normal, wallNormal) >= StealthConstants.WALL_CORNER_MIN_TURN_ANGLE;
        }

        public static bool TryIntersectFaces(WallContact first, WallContact second, out Vector3 edge)
        {
            Vector3 alongSecond = Vector3.Cross(Vector3.up, second.Normal);
            float alignment = Vector3.Dot(alongSecond, first.Normal);
            edge = second.Point;

            if (Mathf.Abs(alignment) < StealthConstants.MIN_MOVE_DISTANCE) return false;

            edge = second.Point + alongSecond * (Vector3.Dot(first.Point - second.Point, first.Normal) / alignment);
            return true;
        }

        public bool IsPushingInto(Vector3 inputDirection, WallContact contact)
        {
            return Vector3.Angle(inputDirection, -contact.Normal) <= config.WallAngleTolerance;
        }

        public bool ReachesHeight(Vector3 position, float radius, Vector3 wallNormal, float height)
        {
            Vector3 origin = position + Vector3.up * (height - StealthConstants.WALL_HEIGHT_CHECK_MARGIN);
            return collisionWorld.Raycast(origin, -wallNormal, radius + config.WallDetectDistance, out CollisionHit hit);
        }

        private bool TryCreateContact(CollisionHit hit, out WallContact contact)
        {
            contact = default;

            if (Vector3.Angle(hit.Normal, Vector3.up) <= config.SlopeLimit) return false;

            Vector3 flattened = new Vector3(hit.Normal.x, 0f, hit.Normal.z);
            if (flattened.sqrMagnitude < StealthConstants.MIN_MOVE_DISTANCE) return false;

            contact = new WallContact(hit.Point, flattened.normalized, hit.Distance);
            return true;
        }
    }
}
