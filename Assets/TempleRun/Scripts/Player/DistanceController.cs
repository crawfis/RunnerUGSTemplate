using CrawfisSoftware.Events;

using System.Collections;

using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Speed controller that updates a DistanceTracker.
    ///    Dependencies: Blackboard, DistanceTracker and GameConfig (from Blackboard)
    ///    Subscribes: GameStarted
    ///    Subscribes: GameOver
    ///    Subscribes: TeleportStarted
    ///    Subscribes: TeleportEnded
    /// </summary>
    internal class DistanceController : MonoBehaviour
    {
        private float _initialSpeed;
        private float _maxSpeed;
        private float _acceleration;
        private float _speed;
        private Coroutine _coroutine;
        private bool _isMoving = true;
        private float _trackDistance = 0;

        private void Awake()
        {
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.PlayerFailing, OnResetSpeed);
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameStarted, OnGameStarted);
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameEnding, OnGameOver);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.TeleportStarted, OnTeleportStarted);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.TeleportEnded, OnTeleportEnded);
            //EventsPublisherTempleRun.Instance.SubscribeToEvent(GamePlayEvents.ActiveTrackChanging, OnTrackChanging);
        }

        private void OnDestroy()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.PlayerFailing, OnResetSpeed);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameStarted, OnGameStarted);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameEnding, OnGameOver);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.TeleportStarted, OnTeleportStarted);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.TeleportEnded, OnTeleportEnded);
            DeleteCoroutine();
        }

        private void OnResetSpeed(string eventName, object sender, object data)
        {
            _speed = _initialSpeed;
        }

        private void OnGameStarted(string eventName, object sender, object data)
        {
            _initialSpeed = Blackboard.Instance.GameConfig.InitialSpeed;
            _maxSpeed = Blackboard.Instance.GameConfig.MaxSpeed;
            _acceleration = Blackboard.Instance.GameConfig.Acceleration;
            _speed = _initialSpeed;
            float initialDistance = Blackboard.Instance.TrackWidthOffset;
            //Blackboard.Instance.DistanceTracker.UpdateDistance(initialDistance);
            _coroutine = StartCoroutine(UpdateAfterGameStart());
        }

        private void OnGameOver(string eventName, object sender, object data)
        {
            DeleteCoroutine();
        }

        //private void OnTrackChanging(object sender, object data)
        //{
        //    var (_, segmentDistance) = ((Direction direction, float segmentDistance))data;
        //    _trackDistance += segmentDistance;
        //}

        private void OnTeleportStarted(string eventName, object sender, object data)
        {
            _isMoving = false;
        }

        private void OnTeleportEnded(string eventName, object sender, object data)
        {
            _isMoving = true;
            Blackboard.Instance.DistanceTracker.UpdateDistance(_trackDistance - Blackboard.Instance.DistanceTracker.DistanceTravelled);
            var (point1, point2, _) = ((Vector3 point1, Vector3 point2, Direction direction))data;
            _trackDistance += Vector3.Magnitude(point1 - point2);
        }

        IEnumerator UpdateAfterGameStart()
        {
            DistanceTracker _distanceTracker = Blackboard.Instance.DistanceTracker;
            while (true)
            {
                if (_isMoving)
                {
                    _distanceTracker.UpdateDistance(_speed * GameTime.Instance.deltaTime);
                    _speed += _acceleration * GameTime.Instance.deltaTime;
                    _speed = Mathf.Clamp(_speed, _initialSpeed, _maxSpeed);
                    Blackboard.Instance.CurrentSpeed = _speed;
                }
                yield return new WaitForEndOfFrame();
            }
        }

        private void DeleteCoroutine()
        {
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }
}