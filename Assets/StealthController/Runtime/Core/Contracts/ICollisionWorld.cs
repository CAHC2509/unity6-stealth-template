using UnityEngine;

namespace StealthController.Core
{
    public interface ICollisionWorld
    {
        bool CapsuleCast(Vector3 bottom, Vector3 top, float radius, Vector3 direction, float distance, out CollisionHit hit);
        bool Raycast(Vector3 origin, Vector3 direction, float distance, out CollisionHit hit);
        bool CheckCapsule(Vector3 bottom, Vector3 top, float radius);
        bool TryComputePenetration(Vector3 bottom, Vector3 top, float radius, out Vector3 correction);
    }
}
