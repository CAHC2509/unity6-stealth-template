using UnityEngine;
using UnityEngine.Audio;

public class AudioSettingsController : SettingsSubMenuControllerBase<AudioSettingsView>
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSettingsData audioSettingsData;

    public override void Initialize(params object[] parameters)
    {
        RequireDependency(audioMixer, nameof(audioMixer));

        base.Initialize(parameters);

        LoadSettings();
        ApplyAllVolumes();

        View.SetData(audioSettingsData);
    }

    public override void SaveSettings()
    {
        SaveSystem.Save(Constants.AUDIO_SETTINGS_SAVE_KEY, audioSettingsData);
    }

    public override void LoadSettings()
    {
        audioSettingsData = SaveSystem.Load<AudioSettingsData>(Constants.AUDIO_SETTINGS_SAVE_KEY);
    }

    public override void RestoreDefaultSettings()
    {
        audioSettingsData = new AudioSettingsData();

        ApplyAllVolumes();
        View.SetData(audioSettingsData);

        SaveSettings();
    }

    public void UpdateGeneralVolume(float volume)
    {
        audioSettingsData.GeneralVolume = volume;
        SetVolume(Constants.AUDIO_GENERAL_VOL_SAVE_KEY, volume);
    }

    public void UpdateMusicVolume(float volume)
    {
        audioSettingsData.MusicVolume = volume;
        SetVolume(Constants.AUDIO_MUSIC_VOL_SAVE_KEY, volume);
    }

    public void UpdateEffectsVolume(float volume)
    {
        audioSettingsData.EffectsVolume = volume;
        SetVolume(Constants.AUDIO_EFFECTS_VOL_SAVE_KEY, volume);
    }

    public void UpdateUIVolume(float volume)
    {
        audioSettingsData.UIVolume = volume;
        SetVolume(Constants.AUDIO_UI_VOL_SAVE_KEY, volume);
    }

    private void ApplyAllVolumes()
    {
        SetVolume(Constants.AUDIO_GENERAL_VOL_SAVE_KEY, audioSettingsData.GeneralVolume);
        SetVolume(Constants.AUDIO_MUSIC_VOL_SAVE_KEY, audioSettingsData.MusicVolume);
        SetVolume(Constants.AUDIO_EFFECTS_VOL_SAVE_KEY, audioSettingsData.EffectsVolume);
        SetVolume(Constants.AUDIO_UI_VOL_SAVE_KEY, audioSettingsData.UIVolume);
    }

    private void SetVolume(string parameter, float value)
    {
        float dB = LinearToDecibels(value);
        audioMixer.SetFloat(parameter, dB);
    }

    private float LinearToDecibels(float value)
    {
        if (value <= Constants.AUDIO_MIN_LINEAR_VOLUME)
            return Constants.AUDIO_MUTED_DECIBELS;

        return Mathf.Log10(value) * Constants.AUDIO_DECIBELS_PER_LOG_UNIT;
    }
}
