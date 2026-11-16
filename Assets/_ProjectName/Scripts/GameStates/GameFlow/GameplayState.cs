using StealthController.Unity;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class GameplayState : GameStateBase
{
    [SerializeField] private StealthGameController stealthGame;
    [SerializeField] private StealthDevToolsProviderController devToolsProvider;

    public override void Initialize(params object[] parameters)
    {
        bool hasDependencies = RequireDependency(stealthGame, nameof(stealthGame))
            & RequireDependency(devToolsProvider, nameof(devToolsProvider));

        if (!hasDependencies) return;

        stealthGame.Initialize();
        devToolsProvider.Initialize(stealthGame.Session);
        ApplyLanguage(LocalizationSettings.SelectedLocale);

        base.Initialize(parameters);
    }

    public override void Conclude()
    {
        base.Conclude();

        if (devToolsProvider != null)
            devToolsProvider.Conclude();

        if (stealthGame != null)
            stealthGame.Conclude();
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        stealthGame.ExitRequested += GoToMain;
        LocalizationSettings.SelectedLocaleChanged += ApplyLanguage;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        LocalizationSettings.SelectedLocaleChanged -= ApplyLanguage;

        if (stealthGame != null)
            stealthGame.ExitRequested -= GoToMain;
    }

    public void GoToMain()
    {
        NextState = GameStates.Main;
        ExitState();
    }

    private void ApplyLanguage(Locale locale)
    {
        if (locale == null) return;

        stealthGame.SetLanguage(locale.Identifier.Code);
    }
}
