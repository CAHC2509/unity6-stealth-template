namespace StealthController.Core
{
    public abstract class PlayerStateBase
    {
        protected readonly PlayerLogic player;

        protected PlayerMovementConfig Config => player.Config;

        public abstract PlayerPoseState PoseState { get; }

        protected PlayerStateBase(PlayerLogic player)
        {
            this.player = player;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }

        public abstract void Tick(float deltaTime, PlayerInputFrame input, float referenceYaw);
    }
}
