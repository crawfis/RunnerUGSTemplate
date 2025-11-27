using CrawfisSoftware.Events;

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.UGS.Events
{
    internal class DEBUG_UGSOnlyEventFlow : MonoBehaviour
    {
        [SerializeField] private float _delayBetweenEvents = 0.1f;

        protected readonly Dictionary<UGS_EventsEnum, UGS_EventsEnum> _autoUGS2UGSEvents = new Dictionary<UGS_EventsEnum, UGS_EventsEnum>()
        {
            { UGS_EventsEnum.UnityServicesInitialized, UGS_EventsEnum.RemoteConfigFetching },
            { UGS_EventsEnum.RemoteConfigFetching, UGS_EventsEnum.RemoteConfigFetched },
            { UGS_EventsEnum.RemoteConfigFetched, UGS_EventsEnum.RemoteConfigUpdated },
            { UGS_EventsEnum.RemoteConfigUpdated, UGS_EventsEnum.PlayerSigningIn },
            { UGS_EventsEnum.PlayerSigningIn, UGS_EventsEnum.PlayerSignedIn },
            { UGS_EventsEnum.PlayerSignedIn, UGS_EventsEnum.PlayerAuthenticating },
            { UGS_EventsEnum.PlayerAuthenticating, UGS_EventsEnum.PlayerAuthenticated },
            { UGS_EventsEnum.PlayerSigningOut, UGS_EventsEnum.PlayerSignedOut },
            { UGS_EventsEnum.ScoreUpdating, UGS_EventsEnum.ScoreUpdated },
            { UGS_EventsEnum.LeaderboardOpening, UGS_EventsEnum.LeaderboardOpened  },
            { UGS_EventsEnum.LeaderboardOpened, UGS_EventsEnum.LeaderboardClosing },
            { UGS_EventsEnum.LeaderboardClosing, UGS_EventsEnum.LeaderboardClosed },
            { UGS_EventsEnum.LeaderboardClosed, UGS_EventsEnum.AchievementsOpening },
            { UGS_EventsEnum.AchievementsOpening, UGS_EventsEnum.AchievementsOpened },
            { UGS_EventsEnum.AchievementsOpened, UGS_EventsEnum.AchievementsClosing },
            { UGS_EventsEnum.AchievementsClosing, UGS_EventsEnum.AchievementsClosed },
            { UGS_EventsEnum.AchievementsClosed, UGS_EventsEnum.RewardAdWatching },
            { UGS_EventsEnum.RewardAdWatching, UGS_EventsEnum.RewardAdWatched },
            { UGS_EventsEnum.RewardAdWatched, UGS_EventsEnum.PlayerAuthenticating }, // Loop back to PlayerAuthenticating for continuous flow and a check on whether the player is still authenticated
        };
        protected readonly Dictionary<UGS_EventsEnum, GameFlowEvents> _autoUGS2GameFlowEvents = new Dictionary<UGS_EventsEnum, GameFlowEvents>()
        {
            { UGS_EventsEnum.PlayerAuthenticated, GameFlowEvents.GameplayReady }, 
        };

        protected readonly Dictionary<GameFlowEvents, UGS_EventsEnum> _autoGameFlow2UGSEvents = new Dictionary<GameFlowEvents, UGS_EventsEnum>()
        {
            { GameFlowEvents.GameScenesUnloaded, UGS_EventsEnum.LeaderboardOpening },
        };

        protected readonly Dictionary<GameFlowEvents, GameFlowEvents> _autoGameFlow2GameFlowEvents = new Dictionary<GameFlowEvents, GameFlowEvents>()
        {
            { GameFlowEvents.GameEnding, GameFlowEvents.GameEnded },
            { GameFlowEvents.GameEnded, GameFlowEvents.GameScenesUnloading },
            { GameFlowEvents.GameScenesUnloading, GameFlowEvents.GameScenesUnloaded },
        };

        private void Awake()
        {
            EventsPublisherUGS.Instance.SubscribeToAllEnumEvents(AutoFireUGSEventFromUGSEvent);
            EventsPublisherUGS.Instance.SubscribeToAllEnumEvents(AutoFireGameFlowEventFromUGSEvent);
            EventsPublisherGameFlow.Instance.SubscribeToAllEnumEvents(AutoFireUGSEventFromGameFlowEvent);
        }

        private void AutoFireUGSEventFromUGSEvent(string eventName, object sender, object data)
        {
            if (_autoUGS2UGSEvents.TryGetValue((UGS_EventsEnum)Enum.Parse(typeof(UGS_EventsEnum), eventName), out UGS_EventsEnum autoEvent))
            {
                StartCoroutine(DelayPublishingNextAutoEvent(autoEvent.ToString(), sender, data));
            }
        }

        private void AutoFireUGSEventFromGameFlowEvent(string eventName, object sender, object data)
        {
            if (_autoGameFlow2UGSEvents.TryGetValue((GameFlowEvents)Enum.Parse(typeof(GameFlowEvents), eventName), out UGS_EventsEnum autoEvent))
            {
                StartCoroutine(DelayPublishingNextAutoEvent(autoEvent.ToString(), sender, data));
            }
        }

        private void AutoFireGameFlowEventFromUGSEvent(string eventName, object sender, object data)
        {
            if (_autoUGS2GameFlowEvents.TryGetValue((UGS_EventsEnum)Enum.Parse(typeof(UGS_EventsEnum), eventName), out GameFlowEvents autoEvent))
            {
                StartCoroutine(DelayPublishingNextAutoEvent(autoEvent.ToString(), sender, data));
            }
        }

        private IEnumerator DelayPublishingNextAutoEvent(string eventName, object sender, object data)
        {
            yield return new WaitForSeconds(_delayBetweenEvents);
            EventsPublisher.Instance.PublishEvent(eventName, sender, data);
        }
    }
}