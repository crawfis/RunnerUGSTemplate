using CrawfisSoftware.Events;

using UnityEngine;

namespace CrawfisSoftware.SceneManagement
{
    class CloseSceneOnEvent : MonoBehaviour
    {
        [SerializeField] private GameFlowEvents _eventToSubscribeTo;
        [SerializeField] private bool _unsubscribeOnEvent = true;

        private void Awake()
        {
            EventsPublisherGameFlow.Instance.SubscribeToEvent(_eventToSubscribeTo, OnEventReceived);
        }
        private void OnDestroy()
        {
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(_eventToSubscribeTo, OnEventReceived);
        }

        private void OnEventReceived(string eventName, object sender, object data)
        {
            if(_unsubscribeOnEvent)
                EventsPublisherGameFlow.Instance.UnsubscribeToEvent(_eventToSubscribeTo, OnEventReceived);
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(this.gameObject.scene);
        }
    }
}