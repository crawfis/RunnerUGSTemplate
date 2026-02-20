using CrawfisSoftware.TempleRun.GameConfig;
using CrawfisSoftware.Utility;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// TempleRun-domain singleton holding gameplay state.
    ///    Dependencies: None
    ///    Subscribes: TempleRunEvents.TempleRunConfigApplied (bridged from GameFlow)
    ///    Subscribes: TempleRunEvents.TempleRunDifficultyChanged (bridged from GameFlow)
    /// </summary>
    public class Blackboard : MonoBehaviour
    {
        [SerializeField] private RandomProviderFromList _randomProvider;
        [SerializeField] private LaneConfig _laneConfig;
        [SerializeField] private JumpConfig _jumpConfig;
        [SerializeField] private LaneChangeController _laneChangeController;
        public static Blackboard Instance { get; private set; }
        public System.Random MasterRandom { get { return _randomProvider.RandomGenerator; } }
        public DifficultyConfig GameConfig { get; set; }  = new DifficultyConfig();
        public DistanceTracker DistanceTracker { get; set; } = new DistanceTracker();
        public float TrackWidthOffset { get; set; } = 1f;
        public float TileLength { get; set; } = 4f;
        public float CurrentSpeed { get; set; } = 0f;

        // ---------- Lane State ----------
        public LaneConfig LaneConfig { get => _laneConfig; set => _laneConfig = value; }
        public LaneChangeController LaneChangeController { get => _laneChangeController; set => _laneChangeController = value; }

        // ---------- Jump State ----------
        public JumpConfig JumpConfig { get => _jumpConfig; set => _jumpConfig = value; }
        public float JumpHeightOffset { get; set; } = 0f;    // Current Y offset during jump arc

        private const float DEFAULT_TRACK_WIDTH_OFFSET = 1f;
        private const float DEFAULT_TILE_LENGTH = 4f;

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
            ResetState();
        }

        /// <summary>
        /// Resets all gameplay state to initial defaults. Called on scene unload.
        /// </summary>
        private void ResetState()
        {
            GameConfig = new DifficultyConfig();
            DistanceTracker = new DistanceTracker();
            CurrentSpeed = 0f;
            TrackWidthOffset = DEFAULT_TRACK_WIDTH_OFFSET;
            TileLength = DEFAULT_TILE_LENGTH;
            JumpHeightOffset = 0f;
        }

        private void OnConfigApplied(string eventName, object sender, object data)
        {
            DifficultyConfig difficulty = data as DifficultyConfig;
            if (difficulty != null)
            {
                GameConfig = difficulty;
                Debug.Log($"Blackboard: GameConfig set to '{difficulty.DifficultyName}'");
            }
        }

        private void SubscribeToEvents()
        {
            EventsPublisherTempleRun.Instance.SubscribeToEvent(
                TempleRunEvents.TempleRunConfigApplied, OnConfigApplied);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(
                TempleRunEvents.TempleRunDifficultyChanged, OnConfigApplied);
        }

        private void UnsubscribeToEvents()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(
                TempleRunEvents.TempleRunConfigApplied, OnConfigApplied);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(
                TempleRunEvents.TempleRunDifficultyChanged, OnConfigApplied);
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
