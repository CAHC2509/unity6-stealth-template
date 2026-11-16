using StealthController.Core;
using UnityEngine;

namespace StealthController.Unity
{
    public class UnityCollisionWorldAdapter : ICollisionWorld
    {
        private readonly LayerMask environmentMask;
        private readonly CapsuleCollider probeCollider;
        private readonly Collider[] overlapBuffer = new Collider[StealthUnityConstants.OVERLAP_BUFFER_SIZE];

        public UnityCollisionWorldAdapter(LayerMask environmentMask, CapsuleCollider probeCollider)
        {
            this.environmentMask = environmentMask;
            this.probeCollider = probeCollider;
        }

        public bool CapsuleCast(Vector3 bottom, Vector3 top, float radius, Vector3 direction, float distance, out CollisionHit hit)
        {
            if (!Physics.CapsuleCast(bottom, top, radius, direction, out RaycastHit raycastHit, distance, environmentMask, QueryTriggerInteraction.Ignore))
            {
                hit = default;
                return false;
            }

            hit = new CollisionHit(raycastHit.point, raycastHit.normal, raycastHit.distance);
            return true;
        }

        public bool Raycast(Vector3 origin, Vector3 direction, float distance, out CollisionHit hit)
        {
            if (!Physics.Raycast(origin, direction, out RaycastHit raycastHit, distance, environmentMask, QueryTriggerInteraction.Ignore))
            {
                hit = default;
                return false;
            }

            hit = new CollisionHit(raycastHit.point, raycastHit.normal, raycastHit.distance);
            return true;
        }

        public bool CheckCapsule(Vector3 bottom, Vector3 top, float radius)
        {
            return Physics.CheckCapsule(bottom, top, radius, environmentMask, QueryTriggerInteraction.Ignore);
        }

        public bool TryComputePenetration(Vector3 bottom, Vector3 top, float radius, out Vector3 correction)
        {
            correction = Vector3.zero;

            if (probeCollider == null) return false;

            int overlapCount = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, overlapBuffer, environmentMask, QueryTriggerInteraction.Ignore);
            if (overlapCount == 0) return false;

            Vector3 center = (bottom + top) * 0.5f;
            ConfigureProbe(bottom, top, radius);

            for (int i = 0; i < overlapCount; i++)
            {
                Collider other = overlapBuffer[i];
                Transform otherTransform = other.transform;

                if (!Physics.ComputePenetration(probeCollider, center, Quaternion.identity, other, otherTransform.position, otherTransform.rotation, out Vector3 direction, out float distance))
                    continue;

                correction += direction * distance;
            }

            return correction.sqrMagnitude > 0f;
        }

        private void ConfigureProbe(Vector3 bottom, Vector3 top, float radius)
        {
            probeCollider.direction = StealthUnityConstants.CAPSULE_DIRECTION_Y_AXIS;
            probeCollider.center = Vector3.zero;
            probeCollider.radius = radius;
            probeCollider.height = Vector3.Distance(bottom, top) + 2f * radius;
        }
    }
}
