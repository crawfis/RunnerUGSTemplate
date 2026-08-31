using CrawfisSoftware.Contracts;
using CrawfisSoftware.Events;
using CrawfisSoftware.TempleRun;

using UnityEngine;

namespace CrawfisSoftware.UGS.Events
{
    /// <summary>
    /// Maps this game's gameplay events onto the game-agnostic <see cref="GameServiceEvents"/>
    /// contract.
    /// </summary>
    /// <remarks>
    /// <para>This is the per-game half of the seam, and the only file here that names
    /// <c>TempleRunEvents</c>. Swapping the runner for a different game means rewriting this table
    /// and nothing else - UGS never sees a game type.</para>
    /// <para>It is deliberately NOT its own assembly. Glue is the most volatile thing in the
    /// system: it changes whenever either side does, and it is the one place licensed to know
    /// both. An assembly boundary here would enforce nothing and cost a reference edit on every
    /// change.</para>
    /// <para>One-way by design. Gameplay announces; services observe. Nothing maps back into
    /// <c>TempleRunEvents</c>, so the game cannot come to depend on a service being present.</para>
    /// </remarks>
    internal class TempleRunUGSBridge : MonoBehaviour
    {
        private static readonly (TempleRunEvents From, GameServiceEvents To)[] GameplayToGameService =
        {
            // Distance is this game's score metric. A different game maps whatever its is.
            (TempleRunEvents.DistanceUpdated, GameServiceEvents.ScoreUpdated),

            // CoinCollected carries Blackboard.SessionCoinCount - a running total, which is why
            // the contract member is CurrencyTotalChanged rather than CurrencyEarned.
            (TempleRunEvents.CoinCollected, GameServiceEvents.CurrencyTotalChanged),
        };

        private readonly EventChainDispatcher<TempleRunEvents, GameServiceEvents> _gameplayToGameService =
            new EventChainDispatcher<TempleRunEvents, GameServiceEvents>(GameplayToGameService);

        protected virtual void Awake()
        {
            _gameplayToGameService.Attach();
        }

        protected virtual void OnDestroy()
        {
            _gameplayToGameService.Detach();
        }
    }
}
