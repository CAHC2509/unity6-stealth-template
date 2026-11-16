using System.Collections.Generic;
using UnityEngine;

namespace StealthController.Core
{
    public class VisionConeShapeLogic
    {
        private readonly VisionConeShapeSettings settings;
        private readonly ICollisionWorld occluders;
        private readonly ICollisionWorld ground;
        private readonly List<ViewCast> boundary = new List<ViewCast>();
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<int> triangles = new List<int>();
        private readonly Dictionary<(int, int, int), GroundSample> groundSamples = new Dictionary<(int, int, int), GroundSample>();

        private Vector3 sightOrigin;
        private float range;
        private float minRange;
        private float lastGroundHeight;

        public IReadOnlyList<Vector3> Vertices => vertices;
        public IReadOnlyList<int> Triangles => triangles;

        public VisionConeShapeLogic(VisionConeShapeSettings settings, ICollisionWorld occluders, ICollisionWorld ground)
        {
            this.settings = settings;
            this.occluders = occluders;
            this.ground = ground;
        }

        public void Build(Vector3 origin, float yaw, float viewRange, float blindRange, float fullAngle, float fallbackGroundHeight)
        {
            sightOrigin = origin;
            range = viewRange;
            minRange = Mathf.Min(blindRange, viewRange);
            lastGroundHeight = TryGetGroundHeight(origin.x, origin.z, out float originGround) ? originGround : fallbackGroundHeight;

            BuildVisibilityBoundary(yaw - fullAngle * 0.5f, fullAngle / settings.Segments);
            BuildProjectedShape();
        }

        private void BuildVisibilityBoundary(float startYaw, float yawStep)
        {
            boundary.Clear();

            ViewCast previous = Cast(startYaw);
            boundary.Add(previous);

            for (int i = 1; i <= settings.Segments; i++)
            {
                ViewCast current = Cast(startYaw + yawStep * i);

                if (IsEdgeBetween(previous, current))
                {
                    FindEdge(previous, current, out ViewCast nearSide, out ViewCast farSide);
                    boundary.Add(nearSide);
                    boundary.Add(farSide);
                }

                boundary.Add(current);
                previous = current;
            }
        }

        private void BuildProjectedShape()
        {
            vertices.Clear();
            triangles.Clear();

            int rings = settings.Rings;
            int ringVertexCount = rings + 1;

            for (int i = 0; i < boundary.Count; i++)
                AddRadialStrip(boundary[i]);

            for (int i = 0; i < boundary.Count - 1; i++)
            {
                int strip = i * ringVertexCount;
                int nextStrip = strip + ringVertexCount;

                for (int ring = 0; ring < rings; ring++)
                {
                    int inner = strip + ring;
                    int outer = inner + 1;
                    int nextInner = nextStrip + ring;
                    int nextOuter = nextInner + 1;

                    triangles.Add(inner);
                    triangles.Add(outer);
                    triangles.Add(nextOuter);

                    triangles.Add(inner);
                    triangles.Add(nextOuter);
                    triangles.Add(nextInner);
                }
            }
        }

        private void AddRadialStrip(ViewCast cast)
        {
            int rings = settings.Rings;
            float innerDistance = Mathf.Min(minRange, cast.Distance);
            float groundHeight = lastGroundHeight;

            for (int ring = 0; ring <= rings; ring++)
            {
                float distance = Mathf.Lerp(innerDistance, cast.Distance, (float)ring / rings);
                Vector3 flatPoint = sightOrigin + cast.Direction * distance;

                if (TryGetGroundHeight(flatPoint.x, flatPoint.z, out float height) && height - groundHeight <= settings.MaxGroundRise)
                    groundHeight = height;

                if (ring == 0)
                    lastGroundHeight = groundHeight;

                vertices.Add(new Vector3(flatPoint.x, groundHeight + settings.FloorOffset, flatPoint.z));
            }
        }

        private bool TryGetGroundHeight(float x, float z, out float height)
        {
            float cellSize = settings.GroundCellSize;
            int cellX = Mathf.FloorToInt(x / cellSize);
            int cellZ = Mathf.FloorToInt(z / cellSize);
            int band = Mathf.FloorToInt(sightOrigin.y / StealthConstants.VISION_CONE_GROUND_HEIGHT_BAND);
            (int, int, int) key = (cellX, cellZ, band);

            if (!groundSamples.TryGetValue(key, out GroundSample sample))
            {
                Vector3 probeStart = new Vector3((cellX + 0.5f) * cellSize, sightOrigin.y + settings.GroundProbeHeight, (cellZ + 0.5f) * cellSize);
                bool hasGround = ground.Raycast(probeStart, Vector3.down, settings.GroundProbeDistance, out CollisionHit hit)
                    && hit.Normal.y >= StealthConstants.VISION_CONE_MIN_GROUND_NORMAL_Y;
                sample = hasGround ? new GroundSample(true, hit.Point, hit.Normal) : default;
                groundSamples.Add(key, sample);
            }

            height = sample.HeightAt(x, z);
            return sample.HasGround;
        }

        private ViewCast Cast(float yaw)
        {
            float radians = yaw * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians));
            bool hasHit = occluders.Raycast(sightOrigin, direction, range, out CollisionHit hit);
            float distance = hasHit ? hit.Distance : range;

            return new ViewCast(yaw, direction, hasHit, distance);
        }

        private bool IsEdgeBetween(ViewCast first, ViewCast second)
        {
            if (first.HasHit != second.HasHit) return true;

            return first.HasHit && Mathf.Abs(first.Distance - second.Distance) > settings.EdgeDistanceThreshold;
        }

        private void FindEdge(ViewCast first, ViewCast second, out ViewCast nearSide, out ViewCast farSide)
        {
            nearSide = first;
            farSide = second;

            for (int i = 0; i < settings.EdgeIterations; i++)
            {
                ViewCast middle = Cast((nearSide.Yaw + farSide.Yaw) * 0.5f);

                if (IsEdgeBetween(nearSide, middle))
                    farSide = middle;
                else
                    nearSide = middle;
            }
        }

        private readonly struct GroundSample
        {
            public bool HasGround { get; }
            public Vector3 Point { get; }
            public Vector3 Normal { get; }

            public GroundSample(bool hasGround, Vector3 point, Vector3 normal)
            {
                HasGround = hasGround;
                Point = point;
                Normal = normal;
            }

            public float HeightAt(float x, float z)
            {
                if (!HasGround) return float.NaN;

                return Point.y - (Normal.x * (x - Point.x) + Normal.z * (z - Point.z)) / Normal.y;
            }
        }

        private readonly struct ViewCast
        {
            public float Yaw { get; }
            public Vector3 Direction { get; }
            public bool HasHit { get; }
            public float Distance { get; }

            public ViewCast(float yaw, Vector3 direction, bool hasHit, float distance)
            {
                Yaw = yaw;
                Direction = direction;
                HasHit = hasHit;
                Distance = distance;
            }
        }
    }
}
