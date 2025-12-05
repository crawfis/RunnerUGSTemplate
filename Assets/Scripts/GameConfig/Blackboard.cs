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
        public static Blackboard Instance { get; private set; }
        public System.Random MasterRandom { get { return _randomProvider.RandomGenerator; } }
        public DifficultyConfig GameConfig { get; set; }  = new DifficultyConfig();
        public DistanceTracker DistanceTracker { get; set; } = new DistanceTracker();
        public float TrackWidthOffset { get; set; } = 1f;
        public float TileLength { get; set; } = 4f;
        public float CurrentSpeed { get; set; }

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
                GameConfig = difficulty;
                Debug.Log($"Successfully set game difficulty to '{difficulty.DifficultyName}'");
                EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.GameConfigured, this, GameConfig);
            }
        }

        private void SubscribeToEvents()
        {
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameDifficultyChanged, OnGameDifficultyChanged);
        }

        private void UnsubscribeToEvents()
        {
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameDifficultyChanged, OnGameDifficultyChanged);
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