using CrawfisSoftware.Events;

using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Handles game configuration completion and initializes game-specific runtime state (e.g., lives).
    /// Subscribes to GameFlow game config applied event if not already configured.
    /// </summary>
    internal class GameConfigInitializer : MonoBehaviour
    {
        private GameInitialization _gameInitializer;
        private bool _isInitialized;

        private void Awake()
        {
            if (GameState.IsGameConfigured)
            {
                InitializeGame();
            }
            else
            {
                EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameConfigApplied, OnGameConfigured);
            }
        }

        private void OnDestroy()
        {
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameConfigApplied, OnGameConfigured);
            _gameInitializer?.Dispose();
        }

        private void OnGameConfigured(string eventName, object sender, object data)
        {
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameConfigApplied, OnGameConfigured);
            InitializeGame();
        }

        private void InitializeGame()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            _gameInitializer = new GameInitialization(Blackboard.Instance.GameConfig.NumberOfLives);
        }
    }
}
