using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlBindingView : UIViewBase
{
    [Header("Transition settings")]
    [SerializeField] private UIFadeTransitionData fadeTransitionData;

    [Header("Display")]
    [SerializeField] private TextMeshProUGUI actionLabel;
    [SerializeField] private TextMeshProUGUI bindingText;

    [Header("Buttons")]
    [SerializeField] private Button rebindButton;
    [SerializeField] private Button resetButton;

    private ControlBindingController controlBindingController;

    public Button RebindButton => rebindButton;

    protected override bool RequiresSelection => false;
    protected override bool StartsHidden => false;

    protected override UITransitionBase CreateTransition() => new UIFadeTransition(fadeTransitionData);

    public override void Initialize(params object[] parameters)
    {
        controlBindingController = parameters[0] as ControlBindingController;

        RequireDependency(controlBindingController, nameof(controlBindingController));
        RequireDependency(actionLabel, nameof(actionLabel));
        RequireDependency(bindingText, nameof(bindingText));
        RequireDependency(rebindButton, nameof(rebindButton));
        RequireDependency(resetButton, nameof(resetButton));

        base.Initialize(parameters);

        ConfigureButtonNavigation();
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        rebindButton.onClick.AddListener(OnRebindClicked);
        resetButton.onClick.AddListener(OnResetClicked);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        rebindButton.onClick.RemoveListener(OnRebindClicked);
        resetButton.onClick.RemoveListener(OnResetClicked);
    }

    public void SetActionLabel(string text)
    {
        actionLabel.text = text;
    }

    public void SetBindingText(string text)
    {
        bindingText.text = text;
    }

    public void SetInteractable(bool interactable)
    {
        rebindButton.interactable = interactable;
        resetButton.interactable = interactable;
    }

    private void OnRebindClicked() => controlBindingController.RequestRebind();
    private void OnResetClicked() => controlBindingController.RequestReset();

    private void ConfigureButtonNavigation()
    {
        Navigation rebindNavigation = rebindButton.navigation;
        rebindNavigation.mode = Navigation.Mode.Explicit;
        rebindNavigation.selectOnRight = resetButton;
        rebindButton.navigation = rebindNavigation;

        Navigation resetNavigation = resetButton.navigation;
        resetNavigation.mode = Navigation.Mode.Explicit;
        resetNavigation.selectOnLeft = rebindButton;
        resetButton.navigation = resetNavigation;
    }
}
