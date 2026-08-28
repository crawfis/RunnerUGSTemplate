using CrawfisSoftware.Events;
using CrawfisSoftware.TempleRun;

using UnityEngine;

namespace CrawfisSoftware.GameFlow.Events
{
    /// <summary>
    /// The sanctioned crossing between the TempleRun and GameFlow domains. Bidirectional, so it
    /// holds two dispatchers rather than inheriting AutoEventFlowBase, which covers one.
    /// </summary>
    /// <remarks>The TempleRun -> UGS passthrough that used to live here now sits in
    /// <c>TempleRunUGSBridge</c>, under <c>Assets/UGS/</c>. A GameFlow file naming
    /// <c>UGS_EventsEnum</c> made the game fail to compile without the UGS folder.</remarks>
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

        private readonly EventChainDispatcher<TempleRunEvents, GameFlowEvents> _templeRunToGameFlow =
            new EventChainDispatcher<TempleRunEvents, GameFlowEvents>(TempleRunToGameFlow);

        private readonly EventChainDispatcher<GameFlowEvents, TempleRunEvents> _gameFlowToTempleRun =
            new EventChainDispatcher<GameFlowEvents, TempleRunEvents>(GameFlowToTempleRun);

        protected virtual void Awake()
        {
            _templeRunToGameFlow.Attach();
            _gameFlowToTempleRun.Attach();
        }

        protected virtual void OnDestroy()
        {
            _templeRunToGameFlow.Detach();
            _gameFlowToTempleRun.Detach();
        }
    }
}
