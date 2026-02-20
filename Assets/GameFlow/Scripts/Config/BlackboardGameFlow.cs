using CrawfisSoftware.TempleRun.GameConfig;

using UnityEngine;
using CrawfisSoftware.GameFlow.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CrawfisSoftware.GameFlow
{
    /// <summary>
    /// GameFlow-domain singleton holding session-level configuration.
    /// Subscribes to GameFlowEvents.DifficultyChanged and publishes
    /// GameFlowEvents.GameConfigApplying / GameConfigApplied.
    ///    Dependencies: None
    ///    Subscribes: GameFlowEvents.DifficultyChanged
    ///    Publishes: GameFlowEvents.GameConfigApplying, GameFlowEvents.GameConfigApplied
    /// </summary>
    public class BlackboardGameFlow : MonoBehaviour
    {
        public static BlackboardGameFlow Instance { get; private set; }

        public DifficultyConfig GameConfig { get; set; } = new DifficultyConfig();

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
