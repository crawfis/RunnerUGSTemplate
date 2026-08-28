using CrawfisSoftware.Contracts;
using CrawfisSoftware.Events;

using UnityEngine;

namespace CrawfisSoftware.UGS.Events
{
    /// <summary>
    /// Translates between the game-agnostic <see cref="GameSignals"/> contract and this layer's
    /// own UGS events. The only boundary UGS has with the outside world.
    /// </summary>
    /// <remarks>
    /// <para>This lives inside UGS and is still generic: it names <see cref="GameSignals"/> and
    /// <see cref="UGS_EventsEnum"/>, and no game type at all. Whichever game is underneath, this
    /// file is unchanged.</para>
    /// <para>UGS keeps its internal events rather than consuming the contract directly, so its own
    /// auto-chains (auth flow, leaderboard UI states) keep working untouched. The contract is a
    /// boundary, not a replacement for the domain's internal vocabulary.</para>
    /// </remarks>
    internal class GameSignalsUGSBridge : MonoBehaviour
    {
        private static readonly (GameSignals From, UGS_EventsEnum To)[] SignalsToUGS =
        {
            // The game's score metric drives distance-based achievements. UGS does not know the
            // metric is metres - only that it is the number the game scores on.
            (GameSignals.ScoreUpdated, UGS_EventsEnum.UGS_DistanceUpdated),

            // Soft-currency total drives economy sync and coin achievements.
            (GameSignals.CurrencyTotalChanged, UGS_EventsEnum.UGS_CoinUpdated),

            // A finished run is a score to submit, then a leaderboard to show.
            (GameSignals.SessionEnding, UGS_EventsEnum.ScoreUpdating),
            (GameSignals.SessionEnded, UGS_EventsEnum.LeaderboardOpening),
        };

        private static readonly (UGS_EventsEnum From, GameSignals To)[] UGSToSignals =
        {
            (UGS_EventsEnum.PlayerAuthenticated, GameSignals.ServicesReady),
            (UGS_EventsEnum.PlayerSignedOut, GameSignals.ServicesUnavailable),

            // UGS announces that config arrived. What the host does about it - hiding a loading
            // screen, say - is the host's business, not this layer's.
            (UGS_EventsEnum.RemoteConfigUpdated, GameSignals.RemoteConfigApplied),
            (UGS_EventsEnum.DifficultySettingsFetched, GameSignals.DifficultySettingsAvailable),
        };

        private readonly EventChainDispatcher<GameSignals, UGS_EventsEnum> _signalsToUGS =
            new EventChainDispatcher<GameSignals, UGS_EventsEnum>(SignalsToUGS);

        private readonly EventChainDispatcher<UGS_EventsEnum, GameSignals> _ugsToSignals =
            new EventChainDispatcher<UGS_EventsEnum, GameSignals>(UGSToSignals);

        protected virtual void Awake()
        {
            _signalsToUGS.Attach();
            _ugsToSignals.Attach();
        }

        protected virtual void OnDestroy()
        {
            _signalsToUGS.Detach();
            _ugsToSignals.Detach();
        }
    }
}
