using CrawfisSoftware.Events;

using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Compares the distance from DistanceTracker and compares it to the current track length. Fires the PlayerFailed event if the
    /// distance is greater than (or equal) the active track distance.
    ///    Dependencies: Blackboard, DistanceTracker, EventsPublisherTempleRun
    ///    Subscribes: ActiveTrackChanging - increased the active track length
    ///    Subscribes: GameStarted - useful if there is a delay between when the tracks are sent and the player has control.
    ///    Subscribes: GameEnded - useful if multiple players and we need to stop the checking.
    ///    Publishes: PlayerFailed event. Data is the current player distance.
    /// </summary>
    /// <remarks> For local multi-player we may need a player ID. Would be good to include this in the event data.</remarks>
    internal class DeathWatcher : MonoBehaviour
    {

        private float _currentSegmentDistance = 0f;
        private bool _isRunning = false;
        private bool _gameStarted = false;

        private void Awake()
        {
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.ActiveTrackChanging, OnTrackChanging);
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameStarted, OnGameStarted);
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameEnding, OnGameEnding);
        }

        private void Update()
        {
            float distance = Blackboard.Instance.DistanceTracker.DistanceTravelled;
            if (_isRunning && _gameStarted && distance >= _currentSegmentDistance)
            {
                _isRunning = false;
                Debug.Log(string.Format("Player Died at Distance: {0}", (int)_currentSegmentDistance));
                EventsPublisherTempleRun.Instance.PublishEvent(TempleRunEvents.PlayerFailing, this, distance);
            }
        }

        private void OnDestroy()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.ActiveTrackChanging, OnTrackChanging);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameStarted, OnGameStarted);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameEnding, OnGameEnding);
        }

        private void OnTrackChanging(string eventName, object sender, object data)
        {
            _isRunning = true;
            (Direction _, float distance) = ((Direction, float))data;
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