using TMPro;
using UnityEngine;

namespace StealthController.Unity.UI
{
    public class ActionPromptView : StealthBehaviourBase
    {
        [Header("Elements")]
        [SerializeField] private TMP_Text keyText;
        [SerializeField, Tooltip("Optional. The action name next to the key.")] private LocalizedTextView label;
        [SerializeField, Tooltip("Optional. Shown when the action asks to hold the key.")] private GameObject holdTag;
        [SerializeField, Tooltip("Optional. The text inside the hold tag.")] private LocalizedTextView holdText;

        [Header("Behaviour")]
        [SerializeField] private bool requiresHold;

        public override void Initialize(params object[] parameters)
        {
            if (!RequireDependency(keyText, nameof(keyText))) return;

            base.Initialize(parameters);

            if (label != null)
                label.Initialize(parameters);

            if (holdText != null)
                holdText.Initialize(parameters);

            if (holdTag != null)
                holdTag.SetActive(requiresHold);
        }

        public override void Conclude()
        {
            if (label != null)
                label.Conclude();

            if (holdText != null)
                holdText.Conclude();

            base.Conclude();
        }

        public void Show(string keyLabel)
        {
            keyText.text = keyLabel;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
