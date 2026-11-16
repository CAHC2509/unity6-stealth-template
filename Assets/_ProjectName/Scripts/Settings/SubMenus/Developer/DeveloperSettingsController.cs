using UnityEngine;

public class DeveloperSettingsController : SettingsSubMenuControllerBase<DeveloperSettingsView>
{
    [SerializeField] private DeveloperSettingsData developerSettingsData;

    public override void Initialize(params object[] parameters)
    {
        base.Initialize(parameters);

        LoadSettings();

        View.SetData(developerSettingsData);
    }

    public override void SaveSettings()
    {
        SaveSystem.Save(Constants.DEVELOPER_SETTINGS_SAVE_KEY, developerSettingsData);
    }

    public override void LoadSettings()
    {
        developerSettingsData = SaveSystem.Load<DeveloperSettingsData>(Constants.DEVELOPER_SETTINGS_SAVE_KEY);
        ShareSettings();
    }

    public override void RestoreDefaultSettings()
    {
        developerSettingsData = new DeveloperSettingsData();
        ShareSettings();

        View.SetData(developerSettingsData);
        SaveSettings();
    }

    public void UpdateDevToolsEnabled(bool enabled)
    {
        developerSettingsData.DevToolsEnabled = enabled;
        ShareSettings();
    }

    private void ShareSettings()
    {
        DevToolsManager.SetDevToolsEnabledRequest?.Invoke(PlatformCapabilityUtility.SupportsDevTools && developerSettingsData.DevToolsEnabled);
    }
}
