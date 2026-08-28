using CrawfisSoftware.GameFlow.Events;

using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;

using UnityEngine;
using UnityEngine.UIElements;

using GameFlowBus = CrawfisSoftware.Events.EventsFor<CrawfisSoftware.GameFlow.Events.GameFlowEvents>;

namespace CrawfisSoftware.GameFlow.UI
{
    /// <summary>
    /// Wires the main menu buttons (play, quit, UGS sign-out) to GameFlow events.
    ///    Dependencies: PanelRenderer (main menu panel), AuthenticationService, PlayerAccountService
    ///    Subscribes: none
    ///    Publishes: LevelSelectorShowRequested, QuitRequested
    /// </summary>
    class MainMenuController : MonoBehaviour
    {
        [SerializeField] private PanelRenderer _panel;

        private Button _startGameButton;
        private Button _quitGameButton;
        private Button _signOutButton;

        private VisualElement _root;

        private void OnEnable()
        {
            _panel.RegisterUIReloadCallback(OnUIReload);
        }

        private void OnDisable()
        {
            _panel.UnregisterUIReloadCallback(OnUIReload);
            if (_startGameButton != null) _startGameButton.clicked -= OnStartGameButtonClicked;
            if (_quitGameButton != null) _quitGameButton.clicked -= OnQuitButtonClicked;
            if (_signOutButton != null) _signOutButton.clicked -= OnSignOutButtonClicked;
        }

        // The PanelRenderer surfaces its visual tree only through this callback (it has no
        // root-tree property). It can fire again on LiveReload, so wiring is idempotent:
        // unhook before re-hooking.
        private void OnUIReload(PanelRenderer renderer, VisualElement root)
        {
            _root = root;

            if (_startGameButton != null) _startGameButton.clicked -= OnStartGameButtonClicked;
            _startGameButton = root.Q<Button>("BtnPlay");
            if (_startGameButton != null) _startGameButton.clicked += OnStartGameButtonClicked;

            if (_quitGameButton != null) _quitGameButton.clicked -= OnQuitButtonClicked;
            _quitGameButton = root.Q<Button>("BtnQuit");
            if (_quitGameButton != null) _quitGameButton.clicked += OnQuitButtonClicked;

            // UGS build: the sign-out button stays wired (unlike the non-UGS template, which hides it).
            if (_signOutButton != null) _signOutButton.clicked -= OnSignOutButtonClicked;
            _signOutButton = root.Q<Button>("BtnSignOut");
            if (_signOutButton != null) _signOutButton.clicked += OnSignOutButtonClicked;
        }

        private void OnQuitButtonClicked()
        {
            GameFlowBus.Publish(GameFlowEvents.QuitRequested, "Main Menu", null);
        }

        private void OnSignOutButtonClicked()
        {
            AuthenticationService.Instance.SignOut();
            PlayerAccountService.Instance.SignOut();
            AuthenticationService.Instance.ClearSessionToken();
        }

        private void OnStartGameButtonClicked()
        {
            GameFlowBus.Publish(GameFlowEvents.LevelSelectorShowRequested, this, null);
        }
    }
}
