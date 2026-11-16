using System;
using UnityEngine;

namespace StealthController.Unity
{
    [CreateAssetMenu(fileName = "TextTable", menuName = "Stealth Controller/Text Table")]
    public class TextTableData : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            [SerializeField] private string key;
            [SerializeField, Tooltip("One text per language, in the same order as the table's language codes.")] private string[] texts = Array.Empty<string>();

            public string Key => key;

            public Entry(string key, params string[] texts)
            {
                this.key = key;
                this.texts = texts;
            }

            public string TextAt(int languageIndex)
            {
                return languageIndex >= 0 && languageIndex < texts.Length ? texts[languageIndex] : string.Empty;
            }
        }

        [SerializeField, Tooltip("ISO 639-1 codes. The first one is the fallback language.")] private string[] languageCodes = { StealthUnityConstants.LANGUAGE_CODE_ENGLISH, StealthUnityConstants.LANGUAGE_CODE_SPANISH };
        [SerializeField] private Entry[] entries = Array.Empty<Entry>();

        public int LanguageCount => languageCodes.Length;

        public int IndexOfLanguage(string languageCode)
        {
            for (int i = 0; i < languageCodes.Length; i++)
            {
                if (string.Equals(languageCodes[i], languageCode, StringComparison.OrdinalIgnoreCase)) return i;
            }

            return -1;
        }

        public bool TryGetText(string key, int languageIndex, out string text)
        {
            foreach (Entry entry in entries)
            {
                if (entry.Key != key) continue;

                text = entry.TextAt(languageIndex);
                return true;
            }

            text = string.Empty;
            return false;
        }

        public void SetEntries(string[] codes, Entry[] values)
        {
            languageCodes = codes;
            entries = values;
        }
    }
}
