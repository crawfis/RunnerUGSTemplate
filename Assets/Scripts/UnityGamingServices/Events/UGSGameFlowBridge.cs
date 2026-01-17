using CrawfisSoftware.Events;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.UGS.Events
{
    internal class UGSGameFlowBridge : AutoEventFlowBase
    {
        [SerializeField] private Dictionary<UGS_EventsEnum, GameFlowEvents> _autoUGS2GameFlowEvents = new Dictionary<UGS_EventsEnum, GameFlowEvents>()
        {
            { UGS_EventsEnum.PlayerAuthenticated, GameFlowEvents.GameplayReady },
            { UGS_EventsEnum.PlayerSignedOut, GameFlowEvents.GameplayNotReady },

            // Remote config update requests the loading screen to hide; flow auto-fires LoadingScreenHiding.
            { UGS_EventsEnum.RemoteConfigUpdated, GameFlowEvents.LoadingScreenHideRequested },
        };

        [SerializeField] private Dictionary<GameFlowEvents, UGS_EventsEnum> _autoGameFlow2UGSEvents = new Dictionary<GameFlowEvents, UGS_EventsEnum>()
        {
            { GameFlowEvents.GameEnding, UGS_EventsEnum.ScoreUpdating },
            { GameFlowEvents.GameEnded, UGS_EventsEnum.LeaderboardOpening },

            // Alternative: kick leaderboard earlier
            //{ GameFlowEvents.GameScenesUnloaded, UGS_EventsEnum.LeaderboardOpening },
        };

        protected virtual void Awake()
        {
            EventsPublisherUGS.Instance.SubscribeToAllEnumEvents(AutoFireGameFlowEventFromUGSEvent);
            EventsPublisherGameFlow.Instance.SubscribeToAllEnumEvents(AutoFireUGSEventFromGameFlowEvent);
        }

        protected virtual void OnDestroy()
        {
            EventsPublisherUGS.Instance.UnsubscribeToAllEnumEvents(AutoFireGameFlowEventFromUGSEvent);
            EventsPublisherGameFlow.Instance.UnsubscribeToAllEnumEvents(AutoFireUGSEventFromGameFlowEvent);
        }

        private void AutoFireGameFlowEventFromUGSEvent(string eventName, object sender, object data)
        {
            if (_autoUGS2GameFlowEvents.TryGetValue((UGS_EventsEnum)Enum.Parse(typeof(UGS_EventsEnum), eventName), out GameFlowEvents autoEvent))
            {
                DelayedFire(_delayBetweenEvents, autoEvent.ToString(), sender, data);
            }
        }

        private void AutoFireUGSEventFromGameFlowEvent(string eventName, object sender, object data)
        {
            if (_autoGameFlow2UGSEvents.TryGetValue((GameFlowEvents)Enum.Parse(typeof(GameFlowEvents), eventName), out UGS_EventsEnum autoEvent))
            {
                DelayedFire(_delayBetweenEvents, autoEvent.ToString(), sender, data);
            }
        }
    }
}