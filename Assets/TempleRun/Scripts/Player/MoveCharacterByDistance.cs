using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Moves the player along the current spline with lateral lane offset and jump height.
    ///    Dependencies: Blackboard, DistanceTracker, EventsPublisherTempleRun
    ///    Subscribes: CurrentSplineChanged
    /// </summary>
    public class MoveCharacterByDistance : MonoBehaviour
    {
        [SerializeField] private Transform _objectToMove;

        private Vector3 _currentDirection = Vector3.forward;
        private Vector3 _lastAnchorPoint;
        private float _lastAnchorDistance;
        private float _currentDistance = 0;
        private float _yPosition;

        private void Awake()
        {
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.CurrentSplineChanged, OnSplineChanged);
            _yPosition = transform.localPosition.y;
        }

        private void OnSplineChanged(string eventName, object sender, object data)
        {
            // Create prefab from the two points.
            var (point1, point2, direction) = ((Vector3 point1, Vector3 point2, Direction direction))(data);
            _currentDirection = (point2 - point1).normalized;
            _lastAnchorPoint = point1;
            _lastAnchorDistance = Blackboard.Instance.DistanceTracker.DistanceTravelled;
            float yPos = _yPosition + Blackboard.Instance.JumpHeightOffset;
            Vector3 basePos = new Vector3(point1.x, yPos, point1.z);
            basePos += GetLateralOffset();
            _objectToMove.localPosition = basePos;
            SetRotation(_currentDirection);
        }

        private void SetRotation(Vector3 direction)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            _objectToMove.localRotation = rotation;
        }

        private void Update()
        {
            float distance = Blackboard.Instance.DistanceTracker.DistanceTravelled;
            if (distance - _currentDistance < 0.001f) return;

            Vector3 newPosition = _lastAnchorPoint + (distance - _lastAnchorDistance) * _currentDirection;
            float yPos = _yPosition + Blackboard.Instance.JumpHeightOffset;
            newPosition = new Vector3(newPosition.x, yPos, newPosition.z);
            newPosition += GetLateralOffset();
            _objectToMove.localPosition = newPosition;
            _currentDistance = distance;
        }

        /// <summary>
        /// Computes the lateral offset perpendicular to the current movement direction.
        /// Positive LateralLaneOffset shifts right, negative shifts left (from the player's perspective).
        /// </summary>
        private Vector3 GetLateralOffset()
        {
            float laneOffset = Blackboard.Instance.LateralLaneOffset;
            if (Mathf.Abs(laneOffset) < 0.001f) return Vector3.zero;

            Vector3 perpendicular = Vector3.Cross(_currentDirection, Vector3.up).normalized;
            return laneOffset * perpendicular;
        }

        private void OnDestroy()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.CurrentSplineChanged, OnSplineChanged);
        }
    }
}