using UnityEngine;
using UnityEngine.UI;

public class SingleButtonConfirmationWindowView : ConfirmationWindowViewBase
{
    [SerializeField] private Button submitButton;

    protected override void Awake()
    {
        defaultSelection = submitButton;
        base.Awake();
    }

    public override void Initialize(params object[] parameters)
    {
        RequireDependency(submitButton, nameof(submitButton));

        base.Initialize(parameters);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        submitButton.onClick.AddListener(controller.Submit);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        submitButton.onClick.RemoveListener(controller.Submit);
    }
}
