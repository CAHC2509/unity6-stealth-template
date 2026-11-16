using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class SceneLoadingManager : ManagerBase
{
    public event Action SwitchScenesCompleted;
    public event Action SwitchScenesFailed;

    private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> loadedScenes = new Dictionary<string, AsyncOperationHandle<SceneInstance>>();

    public void LoadScene(string sceneKey)
    {
        if (loadedScenes.ContainsKey(sceneKey) || IsLoadedInScenes(sceneKey))
        {
            Debug.LogWarning($"Scene '{sceneKey}' already loaded.", this);
            SwitchScenesCompleted?.Invoke();
            return;
        }

        AsyncOperationHandle<SceneInstance> handle =
            Addressables.LoadSceneAsync(sceneKey, LoadSceneMode.Additive);

        handle.Completed += operation =>
        {
            if (operation.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Error loading scene '{sceneKey}'.", this);
                return;
            }

            SceneInstance sceneInstance = operation.Result;
            loadedScenes[sceneKey] = handle;

            Scene loadedScene = sceneInstance.Scene;
            SceneManager.SetActiveScene(loadedScene);

            SwitchScenesCompleted?.Invoke();
        };
    }

    public void SwitchScenes(string currentScene, string nextScene)
    {
        AsyncOperationHandle<IList<IResourceLocation>> locationsHandle = Addressables.LoadResourceLocationsAsync(nextScene);

        locationsHandle.Completed += operation =>
        {
            bool sceneExists = operation.Status == AsyncOperationStatus.Succeeded && operation.Result != null && operation.Result.Count > 0;
            Addressables.Release(operation);

            if (!sceneExists)
            {
                Debug.LogError($"Scene '{nextScene}' has no Addressables entry in the 'Scenes' group. Staying in '{currentScene}'.", this);
                SwitchScenesFailed?.Invoke();
                return;
            }

            UnloadAndLoad(currentScene, nextScene);
        };
    }

    private void UnloadAndLoad(string currentScene, string nextScene)
    {
        if (!loadedScenes.ContainsKey(currentScene))
        {
            Debug.LogWarning($"Scene '{currentScene}' not loaded, loading '{nextScene}' directly.", this);
            LoadScene(nextScene);
            return;
        }

        AsyncOperationHandle<SceneInstance> handle = loadedScenes[currentScene];

        Addressables.UnloadSceneAsync(handle).Completed += operation =>
        {
            if (operation.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Error unloading scene '{currentScene}'.", this);
                return;
            }

            loadedScenes.Remove(currentScene);
            LoadScene(nextScene);
        };
    }

    private static bool IsLoadedInScenes(string sceneKey)
    {
        for (int i = 0; i < SceneManager.loadedSceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == sceneKey)
                return true;
        }

        return false;
    }
}