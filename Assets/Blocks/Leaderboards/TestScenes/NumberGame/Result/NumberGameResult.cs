using UnityEngine;
using TrustedNumberGameResult = Unity.Services.CloudCode.GeneratedBindings.BlocksGameModule.Leaderboards.NumberGameResult;

namespace Blocks.Leaderboards
{
    public class NumberGameResult
    {
        public int Goal { get; }
        public int Score { get; }
        public NumberGameResult(int goal, int score)
        {
            Goal = goal;
            Score = score;
        }

        public NumberGameResult(TrustedNumberGameResult result)
        {
            Goal = result.Goal;
            Score = result.Score;
        }
    }
}
