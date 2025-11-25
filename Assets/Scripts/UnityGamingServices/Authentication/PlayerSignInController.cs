using Blocks.PlayerAccount;

using CrawfisSoftware.Events;
using CrawfisSoftware.TempleRun;

using Unity.Services.Authentication;

using UnityEngine;
using UnityEngine.UIElements;

namespace CrawfisSoftware.UGS.Authentication
{
    public class PlayerSignInController : MonoBehaviour
    {
        [SerializeField] UIDocument uiDocument;

        AuthenticationObserver m_AuthenticationObserver;
        const string k_HiddenClass = "hidden";
        VisualElement _root;
        VisualElement _signInElement;
        bool _signedIn = false;

        private void Awake()
        {
            _root = uiDocument.rootVisualElement;
            _signInElement = _root.Q<PlayerSignIn>();
            m_AuthenticationObserver = new AuthenticationObserver();

            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.LoadingScreenHidden, OnLoadingScreenHidden);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.PlayerSignedIn, OnSignIn);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.PlayerSignedOut, OnPlayerSignOut);
        }

        private void OnDestroy()
        {
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.LoadingScreenHidden, OnLoadingScreenHidden);
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.PlayerSignedIn, OnSignIn);
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.PlayerSignedOut, OnPlayerSignOut);
        }
        private void OnSignIn(string arg1, object arg2, object arg3)
        {
            _signInElement.AddToClassList(k_HiddenClass);
            _root.AddToClassList(k_HiddenClass);
            _signedIn = true;
            this.gameObject.SetActive(false);
        }

        private void OnPlayerSignOut(string arg1, object arg2, object arg3)
        {
            _signedIn = false;
            _signInElement.RemoveFromClassList(k_HiddenClass);
            _root.RemoveFromClassList(k_HiddenClass);
            this.gameObject.SetActive(true);
        }

        private void OnLoadingScreenHidden(string arg1, object arg2, object arg3)
        {
            if(_signedIn)
                return;
            _root.RemoveFromClassList(k_HiddenClass);
            _signInElement.RemoveFromClassList(k_HiddenClass);
        }

        async void Start()
        {
            if (uiDocument == null)
            {
                Debug.LogError("No UIDocument found in scene!");
                return;
            }
            _signInElement.AddToClassList(k_HiddenClass);
            _root.AddToClassList(k_HiddenClass);

            m_AuthenticationObserver.RegisterSignedInCallback(() =>
            {
                _signInElement.AddToClassList(k_HiddenClass);
                _root.AddToClassList(k_HiddenClass);
            });

            // Sign in returning player using the Session Token stored after sign in.
            // Players don't need to sign in with Unity Player Account again; using 
            // this method will re-authorise them (they will keep the same Player ID).
            if (AuthenticationService.Instance.SessionTokenExists)
            {
                _signedIn = true;
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.PlayerSignedIn, this, (AuthenticationService.Instance.PlayerName, AuthenticationService.Instance.PlayerId));
                Debug.Log($"Returning Player ID: {AuthenticationService.Instance.PlayerId}");
                Debug.Log($"Returning Player is SignedIn: {AuthenticationService.Instance.IsSignedIn}");
                Debug.Log($"Returning Player is Authorized: {AuthenticationService.Instance.IsAuthorized}");
                EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.PlayerAuthenticated, this, (AuthenticationService.Instance.PlayerName, AuthenticationService.Instance.PlayerId));
            }
        }
    }
}