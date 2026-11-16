using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;

public class ControlBindingController : UIControllerBase<ControlBindingView>
{
    [Header("Binding")]
    [SerializeField] private InputActionReference actionReference;
    [SerializeField] private LocalizedString actionName;

    [Header("Composite")]
    [SerializeField] private bool useCompositeRoot;
    [SerializeField] private string compositeName;

    [Header("Rebind")]
    [SerializeField] private DeviceType rebindDeviceType = DeviceType.Any;

    [Header("Display")]
    [SerializeField] private InputBinding.DisplayStringOptions displayStringOptions;

    public Button RebindButton => View.RebindButton;
    public LocalizedString ActionLocalizedString => actionName;

    private ControlsSettingsController controlsSettingsController;
    private InputAction runtimeAction;
    private int resolvedBindingIndex = -1;

    public DeviceType RebindDeviceType => rebindDeviceType;

    public override void Initialize(params object[] parameters)
    {
        controlsSettingsController = parameters[0] as ControlsSettingsController;

        RequireDependency(controlsSettingsController, nameof(controlsSettingsController));
        RequireDependency(actionReference, nameof(actionReference));

        base.Initialize(parameters);

        ResolveRuntimeAction();
        RefreshBindingResolution();
        UpdateActionLabel();
        UpdateBindingDisplay();
    }

    public void RequestRebind() => controlsSettingsController.RequestRebind(this);
    public void RequestReset() => controlsSettingsController.RequestReset(this);

    private void ResolveRuntimeAction()
    {
        runtimeAction = null;

        if (actionReference == null || actionReference.action == null) return;

        InputActionAsset asset = controlsSettingsController.ActionAsset;
        if (asset == null)
        {
            Debug.LogError($"[{name}] No InputActionAsset available.", this);
            return;
        }

        InputAction referencedAction = actionReference.action;
        string actionPath = referencedAction.actionMap != null ? $"{referencedAction.actionMap.name}/{referencedAction.name}" : referencedAction.name;

        runtimeAction = asset.FindAction(referencedAction.id) ?? asset.FindAction(actionPath);

        if (runtimeAction == null)
            Debug.LogError($"[{name}] Action '{actionPath}' not found in asset '{asset.name}'.", this);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        if (!actionName.IsEmpty)
            actionName.StringChanged += OnActionNameChanged;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        actionName.StringChanged -= OnActionNameChanged;
    }

    public bool RefreshBindingResolution()
    {
        resolvedBindingIndex = -1;

        if (runtimeAction == null)
        {
            ResolveRuntimeAction();
            if (runtimeAction == null) return false;
        }

        if (useCompositeRoot)
        {
            for (int i = 0; i < runtimeAction.bindings.Count; ++i)
            {
                InputBinding binding = runtimeAction.bindings[i];
                if (!binding.isComposite) continue;
                if (!string.IsNullOrEmpty(compositeName) && binding.name != compositeName) continue;
                resolvedBindingIndex = i;
                return true;
            }
            Debug.LogWarning($"[{name}] Composite '{compositeName}' wasn't found in the action '{runtimeAction.name}'.", this);
            return false;
        }

        resolvedBindingIndex = InputBindingDisplayUtility.FindBindingIndex(runtimeAction, rebindDeviceType);
        return resolvedBindingIndex >= 0;
    }

    public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
    {
        action = runtimeAction;
        bindingIndex = resolvedBindingIndex;

        if (action == null)
        {
            ResolveRuntimeAction();
            action = runtimeAction;
        }

        return action != null && bindingIndex >= 0 && bindingIndex < action.bindings.Count;
    }

    public void UpdateBindingDisplay()
    {
        if (!ResolveActionAndBinding(out InputAction action, out int bindingIndex))
        {
            View.SetBindingText(string.Empty);
            return;
        }

        InputBinding binding = action.bindings[bindingIndex];

        string displayString = binding.isComposite
            ? BuildCompositeDisplayString(action, bindingIndex)
            : action.GetBindingDisplayString(bindingIndex, displayStringOptions);

        View.SetBindingText(displayString);
    }

    public void ResetToDefault()
    {
        if (!ResolveActionAndBinding(out InputAction action, out int bindingIndex)) return;

        InputBinding binding = action.bindings[bindingIndex];

        if (binding.isComposite)
        {
            int partIdx = bindingIndex + 1;
            while (partIdx < action.bindings.Count && action.bindings[partIdx].isPartOfComposite)
            {
                action.RemoveBindingOverride(partIdx);
                partIdx++;
            }
        }
        else
        {
            action.RemoveBindingOverride(bindingIndex);
        }

        UpdateBindingDisplay();
    }

    public void SetInteractable(bool interactable)
    {
        View.SetInteractable(interactable);
    }

    private string BuildCompositeDisplayString(InputAction action, int compositeRootIndex)
    {
        string up = "", down = "", left = "", right = "";
        for (int i = compositeRootIndex + 1; i < action.bindings.Count; ++i)
        {
            InputBinding binding = action.bindings[i];
            if (!binding.isPartOfComposite) break;
            string display = action.GetBindingDisplayString(i, displayStringOptions);
            switch (binding.name.ToLower())
            {
                case "up": up = display; break;
                case "down": down = display; break;
                case "left": left = display; break;
                case "right": right = display; break;
            }
        }
        return $"{up} {left} {down} {right}";
    }

    private void OnActionNameChanged(string localizedActionName)
    {
        View.SetActionLabel(localizedActionName);
    }

    private void UpdateActionLabel()
    {
        if (!actionName.IsEmpty) return;

        Debug.LogWarning($"[{name}] Missing localized action name, using the action name as fallback.", this);
        View.SetActionLabel(runtimeAction != null ? runtimeAction.name : string.Empty);
    }
}