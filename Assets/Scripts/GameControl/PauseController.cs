using CrawfisSoftware.TempleRun;
using CrawfisSoftware.Events;

using UnityEngine;

namespace CrawfisSoftware.GameControl
{
    internal class PauseController : MonoBehaviour
    {
        private bool _isPaused = false;

        public bool IsPaused { get { return _isPaused; } }

        private void Awake()
        {
            EventsPublisherUserInitiated.Instance.SubscribeToEvent(UserInitiatedEvents.PauseToggle, OnPauseToggle);

            EventsPublisherTempleRun.Instance.SubscribeToEvent(GamePlayEvents.Pause, OnPause);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(GamePlayEvents.Resume, OnResume);
        }

        private void OnDestroy()
        {
            EventsPublisherUserInitiated.Instance.UnsubscribeToEvent(UserInitiatedEvents.PauseToggle, OnPauseToggle);

            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(GamePlayEvents.Pause, OnPause);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(GamePlayEvents.Resume, OnResume);
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
                EventsPublisherTempleRun.Instance.PublishEvent(GamePlayEvents.Resume, this, UnityEngine.Time.time);
            else
                EventsPublisherTempleRun.Instance.PublishEvent(GamePlayEvents.Pause, this, UnityEngine.Time.time);
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
