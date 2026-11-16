using NUnit.Framework;
using UnityEngine;

public class SettingsAndUtilityTests
{
    [Test]
    public void GraphicsSettingsData_LegacySaveFallsBackToDefaults()
    {
        const string legacyJson = "{\"resolutionIndex\":3,\"qualityIndex\":1,\"isFullscreen\":false}";

        GraphicsSettingsData data = JsonUtility.FromJson<GraphicsSettingsData>(legacyJson);

        Assert.IsFalse(data.HasResolution);
        Assert.AreEqual(1, data.QualityIndex);
        Assert.IsFalse(data.IsFullscreen);
        Assert.IsTrue(data.IsVSyncEnabled);
    }

    [Test]
    public void GraphicsSettingsData_StoresResolutionBySize()
    {
        GraphicsSettingsData data = new GraphicsSettingsData();

        data.SetResolution(1600, 900);
        GraphicsSettingsData loaded = JsonUtility.FromJson<GraphicsSettingsData>(JsonUtility.ToJson(data));

        Assert.IsTrue(loaded.HasResolution);
        Assert.AreEqual(1600, loaded.ResolutionWidth);
        Assert.AreEqual(900, loaded.ResolutionHeight);
    }

    [TestCase(RuntimePlatform.WindowsPlayer, true, true, true)]
    [TestCase(RuntimePlatform.OSXPlayer, true, true, true)]
    [TestCase(RuntimePlatform.LinuxPlayer, true, true, true)]
    [TestCase(RuntimePlatform.WebGLPlayer, false, true, false)]
    [TestCase(RuntimePlatform.Android, false, false, false)]
    [TestCase(RuntimePlatform.IPhonePlayer, false, false, false)]
    public void PlatformCapabilityUtility_ReportsDisplayOptionsPerPlatform(RuntimePlatform platform, bool resolution, bool fullscreen, bool vSync)
    {
        Assert.AreEqual(resolution, PlatformCapabilityUtility.SupportsResolutionSelectionOn(platform));
        Assert.AreEqual(fullscreen, PlatformCapabilityUtility.SupportsFullscreenToggleOn(platform));
        Assert.AreEqual(vSync, PlatformCapabilityUtility.SupportsVSyncControlOn(platform));
    }

    [TestCase(RuntimePlatform.WindowsPlayer, true)]
    [TestCase(RuntimePlatform.Android, true)]
    [TestCase(RuntimePlatform.WebGLPlayer, false)]
    public void PlatformCapabilityUtility_ReportsApplicationQuitPerPlatform(RuntimePlatform platform, bool canQuit)
    {
        Assert.AreEqual(canQuit, PlatformCapabilityUtility.SupportsApplicationQuitOn(platform));
    }

    [TestCase(0f, "0")]
    [TestCase(0.5f, "50")]
    [TestCase(1f, "100")]
    [TestCase(0.333f, "33")]
    public void TextFormatUtility_FormatsNormalizedValuesAsPercent(float value, string expected)
    {
        Assert.AreEqual(expected, TextFormatUtility.ToPercentText(value));
    }

    [Test]
    public void UITransitionHandle_InvokesLateSubscribersAfterCompletion()
    {
        UITransitionHandle handle = new UITransitionHandle();
        int completions = 0;

        handle.OnComplete(() => completions++);
        handle.InvokeComplete();
        handle.OnComplete(() => completions++);

        Assert.AreEqual(2, completions);
        Assert.IsTrue(handle.IsCompleted);
    }

    [Test]
    public void UITransitionHandle_CancelPreventsCompletion()
    {
        UITransitionHandle handle = new UITransitionHandle();
        bool completed = false;
        bool canceled = false;

        handle.OnComplete(() => completed = true).OnCancel(() => canceled = true);
        handle.Cancel();
        handle.InvokeComplete();

        Assert.IsTrue(canceled);
        Assert.IsFalse(completed);
        Assert.IsFalse(handle.IsCompleted);
    }
}
