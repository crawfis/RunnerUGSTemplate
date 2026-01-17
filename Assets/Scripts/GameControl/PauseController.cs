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

            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.Paused, OnPause);
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.Resumed, OnResume);
        }

        private void OnDestroy()
        {
            EventsPublisherUserInitiated.Instance.UnsubscribeToEvent(UserInitiatedEvents.PauseToggle, OnPauseToggle);

            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.Paused, OnPause);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.Resumed, OnResume);
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
                EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.ResumeRequested, this, UnityEngine.Time.time);
            else
                EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.PauseRequested, this, UnityEngine.Time.time);
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
