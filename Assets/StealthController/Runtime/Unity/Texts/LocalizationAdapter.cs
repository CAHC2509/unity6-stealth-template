using System;
using UnityEngine;

namespace StealthController.Unity
{
    public class LocalizationAdapter
    {
        private readonly TextTableData table;
        private int languageIndex;

        public event Action LanguageChanged;

        public LocalizationAdapter(TextTableData table)
        {
            this.table = table;
            languageIndex = ResolveIndex(SystemLanguageCode());
        }

        public void SetLanguage(string languageCode)
        {
            int index = ResolveIndex(languageCode);
            if (index == languageIndex) return;

            languageIndex = index;
            LanguageChanged?.Invoke();
        }

        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;

            if (table.TryGetText(key, languageIndex, out string text) && !string.IsNullOrEmpty(text)) return text;
            if (table.TryGetText(key, StealthUnityConstants.TEXT_FALLBACK_LANGUAGE_INDEX, out string fallback) && !string.IsNullOrEmpty(fallback)) return fallback;

            Debug.LogWarning($"{nameof(LocalizationAdapter)}: no text for key '{key}' in '{table.name}'.", table);
            return key;
        }

        private int ResolveIndex(string languageCode)
        {
            int index = table.IndexOfLanguage(languageCode);
            return index >= 0 ? index : StealthUnityConstants.TEXT_FALLBACK_LANGUAGE_INDEX;
        }

        private static string SystemLanguageCode()
        {
            return Application.systemLanguage == SystemLanguage.Spanish ? StealthUnityConstants.LANGUAGE_CODE_SPANISH : StealthUnityConstants.LANGUAGE_CODE_ENGLISH;
        }
    }
}
