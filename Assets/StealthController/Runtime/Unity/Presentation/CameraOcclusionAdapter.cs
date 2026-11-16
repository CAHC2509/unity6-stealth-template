using StealthController.Core;
using UnityEngine;

namespace StealthController.Unity
{
    public class CameraOcclusionAdapter
    {
        private static readonly int TargetId = Shader.PropertyToID(StealthUnityConstants.OCCLUSION_CUTOUT_TARGET_PROPERTY);
        private static readonly int RadiusId = Shader.PropertyToID(StealthUnityConstants.OCCLUSION_CUTOUT_RADIUS_PROPERTY);
        private static readonly int SoftnessId = Shader.PropertyToID(StealthUnityConstants.OCCLUSION_CUTOUT_SOFTNESS_PROPERTY);
        private static readonly int DepthMarginId = Shader.PropertyToID(StealthUnityConstants.OCCLUSION_CUTOUT_DEPTH_MARGIN_PROPERTY);

        private readonly CameraOcclusionLogic logic;
        private readonly float softness;
        private readonly float depthMargin;

        public CameraOcclusionAdapter(CameraOcclusionLogic logic, float softness, float depthMargin)
        {
            this.logic = logic;
            this.softness = softness;
            this.depthMargin = depthMargin;
        }

        public void Refresh(float deltaTime, Vector3 cameraPosition, PlayerPose pose)
        {
            logic.Tick(deltaTime, cameraPosition, pose);

            Shader.SetGlobalVector(TargetId, logic.CutoutTarget);
            Shader.SetGlobalFloat(RadiusId, logic.CutoutRadius);
            Shader.SetGlobalFloat(SoftnessId, softness);
            Shader.SetGlobalFloat(DepthMarginId, depthMargin);
        }

        public void RestoreAll()
        {
            logic.Reset();
            Shader.SetGlobalFloat(RadiusId, 0f);
        }
    }
}
