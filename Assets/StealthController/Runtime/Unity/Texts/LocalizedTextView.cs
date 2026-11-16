using TMPro;
using UnityEngine;

namespace StealthController.Unity
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTextView : StealthBehaviourBase
    {
        [SerializeField, Tooltip("Key in the text table. Leave empty when the owner sets it at runtime.")] private string key;

        private TMP_Text label;
        private LocalizationAdapter localization;

        public override void Initialize(params object[] parameters)
        {
            localization = parameters.Length > 0 ? parameters[0] as LocalizationAdapter : null;

            if (!RequireService(localization, nameof(localization))) return;

            label = GetComponent<TMP_Text>();

            base.Initialize(parameters);

            Refresh();
        }

        protected override void AddListeners()
        {
            base.AddListeners();

            localization.LanguageChanged += Refresh;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();

            if (localization != null)
                localization.LanguageChanged -= Refresh;
        }

        public void SetKey(string newKey)
        {
            key = newKey;
            Refresh();
        }

        private void Refresh()
        {
            if (localization == null || string.IsNullOrEmpty(key)) return;

            label.text = localization.Get(key);
        }
    }
}
