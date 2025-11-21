using CrawfisSoftware.Events;
using CrawfisSoftware.GameConfig;

using System.Collections;

using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Handles quitting.
    ///    Dependency: EventsPublisherTempleRun
    ///    Subscribes: GameOver - Currently it quits the application.
    /// </summary>
    public class GameQuittedController : MonoBehaviour
    {
        private void Start()
        {
            EventsPublisherTempleRun.Instance.SubscribeToEvent(GamePlayEvents.Quitting, OnQuitted);
        }

        private void OnQuitted(string EventName, object sender, object data)
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(GamePlayEvents.Quitting, OnQuitted);
            StartCoroutine(Quit());
        }
        private IEnumerator Quit()
        {
            yield return new WaitForSecondsRealtime(GameConstants.QuitDelay);

            // This shows the proper way to quit a game both in Editor and with a build
#if UNITY_EDITOR
            EventsPublisher publisher = (EventsPublisher)(EventsPublisher.Instance);
            foreach ((string eventName, string targetName) subscriberData in publisher.GetSubscribers())
            {
                Debug.LogWarning($"{subscriberData.targetName} did not unsubscribe {subscriberData.eventName}.");
            }
            // Needed in Unity editor to clear any subscribers who forgot to unsubscribe.
            // Best to unsubscribe in the OnDestroy method of the subscriber.
            //If lazy, uncomment the next line to clear all subscribers.
            EventsPublisher.Instance.Clear();
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}