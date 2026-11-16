namespace StealthController.Core
{
    public class PlayerTakedownState : PlayerStateBase
    {
        public override PlayerPoseState PoseState => PlayerPoseState.Takedown;

        public PlayerTakedownState(PlayerLogic player) : base(player) { }

        public override void Tick(float deltaTime, PlayerInputFrame input, float referenceYaw)
        {
            player.Locomotion.Stop();
            player.Takedown.Tick(deltaTime);

            if (!player.Takedown.IsFinished) return;

            player.Motor.Teleport(player.Takedown.ExitPosition);
            player.Facing.Reset(player.Takedown.ExitYaw);
            player.ChangeState(player.StandingState);
        }
    }
}
