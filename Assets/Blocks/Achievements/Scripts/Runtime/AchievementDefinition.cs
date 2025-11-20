using System;
using UnityEngine;

using ClientDefinition = Unity.Services.CloudCode.GeneratedBindings.BlocksGameModule.Achievements.AchievementDefinition;

namespace Blocks.Achievements
{
    /// <summary>
    /// The model representation of an achievement
    /// </summary>
    [Serializable]
    public class AchievementDefinition
    {
        /// <summary>
        /// Id of the achievement matched in AchievementRecord
        /// </summary>
        public string Id;
        /// <summary>
        /// Icon to display
        /// </summary>
        public string Icon;
        /// <summary>
        /// Title of the achievement
        /// </summary>
        public string Title;
        /// <summary>
        /// Description of the achievement
        /// </summary>
        public string Description;
        /// <summary>
        /// Determines whether the achievement title and description should be hidden while locked
        /// </summary>
        public bool IsHidden;
        /// <summary>
        /// Optional way to define achievements that are multi-staged
        /// Values of 1 or less are ignored and considered to have no progress
        /// </summary>
        public int ProgressTarget;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AchievementDefinition() { }

        /// <summary>
        /// ctor using model from the cloud code client
        /// </summary>
        /// <param name="clientDefinition">model from the cloud code module</param>
        public AchievementDefinition(ClientDefinition clientDefinition)
        {
            Id = clientDefinition.Id;
            Icon = clientDefinition.Icon;
            Title = clientDefinition.Title;
            Description = clientDefinition.Description;
            IsHidden = clientDefinition.IsHidden;
            ProgressTarget = clientDefinition.ProgressTarget;
        }
    }
}
