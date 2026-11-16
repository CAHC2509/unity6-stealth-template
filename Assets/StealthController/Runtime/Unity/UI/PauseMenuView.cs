using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StealthController.Unity.UI
{
    public class PauseMenuView : FadeViewBase
    {
        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField, Tooltip("Optional. Only shown when the game offers a way out of the mission.")] private Button exitButton;

        [Header("Texts")]
        [SerializeField] private LocalizedTextView[] texts = Array.Empty<LocalizedTextView>();

        public event Action ResumeSelected;
        public event Action RestartSelected;
        public event Action ExitSelected;

        public override void Initialize(params object[] parameters)
        {
            bool hasDependencies = RequireDependency(resumeButton, nameof(resumeButton))
                & RequireDependency(restartButton, nameof(restartButton));

            if (!hasDependencies) return;

            base.Initialize(parameters);

            foreach (LocalizedTextView text in texts)
            {
                if (text != null)
                    text.Initialize(parameters);
            }

            HideInstant();
        }

        public override void Conclude()
        {
            foreach (LocalizedTextView text in texts)
            {
                if (text != null)
                    text.Conclude();
            }

            base.Conclude();
        }

        protected override void AddListeners()
        {
            base.AddListeners();

            resumeButton.onClick.AddListener(OnResumeClicked);
            restartButton.onClick.AddListener(OnRestartClicked);

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();

            resumeButton.onClick.RemoveListener(OnResumeClicked);
            restartButton.onClick.RemoveListener(OnRestartClicked);

            if (exitButton != null)
                exitButton.onClick.RemoveListener(OnExitClicked);
        }

        public void Open(bool canExit)
        {
            if (exitButton != null)
                exitButton.gameObject.SetActive(canExit);

            Show();
            SelectDefault();
        }

        public void Close(Action onComplete = null)
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            Hide(onComplete);
        }

        private void SelectDefault()
        {
            if (EventSystem.current == null) return;

            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }

        private void OnResumeClicked()
        {
            ResumeSelected?.Invoke();
        }

        private void OnRestartClicked()
        {
            RestartSelected?.Invoke();
        }

        private void OnExitClicked()
        {
            ExitSelected?.Invoke();
        }
    }
}
