using UnityEngine;

public class SensitivitySettingsController : SettingsSubMenuControllerBase<SensitivitySettingsView>
{
    [SerializeField] private SensitivitySettingsData sensitivitySettingsData;

    public override void Initialize(params object[] parameters)
    {
        base.Initialize(parameters);

        LoadSettings();

        View.SetData(sensitivitySettingsData);
    }

    public override void SaveSettings()
    {
        SaveSystem.Save(Constants.SENSITIVITY_SETTINGS_SAVE_KEY, sensitivitySettingsData);
    }

    public override void LoadSettings()
    {
        sensitivitySettingsData = SaveSystem.Load<SensitivitySettingsData>(Constants.SENSITIVITY_SETTINGS_SAVE_KEY);
        ShareSettings();
    }

    public override void RestoreDefaultSettings()
    {
        sensitivitySettingsData = new SensitivitySettingsData();
        ShareSettings();

        View.SetData(sensitivitySettingsData);
        SaveSettings();
    }

    public void UpdateGeneralSensitivity(float value)
    {
        sensitivitySettingsData.GeneralSensitivity = value;
    }

    public void UpdateInvertY(bool value)
    {
        sensitivitySettingsData.InvertY = value;
    }

    private void ShareSettings()
    {
        Globals.SensitivitySettings = sensitivitySettingsData;
    }
}
