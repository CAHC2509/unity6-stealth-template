using System.Text;
using TMPro;
using UnityEngine;

public class DevToolsOverlayView : UIViewBase
{
    [Header("Transition settings")]
    [SerializeField] private UIFadeTransitionData fadeTransitionData;

    [Header("Texts")]
    [SerializeField] private TMP_Text performanceText;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private TMP_Text inputText;

    protected override bool RequiresSelection => false;

    protected override UITransitionBase CreateTransition() => new UIFadeTransition(fadeTransitionData);

    public override void Initialize(params object[] parameters)
    {
        bool hasDependencies = RequireDependency(performanceText, nameof(performanceText))
            & RequireDependency(stateText, nameof(stateText))
            & RequireDependency(inputText, nameof(inputText));

        if (!hasDependencies) return;

        base.Initialize(parameters);
    }

    protected override void OnViewOpened()
    {
        base.OnViewOpened();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void SetPerformanceText(StringBuilder text) => performanceText.SetText(text);

    public void SetStateText(StringBuilder text) => stateText.SetText(text);

    public void SetInputText(StringBuilder text) => inputText.SetText(text);

    public void SetInputVisible(bool visible) => inputText.gameObject.SetActive(visible);

    public void SetCaptureHidden(bool hidden)
    {
        if (!IsVisible) return;

        canvasGroup.alpha = hidden ? 0f : 1f;
    }
}
