using CrawfisSoftware.Events;

using UnityEngine;

namespace CrawfisSoftware.Test
{
    public class Test_AutoFireEvent : MonoBehaviour
    {
        [SerializeField] private string _eventNameToListenFor = "GameEnded";
        [SerializeField] private string _eventNameToFire = "GameplayReady";
        void Start()
        {
            EventsPublisher.Instance.SubscribeToEvent(_eventNameToListenFor, OnEventReceived);
        }

        private void OnDestroy()
        {
            EventsPublisher.Instance.UnsubscribeToEvent(_eventNameToListenFor, OnEventReceived);
        }
        private void OnEventReceived(string arg1, object arg2, object arg3)
        {
            EventsPublisher.Instance.PublishEvent(_eventNameToFire, this, null);
        }
    }
}