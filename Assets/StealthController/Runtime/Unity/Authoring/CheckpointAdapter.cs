using StealthController.Core;
using UnityEngine;

namespace StealthController.Unity
{
    public class CheckpointAdapter : GroundVolumeAdapterBase
    {
        [Header("Checkpoint")]
        [SerializeField, BasicSetting, Tooltip("A checkpoint only takes over when its order is not lower than the active one. The lowest order is the mission start.")] private int order;

        protected override Color GizmoColor => Color.green;

        public CheckpointData ToCheckpointData() => new CheckpointData(order, VolumeBounds, transform.position, transform.eulerAngles.y);
    }
}
