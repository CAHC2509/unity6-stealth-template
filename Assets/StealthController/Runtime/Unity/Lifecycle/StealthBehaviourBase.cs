using UnityEngine;

namespace StealthController.Unity
{
    public abstract class StealthBehaviourBase : MonoBehaviour
    {
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public virtual void Initialize(params object[] parameters)
        {
            isInitialized = true;
            AddListeners();
        }

        public virtual void Conclude()
        {
            if (!isInitialized) return;

            isInitialized = false;
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
}
