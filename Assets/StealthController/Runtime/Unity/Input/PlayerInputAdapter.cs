using StealthController.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace StealthController.Unity
{
    public class PlayerInputAdapter : IPlayerInputSource
    {
        private readonly InputAction moveAction;
        private readonly InputAction sprintAction;
        private readonly InputAction crouchAction;
        private readonly InputAction standAction;
        private readonly InputAction takedownAction;

        private Vector2 move;
        private bool isMoveAnalog;
        private bool sprintHeld;
        private bool crouchPressed;
        private bool crouchHeld;
        private bool crouchReleased;
        private bool standPressed;
        private bool takedownPressed;

        public bool IsValid => moveAction != null && sprintAction != null && crouchAction != null && standAction != null && takedownAction != null;

        public PlayerInputAdapter(InputActionsAdapter input)
        {
            moveAction = input.Find(StealthInputAction.Move);
            sprintAction = input.Find(StealthInputAction.Sprint);
            crouchAction = input.Find(StealthInputAction.Crouch);
            standAction = input.Find(StealthInputAction.Stand);
            takedownAction = input.Find(StealthInputAction.Takedown);
        }

        public void AddListeners()
        {
            moveAction.performed += OnMoveChanged;
            moveAction.canceled += OnMoveChanged;
            sprintAction.started += OnSprintStarted;
            sprintAction.canceled += OnSprintCanceled;
            crouchAction.started += OnCrouchStarted;
            crouchAction.canceled += OnCrouchCanceled;
            standAction.started += OnStandStarted;
            takedownAction.started += OnTakedownStarted;
        }

        public void RemoveListeners()
        {
            moveAction.performed -= OnMoveChanged;
            moveAction.canceled -= OnMoveChanged;
            sprintAction.started -= OnSprintStarted;
            sprintAction.canceled -= OnSprintCanceled;
            crouchAction.started -= OnCrouchStarted;
            crouchAction.canceled -= OnCrouchCanceled;
            standAction.started -= OnStandStarted;
            takedownAction.started -= OnTakedownStarted;

            ResetState();
        }

        public PlayerInputFrame ConsumeFrame()
        {
            PlayerInputFrame frame = new PlayerInputFrame(move, isMoveAnalog, sprintHeld, crouchPressed, crouchHeld, crouchReleased, standPressed, takedownPressed);
            crouchPressed = false;
            crouchReleased = false;
            standPressed = false;
            takedownPressed = false;
            return frame;
        }

        public void ResetState()
        {
            move = Vector2.zero;
            isMoveAnalog = false;
            sprintHeld = false;
            crouchPressed = false;
            crouchHeld = false;
            crouchReleased = false;
            standPressed = false;
            takedownPressed = false;
        }

        private void OnMoveChanged(InputAction.CallbackContext context)
        {
            move = context.ReadValue<Vector2>();
            isMoveAnalog = context.control is StickControl;
        }

        private void OnSprintStarted(InputAction.CallbackContext context)
        {
            sprintHeld = true;
        }

        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            sprintHeld = false;
        }

        private void OnCrouchStarted(InputAction.CallbackContext context)
        {
            crouchPressed = true;
            crouchHeld = true;
        }

        private void OnCrouchCanceled(InputAction.CallbackContext context)
        {
            crouchHeld = false;
            crouchReleased = true;
        }

        private void OnStandStarted(InputAction.CallbackContext context)
        {
            standPressed = true;
        }

        private void OnTakedownStarted(InputAction.CallbackContext context)
        {
            takedownPressed = true;
        }
    }
}
