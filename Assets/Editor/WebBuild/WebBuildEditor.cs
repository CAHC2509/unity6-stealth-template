using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.WebGL;
using UnityEngine;

public static class WebBuildEditor
{
    private const string APPLY_SETTINGS_MENU_PATH = "Tools/Web Build/Apply Release Settings";
    private const string BUILD_MENU_PATH = "Tools/Web Build/Build Release";

    [MenuItem(APPLY_SETTINGS_MENU_PATH)]
    public static void ApplyReleaseSettings()
    {
        NamedBuildTarget webTarget = NamedBuildTarget.WebGL;

        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
        PlayerSettings.WebGL.wasm2023 = true;
        PlayerSettings.WebGL.maximumMemorySize = Constants.WEB_BUILD_MAXIMUM_MEMORY_MB;
        PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;
        PlayerSettings.defaultWebScreenWidth = Constants.WEB_BUILD_CANVAS_WIDTH;
        PlayerSettings.defaultWebScreenHeight = Constants.WEB_BUILD_CANVAS_HEIGHT;

        PlayerSettings.stripEngineCode = true;
        PlayerSettings.stripUnusedMeshComponents = true;
        PlayerSettings.SetManagedStrippingLevel(webTarget, ManagedStrippingLevel.High);
        PlayerSettings.SetIl2CppCodeGeneration(webTarget, Il2CppCodeGeneration.OptimizeSize);
        PlayerSettings.SetApiCompatibilityLevel(webTarget, ApiCompatibilityLevel.NET_Standard);

        UserBuildSettings.codeOptimization = WasmCodeOptimization.DiskSizeLTO;

        AssetDatabase.SaveAssets();

        Debug.Log($"[{nameof(WebBuildEditor)}] Web release settings: compression={PlayerSettings.WebGL.compressionFormat}, decompressionFallback={PlayerSettings.WebGL.decompressionFallback}, "
            + $"exceptions={PlayerSettings.WebGL.exceptionSupport}, wasm2023={PlayerSettings.WebGL.wasm2023}, stripEngineCode={PlayerSettings.stripEngineCode}, "
            + $"managedStripping={PlayerSettings.GetManagedStrippingLevel(webTarget)}, il2cppCodeGeneration={PlayerSettings.GetIl2CppCodeGeneration(webTarget)}, "
            + $"apiCompatibility={PlayerSettings.GetApiCompatibilityLevel(webTarget)}, codeOptimization={UserBuildSettings.codeOptimization}, "
            + $"memory={PlayerSettings.WebGL.initialMemorySize}-{PlayerSettings.WebGL.maximumMemorySize} MB ({PlayerSettings.WebGL.memoryGrowthMode})");
    }

    [MenuItem(BUILD_MENU_PATH)]
    public static void BuildRelease()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
        {
            Debug.LogError($"[{nameof(WebBuildEditor)}] Switch the active build target to Web before building.");
            return;
        }

        ApplyReleaseSettings();

        if (!BuildAddressables()) return;

        string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", Constants.WEB_BUILD_OUTPUT_FOLDER));
        if (Directory.Exists(outputPath))
        {
            Directory.Delete(outputPath, true);
        }

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
            locationPathName = outputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"[{nameof(WebBuildEditor)}] Web build {report.summary.result} with {report.summary.totalErrors} errors.");
            return;
        }

        Debug.Log($"[{nameof(WebBuildEditor)}] Web build succeeded at {outputPath}: {(float)GetFolderSize(outputPath) / Constants.BYTES_PER_MEGABYTE:F1} MB in {report.summary.totalTime}.");
    }

    private static bool BuildAddressables()
    {
        AddressableAssetSettings.CleanPlayerContent();
        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);

        if (string.IsNullOrEmpty(result.Error)) return true;

        Debug.LogError($"[{nameof(WebBuildEditor)}] Addressables build failed: {result.Error}");
        return false;
    }

    private static long GetFolderSize(string folderPath)
    {
        return new DirectoryInfo(folderPath).GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
    }
}
