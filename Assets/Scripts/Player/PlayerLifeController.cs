using System;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Manages the number of lives a player has, converting the PlayerFailed event to a PlayerDied event when 
    /// all of the lives run out.
    ///    Dependencies: EventsPublisherTempleRun
    ///    Subscribes: PlayerFailed event
    ///    Publishes: PlayerDied event. Data can be a player id.
    /// </summary>
    internal class PlayerLifeController : IDisposable
    {
        private int _numberOfLives;
        private readonly int _playerID;

        public PlayerLifeController(int numberOfLives, int playerID = 0)
        {
            _numberOfLives = numberOfLives;
            _playerID = playerID;
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.PlayerFailing, OnPlayerFailed);
        }

        private void OnPlayerFailed(string eventName, object sender, object data)
        {
            // Todo: Check playerID
            _numberOfLives--;
            if (_numberOfLives <= 0)
            {
                EventsPublisherTempleRun.Instance.PublishEvent(TempleRunEvents.PlayerDied, this, _playerID);
                EventsPublisherTempleRun.Instance.PublishEvent(TempleRunEvents.PlayerResume, this, UnityEngine.Time.time);
            }
        }

        public void Dispose()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.PlayerFailing, OnPlayerFailed);
        }
    }
}