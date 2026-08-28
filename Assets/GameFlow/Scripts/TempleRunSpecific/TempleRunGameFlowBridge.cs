using CrawfisSoftware.Events;
using CrawfisSoftware.TempleRun;
using CrawfisSoftware.UGS.Events;

using UnityEngine;

namespace CrawfisSoftware.GameFlow.Events
{
    /// <summary>
    /// Sanctioned crossings out of the TempleRun domain. Three directions, so it holds three
    /// dispatchers rather than inheriting AutoEventFlowBase, which covers one.
    /// </summary>
    internal class TempleRunGameFlowBridge : MonoBehaviour
    {
        private static readonly (TempleRunEvents From, GameFlowEvents To)[] TempleRunToGameFlow =
        {
            // TempleRun paused -> request GameFlow pause (for menus/UI)
            (TempleRunEvents.PlayerPaused, GameFlowEvents.PauseRequested),

            // TempleRun resumed -> request GameFlow resume. The counterpart to the line above:
            // without it nothing ever publishes GameFlowEvents.ResumeRequested, so the
            // ResumeRequested -> Resuming -> Resumed chain never runs and GameState.IsGamePaused
            // stays true forever once the player has paused even once.
            (TempleRunEvents.PlayerResumed, GameFlowEvents.ResumeRequested),

            // Countdown ended -> game officially started (absorbed from GameController)
            (TempleRunEvents.CountdownEnded, GameFlowEvents.GameStarted),

            // Player died -> game ending (absorbed from GameController)
            (TempleRunEvents.TempleRunEnded, GameFlowEvents.GameEnding),
        };

        private static readonly (GameFlowEvents From, TempleRunEvents To)[] GameFlowToTempleRun =
        {
            // Bridge start: when the broader game signals started, fire TempleRun start requested
            (GameFlowEvents.GameStarted, TempleRunEvents.TempleRunStartRequested),

            // GameFlow starting -> kick off countdown in TempleRun
            (GameFlowEvents.GameStarting, TempleRunEvents.CountdownStartRequested),

            // Config/scenes bridged to TempleRun domain
            (GameFlowEvents.GameConfigApplied, TempleRunEvents.TempleRunConfigApplied),
            (GameFlowEvents.LevelApplied, TempleRunEvents.TempleRunLevelApplied),
            (GameFlowEvents.GameScenesLoaded, TempleRunEvents.TempleRunScenesReady),
        };

        // TempleRun -> UGS passthrough (bypasses GameFlow - this bridge is the authorized crossing point)
        private static readonly (TempleRunEvents From, UGS_EventsEnum To)[] TempleRunToUGS =
        {
            // Distance updates for achievement tracking
            (TempleRunEvents.DistanceUpdated, UGS_EventsEnum.UGS_DistanceUpdated),

            // Coin collection for economy sync and achievement tracking
            (TempleRunEvents.CoinCollected, UGS_EventsEnum.UGS_CoinUpdated),
        };

        private readonly EventChainDispatcher<TempleRunEvents, GameFlowEvents> _templeRunToGameFlow =
            new EventChainDispatcher<TempleRunEvents, GameFlowEvents>(TempleRunToGameFlow);

        private readonly EventChainDispatcher<GameFlowEvents, TempleRunEvents> _gameFlowToTempleRun =
            new EventChainDispatcher<GameFlowEvents, TempleRunEvents>(GameFlowToTempleRun);

        private readonly EventChainDispatcher<TempleRunEvents, UGS_EventsEnum> _templeRunToUGS =
            new EventChainDispatcher<TempleRunEvents, UGS_EventsEnum>(TempleRunToUGS);

        protected virtual void Awake()
        {
            _templeRunToGameFlow.Attach();
            _gameFlowToTempleRun.Attach();
            _templeRunToUGS.Attach();
        }

        protected virtual void OnDestroy()
        {
            _templeRunToGameFlow.Detach();
            _gameFlowToTempleRun.Detach();
            _templeRunToUGS.Detach();
        }
    }
}
