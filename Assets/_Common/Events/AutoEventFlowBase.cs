using System.Collections;

using UnityEngine;

namespace CrawfisSoftware.Events
{
    internal abstract class AutoEventFlowBase : MonoBehaviour
    {
        [SerializeField] protected float _delayBetweenEvents = 0f;
        protected void DelayedFire(float delayBetweenEvents, string eventName, object sender, object data)
        {
            if (delayBetweenEvents <= 0)
            {
                EventsPublisher.Instance.PublishEvent(eventName, this, data);
                return;
            }
            StartCoroutine(DelayPublishingNextAutoEvent(delayBetweenEvents, eventName, sender, data));
        }

        private IEnumerator DelayPublishingNextAutoEvent(float delay, string eventName, object sender, object data)
        {
            yield return new WaitForSeconds(delay);
            EventsPublisher.Instance.PublishEvent(eventName, this, data);
        }
    }
}
