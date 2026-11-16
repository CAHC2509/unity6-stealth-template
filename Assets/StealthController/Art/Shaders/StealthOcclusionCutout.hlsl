#ifndef STEALTH_OCCLUSION_CUTOUT_INCLUDED
#define STEALTH_OCCLUSION_CUTOUT_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Random.hlsl"

float3 _StealthCutoutTarget;
float _StealthCutoutRadius;
float _StealthCutoutSoftness;
float _StealthCutoutDepthMargin;

void ApplyStealthOcclusionCutout(float4 positionCS)
{
    if (_StealthCutoutRadius <= 0.0)
        return;

    float2 screenUV = GetNormalizedScreenSpaceUV(positionCS);
    float3 positionWS = ComputeWorldSpacePosition(screenUV, positionCS.z, UNITY_MATRIX_I_VP);

    float3 cameraToTarget = _StealthCutoutTarget - _WorldSpaceCameraPos;
    float targetDistance = length(cameraToTarget);
    float3 tunnelAxis = cameraToTarget / max(targetDistance, HALF_MIN);

    float3 cameraToPixel = positionWS - _WorldSpaceCameraPos;
    float alongTunnel = dot(cameraToPixel, tunnelAxis);

    if (alongTunnel > targetDistance - _StealthCutoutDepthMargin)
        return;

    float radialDistance = length(cameraToPixel - tunnelAxis * alongTunnel);
    float cutAmount = saturate((_StealthCutoutRadius - radialDistance) / max(_StealthCutoutSoftness, HALF_MIN));

    clip(InterleavedGradientNoise(positionCS.xy, 0) - cutAmount);
}

#endif
