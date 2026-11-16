using System.Collections.Generic;
using StealthController.Core;
using UnityEngine;

public class PlaneCollisionWorldFake : ICollisionWorld
{
    private readonly List<Vector3> normals = new List<Vector3>();
    private readonly List<float> offsets = new List<float>();

    public PlaneCollisionWorldFake AddPlane(Vector3 freeSideNormal, Vector3 pointOnPlane)
    {
        Vector3 normal = freeSideNormal.normalized;

        normals.Add(normal);
        offsets.Add(Vector3.Dot(normal, pointOnPlane));
        return this;
    }

    public bool CapsuleCast(Vector3 bottom, Vector3 top, float radius, Vector3 direction, float distance, out CollisionHit hit)
    {
        hit = default;
        bool hasHit = false;
        float closestTravel = distance;

        for (int i = 0; i < normals.Count; i++)
        {
            Vector3 normal = normals[i];
            float approachSpeed = -Vector3.Dot(direction, normal);
            if (approachSpeed <= 0f) continue;

            float clearance = ComputeClearance(i, bottom, top, radius);
            float travel = Mathf.Max(clearance, 0f) / approachSpeed;
            if (travel > closestTravel) continue;

            Vector3 closestCenter = Vector3.Dot(normal, bottom) <= Vector3.Dot(normal, top) ? bottom : top;

            closestTravel = travel;
            hasHit = true;
            hit = new CollisionHit(closestCenter + direction * travel - normal * radius, normal, travel);
        }

        return hasHit;
    }

    public bool Raycast(Vector3 origin, Vector3 direction, float distance, out CollisionHit hit)
    {
        return CapsuleCast(origin, origin, 0f, direction, distance, out hit);
    }

    public bool CheckCapsule(Vector3 bottom, Vector3 top, float radius)
    {
        for (int i = 0; i < normals.Count; i++)
        {
            if (ComputeClearance(i, bottom, top, radius) < 0f)
                return true;
        }

        return false;
    }

    public bool TryComputePenetration(Vector3 bottom, Vector3 top, float radius, out Vector3 correction)
    {
        correction = Vector3.zero;

        for (int i = 0; i < normals.Count; i++)
        {
            float clearance = ComputeClearance(i, bottom, top, radius);
            if (clearance >= 0f) continue;

            correction += normals[i] * -clearance;
        }

        return correction.sqrMagnitude > 0f;
    }

    private float ComputeClearance(int planeIndex, Vector3 bottom, Vector3 top, float radius)
    {
        Vector3 normal = normals[planeIndex];
        float closestSignedDistance = Mathf.Min(Vector3.Dot(normal, bottom), Vector3.Dot(normal, top)) - offsets[planeIndex];
        return closestSignedDistance - radius;
    }
}
