using System.IO;
using UnityEngine;

public abstract class FileSaveSystemBase : SaveSystemBase
{
    private readonly string savePath;

    protected abstract string FileExtension { get; }

    protected FileSaveSystemBase()
    {
        savePath = Application.persistentDataPath + "/";
    }

    public override bool HasKey(string key)
    {
        return File.Exists(GetPath(key));
    }

    protected override void Remove(string key)
    {
        string path = GetPath(key);

        if (File.Exists(path))
            File.Delete(path);
    }

    protected string GetPath(string key)
    {
        return $"{savePath}{key}.{FileExtension}";
    }

    protected void WriteSafe(string path, string content) => FileWriteUtility.WriteAtomic(path, content);
}
