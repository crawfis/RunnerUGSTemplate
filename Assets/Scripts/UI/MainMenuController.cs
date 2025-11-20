using CrawfisSoftware.TempleRun;

using System;

using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;

using UnityEngine;
using UnityEngine.UIElements;

namespace CrawfisSoftware.UI
{
    class MainMenuController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _quitGameButton;
        [SerializeField] private Button _signOutButton;
        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;
            _startGameButton = root.Q<Button>("BtnPlay");
            _startGameButton.clicked += OnStartGameButtonClicked;
            _quitGameButton = root.Q<Button>("BtnQuit");
            _quitGameButton.clicked += OnQuitButtonClicked;
            _signOutButton = root.Q<Button>("BtnSignOut");
            _signOutButton.clicked += OnSignOutButtonClicked;
        }

        private void OnQuitButtonClicked()
        {
            EventsPublisherTempleRun.Instance.PublishEvent(GamePlayEvents.Quit, this, null);
        }

        private void OnSignOutButtonClicked()
        {
            AuthenticationService.Instance.SignOut();
            PlayerAccountService.Instance.SignOut();
            AuthenticationService.Instance.ClearSessionToken();
        }

        private void OnStartGameButtonClicked()
        {
            EventsPublisherTempleRun.Instance.PublishEvent(GamePlayEvents.GameStarting, this, null);
        }
    }
}