using CrawfisSoftware.Contracts;
using CrawfisSoftware.Events;
using CrawfisSoftware.GameFlow.Events;

using UnityEngine;
using GameFlowBus = CrawfisSoftware.Events.EventsFor<CrawfisSoftware.GameFlow.Events.GameFlowEvents>;
using GameServiceBus = CrawfisSoftware.Events.EventsFor<CrawfisSoftware.Contracts.GameServiceEvents>;

namespace CrawfisSoftware.UGS.Events
{
    /// <summary>
    /// Maps this application's session lifecycle onto the game-agnostic
    /// <see cref="GameServiceEvents"/> contract, and the contract's answers back onto GameFlow.
    /// </summary>
    /// <remarks>
    /// <para>The host half of the seam. It names GameFlow and the contract, never a UGS type, so
    /// swapping UGS for a different backend - or none - changes nothing here.</para>
    /// <para><b>Why the status is read as a level, not an edge.</b> This component lives in an
    /// additively-loaded scene, so it may subscribe well after services came up. Reacting to the
    /// transient <c>ServicesReady</c> edge meant that if it lost that race, GameplayReady was never
    /// published and the boot stalled at LoadingScreenHidden - no menu, no error. Subscribing to
    /// the Sticky level instead, the current status is delivered on subscribe however late that
    /// is. The race cannot occur rather than being unlikely.</para>
    /// <para><b>Not its own assembly, deliberately.</b> Glue is the most volatile thing here: it
    /// changes whenever either side does, and it is the one place licensed to know both. An
    /// assembly boundary would enforce nothing and cost a reference edit on every change.</para>
    /// </remarks>
    internal class UGSGameFlowBridge : MonoBehaviour
    {
        private static readonly EventId<ServicesStatus> StatusChanged =
            GameServiceBus.Id<ServicesStatus>(GameServiceEvents.ServicesStatusChanged);

        private static readonly (GameFlowEvents From, GameServiceEvents To)[] GameFlowToGameService =
        {
            // A run has finished: its score is final, then the session is over.
            (GameFlowEvents.GameEnding, GameServiceEvents.SessionEnding),
            (GameFlowEvents.GameEnded, GameServiceEvents.SessionEnded),
        };

        private static readonly (GameServiceEvents From, GameFlowEvents To)[] GameServiceToGameFlow =
        {
            // The host's choice, not the service's: config has arrived, so stop showing loading.
            (GameServiceEvents.RemoteConfigApplied, GameFlowEvents.LoadingScreenHideRequested),

            (GameServiceEvents.DifficultySettingsAvailable, GameFlowEvents.DifficultySettingsApplied),

            // The banked balance, forwarded unchanged. A pair is enough here, unlike on the UGS
            // side of the seam where the payload is a services type that had to be unwrapped: by
            // this point it is already a plain long. Sticky survives the hop because a dispatcher
            // attaches with SubscribeToAll, which subscribes to each member individually and so
            // receives the same retained-value replay any subscribe does.
            (GameServiceEvents.CurrencyBalanceChanged, GameFlowEvents.CurrencyBalanceChanged),
        };

        private readonly EventChainDispatcher<GameFlowEvents, GameServiceEvents> _gameFlowToGameService =
            new EventChainDispatcher<GameFlowEvents, GameServiceEvents>(GameFlowToGameService);

        private readonly EventChainDispatcher<GameServiceEvents, GameFlowEvents> _gameServiceToGameFlow =
            new EventChainDispatcher<GameServiceEvents, GameFlowEvents>(GameServiceToGameFlow);

        protected virtual void Awake()
        {
            _gameFlowToGameService.Attach();
            _gameServiceToGameFlow.Attach();

            // Sticky: if services are already up, this fires immediately on subscribe.
            StatusChanged.Subscribe(OnServicesStatusChanged);

        }

        protected virtual void OnDestroy()
        {
            _gameFlowToGameService.Detach();
            _gameServiceToGameFlow.Detach();

            StatusChanged.Unsubscribe(OnServicesStatusChanged);
        }

        private void OnServicesStatusChanged(string eventName, object sender, ServicesStatus status)
        {
            switch (status)
            {
                case ServicesStatus.Ready:
                    GameFlowBus.Publish(GameFlowEvents.GameplayReady, sender, null);
                    break;

                case ServicesStatus.Unavailable:
                    GameFlowBus.Publish(GameFlowEvents.GameplayNotReady, sender, null);
                    break;

                case ServicesStatus.Connecting:
                    // Nothing to announce to GameFlow yet. A "Connecting to Unity Services..."
                    // panel should subscribe to the level directly rather than have this
                    // translate it - the host owns how waiting is presented.
                    break;
            }
        }
    }
}
