using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SensitivitySettingsView : CancelableUIViewBase
{
    [Header("Transition settings")]
    [SerializeField] private UIFadeTransitionData fadeTransitionData;

    [Header("Sensitivity Controls")]
    [SerializeField] private Slider generalSensitivitySlider;
    [SerializeField] private Toggle invertYToggle;
    [SerializeField] private Button restoreDefaultSettings;

    [Header("Sensitivity Labels")]
    [SerializeField] private TMP_Text generalSensitivityText;

    private SensitivitySettingsController sensitivitySettingsController;

    protected override UITransitionBase CreateTransition() => new UIFadeTransition(fadeTransitionData);

    protected override void Awake()
    {
        defaultSelection = generalSensitivitySlider;

        base.Awake();
    }

    public override void Initialize(params object[] parameters)
    {
        sensitivitySettingsController = parameters[0] as SensitivitySettingsController;

        RequireDependency(sensitivitySettingsController, nameof(sensitivitySettingsController));
        RequireDependency(generalSensitivitySlider, nameof(generalSensitivitySlider));
        RequireDependency(invertYToggle, nameof(invertYToggle));
        RequireDependency(restoreDefaultSettings, nameof(restoreDefaultSettings));
        RequireDependency(generalSensitivityText, nameof(generalSensitivityText));

        base.Initialize(parameters);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        generalSensitivitySlider.onValueChanged.AddListener(OnGeneralSensitivityChanged);
        invertYToggle.onValueChanged.AddListener(OnInvertYChanged);
        restoreDefaultSettings.onClick.AddListener(sensitivitySettingsController.RestoreDefaultSettings);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        generalSensitivitySlider.onValueChanged.RemoveListener(OnGeneralSensitivityChanged);
        invertYToggle.onValueChanged.RemoveListener(OnInvertYChanged);
        restoreDefaultSettings.onClick.RemoveListener(sensitivitySettingsController.RestoreDefaultSettings);
    }

    public void SetData(SensitivitySettingsData data)
    {
        generalSensitivitySlider.SetValueWithoutNotify(data.GeneralSensitivity);
        invertYToggle.SetIsOnWithoutNotify(data.InvertY);

        UpdateGeneralSensitivityLabel(data.GeneralSensitivity);
    }

    protected override void OnCancelRequested()
    {
        sensitivitySettingsController.CloseSubMenu();
    }

    private void OnGeneralSensitivityChanged(float value)
    {
        sensitivitySettingsController.UpdateGeneralSensitivity(value);
        UpdateGeneralSensitivityLabel(value);
    }

    private void OnInvertYChanged(bool value)
    {
        sensitivitySettingsController.UpdateInvertY(value);
    }

    private void UpdateGeneralSensitivityLabel(float value)
    {
        generalSensitivityText.text = TextFormatUtility.ToPercentText(value);
    }
}
