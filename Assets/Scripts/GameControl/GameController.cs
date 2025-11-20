using CrawfisSoftware.GameConfig;

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
            EventsPublisherTempleRun.Instance.SubscribeToEvent(GamePlayEvents.PlayerDied, OnPlayerDied);
        }
        private void UnsubscribeToEvents()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(GamePlayEvents.PlayerDied, OnPlayerDied);
        }

        private void Start()
        {
            _ = StartCoroutine(StartGame());
        }

        private IEnumerator StartGame()
        {
            yield return null;
            EventsPublisherTempleRun.Instance.PublishEvent(GamePlayEvents.Resume, this, UnityEngine.Time.time);
            yield return new WaitForSecondsRealtime(GameConstants.CountdownSeconds);
            EventsPublisherTempleRun.Instance.PublishEvent(GamePlayEvents.GameStarted, this, UnityEngine.Time.time);
            //EventsPublisherTempleRun.Instance.PublishEvent(GamePlayEvents.Resume, this, UnityEngine.Time.time);
        }

        private void OnPlayerDied(string EventName, object sender, object data)
        {
            EventsPublisherTempleRun.Instance.PublishEvent(GamePlayEvents.GameEnding, this, UnityEngine.Time.time);
        }

        private void OnDestroy()
        {
            UnsubscribeToEvents();
            _gameInitializer.Dispose();
        }

    }
}