//using Blocks.Leaderboards;

using Blocks.Leaderboards;

using CrawfisSoftware.TempleRun;

using System;
using System.Threading.Tasks;

using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;

using UnityEngine;

namespace CrawfisSoftware.UGS.Leaderboard
{
    public class LeaderboardPlayerController : MonoBehaviour
    {
        [SerializeField] private string LeaderboardId = "DailyDistance";
        [SerializeField] private bool _useTrustedClient = false;

        public bool IsUpdating { get; private set; } = false;
        private void Start()
        {
            EventsPublisherTempleRun.Instance.SubscribeToEvent(TempleRunEvents.PlayerDied, OnPlayerDied);
            LeaderboardsObserver.Instance.LeaderboardId = LeaderboardId;
            LeaderboardsObserver.Instance.UseTrustedClient = _useTrustedClient;
        }
        private void OnDestroy()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(TempleRunEvents.PlayerDied, OnPlayerDied);
        }
        private void OnPlayerDied(string eventName, object sender, object data)
        {
            IsUpdating = true;
            long score = (long)Blackboard.Instance.DistanceTracker.DistanceTravelled;
            // Use an async lambda to handle the awaiting
            var _ = HandleScoreUpload(LeaderboardId, score);
            //var _ = LeaderboardsService.Instance.AddPlayerScoreAsync("DailyDistance", score);
            //EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.ScoreUpdated, this, null);
        }

        private async Task HandleScoreUpload(string leaderboardId, long score)
        {
            try
            {
                EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.ScoreUpdating, this, score);
                var playerEntry = await AddPlayerScore(leaderboardId, score);
                UnityEngine.Debug.Log($"Score {score} uploaded successfully! Player rank: {playerEntry.Rank}");
                EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.ScoreUpdated, this, (playerEntry.PlayerName, playerEntry.Score, playerEntry.PlayerId));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to upload score: {e}");
                EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.ScoreFailedToUpdate, this, leaderboardId);
            }
            finally
            {
                IsUpdating = false;
            }
        }

        public async Task<LeaderboardEntry> AddPlayerScore(string leaderboardId, long score)
        {
            try
            {
                var playerEntry = await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
                return playerEntry;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
                throw; // Re-throw so calling code can handle it
            }
        }
    }
}