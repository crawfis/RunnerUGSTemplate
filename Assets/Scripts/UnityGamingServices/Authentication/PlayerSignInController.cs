using Blocks.PlayerAccount;

using CrawfisSoftware.TempleRun;

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

            EventsPublisherTempleRun.Instance.SubscribeToEvent(GamePlayEvents.LoadingScreenHidden, OnLoadingScreenHidden);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.PlayerSignedIn, OnSignIn);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.PlayerSignedOut, OnPlayerSignOut);
        }

        private void OnDestroy()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(GamePlayEvents.LoadingScreenHidden, OnLoadingScreenHidden);
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

        void Start()
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
        }
    }
}