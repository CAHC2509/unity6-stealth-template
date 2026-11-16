namespace StealthController.Core
{
    public abstract class GuardStateBase
    {
        protected readonly GuardLogic guard;

        protected GuardConfig Config => guard.Config;

        public abstract GuardAlertState AlertState { get; }

        protected GuardStateBase(GuardLogic guard)
        {
            this.guard = guard;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }

        public abstract void Tick(float deltaTime, PlayerPose player);
    }
}
