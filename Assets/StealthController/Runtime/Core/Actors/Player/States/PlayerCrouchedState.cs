using UnityEngine;

namespace StealthController.Core
{
    public class PlayerCrouchedState : PlayerStateBase
    {
        public override PlayerPoseState PoseState => PlayerPoseState.Crouched;

        public PlayerCrouchedState(PlayerLogic player) : base(player) { }

        public override void Tick(float deltaTime, PlayerInputFrame input, float referenceYaw)
        {
            if (player.Posture.HasQueuedStandUp)
            {
                if (player.CrouchButton.WasPressed)
                    player.Posture.ClearQueuedStandUp();
                else if (TryCompleteQueuedStandUp())
                    return;
            }

            if (player.MoveInput.WantsToStandUp(input) && player.Posture.TryChange(Config.StandingRadius, Config.StandingHeight, Config.PostureTransitionDuration))
            {
                player.CrouchButton.ConsumePress();
                player.ChangeState(player.StandingState);
                return;
            }

            if (player.CrouchButton.WasTapped)
            {
                if (player.Posture.TryChange(Config.StandingRadius, Config.StandingHeight, Config.PostureTransitionDuration))
                    player.ChangeState(player.StandingState);

                return;
            }

            if (player.CrouchButton.WasHeld)
            {
                if (player.Posture.TryChange(Config.CrawlingRadius, Config.CrawlingHeight, Config.ProneTransitionDuration))
                {
                    player.Locomotion.Stop();
                    player.ChangeState(player.CrawlingState);
                }

                return;
            }

            Vector3 direction = player.MoveInput.ComputeWorldDirection(input.Move, referenceYaw);
            float targetSpeed = player.MoveInput.ComputeCrouchedTargetSpeed(input) * player.Posture.MovementScale;

            player.Facing.RotateTowards(direction, Config.StandingTurnSmoothTime, Config.StandingTurnSpeed, deltaTime);
            player.Locomotion.AccelerateTowards(direction * targetSpeed, deltaTime);
        }

        public override void Exit()
        {
            player.Posture.ClearQueuedStandUp();
        }

        private bool TryCompleteQueuedStandUp()
        {
            if (player.Posture.IsInTransition) return false;

            player.Posture.ClearQueuedStandUp();
            if (!player.Posture.TryChange(Config.StandingRadius, Config.StandingHeight, Config.PostureTransitionDuration)) return false;

            player.ChangeState(player.StandingState);
            return true;
        }
    }
}
