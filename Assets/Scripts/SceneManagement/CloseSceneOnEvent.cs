using CrawfisSoftware.UGS;

using UnityEngine;

namespace CrawfisSoftware.SceneManagement
{
    class CloseSceneOnEvent : MonoBehaviour
    {
        [SerializeField] private UGS_EventsEnum _eventToSubscribeTo;
        [SerializeField] private bool _unsubscribeOnEvent = true;

        private void Awake()
        {
            EventsPublisherUGS.Instance.SubscribeToEvent(_eventToSubscribeTo, OnEventReceived);
        }
        private void OnDestroy()
        {
            EventsPublisherUGS.Instance.UnsubscribeToEvent(_eventToSubscribeTo, OnEventReceived);
        }

        private void OnEventReceived(string eventName, object sender, object data)
        {
            if(_unsubscribeOnEvent)
                EventsPublisherUGS.Instance.UnsubscribeToEvent(_eventToSubscribeTo, OnEventReceived);
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(this.gameObject.scene);
        }
    }
}