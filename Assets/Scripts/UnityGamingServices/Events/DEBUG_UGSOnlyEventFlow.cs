using CrawfisSoftware.Events;

using System.Collections.Generic;

namespace CrawfisSoftware.UGS.Events
{
    internal class DEBUG_UGSOnlyEventFlow : UGSAndGameEventFlow
    {
        // --------------------------------------------------------------------------------------------
        // UGS-only UGS timeline (same idea you already had; includes full commented hooks)
        // --------------------------------------------------------------------------------------------
        protected Dictionary<UGS_EventsEnum, UGS_EventsEnum> _newAutoUGS2UGSEvents = new Dictionary<UGS_EventsEnum, UGS_EventsEnum>()
        {
            // UnityServicesInitialized is fired by Unity ServicesInitialization component.
            // Since that happens before any of this is set-up, we need to handle it specially in Start()
            { UGS_EventsEnum.UnityServicesInitialized, UGS_EventsEnum.RemoteConfigFetching },

            //{ UGS_EventsEnum.RemoteConfigFetching, UGS_EventsEnum.RemoteConfigFetched },
            //{ UGS_EventsEnum.RemoteConfigFetched, UGS_EventsEnum.RemoteConfigUpdated },
            { UGS_EventsEnum.RemoteConfigUpdated, UGS_EventsEnum.CheckForExistingSession },

            //{ UGS_EventsEnum.CheckForExistingSession, UGS_EventsEnum.CheckForExistingSessionSucceeded },
            { UGS_EventsEnum.CheckForExistingSessionSucceeded, UGS_EventsEnum.PlayerAuthenticating },
            { UGS_EventsEnum.CheckForExistingSessionFailed, UGS_EventsEnum.PlayerSigningIn },

            //{ UGS_EventsEnum.PlayerSigningIn, UGS_EventsEnum.PlayerSignedIn },           // PlayerSignedIn fired by PlayerSignInController
            { UGS_EventsEnum.PlayerSignedIn, UGS_EventsEnum.PlayerAuthenticating },
            //{ UGS_EventsEnum.PlayerAuthenticating, UGS_EventsEnum.PlayerAuthenticated }, // PlayerAuthenticated fired by PlayerSignInController

            //{ UGS_EventsEnum.PlayerSigningOut, UGS_EventsEnum.PlayerSignedOut },
            { UGS_EventsEnum.PlayerSignedOut, UGS_EventsEnum.PlayerSigningIn }, // Loop back to allow re-sign in
            { UGS_EventsEnum.PlayerSignInFailed, UGS_EventsEnum.PlayerSigningIn },

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
        // UGS-only GameFlow timeline (bypasses gameplay/UI to test the UGS loop)
        // --------------------------------------------------------------------------------------------
        protected Dictionary<GameFlowEvents, GameFlowEvents> _newAutoGameFlow2GameFlowEvents = new Dictionary<GameFlowEvents, GameFlowEvents>()
        {
            // ========================================================================================
            // DOCUMENTED FULL TIMELINE (commented)
            // ========================================================================================

            // --- Loading screen (usually UI-owned) ---
            //{ GameFlowEvents.LoadingScreenShowRequested, GameFlowEvents.LoadingScreenShowing },
            //{ GameFlowEvents.LoadingScreenShowing, GameFlowEvents.LoadingScreenShown },
            //{ GameFlowEvents.LoadingScreenShown, GameFlowEvents.LoadingScreenHideRequested },
            //{ GameFlowEvents.LoadingScreenHideRequested, GameFlowEvents.LoadingScreenHiding },
            //{ GameFlowEvents.LoadingScreenHiding, GameFlowEvents.LoadingScreenHidden },
            //{ GameFlowEvents.LoadingScreenHidden, GameFlowEvents.GameplayReady },

            // --- Ready -> menu (usually UI-owned) ---
            { GameFlowEvents.GameplayReady, GameFlowEvents.MainMenuShowRequested },
            { GameFlowEvents.MainMenuShowRequested, GameFlowEvents.MainMenuShowing },
            //{ GameFlowEvents.MainMenuShowing, GameFlowEvents.MainMenuShown },

            // ========================================================================================
            // ACTIVE UGS-ONLY TEST PATH (bypass menu + countdown + gameplay)
            // ========================================================================================

            // Bypass menu: pretend scenes are loading then instantly "loaded"
            { GameFlowEvents.GameScenesLoadRequested, GameFlowEvents.GameScenesLoading },
            { GameFlowEvents.GameScenesLoading, GameFlowEvents.GameScenesLoaded },

            // Start request path
            { GameFlowEvents.GameScenesLoaded, GameFlowEvents.GameStartRequested },
            { GameFlowEvents.GameStartRequested, GameFlowEvents.GameStarting },

            // Bypass countdown by jumping to GameStarted (or keep countdown if you want)
            //{ GameFlowEvents.GameStarting, GameFlowEvents.CountdownStartRequested },
            //{ GameFlowEvents.CountdownStartRequested, GameFlowEvents.CountdownStarting },
            //{ GameFlowEvents.CountdownEnding, GameFlowEvents.CountdownEnded },
            //{ GameFlowEvents.GameStarting, GameFlowEvents.GameStarted },

            // Immediately end (UGS-only testing)
            //{ GameFlowEvents.GameStarted, GameFlowEvents.GameEndRequested },
            //{ GameFlowEvents.GameEndRequested, GameFlowEvents.GameEnding },

            // Unload
            { GameFlowEvents.GameEnding, GameFlowEvents.GameScenesUnloadRequested },
            { GameFlowEvents.GameScenesUnloadRequested, GameFlowEvents.GameScenesUnloading },
            //{ GameFlowEvents.GameScenesUnloading, GameFlowEvents.GameScenesUnloaded },
            //{ GameFlowEvents.GameScenesUnloaded, GameFlowEvents.GameEnded },

            // ========================================================================================
            // Pause/Resume documented
            // ========================================================================================
            { GameFlowEvents.PauseRequested, GameFlowEvents.Pausing },
            { GameFlowEvents.Pausing, GameFlowEvents.Paused },
            { GameFlowEvents.ResumeRequested, GameFlowEvents.Resuming },
            { GameFlowEvents.Resuming, GameFlowEvents.Resumed },
        };

        protected override void Awake()
        {
            //base._autoUGS2UGSEvents = _newAutoUGS2UGSEvents;
            base._autoGameFlow2GameFlowEvents = _newAutoGameFlow2GameFlowEvents;

            base.Awake();
        }
    }
}