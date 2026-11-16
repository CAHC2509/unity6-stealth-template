namespace StealthController.Core
{
    public class GuardTakenDownState : GuardStateBase
    {
        public override GuardAlertState AlertState => GuardAlertState.TakenDown;

        public GuardTakenDownState(GuardLogic guard) : base(guard) { }

        public override void Enter()
        {
            guard.PathFollower.Halt();
            guard.Vision.Reset();
        }

        public override void Tick(float deltaTime, PlayerPose player)
        {
            guard.TakedownVictim.Tick(deltaTime);
        }
    }
}
