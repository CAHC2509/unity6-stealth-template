using UnityEngine;

namespace StealthController.Core
{
    public class WallCoverComponent
    {
        private readonly PlayerMovementConfig config;
        private readonly CapsuleMotorComponent motor;
        private readonly LocomotionComponent locomotion;
        private readonly MoveInputComponent moveInput;
        private readonly WallSensorLogic sensor;

        private Vector3 wallNormal;
        private float pushElapsed;

        public WallSensorLogic Sensor => sensor;
        public Vector3 Normal => wallNormal;

        public WallCoverComponent(PlayerMovementConfig config, ICollisionWorld collisionWorld, CapsuleMotorComponent motor, LocomotionComponent locomotion, MoveInputComponent moveInput)
        {
            this.config = config;
            this.motor = motor;
            this.locomotion = locomotion;
            this.moveInput = moveInput;

            sensor = new WallSensorLogic(collisionWorld, config);
        }

        public float ProbeHeightFor(float minWallHeight) => minWallHeight * StealthConstants.WALL_PROBE_HEIGHT_RATIO;

        public bool UpdatePush(PlayerInputFrame input, float referenceYaw, float minWallHeight, float deltaTime, out WallContact contact)
        {
            contact = default;

            Vector3 direction = moveInput.ComputeWorldDirection(input.Move, referenceYaw);
            bool isPushingWall = moveInput.HasMoveInput(input)
                && sensor.TryDetectWall(motor.Position, motor.Radius, direction, ProbeHeightFor(minWallHeight), out contact)
                && sensor.IsPushingInto(direction, contact)
                && sensor.ReachesHeight(motor.Position, motor.Radius, contact.Normal, minWallHeight);

            if (!isPushingWall)
            {
                pushElapsed = 0f;
                return false;
            }

            pushElapsed += deltaTime;
            return pushElapsed >= config.WallPushDuration;
        }

        public void ResetPush()
        {
            pushElapsed = 0f;
        }

        public void Attach(WallContact contact)
        {
            locomotion.Stop();
            UpdateContact(contact);
        }

        public void UpdateContact(WallContact contact)
        {
            wallNormal = contact.Normal;
            locomotion.AddDisplacement(-contact.Normal * Mathf.Max(contact.Distance - motor.Radius, 0f));
        }

        public void SetNormal(Vector3 normal)
        {
            wallNormal = normal;
        }

        public void Detach()
        {
            wallNormal = Vector3.zero;
        }
    }
}
