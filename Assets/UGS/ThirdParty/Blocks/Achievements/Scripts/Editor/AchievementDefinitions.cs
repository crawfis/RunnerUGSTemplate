using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks.Achievements.Editor
{
    [Serializable]
    class AchievementDefinitions : ScriptableObject
    {
        public List<AchievementDefinition> Achievements = new List<AchievementDefinition>();

        public void CopyFrom(AchievementDefinitions other)
        {
            Achievements = new  List<AchievementDefinition>(other.Achievements);
        }
    }
}
