using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.Leaderboards.Model;

namespace BlocksGameModule.Leaderboards;

public partial class LeaderboardClient
{
    IExecutionContext m_ExecutionContext;
    IGameApiClient m_GameApiClient;

    public LeaderboardClient(IExecutionContext executionContext, IGameApiClient gameApiClient)
    {
        m_ExecutionContext = executionContext;
        m_GameApiClient = gameApiClient;
    }

    [CloudCodeFunction("GetScoresAsync")]
    public async Task<List<LeaderboardEntry>> GetScoresAsync(string leaderboardId, int limitParam, bool isGlobal)
    {
        return isGlobal
            ? (await m_GameApiClient.Leaderboards.GetLeaderboardScoresAsync(
                m_ExecutionContext,
                m_ExecutionContext.AccessToken,
                Guid.Parse(m_ExecutionContext.ProjectId),
                leaderboardId,
                limit : limitParam)).Data.Results
            : (await m_GameApiClient.Leaderboards.GetLeaderboardScoresPlayerRangeAsync(
                m_ExecutionContext,
                m_ExecutionContext.AccessToken,
                Guid.Parse(m_ExecutionContext.ProjectId),
                leaderboardId,
                m_ExecutionContext.PlayerId,
                rangeLimit: limitParam)).Data.Results;
    }

    [CloudCodeFunction("AddPlayerScoreAsync")]
    public async Task AddPlayerScoreAsync(string leaderboardId, double score)
    {
        await m_GameApiClient.Leaderboards.AddLeaderboardPlayerScoreAsync(
            m_ExecutionContext,
            m_ExecutionContext.ServiceToken,
            Guid.Parse(m_ExecutionContext.ProjectId),
            leaderboardId,
            m_ExecutionContext.PlayerId,
            new LeaderboardScore(score));
    }
}
