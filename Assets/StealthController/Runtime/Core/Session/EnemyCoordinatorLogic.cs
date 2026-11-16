using System.Collections.Generic;
using UnityEngine;

namespace StealthController.Core
{
    public class EnemyCoordinatorLogic
    {
        private readonly List<GuardLogic> guards = new List<GuardLogic>();
        private readonly List<SurveillanceCameraLogic> surveillanceCameras = new List<SurveillanceCameraLogic>();
        private readonly float alertRadius;

        private bool hasCapturedPlayer;
        private bool isPlayerDetectable = true;

        public IReadOnlyList<GuardLogic> Guards => guards;
        public IReadOnlyList<SurveillanceCameraLogic> SurveillanceCameras => surveillanceCameras;
        public bool HasCapturedPlayer => hasCapturedPlayer;
        public bool IsPlayerDetectable => isPlayerDetectable;

        public EnemyCoordinatorLogic(float alertRadius)
        {
            this.alertRadius = alertRadius;
        }

        public void AddGuard(GuardLogic guard)
        {
            guards.Add(guard);
            guard.Vision.SetIgnoresPlayer(!isPlayerDetectable);
        }

        public void AddSurveillanceCamera(SurveillanceCameraLogic surveillanceCamera)
        {
            surveillanceCameras.Add(surveillanceCamera);
            surveillanceCamera.Vision.SetIgnoresPlayer(!isPlayerDetectable);
        }

        public void Tick(float deltaTime, PlayerPose player)
        {
            for (int i = 0; i < guards.Count; i++)
                guards[i].Tick(deltaTime, player);

            for (int i = 0; i < surveillanceCameras.Count; i++)
            {
                SurveillanceCameraLogic surveillanceCamera = surveillanceCameras[i];
                surveillanceCamera.Tick(deltaTime, player);

                if (surveillanceCamera.TryConsumeAlert(out Vector3 knownPosition))
                    AlertGuardsNear(surveillanceCamera.EyePosition, knownPosition);
            }

            hasCapturedPlayer = false;

            for (int i = 0; i < guards.Count; i++)
                hasCapturedPlayer |= guards[i].HasCapturedPlayer;
        }

        public void SetPlayerDetectable(bool detectable)
        {
            isPlayerDetectable = detectable;

            for (int i = 0; i < guards.Count; i++)
            {
                GuardLogic guard = guards[i];
                guard.Vision.SetIgnoresPlayer(!detectable);

                if (!detectable && !guard.IsTakenDown)
                    guard.ReturnToPatrol();
            }

            for (int i = 0; i < surveillanceCameras.Count; i++)
                surveillanceCameras[i].Vision.SetIgnoresPlayer(!detectable);
        }

        public void ResetAll()
        {
            hasCapturedPlayer = false;

            for (int i = 0; i < guards.Count; i++)
                guards[i].Reset();

            for (int i = 0; i < surveillanceCameras.Count; i++)
                surveillanceCameras[i].Reset();
        }

        private void AlertGuardsNear(Vector3 alertOrigin, Vector3 knownPosition)
        {
            for (int i = 0; i < guards.Count; i++)
            {
                GuardLogic guard = guards[i];

                if (guard.PathFollower.HorizontalDistanceTo(alertOrigin) <= alertRadius)
                    guard.Pursue(knownPosition);
            }
        }
    }
}
