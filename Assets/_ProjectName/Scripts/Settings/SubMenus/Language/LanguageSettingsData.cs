using System;
using UnityEngine;

[Serializable]
public class LanguageSettingsData
{
    [SerializeField] private int languageIndex;

    public int LanguageIndex { get => languageIndex; set => languageIndex = value; }
}