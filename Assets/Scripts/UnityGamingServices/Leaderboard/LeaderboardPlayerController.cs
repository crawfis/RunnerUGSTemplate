//using Blocks.Leaderboards;

using Blocks.Leaderboards;

using CrawfisSoftware.Events;
using CrawfisSoftware.UGS.Events;

using System;
using System.Threading.Tasks;

using Unity.Mathematics;
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
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.ScoreUpdating, OnGameEnding);
            LeaderboardsObserver.Instance.LeaderboardId = LeaderboardId;
            LeaderboardsObserver.Instance.UseTrustedClient = _useTrustedClient;
        }
        private void OnDestroy()
        {
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.ScoreUpdating, OnGameEnding);
        }
        private void OnGameEnding(string eventName, object sender, object data)
        {
            IsUpdating = true;
            Type type = data.GetType();
            Debug.Log($"LeaderboardPlayerController: OnGameEnding called with data of type {type}");
            float? scoref = data as float?;
            long score = (int)scoref.Value;
            // Use an async lambda to handle the awaiting
            var _ = HandleScoreUpload(LeaderboardId, score);
            //var _ = LeaderboardsService.Instance.AddPlayerScoreAsync("DailyDistance", score);
            //EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.ScoreUpdated, this, null);
        }

        private async Task HandleScoreUpload(string leaderboardId, long score)
        {
            try
            {
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