using System.IO;
using UnityEditor;
using UnityEngine;

public static class StealthPackageExportEditor
{
    private const string EXPORT_MENU_PATH = "Tools/Stealth Controller/Export Package";

    [MenuItem(EXPORT_MENU_PATH)]
    public static void ExportPackage()
    {
        if (!AssetDatabase.IsValidFolder(Constants.STEALTH_PACKAGE_ROOT))
        {
            Debug.LogError($"[{nameof(StealthPackageExportEditor)}] Package folder '{Constants.STEALTH_PACKAGE_ROOT}' not found.");
            return;
        }

        if (EditorApplication.isCompiling || EditorUtility.scriptCompilationFailed)
        {
            Debug.LogError($"[{nameof(StealthPackageExportEditor)}] Fix compile errors and wait for compilation to finish before exporting.");
            return;
        }

        AssetDatabase.SaveAssets();

        string exportFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", Constants.STEALTH_PACKAGE_EXPORT_FOLDER));
        Directory.CreateDirectory(exportFolder);

        string packagePath = Path.Combine(exportFolder, Constants.STEALTH_PACKAGE_FILE_NAME);
        AssetDatabase.ExportPackage(Constants.STEALTH_PACKAGE_ROOT, packagePath, ExportPackageOptions.Recurse);

        if (!File.Exists(packagePath))
        {
            Debug.LogError($"[{nameof(StealthPackageExportEditor)}] Export failed: '{packagePath}' was not written.");
            return;
        }

        float sizeInMegabytes = (float)new FileInfo(packagePath).Length / Constants.BYTES_PER_MEGABYTE;
        Debug.Log($"[{nameof(StealthPackageExportEditor)}] Exported {Constants.STEALTH_PACKAGE_ROOT} to {packagePath} ({sizeInMegabytes:F1} MB).");
        EditorUtility.RevealInFinder(packagePath);
    }
}
