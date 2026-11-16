using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class SceneBootstrapper
{
    private const string PREVIOUS_SCENE_PREF_KEY = "PreviousScene";
    private const string SHOULD_LOAD_BOOTSTRAP_PREF_KEY = "LoadBootstrapScene";
    private const string TEST_RUN_SESSION_KEY = "SceneBootstrapperTestRun";
    private const string TEST_RUNNER_SCENE_PREFIX = "InitTestScene";

    private const string LOAD_BOOTSTRAP_MENU = "Scene Bootstrapper/Load Bootstrap Scene On Play";
    private const string DONT_LOAD_BOOTSTRAP_MENU = "Scene Bootstrapper/Don't Load Bootstrap Scene On Play";

    private static string BootstrapScene => EditorBuildSettings.scenes.Length > 0 ? EditorBuildSettings.scenes[0].path : string.Empty;

    private static string PreviousScene
    {
        get => EditorPrefs.GetString(PREVIOUS_SCENE_PREF_KEY);
        set => EditorPrefs.SetString(PREVIOUS_SCENE_PREF_KEY, value);
    }

    private static bool ShouldLoadBootstrapScene
    {
        get => EditorPrefs.GetBool(SHOULD_LOAD_BOOTSTRAP_PREF_KEY, true);
        set => EditorPrefs.SetBool(SHOULD_LOAD_BOOTSTRAP_PREF_KEY, value);
    }

    static SceneBootstrapper()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
    {
        if (!ShouldLoadBootstrapScene) return;

        switch (playModeStateChange)
        {
            case PlayModeStateChange.ExitingEditMode:
                SessionState.SetBool(TEST_RUN_SESSION_KEY, IsTestRunnerScene());
                if (SessionState.GetBool(TEST_RUN_SESSION_KEY, false)) return;

                OpenBootstrapScene();
                break;

            case PlayModeStateChange.EnteredEditMode:
                if (SessionState.GetBool(TEST_RUN_SESSION_KEY, false)) return;

                RestorePreviousScene();
                break;
        }
    }

    private static bool IsTestRunnerScene()
    {
        return EditorSceneManager.GetActiveScene().name.StartsWith(TEST_RUNNER_SCENE_PREFIX);
    }

    private static void OpenBootstrapScene()
    {
        PreviousScene = EditorSceneManager.GetActiveScene().path;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (!IsSceneInBuildSettings(BootstrapScene)) return;

        EditorSceneManager.OpenScene(BootstrapScene);
    }

    private static void RestorePreviousScene()
    {
        if (string.IsNullOrEmpty(PreviousScene)) return;

        EditorSceneManager.OpenScene(PreviousScene);
    }

    [MenuItem(LOAD_BOOTSTRAP_MENU)]
    private static void EnableBootstrapper() => ShouldLoadBootstrapScene = true;

    [MenuItem(LOAD_BOOTSTRAP_MENU, true)]
    private static bool ValidateEnableBootstrapper() => !ShouldLoadBootstrapScene;

    [MenuItem(DONT_LOAD_BOOTSTRAP_MENU)]
    private static void DisableBootstrapper() => ShouldLoadBootstrapScene = false;

    [MenuItem(DONT_LOAD_BOOTSTRAP_MENU, true)]
    private static bool ValidateDisableBootstrapper() => ShouldLoadBootstrapScene;

    private static bool IsSceneInBuildSettings(string scenePath)
    {
        if (string.IsNullOrEmpty(scenePath)) return false;

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.path == scenePath)
                return true;
        }

        return false;
    }
}
