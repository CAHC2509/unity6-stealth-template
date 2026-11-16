using System;
using UnityEngine;

[Serializable]
public class AudioSettingsData
{
    [SerializeField] private float generalVolume = Constants.AUDIO_DEFAULT_GENERAL_VOL;
    [SerializeField] private float musicVolume = Constants.AUDIO_DEFAULT_MUSIC_VOL;
    [SerializeField] private float effectsVolume = Constants.AUDIO_DEFAULT_EFFECTS_VOL;
    [SerializeField] private float uiVolume = Constants.AUDIO_DEFAULT_UI_VOL;

    public float GeneralVolume { get => generalVolume; set => generalVolume = value; }
    public float MusicVolume { get => musicVolume; set => musicVolume = value; }
    public float EffectsVolume { get => effectsVolume; set => effectsVolume = value; }
    public float UIVolume { get => uiVolume; set => uiVolume = value; }
}
