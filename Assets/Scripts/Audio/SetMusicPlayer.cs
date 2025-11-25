using CrawfisSoftware.Events;

using GTMY.Audio;

using System;

using UnityEngine;

namespace CrawfisSoftware.TempleRun.Audio
{
    [RequireComponent(typeof(MusicPlayerExplicit))]
    internal class SetMusicPlayer : MonoBehaviour
    {
        [SerializeField] private MusicPlayerExplicit _musicPlayer;
        [SerializeField] private float _initialVolume = 0.5f;
        private void Awake()
        {
            AudioManagerSingleton.Instance.SetMusicPlayer(_musicPlayer);
            _musicPlayer.Volume = _initialVolume;
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameStarted, OnGameStarted);
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameEnding, OnGameOver);
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.Pause, OnPause);
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.Resume, OnResume);
        }

        private void OnPause(string eventName, object sender, object data)
        {
            AudioManagerSingleton.Instance.Music.Pause();
            _musicPlayer.Pause();
        }

        private void OnResume(string eventName, object sender, object data)
        {
            AudioManagerSingleton.Instance.Music.UnPause();
            _musicPlayer.UnPause();
        }

        private void OnDestroy()
        {
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameStarted, OnGameStarted);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameEnding, OnGameOver);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.Pause, OnPause);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.Resume, OnResume);
        }

        private void OnGameStarted(string eventName, object sender, object data)
        {
            _musicPlayer.Play();
        }

        private void OnGameOver(string eventName, object sender, object data)
        {
            _musicPlayer.Stop();
        }
    }
}