using System;
using System.IO;
using UnityEngine;

public static class FileWriteUtility
{
    public static void WriteAtomic(string path, string content)
    {
        string directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        string temporaryPath = path + Constants.TEMPORARY_FILE_SUFFIX;

        File.WriteAllText(temporaryPath, content);

        if (File.Exists(path))
            File.Delete(path);

        File.Move(temporaryPath, path);
    }

    public static string GetPersistentFolder(string subFolder)
    {
        return Path.GetFullPath(Path.Combine(Application.persistentDataPath, subFolder));
    }

    public static string BuildTimestampedPath(string rootFolder, string subFolder, string filePrefix, string extension, DateTime timestamp)
    {
        string fileName = $"{filePrefix}_{timestamp.ToString(Constants.FILE_TIMESTAMP_FORMAT)}.{extension}";
        return Path.GetFullPath(Path.Combine(rootFolder, subFolder, fileName));
    }
}
