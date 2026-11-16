namespace StealthController.Core
{
    public readonly struct VisionConeShapeSettings
    {
        public int Segments { get; }
        public int Rings { get; }
        public float FloorOffset { get; }
        public int EdgeIterations { get; }
        public float EdgeDistanceThreshold { get; }
        public float GroundProbeHeight { get; }
        public float GroundProbeDistance { get; }
        public float GroundCellSize { get; }
        public float MaxGroundRise { get; }

        public VisionConeShapeSettings(
            int segments,
            int rings,
            float floorOffset,
            int edgeIterations,
            float edgeDistanceThreshold,
            float groundProbeHeight,
            float groundProbeDistance,
            float groundCellSize,
            float maxGroundRise)
        {
            Segments = segments;
            Rings = rings;
            FloorOffset = floorOffset;
            EdgeIterations = edgeIterations;
            EdgeDistanceThreshold = edgeDistanceThreshold;
            GroundProbeHeight = groundProbeHeight;
            GroundProbeDistance = groundProbeDistance;
            GroundCellSize = groundCellSize;
            MaxGroundRise = maxGroundRise;
        }
    }
}
