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
    [RequireComponent(typeof(GameConfigInitializer))]
    internal class GameController : MonoBehaviour
    {
        private void Awake()
        {
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.PlayerDied, OnPlayerDied);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.CountdownEnded, OnCountdownEnded);
        }
        private void UnsubscribeToEvents()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.PlayerDied, OnPlayerDied);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.CountdownEnded, OnCountdownEnded);
        }

        private void OnCountdownEnded(string EventName, object sender, object data)
        {
            if (!GameState.IsGameConfigured)
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
        }

    }
}