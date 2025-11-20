using System.Collections.Generic;
using Newtonsoft.Json;

namespace Blocks.Achievements.Editor
{
    class RemoteConfigFileContent
    {
        [JsonIgnore]
        public AchievementSettings AchievementSettings => entries.ContainsKey(Constants.AchievementsKey) ? entries[Constants.AchievementsKey] : null;

        [JsonProperty("$schema")]
        public string schema = "https://ugs-config-schemas.unity3d.com/v1/remote-config.schema.json";
        public Dictionary<string, AchievementSettings> entries;
        public Dictionary<string, string> types;

        public RemoteConfigFileContent(AchievementSettings settings)
        {
            entries = new Dictionary<string, AchievementSettings>()
            {
                { Constants.AchievementsKey, settings }
            };
            types = new Dictionary<string, string>()
            {
                { Constants.AchievementsKey, Constants.JsonType }
            };
        }
    }
}
