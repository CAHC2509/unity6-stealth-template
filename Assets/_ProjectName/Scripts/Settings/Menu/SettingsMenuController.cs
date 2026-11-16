using System;
using UnityEngine;

public class SettingsMenuController : UIControllerBase<SettingsMenuView>
{
    [SerializeField] private GraphicsSettingsController graphicsSettingsController;
    [SerializeField] private AudioSettingsController audioSettingsController;
    [SerializeField] private ControlsSettingsController controlsSettingsController;
    [SerializeField] private SensitivitySettingsController sensitivitySettingsController;
    [SerializeField] private LanguageSettingsController languageSettingsController;
    [SerializeField] private DeveloperSettingsController developerSettingsController;

    private SettingsManager settingsManager;
    private int navigationVersion;

    public override void Initialize(params object[] parameters)
    {
        settingsManager = parameters[0] as SettingsManager;

        RequireDependency(settingsManager, nameof(settingsManager));
        RequireDependency(graphicsSettingsController, nameof(graphicsSettingsController));
        RequireDependency(audioSettingsController, nameof(audioSettingsController));
        RequireDependency(controlsSettingsController, nameof(controlsSettingsController));
        RequireDependency(sensitivitySettingsController, nameof(sensitivitySettingsController));
        RequireDependency(languageSettingsController, nameof(languageSettingsController));
        RequireDependency(developerSettingsController, nameof(developerSettingsController));

        base.Initialize(parameters);

        graphicsSettingsController.Initialize(this);
        audioSettingsController.Initialize(this);
        controlsSettingsController.Initialize(this);
        sensitivitySettingsController.Initialize(this);
        languageSettingsController.Initialize(this);
        developerSettingsController.Initialize(this);

        View.SetDeveloperAvailable(PlatformCapabilityUtility.SupportsDevTools);
    }

    public override void Conclude()
    {
        graphicsSettingsController.Conclude();
        audioSettingsController.Conclude();
        controlsSettingsController.Conclude();
        sensitivitySettingsController.Conclude();
        languageSettingsController.Conclude();
        developerSettingsController.Conclude();

        base.Conclude();
    }

    public UITransitionHandle ShowMenu() => View.ShowView();

    public UITransitionHandle HideMenu() => View.HideView();

    public void OpenGraphicsMenu() => OpenSubMenuAfterHiding(graphicsSettingsController.OpenSubMenu);

    public void OpenAudioMenu() => OpenSubMenuAfterHiding(audioSettingsController.OpenSubMenu);

    public void OpenControlsMenu() => OpenSubMenuAfterHiding(controlsSettingsController.OpenSubMenu);

    public void OpenSensitivityMenu() => OpenSubMenuAfterHiding(sensitivitySettingsController.OpenSubMenu);

    public void OpenLanguageMenu() => OpenSubMenuAfterHiding(languageSettingsController.OpenSubMenu);

    public void OpenDeveloperMenu() => OpenSubMenuAfterHiding(developerSettingsController.OpenSubMenu);

    public void CloseSettingsMenu()
    {
        settingsManager.CloseSettingsMenu();
    }

    public void ReturnFromSubMenu(UITransitionHandle subMenuHideHandle)
    {
        RunIfStillCurrent(subMenuHideHandle, () => ShowMenu());
    }

    private void OpenSubMenuAfterHiding(Action openSubMenu)
    {
        RunIfStillCurrent(HideMenu(), openSubMenu);
    }

    private void RunIfStillCurrent(UITransitionHandle handle, Action navigation)
    {
        int requestVersion = ++navigationVersion;

        handle.OnComplete(() =>
        {
            if (requestVersion != navigationVersion) return;

            navigation();
        });
    }
}
