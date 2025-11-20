using System.Threading.Tasks;
using Blocks.Leaderboards.TestScene;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using UnityEngine;

namespace Blocks.Leaderboards
{
    public class TrustedNumberGameClient : INumberGameClient
    {
        NumberGameClientBindings m_ClientBindings = new NumberGameClientBindings(CloudCodeService.Instance);

        public async Task<NumberGameResult> RequestGoalAndScore(string leaderboardId, int guess)
        {
            var clientResult = await m_ClientBindings.RequestGoalAndScore(leaderboardId, guess);
            return new NumberGameResult(clientResult);
        }
    }
}
