using System.Collections.Generic;
using System;
using StealthController.Core;
using UnityEngine;

namespace StealthController.Unity
{
    public class GuardAdapter : StealthBehaviourBase
    {
        [Header("Behaviour")]
        [SerializeField] private GuardConfig guardConfig = new GuardConfig();

        [Header("Route")]
        [SerializeField, BasicSetting, Tooltip("Points the guard walks through, in order. Each point can make the guard wait and look in a direction.")] private GuardWaypointAdapter[] route = Array.Empty<GuardWaypointAdapter>();

        [Header("Presentation")]
        [SerializeField] private GuardAnimationAdapter animationAdapter;
        [SerializeField] private VisionConeAdapter visionCone;
        [SerializeField] private Color patrolConeColor = Color.green;
        [SerializeField] private Color chaseConeColor = Color.red;

        private readonly List<GuardWaypoint> waypoints = new List<GuardWaypoint>();

        private GuardLogic guardLogic;

        public GuardLogic Logic => guardLogic;
        public bool IsReady => guardLogic != null;

        public override void Initialize(params object[] parameters)
        {
            ICollisionWorld collisionWorld = parameters.Length > 0 ? parameters[0] as ICollisionWorld : null;
            IPathfinder pathfinder = parameters.Length > 1 ? parameters[1] as IPathfinder : null;

            bool hasDependencies = RequireDependency(animationAdapter, nameof(animationAdapter))
                & RequireDependency(visionCone, nameof(visionCone))
                & RequireService(collisionWorld, nameof(collisionWorld))
                & RequireService(pathfinder, nameof(pathfinder))
                & HasValidRoute();

            if (!hasDependencies) return;

            waypoints.Clear();

            foreach (GuardWaypointAdapter waypoint in route)
                waypoints.Add(waypoint.ToWaypoint());

            guardLogic = new GuardLogic(guardConfig, waypoints, transform.position, transform.eulerAngles.y, collisionWorld, pathfinder);

            base.Initialize(parameters);

            SnapPose();
        }

        public override void Conclude()
        {
            base.Conclude();

            guardLogic = null;
        }

        public void SnapPose()
        {
            if (!IsReady) return;

            GuardPose pose = guardLogic.Pose;
            animationAdapter.Snap(pose);
            ApplyTransformAndCone(pose);
        }

        public void ApplyPose(float deltaTime)
        {
            if (!IsReady) return;

            GuardPose pose = guardLogic.Pose;
            animationAdapter.Apply(pose, deltaTime);
            ApplyTransformAndCone(pose);
        }

        private void ApplyTransformAndCone(GuardPose pose)
        {
            transform.SetPositionAndRotation(pose.Position, pose.Rotation);
            visionCone.gameObject.SetActive(!pose.IsTakenDown);

            if (pose.IsTakenDown) return;

            Color coneColor = pose.AlertState == GuardAlertState.Chase ? chaseConeColor : patrolConeColor;
            visionCone.Draw(guardLogic.EyePosition, pose.Yaw, guardConfig.Vision.Range, guardConfig.Vision.MinRange, guardConfig.Vision.FullAngle, coneColor);
        }

        private bool HasValidRoute()
        {
            foreach (GuardWaypointAdapter waypoint in route)
            {
                if (waypoint != null && waypoint.Point != null) continue;

                Debug.LogError($"{nameof(GuardAdapter)} on '{name}': a route waypoint has no point assigned.", this);
                return false;
            }

            return true;
        }
    }
}
