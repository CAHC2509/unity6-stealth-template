using UnityEngine;

namespace StealthController.Core
{
    public class PlayerLogic
    {
        private readonly PlayerMovementConfig config;
        private readonly CapsuleMotorComponent motor;
        private readonly PostureComponent posture;
        private readonly LocomotionComponent locomotion;
        private readonly FacingComponent facing;
        private readonly MoveInputComponent moveInput;
        private readonly CrouchButtonComponent crouchButton;
        private readonly WallCoverComponent wallCover;
        private readonly TakedownAttackerComponent takedown;
        private readonly PlayerStandingState standingState;
        private readonly PlayerCrouchedState crouchedState;
        private readonly PlayerCrawlingState crawlingState;
        private readonly PlayerWallStandingState wallStandingState;
        private readonly PlayerTakedownState takedownState;

        private PlayerStateBase currentState;

        public PlayerMovementConfig Config => config;
        public CapsuleMotorComponent Motor => motor;
        public PostureComponent Posture => posture;
        public LocomotionComponent Locomotion => locomotion;
        public FacingComponent Facing => facing;
        public MoveInputComponent MoveInput => moveInput;
        public CrouchButtonComponent CrouchButton => crouchButton;
        public WallCoverComponent WallCover => wallCover;
        public TakedownAttackerComponent Takedown => takedown;
        public PlayerStandingState StandingState => standingState;
        public PlayerCrouchedState CrouchedState => crouchedState;
        public PlayerCrawlingState CrawlingState => crawlingState;
        public PlayerWallStandingState WallStandingState => wallStandingState;
        public PlayerTakedownState TakedownState => takedownState;

        public PlayerPoseState PoseState => currentState.PoseState;

        public PlayerPose Pose => new PlayerPose(
            motor.Position,
            facing.Rotation,
            PoseState,
            config.RunSpeed > 0f ? locomotion.MeasuredSpeed / config.RunSpeed : 0f,
            posture.TransitionProgress,
            motor.Radius,
            motor.Height,
            wallCover.Normal,
            PoseState == PlayerPoseState.Takedown ? takedown.Progress : 0f);

        public PlayerLogic(PlayerMovementConfig config, ICollisionWorld collisionWorld)
        {
            this.config = config;

            motor = new CapsuleMotorComponent(collisionWorld, config);
            posture = new PostureComponent(motor);
            locomotion = new LocomotionComponent(motor, config.Acceleration, config.Deceleration);
            facing = new FacingComponent();
            moveInput = new MoveInputComponent(config);
            crouchButton = new CrouchButtonComponent(config);
            wallCover = new WallCoverComponent(config, collisionWorld, motor, locomotion, moveInput);
            takedown = new TakedownAttackerComponent();

            standingState = new PlayerStandingState(this);
            crouchedState = new PlayerCrouchedState(this);
            crawlingState = new PlayerCrawlingState(this);
            wallStandingState = new PlayerWallStandingState(this);
            takedownState = new PlayerTakedownState(this);

            currentState = standingState;
        }

        public void Teleport(Vector3 position, float yaw)
        {
            motor.SetShape(config.StandingRadius, config.StandingHeight);
            motor.Teleport(position);

            facing.Reset(yaw);
            locomotion.Reset();
            posture.Reset();
            moveInput.Reset();
            crouchButton.Reset();
            wallCover.Detach();
            takedown.Reset();
            ChangeState(standingState);
        }

        public void BeginTakedown(Vector3 startPosition, float yaw, float startTime, float clipLength, float endTime, Vector3 exitPosition, float exitYaw)
        {
            Teleport(startPosition, yaw);
            takedown.Begin(startTime, clipLength, endTime, exitPosition, exitYaw);
            ChangeState(takedownState);
        }

        public void Tick(float deltaTime, PlayerInputFrame input, float referenceYaw)
        {
            if (deltaTime <= 0f) return;

            posture.Tick(deltaTime);
            crouchButton.Update(input, deltaTime);
            moveInput.Update(input);

            currentState.Tick(deltaTime, input, referenceYaw);

            locomotion.Apply(deltaTime);
        }

        public void ChangeState(PlayerStateBase nextState)
        {
            currentState.Exit();
            currentState = nextState;
            wallCover.ResetPush();
            currentState.Enter();
        }
    }
}
