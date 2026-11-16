namespace StealthController.Core
{
    public class PlayerCaptureComponent
    {
        private readonly float captureDistance;

        private bool hasCapturedPlayer;

        public bool HasCapturedPlayer => hasCapturedPlayer;

        public PlayerCaptureComponent(float captureDistance)
        {
            this.captureDistance = captureDistance;
        }

        public void Reset()
        {
            hasCapturedPlayer = false;
        }

        public bool TryCapture(float horizontalDistanceToPlayer)
        {
            if (horizontalDistanceToPlayer > captureDistance) return false;

            hasCapturedPlayer = true;
            return true;
        }
    }
}
