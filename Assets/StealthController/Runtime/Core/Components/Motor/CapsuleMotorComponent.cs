using UnityEngine;

namespace StealthController.Core
{
    public class CapsuleMotorComponent
    {
        private readonly ICollisionWorld collisionWorld;
        private readonly PlayerMovementConfig config;

        private Vector3 position;
        private float radius;
        private float height;
        private float verticalSpeed;
        private bool isGrounded;
        private Vector3 groundNormal = Vector3.up;

        public Vector3 Position => position;
        public float Radius => radius;
        public float Height => height;
        public bool IsGrounded => isGrounded;
        public Vector3 GroundNormal => groundNormal;

        public Vector3 BottomSphereCenter => position + Vector3.up * radius;
        public Vector3 TopSphereCenter => position + Vector3.up * Mathf.Max(radius, height - radius);

        public CapsuleMotorComponent(ICollisionWorld collisionWorld, PlayerMovementConfig config)
        {
            this.collisionWorld = collisionWorld;
            this.config = config;

            radius = config.StandingRadius;
            height = config.StandingHeight;
        }

        public void Teleport(Vector3 targetPosition)
        {
            position = targetPosition + Vector3.up * config.SkinWidth;
            verticalSpeed = 0f;
            isGrounded = false;
            groundNormal = Vector3.up;
        }

        public bool CanFitShape(float targetRadius, float targetHeight)
        {
            float skin = config.SkinWidth;
            float checkRadius = Mathf.Min(radius, targetRadius) - skin;
            float floorClearance = StealthConstants.SHAPE_CHECK_FLOOR_CLEARANCE_SKINS * skin;
            Vector3 bottom = position + Vector3.up * (checkRadius + floorClearance);
            Vector3 top = position + Vector3.up * Mathf.Max(checkRadius + floorClearance, targetHeight - checkRadius - skin);

            return !collisionWorld.CheckCapsule(bottom, top, checkRadius);
        }

        public void SetShape(float targetRadius, float targetHeight)
        {
            radius = targetRadius;
            height = targetHeight;
        }

        public void Move(Vector3 horizontalDisplacement, float deltaTime)
        {
            ResolvePenetration();

            if (isGrounded)
                MoveGrounded(horizontalDisplacement);
            else
                MoveAirborne(horizontalDisplacement, deltaTime);
        }

        private void ResolvePenetration()
        {
            if (!collisionWorld.TryComputePenetration(BottomSphereCenter, TopSphereCenter, radius, out Vector3 correction)) return;

            position += correction;
        }

        private void MoveGrounded(Vector3 horizontalDisplacement)
        {
            Vector3 start = position;
            bool hitWall = Slide(horizontalDisplacement, true);

            if (hitWall && TryStepUp(start, horizontalDisplacement)) return;
            if (TrySnapToGround(config.GroundSnapDistance)) return;

            isGrounded = false;
            verticalSpeed = 0f;
            groundNormal = Vector3.up;
        }

        private bool TryStepUp(Vector3 start, Vector3 horizontalDisplacement)
        {
            Vector3 flatEnd = position;
            float flatTravel = HorizontalDistance(start, flatEnd);

            position = start;
            float stepHeight = ComputeStepClearance();

            if (stepHeight > StealthConstants.MIN_MOVE_DISTANCE)
            {
                position += Vector3.up * stepHeight;
                Slide(horizontalDisplacement, true);

                bool advancedFurther = HorizontalDistance(start, position) > flatTravel + StealthConstants.MIN_MOVE_DISTANCE;
                if (advancedFurther && TrySnapToGround(stepHeight + config.GroundSnapDistance)) return true;
            }

            position = flatEnd;
            return false;
        }

        private static float HorizontalDistance(Vector3 from, Vector3 to)
        {
            Vector3 offset = to - from;
            offset.y = 0f;
            return offset.magnitude;
        }

        private void MoveAirborne(Vector3 horizontalDisplacement, float deltaTime)
        {
            Slide(horizontalDisplacement, true);

            verticalSpeed = Mathf.Max(verticalSpeed - config.Gravity * deltaTime, -config.MaxFallSpeed);
            float verticalDisplacement = verticalSpeed * deltaTime;

            if (verticalDisplacement < 0f && TrySnapToGround(-verticalDisplacement))
            {
                verticalSpeed = 0f;
                return;
            }

            Slide(Vector3.up * verticalDisplacement, false);
        }

        private float ComputeStepClearance()
        {
            if (config.StepOffset <= 0f) return 0f;

            if (!Cast(Vector3.up, config.StepOffset + config.SkinWidth, out CollisionHit hit))
                return config.StepOffset;

            return Mathf.Max(hit.Distance - config.SkinWidth, 0f);
        }

        private bool TrySnapToGround(float distance)
        {
            if (!Cast(Vector3.down, distance + config.SkinWidth, out CollisionHit hit)) return false;
            if (!TryResolveGroundNormal(hit, out Vector3 surfaceNormal)) return false;

            position += Vector3.down * Mathf.Max(hit.Distance - config.SkinWidth, 0f);
            isGrounded = true;
            groundNormal = surfaceNormal;
            return true;
        }

        private bool TryResolveGroundNormal(CollisionHit hit, out Vector3 surfaceNormal)
        {
            surfaceNormal = hit.Normal;
            if (IsWalkable(hit.Normal)) return true;

            Vector3 outward = hit.Point - position;
            outward.y = 0f;
            if (outward.sqrMagnitude < StealthConstants.MIN_MOVE_DISTANCE) return false;

            Vector3 probeOrigin = hit.Point
                + outward.normalized * StealthConstants.GROUND_EDGE_PROBE_INSET
                + Vector3.up * StealthConstants.GROUND_EDGE_PROBE_HEIGHT;

            if (!collisionWorld.Raycast(probeOrigin, Vector3.down, StealthConstants.GROUND_EDGE_PROBE_LENGTH, out CollisionHit surfaceHit)) return false;
            if (!IsWalkable(surfaceHit.Normal)) return false;

            surfaceNormal = surfaceHit.Normal;
            return true;
        }

        private bool Slide(Vector3 displacement, bool treatSteepSurfacesAsWalls)
        {
            Vector3 remaining = displacement;
            bool hitWall = false;

            for (int iteration = 0; iteration < config.MaxSlideIterations; iteration++)
            {
                float distance = remaining.magnitude;
                if (distance < StealthConstants.MIN_MOVE_DISTANCE) return hitWall;

                Vector3 direction = remaining / distance;

                if (!Cast(direction, distance + config.SkinWidth, out CollisionHit hit))
                {
                    position += remaining;
                    return hitWall;
                }

                float travel = Mathf.Max(hit.Distance - config.SkinWidth, 0f);
                position += direction * travel;

                hitWall |= !IsWalkable(hit.Normal);

                Vector3 slideNormal = treatSteepSurfacesAsWalls ? ToWallNormal(hit.Normal) : hit.Normal;
                if (slideNormal == Vector3.zero) return hitWall;

                remaining = Vector3.ProjectOnPlane(direction * (distance - travel), slideNormal);
            }

            return hitWall;
        }

        private Vector3 ToWallNormal(Vector3 normal)
        {
            if (IsWalkable(normal)) return normal;

            Vector3 flattened = new Vector3(normal.x, 0f, normal.z);
            if (flattened.sqrMagnitude < StealthConstants.MIN_MOVE_DISTANCE) return Vector3.zero;

            return flattened.normalized;
        }

        private bool IsWalkable(Vector3 normal) => Vector3.Angle(normal, Vector3.up) <= config.SlopeLimit;

        private bool Cast(Vector3 direction, float distance, out CollisionHit hit)
        {
            return collisionWorld.CapsuleCast(BottomSphereCenter, TopSphereCenter, radius, direction, distance, out hit);
        }
    }
}
