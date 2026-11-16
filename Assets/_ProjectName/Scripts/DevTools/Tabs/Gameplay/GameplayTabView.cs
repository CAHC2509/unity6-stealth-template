using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class GameplayTabView : DevToolsTabViewBase
{
    [Header("Commands")]
    [SerializeField] private RectTransform commandsRoot;
    [SerializeField] private Button commandButtonPrefab;
    [SerializeField] private GameObject commandToggleItemPrefab;
    [SerializeField] private GameObject emptyMessage;
    [SerializeField] private Button refreshButton;

    private readonly List<Button> commandButtons = new();
    private readonly List<Toggle> commandToggles = new();
    private readonly List<GameObject> commandToggleItems = new();
    private readonly List<UnityAction> buttonHandlers = new();
    private readonly List<UnityAction<bool>> toggleHandlers = new();
    private readonly List<int> toggleCommandIndices = new();

    private GameplayTabController gameplayTabController;
    private int usedButtons;
    private int usedToggles;

    public override void Initialize(params object[] parameters)
    {
        gameplayTabController = parameters[0] as GameplayTabController;

        bool hasDependencies = RequireDependency(gameplayTabController, nameof(gameplayTabController))
            & RequireDependency(commandsRoot, nameof(commandsRoot))
            & RequireDependency(commandButtonPrefab, nameof(commandButtonPrefab))
            & RequireDependency(commandToggleItemPrefab, nameof(commandToggleItemPrefab))
            & RequireDependency(emptyMessage, nameof(emptyMessage))
            & RequireDependency(refreshButton, nameof(refreshButton));

        if (!hasDependencies) return;

        base.Initialize(parameters);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        refreshButton.onClick.AddListener(gameplayTabController.OnRefreshClicked);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        refreshButton.onClick.RemoveListener(gameplayTabController.OnRefreshClicked);

        for (int i = 0; i < commandButtons.Count; i++)
            commandButtons[i].onClick.RemoveListener(buttonHandlers[i]);

        for (int i = 0; i < commandToggles.Count; i++)
            commandToggles[i].onValueChanged.RemoveListener(toggleHandlers[i]);
    }

    public void ShowCommands(IReadOnlyList<DevToolsCommandData> commands)
    {
        usedButtons = 0;
        usedToggles = 0;
        toggleCommandIndices.Clear();

        Selectable firstItem = null;

        for (int i = 0; i < commands.Count; i++)
        {
            DevToolsCommandData command = commands[i];
            Selectable item = command.IsToggle ? ShowToggle(command, i) : ShowButton(command, i);

            if (firstItem == null)
                firstItem = item;
        }

        for (int i = usedButtons; i < commandButtons.Count; i++)
            commandButtons[i].gameObject.SetActive(false);

        for (int i = usedToggles; i < commandToggleItems.Count; i++)
            commandToggleItems[i].SetActive(false);

        emptyMessage.SetActive(commands.Count == 0);

        defaultSelection = firstItem != null ? firstItem : refreshButton;
        currentSelection = defaultSelection;
    }

    public void RefreshToggles(IReadOnlyList<DevToolsCommandData> commands)
    {
        for (int i = 0; i < usedToggles; i++)
        {
            int commandIndex = toggleCommandIndices[i];

            if (commandIndex < commands.Count && commands[commandIndex].IsToggle)
                commandToggles[i].SetIsOnWithoutNotify(commands[commandIndex].ReadToggle());
        }
    }

    private Selectable ShowButton(DevToolsCommandData command, int commandIndex)
    {
        if (usedButtons == commandButtons.Count)
            CreateButton();

        Button button = commandButtons[usedButtons];
        button.onClick.RemoveListener(buttonHandlers[usedButtons]);

        int capturedIndex = commandIndex;
        buttonHandlers[usedButtons] = () => gameplayTabController.OnCommandClicked(capturedIndex);
        button.onClick.AddListener(buttonHandlers[usedButtons]);

        SetLabel(button.gameObject, command);
        button.gameObject.SetActive(true);
        button.transform.SetSiblingIndex(commandIndex);
        usedButtons++;

        return button;
    }

    private Selectable ShowToggle(DevToolsCommandData command, int commandIndex)
    {
        if (usedToggles == commandToggles.Count)
            CreateToggle();

        if (usedToggles >= commandToggles.Count) return refreshButton;

        Toggle toggle = commandToggles[usedToggles];
        toggle.onValueChanged.RemoveListener(toggleHandlers[usedToggles]);
        toggle.SetIsOnWithoutNotify(command.ReadToggle());

        int capturedIndex = commandIndex;
        toggleHandlers[usedToggles] = _ => gameplayTabController.OnCommandClicked(capturedIndex);
        toggle.onValueChanged.AddListener(toggleHandlers[usedToggles]);

        GameObject toggleItem = commandToggleItems[usedToggles];

        toggleCommandIndices.Add(commandIndex);
        SetLabel(toggleItem, command);
        toggleItem.SetActive(true);
        toggleItem.transform.SetSiblingIndex(commandIndex);
        usedToggles++;

        return toggle;
    }

    private void CreateButton()
    {
        Button button = Instantiate(commandButtonPrefab, commandsRoot);
        commandButtons.Add(button);
        buttonHandlers.Add(NoOperation);
    }

    private void CreateToggle()
    {
        GameObject toggleItem = Instantiate(commandToggleItemPrefab, commandsRoot);
        Toggle toggle = toggleItem.GetComponentInChildren<Toggle>(true);

        if (toggle == null)
        {
            Debug.LogError($"{nameof(GameplayTabView)} on '{name}': '{commandToggleItemPrefab.name}' has no Toggle.", this);
            Destroy(toggleItem);
            return;
        }

        commandToggleItems.Add(toggleItem);
        commandToggles.Add(toggle);
        toggleHandlers.Add(NoOperation);
    }

    private static void SetLabel(GameObject item, DevToolsCommandData command)
    {
        LocalizeStringEvent label = item.GetComponentInChildren<LocalizeStringEvent>(true);
        if (label == null) return;

        LocalizedString labelString = new LocalizedString(Constants.DEVTOOLS_TABLE, command.LabelKey);
        labelString.Arguments = command.LabelArguments;

        label.StringReference = labelString;
        label.RefreshString();
    }

    private static void NoOperation() { }

    private static void NoOperation(bool value) { }
}
