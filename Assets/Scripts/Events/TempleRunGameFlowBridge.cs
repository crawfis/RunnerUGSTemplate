using CrawfisSoftware.TempleRun;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.Events
{
    internal class TempleRunGameFlowBridge : AutoEventFlowBase
    {
        [SerializeField] private Dictionary<TempleRunEvents, GameFlowEvents> _autoTempleRun2GameFlowEvents = new Dictionary<TempleRunEvents, GameFlowEvents>()
        {
            // Example: bridge Temple Run failures into the GameFlow end sequence
            //{ TempleRunEvents.PlayerFailed, GameFlowEvents.GameEndRequested },
            //{ TempleRunEvents.PlayerDied, GameFlowEvents.GameEndRequested },

            // TempleRun paused -> request GameFlow pause (for menus/UI)
            { TempleRunEvents.PlayerPaused, GameFlowEvents.PauseRequested },
        };

        [SerializeField] private Dictionary<GameFlowEvents, TempleRunEvents> _autoGameFlow2TempleRunEvents = new Dictionary<GameFlowEvents, TempleRunEvents>()
        {
            // Example: drive Temple Run pause/resume hooks from GameFlow
            //{ GameFlowEvents.PauseRequested, TempleRunEvents.PlayerPause },
            //{ GameFlowEvents.ResumeRequested, TempleRunEvents.PlayerResume },

            // Bridge start: when the broader game signals started, fire TempleRun start requested
            { GameFlowEvents.GameStarted, TempleRunEvents.TempleRunStartRequested },

            // GameFlow resumed -> request TempleRun resume (e.g., after menus)
            { GameFlowEvents.Resumed, TempleRunEvents.PlayerResumeRequested },
        };

        protected virtual void Awake()
        {
            EventsPublisherTempleRun.Instance.SubscribeToAllEnumEvents(AutoFireGameFlowEventFromTempleRunEvent);
            EventsPublisherGameFlow.Instance.SubscribeToAllEnumEvents(AutoFireTempleRunEventFromGameFlowEvent);
        }

        protected virtual void OnDestroy()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToAllEnumEvents(AutoFireGameFlowEventFromTempleRunEvent);
            EventsPublisherGameFlow.Instance.UnsubscribeToAllEnumEvents(AutoFireTempleRunEventFromGameFlowEvent);
        }

        private void AutoFireGameFlowEventFromTempleRunEvent(string eventName, object sender, object data)
        {
            if (_autoTempleRun2GameFlowEvents.TryGetValue((TempleRunEvents)Enum.Parse(typeof(TempleRunEvents), eventName), out GameFlowEvents autoEvent))
            {
                DelayedFire(_delayBetweenEvents, autoEvent.ToString(), sender, data);
            }
        }

        private void AutoFireTempleRunEventFromGameFlowEvent(string eventName, object sender, object data)
        {
            if (_autoGameFlow2TempleRunEvents.TryGetValue((GameFlowEvents)Enum.Parse(typeof(GameFlowEvents), eventName), out TempleRunEvents autoEvent))
            {
                DelayedFire(_delayBetweenEvents, autoEvent.ToString(), sender, data);
            }
        }
    }
}