using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class SaveSystemTests
{
    private const string TEST_KEY_PREFIX = "SaveSystemTests_";

    private readonly List<KeyValuePair<ISaveSystem, string>> createdKeys = new List<KeyValuePair<ISaveSystem, string>>();

    [TearDown]
    public void TearDown()
    {
        foreach (KeyValuePair<ISaveSystem, string> pair in createdKeys)
            pair.Key.Delete(pair.Value);

        createdKeys.Clear();
        SaveSystem.ClearCache();
    }

    [Test]
    public void JsonSaveSystem_RoundTripsData()
    {
        ISaveSystem saveSystem = new JSONSaveSystem();
        string key = CreateKey(saveSystem, "Json");
        SaveSample original = new SaveSample("Player One", 42, 0.75f);

        saveSystem.Save(key, original);
        SaveSample loaded = saveSystem.Load<SaveSample>(key);

        AssertEqual(original, loaded);
    }

    [Test]
    public void CsvSaveSystem_RoundTripsTextWithSeparatorsAndQuotes()
    {
        ISaveSystem saveSystem = new CSVSaveSystem();
        string key = CreateKey(saveSystem, "Csv");
        SaveSample original = new SaveSample("Name, with \"quotes\" and\nnew line", 7, 1.5f);

        saveSystem.Save(key, original);
        SaveSample loaded = saveSystem.Load<SaveSample>(key);

        AssertEqual(original, loaded);
    }

    [Test]
    public void CsvSaveSystem_RoundTripsCollections()
    {
        ISaveSystem saveSystem = new CSVSaveSystem();
        string key = CreateKey(saveSystem, "CsvList");
        List<SaveSample> original = new List<SaveSample>
        {
            new SaveSample("First", 1, 0.1f),
            new SaveSample("Second, item", 2, 0.2f)
        };

        saveSystem.Save(key, original);
        List<SaveSample> loaded = saveSystem.Load<List<SaveSample>>(key);

        Assert.AreEqual(original.Count, loaded.Count);
        for (int i = 0; i < original.Count; i++)
            AssertEqual(original[i], loaded[i]);
    }

    [Test]
    public void FileSaveSystem_ReturnsDefaultInstanceForMissingKey()
    {
        ISaveSystem saveSystem = new JSONSaveSystem();
        string key = CreateKey(saveSystem, "Missing");

        Assert.IsFalse(saveSystem.HasKey(key));
        Assert.IsNotNull(saveSystem.Load<SaveSample>(key));
    }

    [Test]
    public void SaveSystemFacade_TryLoadReportsMissingKeys()
    {
        SaveSystem.CreateImplementation(SaveSystemType.JSON);
        string key = TEST_KEY_PREFIX + "Facade_" + Guid.NewGuid().ToString("N");

        Assert.IsFalse(SaveSystem.TryLoad(key, out SaveSample missing));
        Assert.IsNull(missing);

        SaveSystem.Save(key, new SaveSample("Saved", 3, 0.3f));
        Assert.IsTrue(SaveSystem.TryLoad(key, out SaveSample loaded));
        Assert.AreEqual("Saved", loaded.Name);

        SaveSystem.Delete(key);
    }

    private string CreateKey(ISaveSystem saveSystem, string suffix)
    {
        string key = TEST_KEY_PREFIX + suffix + "_" + Guid.NewGuid().ToString("N");
        createdKeys.Add(new KeyValuePair<ISaveSystem, string>(saveSystem, key));
        return key;
    }

    private static void AssertEqual(SaveSample expected, SaveSample actual)
    {
        Assert.IsNotNull(actual);
        Assert.AreEqual(expected.Name, actual.Name);
        Assert.AreEqual(expected.Score, actual.Score);
        Assert.AreEqual(expected.Ratio, actual.Ratio, 0.0001f);
    }

    [Serializable]
    private class SaveSample
    {
        [SerializeField] private string name;
        [SerializeField] private int score;
        [SerializeField] private float ratio;

        public string Name => name;
        public int Score => score;
        public float Ratio => ratio;

        public SaveSample() { }

        public SaveSample(string name, int score, float ratio)
        {
            this.name = name;
            this.score = score;
            this.ratio = ratio;
        }
    }
}
