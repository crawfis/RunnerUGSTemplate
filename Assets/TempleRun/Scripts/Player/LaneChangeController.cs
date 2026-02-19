using CrawfisSoftware.Events;

using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Validates lane change requests and publishes lane change events.
    /// Blocks lane changes at boundaries and while a change is in progress.
    ///    Dependencies: Blackboard, LaneConfig
    ///    Subscribes: UserInitiatedEvents.LeftLaneChangeRequested, RightLaneChangeRequested
    ///    Subscribes: TempleRunEvents.LaneChangedLeft, LaneChangedRight (clear _isChanging)
    ///    Subscribes: GameFlowEvents.GameStarting (reset lane state)
    ///    Publishes: TempleRunEvents.LaneChangeLeftRequested, LaneChangeRightRequested
    ///    Publishes: TempleRunEvents.LaneChangeLeftFailed, LaneChangeRightFailed
    /// </summary>
    internal class LaneChangeController : MonoBehaviour
    {
        private int _minLane;
        private int _maxLane;
        private bool _isChanging = false;

        private void Start()
        {
            // Subscribe to raw input events
            EventsPublisherUserInitiated.Instance.SubscribeToEvent(
                UserInitiatedEvents.LeftLaneChangeRequested, OnLeftLaneChangeRequested);
            EventsPublisherUserInitiated.Instance.SubscribeToEvent(
                UserInitiatedEvents.RightLaneChangeRequested, OnRightLaneChangeRequested);

            // Subscribe to completion events to clear the _isChanging flag
            EventsPublisherTempleRun.Instance.SubscribeToEvent(
                TempleRunEvents.LaneChangedLeft, OnLaneChangeCompleted);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(
                TempleRunEvents.LaneChangedRight, OnLaneChangeCompleted);

            // Subscribe to game start to reset lane state
            EventsPublisherGameFlow.Instance.SubscribeToEvent(
                GameFlowEvents.GameStarting, OnGameStarting);

            // Compute lane boundaries from config
            var laneConfig = Blackboard.Instance.LaneConfig;
            if (laneConfig != null)
            {
                int halfLanes = (laneConfig.LaneCount - 1) / 2;
                _minLane = -halfLanes;  // -1 for 3 lanes
                _maxLane = halfLanes;   //  1 for 3 lanes
            }
            else
            {
                // Sensible defaults if no config assigned
                _minLane = -1;
                _maxLane = 1;
                Debug.LogWarning("LaneChangeController: No LaneConfig assigned on Blackboard. Using default 3-lane layout.");
            }
        }

        private void OnDestroy()
        {
            EventsPublisherUserInitiated.Instance.UnsubscribeToEvent(
                UserInitiatedEvents.LeftLaneChangeRequested, OnLeftLaneChangeRequested);
            EventsPublisherUserInitiated.Instance.UnsubscribeToEvent(
                UserInitiatedEvents.RightLaneChangeRequested, OnRightLaneChangeRequested);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(
                TempleRunEvents.LaneChangedLeft, OnLaneChangeCompleted);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(
                TempleRunEvents.LaneChangedRight, OnLaneChangeCompleted);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(
                GameFlowEvents.GameStarting, OnGameStarting);
        }

        private void OnLeftLaneChangeRequested(string eventName, object sender, object data)
        {
            if (_isChanging)
            {
                //EventsPublisherTempleRun.Instance.PublishEvent(
                //    TempleRunEvents.LaneChangeLeftFailed, this, Blackboard.Instance.CurrentLane);
                return;
            }

            int currentLane = Blackboard.Instance.CurrentLane;
            if (currentLane <= _minLane)
            {
                // Already at leftmost lane
                EventsPublisherTempleRun.Instance.PublishEvent(
                    TempleRunEvents.LaneChangeLeftFailed, this, currentLane);
                return;
            }

            _isChanging = true;
            Blackboard.Instance.CurrentLane = currentLane - 1;
            EventsPublisherTempleRun.Instance.PublishEvent(
                TempleRunEvents.LaneChangingLeft, this, Blackboard.Instance.CurrentLane);
        }

        private void OnRightLaneChangeRequested(string eventName, object sender, object data)
        {
            if (_isChanging)
            {
                //EventsPublisherTempleRun.Instance.PublishEvent(
                //    TempleRunEvents.LaneChangeRightFailed, this, Blackboard.Instance.CurrentLane);
                return;
            }

            int currentLane = Blackboard.Instance.CurrentLane;
            if (currentLane >= _maxLane)
            {
                // Already at rightmost lane
                EventsPublisherTempleRun.Instance.PublishEvent(
                    TempleRunEvents.LaneChangeRightFailed, this, currentLane);
                return;
            }

            _isChanging = true;
            Blackboard.Instance.CurrentLane = currentLane + 1;
            EventsPublisherTempleRun.Instance.PublishEvent(
                TempleRunEvents.LaneChangingRight, this, Blackboard.Instance.CurrentLane);
        }

        private void OnLaneChangeCompleted(string eventName, object sender, object data)
        {
            _isChanging = false;
        }

        private void OnGameStarting(string eventName, object sender, object data)
        {
            _isChanging = false;
            Blackboard.Instance.CurrentLane = 0;
            Blackboard.Instance.LateralLaneOffset = 0f;
        }
    }
}
