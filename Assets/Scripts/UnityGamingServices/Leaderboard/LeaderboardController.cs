using CrawfisSoftware.TempleRun;
using CrawfisSoftware.GameConfig;

using System;
using System.Collections;
using System.Threading.Tasks;

using Unity.Services.Leaderboards;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrawfisSoftware.UGS.Leaderboard
{
    internal class LeaderboardController : MonoBehaviour
    {
        [SerializeField] private string LeaderboardId = "DailyDistance";
        [SerializeField] private string _tier;
        [SerializeField] private int _numberToDisplay;
        [SerializeField] private string _sceneToLoad;

        private bool _isUpdating = false;

        // TCS to signal when a score update finishes (success or failure)
        private TaskCompletionSource<bool> _scoreUpdatedTcs;

        private void Start()
        {
            EventsPublisherTempleRun.Instance.SubscribeToEvent(GamePlayEvents.GameScenesUnloaded, OnGameOver);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.ScoreUpdating, OnScoreUpdating);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.ScoreUpdated, OnScoreUpdated);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.ScoreFailedToUpdate, OnScoreUpdated);
        }

        private void OnDestroy()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(GamePlayEvents.GameScenesUnloaded, OnGameOver);
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.ScoreUpdating, OnScoreUpdating);
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.ScoreUpdated, OnScoreUpdated);
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.ScoreFailedToUpdate, OnScoreUpdated);
        }

        private void OnScoreUpdating(string eventName, object sender, object data)
        {
            _isUpdating = true;
            // Create a fresh TCS for the new update cycle
            _scoreUpdatedTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private void OnScoreUpdated(string eventName, object sender, object data)
        {
            _isUpdating = false;
            // Signal anyone awaiting the update completion
            _scoreUpdatedTcs?.TrySetResult(true);
        }

        private void OnGameOver(string eventName, object sender, object data)
        {
            SceneManager.LoadSceneAsync(_sceneToLoad, LoadSceneMode.Additive);
            StartCoroutine(CloseLeaderboard());
            //_ = ShowLeaderboard();
        }

        private IEnumerator CloseLeaderboard()
        {
            yield return new WaitForSeconds(GameConstants.LeaderboardDisplayTime);
            EventsPublisherTempleRun.Instance.PublishEvent(GamePlayEvents.LeaderboardClosing, this, UnityEngine.Time.time);
        }

        private async Task ShowLeaderboard()
        {
            try
            {
                // Fetch Leaderboard scores
                var tierOptions = new GetScoresByTierOptions()
                {
                    Limit = _numberToDisplay
                };
                var options = new GetScoresOptions()
                {
                    Limit = _numberToDisplay
                };

                // If an update is in progress, await the event instead of polling
                var tcs = _scoreUpdatedTcs;
                if (_isUpdating && tcs != null)
                {
                    await tcs.Task;
                }

                //var page = await LeaderboardsService.Instance.GetScoresByTierAsync("WeeklyDistance", _tier, tierOptions);
                var page = await LeaderboardsService.Instance.GetScoresAsync(LeaderboardId, options);

                int position = 1;
                foreach (var item in page.Results)
                {
                    Debug.Log($"{position++}, {item.PlayerName},  {item.Score}");
                }

                EventsPublisherTempleRun.Instance.PublishEvent(GamePlayEvents.LeaderboardDisplayed, this, UnityEngine.Time.time);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }
    }
}