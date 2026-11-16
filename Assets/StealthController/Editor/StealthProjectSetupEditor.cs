using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Rendering;

namespace StealthController.Editor
{
    [InitializeOnLoad]
    public static class StealthProjectSetupEditor
    {
        private static readonly string[] RequiredPackages =
        {
            StealthEditorConstants.INPUT_SYSTEM_PACKAGE,
            StealthEditorConstants.AI_NAVIGATION_PACKAGE,
            StealthEditorConstants.UGUI_PACKAGE
        };

        private static AddAndRemoveRequest installRequest;

        static StealthProjectSetupEditor()
        {
            if (SessionState.GetBool(StealthEditorConstants.SESSION_KEY, false)) return;

            EditorApplication.delayCall += RunSetup;
        }

        [MenuItem("Tools/Stealth Controller/Run Project Setup")]
        public static void RunSetupFromMenu()
        {
            SessionState.EraseBool(StealthEditorConstants.SESSION_KEY);
            RunSetup();
        }

        private static void RunSetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += RunSetup;
                return;
            }

            SessionState.SetBool(StealthEditorConstants.SESSION_KEY, true);

            if (InstallMissingPackages()) return;

            ImportTextMeshProEssentials();
            NameStealthLayers();
            WarnAboutProjectSettings();
        }

        private static bool InstallMissingPackages()
        {
            string lockFile = File.Exists(StealthEditorConstants.PACKAGES_LOCK_PATH) ? File.ReadAllText(StealthEditorConstants.PACKAGES_LOCK_PATH) : string.Empty;
            List<string> missing = new List<string>();

            foreach (string package in RequiredPackages)
            {
                if (!lockFile.Contains("\"" + package + "\""))
                    missing.Add(package);
            }

            if (missing.Count == 0) return false;

            Debug.Log(StealthEditorConstants.LOG_PREFIX + "Installing required packages: " + string.Join(", ", missing) + ". The setup continues after the next domain reload.");
            installRequest = Client.AddAndRemove(missing.ToArray());
            EditorApplication.update += WaitForInstall;
            SessionState.EraseBool(StealthEditorConstants.SESSION_KEY);
            return true;
        }

        private static void WaitForInstall()
        {
            if (installRequest == null || !installRequest.IsCompleted) return;

            EditorApplication.update -= WaitForInstall;

            if (installRequest.Status == StatusCode.Failure)
                Debug.LogError(StealthEditorConstants.LOG_PREFIX + "Package installation failed: " + installRequest.Error.message + ". Install them from the Package Manager and run Tools > Stealth Controller > Run Project Setup.");

            installRequest = null;
        }

        private static void ImportTextMeshProEssentials()
        {
            if (AssetDatabase.FindAssets(StealthEditorConstants.TMP_SETTINGS_SEARCH).Length > 0) return;
            if (!Directory.Exists(StealthEditorConstants.PACKAGE_CACHE_FOLDER)) return;

            foreach (string folder in Directory.GetDirectories(StealthEditorConstants.PACKAGE_CACHE_FOLDER))
            {
                if (!Path.GetFileName(folder).StartsWith(StealthEditorConstants.UGUI_PACKAGE_FOLDER_PREFIX)) continue;

                string package = Path.Combine(folder, StealthEditorConstants.TMP_ESSENTIALS_RELATIVE_PATH);
                if (!File.Exists(package)) continue;

                Debug.Log(StealthEditorConstants.LOG_PREFIX + "Importing TextMesh Pro Essential Resources.");
                AssetDatabase.ImportPackage(package, false);
                return;
            }

            Debug.LogWarning(StealthEditorConstants.LOG_PREFIX + "TextMesh Pro Essential Resources were not found. Import them from Window > TextMeshPro > Import TMP Essential Resources.");
        }

        private static void NameStealthLayers()
        {
            Object[] tagManagerAssets = AssetDatabase.LoadAllAssetsAtPath(StealthEditorConstants.TAG_MANAGER_PATH);
            if (tagManagerAssets.Length == 0) return;

            SerializedObject tagManager = new SerializedObject(tagManagerAssets[0]);
            SerializedProperty layers = tagManager.FindProperty(StealthEditorConstants.TAG_MANAGER_LAYERS_PROPERTY);
            if (layers == null) return;

            bool changed = NameLayer(layers, StealthEditorConstants.PLAYER_LAYER_INDEX, StealthEditorConstants.PLAYER_LAYER_NAME) | NameLayer(layers, StealthEditorConstants.ENEMY_LAYER_INDEX, StealthEditorConstants.ENEMY_LAYER_NAME);
            if (!changed) return;

            tagManager.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log(StealthEditorConstants.LOG_PREFIX + "Named layers " + StealthEditorConstants.PLAYER_LAYER_INDEX + " and " + StealthEditorConstants.ENEMY_LAYER_INDEX + " as '" + StealthEditorConstants.PLAYER_LAYER_NAME + "' and '" + StealthEditorConstants.ENEMY_LAYER_NAME + "'.");
        }

        private static bool NameLayer(SerializedProperty layers, int index, string layerName)
        {
            if (index >= layers.arraySize) return false;

            SerializedProperty layer = layers.GetArrayElementAtIndex(index);
            if (!string.IsNullOrEmpty(layer.stringValue)) return false;

            layer.stringValue = layerName;
            return true;
        }

        private static void WarnAboutProjectSettings()
        {
            if (GraphicsSettings.defaultRenderPipeline == null)
                Debug.LogWarning(StealthEditorConstants.LOG_PREFIX + "No render pipeline is active. The stealth materials need the Universal Render Pipeline (create the project from the Universal 3D template).");

            Object[] playerSettings = AssetDatabase.LoadAllAssetsAtPath(StealthEditorConstants.PLAYER_SETTINGS_PATH);
            if (playerSettings.Length == 0) return;

            SerializedProperty inputHandler = new SerializedObject(playerSettings[0]).FindProperty(StealthEditorConstants.ACTIVE_INPUT_HANDLER_PROPERTY);
            if (inputHandler != null && inputHandler.intValue == StealthEditorConstants.ACTIVE_INPUT_HANDLER_OLD_ONLY)
                Debug.LogWarning(StealthEditorConstants.LOG_PREFIX + "Active Input Handling is set to the old Input Manager. Set it to 'Input System Package (New)' or 'Both' in Project Settings > Player.");
        }
    }
}
