public abstract class SettingsSubMenuControllerBase<TView> : UIControllerBase<TView> where TView : UIViewBase
{
    private SettingsMenuController settingsMenuController;

    public override void Initialize(params object[] parameters)
    {
        settingsMenuController = parameters[0] as SettingsMenuController;

        RequireDependency(settingsMenuController, nameof(settingsMenuController));

        base.Initialize(parameters);
    }

    public virtual void OpenSubMenu()
    {
        View.ShowView();
    }

    public virtual void CloseSubMenu()
    {
        SaveSettings();

        settingsMenuController.ReturnFromSubMenu(View.HideView());
    }

    public abstract void SaveSettings();
    public abstract void LoadSettings();
    public abstract void RestoreDefaultSettings();
}
