using StealthController.Core;
using UnityEngine;
using UnityEngine.UI;

namespace StealthController.Unity.UI
{
    public class PostureHudView : FadeViewBase
    {
        [Header("Current Posture")]
        [SerializeField] private Image postureIcon;
        [SerializeField] private Sprite standingIcon;
        [SerializeField] private Sprite crouchedIcon;
        [SerializeField] private Sprite proneIcon;
        [SerializeField] private LocalizedTextView postureName;

        [Header("Action Prompts")]
        [SerializeField] private ActionPromptView sprintPrompt;
        [SerializeField] private ActionPromptView standPrompt;
        [SerializeField] private ActionPromptView crouchPrompt;
        [SerializeField] private ActionPromptView pronePrompt;

        [Header("Feedback")]
        [SerializeField, Min(0f)] private float postureChangePunch = StealthUnityConstants.POSTURE_HUD_PUNCH_SCALE;
        [SerializeField, Min(0f)] private float postureChangeDuration = StealthUnityConstants.POSTURE_HUD_PUNCH_DURATION;

        private bool hasShownPosture;
        private PlayerPoseState shownPosture;
        private float punchElapsed = float.MaxValue;

        public override void Initialize(params object[] parameters)
        {
            bool hasDependencies = RequireDependency(postureIcon, nameof(postureIcon))
                & RequireDependency(postureName, nameof(postureName))
                & RequireDependency(sprintPrompt, nameof(sprintPrompt))
                & RequireDependency(standPrompt, nameof(standPrompt))
                & RequireDependency(crouchPrompt, nameof(crouchPrompt))
                & RequireDependency(pronePrompt, nameof(pronePrompt));

            if (!hasDependencies) return;

            base.Initialize(parameters);

            postureName.Initialize(parameters);
            sprintPrompt.Initialize(parameters);
            standPrompt.Initialize(parameters);
            crouchPrompt.Initialize(parameters);
            pronePrompt.Initialize(parameters);
        }

        public override void Conclude()
        {
            if (postureName != null)
                postureName.Conclude();

            if (sprintPrompt != null)
                sprintPrompt.Conclude();

            if (standPrompt != null)
                standPrompt.Conclude();

            if (crouchPrompt != null)
                crouchPrompt.Conclude();

            if (pronePrompt != null)
                pronePrompt.Conclude();

            base.Conclude();
        }

        public void ShowPosture(PlayerPoseState posture, string sprintKey, string standKey, string crouchKey)
        {
            bool hasChanged = hasShownPosture && posture != shownPosture;
            hasShownPosture = true;
            shownPosture = posture;

            postureIcon.sprite = IconFor(posture);
            postureName.SetKey(NameKeyFor(posture));

            sprintPrompt.Show(sprintKey);
            ShowPromptUnless(standPrompt, posture == PlayerPoseState.Standing, standKey);
            ShowPromptUnless(crouchPrompt, posture == PlayerPoseState.Crouched, crouchKey);
            ShowPromptUnless(pronePrompt, posture == PlayerPoseState.Crawling, crouchKey);

            if (hasChanged)
                punchElapsed = 0f;
        }

        private void LateUpdate()
        {
            if (punchElapsed > postureChangeDuration) return;

            punchElapsed += Time.unscaledDeltaTime;
            float progress = postureChangeDuration > 0f ? Mathf.Clamp01(punchElapsed / postureChangeDuration) : 1f;
            float scale = 1f + postureChangePunch * Mathf.Sin(progress * Mathf.PI);
            postureIcon.rectTransform.localScale = Vector3.one * scale;
        }

        private static void ShowPromptUnless(ActionPromptView prompt, bool isCurrentPosture, string keyLabel)
        {
            if (isCurrentPosture)
                prompt.Hide();
            else
                prompt.Show(keyLabel);
        }

        private Sprite IconFor(PlayerPoseState posture)
        {
            switch (posture)
            {
                case PlayerPoseState.Crouched: return crouchedIcon;
                case PlayerPoseState.Crawling: return proneIcon;
                default: return standingIcon;
            }
        }

        private static string NameKeyFor(PlayerPoseState posture)
        {
            switch (posture)
            {
                case PlayerPoseState.Crouched: return StealthUnityConstants.TEXT_KEY_POSTURE_CROUCHED;
                case PlayerPoseState.Crawling: return StealthUnityConstants.TEXT_KEY_POSTURE_PRONE;
                default: return StealthUnityConstants.TEXT_KEY_POSTURE_STANDING;
            }
        }
    }
}
