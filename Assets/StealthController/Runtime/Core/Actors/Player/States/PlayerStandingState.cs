using UnityEngine;

namespace StealthController.Core
{
    public class PlayerStandingState : PlayerStateBase
    {
        public override PlayerPoseState PoseState => PlayerPoseState.Standing;

        public PlayerStandingState(PlayerLogic player) : base(player) { }

        public override void Tick(float deltaTime, PlayerInputFrame input, float referenceYaw)
        {
            if (player.CrouchButton.WasPressed && player.Posture.TryChange(Config.StandingRadius, Config.CrouchedHeight, Config.PostureTransitionDuration))
            {
                player.CrouchButton.ConsumeTap();
                player.ChangeState(player.CrouchedState);
                return;
            }

            if (!player.Posture.IsInTransition && player.WallCover.UpdatePush(input, referenceYaw, Config.StandingWallMinHeight, deltaTime, out WallContact contact))
            {
                player.WallCover.Attach(contact);
                player.ChangeState(player.WallStandingState);
                return;
            }

            Vector3 direction = player.MoveInput.ComputeWorldDirection(input.Move, referenceYaw);
            float targetSpeed = player.MoveInput.ComputeStandingTargetSpeed(input) * player.Posture.MovementScale;

            player.Facing.RotateTowards(direction, Config.StandingTurnSmoothTime, Config.StandingTurnSpeed, deltaTime);
            player.Locomotion.AccelerateTowards(direction * targetSpeed, deltaTime);
        }
    }
}
