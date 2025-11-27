using CrawfisSoftware.Events;
using CrawfisSoftware.UGS;

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.Events
{
    internal class UGSAndGameEventFlow : MonoBehaviour
    {
        [SerializeField] private float _delayBetweenEvents = 0f;

        protected readonly Dictionary<UGS_EventsEnum, UGS_EventsEnum> _autoUGS2UGSEvents = new Dictionary<UGS_EventsEnum, UGS_EventsEnum>()
        {
            { UGS_EventsEnum.UnityServicesInitialized, UGS_EventsEnum.RemoteConfigFetching },
            //{ UGS_EventsEnum.RemoteConfigFetching, UGS_EventsEnum.RemoteConfigFetched },
            //{ UGS_EventsEnum.RemoteConfigFetched, UGS_EventsEnum.RemoteConfigUpdated },
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
            { UGS_EventsEnum.RemoteConfigUpdated, GameFlowEvents.LoadingScreenHidding },
        };

        protected readonly Dictionary<GameFlowEvents, UGS_EventsEnum> _autoGameFlow2UGSEvents = new Dictionary<GameFlowEvents, UGS_EventsEnum>()
        {
            { GameFlowEvents.GameScenesUnloaded, UGS_EventsEnum.LeaderboardOpening },
        };

        protected readonly Dictionary<GameFlowEvents, GameFlowEvents> _autoGameFlow2GameFlowEvents = new Dictionary<GameFlowEvents, GameFlowEvents>()
        {
            { GameFlowEvents.LoadingScreenShowing, GameFlowEvents.LoadingScreenShown }, // => Show Loading
            //{ GameFlowEvents.LoadingScreenShown, GameFlowEvents.LoadingScreenHidding }, // => Start Hidding Loading
            //{ GameFlowEvents.LoadingScreenHidding, GameFlowEvents.LoadingScreenHidden },
            //{ GameFlowEvents.LoadingScreenHidden, GameFlowEvents.GameplayReady },
            { GameFlowEvents.GameplayReady, GameFlowEvents.MainMenuShowing }, // => Show Menu
            { GameFlowEvents.MainMenuShowing, GameFlowEvents.MainMenuShown },
            { GameFlowEvents.GameScenesLoading, GameFlowEvents.MainMenuHidden }, // => Press Play, Hide Menu
            { GameFlowEvents.MainMenuHidden, GameFlowEvents.GameScenesLoaded },
            { GameFlowEvents.GameScenesLoaded, GameFlowEvents.GameStarting },
            { GameFlowEvents.GameStarting, GameFlowEvents.CountdownStarting },
            //{ GameFlowEvents.CountdownStarting, GameFlowEvents.CountdownStarted },
            //{ GameFlowEvents.CountdownStarted, GameFlowEvents.CountdownTick },
            //{ GameFlowEvents.CountdownTick, GameFlowEvents.GameStarted },
            { GameFlowEvents.GameEnding, GameFlowEvents.GameScenesUnloading },
            //{ GameFlowEvents.GameScenesUnloading, GameFlowEvents.GameScenesUnloaded },
            { GameFlowEvents.GameScenesUnloaded, GameFlowEvents.GameEnded },
            //{ GameFlowEvents.GameEnded, GameFlowEvents.GameplayReady  }, //=> Show Menu
            //{ GameFlowEvents.Pause, GameFlowEvents.Resume  },
            { GameFlowEvents.Quitting, GameFlowEvents.Quitted },
        };

        private void Awake()
        {
            EventsPublisherUGS.Instance.SubscribeToAllEnumEvents(AutoFireUGSEventFromUGSEvent);
            EventsPublisherUGS.Instance.SubscribeToAllEnumEvents(AutoFireGameFlowEventFromUGSEvent);
            EventsPublisherGameFlow.Instance.SubscribeToAllEnumEvents(AutoFireUGSEventFromGameFlowEvent);
            EventsPublisherGameFlow.Instance.SubscribeToAllEnumEvents(AutoFireGameFlowEventFromGameFlowEvent);
        }

        private void AutoFireUGSEventFromUGSEvent(string eventName, object sender, object data)
        {
            if (_autoUGS2UGSEvents.TryGetValue((UGS_EventsEnum)Enum.Parse(typeof(UGS_EventsEnum), eventName), out UGS_EventsEnum autoEvent))
            {
                DelayedFire(autoEvent.ToString(), sender, data);
            }
        }

        private void AutoFireUGSEventFromGameFlowEvent(string eventName, object sender, object data)
        {
            if (_autoGameFlow2UGSEvents.TryGetValue((GameFlowEvents)Enum.Parse(typeof(GameFlowEvents), eventName), out UGS_EventsEnum autoEvent))
            {
                DelayedFire(autoEvent.ToString(), sender, data);
            }
        }

        private void AutoFireGameFlowEventFromUGSEvent(string eventName, object sender, object data)
        {
            if (_autoUGS2GameFlowEvents.TryGetValue((UGS_EventsEnum)Enum.Parse(typeof(UGS_EventsEnum), eventName), out GameFlowEvents autoEvent))
            {
                DelayedFire(autoEvent.ToString(), sender, data);
            }
        }

        private void AutoFireGameFlowEventFromGameFlowEvent(string eventName, object sender, object data)
        {
            if (_autoGameFlow2GameFlowEvents.TryGetValue((GameFlowEvents)Enum.Parse(typeof(GameFlowEvents), eventName), out GameFlowEvents autoEvent))
            {
                DelayedFire(autoEvent.ToString(), sender, data);
            }
        }

        private void DelayedFire(string eventName, object sender, object data)
        {
            if (_delayBetweenEvents <= 0)
            {
                EventsPublisher.Instance.PublishEvent(eventName, sender, data);
                return;
            }
            StartCoroutine(DelayPublishingNextAutoEvent(eventName, sender, data));
        }

        private IEnumerator DelayPublishingNextAutoEvent(string eventName, object sender, object data)
        {
            yield return new WaitForSeconds(_delayBetweenEvents);
            EventsPublisher.Instance.PublishEvent(eventName, sender, data);
        }
    }
}