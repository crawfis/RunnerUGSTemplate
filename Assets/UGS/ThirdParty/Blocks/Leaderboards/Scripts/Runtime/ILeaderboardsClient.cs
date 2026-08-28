using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Leaderboards.Models;

namespace Blocks.Leaderboards
{
    /// <summary>
    /// Interface for the leaderboard clients
    /// </summary>
    public interface ILeaderboardsClient
    {
        /// <summary>
        /// Get the scores from the leaderboard service
        /// </summary>
        /// <param name="leaderboardId">The id of the leaderboard</param>
        /// <param name="limit">The maximum number of entries to get for the leaderboard</param>
        /// <param name="isGlobal">Choose to fetch all scores or only player scores</param>
        /// <returns>List of LeaderboardEntry</returns>
        Task<List<LeaderboardEntry>> GetScoresAsync(string leaderboardId, int limit, bool isGlobal);

        /// <summary>
        /// Add the currently authenticated players score to the leaderboard
        /// </summary>
        /// <param name="leaderboardId">Leaderboard id to add score to</param>
        /// <param name="score">The score to add</param>
        /// <returns></returns>
        Task AddPlayerScoreAsync(string leaderboardId, double score);
    }
}
