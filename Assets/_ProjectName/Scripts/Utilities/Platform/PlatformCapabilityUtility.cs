using UnityEngine;

public static class PlatformCapabilityUtility
{
    public static bool SupportsVSyncControl => SupportsVSyncControlOn(Application.platform);
    public static bool SupportsFullscreenToggle => SupportsFullscreenToggleOn(Application.platform);
    public static bool SupportsResolutionSelection => SupportsResolutionSelectionOn(Application.platform);
    public static bool SupportsApplicationQuit => SupportsApplicationQuitOn(Application.platform);
    public static bool SupportsDevTools => Debug.isDebugBuild;
    public static bool SupportsFileBrowsing => IsDesktopPlatform(Application.platform);

    public static bool SupportsVSyncControlOn(RuntimePlatform platform)
    {
        switch (platform)
        {
            case RuntimePlatform.Android:
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.tvOS:
            case RuntimePlatform.VisionOS:
            case RuntimePlatform.WebGLPlayer:
                return false;
            default:
                return true;
        }
    }

    public static bool SupportsFullscreenToggleOn(RuntimePlatform platform)
    {
        return IsDesktopPlatform(platform) || platform == RuntimePlatform.WebGLPlayer;
    }

    public static bool SupportsResolutionSelectionOn(RuntimePlatform platform)
    {
        return IsDesktopPlatform(platform);
    }

    public static bool SupportsApplicationQuitOn(RuntimePlatform platform)
    {
        return platform != RuntimePlatform.WebGLPlayer;
    }

    private static bool IsDesktopPlatform(RuntimePlatform platform)
    {
        switch (platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.LinuxEditor:
                return true;
            default:
                return false;
        }
    }
}
