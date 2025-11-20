using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudSave.Model;

namespace BlocksGameModule.Achievements;

public interface IAchievementApi
{
    public Task<List<Achievement>> GetAchievementsAsync(string playerId);
    public Task<AchievementRecord> UnlockAchievementAsync(string achievementId);
}

public class AchievementApi : IAchievementApi
{
    const string SettingsKey = "settings";
    const string AchievementsKey = "achievements";

    readonly IExecutionContext Context;
    readonly IGameApiClient Client;
    readonly List<string> Keys;

    public AchievementApi(IExecutionContext context, IGameApiClient client)
    {
        Context = context;
        Client = client;
        Keys = new List<string>() { AchievementsKey };
    }

    public async Task ResetAchievementAsync()
    {
        var result = await Client.CloudSaveData.DeleteProtectedItemAsync(Context, Context.ServiceToken, AchievementsKey, Context.ProjectId, Context.PlayerId!);
        if (!IsSuccessful(result.StatusCode))
        {
            throw new AchievementException($"Failed to reset achievements for player {Context.PlayerId}.");
        }
    }

    public async Task<List<Achievement>> GetAchievementsAsync(string playerId)
    {
        var definitionsTask = GetDefinitionsAsync();
        var recordsTask = GetPlayerRecordAsync(playerId);

        await Task.WhenAll(definitionsTask, recordsTask);

        var definitions = definitionsTask.Result;
        var records = recordsTask.Result;

        var achievements = new List<Achievement>();

        if (definitions != null)
        {
            foreach (var def in definitions)
            {
                var achievement = new Achievement(def);
                achievements.Add(achievement);

                var record = records?.FirstOrDefault(x => x.Id == def.Id);

                if (record != null)
                {
                    achievement.Record = record;
                }
                else
                {
                    achievement.Record = new AchievementRecord()
                    {
                        Id = def.Id
                    };
                }
            }
        }

        return achievements;
    }

    public async Task<AchievementRecord> UnlockAchievementAsync(string achievementId)
    {
        var playerRecords = await GetPlayerRecordAsync(Context.PlayerId);

        var record = playerRecords.FirstOrDefault(x => x.Id == achievementId);

        if (record == null)
        {
            record = new AchievementRecord()
            {
                Id = achievementId,
                Unlocked = true
            };

            playerRecords.Add(record);
        }
        else
        {
            record.Unlocked = true;
        }

        await UpdatePlayerRecordAsync(playerRecords);

        return record;
    }

    public async Task<AchievementRecord> UpdateAchievementProgressAsync(string achievementId, int count)
    {
        var playerRecords = await GetPlayerRecordAsync(Context.PlayerId);
        var record = playerRecords.FirstOrDefault(x => x.Id == achievementId);

        if (record == null)
        {
            record = new AchievementRecord()
            {
                Id = achievementId,
                Unlocked = false,
                ProgressCount = count
            };
        }
        else
        {
            record.ProgressCount += count;
        }

        await UpdatePlayerRecordAsync(playerRecords);

        return record;
    }

    static bool IsSuccessful(HttpStatusCode code)
    {
        return (int)code >= 200 && (int)code < 300;
    }

    async Task<List<AchievementRecord>> GetPlayerRecordAsync(string playerId)
    {
        var result = await Client.CloudSaveData.GetProtectedItemsAsync(
            Context, Context.ServiceToken, Context.ProjectId, playerId, Keys);
        if (IsSuccessful(result.StatusCode))
        {
            var value = result.Data?.Results?.FirstOrDefault()?.Value;

            if (value == null)
            {
                return new List<AchievementRecord>();
            }

            var contents = value.ToString();

            if (!string.IsNullOrEmpty(contents))
            {
                return JsonConvert.DeserializeObject<List<AchievementRecord>>(contents);
            }
            else
            {
                return new List<AchievementRecord>();
            }
        }

        throw new AchievementException("Failed to load achievement records from cloud save.");
    }

    async Task UpdatePlayerRecordAsync(List<AchievementRecord> record)
    {
        var json = JsonConvert.SerializeObject(record, Formatting.Indented);
        var body = new SetItemBody(AchievementsKey, json);
        var result = await Client.CloudSaveData.SetProtectedItemAsync(
            Context, Context.ServiceToken, Context.ProjectId, Context.PlayerId, body);

        if (!IsSuccessful(result.StatusCode))
        {
            throw new AchievementException("Failed to update achievement record in cloud save.");
        }
    }

    async Task<List<AchievementDefinition>> GetDefinitionsAsync()
    {
        var result = await Client.RemoteConfigSettings.AssignSettingsGetAsync(
            Context, Context.AccessToken, Context.ProjectId, Context.EnvironmentId, SettingsKey, Keys);

        if (IsSuccessful(result.StatusCode))
        {
            var settingsDictionary = result.Data.Configs?.Settings;

            if (settingsDictionary != null && settingsDictionary.Count > 0)
            {
                var value = settingsDictionary.FirstOrDefault().Value?.ToString();

                if (!string.IsNullOrEmpty(value))
                {
                    return JsonConvert.DeserializeObject<List<AchievementDefinition>>(value);
                }
                else
                {
                    return new List<AchievementDefinition>();
                }
            }
            else
            {
                return new List<AchievementDefinition>();
            }
        }

        throw new AchievementException("Failed to load achievement definitions from remote config.");
    }
}
