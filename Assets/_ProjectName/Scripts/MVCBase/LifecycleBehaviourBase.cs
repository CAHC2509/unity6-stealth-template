using UnityEngine;

public abstract class LifecycleBehaviourBase : MonoBehaviour
{
    public virtual void Initialize(params object[] parameters)
    {
        AddListeners();
    }

    public virtual void Conclude()
    {
        RemoveListeners();
    }

    protected virtual void AddListeners() { }
    protected virtual void RemoveListeners() { }

    protected bool RequireDependency(Object dependency, string dependencyName)
    {
        if (dependency != null) return true;

        Debug.LogError($"{GetType().Name} on '{name}': required dependency '{dependencyName}' is missing.", this);
        return false;
    }

    protected bool RequireService(object service, string serviceName)
    {
        if (service != null) return true;

        Debug.LogError($"{GetType().Name} on '{name}': required service '{serviceName}' was not provided to Initialize.", this);
        return false;
    }
}
