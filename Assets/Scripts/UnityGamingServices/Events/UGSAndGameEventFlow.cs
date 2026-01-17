using CrawfisSoftware.Events;

using System;
using System.Collections.Generic;

using Unity.Services.Core;

namespace CrawfisSoftware.UGS.Events
{
    internal class UGSAndGameEventFlow : AutoEventFlowBase
    {
    
        // --------------------------------------------------------------------------------------------
        // UGS -> UGS timeline
        // --------------------------------------------------------------------------------------------
        // Commented out events are not auto-fired here because they are fired by other components.
        // They are left as comments so this file is the "single source of truth" timeline.
        protected Dictionary<UGS_EventsEnum, UGS_EventsEnum> _newAutoUGS2UGSEvents = new Dictionary<UGS_EventsEnum, UGS_EventsEnum>()
        {
            // --- Initialization / boot ---
            // UnityServicesInitialized is fired by Unity's ServicesInitialization component.
            // Since that happens before any of this is set-up, we need to handle it specially in Start()
            //{ UGS_EventsEnum.UGS_InitializationStarted, UGS_EventsEnum.UGS_InitializationCompleted },
            { UGS_EventsEnum.UnityServicesInitialized, UGS_EventsEnum.CheckForExistingSession },
            //{ UGS_EventsEnum.UnityServicesInitializationFailed, UGS_EventsEnum.UGS_InitializationFailed },

            // Session -> Auth
            //{ UGS_EventsEnum.CheckForExistingSession, UGS_EventsEnum.CheckForExistingSessionSucceeded },
            { UGS_EventsEnum.CheckForExistingSessionSucceeded, UGS_EventsEnum.PlayerAuthenticating },
            { UGS_EventsEnum.CheckForExistingSessionFailed, UGS_EventsEnum.PlayerSigningIn },

            // Sign in loop
            //{ UGS_EventsEnum.PlayerSigningIn, UGS_EventsEnum.PlayerSignedIn },          // Published by PlayerSignInController
            { UGS_EventsEnum.PlayerSignedIn, UGS_EventsEnum.PlayerAuthenticating },
            //{ UGS_EventsEnum.PlayerAuthenticating, UGS_EventsEnum.PlayerAuthenticated }, // Published by PlayerSignInController

            // Remote config refresh anytime authenticated changes
            { UGS_EventsEnum.PlayerAuthenticated, UGS_EventsEnum.RemoteConfigFetching }, // First time and anytime player changes.
            //{ UGS_EventsEnum.RemoteConfigFetching, UGS_EventsEnum.RemoteConfigFetched },
            //{ UGS_EventsEnum.RemoteConfigFetched, UGS_EventsEnum.RemoteConfigUpdated },
            //{ UGS_EventsEnum.RemoteConfigFetchFailed, UGS_EventsEnum.RemoteConfigFailed },

            // Sign out loop
            //{ UGS_EventsEnum.PlayerSigningOut, UGS_EventsEnum.PlayerSignedOut },
            { UGS_EventsEnum.PlayerSignedOut, UGS_EventsEnum.PlayerSigningIn }, // Loop back to allow re-sign in
            { UGS_EventsEnum.PlayerSignInFailed, UGS_EventsEnum.PlayerSigningIn }, // Loop back to allow re-sign in

            // --- Post-game UGS UI loop ---
            { UGS_EventsEnum.ScoreUpdating, UGS_EventsEnum.ScoreUpdated },

            //{ UGS_EventsEnum.LeaderboardOpening, UGS_EventsEnum.LeaderboardOpened }, // Published by LeaderboardController
            //{ UGS_EventsEnum.LeaderboardOpened, UGS_EventsEnum.LeaderboardClosing },
            //{ UGS_EventsEnum.LeaderboardClosing, UGS_EventsEnum.LeaderboardClosed },
            { UGS_EventsEnum.LeaderboardClosed, UGS_EventsEnum.AchievementsOpening },

            //{ UGS_EventsEnum.AchievementsOpening, UGS_EventsEnum.AchievementsOpened },
            //{ UGS_EventsEnum.AchievementsOpened, UGS_EventsEnum.AchievementsClosing },
            //{ UGS_EventsEnum.AchievementsClosing, UGS_EventsEnum.AchievementsClosed },
            { UGS_EventsEnum.AchievementsClosed, UGS_EventsEnum.RewardAdWatching },

            { UGS_EventsEnum.RewardAdWatching, UGS_EventsEnum.RewardAdWatched },
            { UGS_EventsEnum.RewardAdWatched, UGS_EventsEnum.PlayerAuthenticating }, // Loop back for continuous checks
        };

        // --------------------------------------------------------------------------------------------
        // UGS -> GameFlow timeline
        // --------------------------------------------------------------------------------------------
        protected Dictionary<UGS_EventsEnum, GameFlowEvents> _autoUGS2GameFlowEvents = new Dictionary<UGS_EventsEnum, GameFlowEvents>()
        {
            { UGS_EventsEnum.PlayerAuthenticated, GameFlowEvents.GameplayReady },
            { UGS_EventsEnum.PlayerSignedOut, GameFlowEvents.GameplayNotReady },

            // OLD:
            //{ UGS_EventsEnum.RemoteConfigUpdated, GameFlowEvents.LoadingScreenHiding },

            // NEW (recommended):
            // Remote config update requests the loading screen to hide; flow auto-fires LoadingScreenHiding.
            { UGS_EventsEnum.RemoteConfigUpdated, GameFlowEvents.LoadingScreenHideRequested },
        };

        // --------------------------------------------------------------------------------------------
        // GameFlow -> UGS timeline
        // --------------------------------------------------------------------------------------------
        protected Dictionary<GameFlowEvents, UGS_EventsEnum> _autoGameFlow2UGSEvents = new Dictionary<GameFlowEvents, UGS_EventsEnum>()
        {
            { GameFlowEvents.GameEnding, UGS_EventsEnum.ScoreUpdating },
            { GameFlowEvents.GameEnded, UGS_EventsEnum.LeaderboardOpening },

            // Alternative: kick leaderboard earlier
            //{ GameFlowEvents.GameScenesUnloaded, UGS_EventsEnum.LeaderboardOpening },
        };

        // --------------------------------------------------------------------------------------------
        // GameFlow -> GameFlow timelines
        // --------------------------------------------------------------------------------------------
        protected Dictionary<GameFlowEvents, GameFlowEvents> _autoGameFlow2GameFlowEvents = new Dictionary<GameFlowEvents, GameFlowEvents>()
        {
            // ========================================================================================
            // BRIDGES (Requested -> ...ing) + AUTO CHAINS (PauseRequested -> Pausing -> Paused)
            // ========================================================================================

            // Loading Screen bridges
            { GameFlowEvents.LoadingScreenShowRequested, GameFlowEvents.LoadingScreenShowing },
            { GameFlowEvents.LoadingScreenHideRequested, GameFlowEvents.LoadingScreenHiding },

            // Main Menu bridges
            { GameFlowEvents.MainMenuShowRequested, GameFlowEvents.MainMenuShowing },
            { GameFlowEvents.MainMenuHideRequested, GameFlowEvents.MainMenuHiding },

            // Session bridges
            { GameFlowEvents.GameStartRequested, GameFlowEvents.GameStarting },
            //{ GameFlowEvents.GameEndRequested, GameFlowEvents.GameEnding }, // Keep data-rich GameEnding published by controller

            // Countdown bridge
            { GameFlowEvents.CountdownStartRequested, GameFlowEvents.CountdownStarting },

            // Scene bridges
            { GameFlowEvents.GameScenesLoadRequested, GameFlowEvents.GameScenesLoading },
            { GameFlowEvents.GameScenesUnloadRequested, GameFlowEvents.GameScenesUnloading },

            // Save / Load bridges (optional)
            //{ GameFlowEvents.SaveLoadRequested, GameFlowEvents.SaveLoading },
            //{ GameFlowEvents.SaveLoading, GameFlowEvents.SaveLoaded },
            //{ GameFlowEvents.SaveRequested, GameFlowEvents.Saving },
            //{ GameFlowEvents.Saving, GameFlowEvents.Saved },

            // Config / Difficulty bridges
            { GameFlowEvents.GameConfigChangeRequested, GameFlowEvents.GameConfigApplying },
            { GameFlowEvents.DifficultyChangeRequested, GameFlowEvents.DifficultyChanging },

            // Quit bridge
            { GameFlowEvents.QuitRequested, GameFlowEvents.Quitting },

            // Pause/Resume auto chains (publish PauseRequested/ResumeRequested from input)
            { GameFlowEvents.PauseRequested, GameFlowEvents.Pausing },
            { GameFlowEvents.Pausing, GameFlowEvents.Paused },

            { GameFlowEvents.ResumeRequested, GameFlowEvents.Resuming },
            { GameFlowEvents.Resuming, GameFlowEvents.Resumed },

            // ========================================================================================
            // NORMAL FULL GAME TIMELINE (documented here, only some entries are auto-fired)
            // ========================================================================================

            // --- Loading Screen ---
            //{ GameFlowEvents.LoadingScreenShowRequested, GameFlowEvents.LoadingScreenShowing },
            //{ GameFlowEvents.LoadingScreenShowing, GameFlowEvents.LoadingScreenShown },      // usually fired by UIPanelController
            //{ GameFlowEvents.LoadingScreenShown, GameFlowEvents.LoadingScreenHideRequested },// optional
            //{ GameFlowEvents.LoadingScreenHideRequested, GameFlowEvents.LoadingScreenHiding },
            //{ GameFlowEvents.LoadingScreenHiding, GameFlowEvents.LoadingScreenHidden },      // usually fired by UIPanelController

            // --- Ready gate -> Main Menu ---
            { GameFlowEvents.GameplayReady, GameFlowEvents.MainMenuShowRequested },
            //{ GameFlowEvents.MainMenuShowRequested, GameFlowEvents.MainMenuShowing },
            //{ GameFlowEvents.MainMenuShowing, GameFlowEvents.MainMenuShown },                // usually fired by MainMenuPanelController

            // --- Play pressed: hide menu then load scenes ---
            //{ GameFlowEvents.GameStartRequested, GameFlowEvents.MainMenuHideRequested },
            //{ GameFlowEvents.MainMenuHideRequested, GameFlowEvents.MainMenuHiding },
            //{ GameFlowEvents.MainMenuHiding, GameFlowEvents.MainMenuHidden },                // usually fired by MainMenuPanelController
            //{ GameFlowEvents.MainMenuHidden, GameFlowEvents.GameScenesLoadRequested },
            //{ GameFlowEvents.GameScenesLoadRequested, GameFlowEvents.GameScenesLoading },
            //{ GameFlowEvents.GameScenesLoading, GameFlowEvents.GameScenesLoaded },           // usually fired by Scene loader
            //{ GameFlowEvents.GameScenesLoaded, GameFlowEvents.GameScenesActivating },        // optional hook
            //{ GameFlowEvents.GameScenesActivating, GameFlowEvents.GameScenesActivated },     // optional hook

            // --- Start -> Countdown -> Gameplay ---
            { GameFlowEvents.GameScenesLoaded, GameFlowEvents.GameStartRequested },
            //{ GameFlowEvents.GameStartRequested, GameFlowEvents.GameStarting },
            { GameFlowEvents.GameStarting, GameFlowEvents.CountdownStartRequested },
            //{ GameFlowEvents.CountdownStartRequested, GameFlowEvents.CountdownStarting },
            //{ GameFlowEvents.CountdownStarting, GameFlowEvents.CountdownStarted },           // usually fired by Countdown controller
            //{ GameFlowEvents.CountdownStarted, GameFlowEvents.CountdownTick },               // repeated
            //{ GameFlowEvents.CountdownTick, GameFlowEvents.CountdownTick },                  // repeated
            //{ GameFlowEvents.CountdownEnding, GameFlowEvents.CountdownEnded },               // usually fired by Countdown controller
            //{ GameFlowEvents.CountdownEnded, GameFlowEvents.GameplayStarting },              // optional if you use GameplayStarting/Started
            //{ GameFlowEvents.GameplayStarting, GameFlowEvents.GameplayStarted },

            // --- End -> Unload -> Return to menu ---
            { GameFlowEvents.GameEnding, GameFlowEvents.GameScenesUnloadRequested },
            //{ GameFlowEvents.GameScenesUnloadRequested, GameFlowEvents.GameScenesUnloading },
            //{ GameFlowEvents.GameScenesUnloading, GameFlowEvents.GameScenesUnloaded },       // usually fired by Scene loader
            //{ GameFlowEvents.GameScenesUnloaded, GameFlowEvents.GameEnded },
            //{ GameFlowEvents.GameEnded, GameFlowEvents.MainMenuShowRequested },              // loop back

            // ========================================================================================
            // ALT PATHS (commented): flip by uncommenting blocks
            // ========================================================================================

            // --- ALT: bypass Main Menu entirely (auto start) ---
            //{ GameFlowEvents.GameplayReady, GameFlowEvents.GameScenesLoadRequested },
            //{ GameFlowEvents.GameScenesLoaded, GameFlowEvents.GameStartRequested },

            // --- ALT: bypass Countdown (instant start) ---
            //{ GameFlowEvents.GameStarting, GameFlowEvents.GameStarted },

            // --- ALT: auto-complete UI transitions if UI controllers are absent ---
            //{ GameFlowEvents.LoadingScreenShowing, GameFlowEvents.LoadingScreenShown },
            //{ GameFlowEvents.LoadingScreenHiding, GameFlowEvents.LoadingScreenHidden },
            //{ GameFlowEvents.MainMenuShowing, GameFlowEvents.MainMenuShown },
            //{ GameFlowEvents.MainMenuHiding, GameFlowEvents.MainMenuHidden },
            //{ GameFlowEvents.GameScenesLoading, GameFlowEvents.GameScenesLoaded },
            //{ GameFlowEvents.GameScenesUnloading, GameFlowEvents.GameScenesUnloaded },

            // --- ALT: pause toggles (one-key test) ---
            //{ GameFlowEvents.Paused, GameFlowEvents.ResumeRequested },
            //{ GameFlowEvents.Resumed, GameFlowEvents.PauseRequested },
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
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                DelayedFire(0, UGS_EventsEnum.UnityServicesInitialized.ToString(), this, null);
            }
        }

        protected void AutoFireUGSEventFromUGSEvent(string eventName, object sender, object data)
        {
            if (_newAutoUGS2UGSEvents.TryGetValue((UGS_EventsEnum)Enum.Parse(typeof(UGS_EventsEnum), eventName), out UGS_EventsEnum autoEvent))
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

    }
}