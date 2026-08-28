using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using Unity.Services.CloudSave;
using Unity.Services.RemoteConfig;
using UnityEngine;

namespace Blocks.Achievements
{
    /// <summary>
    /// The client which uses Cloud Code Modules to interface with achievements
    /// The module can be found at `Assets/CloudCode/KitsGameModule~`
    /// </summary>
    public class TrustedAchievementClient : LocalAchievementClient
    {
        AchievementClientBindings m_AchievementClientBindings;

        /// <summary>
        /// ctor for the TrustedAchievementService
        /// </summary>
        /// <param name="authentication">The Authentication service instance</param>
        /// <param name="cloudCode">The Cloud Code service instance</param>
        /// <param name="cloudSave">The Cloud Save service instance</param>
        /// <param name="remoteConfig">The Remote Config service instance</param>
        public TrustedAchievementClient(
            IAuthenticationService authentication,
            ICloudCodeService cloudCode,
            ICloudSaveService cloudSave,
            RemoteConfigService remoteConfig)
            : base(authentication, cloudSave, remoteConfig)
        {
            m_AchievementClientBindings = new AchievementClientBindings(cloudCode);
        }

        /// <inheritdoc/>
        public override async Task ResetAllAchievementsAsync()
        {
            await m_AchievementClientBindings.ResetAllAchievements();
        }

        /// <inheritdoc/>
        public override async Task<List<Achievement>> GetAchievementsAsync(string playerId)
        {
            var clientAchievements = await m_AchievementClientBindings.GetAchievements(playerId);
            return clientAchievements.Select(clientAchievement => new Achievement(clientAchievement)).ToList();
        }

        /// <inheritdoc/>
        public override async Task<bool> UnlockAchievementAsync(Achievement achievement)
        {
            try
            {
                var clientRecord = await m_AchievementClientBindings.UnlockAchievement(achievement.Id);
                achievement.Record = new AchievementRecord(clientRecord);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override async Task UpdateAchievementProgressAsync(Achievement achievement, int count)
        {
            var clientRecord = await m_AchievementClientBindings.UpdateAchievementProgress(achievement.Id, count);
            achievement.Record = new AchievementRecord(clientRecord);
        }
    }
}
