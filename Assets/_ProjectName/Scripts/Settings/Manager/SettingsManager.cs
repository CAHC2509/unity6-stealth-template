using System;
using UnityEngine;

public class SettingsManager : ManagerBase
{
    public event Action SettingsClosed;

    [SerializeField] private SettingsMenuController settingsMenuController;

    public override void Initialize(params object[] parameters)
    {
        if (!RequireDependency(settingsMenuController, nameof(settingsMenuController))) return;

        base.Initialize(parameters);

        settingsMenuController.Initialize(this);
    }

    public override void Conclude()
    {
        base.Conclude();

        settingsMenuController.Conclude();
    }

    public void OpenSettingsMenu()
    {
        settingsMenuController.ShowMenu();
    }

    public void CloseSettingsMenu()
    {
        settingsMenuController.HideMenu()
            .OnComplete(() => SettingsClosed?.Invoke());
    }
}
