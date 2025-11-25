using CrawfisSoftware.TempleRun;
using CrawfisSoftware.Events;

using UnityEngine;

namespace CrawfisSoftware.GameControl
{
    public class PauseController : MonoBehaviour
    {
        private bool _isPaused = false;

        public bool IsPaused { get { return _isPaused; } }

        private void Awake()
        {
            EventsPublisherUserInitiated.Instance.SubscribeToEvent(UserInitiatedEvents.PauseToggle, OnPauseToggle);

            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.Pause, OnPause);
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.Resume, OnResume);
        }

        private void OnDestroy()
        {
            EventsPublisherUserInitiated.Instance.UnsubscribeToEvent(UserInitiatedEvents.PauseToggle, OnPauseToggle);

            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.Pause, OnPause);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.Resume, OnResume);
        }
        public void Pause()
        {
            _isPaused = true;
            Time.timeScale = 0f;
        }

        public void Resume()
        {
            _isPaused = false;
            Time.timeScale = 1f;

        }

        public void TogglePauseResume()
        {
            if (_isPaused)
                EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.Resume, this, UnityEngine.Time.time);
            else
                EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.Pause, this, UnityEngine.Time.time);
        }

        private void OnPauseToggle(string eventName, object sender, object data)
        {
            TogglePauseResume();
        }

        private void OnPause(string eventName, object sender, object data)
        {
            Pause();
        }

        private void OnResume(string eventName, object sender, object data)
        {
            Resume();
        }
    }
}
