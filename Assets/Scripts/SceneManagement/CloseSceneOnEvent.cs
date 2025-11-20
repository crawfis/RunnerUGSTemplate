using CrawfisSoftware.UGS;

using UnityEngine;

namespace CrawfisSoftware.SceneManagement
{
    class CloseSceneOnEvent : MonoBehaviour
    {
        [SerializeField] private UGS_EventsEnum _eventToSubscribeTo;

        private void Awake()
        {
            EventsPublisherUGS.Instance.SubscribeToEvent(_eventToSubscribeTo, OnEventReceived);
        }

        private void OnEventReceived(string arg1, object arg2, object arg3)
        {
            EventsPublisherUGS.Instance.UnsubscribeToEvent(_eventToSubscribeTo, OnEventReceived);
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(this.gameObject.scene);
        }
    }
}