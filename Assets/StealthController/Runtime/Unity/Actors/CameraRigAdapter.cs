using StealthController.Core;
using UnityEngine.Serialization;
using UnityEngine;

namespace StealthController.Unity
{
    public class CameraRigAdapter : StealthBehaviourBase
    {
        [Header("Camera")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private CameraRigConfig rigConfig = new CameraRigConfig();

        [Header("Occlusion")]
        [SerializeField, BasicSetting, FormerlySerializedAs("hideOccluders"), Tooltip("Opens a see-through tunnel in the geometry that hides the player from the camera.")]
        private bool useOcclusionCutout = true;
        [SerializeField] private LayerMask occluderMask = StealthUnityConstants.DEFAULT_ENVIRONMENT_MASK;
        [SerializeField, Min(0f)] private float occlusionTargetMinHeight = StealthConstants.CAMERA_OCCLUSION_MIN_TARGET_HEIGHT;
        [SerializeField, Min(0f)] private float cutoutRadius = StealthConstants.OCCLUSION_CUTOUT_RADIUS;
        [SerializeField, Min(0f)] private float cutoutSoftness = StealthUnityConstants.OCCLUSION_CUTOUT_SOFTNESS;
        [SerializeField, Min(0f), Tooltip("Geometry closer than this to the player along the camera ray is never cut.")]
        private float cutoutDepthMargin = StealthUnityConstants.OCCLUSION_CUTOUT_DEPTH_MARGIN;
        [SerializeField, Min(0f)] private float cutoutOpenSpeed = StealthConstants.OCCLUSION_CUTOUT_OPEN_SPEED;
        [SerializeField, Min(0f)] private float cutoutCloseSpeed = StealthConstants.OCCLUSION_CUTOUT_CLOSE_SPEED;

        private CameraRigLogic rigLogic;
        private CameraOcclusionAdapter occlusion;

        public CameraRigLogic Logic => rigLogic;
        public bool IsReady => rigLogic != null;
        public float InputReferenceYaw => IsReady ? rigLogic.InputReferenceYaw : 0f;

        public override void Initialize(params object[] parameters)
        {
            if (!RequireDependency(targetCamera, nameof(targetCamera))) return;

            rigLogic = new CameraRigLogic(rigConfig);
            CameraOcclusionLogic occlusionLogic = new CameraOcclusionLogic(new UnityCollisionWorldAdapter(occluderMask, null), occlusionTargetMinHeight, cutoutRadius, cutoutOpenSpeed, cutoutCloseSpeed);
            occlusion = new CameraOcclusionAdapter(occlusionLogic, cutoutSoftness, cutoutDepthMargin);

            base.Initialize(parameters);
        }

        public override void Conclude()
        {
            base.Conclude();

            if (occlusion != null)
                occlusion.RestoreAll();

            rigLogic = null;
        }

        public void ApplyFraming(PlayerPose pose, float deltaTime)
        {
            if (!IsReady) return;

            CameraFraming framing = rigLogic.Framing;

            targetCamera.transform.SetPositionAndRotation(framing.Position, framing.Rotation);
            targetCamera.fieldOfView = framing.FieldOfView;

            if (!useOcclusionCutout) return;

            occlusion.Refresh(deltaTime, framing.Position, pose);
        }
    }
}
