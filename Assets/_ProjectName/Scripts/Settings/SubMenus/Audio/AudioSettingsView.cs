using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioSettingsView : CancelableUIViewBase
{
    [Header("Transition settings")]
    [SerializeField] private UIFadeTransitionData fadeTransitionData;

    [Header("Volume Sliders")]
    [SerializeField] private Slider generalVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider effectsVolumeSlider;
    [SerializeField] private Slider uiVolumeSlider;
    [SerializeField] private Button restoreDefaultSettingsButton;

    [Header("Volume Labels")]
    [SerializeField] private TextMeshProUGUI generalVolumeText;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI effectsVolumeText;
    [SerializeField] private TextMeshProUGUI uiVolumeText;

    private AudioSettingsController audioSettingsController;

    protected override UITransitionBase CreateTransition() => new UIFadeTransition(fadeTransitionData);

    protected override void Awake()
    {
        defaultSelection = generalVolumeSlider;

        base.Awake();
    }

    public override void Initialize(params object[] parameters)
    {
        audioSettingsController = parameters[0] as AudioSettingsController;

        RequireDependency(audioSettingsController, nameof(audioSettingsController));
        RequireDependency(generalVolumeSlider, nameof(generalVolumeSlider));
        RequireDependency(musicVolumeSlider, nameof(musicVolumeSlider));
        RequireDependency(effectsVolumeSlider, nameof(effectsVolumeSlider));
        RequireDependency(uiVolumeSlider, nameof(uiVolumeSlider));
        RequireDependency(restoreDefaultSettingsButton, nameof(restoreDefaultSettingsButton));
        RequireDependency(generalVolumeText, nameof(generalVolumeText));
        RequireDependency(musicVolumeText, nameof(musicVolumeText));
        RequireDependency(effectsVolumeText, nameof(effectsVolumeText));
        RequireDependency(uiVolumeText, nameof(uiVolumeText));

        base.Initialize(parameters);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        generalVolumeSlider.onValueChanged.AddListener(OnGeneralVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        effectsVolumeSlider.onValueChanged.AddListener(OnEffectsVolumeChanged);
        uiVolumeSlider.onValueChanged.AddListener(OnUIVolumeChanged);
        restoreDefaultSettingsButton.onClick.AddListener(audioSettingsController.RestoreDefaultSettings);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        generalVolumeSlider.onValueChanged.RemoveListener(OnGeneralVolumeChanged);
        musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        effectsVolumeSlider.onValueChanged.RemoveListener(OnEffectsVolumeChanged);
        uiVolumeSlider.onValueChanged.RemoveListener(OnUIVolumeChanged);
        restoreDefaultSettingsButton.onClick.RemoveListener(audioSettingsController.RestoreDefaultSettings);
    }

    public void SetData(AudioSettingsData data)
    {
        generalVolumeSlider.SetValueWithoutNotify(data.GeneralVolume);
        musicVolumeSlider.SetValueWithoutNotify(data.MusicVolume);
        effectsVolumeSlider.SetValueWithoutNotify(data.EffectsVolume);
        uiVolumeSlider.SetValueWithoutNotify(data.UIVolume);

        UpdateVolumeLabel(generalVolumeText, data.GeneralVolume);
        UpdateVolumeLabel(musicVolumeText, data.MusicVolume);
        UpdateVolumeLabel(effectsVolumeText, data.EffectsVolume);
        UpdateVolumeLabel(uiVolumeText, data.UIVolume);
    }

    protected override void OnCancelRequested()
    {
        audioSettingsController.CloseSubMenu();
    }

    private void OnGeneralVolumeChanged(float value)
    {
        audioSettingsController.UpdateGeneralVolume(value);
        UpdateVolumeLabel(generalVolumeText, value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        audioSettingsController.UpdateMusicVolume(value);
        UpdateVolumeLabel(musicVolumeText, value);
    }

    private void OnEffectsVolumeChanged(float value)
    {
        audioSettingsController.UpdateEffectsVolume(value);
        UpdateVolumeLabel(effectsVolumeText, value);
    }

    private void OnUIVolumeChanged(float value)
    {
        audioSettingsController.UpdateUIVolume(value);
        UpdateVolumeLabel(uiVolumeText, value);
    }

    private void UpdateVolumeLabel(TMP_Text label, float value)
    {
        label.text = TextFormatUtility.ToPercentText(value);
    }
}
