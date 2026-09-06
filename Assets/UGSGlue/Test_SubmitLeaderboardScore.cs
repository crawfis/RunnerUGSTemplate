using CrawfisSoftware.Events;
using CrawfisSoftware.GameFlow.Events;

using UnityEngine;

using GameFlowBus = CrawfisSoftware.Events.EventsFor<CrawfisSoftware.GameFlow.Events.GameFlowEvents>;

using CrawfisSoftware.Contracts;
using GameServiceBus = CrawfisSoftware.Events.EventsFor<CrawfisSoftware.Contracts.GameServiceEvents>;

namespace CrawfisSoftware.UGS.Leaderboard.Test
{
    public class Test_SubmitLeaderboardScore : MonoBehaviour
    {
        [SerializeField] private float _minValue = 10;
        [SerializeField] private float _maxValue = 300;
        [SerializeField] private int _numberOfTimesToSubmit = 2;
        [SerializeField] private int _initialDelayInSeconds = 1;
        [SerializeField] private int _delayBetweenSubmissionsInSeconds = 2;
        [SerializeField] private bool _endGameAfterSubmissions = true;
        void Start()
        {
            GameFlowBus.Subscribe(GameFlowEvents.GameStarted, OnGameStarted);
        }

        private void OnDestroy()
        {
            GameFlowBus.Unsubscribe(GameFlowEvents.GameStarted, OnGameStarted);
        }
        private void OnGameStarted(string eventName, object sender, object data)
        {
            _ = SubmitScores();
        }

        private async Awaitable SubmitScores()
        {
            await Awaitable.WaitForSecondsAsync(_initialDelayInSeconds, destroyCancellationToken);
            for (int i = 0; i < _numberOfTimesToSubmit; i++)
            {
                float randomScore = UnityEngine.Random.Range((int)_minValue, (int)_maxValue + 1);
                // Publishes the contract event a real game would, so this harness exercises
                // the same path as gameplay rather than a UGS-internal shortcut.
                GameServiceBus.Publish(GameServiceEvents.SessionEnding, this, randomScore);
                await Awaitable.WaitForSecondsAsync(_delayBetweenSubmissionsInSeconds, destroyCancellationToken);
            }
            if(_endGameAfterSubmissions)
            {
                Debug.Log("All scores submitted. Ending game.");
                GameFlowBus.Publish(GameFlowEvents.GameEnded, this, null);
            }
        }
    }
}