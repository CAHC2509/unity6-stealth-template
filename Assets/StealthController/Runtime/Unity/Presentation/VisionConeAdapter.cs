using System.Collections.Generic;
using StealthController.Core;
using UnityEngine;

namespace StealthController.Unity
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class VisionConeAdapter : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [Header("Shape")]
        [SerializeField, Range(StealthConstants.VISION_CONE_MIN_SEGMENTS, StealthConstants.VISION_CONE_MAX_SEGMENTS)] private int segments = StealthConstants.VISION_CONE_DEFAULT_SEGMENTS;
        [SerializeField, Range(1, StealthConstants.VISION_CONE_MAX_RINGS), Tooltip("Rings between the inner and outer edge, so the cone can bend over slopes and steps.")] private int rings = StealthConstants.VISION_CONE_DEFAULT_RINGS;
        [SerializeField, Min(0f)] private float floorOffset = StealthConstants.VISION_CONE_FLOOR_OFFSET;

        [Header("Occlusion")]
        [SerializeField] private LayerMask occluderMask = StealthUnityConstants.DEFAULT_ENVIRONMENT_MASK;
        [SerializeField, Range(0, StealthConstants.VISION_CONE_MAX_EDGE_ITERATIONS), Tooltip("Binary search steps used to place the cone edge exactly where it meets a wall corner.")]
        private int edgeIterations = StealthConstants.VISION_CONE_EDGE_ITERATIONS;
        [SerializeField, Min(0f), Tooltip("Difference between neighbouring ray distances that counts as a wall edge.")]
        private float edgeDistanceThreshold = StealthConstants.VISION_CONE_EDGE_DISTANCE_THRESHOLD;

        [Header("Ground Projection")]
        [SerializeField] private LayerMask groundMask = StealthUnityConstants.DEFAULT_ENVIRONMENT_MASK;
        [SerializeField, Min(0f), Tooltip("Height above the eye from which the ground below each cone point is searched.")] private float groundProbeHeight = StealthConstants.VISION_CONE_GROUND_PROBE_HEIGHT;
        [SerializeField, Min(0f)] private float groundProbeDistance = StealthConstants.VISION_CONE_GROUND_PROBE_DISTANCE;
        [SerializeField, Min(StealthConstants.MIN_MOVE_DISTANCE), Tooltip("Ground heights are cached per cell of this size, since the level does not move.")] private float groundCellSize = StealthConstants.VISION_CONE_GROUND_CELL_SIZE;
        [SerializeField, Min(0f), Tooltip("A rise bigger than this between rings is the top of an obstacle, not ground, so the cone stays below it.")] private float maxGroundRise = StealthConstants.VISION_CONE_MAX_GROUND_RISE;

        [Header("Look")]
        [SerializeField, Range(0f, 1f)] private float alpha = StealthUnityConstants.VISION_CONE_ALPHA;

        private readonly List<Vector3> localVertices = new List<Vector3>();
        private readonly List<int> triangles = new List<int>();

        private VisionConeShapeLogic shapeLogic;
        private Mesh coneMesh;
        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            EnsureMesh();
        }

        public void Draw(Vector3 origin, float yaw, float viewRange, float blindRange, float fullAngle, Color color)
        {
            EnsureMesh();

            shapeLogic.Build(origin, yaw, viewRange, blindRange, fullAngle, transform.position.y);
            UploadShape();

            color.a = alpha;
            propertyBlock.SetColor(BaseColorId, color);
            meshRenderer.SetPropertyBlock(propertyBlock);
        }

        private void EnsureMesh()
        {
            if (coneMesh != null) return;

            meshRenderer = GetComponent<MeshRenderer>();
            propertyBlock = new MaterialPropertyBlock();

            coneMesh = new Mesh { name = "VisionCone" };
            coneMesh.MarkDynamic();

            GetComponent<MeshFilter>().sharedMesh = coneMesh;

            VisionConeShapeSettings settings = new VisionConeShapeSettings(segments, rings, floorOffset, edgeIterations, edgeDistanceThreshold, groundProbeHeight, groundProbeDistance, groundCellSize, maxGroundRise);
            shapeLogic = new VisionConeShapeLogic(settings, new UnityCollisionWorldAdapter(occluderMask, null), new UnityCollisionWorldAdapter(groundMask, null));
        }

        private void UploadShape()
        {
            localVertices.Clear();
            triangles.Clear();

            IReadOnlyList<Vector3> worldVertices = shapeLogic.Vertices;
            for (int i = 0; i < worldVertices.Count; i++)
                localVertices.Add(transform.InverseTransformPoint(worldVertices[i]));

            IReadOnlyList<int> shapeTriangles = shapeLogic.Triangles;
            for (int i = 0; i < shapeTriangles.Count; i++)
                triangles.Add(shapeTriangles[i]);

            coneMesh.Clear(true);
            coneMesh.SetVertices(localVertices);
            coneMesh.SetTriangles(triangles, 0, true);
        }
    }
}
