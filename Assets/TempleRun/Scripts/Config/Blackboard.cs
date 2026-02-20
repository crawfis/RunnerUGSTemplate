using CrawfisSoftware.Events;
using CrawfisSoftware.GameConfig;
using CrawfisSoftware.TempleRun.GameConfig;
using CrawfisSoftware.Utility;

using System;


#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Space for "global" variables using a singleton.
    /// </summary>
    public class Blackboard : MonoBehaviour
    {
        [SerializeField] private RandomProviderFromList _randomProvider;
        [SerializeField] private LaneConfig _laneConfig;
        [SerializeField] private JumpConfig _jumpConfig;
        public static Blackboard Instance { get; private set; }
        public System.Random MasterRandom { get { return _randomProvider.RandomGenerator; } }
        public DifficultyConfig GameConfig { get; set; }  = new DifficultyConfig();
        public DistanceTracker DistanceTracker { get; set; } = new DistanceTracker();
        public float TrackWidthOffset { get; set; } = 1f;
        public float TileLength { get; set; } = 4f;
        public float CurrentSpeed { get; set; }

        // ---------- Lane State ----------
        public LaneConfig LaneConfig { get => _laneConfig; set => _laneConfig = value; }
        public int CurrentLane { get; set; } = 0;            // -1=left, 0=center, 1=right (for 3 lanes)
        public float LateralLaneOffset { get; set; } = 0f;   // Smooth lateral offset in world units

        // ---------- Jump State ----------
        public JumpConfig JumpConfig { get => _jumpConfig; set => _jumpConfig = value; }
        public float JumpHeightOffset { get; set; } = 0f;    // Current Y offset during jump arc

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }
        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeToEvents();
            DistanceTracker = new DistanceTracker();
        }

        private void OnGameDifficultyChanged(string eventName, object sender, object data)
        {
            DifficultyConfig difficulty = data as DifficultyConfig;
            if (difficulty != null)
            {
                EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.GameConfigApplying, this, difficulty);
                GameConfig = difficulty;
                Debug.Log($"Successfully set game difficulty to '{difficulty.DifficultyName}'");
                EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.GameConfigApplied, this, GameConfig);
            }
        }

        private void SubscribeToEvents()
        {
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.DifficultyChanged, OnGameDifficultyChanged);
        }

        private void UnsubscribeToEvents()
        {
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.DifficultyChanged, OnGameDifficultyChanged);
        }

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode()]
        public static void InitializeOnPlay()
        {
            Instance = null;
        }
#endif
    }
}