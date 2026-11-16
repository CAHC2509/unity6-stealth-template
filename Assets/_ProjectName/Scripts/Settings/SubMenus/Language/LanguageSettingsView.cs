using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageSettingsView : CancelableUIViewBase
{
    [Header("Transition settings")]
    [SerializeField] private UIFadeTransitionData fadeTransitionData;

    [Header("Language settings")]
    [SerializeField] private ObservableDropdown languageDropdown;
    [SerializeField] private Button restoreDefaultSettingsButton;

    private LanguageSettingsController languageSettingsController;

    protected override UITransitionBase CreateTransition() => new UIFadeTransition(fadeTransitionData);

    protected override void Awake()
    {
        defaultSelection = languageDropdown;

        base.Awake();
    }

    public override void Initialize(params object[] parameters)
    {
        languageSettingsController = parameters[0] as LanguageSettingsController;

        RequireDependency(languageSettingsController, nameof(languageSettingsController));
        RequireDependency(languageDropdown, nameof(languageDropdown));
        RequireDependency(restoreDefaultSettingsButton, nameof(restoreDefaultSettingsButton));

        base.Initialize(parameters);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        languageDropdown.Opened += RegisterSubViewOpen;
        languageDropdown.Closed += RegisterSubViewClose;
        languageDropdown.onValueChanged.AddListener(languageSettingsController.UpdateLanguage);
        restoreDefaultSettingsButton.onClick.AddListener(languageSettingsController.RestoreDefaultSettings);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        languageDropdown.Opened -= RegisterSubViewOpen;
        languageDropdown.Closed -= RegisterSubViewClose;
        languageDropdown.onValueChanged.RemoveListener(languageSettingsController.UpdateLanguage);
        restoreDefaultSettingsButton.onClick.RemoveListener(languageSettingsController.RestoreDefaultSettings);
    }

    public void SetLanguageOptions(List<string> options)
    {
        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(options);
    }

    public void SetData(LanguageSettingsData data)
    {
        languageDropdown.SetValueWithoutNotify(data.LanguageIndex);
    }

    protected override void OnCancelRequested()
    {
        languageSettingsController.CloseSubMenu();
    }
}
