namespace StealthController.Core
{
    public class PlayerWallStandingState : PlayerWallStateBase
    {
        public override PlayerPoseState PoseState => PlayerPoseState.WallStanding;

        protected override float MinWallHeight => Config.StandingWallMinHeight;
        protected override float ShuffleSpeed => Config.WallShuffleStandingSpeed;
        protected override PlayerStateBase DetachedState => player.StandingState;

        public PlayerWallStandingState(PlayerLogic player) : base(player) { }

        protected override bool TryHandlePostureInput(PlayerInputFrame input)
        {
            if (!player.CrouchButton.WasPressed) return false;
            if (!player.Posture.TryChange(Config.StandingRadius, Config.CrouchedHeight, Config.PostureTransitionDuration)) return true;

            player.CrouchButton.ConsumePress();
            player.WallCover.Detach();
            player.ChangeState(player.CrouchedState);
            return true;
        }
    }
}
