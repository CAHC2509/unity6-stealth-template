using UnityEngine;

namespace StealthController.Core
{
    public class WallCornerTraversalLogic
    {
        private Vector3 start;
        private Vector3 edge;
        private Vector3 fromNormal;
        private Vector3 toNormal;
        private Vector3 turnAxis;
        private Vector3 approachDirection;
        private Vector3 exitDirection;
        private float approachLength;
        private float arcRadius;
        private float arcLength;
        private float exitLength;
        private float travelled;
        private bool isActive;

        public bool IsActive => isActive;
        public bool HasReachedEnd => isActive && travelled >= TotalLength;
        public bool HasReturnedToStart => isActive && travelled <= 0f;
        public Vector3 ToNormal => toNormal;
        public Vector3 StartDirection => Vector3.Cross(turnAxis, fromNormal).normalized;
        public Vector3 ExitDirection => exitDirection;
        public Vector3 CurrentNormal => travelled <= approachLength ? fromNormal : Vector3.Slerp(fromNormal, toNormal, ArcProgress).normalized;

        public Vector3 CurrentPoint
        {
            get
            {
                if (travelled <= approachLength) return start + approachDirection * travelled;

                Vector3 arcPoint = edge + Vector3.Slerp(fromNormal, toNormal, ArcProgress) * arcRadius;
                return arcPoint + exitDirection * Mathf.Max(travelled - approachLength - arcLength, 0f);
            }
        }

        public Vector3 ForwardDirection
        {
            get
            {
                if (travelled < approachLength) return approachDirection;
                if (travelled >= approachLength + arcLength) return exitDirection;

                return Vector3.Cross(turnAxis, CurrentNormal).normalized;
            }
        }

        private float TotalLength => approachLength + arcLength + exitLength;
        private float ArcProgress => arcLength > 0f ? Mathf.Clamp01((travelled - approachLength) / arcLength) : 1f;

        public void Begin(Vector3 position, Vector3 edgePoint, Vector3 currentNormal, Vector3 nextNormal, float exitDistance)
        {
            start = position;
            edge = new Vector3(edgePoint.x, position.y, edgePoint.z);
            fromNormal = currentNormal;
            toNormal = nextNormal;
            turnAxis = Vector3.Cross(fromNormal, toNormal).normalized;

            Vector3 toEdge = Vector3.ProjectOnPlane(edge - start, fromNormal);
            approachLength = toEdge.magnitude;
            approachDirection = approachLength > 0f ? toEdge / approachLength : Vector3.zero;

            arcRadius = Mathf.Max(Vector3.Dot(start - edge, fromNormal), 0f);
            arcLength = arcRadius * Vector3.Angle(fromNormal, toNormal) * Mathf.Deg2Rad;
            exitDirection = Vector3.ProjectOnPlane(-fromNormal, toNormal).normalized;
            exitLength = exitDistance;
            travelled = 0f;
            isActive = true;
        }

        public void Advance(float distance)
        {
            travelled = Mathf.Clamp(travelled + distance, 0f, TotalLength);
        }

        public void End()
        {
            isActive = false;
        }
    }
}
