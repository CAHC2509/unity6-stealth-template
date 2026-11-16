using StealthController.Core;
using UnityEngine;

namespace StealthController.Unity
{
    public class MissionExitAdapter : GroundVolumeAdapterBase
    {
        protected override Color GizmoColor => Color.magenta;

        public Bounds ToBounds() => VolumeBounds;
    }
}
