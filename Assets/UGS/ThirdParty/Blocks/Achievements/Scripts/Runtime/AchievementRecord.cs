using System;

using ClientRecord = Unity.Services.CloudCode.GeneratedBindings.BlocksGameModule.Achievements.AchievementRecord;

namespace Blocks.Achievements
{
    /// <summary>
    /// Player record for a given achievement.
    /// Persisted in CloudSave.
    /// </summary>
    public class AchievementRecord
    {
        /// <summary>
        /// Invoked when Unlocked value is changed
        /// </summary>
        public event Action Changed;

        /// <summary>
        /// The Id of the achievement matched in AchievementDefinition
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// State of the achievement
        /// </summary>
        public bool Unlocked { get; private set; }
        /// <summary>
        /// Current progress of the achievement if multi-staged
        /// </summary>
        public int ProgressCount { get; set; }

        /// <summary>
        /// Default ctor
        /// </summary>
        public AchievementRecord() { }

        public AchievementRecord(string id, bool unlocked)
        {
            Id = id;
            Unlocked = unlocked;
        }

        /// <summary>
        /// ctor using model from cloud code module
        /// </summary>
        /// <param name="clientRecord">model from cloud code module</param>
        public AchievementRecord(ClientRecord clientRecord)
        {
            Id = clientRecord.Id;
            Unlocked = clientRecord.Unlocked;
            ProgressCount = clientRecord.ProgressCount;
        }

        /// <summary>
        /// Update the state of the record
        /// </summary>
        /// <param name="unlocked"></param>
        public void Update(bool unlocked)
        {
            if (Unlocked != unlocked)
            {
                Unlocked = unlocked;
                Changed?.Invoke();
            }
        }
    }
}
