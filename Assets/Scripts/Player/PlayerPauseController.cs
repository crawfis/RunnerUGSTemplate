using CrawfisSoftware.Events;

using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    public class PlayerPauseController : MonoBehaviour
    {
        private bool _isPaused = false;

        public bool IsPaused { get { return _isPaused; } }

        private void Awake()
        {
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.PlayerPause, OnPause);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.PlayerResume, OnResume);
        }

        private void OnDestroy()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.PlayerPause, OnPause);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.PlayerResume, OnResume);
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