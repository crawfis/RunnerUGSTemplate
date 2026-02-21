using CrawfisSoftware.Events;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.TempleRun.Events
{
    internal class Input2TempleRunAutoEventBridge : AutoEventFlowBase
    {
        private Dictionary<UserInitiatedEvents, TempleRunEvents> _autoUserInitiated2TempleRunEvents = new Dictionary<UserInitiatedEvents, TempleRunEvents>()
        {
            // User input bridges: raw input events -> gameplay events
            // This allows gameplay mechanics to be triggered from any source (player input, AI, replay, network)
            // Controllers subscribe to TempleRun domain events, not UserInitiated events
            { UserInitiatedEvents.UserQuitRequested, TempleRunEvents.TempleRunEndRequested },
            { UserInitiatedEvents.UserSlideRequested, TempleRunEvents.SlideRequested },
            { UserInitiatedEvents.UserDashRequested, TempleRunEvents.DashRequested },
        };

        protected virtual void Awake()
        {
            EventsPublisherUserInitiated.Instance.SubscribeToAllEnumEvents(AutoFireTempleRunEventFromUserInitiatedEvent);
        }

        protected virtual void OnDestroy()
        {
            EventsPublisherUserInitiated.Instance.UnsubscribeToAllEnumEvents(AutoFireTempleRunEventFromUserInitiatedEvent);
        }

        private void AutoFireTempleRunEventFromUserInitiatedEvent(string eventName, object sender, object data)
        {
            if (_autoUserInitiated2TempleRunEvents.TryGetValue((UserInitiatedEvents)Enum.Parse(typeof(UserInitiatedEvents), eventName), out TempleRunEvents autoEvent))
            {
                //Debug.Log($"Auto firing event UserInitiatedEvents.{eventName} to TempleRunEvents.{autoEvent.ToString()}");
                DelayedFire(_delayBetweenEvents, autoEvent.ToString(), sender, data);
            }
        }
    }
}