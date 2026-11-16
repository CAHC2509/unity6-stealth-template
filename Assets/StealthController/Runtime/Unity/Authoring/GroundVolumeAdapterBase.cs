using StealthController.Core;
using UnityEngine;

namespace StealthController.Unity
{
    public abstract class GroundVolumeAdapterBase : MonoBehaviour
    {
        [Header("Volume")]
        [SerializeField, BasicSetting, Tooltip("Size of the area in metres: width, height and depth.")] private Vector3 size = new Vector3(
            StealthUnityConstants.MISSION_VOLUME_DEFAULT_WIDTH,
            StealthUnityConstants.MISSION_VOLUME_DEFAULT_HEIGHT,
            StealthUnityConstants.MISSION_VOLUME_DEFAULT_DEPTH);

        [Header("Debug")]
        [SerializeField] private bool drawGizmo = true;

        protected Bounds VolumeBounds => new Bounds(transform.position + Vector3.up * (size.y * 0.5f), size);

        protected abstract Color GizmoColor { get; }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawGizmo) return;

            Bounds bounds = VolumeBounds;
            Gizmos.color = GizmoColor;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            Gizmos.DrawRay(transform.position, transform.forward * StealthUnityConstants.MISSION_GIZMO_ARROW_LENGTH);
        }
#endif
    }
}
