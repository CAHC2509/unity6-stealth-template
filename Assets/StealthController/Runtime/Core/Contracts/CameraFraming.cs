using UnityEngine;

namespace StealthController.Core
{
    public readonly struct CameraFraming
    {
        public Vector3 Pivot { get; }
        public float Pitch { get; }
        public float Yaw { get; }
        public float Distance { get; }
        public float FieldOfView { get; }

        public Quaternion Rotation => Quaternion.Euler(Pitch, Yaw, 0f);
        public Vector3 Position => Pivot - Rotation * Vector3.forward * Distance;

        public CameraFraming(Vector3 pivot, float pitch, float yaw, float distance, float fieldOfView)
        {
            Pivot = pivot;
            Pitch = pitch;
            Yaw = yaw;
            Distance = distance;
            FieldOfView = fieldOfView;
        }
    }
}
