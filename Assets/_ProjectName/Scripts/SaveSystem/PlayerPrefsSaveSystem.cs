using UnityEngine;

public class PlayerPrefsSaveSystem : SaveSystemBase
{
    public override bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    protected override void Write<T>(string key, T data)
    {
        PlayerPrefs.SetString(key, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    protected override T Read<T>(string key)
    {
        return FromJson<T>(PlayerPrefs.GetString(key));
    }

    protected override void Remove(string key)
    {
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
    }
}
