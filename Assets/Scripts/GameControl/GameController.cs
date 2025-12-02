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
            _gameInitializer = new GameInitialization(Blackboard.Instance.GameConfig.NumberOfLives);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.PlayerDied, OnPlayerDied);
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.CountdownEnded, OnCountdownEnded);
        }
        private void UnsubscribeToEvents()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.PlayerDied, OnPlayerDied);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.CountdownEnded, OnCountdownEnded);
        }

        private void OnCountdownEnded(string EventName, object sender, object data)
        {
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
            _gameInitializer.Dispose();
        }

    }
}