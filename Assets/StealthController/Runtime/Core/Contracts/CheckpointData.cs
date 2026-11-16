using UnityEngine;

namespace StealthController.Core
{
    public readonly struct CheckpointData
    {
        public int Order { get; }
        public Bounds Bounds { get; }
        public Vector3 SpawnPosition { get; }
        public float SpawnYaw { get; }

        public CheckpointData(int order, Bounds bounds, Vector3 spawnPosition, float spawnYaw)
        {
            Order = order;
            Bounds = bounds;
            SpawnPosition = spawnPosition;
            SpawnYaw = spawnYaw;
        }
    }
}
