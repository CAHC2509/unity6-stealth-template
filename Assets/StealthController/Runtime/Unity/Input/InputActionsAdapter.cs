using UnityEngine;
using UnityEngine.InputSystem;

namespace StealthController.Unity
{
    public class InputActionsAdapter
    {
        private readonly InputActionAsset actions;
        private readonly InputActionMap playerMap;
        private readonly InputActionMap uiMap;

        public bool IsValid => playerMap != null && uiMap != null;

        public InputActionsAdapter(InputActionAsset source)
        {
            actions = Object.Instantiate(source);
            playerMap = actions.FindActionMap(StealthInputMap.Player.ToString());
            uiMap = actions.FindActionMap(StealthInputMap.UI.ToString());
        }

        public InputAction Find(StealthInputAction action)
        {
            InputActionMap map = action == StealthInputAction.Cancel ? uiMap : playerMap;
            return map?.FindAction(action.ToString());
        }

        public void SwitchTo(StealthInputMap map)
        {
            if (!IsValid) return;

            if (map == StealthInputMap.Player)
            {
                uiMap.Disable();
                playerMap.Enable();
                return;
            }

            playerMap.Disable();
            uiMap.Enable();
        }

        public void DisableAll()
        {
            if (!IsValid) return;

            playerMap.Disable();
            uiMap.Disable();
        }

        public void Dispose()
        {
            actions.Disable();
            Object.Destroy(actions);
        }
    }
}
