using CrawfisSoftware.Contracts;
using CrawfisSoftware.Events;
using CrawfisSoftware.GameFlow.Events;

using UnityEngine;

namespace CrawfisSoftware.UGS.Events
{
    /// <summary>
    /// Maps this application's session lifecycle onto the game-agnostic <see cref="GameSignals"/>
    /// contract, and the contract's answers back onto GameFlow.
    /// </summary>
    /// <remarks>
    /// <para>The host half of the seam. It no longer names a single UGS type: it speaks GameFlow
    /// and the contract, so swapping UGS for a different backend - or none - changes nothing here.
    /// The class name is kept because scene components reference it.</para>
    /// <para>Note what moved. <c>RemoteConfigUpdated -> LoadingScreenHideRequested</c> used to live
    /// on the UGS side, which meant UGS knew its host had a loading screen. Now UGS announces
    /// <see cref="GameSignals.RemoteConfigApplied"/> and this file - the host's - decides that
    /// hiding the loading screen is the right response.</para>
    /// </remarks>
    internal class UGSGameFlowBridge : MonoBehaviour
    {
        private static readonly (GameFlowEvents From, GameSignals To)[] GameFlowToSignals =
        {
            // A run has finished: its score is final, then the session is over.
            (GameFlowEvents.GameEnding, GameSignals.SessionEnding),
            (GameFlowEvents.GameEnded, GameSignals.SessionEnded),
        };

        private static readonly (GameSignals From, GameFlowEvents To)[] SignalsToGameFlow =
        {
            (GameSignals.ServicesReady, GameFlowEvents.GameplayReady),
            (GameSignals.ServicesUnavailable, GameFlowEvents.GameplayNotReady),

            // The host's choice, not the service's: config has arrived, so stop showing loading.
            (GameSignals.RemoteConfigApplied, GameFlowEvents.LoadingScreenHideRequested),

            (GameSignals.DifficultySettingsAvailable, GameFlowEvents.DifficultySettingsApplied),
        };

        private readonly EventChainDispatcher<GameFlowEvents, GameSignals> _gameFlowToSignals =
            new EventChainDispatcher<GameFlowEvents, GameSignals>(GameFlowToSignals);

        private readonly EventChainDispatcher<GameSignals, GameFlowEvents> _signalsToGameFlow =
            new EventChainDispatcher<GameSignals, GameFlowEvents>(SignalsToGameFlow);

        protected virtual void Awake()
        {
            _gameFlowToSignals.Attach();
            _signalsToGameFlow.Attach();
        }

        protected virtual void OnDestroy()
        {
            _gameFlowToSignals.Detach();
            _signalsToGameFlow.Detach();
        }
    }
}
