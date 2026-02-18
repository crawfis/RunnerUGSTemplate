using CrawfisSoftware.Events;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.TempleRun.Events
{
    /// <summary>
    /// Auto-chain TempleRun-specific events. Keep this focused on TempleRun internal lifecycles;
    /// cross-system bridges live in TempleRunGameFlowBridge.
    /// </summary>
    internal class TempleRunAutoEventFlow : AutoEventFlowBase
    {
        [SerializeField] private Dictionary<TempleRunEvents, TempleRunEvents> _autoTempleRun2TempleRunEvents = new Dictionary<TempleRunEvents, TempleRunEvents>()
        {
            // ================================================================================
            // PAUSE / RESUME BRIDGES (mirror GameFlowAutoEventFlow)
            // ================================================================================
            { TempleRunEvents.PlayerPauseRequested, TempleRunEvents.PlayerPausing },
            { TempleRunEvents.PlayerPausing, TempleRunEvents.PlayerPaused },
            { TempleRunEvents.PlayerResumeRequested, TempleRunEvents.PlayerResuming },
            { TempleRunEvents.PlayerResuming, TempleRunEvents.PlayerResumed },

            // ================================================================================
            // COUNTDOWN BRIDGE (mirror GameFlowAutoEventFlow)
            // ================================================================================
            { TempleRunEvents.CountdownStartRequested, TempleRunEvents.CountdownStarting },
            // CountdownStarting -> CountdownTick(s) -> CountdownEnding -> CountdownEnded: published elsewhere

            // ================================================================================
            // GAME START BRIDGE
            // ================================================================================
            { TempleRunEvents.TempleRunStartRequested, TempleRunEvents.TempleRunStarting },
            { TempleRunEvents.TempleRunStarting, TempleRunEvents.TempleRunStarted },
        };

        protected virtual void Awake()
        {
            EventsPublisherTempleRun.Instance.SubscribeToAllEnumEvents(AutoFireTempleRunEventFromTempleRunEvent);
        }

        protected virtual void OnDestroy()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToAllEnumEvents(AutoFireTempleRunEventFromTempleRunEvent);
        }

        private void AutoFireTempleRunEventFromTempleRunEvent(string eventName, object sender, object data)
        {
            if (_autoTempleRun2TempleRunEvents.TryGetValue((TempleRunEvents)Enum.Parse(typeof(TempleRunEvents), eventName), out TempleRunEvents autoEvent))
            {
                DelayedFire(_delayBetweenEvents, autoEvent.ToString(), sender, data);
            }
        }
    }
}
