using System;

namespace Blocks.Achievements
{
    /// <summary>
    /// Specific exception for this starter kit when interacting with the different services
    /// </summary>
    public class AchievementException : Exception
    {
        /// <summary>
        /// ctor with message
        /// </summary>
        /// <param name="message">exception message</param>
        public AchievementException(string message) : base(message)
        {
        }
    }
}
