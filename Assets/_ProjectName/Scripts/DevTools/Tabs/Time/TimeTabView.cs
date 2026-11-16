using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeTabView : DevToolsTabViewBase
{
    [Header("Controls")]
    [SerializeField] private Slider timeScaleSlider;
    [SerializeField] private TMP_Text timeScaleValue;
    [SerializeField] private Toggle pauseToggle;
    [SerializeField] private Button stepButton;
    [SerializeField] private Button resetButton;

    private TimeTabController timeTabController;

    public override void Initialize(params object[] parameters)
    {
        timeTabController = parameters[0] as TimeTabController;

        bool hasDependencies = RequireDependency(timeTabController, nameof(timeTabController))
            & RequireDependency(timeScaleSlider, nameof(timeScaleSlider))
            & RequireDependency(timeScaleValue, nameof(timeScaleValue))
            & RequireDependency(pauseToggle, nameof(pauseToggle))
            & RequireDependency(stepButton, nameof(stepButton))
            & RequireDependency(resetButton, nameof(resetButton));

        if (!hasDependencies) return;

        timeScaleSlider.minValue = Constants.DEVTOOLS_MIN_TIME_SCALE;
        timeScaleSlider.maxValue = Constants.DEVTOOLS_MAX_TIME_SCALE;
        timeScaleSlider.wholeNumbers = false;

        base.Initialize(parameters);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        timeScaleSlider.onValueChanged.AddListener(timeTabController.OnTimeScaleChanged);
        pauseToggle.onValueChanged.AddListener(timeTabController.OnPauseToggled);
        stepButton.onClick.AddListener(timeTabController.OnStepClicked);
        resetButton.onClick.AddListener(timeTabController.OnResetClicked);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        timeScaleSlider.onValueChanged.RemoveListener(timeTabController.OnTimeScaleChanged);
        pauseToggle.onValueChanged.RemoveListener(timeTabController.OnPauseToggled);
        stepButton.onClick.RemoveListener(timeTabController.OnStepClicked);
        resetButton.onClick.RemoveListener(timeTabController.OnResetClicked);
    }

    public void SetData(float timeScale, bool isPaused)
    {
        timeScaleSlider.SetValueWithoutNotify(timeScale);
        pauseToggle.SetIsOnWithoutNotify(isPaused);
        stepButton.interactable = isPaused;
        timeScaleValue.text = Constants.DEVTOOLS_TIME_SCALE_PREFIX + DevToolsReportUtility.FormatDecimal(timeScale);
    }
}
