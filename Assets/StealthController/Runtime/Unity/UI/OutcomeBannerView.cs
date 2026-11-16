using UnityEngine;

namespace StealthController.Unity.UI
{
    public class OutcomeBannerView : FadeViewBase
    {
        [Header("Message")]
        [SerializeField] private LocalizedTextView message;

        public override void Initialize(params object[] parameters)
        {
            if (!RequireDependency(message, nameof(message))) return;

            base.Initialize(parameters);

            message.Initialize(parameters);
            HideInstant();
        }

        public override void Conclude()
        {
            if (message != null)
                message.Conclude();

            base.Conclude();
        }

        public void ShowMessage(string textKey)
        {
            message.SetKey(textKey);
            Show();
        }
    }
}
