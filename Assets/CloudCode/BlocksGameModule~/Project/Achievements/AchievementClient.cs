using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace BlocksGameModule.Achievements;

public partial class AchievementClient
{
    /// <summary>
    /// Retrieves a player's achievements
    /// </summary>
    /// <param name="context">The execution context</param>
    /// <param name="client">The UGS api client</param>
    /// <param name="playerId">The player's id to fetch achievements for</param>
    /// <returns>List of achievements for the specified player</returns>
    [CloudCodeFunction("GetAchievements")]
    public Task<List<Achievement>> GetAchievementsAsync(IExecutionContext context, IGameApiClient client, string playerId)
    {
        var api = new AchievementApi(context, client);
        return api.GetAchievementsAsync(playerId);
    }

    /// <summary>
    /// Unlock an achievement for the current player
    /// </summary>
    /// <param name="context">The execution context</param>
    /// <param name="client">The UGS api client</param>
    /// <param name="achievementId">The achievement id</param>
    /// <returns>The achievement record for the achievement id</returns>
    [CloudCodeFunction("UnlockAchievement")]
    public Task<AchievementRecord> UnlockAchievementAsync(IExecutionContext context, IGameApiClient client, string achievementId)
    {
        var api = new AchievementApi(context, client);
        return api.UnlockAchievementAsync(achievementId);
    }

    /// <summary>
    /// Reset all achievement progress for the current player
    /// </summary>
    /// <param name="context">The execution context</param>
    /// <param name="client">The UGS api client</param>
    [CloudCodeFunction("ResetAllAchievements")]
    public Task ResetAllAchievementsAsync(IExecutionContext context, IGameApiClient client)
    {
        var api = new AchievementApi(context, client);
        return api.ResetAchievementAsync();
    }

    /// <summary>
    /// Update the progress of an achievement
    /// </summary>
    /// <param name="context">The execution context</param>
    /// <param name="client">The UGS api client</param>
    /// <param name="achievementId">The achievement id</param>
    /// <param name="count">The current progress of the achievement</param>
    [CloudCodeFunction("UpdateAchievementProgress")]
    public Task<AchievementRecord> UpdateAchievementProgressAsync(IExecutionContext context, IGameApiClient client, string achievementId, int count)
    {
        var api = new AchievementApi(context, client);
        return api.UpdateAchievementProgressAsync(achievementId, count);
    }
}
