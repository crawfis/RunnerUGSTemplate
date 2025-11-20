namespace BlocksGameModule.Achievements;

public class AchievementDefinition
{
    public string Icon { get; set; }
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsHidden { get; set; }
    public int ProgressTarget { get; set; }
}
