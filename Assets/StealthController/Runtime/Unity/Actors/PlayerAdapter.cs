using StealthController.Core;
using UnityEngine;

namespace StealthController.Unity
{
    public class PlayerAdapter : StealthBehaviourBase
    {
        [Header("Movement")]
        [SerializeField] private PlayerMovementConfig movementConfig = new PlayerMovementConfig();

        [Header("Collision")]
        [SerializeField] private LayerMask environmentMask = StealthUnityConstants.DEFAULT_ENVIRONMENT_MASK;
        [SerializeField] private CapsuleCollider bodyCollider;
        [SerializeField] private CapsuleCollider collisionProbe;

        [Header("Presentation")]
        [SerializeField] private PlayerAnimationAdapter animationAdapter;

        [Header("Debug")]
        [SerializeField] private bool drawDebugGizmos;

        private PlayerLogic playerLogic;
        private PlayerInputAdapter inputAdapter;

        public PlayerLogic Logic => playerLogic;
        public PlayerMovementConfig MovementConfig => movementConfig;
        public bool IsReady => playerLogic != null;

        public override void Initialize(params object[] parameters)
        {
            InputActionsAdapter input = parameters.Length > 0 ? parameters[0] as InputActionsAdapter : null;

            bool hasDependencies = RequireDependency(bodyCollider, nameof(bodyCollider))
                & RequireDependency(collisionProbe, nameof(collisionProbe))
                & RequireDependency(animationAdapter, nameof(animationAdapter))
                & RequireService(input, nameof(input));

            if (!hasDependencies) return;

            PlayerInputAdapter playerInput = new PlayerInputAdapter(input);

            if (!playerInput.IsValid)
            {
                Debug.LogError($"{nameof(PlayerAdapter)} on '{name}': the input actions asset is missing a Player action.", this);
                return;
            }

            UnityCollisionWorldAdapter collisionWorld = new UnityCollisionWorldAdapter(environmentMask, collisionProbe);
            playerLogic = new PlayerLogic(movementConfig, collisionWorld);
            inputAdapter = playerInput;

            base.Initialize(parameters);

            playerLogic.Teleport(transform.position, transform.eulerAngles.y);
            SnapPose();
        }

        public override void Conclude()
        {
            base.Conclude();

            playerLogic = null;
        }

        protected override void AddListeners()
        {
            base.AddListeners();

            inputAdapter.AddListeners();
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();

            if (inputAdapter == null) return;

            inputAdapter.RemoveListeners();
        }

        public PlayerInputFrame ConsumeInput()
        {
            return inputAdapter.ConsumeFrame();
        }

        public void ApplyPose(float deltaTime)
        {
            if (!IsReady) return;

            PlayerPose pose = playerLogic.Pose;
            ApplyBody(pose);
            animationAdapter.Apply(pose, playerLogic.Locomotion.HorizontalVelocity, playerLogic.Locomotion.MeasuredSpeed, movementConfig.WallShuffleStandingSpeed, deltaTime);
        }

        public void SnapPose()
        {
            if (!IsReady) return;

            PlayerPose pose = playerLogic.Pose;
            ApplyBody(pose);
            animationAdapter.Snap(pose);
        }

        private void ApplyBody(PlayerPose pose)
        {
            transform.SetPositionAndRotation(pose.Position, pose.Rotation);

            bodyCollider.radius = pose.CapsuleRadius;
            bodyCollider.height = pose.CapsuleHeight;
            bodyCollider.center = Vector3.up * (pose.CapsuleHeight * 0.5f);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawDebugGizmos || playerLogic == null) return;

            CapsuleMotorComponent motor = playerLogic.Motor;

            Gizmos.color = motor.IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(motor.BottomSphereCenter, motor.Radius);
            Gizmos.DrawWireSphere(motor.TopSphereCenter, motor.Radius);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(motor.Position, motor.Position + playerLogic.Locomotion.HorizontalVelocity);

            if (playerLogic.WallCover.Normal == Vector3.zero) return;

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(motor.TopSphereCenter, motor.TopSphereCenter + playerLogic.WallCover.Normal);
        }
#endif
    }
}
