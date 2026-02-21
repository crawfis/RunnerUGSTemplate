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

        protected virtual void Awake()
        {
            EventsPublisherTempleRun.Instance.SubscribeToAllEnumEvents(AutoFireGameFlowEventFromTempleRunEvent);
            EventsPublisherGameFlow.Instance.SubscribeToAllEnumEvents(AutoFireTempleRunEventFromGameFlowEvent);

            // Todo: This should auto fire a GameFlow Distance updated which then should fire a UGS Distance updated.
            // Manual bridge: TempleRun distance updates → UGS distance tracking
            EventsPublisherTempleRun.Instance.SubscribeToEvent(
                TempleRunEvents.DistanceUpdated, OnDistanceUpdated);
        }

        protected virtual void OnDestroy()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToAllEnumEvents(AutoFireGameFlowEventFromTempleRunEvent);
            EventsPublisherGameFlow.Instance.UnsubscribeToAllEnumEvents(AutoFireTempleRunEventFromGameFlowEvent);

            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(
                TempleRunEvents.DistanceUpdated, OnDistanceUpdated);
        }

        private void OnDistanceUpdated(string eventName, object sender, object data)
        {
            // Bridge TempleRun distance updates to UGS for achievement tracking
            // Only publish if UGS is initialized (may not be available in all boot sequences)
            if (EventsPublisherUGS.Instance != null)
            {
                EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.DistanceUpdated, sender, data);
            }
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