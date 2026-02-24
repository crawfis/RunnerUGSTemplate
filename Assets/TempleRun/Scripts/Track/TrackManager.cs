using System;
using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Provides new track distance for each turn. It publishes a new track segment
    ///       when needed (either to create visuals or to determine the currently active track).
    ///    Dependencies: EventsPublisherTempleRun, Blackboard.GameConfig, Blackboard.MasterRandom,
    ///                  Blackboard.TrackLevelDefinition (set by level selection)
    ///    Subscribes to the Turn Succeeded events (LeftTurnSucceeded, RightTurnSucceeded)
    ///    Publishes: TrackSegmentCreated. Useful for creating prefabs. Several of these will be created at the start. Data is a TrackSegmentInfo
    ///    Publishes: ActiveTrackChanging. The track that we are transitioning to. Data is a TrackSegmentInfo
    /// </summary>
    /// <remarks> Obstacle and gap distances should be in a separate class(es).
    /// Random distances (_random) could be replaced with a list of possible distances, but a better / cleaner solution would
    /// be to have another class subscribe to the event, massage the data and publish a new event. This may be needed
    /// for example to map the distance to a number of tiles.</remarks>
    /// <remarks>Used as a base class for integer-based tracks (voxels or tiles) and a fixed set of track lengths.</remarks>
    public class TrackManager : TrackManagerAbstract
    {
        [SerializeField] int _numberOfLookAheadTracks = 12;
        [SerializeField] private TextAsset _trackSegmentLibraryJson;

        protected Queue<TrackSegmentInfo> _trackSegments;
        protected float _startDistance = 10f;
        protected float _minDistance = 3;
        protected float _maxDistance = 9;
        protected System.Random _random;
        private TrackSegmentLibrary _segmentLibrary;
        private string _lastSegmentId;
        private int _lastSegmentRepeatCount;
        private bool _isInitialized = false;

        protected virtual void Awake()
        {
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.TempleRunScenesReady, OnGameStarting);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.TempleRunConfigApplied, OnGameConfigured);
        }

        protected virtual void OnDestroy()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.TempleRunScenesReady, OnGameStarting);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.TempleRunConfigApplied, OnGameConfigured);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.TurnLeftCompleted, OnTurnSucceeded);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.TurnRightCompleted, OnTurnSucceeded);
        }

        private void Start()
        {
            _trackSegments = new(_numberOfLookAheadTracks);

        }
        private void OnGameConfigured(string eventName, object sender, object data)
        {
            Initialize();
        }

        private void Initialize()
        {
            var gameConfig = Blackboard.Instance.GameConfig;
            Initialize(gameConfig.StartRunway, gameConfig.MinTrackLength,
                gameConfig.MaxTrackLength, Blackboard.Instance.MasterRandom);
            _isInitialized = true;
        }

        protected virtual void OnGameStarting(string eventName, object sender, object data)
        {
            if(!_isInitialized)
            {
                Initialize();
            }
            CreateInitialTrack();
        }

        public override void AdvanceToNextSegment()
        {
            _ = _trackSegments.Dequeue();
            AddTrackSegment();
            EventsPublisherTempleRun.Instance.PublishEvent(TempleRunEvents.ActiveTrackChanging, this, _trackSegments.Peek());
        }

        protected virtual void Initialize(float startDistance, float minDistance, float maxDistance, System.Random random)
        {
            _startDistance = startDistance;
            _minDistance = minDistance;
            _maxDistance = maxDistance;
            _random = random;

            // Build runtime library from Blackboard's level definition + registry
            var levelDef = Blackboard.Instance.TrackLevelDefinition;
            if (levelDef != null)
            {
                string registryJson = null;
                if (!string.IsNullOrWhiteSpace(levelDef.SegmentRegistryFile))
                {
                    var registryAsset = Resources.Load<TextAsset>(levelDef.SegmentRegistryFile);
                    registryJson = registryAsset?.text;
                }
                _segmentLibrary = TrackSegmentLibrary.LoadFromDefinition(levelDef, registryJson);
            }

            // Fall back to single-file loading (legacy / inspector-assigned TextAsset)
            if (_segmentLibrary == null)
            {
                if (_trackSegmentLibraryJson == null)
                {
                    _trackSegmentLibraryJson = Resources.Load<TextAsset>("TrackSegments");
                }
                _segmentLibrary = TrackSegmentLibrary.LoadFromJson(_trackSegmentLibraryJson?.text);
            }
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.TurnLeftCompleted, OnTurnSucceeded);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.TurnRightCompleted, OnTurnSucceeded);
        }

        protected virtual void CreateInitialTrack()
        {
            _maxDistance = Mathf.Max(_minDistance, _maxDistance);
            var newTrackSegment = CreateTrackSegment(isStartSegment: true);
            _trackSegments.Enqueue(newTrackSegment);
            EventsPublisherTempleRun.Instance.PublishEvent(TempleRunEvents.TrackSegmentCreated, this, newTrackSegment);
            for (int i = 1; i < _numberOfLookAheadTracks; i++)
            {
                AddTrackSegment();
            }
            EventsPublisherTempleRun.Instance.PublishEvent(TempleRunEvents.ActiveTrackChanging, this, _trackSegments.Peek());
        }

        protected virtual void OnTurnSucceeded(string eventName, object sender, object data)
        {
            AdvanceToNextSegment();
        }

        protected virtual void AddTrackSegment()
        {
            var newTrackSegment = CreateTrackSegment(isStartSegment: false);
            _trackSegments.Enqueue(newTrackSegment);
            EventsPublisherTempleRun.Instance.PublishEvent(TempleRunEvents.TrackSegmentCreated, this, newTrackSegment);
        }

        protected virtual TrackSegmentInfo CreateTrackSegment(bool isStartSegment)
        {
            if (_segmentLibrary != null)
            {
                var segmentDefinition = isStartSegment
                    ? _segmentLibrary.GetStartSegment(_random)
                    : _segmentLibrary.SelectNext(_lastSegmentId, _lastSegmentRepeatCount, _random);

                if (segmentDefinition != null)
                {
                    UpdateRepeatTracking(segmentDefinition.Id);
                    var direction = ParseDirection(segmentDefinition.Direction, GetNewDirection());
                    return new TrackSegmentInfo(segmentDefinition.Id, direction.ToString(), (int)direction, segmentDefinition.Length);
                }
            }

            float segmentLength = isStartSegment ? _startDistance : GetNewSegmentLength();
            var fallbackDirection = GetNewDirection();
            return new TrackSegmentInfo("random", fallbackDirection.ToString(), (int)fallbackDirection, segmentLength);
        }

        private static Direction ParseDirection(string directionValue, Direction fallback)
        {
            if (!string.IsNullOrWhiteSpace(directionValue) && Enum.TryParse(directionValue, true, out Direction parsed))
            {
                return parsed;
            }

            return fallback;
        }


        private void UpdateRepeatTracking(string segmentId)
        {
            if (string.Equals(_lastSegmentId, segmentId, System.StringComparison.Ordinal))
            {
                _lastSegmentRepeatCount++;
            }
            else
            {
                _lastSegmentId = segmentId;
                _lastSegmentRepeatCount = 1;
            }
        }

        protected virtual float GetNewSegmentLength()
        {
            return (float)_random.NextDouble() * (_maxDistance - _minDistance) + _minDistance;
        }

        protected virtual Direction GetNewDirection()
        {
            float randomValue = (float)_random.NextDouble();
            return randomValue switch
            {
                < 0.4f => Direction.Left,
                < 0.8f => Direction.Right,
                _ => Direction.Left,
            };
        }
    }
}