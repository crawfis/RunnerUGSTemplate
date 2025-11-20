using System;

namespace BlocksGameModule.Achievements;

public class AchievementException : Exception
{
    public AchievementException(string message) : base(message) { }
}
