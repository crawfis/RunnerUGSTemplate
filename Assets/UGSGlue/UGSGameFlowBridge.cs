using CrawfisSoftware.Events;
using CrawfisSoftware.GameFlow.Events;

using UnityEngine;

namespace CrawfisSoftware.UGS.Events
{
    /// <summary>
    /// Bridges UGS events to/from GameFlow events. This class connects the two event domains.
    /// Bidirectional, so it holds two dispatchers rather than inheriting AutoEventFlowBase.
    ///
    /// --- BOOT: UGS to GameFlow ---
    /// [BRIDGE] PlayerAuthenticated -> GameplayReady
    /// [BRIDGE] RemoteConfigUpdated -> LoadingScreenHideRequested
    /// [BRIDGE] PlayerSignedOut -> GameplayNotReady
    ///
    /// --- GAME END: GameFlow to UGS ---
    /// [BRIDGE] GameEnding -> ScoreUpdating
    /// [BRIDGE] GameEnded -> LeaderboardOpening
    /// </summary>
    internal class UGSGameFlowBridge : MonoBehaviour
    {
        private static readonly (UGS_EventsEnum From, GameFlowEvents To)[] UGSToGameFlow =
        {
            (UGS_EventsEnum.PlayerAuthenticated, GameFlowEvents.GameplayReady),
            (UGS_EventsEnum.PlayerSignedOut, GameFlowEvents.GameplayNotReady),

            // Remote config update requests the loading screen to hide; flow auto-fires LoadingScreenHiding.
            (UGS_EventsEnum.RemoteConfigUpdated, GameFlowEvents.LoadingScreenHideRequested),

            // Difficulty settings fetched from remote config
            (UGS_EventsEnum.DifficultySettingsFetched, GameFlowEvents.DifficultySettingsApplied),
        };

        private static readonly (GameFlowEvents From, UGS_EventsEnum To)[] GameFlowToUGS =
        {
            (GameFlowEvents.GameEnding, UGS_EventsEnum.ScoreUpdating),
            (GameFlowEvents.GameEnded, UGS_EventsEnum.LeaderboardOpening),

            // Alternative: kick leaderboard earlier
            //(GameFlowEvents.GameScenesUnloaded, UGS_EventsEnum.LeaderboardOpening),
        };

        private readonly EventChainDispatcher<UGS_EventsEnum, GameFlowEvents> _ugsToGameFlow =
            new EventChainDispatcher<UGS_EventsEnum, GameFlowEvents>(UGSToGameFlow);

        private readonly EventChainDispatcher<GameFlowEvents, UGS_EventsEnum> _gameFlowToUGS =
            new EventChainDispatcher<GameFlowEvents, UGS_EventsEnum>(GameFlowToUGS);

        protected virtual void Awake()
        {
            _ugsToGameFlow.Attach();
            _gameFlowToUGS.Attach();
        }

        protected virtual void OnDestroy()
        {
            _ugsToGameFlow.Detach();
            _gameFlowToUGS.Detach();
        }
    }
}
