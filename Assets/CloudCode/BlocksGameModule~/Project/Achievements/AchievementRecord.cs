namespace BlocksGameModule.Achievements;

/// <summary>
/// Player record for a given achievement.
/// Persisted in CloudSave.
/// </summary>
public class AchievementRecord
{
    public string Id { get; set; }
    public bool Unlocked { get; set; }
    public int ProgressCount { get; set; }
}
