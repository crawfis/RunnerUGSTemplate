using CrawfisSoftware.Events;

using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    public class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }

        public static bool IsMainMenuActive { get; set; } = false;
        public static bool IsGameStarted { get; set; } = false;
        public static bool IsGameOver { get; internal set; } = false;
        public static bool IsGamePaused { get; internal set; } = false;
        public static bool IsGameConfigured { get; internal set; } = false;

        public  void Reset()
        {
            IsMainMenuActive = false;
            IsGameStarted = false;
            IsGameOver = false;
            IsGamePaused = false;
            IsGameConfigured = false;

        }
        private void Awake()
        {
            if (Instance != null)
            {
                DestroyImmediate(Instance);
            }
            Instance = this;
            Reset();

            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.MainMenuShowing, OnMainMenuShowing);
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.MainMenuHidden, OnMainMenuHidden);
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameStarted, OnGameStarted);
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameEnding, OnGameOver);
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameConfigApplied, OnGameConfigured);
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.Paused, OnPause);
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.Resumed, OnResume);
        }

        private void OnDestroy()
        {
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.MainMenuShowing, OnMainMenuShowing);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.MainMenuHidden, OnMainMenuHidden);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameStarted, OnGameStarted);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameEnding, OnGameOver);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameConfigApplied, OnGameConfigured);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.Paused, OnPause);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.Resumed, OnResume);
        }

        private void OnMainMenuShowing(string eventName, object sender, object data)
        {
            IsMainMenuActive = true;
        }
        private void OnMainMenuHidden(string eventName, object sender, object data)
        {
            IsMainMenuActive = false;
        }
        private void OnGameStarted(string eventName, object sender, object data)
        {
            GameState.IsGameStarted = true;
            GameState.IsGameOver = false;
        }

        private void OnGameOver(string eventName, object sender, object data)
        {
            GameState.IsGameOver = true;
            GameState.IsGameStarted = false;
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