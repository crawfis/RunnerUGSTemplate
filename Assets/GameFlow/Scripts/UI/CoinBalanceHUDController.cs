using CrawfisSoftware.Events;
using CrawfisSoftware.GameFlow.Events;

using UnityEngine;
using UnityEngine.UIElements;

using GameFlowBus = CrawfisSoftware.Events.EventsFor<CrawfisSoftware.GameFlow.Events.GameFlowEvents>;

namespace CrawfisSoftware.GameFlow.UI
{
    /// <summary>
    /// Shows the player's banked lifetime coin balance on the gameplay HUD.
    ///    Dependencies: PanelRenderer (the HUD overlay panel)
    ///    Subscribes: GameFlowEvents.CurrencyBalanceChanged (data: long),
    ///                GameFlowEvents.SessionCoinsChanged (data: int)
    ///    Publishes: none
    /// </summary>
    /// <remarks>
    /// <para><b>Why this is a GameFlow component on a TempleRun panel.</b> The balance is account
    /// state, not gameplay state - it survives runs and comes from the services layer. TempleRun
    /// code may only name TempleRunEvents, so a component there could not hear it without making
    /// gameplay depend on a service being present, which the one-way TempleRunUGSBridge exists to
    /// prevent. It shares the HUD's PanelRenderer with <c>GUIController</c>; several components
    /// may register reload callbacks on one panel.</para>
    /// <para><b>What is displayed is the banked balance plus this run's coins.</b> The balance
    /// itself only moves at sign-in and when a finished run banks, so on its own it would sit
    /// still for the whole run. Adding the run's running count makes the number climb as coins
    /// are picked up, and the two reconcile at the end: a newly banked balance already contains
    /// those coins, so the run count is cleared the moment one arrives rather than being added
    /// twice.</para>
    /// <para><b>If banking fails</b> the coins stay pending and no new balance arrives, so the
    /// display keeps including them - which is honest. The next run's first pickup then replaces
    /// the run count and the pending coins stop being shown until they do bank. A display-only
    /// artefact of a failure path, not a lost coin.</para>
    /// </remarks>
    public class CoinBalanceHUDController : MonoBehaviour
    {
        [Tooltip("The HUD panel. Shared with GUIController; both register their own reload callback.")]
        [SerializeField] private PanelRenderer _panel;

        [Tooltip("Name of the Label in the panel's UXML that displays the balance.")]
        [SerializeField] private string _labelName = "_coinBalanceLabel";

        private Label _balanceLabel;

        // Held because the label is reached through a reload callback that can arrive after the
        // first balance does - a tree rebuild would otherwise blank a number already delivered.
        private long _balance;
        private bool _hasBalance;

        // Assigned, never accumulated: SessionCoinsChanged carries the run's running total, so
        // adding it would over-count by roughly the square of the coins collected.
        private int _sessionCoins;

        private void Awake()
        {
            // Sticky: a balance read at sign-in, long before this scene loaded, arrives here on
            // subscribe rather than being lost.
            GameFlowBus.Subscribe(GameFlowEvents.CurrencyBalanceChanged, OnBalanceChanged);
            GameFlowBus.Subscribe(GameFlowEvents.SessionCoinsChanged, OnSessionCoinsChanged);
        }

        private void OnDestroy()
        {
            GameFlowBus.Unsubscribe(GameFlowEvents.CurrencyBalanceChanged, OnBalanceChanged);
            GameFlowBus.Unsubscribe(GameFlowEvents.SessionCoinsChanged, OnSessionCoinsChanged);
        }

        private void OnEnable()
        {
            if (_panel == null) return;
            _panel.RegisterUIReloadCallback(OnUIReload);
        }

        private void OnDisable()
        {
            if (_panel != null) _panel.UnregisterUIReloadCallback(OnUIReload);
        }

        // The PanelRenderer surfaces its tree only through this callback, and a reload rebuilds
        // it, so the label is re-queried and repainted every time rather than cached once.
        private void OnUIReload(PanelRenderer renderer, VisualElement root)
        {
            _balanceLabel = root?.Q<Label>(_labelName);
            Repaint();
        }

        private void OnBalanceChanged(string eventName, object sender, object data)
        {
            if (data is not long balance) return;

            _balance = balance;
            _hasBalance = true;

            // A balance the service just reported already contains everything banked up to now,
            // including this run's coins. Keeping the run count would show them twice.
            _sessionCoins = 0;
            Repaint();
        }

        private void OnSessionCoinsChanged(string eventName, object sender, object data)
        {
            if (data is not int coins) return;

            _sessionCoins = coins;
            Repaint();
        }

        private void Repaint()
        {
            if (_balanceLabel == null) return;

            // Blank until a balance has actually arrived. Showing "0" before the first read would
            // claim the player has no coins, which is a different statement from "not known yet".
            _balanceLabel.text = _hasBalance ? (_balance + _sessionCoins).ToString("N0") : string.Empty;
        }
    }
}
