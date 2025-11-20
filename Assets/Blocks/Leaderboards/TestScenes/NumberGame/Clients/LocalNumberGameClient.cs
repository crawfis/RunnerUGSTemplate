using System.Threading.Tasks;
using UnityEngine;

namespace Blocks.Leaderboards.TestScene
{
    public class LocalNumberGameClient : INumberGameClient
    {
        public async Task<NumberGameResult> RequestGoalAndScore(string leaderboardName, int guess)
        {
            var goal = Random.Range(0, 99);
            var score = 100 - Mathf.Abs(goal - guess);
            await LeaderboardsObserver.Instance.AddPlayerScoreAsync(score);
            return new NumberGameResult(goal, score);
        }
    }
}
