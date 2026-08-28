using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.CloudSave.Models.Data.Player;
using Unity.Services.RemoteConfig;
using UnityEngine;

namespace Blocks.Achievements
{
    /// <summary>
    /// The local client for interfacing with the achievements
    /// </summary>
    public class LocalAchievementClient : IAchievementService
    {
        IAuthenticationService m_Authentication;
        ICloudSaveService m_CloudSave;
        RemoteConfigService m_RemoteConfig;

        /// <summary>
        /// ctor for the local client
        /// </summary>
        /// <param name="authentication">The Authentication service instance</param>
        /// <param name="cloudSave">The Cloud Save service instance</param>
        /// <param name="remoteConfig">The Remote Config service instance</param>
        public LocalAchievementClient(
            IAuthenticationService authentication,
            ICloudSaveService cloudSave,
            RemoteConfigService remoteConfig)
        {
            m_Authentication = authentication;
            m_CloudSave = cloudSave;
            m_RemoteConfig = remoteConfig;
        }

        /// <inheritdoc/>
        public virtual async Task ResetAllAchievementsAsync()
        {
            try
            {
                await m_CloudSave.Data.Player.DeleteAsync(
                    "achievements",
                    new Unity.Services.CloudSave.Models.Data.Player.DeleteOptions(new PublicWriteAccessClassOptions()));
            }
            catch (Exception e)
            {
                LogException(e);
            }
        }

        async Task<List<AchievementDefinition>> GetDefinitions()
        {
            var configs = await m_RemoteConfig.FetchConfigsAsync(new EmptyStruct(), new EmptyStruct());
            var achievementsJobject = configs.config["achievements"];
            return achievementsJobject.ToObject<List<AchievementDefinition>>();
        }

        /// <inheritdoc/>
        public virtual async Task<List<Achievement>> GetAchievementsAsync(string playerId)
        {
            var definitions = await GetDefinitions();
            var achievementRecords = await GetAchievementRecords(playerId);
            var playerAchievements = new List<Achievement>();
            foreach (var def in definitions)
            {
                var achievement = new Achievement(def);
                playerAchievements.Add(achievement);

                var record = achievementRecords?.FirstOrDefault(x => x.Id == def.Id);

                if (record != null)
                    achievement.Record = record;
                else
                    achievement.Record = new AchievementRecord { Id = def.Id };
            }

            return playerAchievements;
        }

        async Task<List<AchievementRecord>> GetAchievementRecords(string playerId)
        {
            var achievementsRaw =
                await m_CloudSave.Data.Player.LoadAsync((new[] { "achievements" }).ToHashSet(),
                    new LoadOptions(new PublicReadAccessClassOptions(playerId)));

            Item achievementsRecord;
            if (!achievementsRaw.TryGetValue("achievements", out achievementsRecord))
                return new List<AchievementRecord>();

            var achievementRecordsStr = achievementsRecord
                .Value.GetAsString();
            var achievementRecords = JsonConvert.DeserializeObject<List<AchievementRecord>>(achievementRecordsStr);
            return achievementRecords;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> UnlockAchievementAsync(Achievement achievement)
        {
            var achievementRecords = await GetAchievementRecords(m_Authentication.PlayerId);
            var record = achievementRecords.FirstOrDefault(r => r.Id == achievement.Id);
            if (record != null)
            {
                record.Update(true);
            }
            else
            {
                record = new AchievementRecord(achievement.Id, true);
                achievementRecords.Add(record);
            }

            try
            {
                await m_CloudSave.Data.Player.SaveAsync(
                    new Dictionary<string, SaveItem>()
                    {
                        { "achievements", new SaveItem(JsonConvert.SerializeObject(achievementRecords), null) }
                    }, new Unity.Services.CloudSave.Models.Data.Player.SaveOptions(new PublicWriteAccessClassOptions()));

                achievement.Record = record;
            }
            catch (Exception e)
            {
                LogException(e);
                return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public virtual async Task UpdateAchievementProgressAsync(Achievement achievement, int count)
        {
            var  achievementRecords = await GetAchievementRecords(m_Authentication.PlayerId);
            var record = achievementRecords.FirstOrDefault(r => r.Id == achievement.Id);
            if (record != null)
            {
                record.ProgressCount = count;
            }
            else
            {
                record = new AchievementRecord() { Id = achievement.Id, ProgressCount = count };
                achievementRecords.Add(record);
            }

            try
            {
                await m_CloudSave.Data.Player.SaveAsync(
                    new Dictionary<string, SaveItem>()
                    {
                        { "achievements", new SaveItem(JsonConvert.SerializeObject(achievementRecords), null) }
                    }, new Unity.Services.CloudSave.Models.Data.Player.SaveOptions(new PublicWriteAccessClassOptions()));

                achievement.Record = record;
            }
            catch (Exception e)
            {
                LogException(e);
            }
        }

        static void LogException(Exception e)
        {
            if (e.InnerException != null && e.InnerException.Message.Contains("403"))
            {
                Debug.LogError("This client has been denied access to writing to Cloud Save. You can either `Delete Remote` in the Deployment window context menu by right clicking the `AchievementsAccessControl.ac` or you can use the trusted achievement client.");
            }
            else
            {
                Debug.LogError(e.Message);
            }
        }

        struct EmptyStruct {}
    }
}
