using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public abstract class DevToolsTabViewBase : UIViewBase
{
    [Header("Transition settings")]
    [SerializeField] private UIFadeTransitionData fadeTransitionData;

    [Header("Selection")]
    [SerializeField] private Selectable firstSelectable;

    protected override UITransitionBase CreateTransition() => new UIFadeTransition(fadeTransitionData);

    protected override void Awake()
    {
        defaultSelection = firstSelectable;

        base.Awake();
    }

    public override void Initialize(params object[] parameters)
    {
        RequireDependency(firstSelectable, nameof(firstSelectable));

        base.Initialize(parameters);
    }

    protected void ShowMessage(LocalizeStringEvent target, LocalizedString message, params object[] arguments)
    {
        if (target == null || message == null) return;

        message.Arguments = arguments;
        target.StringReference = message;
        target.RefreshString();
    }

    protected void ClearMessage(LocalizeStringEvent target)
    {
        if (target == null) return;

        target.StringReference.Clear();
        target.OnUpdateString.Invoke(string.Empty);
    }
}
