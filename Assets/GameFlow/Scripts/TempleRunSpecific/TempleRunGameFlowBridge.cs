using CrawfisSoftware.Events;
using CrawfisSoftware.GameFlow.Events;
using CrawfisSoftware.TempleRun;
using CrawfisSoftware.UGS;
using CrawfisSoftware.UGS.Events;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.GameFlow.Events
{
    internal class TempleRunGameFlowBridge : AutoEventFlowBase
    {
        private Dictionary<TempleRunEvents, GameFlowEvents> _autoTempleRun2GameFlowEvents = new Dictionary<TempleRunEvents, GameFlowEvents>()
        {
            // TempleRun paused -> request GameFlow pause (for menus/UI)
            { TempleRunEvents.PlayerPaused, GameFlowEvents.PauseRequested },

            // Countdown ended -> game officially started (absorbed from GameController)
            { TempleRunEvents.CountdownEnded, GameFlowEvents.GameStarted },

            // Player died -> game ending (absorbed from GameController)
            { TempleRunEvents.TempleRunEnded, GameFlowEvents.GameEnding },
        };

        private Dictionary<GameFlowEvents, TempleRunEvents> _autoGameFlow2TempleRunEvents = new Dictionary<GameFlowEvents, TempleRunEvents>()
        {
            // Bridge start: when the broader game signals started, fire TempleRun start requested
            { GameFlowEvents.GameStarted, TempleRunEvents.TempleRunStartRequested },

            // GameFlow starting -> kick off countdown in TempleRun
            { GameFlowEvents.GameStarting, TempleRunEvents.CountdownStartRequested },

            // Config/scenes bridged to TempleRun domain
            { GameFlowEvents.GameConfigApplied, TempleRunEvents.TempleRunConfigApplied },
            { GameFlowEvents.GameScenesLoaded, TempleRunEvents.TempleRunScenesReady },
        };

        // TempleRun → UGS passthrough (bypasses GameFlow — this bridge is the authorized crossing point)
        private Dictionary<TempleRunEvents, UGS_EventsEnum> _autoTempleRun2UGSEvents = new Dictionary<TempleRunEvents, UGS_EventsEnum>()
        {
            // Distance updates for achievement tracking
            { TempleRunEvents.DistanceUpdated, UGS_EventsEnum.DistanceUpdated },

            // Coin collection for economy sync and achievement tracking
            { TempleRunEvents.CoinCollected, UGS_EventsEnum.CoinUpdated },
        };

        protected virtual void Awake()
        {
            EventsPublisherTempleRun.Instance.SubscribeToAllEnumEvents(AutoFireGameFlowEventFromTempleRunEvent);
            EventsPublisherGameFlow.Instance.SubscribeToAllEnumEvents(AutoFireTempleRunEventFromGameFlowEvent);
            EventsPublisherTempleRun.Instance.SubscribeToAllEnumEvents(AutoFireUGSEventFromTempleRunEvent);
        }

        protected virtual void OnDestroy()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToAllEnumEvents(AutoFireGameFlowEventFromTempleRunEvent);
            EventsPublisherGameFlow.Instance.UnsubscribeToAllEnumEvents(AutoFireTempleRunEventFromGameFlowEvent);
            EventsPublisherTempleRun.Instance.UnsubscribeToAllEnumEvents(AutoFireUGSEventFromTempleRunEvent);
        }

        private void AutoFireGameFlowEventFromTempleRunEvent(string eventName, object sender, object data)
        {
            if (_autoTempleRun2GameFlowEvents.TryGetValue((TempleRunEvents)Enum.Parse(typeof(TempleRunEvents), eventName), out GameFlowEvents autoEvent))
            {
                DelayedFire(_delayBetweenEvents, autoEvent.ToString(), sender, data);
            }
        }

        private void AutoFireUGSEventFromTempleRunEvent(string eventName, object sender, object data)
        {
            if (EventsPublisherUGS.Instance == null) return;
            if (_autoTempleRun2UGSEvents.TryGetValue((TempleRunEvents)Enum.Parse(typeof(TempleRunEvents), eventName), out UGS_EventsEnum autoEvent))
            {
                EventsPublisherUGS.Instance.PublishEvent(autoEvent, sender, data);
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