using System;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace BlocksGameModule.Leaderboards;

public partial class NumberGameClient
{
    Random m_Prng = new();
    LeaderboardClient m_LeaderboardClient;

    public NumberGameClient(IExecutionContext executionContext, IGameApiClient gameApiClient)
    {
        m_LeaderboardClient = new LeaderboardClient(executionContext, gameApiClient);
    }

    [CloudCodeFunction("RequestGoalAndScore")]
    public async Task<NumberGameResult> RequestGoalAndScore(string leaderboardId, int guess)
    {
        // This code is executed from the Cloud Code service
        // Generating the goal here prevents users from being able to cheat their score
        var goal = m_Prng.Next(0, 99);
        var score = 100 - Math.Abs(goal - guess);

        await m_LeaderboardClient.AddPlayerScoreAsync(leaderboardId, score);

        return new NumberGameResult(goal, score);
    }
}

public class NumberGameResult
{
    public int Goal { get; private set; }
    public int Score { get; private set; }

    public NumberGameResult(int goal, int score)
    {
        Goal = goal;
        Score = score;
    }
}


