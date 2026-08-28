using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

namespace Blocks.Leaderboards
{
    /// <summary>
    /// The local leaderboard client.
    /// Assumes full authority to read and write to the leaderboard service.
    /// </summary>
    class LocalLeaderboardsClient : ILeaderboardsClient
    {
        ILeaderboardsService m_LeaderboardsService;

        /// <summary>
        /// The ctor for the client
        /// </summary>
        /// <param name="leaderboardsService">The leaderboard service</param>
        public LocalLeaderboardsClient(ILeaderboardsService  leaderboardsService)
        {
            m_LeaderboardsService = leaderboardsService;
        }

        /// <inheritdoc/>
        public virtual async Task<List<LeaderboardEntry>> GetScoresAsync(string leaderboardId, int limit, bool isGlobal)
        {
            try
            {
                return isGlobal
                    ? (await m_LeaderboardsService.GetScoresAsync(leaderboardId, new GetScoresOptions() { Limit = limit })).Results
                    : (await m_LeaderboardsService.GetPlayerRangeAsync(leaderboardId, new GetPlayerRangeOptions() { RangeLimit = limit })).Results;
            }
            catch (LeaderboardsException e)
            {
                if (e.Reason == LeaderboardsExceptionReason.LeaderboardNotFound)
                {
                    Debug.LogError(
                        $"The leaderboard '{leaderboardId}' was not found, make sure the name is correct and it has been deployed.");
                }
                else if (e.Reason == LeaderboardsExceptionReason.EntryNotFound)
                {
                    Debug.LogWarning($"The entry was not found, make sure the player has sent their record.");
                }
                else
                {
                    Debug.LogError($"Error while fetching leaderboard entries for '{leaderboardId}': {e}");
                }
            }

            return new List<LeaderboardEntry>();
        }

        /// <inheritdoc/>
        public virtual async Task AddPlayerScoreAsync(string leaderboardId, double score)
        {
            await m_LeaderboardsService.AddPlayerScoreAsync(leaderboardId, score);
        }
    }
}
