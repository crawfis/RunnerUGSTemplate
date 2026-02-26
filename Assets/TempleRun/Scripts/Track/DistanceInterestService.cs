using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Registration-based distance notification service. Any component can register
    /// a distance threshold and receive a callback when the player reaches it.
    ///    Dependencies: EventsPublisherTempleRun, Blackboard.DistanceTracker
    ///    Subscribes: TempleRunEvents.DistanceUpdated
    /// </summary>
    public class DistanceInterestService : MonoBehaviour
    {
        public static DistanceInterestService Instance { get; private set; }

        private struct DistanceInterest
        {
            public float Distance;
            public Action Callback;
        }

        // Sorted by distance ascending for efficient front-removal.
        private readonly List<DistanceInterest> _interests = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.DistanceUpdated, OnDistanceUpdated);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.DistanceUpdated, OnDistanceUpdated);
        }

        /// <summary>
        /// Register a one-shot callback that fires when the player's cumulative
        /// distance reaches or exceeds <paramref name="distance"/>.
        /// </summary>
        public void Register(float distance, Action callback)
        {
            var interest = new DistanceInterest { Distance = distance, Callback = callback };
            // Insert in sorted order.
            int index = _interests.FindIndex(i => i.Distance > distance);
            if (index < 0)
                _interests.Add(interest);
            else
                _interests.Insert(index, interest);
        }

        /// <summary>
        /// Remove all registered interests matching the given callback.
        /// </summary>
        public void Unregister(Action callback)
        {
            _interests.RemoveAll(i => i.Callback == callback);
        }

        /// <summary>
        /// Remove all registered interests.
        /// </summary>
        public void Clear()
        {
            _interests.Clear();
        }

        private void OnDistanceUpdated(string eventName, object sender, object data)
        {
            float currentDistance = (float)data;
            // Fire all interests whose threshold has been reached.
            while (_interests.Count > 0 && _interests[0].Distance <= currentDistance)
            {
                var interest = _interests[0];
                _interests.RemoveAt(0);
                interest.Callback?.Invoke();
            }
        }

        /// <summary>
        /// Check interests against the current distance without waiting for DistanceUpdated.
        /// Called by SegmentAdvanceTrigger in Update() for frame-accurate checking.
        /// </summary>
        public void CheckNow(float currentDistance)
        {
            while (_interests.Count > 0 && _interests[0].Distance <= currentDistance)
            {
                var interest = _interests[0];
                _interests.RemoveAt(0);
                interest.Callback?.Invoke();
            }
        }
    }
}
