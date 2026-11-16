using System;
using StealthController.Core;
using StealthController.Unity.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StealthController.Unity
{
    public class StealthGameController : StealthBehaviourBase
    {
        [Header("Startup")]
        [SerializeField, BasicSetting, Tooltip("Starts the game on Start. Turn it off when another script calls Initialize.")] private bool initializeOnStart = true;

        [Header("Session")]
        [SerializeField] private StealthSessionManager session;
        [SerializeField] private TextTableData textTable;

        [Header("HUD")]
        [SerializeField] private PostureHudController postureHud;
        [SerializeField] private TakedownPromptController takedownPrompt;

        [Header("Overlays")]
        [SerializeField] private PauseMenuView pauseMenu;
        [SerializeField] private OutcomeBannerView outcomeBanner;
        [SerializeField, BasicSetting, Min(0f), Tooltip("Seconds the Captured or Zone cleared banner stays on screen.")] private float outcomeHoldDuration = StealthUnityConstants.OUTCOME_BANNER_HOLD_DURATION;

        private LocalizationAdapter localization;
        private InputBindingDisplayAdapter bindingDisplay;
        private InputAction pauseAction;
        private InputAction cancelAction;
        private float outcomeTimeLeft;
        private bool isShowingOutcome;

        public event Action ExitRequested;

        public StealthSessionManager Session => session;

        private void Start()
        {
            if (initializeOnStart)
                Initialize();
        }

        private void OnDestroy()
        {
            Conclude();
        }

        public override void Initialize(params object[] parameters)
        {
            if (IsInitialized) return;

            bool hasDependencies = RequireDependency(session, nameof(session))
                & RequireDependency(textTable, nameof(textTable))
                & RequireDependency(postureHud, nameof(postureHud))
                & RequireDependency(takedownPrompt, nameof(takedownPrompt))
                & RequireDependency(pauseMenu, nameof(pauseMenu))
                & RequireDependency(outcomeBanner, nameof(outcomeBanner));

            if (!hasDependencies) return;

            session.Initialize();

            if (!session.IsRunning)
            {
                session.Conclude();
                return;
            }

            localization = new LocalizationAdapter(textTable);
            bindingDisplay = new InputBindingDisplayAdapter(session.Input);
            pauseAction = session.Input.Find(StealthInputAction.Pause);
            cancelAction = session.Input.Find(StealthInputAction.Cancel);

            base.Initialize(parameters);

            postureHud.Initialize(session, bindingDisplay, localization);
            takedownPrompt.Initialize(session, bindingDisplay, localization);
            pauseMenu.Initialize(localization);
            outcomeBanner.Initialize(localization);
        }

        public override void Conclude()
        {
            if (!IsInitialized) return;

            isShowingOutcome = false;

            outcomeBanner.Conclude();
            pauseMenu.Conclude();
            takedownPrompt.Conclude();
            postureHud.Conclude();

            base.Conclude();

            session.Conclude();
        }

        public void SetLanguage(string languageCode)
        {
            if (localization == null) return;

            localization.SetLanguage(languageCode);
        }

        protected override void AddListeners()
        {
            base.AddListeners();

            bindingDisplay.AddListeners();
            session.OutcomeReached += OnOutcomeReached;
            pauseMenu.ResumeSelected += OnResumeSelected;
            pauseMenu.RestartSelected += OnRestartSelected;
            pauseMenu.ExitSelected += OnExitSelected;

            if (pauseAction != null)
                pauseAction.started += OnPausePressed;

            if (cancelAction != null)
                cancelAction.started += OnCancelPressed;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();

            if (cancelAction != null)
                cancelAction.started -= OnCancelPressed;

            if (pauseAction != null)
                pauseAction.started -= OnPausePressed;

            pauseMenu.ExitSelected -= OnExitSelected;
            pauseMenu.RestartSelected -= OnRestartSelected;
            pauseMenu.ResumeSelected -= OnResumeSelected;
            session.OutcomeReached -= OnOutcomeReached;
            bindingDisplay.RemoveListeners();
        }

        private void Update()
        {
            if (!isShowingOutcome) return;

            outcomeTimeLeft -= Time.deltaTime;
            if (outcomeTimeLeft > 0f) return;

            isShowingOutcome = false;
            session.ResolvePendingOutcome();
            outcomeBanner.Hide();
        }

        private void OnPausePressed(InputAction.CallbackContext context)
        {
            if (!session.CanPause) return;

            session.SetPaused(true);
            pauseMenu.Open(ExitRequested != null);
        }

        private void OnCancelPressed(InputAction.CallbackContext context)
        {
            if (!pauseMenu.IsVisible) return;

            OnResumeSelected();
        }

        private void OnResumeSelected()
        {
            pauseMenu.Close(ResumeSession);
        }

        private void ResumeSession()
        {
            session.SetPaused(false);
        }

        private void OnRestartSelected()
        {
            pauseMenu.Close();
            isShowingOutcome = false;
            outcomeBanner.HideInstant();
            session.RestartMission();
        }

        private void OnExitSelected()
        {
            pauseMenu.Close();
            ExitRequested?.Invoke();
        }

        private void OnOutcomeReached(StealthSessionOutcome outcome)
        {
            string messageKey = outcome == StealthSessionOutcome.Captured ? StealthUnityConstants.TEXT_KEY_OUTCOME_CAPTURED : StealthUnityConstants.TEXT_KEY_OUTCOME_ZONE_CLEARED;

            outcomeBanner.ShowMessage(messageKey);
            outcomeTimeLeft = outcomeHoldDuration;
            isShowingOutcome = true;
        }
    }
}
