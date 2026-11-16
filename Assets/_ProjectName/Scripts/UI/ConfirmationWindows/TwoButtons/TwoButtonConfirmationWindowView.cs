using UnityEngine;
using UnityEngine.UI;

public class TwoButtonConfirmationWindowView : ConfirmationWindowViewBase
{
    [SerializeField] private Button submitButton;
    [SerializeField] private Button cancelButton;

    protected override void Awake()
    {
        defaultSelection = submitButton;
        base.Awake();
    }

    public override void Initialize(params object[] parameters)
    {
        RequireDependency(submitButton, nameof(submitButton));
        RequireDependency(cancelButton, nameof(cancelButton));

        base.Initialize(parameters);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        submitButton.onClick.AddListener(controller.Submit);
        cancelButton.onClick.AddListener(controller.Cancel);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        submitButton.onClick.RemoveListener(controller.Submit);
        cancelButton.onClick.RemoveListener(controller.Cancel);
    }
}
