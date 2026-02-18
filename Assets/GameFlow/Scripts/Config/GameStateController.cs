//using CrawfisSoftware.Events;
//using CrawfisSoftware.UGS;

//using UnityEngine;

//namespace CrawfisSoftware.TempleRun
//{
//    [DefaultExecutionOrder(-1000)]
//    class GameStateController : MonoBehaviour
//    {
//        private void Awake()
//        {
//            GameState.Reset();

//            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameStarted, OnGameStarted);
//            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameEnding, OnGameOver);
//            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameDifficultyChanged, OnGameConfigured);
//            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.Pause, OnPause);
//            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.Resume, OnResume);
//        }

//        private void OnDestroy()
//        {
//            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameStarted, OnGameStarted);
//            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameEnding, OnGameOver);
//            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameDifficultyChanged, OnGameConfigured);
//            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.Pause, OnPause);
//            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.Resume, OnResume);
//        }

//        private void OnGameStarted(string eventName, object sender, object data)
//        {
//            GameState.IsPlayerSignedIn = true;
//        }

//        private void OnGameOver(string eventName, object sender, object data)
//        {
//            GameState.IsPlayerSignedIn = false;
//        }

//        private void OnGameConfigured(string eventName, object sender, object data)
//        {
//            GameState.IsGameConfigured = true;
//        }

//        private void OnPause(string eventName, object sender, object data)
//        {
//            GameState.IsGamePaused = true;
//        }

//        private void OnResume(string eventName, object sender, object data)
//        {
//            GameState.IsGamePaused = false;
//        }
//    }
//}