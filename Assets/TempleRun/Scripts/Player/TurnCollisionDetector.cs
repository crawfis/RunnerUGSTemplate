using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Compares the distance from DistanceTracker to the current track segment length.
    /// Fires PlayerFailingAtTurn when the player exceeds the segment distance without turning.
    ///    Dependencies: Blackboard, DistanceTracker, EventsPublisherTempleRun
    ///    Subscribes: TempleRunEvents.ActiveTrackChanging — increases the active track length
    ///    Subscribes: TempleRunEvents.TempleRunStarted — begins distance checking
    ///    Subscribes: TempleRunEvents.PlayerDied — stops distance checking
    ///    Publishes: TempleRunEvents.PlayerFailingAtTurn — Data is the current player distance (float).
    /// </summary>
    /// <remarks>For local multi-player we may need a player ID. Would be good to include this in the event data.</remarks>
    internal class TurnCollisionDetector : MonoBehaviour
    {

        private float _currentSegmentDistance = 0f;
        private bool _isRunning = false;
        private bool _gameStarted = false;

        private void Awake()
        {
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.ActiveTrackChanging, OnTrackChanging);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.TempleRunStarted, OnGameStarted);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.PlayerDied, OnGameEnding);
        }

        private void Update()
        {
            float distance = Blackboard.Instance.DistanceTracker.DistanceTravelled;
            if (_isRunning && _gameStarted && distance >= _currentSegmentDistance)
            {
                _isRunning = false;
                Debug.Log(string.Format("Player failed turn at distance: {0}", (int)_currentSegmentDistance));
                EventsPublisherTempleRun.Instance.PublishEvent(TempleRunEvents.PlayerFailingAtTurn, this, distance);
            }
        }

        private void OnDestroy()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.ActiveTrackChanging, OnTrackChanging);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.TempleRunStarted, OnGameStarted);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.PlayerDied, OnGameEnding);
        }

        private void OnTrackChanging(string eventName, object sender, object data)
        {
            _isRunning = true;
            TrackSegmentInfo trackSegmentInfo = (TrackSegmentInfo) data;
            float distance = trackSegmentInfo.Length;
            //(Direction _, float distance) = ((Direction, float))data;
            _currentSegmentDistance += distance;
        }

        private void OnGameStarted(string eventName, object sender, object data)
        {
            _gameStarted = true;
        }

        private void OnGameEnding(string eventName, object sender, object data)
        {
            _gameStarted = false;
        }
    }
}
