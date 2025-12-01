using CrawfisSoftware.GameConfig;

using System.Collections;
using System.Threading.Tasks;

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
            //EventsPublisherTempleRun.Instance.SubscribeToEvent(GamePlayEvents.GameScenesUnloaded, OnGameOver);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.LeaderboardOpening, LoadLeaderboard);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.ScoreUpdating, OnScoreUpdating);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.ScoreUpdated, OnScoreUpdated);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.ScoreFailedToUpdate, OnScoreUpdateFailed);
        }

        private void OnDestroy()
        {
            //EventsPublisherTempleRun.Instance.UnsubscribeToEvent(GamePlayEvents.GameScenesUnloaded, OnGameOver);
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.LeaderboardOpening, LoadLeaderboard);
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.ScoreUpdating, OnScoreUpdating);
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.ScoreUpdated, OnScoreUpdated);
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.ScoreFailedToUpdate, OnScoreUpdateFailed);
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

        private void OnScoreUpdateFailed(string eventName, object sender, object data)
        {
            Debug.LogWarning("LeaderboardController: Score update failed.");
        }

        private void LoadLeaderboard(string eventName, object sender, object data)
        {
            SceneManager.sceneLoaded += OnLeaderboardSceneLoaded;
            SceneManager.LoadSceneAsync(_sceneToLoad, LoadSceneMode.Additive);
        }

        private void OnLeaderboardSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.name != _sceneToLoad) return;

            SceneManager.sceneLoaded -= OnLeaderboardSceneLoaded;
            EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.LeaderboardOpened, this, LeaderboardId);
            StartCoroutine(CloseLeaderboardAfterDelay());
        }

        private IEnumerator CloseLeaderboardAfterDelay()
        {
            yield return new WaitUntil(() => !_isUpdating);
            yield return new WaitForSeconds(GameConstants.LeaderboardDisplayTime);
            EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.LeaderboardClosing, this, LeaderboardId);
        }
    }
}