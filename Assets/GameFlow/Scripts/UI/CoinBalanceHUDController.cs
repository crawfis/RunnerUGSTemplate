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
    ///    Subscribes: GameFlowEvents.CurrencyBalanceChanged (data: long)
    ///    Publishes: none
    /// </summary>
    /// <remarks>
    /// <para><b>Why this is a GameFlow component on a TempleRun panel.</b> The balance is account
    /// state, not gameplay state - it survives runs and comes from the services layer. TempleRun
    /// code may only name TempleRunEvents, so a component there could not hear it without making
    /// gameplay depend on a service being present, which the one-way TempleRunUGSBridge exists to
    /// prevent. It shares the HUD's PanelRenderer with <c>GUIController</c>; several components
    /// may register reload callbacks on one panel.</para>
    /// <para><b>The balance is not live during a run.</b> It changes at sign-in and again when a
    /// finished run banks its coins, so the number deliberately sits still while playing. The
    /// running count for the current run is a separate, gameplay-side number.</para>
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

        private void Awake()
        {
            // Sticky: a balance read at sign-in, long before this scene loaded, arrives here on
            // subscribe rather than being lost.
            GameFlowBus.Subscribe(GameFlowEvents.CurrencyBalanceChanged, OnBalanceChanged);
        }

        private void OnDestroy()
        {
            GameFlowBus.Unsubscribe(GameFlowEvents.CurrencyBalanceChanged, OnBalanceChanged);
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
            Repaint();
        }

        private void Repaint()
        {
            if (_balanceLabel == null) return;

            // Blank until a balance has actually arrived. Showing "0" before the first read would
            // claim the player has no coins, which is a different statement from "not known yet".
            _balanceLabel.text = _hasBalance ? _balance.ToString("N0") : string.Empty;
        }
    }
}
