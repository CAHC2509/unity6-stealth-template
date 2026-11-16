using StealthController.Core;
using UnityEngine;

namespace StealthController.Unity
{
    public class SurveillanceCameraAdapter : StealthBehaviourBase
    {
        [Header("Behaviour")]
        [SerializeField] private SurveillanceCameraConfig cameraConfig = new SurveillanceCameraConfig();

        [Header("Presentation")]
        [SerializeField] private Transform head;
        [SerializeField, Range(0f, StealthConstants.NEAR_VERTICAL_DEGREES)] private float headPitch = StealthUnityConstants.SURVEILLANCE_HEAD_PITCH;
        [SerializeField] private VisionConeAdapter visionCone;
        [SerializeField] private Color idleConeColor = Color.yellow;
        [SerializeField] private Color alertConeColor = Color.red;

        private SurveillanceCameraLogic cameraLogic;

        public SurveillanceCameraLogic Logic => cameraLogic;
        public bool IsReady => cameraLogic != null;

        public override void Initialize(params object[] parameters)
        {
            ICollisionWorld collisionWorld = parameters.Length > 0 ? parameters[0] as ICollisionWorld : null;

            bool hasDependencies = RequireDependency(head, nameof(head))
                & RequireDependency(visionCone, nameof(visionCone))
                & RequireService(collisionWorld, nameof(collisionWorld));

            if (!hasDependencies) return;

            Vector3 floorPosition = FindFloorBelow(head.position, collisionWorld);
            cameraLogic = new SurveillanceCameraLogic(cameraConfig, head.position, floorPosition, transform.eulerAngles.y, collisionWorld);

            base.Initialize(parameters);

            ApplyPose();
        }

        public override void Conclude()
        {
            base.Conclude();

            cameraLogic = null;
        }

        public void ApplyPose()
        {
            if (!IsReady) return;

            head.rotation = Quaternion.Euler(headPitch, cameraLogic.Yaw, 0f);

            Color coneColor = cameraLogic.IsAlerting ? alertConeColor : idleConeColor;
            VisionConfig vision = cameraConfig.Vision;
            visionCone.Draw(cameraLogic.SightOrigin, cameraLogic.Yaw, vision.Range, vision.MinRange, vision.FullAngle, coneColor);
        }

        private Vector3 FindFloorBelow(Vector3 position, ICollisionWorld collisionWorld)
        {
            if (collisionWorld.Raycast(position, Vector3.down, StealthUnityConstants.SURVEILLANCE_FLOOR_PROBE_DISTANCE, out CollisionHit hit)) return hit.Point;

            return position + Vector3.down * cameraConfig.SightHeight;
        }
    }
}
