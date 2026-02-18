using CrawfisSoftware.Events;

using System.Collections.Generic;
using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Maps input events to game events. Will check if a turn request is the proper direction and within 
    ///    the turn distance. If so, it will fire a turn successful event.
    ///    Dependencies: Blackboard, DistanceTracker, EventsPublisherTempleRun
    ///    Subscribes: LeftTurnRequested and RightTurnRequested. If it is a valid turn published corresponding turn successful event.
    ///    Subscribes: ActiveTrackChanged - adjusts the next valid turn distance.
    ///    Publishes: LeftTurnSucceeded, RightTurnSucceeded
    /// </summary>
    public class TurnController : MonoBehaviour
    {
        public float TurnAvailableDistance { get { return _turnAvailableDistance; } }
        public float TurnFailedDistance { get { return _trackDistance; } }
        public Direction TurnDirection { get { return _nextTrackDirection; } }

        private float _safeTurnDistance = 1f;
        private float _trackDistance = 0;
        private float _turnAvailableDistance;
        // Possible Bug: If Direction is changed to a Flag, then _nextTrackDirection needs to be masked. Could be done now just in case.
        private Direction _nextTrackDirection;
        private readonly Dictionary<Direction, TempleRunEvents> _turnMapping = new()
        {
            [Direction.Left] = TempleRunEvents.TurnLeftCompleted,
            [Direction.Right] = TempleRunEvents.TurnRightCompleted,
            [Direction.Both] = TempleRunEvents.TurnRightCompleted
        };

        public void ForceTurn()
        {
            OnTurnRequested(this, null, _turnMapping[_nextTrackDirection]);
        }
        private void Awake()
        {
            EventsPublisherUserInitiated.Instance.SubscribeToEvent(UserInitiatedEvents.LeftTurnRequested, OnLeftTurnRequested);
            EventsPublisherUserInitiated.Instance.SubscribeToEvent(UserInitiatedEvents.RightTurnRequested, OnRightTurnRequested);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.ActiveTrackChanging, OnTrackChanging);
            _safeTurnDistance = Blackboard.Instance.GameConfig.SafePreTurnDistance;
        }

        private void OnTurnRequested(object sender, object data, TempleRunEvents turnSucceedEvent)
        {
            float distance = Blackboard.Instance.DistanceTracker.DistanceTravelled;
            if (distance > _turnAvailableDistance)
            {
                EventsPublisherTempleRun.Instance.PublishEvent(turnSucceedEvent, this, distance);
            }
        }

        private void OnLeftTurnRequested(string eventName, object sender, object data)
        {
            if (_nextTrackDirection != Direction.Right)
            {
                OnTurnRequested(sender, data, TempleRunEvents.TurnLeftCompleted);
            }
        }

        private void OnRightTurnRequested(string eventName, object sender, object data)
        {
            if (_nextTrackDirection != Direction.Left)
            {
                OnTurnRequested(sender, data, TempleRunEvents.TurnRightCompleted);
            }
        }

        private void OnTrackChanging(string eventName, object sender, object data)
        {
            var (direction, segmentDistance) = ((Direction direction, float segmentDistance))data;
            _nextTrackDirection = direction;
            _trackDistance += segmentDistance;
            _turnAvailableDistance = _trackDistance - _safeTurnDistance;
        }

        private void OnDestroy()
        {
            EventsPublisherUserInitiated.Instance.UnsubscribeToEvent(UserInitiatedEvents.LeftTurnRequested, OnLeftTurnRequested);
            EventsPublisherUserInitiated.Instance.UnsubscribeToEvent(UserInitiatedEvents.RightTurnRequested, OnRightTurnRequested);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.ActiveTrackChanging, OnTrackChanging);
        }
    }
}