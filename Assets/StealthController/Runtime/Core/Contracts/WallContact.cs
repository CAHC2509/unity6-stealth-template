using UnityEngine;

namespace StealthController.Core
{
    public readonly struct WallContact
    {
        public Vector3 Point { get; }
        public Vector3 Normal { get; }
        public float Distance { get; }

        public WallContact(Vector3 point, Vector3 normal, float distance)
        {
            Point = point;
            Normal = normal;
            Distance = distance;
        }
    }
}
