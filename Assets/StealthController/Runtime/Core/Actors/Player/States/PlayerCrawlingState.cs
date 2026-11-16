using UnityEngine;

namespace StealthController.Core
{
    public class PlayerCrawlingState : PlayerStateBase
    {
        public override PlayerPoseState PoseState => PlayerPoseState.Crawling;

        public PlayerCrawlingState(PlayerLogic player) : base(player) { }

        public override void Tick(float deltaTime, PlayerInputFrame input, float referenceYaw)
        {
            if (TryStandUp(input)) return;

            if (player.Posture.IsInTransition)
            {
                player.Locomotion.Stop();
                return;
            }

            if (player.CrouchButton.WasPressed)
            {
                player.Locomotion.Stop();

                if (player.Posture.TryChange(Config.StandingRadius, Config.CrouchedHeight, Config.ProneTransitionDuration, true))
                {
                    player.CrouchButton.ConsumePress();
                    player.ChangeState(player.CrouchedState);
                }

                return;
            }

            if (!player.MoveInput.HasMoveInput(input))
            {
                player.Locomotion.Stop();
                return;
            }

            Vector3 direction = player.MoveInput.ComputeWorldDirection(input.Move, referenceYaw);
            player.Facing.RotateTowards(direction, Config.CrawlingTurnSmoothTime, Config.CrawlingTurnSpeed, deltaTime);
            player.Locomotion.SetHorizontalVelocity(player.Facing.Forward * Config.CrawlSpeed);
        }

        private bool TryStandUp(PlayerInputFrame input)
        {
            if (!player.MoveInput.WantsToStandUp(input)) return false;
            if (!player.Posture.TryChange(Config.StandingRadius, Config.CrouchedHeight, Config.ProneTransitionDuration, true)) return false;

            player.Locomotion.Stop();
            player.CrouchButton.ConsumePress();
            player.Posture.QueueStandUp();
            player.ChangeState(player.CrouchedState);
            return true;
        }
    }
}
