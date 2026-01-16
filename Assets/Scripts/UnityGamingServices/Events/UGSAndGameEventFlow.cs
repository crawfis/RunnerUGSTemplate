using CrawfisSoftware.UGS;

using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Services.Core;

using UnityEngine;

namespace CrawfisSoftware.Events
{
    internal class UGSAndGameEventFlow : MonoBehaviour
    {
        [SerializeField] private float _delayBetweenEvents = 0f;

        // The Commented out events are not auto-fired here because they are fired by other components in the UGS framework.
        // They are left here as comments for reference of the full flow.

        protected Dictionary<UGS_EventsEnum, UGS_EventsEnum> _autoUGS2UGSEvents = new Dictionary<UGS_EventsEnum, UGS_EventsEnum>()
        {
            // UnityServicesInitialized is fired by Unity's ServicesInitialization component in the InitializeServices GameObject in the UGS_Boot_0_Initialization scene.
            // Since that happens before any of this is set-up, we need to handle it specially in Start()
            { UGS_EventsEnum.UnityServicesInitialized, UGS_EventsEnum.CheckForExistingSession },
            { UGS_EventsEnum.PlayerAuthenticated, UGS_EventsEnum.RemoteConfigFetching }, // First time and anytime the player changes.
            //{ UGS_EventsEnum.RemoteConfigFetching, UGS_EventsEnum.RemoteConfigFetched },
            //{ UGS_EventsEnum.RemoteConfigFetched, UGS_EventsEnum.RemoteConfigUpdated },
            //{ UGS_EventsEnum.RemoteConfigUpdated, UGS_EventsEnum.CheckForExistingSession },
            //{ UGS_EventsEnum.CheckForExistingSession, UGS_EventsEnum.CheckForExistingSessionSucceeded },
            { UGS_EventsEnum.CheckForExistingSessionSucceeded, UGS_EventsEnum.PlayerAuthenticating },
            { UGS_EventsEnum.CheckForExistingSessionFailed, UGS_EventsEnum.PlayerSigningIn },
            //{ UGS_EventsEnum.PlayerSigningIn, UGS_EventsEnum.PlayerSignedIn }, // PlayerSignedIn is fired by PlayerSignInController
            { UGS_EventsEnum.PlayerSignedIn, UGS_EventsEnum.PlayerAuthenticating },
            //{ UGS_EventsEnum.PlayerAuthenticating, UGS_EventsEnum.PlayerAuthenticated }, // PlayerAuthenticated is fired by PlayerSignInController
            //{ UGS_EventsEnum.PlayerSigningOut, UGS_EventsEnum.PlayerSignedOut },
            { UGS_EventsEnum.PlayerSignedOut, UGS_EventsEnum.PlayerSigningIn }, // Loop back to PlayerSigningIn to allow re-sign in
            { UGS_EventsEnum.PlayerSignInFailed, UGS_EventsEnum.PlayerSigningIn }, // Loop back to PlayerSigningIn to allow re-sign in
            { UGS_EventsEnum.ScoreUpdating, UGS_EventsEnum.ScoreUpdated },
            //{ UGS_EventsEnum.LeaderboardOpening, UGS_EventsEnum.LeaderboardOpened }, // Published by: LeaderboardController
            //{ UGS_EventsEnum.LeaderboardOpened, UGS_EventsEnum.LeaderboardClosing },
            //{ UGS_EventsEnum.LeaderboardClosing, UGS_EventsEnum.LeaderboardClosed },
            { UGS_EventsEnum.LeaderboardClosed, UGS_EventsEnum.AchievementsOpening },
            //{ UGS_EventsEnum.AchievementsOpening, UGS_EventsEnum.AchievementsOpened },
            //{ UGS_EventsEnum.AchievementsOpened, UGS_EventsEnum.AchievementsClosing },
            //{ UGS_EventsEnum.AchievementsClosing, UGS_EventsEnum.AchievementsClosed },
            { UGS_EventsEnum.AchievementsClosed, UGS_EventsEnum.RewardAdWatching },
            { UGS_EventsEnum.RewardAdWatching, UGS_EventsEnum.RewardAdWatched },
            { UGS_EventsEnum.RewardAdWatched, UGS_EventsEnum.PlayerAuthenticating }, // Loop back to PlayerAuthenticating for continuous flow and a check on whether the player is still authenticated
        };
        protected Dictionary<UGS_EventsEnum, GameFlowEvents> _autoUGS2GameFlowEvents = new Dictionary<UGS_EventsEnum, GameFlowEvents>()
        {
            { UGS_EventsEnum.PlayerAuthenticated, GameFlowEvents.GameplayReady },
            { UGS_EventsEnum.PlayerSignedOut, GameFlowEvents.GameplayNotReady },
            { UGS_EventsEnum.RemoteConfigUpdated, GameFlowEvents.LoadingScreenHidding },
        };

        protected Dictionary<GameFlowEvents, UGS_EventsEnum> _autoGameFlow2UGSEvents = new Dictionary<GameFlowEvents, UGS_EventsEnum>()
        {
            { GameFlowEvents.GameEnding, UGS_EventsEnum.ScoreUpdating },
            { GameFlowEvents.GameEnded, UGS_EventsEnum.LeaderboardOpening },
            //{ GameFlowEvents.GameScenesUnloaded, UGS_EventsEnum.LeaderboardOpening },
        };

        protected Dictionary<GameFlowEvents, GameFlowEvents> _autoGameFlow2GameFlowEvents = new Dictionary<GameFlowEvents, GameFlowEvents>()
        {
            //{ GameFlowEvents.LoadingScreenShowing, GameFlowEvents.LoadingScreenShown }, // (Inititally) Fired by UIPanelController in the Game_Boot_1_UI
            //{ GameFlowEvents.LoadingScreenShown, GameFlowEvents.LoadingScreenHidding }, // => Start Hidding Loading
            //{ GameFlowEvents.LoadingScreenHidding, GameFlowEvents.LoadingScreenHidden }, // LoadingScreenHidden is fired by UIPanelController in Game_Boot_1_UI
            //{ GameFlowEvents.LoadingScreenHidden, GameFlowEvents.GameplayReady },
            { GameFlowEvents.GameplayReady, GameFlowEvents.MainMenuShowing }, // => Show Menu
            //{ GameFlowEvents.GameplayNotReady, GameFlowEvents.MainMenuHidden }, // => Hide Menu
            //{ GameFlowEvents.MainMenuShowing, GameFlowEvents.MainMenuShown }, // MainMenuShown is fired by MainMenuPanelController in Game_Boot_1_UI scene
            //{ GameFlowEvents.GameScenesLoading, GameFlowEvents.MainMenuHidden }, // => Press Play, Hide Menu
            //{ GameFlowEvents.MainMenuHidden, GameFlowEvents.GameScenesLoaded },
            { GameFlowEvents.GameScenesLoaded, GameFlowEvents.GameStarting },
            { GameFlowEvents.GameStarting, GameFlowEvents.CountdownStarting },
            //{ GameFlowEvents.CountdownStarting, GameFlowEvents.CountdownStarted },
            //{ GameFlowEvents.CountdownStarted, GameFlowEvents.CountdownTick },
            //{ GameFlowEvents.CountdownEnding, GameFlowEvents.CountdownEnded },
            //{ GameFlowEvents.CountdownEnded, GameFlowEvents.GameStarted },
            //{ GameFlowEvents.GameEnding, GameFlowEvents.GameFlowEvents.GameEnded },
            //{ GameFlowEvents.GameScenesUnloading, GameFlowEvents.GameScenesUnloaded },
            //{ GameFlowEvents.GameEnded, GameFlowEvents.GameScenesUnloading },
            { GameFlowEvents.GameEnding, GameFlowEvents.GameScenesUnloading },
            //{ GameFlowEvents.GameScenesUnloaded, GameFlowEvents.GameplayReady  }, //=> Loop back to Show Menu
            //{ GameFlowEvents.Pause, GameFlowEvents.Resume  },
            //{ GameFlowEvents.Quitting, GameFlowEvents.Quitted }, // Quitted is fired by th Quitting GameObject in the 0_BootStrap scene
        };

        protected virtual void Awake()
        {
            EventsPublisherUGS.Instance.SubscribeToAllEnumEvents(AutoFireUGSEventFromUGSEvent);
            EventsPublisherUGS.Instance.SubscribeToAllEnumEvents(AutoFireGameFlowEventFromUGSEvent);
            EventsPublisherGameFlow.Instance.SubscribeToAllEnumEvents(AutoFireUGSEventFromGameFlowEvent);
            EventsPublisherGameFlow.Instance.SubscribeToAllEnumEvents(AutoFireGameFlowEventFromGameFlowEvent);
        }
        protected virtual void OnDestroy()
        {
            EventsPublisherUGS.Instance.UnsubscribeToAllEnumEvents(AutoFireUGSEventFromUGSEvent);
            EventsPublisherUGS.Instance.UnsubscribeToAllEnumEvents(AutoFireGameFlowEventFromUGSEvent);
            EventsPublisherGameFlow.Instance.UnsubscribeToAllEnumEvents(AutoFireUGSEventFromGameFlowEvent);
            EventsPublisherGameFlow.Instance.UnsubscribeToAllEnumEvents(AutoFireGameFlowEventFromGameFlowEvent);
        }

        protected virtual void Start()
        {
            if(UnityServices.State == ServicesInitializationState.Initialized)
            {
                DelayedFire(0, UGS_EventsEnum.UnityServicesInitialized.ToString(), this, null);
            }
        }
        protected void AutoFireUGSEventFromUGSEvent(string eventName, object sender, object data)
        {
            if (_autoUGS2UGSEvents.TryGetValue((UGS_EventsEnum)Enum.Parse(typeof(UGS_EventsEnum), eventName), out UGS_EventsEnum autoEvent))
            {
                DelayedFire(_delayBetweenEvents, autoEvent.ToString(), sender, data);
            }
        }

        protected void AutoFireUGSEventFromGameFlowEvent(string eventName, object sender, object data)
        {
            if (_autoGameFlow2UGSEvents.TryGetValue((GameFlowEvents)Enum.Parse(typeof(GameFlowEvents), eventName), out UGS_EventsEnum autoEvent))
            {
                DelayedFire(_delayBetweenEvents, autoEvent.ToString(), sender, data);
            }
        }

        protected void AutoFireGameFlowEventFromUGSEvent(string eventName, object sender, object data)
        {
            if (_autoUGS2GameFlowEvents.TryGetValue((UGS_EventsEnum)Enum.Parse(typeof(UGS_EventsEnum), eventName), out GameFlowEvents autoEvent))
            {
                DelayedFire(_delayBetweenEvents, autoEvent.ToString(), sender, data);
            }
        }

        protected void AutoFireGameFlowEventFromGameFlowEvent(string eventName, object sender, object data)
        {
            if (_autoGameFlow2GameFlowEvents.TryGetValue((GameFlowEvents)Enum.Parse(typeof(GameFlowEvents), eventName), out GameFlowEvents autoEvent))
            {
                DelayedFire(_delayBetweenEvents, autoEvent.ToString(), sender, data);
            }
        }

        private void DelayedFire(float delayBetweenEvents, string eventName, object sender, object data)
        {
            if (delayBetweenEvents <= 0)
            {
                //EventsPublisher.Instance.PublishEvent(eventName, sender, data);
                EventsPublisher.Instance.PublishEvent(eventName, this, data);
                return;
            }
            StartCoroutine(DelayPublishingNextAutoEvent(eventName, sender, data));
        }

        private IEnumerator DelayPublishingNextAutoEvent(string eventName, object sender, object data)
        {
            yield return new WaitForSeconds(_delayBetweenEvents);
            //EventsPublisher.Instance.PublishEvent(eventName, sender, data);
            EventsPublisher.Instance.PublishEvent(eventName, this, data);
        }
    }
}