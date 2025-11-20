using System.Threading.Tasks;
using UnityEngine;

namespace Blocks.Leaderboards.TestScene
{
    interface INumberGameClient
    {
        Task<NumberGameResult> RequestGoalAndScore(string leaderboardName, int guess);
    }
}
