using CrawfisSoftware.Events;
using CrawfisSoftware.UGS;

using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    [DefaultExecutionOrder(-1000)]
    class GameStateController : MonoBehaviour
    {
        private void Awake()
        {
            //EventsPublisher.Instance.Clear();
            GameState.Reset();

            //EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.PlayerSignedIn, OnPlayerSignedIn);
            //EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.UnityServicesInitialized, OnUnityServicesInitialized);

            EventsPublisherTempleRun.Instance.SubscribeToEvent(GamePlayEvents.GameStarted, OnGameStarted);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(GamePlayEvents.GameEnding, OnGameOver);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(GamePlayEvents.GameDifficultyChanged, OnGameConfigured);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(GamePlayEvents.Pause, OnPause);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(GamePlayEvents.Resume, OnResume);
        }

        private void OnDestroy()
        {
            //EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.PlayerSignedIn, OnPlayerSignedIn);
            //EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.UnityServicesInitialized, OnUnityServicesInitialized);

            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(GamePlayEvents.GameStarted, OnGameStarted);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(GamePlayEvents.GameEnding, OnGameOver);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(GamePlayEvents.GameDifficultyChanged, OnGameConfigured);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(GamePlayEvents.Pause, OnPause);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(GamePlayEvents.Resume, OnResume);
        }

        private void OnUnityServicesInitialized(string eventName, object sender, object data)
        {
            GameState.IsUnityServicesInitialized = true;
        }

        private void OnPlayerSignedIn(string eventName, object sender, object data)
        {
            GameState.IsPlayerSignedIn = true;
        }

        private void OnGameStarted(string eventName, object sender, object data)
        {
            GameState.IsPlayerSignedIn = true;
        }

        private void OnGameOver(string eventName, object sender, object data)
        {
            GameState.IsPlayerSignedIn = false;
        }

        private void OnGameConfigured(string eventName, object sender, object data)
        {
            GameState.IsGameConfigured = true;
        }

        private void OnPause(string eventName, object sender, object data)
        {
            GameState.IsGamePaused = true;
        }

        private void OnResume(string eventName, object sender, object data)
        {
            GameState.IsGamePaused = false;
        }
    }
}