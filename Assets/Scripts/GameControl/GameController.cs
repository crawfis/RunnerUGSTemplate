using CrawfisSoftware.Events;

using System.Collections;

using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Overall game control handling game initialization, pausing, resuming and player dying (triggering a Game Over).
    ///    Dependency: GameInitialization, EventsPublisherTempleRun
    ///    Subscribes: PlayerDied - Publishes a GameOver event
    ///    Subscribes: Pause - pauses be setting time scale to zero
    ///    Subscribes: Resume - resets the time scale to one
    ///    Publishes: GameStarted
    ///    Publishes: GameOver
    /// </summary>
    internal class GameController : MonoBehaviour
    {
        private GameInitialization _gameInitializer;
        private void Awake()
        {
            if (GameState.IsGameConfigured) 
                OnGameConfigured("GameConfigured", "Self", null);
            else
                EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameConfigApplied, OnGameConfigured);

            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.PlayerDied, OnPlayerDied);
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.CountdownEnded, OnCountdownEnded);
        }
        private void UnsubscribeToEvents()
        {
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameConfigApplied, OnGameConfigured);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.PlayerDied, OnPlayerDied);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.CountdownEnded, OnCountdownEnded);
        }

        private void OnGameConfigured(string EventName, object sender, object data)
        {
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameConfigApplied, OnGameConfigured);
            _gameInitializer = new GameInitialization(Blackboard.Instance.GameConfig.NumberOfLives);
        }

        private void OnCountdownEnded(string EventName, object sender, object data)
        {
            if (_gameInitializer == null)
                throw new System.ApplicationException("Countdown ended before game was even configured.");
            _ = StartCoroutine(StartGame());
        }

        private IEnumerator StartGame()
        {
            yield return null;
            EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.GameStarted, this, null);
        }

        private void OnPlayerDied(string EventName, object sender, object data)
        {
            float finalScore = (float)data;
            EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.GameEnding, this, finalScore);
        }

        private void OnDestroy()
        {
            UnsubscribeToEvents();
            _gameInitializer?.Dispose();
        }

    }
}