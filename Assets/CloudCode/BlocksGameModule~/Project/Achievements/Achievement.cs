using Newtonsoft.Json;

namespace BlocksGameModule.Achievements;

public class Achievement
{
    [JsonIgnore]
    public string Id => Definition?.Id;

    public AchievementDefinition Definition { get; set; }
    public AchievementRecord Record { get; set; }

    public Achievement(AchievementDefinition definition)
    {
        Definition = definition;
    }
}
