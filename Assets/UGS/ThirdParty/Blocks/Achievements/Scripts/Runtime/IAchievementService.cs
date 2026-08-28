using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blocks.Achievements
{
    /// <summary>
    /// Contract for achievement clients.
    /// Use these clients for interacting with the achievements such as fetching, updating, unlocking.
    /// </summary>
    public interface IAchievementService
    {
        /// <summary>
        /// Get achievements for a player
        /// </summary>
        /// <param name="playerId">The id of the player whose achievements are fetched</param>
        /// <returns>The list of achievements</returns>
        Task<List<Achievement>> GetAchievementsAsync(string playerId);
        /// <summary>
        /// Unlock an achievement
        /// </summary>
        /// <param name="achievement">The achievement to unlock</param>
        Task<bool> UnlockAchievementAsync(Achievement achievement);
        /// <summary>
        /// Reset all achievements
        /// </summary>
        Task ResetAllAchievementsAsync();
        /// <summary>
        /// Update the progress for an achievement
        /// </summary>
        /// <param name="achievement">The achievement to update</param>
        /// <param name="count">The value to set the progress</param>
        Task UpdateAchievementProgressAsync(Achievement achievement, int count);
    }
}
