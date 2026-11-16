using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using StealthController.Core;
using UnityEngine;

public class StealthCoreArchitectureTests
{
    private const string CORE_RELATIVE_PATH = "StealthController/Runtime/Core";
    private const string CONFIG_SUFFIX = "Config.cs";

    private static readonly Regex ForbiddenEngineUsage = new Regex(
        @"\b(Physics|Physics2D|Time|Debug|Application|Resources|Random|Timing|DOTween)\.|\b(MonoBehaviour|ScriptableObject|GameObject|Transform|Component|LayerMask|Collider|Rigidbody|Animator|NavMeshAgent|NavMesh)\b|using\s+(UnityEngine\.(AI|UI|InputSystem|Rendering|Localization)|UnityEditor|DG\.|MEC|TMPro)");

    private static readonly Regex SerializationAttribute = new Regex(
        @"\[(Serializable|SerializeField|Header|Range|Min|Tooltip|FormerlySerializedAs)\b|\[[^\]]*\b(SerializeField|Range|Min|Tooltip|FormerlySerializedAs)\b|using\s+UnityEngine\.Serialization");

    [Test]
    public void CoreSources_UseOnlyPortableEngineTypes()
    {
        List<string> violations = new List<string>();

        foreach (string file in CoreSources())
            CollectMatches(file, ForbiddenEngineUsage, violations);

        Assert.IsEmpty(violations, string.Join("\n", violations));
    }

    [Test]
    public void SerializationAttributes_OnlyAppearInConfigs()
    {
        List<string> violations = new List<string>();

        foreach (string file in CoreSources())
        {
            if (file.EndsWith(CONFIG_SUFFIX)) continue;

            CollectMatches(file, SerializationAttribute, violations);
        }

        Assert.IsEmpty(violations, string.Join("\n", violations));
    }

    private static IEnumerable<string> CoreSources()
    {
        string coreDirectory = Path.Combine(Application.dataPath, CORE_RELATIVE_PATH);
        Assert.IsTrue(Directory.Exists(coreDirectory), "Core folder not found at " + coreDirectory);

        return Directory.GetFiles(coreDirectory, "*.cs", SearchOption.AllDirectories);
    }

    private static void CollectMatches(string file, Regex pattern, List<string> violations)
    {
        string[] lines = File.ReadAllLines(file);

        for (int i = 0; i < lines.Length; i++)
        {
            Match match = pattern.Match(lines[i]);
            if (match.Success)
                violations.Add(Path.GetFileName(file) + ":" + (i + 1) + " uses '" + match.Value + "'");
        }
    }
}
