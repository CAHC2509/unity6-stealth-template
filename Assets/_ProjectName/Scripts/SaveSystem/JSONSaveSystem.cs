using System.IO;
using UnityEngine;

public class JSONSaveSystem : FileSaveSystemBase
{
    protected override string FileExtension => "json";

    protected override void Write<T>(string key, T data)
    {
        WriteSafe(GetPath(key), JsonUtility.ToJson(data));
    }

    protected override T Read<T>(string key)
    {
        return FromJson<T>(File.ReadAllText(GetPath(key)));
    }
}
