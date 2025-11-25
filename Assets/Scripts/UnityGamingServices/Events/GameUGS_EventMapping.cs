using CrawfisSoftware.Events;
using CrawfisSoftware.TempleRun;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.UGS.Events
{
    public class GameUGS_EventMapping : MonoBehaviour
    {
        protected Dictionary<GameFlowEvents, UGS_EventsEnum> gameToUGSEventMapping = new Dictionary<GameFlowEvents, UGS_EventsEnum>()
        {
            { GameFlowEvents.GameEnded, UGS_EventsEnum.LeaderboardOpening },
        };
        protected Dictionary<UGS_EventsEnum, GameFlowEvents> ugsToGameEventMapping = new Dictionary<UGS_EventsEnum, GameFlowEvents>() {
            { UGS_EventsEnum.PlayerAuthenticated, GameFlowEvents.GameplayReady },
        };
        private void Awake()
        {
            UpdateMappings();
            SubscribeToGameEvents();
            SubscribeToUGSEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeToGameEvents();
            UnsubscribeToUGSEvents();
        }

        protected virtual void UpdateMappings()
        {
            UpdateGameToUGSMapping();
            UpdateUGSToGameMapping();
        }
        protected virtual void UpdateGameToUGSMapping()
        {
        }
        protected virtual void UpdateUGSToGameMapping()
        {
        }

        private void SubscribeToGameEvents()
        {
            foreach (var mapping in gameToUGSEventMapping.Keys)
            {
                EventsPublisherGameFlow.Instance.SubscribeToEvent(mapping, MapGameEventsToUGS);
            }
        }

        private void UnsubscribeToGameEvents()
        {
            foreach (var mapping in gameToUGSEventMapping.Keys)
            {
                EventsPublisherGameFlow.Instance.UnsubscribeToEvent(mapping, MapGameEventsToUGS);
            }
        }
        private void SubscribeToUGSEvents()
        {
            foreach (var mapping in ugsToGameEventMapping.Keys)
            {
                EventsPublisherUGS.Instance.SubscribeToEvent(mapping, MapUGSEventsToGame);
            }
        }
        private void UnsubscribeToUGSEvents()
        {
            foreach (var mapping in ugsToGameEventMapping.Keys)
            {
                EventsPublisherUGS.Instance.UnsubscribeToEvent(mapping, MapUGSEventsToGame);
            }
        }
        private void MapGameEventsToUGS(string eventName, object sender, object data)
        {
            if(gameToUGSEventMapping.TryGetValue((GameFlowEvents)Enum.Parse(typeof(GameFlowEvents), eventName), out UGS_EventsEnum ugsEventName))
            {
                EventsPublisherUGS.Instance.PublishEvent(ugsEventName, sender, data);
            }
        }

        private void MapUGSEventsToGame(string eventName, object sender, object data)
        {
            if (ugsToGameEventMapping.TryGetValue((UGS_EventsEnum)Enum.Parse(typeof(UGS_EventsEnum), eventName), out GameFlowEvents gameEventName))
            {
                EventsPublisherGameFlow.Instance.PublishEvent(gameEventName, sender, data);
            }
        }
    }
}