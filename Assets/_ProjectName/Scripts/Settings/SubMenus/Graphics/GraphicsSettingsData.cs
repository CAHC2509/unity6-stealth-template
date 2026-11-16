using System;
using UnityEngine;

[Serializable]
public class GraphicsSettingsData
{
    [SerializeField] private int resolutionWidth;
    [SerializeField] private int resolutionHeight;
    [SerializeField] private int qualityIndex;
    [SerializeField] private bool isFullscreen = true;
    [SerializeField] private bool isVSyncEnabled = true;

    public int ResolutionWidth => resolutionWidth;
    public int ResolutionHeight => resolutionHeight;
    public bool HasResolution => resolutionWidth > 0 && resolutionHeight > 0;
    public int QualityIndex { get => qualityIndex; set => qualityIndex = value; }
    public bool IsFullscreen { get => isFullscreen; set => isFullscreen = value; }
    public bool IsVSyncEnabled { get => isVSyncEnabled; set => isVSyncEnabled = value; }

    public void SetResolution(int width, int height)
    {
        resolutionWidth = width;
        resolutionHeight = height;
    }
}
