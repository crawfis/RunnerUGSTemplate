using System;

using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    public class GameTime : MonoBehaviour
    {
        public static GameTime Instance { get; private set; }

        private float _timeScale = 1f;
        public float deltaTime
        {
            get
            {
                return _timeScale * UnityEngine.Time.deltaTime;
            }
        }

        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(Instance);
            }
            Instance = this;

            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.PlayerPause, OnPlayerPause);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.PlayerResume, OnPlayerResume);
        }

        private void OnPlayerPause(string arg1, object arg2, object arg3)
        {
            _timeScale = 0f;
        }

        private void OnPlayerResume(string arg1, object arg2, object arg3)
        {
            _timeScale = 1f;
        }
    }
}