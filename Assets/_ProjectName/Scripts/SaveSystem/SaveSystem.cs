using System;
using System.Collections.Generic;

public static class SaveSystem
{
    private static ISaveSystem currentImplementation;
    private static readonly Dictionary<string, object> cache = new();

    private static ISaveSystem Implementation
    {
        get
        {
            if (currentImplementation == null)
                currentImplementation = new JSONSaveSystem();

            return currentImplementation;
        }
    }

    public static void CreateImplementation(SaveSystemType type)
    {
        currentImplementation = type switch
        {
            SaveSystemType.PlayerPrefs => new PlayerPrefsSaveSystem(),
            SaveSystemType.JSON => new JSONSaveSystem(),
            SaveSystemType.CSV => new CSVSaveSystem(),
            _ => new JSONSaveSystem()
        };

        cache.Clear();
    }

    public static void Save<T>(string key, T data)
    {
        cache[key] = data;
        Implementation.Save(key, data);
    }

    public static T Load<T>(string key)
    {
        if (cache.TryGetValue(key, out object cached))
            return (T)cached;

        T data = Implementation.Load<T>(key);
        cache[key] = data;

        return data;
    }

    public static bool TryLoad<T>(string key, out T data)
    {
        if (!cache.ContainsKey(key) && !Implementation.HasKey(key))
        {
            data = default;
            return false;
        }

        data = Load<T>(key);
        return true;
    }

    public static bool HasKey(string key)
    {
        return Implementation.HasKey(key);
    }

    public static void Delete(string key)
    {
        cache.Remove(key);
        Implementation.Delete(key);
    }

    public static void ClearCache()
    {
        cache.Clear();
    }
}