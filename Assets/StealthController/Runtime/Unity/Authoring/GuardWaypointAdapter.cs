using System;
using StealthController.Core;
using UnityEngine;

namespace StealthController.Unity
{
    [Serializable]
    public class GuardWaypointAdapter
    {
        [SerializeField, Tooltip("Scene object whose position the guard walks to. Its forward direction is where the guard looks when Face Forward is on.")] private Transform point;
        [SerializeField, Min(0f), Tooltip("Seconds the guard waits at this point.")] private float waitDuration = StealthUnityConstants.GUARD_DEFAULT_WAYPOINT_WAIT;
        [SerializeField, Tooltip("While waiting, the guard turns to face the point's forward direction.")] private bool faceForward;

        public Transform Point => point;

        public GuardWaypoint ToWaypoint() => new GuardWaypoint(point.position, waitDuration, faceForward, point.eulerAngles.y);
    }
}
