using CrawfisSoftware.Events;
using CrawfisSoftware.GameConfig;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Handles quitting.
    ///    Dependency: EventsPublisherTempleRun
    ///    Subscribes: GameOver - Currently it quits the application.
    /// </summary>
    public class GameQuittingController : MonoBehaviour
    {
        private void Start()
        {
            EventsPublisherTempleRun.Instance.SubscribeToEvent(GamePlayEvents.Quit, OnQuitting);
        }

        private void OnQuitting(string EventName, object sender, object data)
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(GamePlayEvents.Quit, OnQuitting);
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
