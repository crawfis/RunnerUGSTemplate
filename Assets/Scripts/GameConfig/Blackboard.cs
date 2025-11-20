using CrawfisSoftware.GameConfig;
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
        public GameDifficultyManager GameDifficultyManager { get; set; }
        public DifficultyConfig GameConfig { get; set; }
        public DistanceTracker DistanceTracker { get; set; }
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
        }

        private void OnGameDifficultyChanged(string eventName, object sender, object data)
        {
            if (GameDifficultyManager != null)
            {
                GameConfig = GameDifficultyManager.CurrentDifficultyConfig;
            }
        }

        private void SubscribeToEvents()
        {
            EventsPublisherTempleRun.Instance.SubscribeToEvent(GamePlayEvents.GameDifficultyChanged, OnGameDifficultyChanged);
        }

        private void UnsubscribeToEvents()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(GamePlayEvents.GameDifficultyChanged, OnGameDifficultyChanged);
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