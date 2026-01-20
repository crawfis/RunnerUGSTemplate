using CrawfisSoftware.Events;

using UnityEngine;

namespace CrawfisSoftware.Test
{
    public class Test_AutoFireEventOnStart : MonoBehaviour
    {
        [SerializeField] private string _eventName = "GameplayReady";
        void Start()
        {
            EventsPublisher.Instance.PublishEvent(_eventName, this, null);
        }
    }
}