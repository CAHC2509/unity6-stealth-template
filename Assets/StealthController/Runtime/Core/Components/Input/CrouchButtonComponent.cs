namespace StealthController.Core
{
    public class CrouchButtonComponent
    {
        private readonly PlayerMovementConfig config;

        private float heldElapsed;
        private bool isTracking;
        private bool holdTriggered;
        private bool tapConsumed;
        private bool holdConsumed;

        public bool WasPressed { get; private set; }
        public bool WasTapped { get; private set; }
        public bool WasHeld { get; private set; }

        public CrouchButtonComponent(PlayerMovementConfig config)
        {
            this.config = config;
        }

        public void Update(PlayerInputFrame input, float deltaTime)
        {
            WasPressed = input.CrouchPressed;
            WasTapped = false;
            WasHeld = false;

            if (input.CrouchPressed)
                BeginTracking();

            if (!isTracking) return;

            if (input.CrouchReleased)
            {
                WasTapped = !holdTriggered && !tapConsumed;
                isTracking = false;
                return;
            }

            if (!input.CrouchHeld)
            {
                isTracking = false;
                return;
            }

            heldElapsed += deltaTime;

            if (holdTriggered || holdConsumed || heldElapsed < config.CrouchHoldToProneDuration) return;

            holdTriggered = true;
            WasHeld = true;
        }

        public void ConsumeTap()
        {
            tapConsumed = true;
        }

        public void ConsumePress()
        {
            tapConsumed = true;
            holdConsumed = true;
        }

        public void Reset()
        {
            isTracking = false;
            WasPressed = false;
            WasTapped = false;
            WasHeld = false;
        }

        private void BeginTracking()
        {
            isTracking = true;
            heldElapsed = 0f;
            holdTriggered = false;
            tapConsumed = false;
            holdConsumed = false;
        }
    }
}
