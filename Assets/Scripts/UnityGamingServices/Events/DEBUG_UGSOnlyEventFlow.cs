using CrawfisSoftware.Events;

using System.Collections.Generic;

namespace CrawfisSoftware.UGS.Events
{
    internal class DEBUG_UGSOnlyEventFlow : UGSAndGameEventFlow
    {
        protected Dictionary<UGS_EventsEnum, UGS_EventsEnum> _newAutoUGS2UGSEvents = new Dictionary<UGS_EventsEnum, UGS_EventsEnum>()
        {
            // UnityServicesInitialized is fired by Unity's ServicesInitialization component in the InitializeServices GameObject in the UGS_Boot_0_Initialization scene.
            // Since that happens before any of this is set-up, we need to handle it specially in Start()
            { UGS_EventsEnum.UnityServicesInitialized, UGS_EventsEnum.RemoteConfigFetching },
            //{ UGS_EventsEnum.RemoteConfigFetching, UGS_EventsEnum.RemoteConfigFetched },
            //{ UGS_EventsEnum.RemoteConfigFetched, UGS_EventsEnum.RemoteConfigUpdated },
            { UGS_EventsEnum.RemoteConfigUpdated, UGS_EventsEnum.CheckForExistingSession },
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
        //protected Dictionary<UGS_EventsEnum, GameFlowEvents> _autoUGS2GameFlowEvents = new Dictionary<UGS_EventsEnum, GameFlowEvents>()
        //{
        //    { UGS_EventsEnum.PlayerAuthenticated, GameFlowEvents.GameplayReady },
        //    { UGS_EventsEnum.PlayerSignedOut, GameFlowEvents.GameplayNotReady },
        //    { UGS_EventsEnum.RemoteConfigUpdated, GameFlowEvents.LoadingScreenHidding },
        //};

        //protected Dictionary<GameFlowEvents, UGS_EventsEnum> _autoGameFlow2UGSEvents = new Dictionary<GameFlowEvents, UGS_EventsEnum>()
        //{
        //    { GameFlowEvents.GameScenesUnloaded, UGS_EventsEnum.LeaderboardOpening },
        //};

        protected Dictionary<GameFlowEvents, GameFlowEvents> _newAutoGameFlow2GameFlowEvents = new Dictionary<GameFlowEvents, GameFlowEvents>()
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

            {GameFlowEvents.GameScenesLoading, GameFlowEvents.GameScenesLoaded }, // For UGS-only testing, bypass Main Menu

            { GameFlowEvents.GameScenesLoaded, GameFlowEvents.GameStarting },
            //{ GameFlowEvents.GameStarting, GameFlowEvents.CountdownStarting },
            //{ GameFlowEvents.CountdownStarting, GameFlowEvents.CountdownStarted },
            //{ GameFlowEvents.CountdownStarted, GameFlowEvents.CountdownTick },
            //{ GameFlowEvents.CountdownEnding, GameFlowEvents.CountdownEnded },
            { GameFlowEvents.CountdownEnded, GameFlowEvents.GameStarted },

            //{ GameFlowEvents.GameStarted, GameFlowEvents.GameEnding }, // Bypass gameplay for testing UGS flow

            { GameFlowEvents.GameEnding, GameFlowEvents.GameScenesUnloading },
            //{ GameFlowEvents.GameScenesUnloading, GameFlowEvents.GameScenesUnloaded },
            //{ GameFlowEvents.GameScenesUnloaded, GameFlowEvents.GameEnded },
            //{ GameFlowEvents.GameEnded, GameFlowEvents.GameplayReady  }, //=> Show Menu
            //{ GameFlowEvents.Pause, GameFlowEvents.Resume  },
            //{ GameFlowEvents.Quitting, GameFlowEvents.Quitted }, // Quitted is fired by th Quitting GameObject in the 0_BootStrap scene
        };
        protected override void Awake()
        {
            base._autoUGS2UGSEvents = _newAutoUGS2UGSEvents;
            base._autoGameFlow2GameFlowEvents = _newAutoGameFlow2GameFlowEvents;
            base.Awake();
        }
    }
}