using Blocks.PlayerAccount;

using CrawfisSoftware.Events;

using Unity.VisualScripting;

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

            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.PlayerSigningIn, OnPlayerSigningIn);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.PlayerAuthenticated, OnSignIn);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.PlayerSignedOut, OnPlayerSignOut);
        }

        private void OnDestroy()
        {
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.PlayerSigningIn, OnPlayerSigningIn);
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.PlayerAuthenticated, OnSignIn);
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.PlayerSignedOut, OnPlayerSignOut);
        }
        private void OnSignIn(string eventName, object sender, object data)
        {
            _signInElement.AddToClassList(k_HiddenClass);
            _root.AddToClassList(k_HiddenClass);
            _signedIn = true;
            this.gameObject.SetActive(false);
            //EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.PlayerSignedIn, this, null);
        }

        private void OnPlayerSignOut(string eventName, object sender, object data)
        {
            _signedIn = false;
            _signInElement.RemoveFromClassList(k_HiddenClass);
            _root.RemoveFromClassList(k_HiddenClass);
            this.gameObject.SetActive(true);
        }

        private void OnPlayerSigningIn(string eventName, object sender, object data)
        {
            if (_signedIn)
            {
                //EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.PlayerSignedIn, this, null);
                return;
            }
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
            //_signInElement.AddToClassList(k_HiddenClass);
            //_root.AddToClassList(k_HiddenClass);
            EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.PlayerSigningIn, this, null);

            m_AuthenticationObserver.RegisterSignedInCallback(() =>
            {
                _signInElement.AddToClassList(k_HiddenClass);
                _root.AddToClassList(k_HiddenClass);
            });
        }
    }
}