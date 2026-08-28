using System;
using Newtonsoft.Json;
using UnityEngine;

using ClientAchievement = Unity.Services.CloudCode.GeneratedBindings.BlocksGameModule.Achievements.Achievement;

namespace Blocks.Achievements
{
    /// <summary>
    /// The combined representation of the data model and the state of the achievement
    /// </summary>
    public class Achievement
    {
        [JsonIgnore]
        public string Id => Definition?.Id;

        /// <summary>
        /// Data model defining the achievement
        /// </summary>
        public AchievementDefinition Definition { get; set; }

        /// <summary>
        /// The current state of the achievement
        /// </summary>
        public AchievementRecord Record { get; set; }

        /// <summary>
        /// Default ctor
        /// </summary>
        public Achievement()
        {
        }

        /// <summary>
        /// ctor using the achievement from the cloud code module
        /// </summary>
        /// <param name="clientAchievement"></param>
        public Achievement(ClientAchievement clientAchievement)
        {
            Definition = new AchievementDefinition(clientAchievement.Definition);
            Record = new AchievementRecord(clientAchievement.Record);
        }

        /// <summary>
        /// ctor for achievements without a record
        /// </summary>
        /// <param name="definition">definition of the achievement</param>
        public Achievement(AchievementDefinition definition)
        {
            Definition = definition;
        }
    }
}
