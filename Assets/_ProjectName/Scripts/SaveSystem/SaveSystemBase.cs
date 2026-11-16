using System;
using UnityEngine;

public abstract class SaveSystemBase : ISaveSystem
{
    private const string NULL_JSON = "null";

    public void Save<T>(string key, T data)
    {
        if (data == null)
        {
            Debug.LogWarning($"SaveSystem: Trying to save null data for key {key}");
            return;
        }

        try
        {
            Write(key, data);
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveSystem: Failed to save key {key}. {e}");
        }
    }

    public T Load<T>(string key)
    {
        try
        {
            return HasKey(key) ? Read<T>(key) : CreateDefault<T>();
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveSystem: Failed to load key {key}. {e}");
            return CreateDefault<T>();
        }
    }

    public void Delete(string key)
    {
        try
        {
            Remove(key);
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveSystem: Failed to delete key {key}. {e}");
        }
    }

    public abstract bool HasKey(string key);

    protected abstract void Write<T>(string key, T data);
    protected abstract T Read<T>(string key);
    protected abstract void Remove(string key);

    protected T FromJson<T>(string json)
    {
        if (string.IsNullOrEmpty(json) || json == NULL_JSON)
            return CreateDefault<T>();

        T data = JsonUtility.FromJson<T>(json);

        return data == null ? CreateDefault<T>() : data;
    }

    protected T CreateDefault<T>()
    {
        try
        {
            return Activator.CreateInstance<T>();
        }
        catch
        {
            return default;
        }
    }
}
