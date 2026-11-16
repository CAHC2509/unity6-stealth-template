using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DevToolsPanelView : CancelableUIViewBase
{
    [Header("Transition settings")]
    [SerializeField] private UIFadeTransitionData fadeTransitionData;

    [Header("Tabs")]
    [SerializeField, Tooltip("One button per DevToolsTab, in the same order as the enum.")] private Button[] tabButtons = Array.Empty<Button>();
    [SerializeField] private Graphic[] tabHighlights = Array.Empty<Graphic>();
    [SerializeField] private Button closeButton;

    private DevToolsPanelController panelController;
    private UnityAction[] tabClickHandlers;

    protected override bool RequiresSelection => false;

    protected override UITransitionBase CreateTransition() => new UIFadeTransition(fadeTransitionData);

    protected override void Awake()
    {
        if (tabButtons.Length > 0)
            defaultSelection = tabButtons[0];

        base.Awake();
    }

    public override void Initialize(params object[] parameters)
    {
        panelController = parameters[0] as DevToolsPanelController;

        bool hasDependencies = RequireDependency(panelController, nameof(panelController))
            & RequireDependency(closeButton, nameof(closeButton));

        int tabCount = Enum.GetValues(typeof(DevToolsTab)).Length;

        if (tabButtons.Length != tabCount || tabHighlights.Length != tabCount)
        {
            Debug.LogError($"{nameof(DevToolsPanelView)} on '{name}': needs {tabCount} tab buttons and highlights, one per {nameof(DevToolsTab)}.", this);
            hasDependencies = false;
        }

        if (!hasDependencies) return;

        tabClickHandlers = new UnityAction[tabCount];

        for (int i = 0; i < tabCount; i++)
        {
            DevToolsTab tab = (DevToolsTab)i;
            tabClickHandlers[i] = () => panelController.OnTabClicked(tab);
        }

        base.Initialize(parameters);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        for (int i = 0; i < tabButtons.Length; i++)
            tabButtons[i].onClick.AddListener(tabClickHandlers[i]);

        closeButton.onClick.AddListener(panelController.OnCloseClicked);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        for (int i = 0; i < tabButtons.Length; i++)
            tabButtons[i].onClick.RemoveListener(tabClickHandlers[i]);

        closeButton.onClick.RemoveListener(panelController.OnCloseClicked);
    }

    public void HighlightTab(DevToolsTab tab)
    {
        for (int i = 0; i < tabHighlights.Length; i++)
            tabHighlights[i].enabled = i == (int)tab;
    }

    public void SelectTabButton(DevToolsTab tab)
    {
        currentSelection = tabButtons[(int)tab];
        SelectCurrentItem();
    }

    public void SetCaptureHidden(bool hidden)
    {
        if (!IsVisible) return;

        canvasGroup.alpha = hidden ? 0f : 1f;
    }

    protected override void OnCancelRequested()
    {
        panelController.OnCloseClicked();
    }
}
