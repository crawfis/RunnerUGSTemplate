using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.Leaderboards.Model;

namespace BlocksAdminModule;

public partial class LeaderboardAdminFunctions
{
    IGameApiClient m_ApiClient;
    Random m_Prng;
    public LeaderboardAdminFunctions(IGameApiClient client)
    {
        m_ApiClient = client;
        m_Prng = new Random();
    }

    [CloudCodeFunction("SetupFakeEntries")]
    public async Task<string> SetupFakeEntries(IExecutionContext ctx, IGameApiClient client, string lbName, int count, int min, int max)
    {
        var leaderboardsClient = client.Leaderboards;
        var projectGuid = Guid.Parse(ctx.ProjectId);

        var tasks = new List<Task>(count);
        for (int i = 0; i < count; ++i)
        {
            var generatedPlayerId = string.Join("", Enumerable.Range(0, 9).Select(_ => (char)m_Prng.Next('A', 'Z' + 1)));
            var generatedScore = m_Prng.Next(min, max + 1);
            var addTask = leaderboardsClient.AddLeaderboardPlayerScoreAsync(
                ctx,
                ctx.ServiceToken,
                projectGuid,
                lbName,
                generatedPlayerId,
                new LeaderboardScore(generatedScore));
            tasks.Add(addTask);
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            ManageExceptions(tasks);
        }

        return $"Generated {count} entries!";
    }

    static void ManageExceptions(List<Task> tasks)
    {
        var firstError = 0;
        var errors = new List<string>();
        for (int i = 0; i < tasks.Count; ++i)
        {
            var exception = tasks[i].Exception;
            if (exception != null)
            {
                errors.Add($"{exception.GetType().Name} - {exception.Message}");
                if (firstError == 0)
                    firstError = i + 1;
            }
        }

        throw new Exception(
            $"One or more updates failed after {firstError}. Messages: {(string.Join("\n", errors))}. ");
    }
}


