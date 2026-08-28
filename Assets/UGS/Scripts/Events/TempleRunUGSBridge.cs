using CrawfisSoftware.Events;
using CrawfisSoftware.TempleRun;

using UnityEngine;

namespace CrawfisSoftware.UGS.Events
{
    /// <summary>
    /// Forwards the gameplay signals UGS needs into the UGS domain.
    /// </summary>
    /// <remarks>
    /// <para>These two mappings used to live in <c>TempleRunGameFlowBridge</c> - a GameFlow-domain
    /// file - which meant the game referenced <c>UGS_EventsEnum</c> at compile time. That is the
    /// one thing a pluggable service layer must not require: deleting <c>Assets/UGS/</c> broke the
    /// build. Owned by UGS and hosted in a UGS scene, the passthrough now disappears with the
    /// folder.</para>
    /// <para>It also fixes a quieter wrong. Hosted in <c>Game_Boot_2_Play</c>, the passthrough was
    /// live even in a GameOnly build, translating gameplay events onto a bus with no listeners.
    /// Hosted in <c>UGS_Boot_0_Initialization</c>, it exists exactly when UGS does.</para>
    /// <para>The direction is one-way by design: gameplay announces, UGS observes. Nothing here
    /// publishes back into <c>TempleRunEvents</c>, so the game cannot come to depend on UGS
    /// being present.</para>
    /// </remarks>
    internal class TempleRunUGSBridge : MonoBehaviour
    {
        private static readonly (TempleRunEvents From, UGS_EventsEnum To)[] TempleRunToUGS =
        {
            // Distance updates for achievement tracking
            (TempleRunEvents.DistanceUpdated, UGS_EventsEnum.UGS_DistanceUpdated),

            // Coin collection for economy sync and achievement tracking
            (TempleRunEvents.CoinCollected, UGS_EventsEnum.UGS_CoinUpdated),
        };

        private readonly EventChainDispatcher<TempleRunEvents, UGS_EventsEnum> _templeRunToUGS =
            new EventChainDispatcher<TempleRunEvents, UGS_EventsEnum>(TempleRunToUGS);

        protected virtual void Awake()
        {
            _templeRunToUGS.Attach();
        }

        protected virtual void OnDestroy()
        {
            _templeRunToUGS.Detach();
        }
    }
}
