using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;

namespace Blocks.Leaderboards
{
    /// <summary>
    /// The trusted client for leaderboards
    /// The client fetches locally, but uses Cloud Code Modules to PUT scores into the leaderboard service.
    /// </summary>
    class TrustedLeaderboardClient : LocalLeaderboardsClient
    {
        LeaderboardClientBindings m_ClientBindings;

        /// <summary>
        /// The trusted client ctor
        /// </summary>
        /// <param name="leaderboardsService">The leaderboard service</param>
        /// <param name="cloudCodeService">The cloud code service</param>
        public TrustedLeaderboardClient(ILeaderboardsService leaderboardsService, ICloudCodeService cloudCodeService) : base(leaderboardsService)
        {
            m_ClientBindings = new LeaderboardClientBindings(cloudCodeService);
        }

        /// <inheritdoc/>
        public override Task<List<LeaderboardEntry>> GetScoresAsync(string leaderboardId, int limit, bool isGlobal)
        {
            // note that you could call the endpoint m_ClientBindings.GetScoresAsync(leaderboardId, limit, isGlobal);
            // This isn't necessary because only the PUT operation needs to be done by the trusted client
            // The local client (this class' base) GET is good to fill the leaderboards

            return base.GetScoresAsync(leaderboardId, limit, isGlobal);
        }

        /// <inheritdoc/>
        public override async Task AddPlayerScoreAsync(string leaderboardId, double score)
        {
            await m_ClientBindings.AddPlayerScoreAsync(leaderboardId, score);
        }
    }
}
