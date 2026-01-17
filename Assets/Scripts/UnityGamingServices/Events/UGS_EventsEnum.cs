namespace CrawfisSoftware.UGS.Events
{
    public enum UGS_EventsEnum
    {
        None,
        UnityServicesInitialized, UnityServicesInitializationFailed,
        RemoteConfigFetching, RemoteConfigFetched, RemoteConfigFetchFailed, RemoteConfigUpdated, RemoteConfigFailed,
        UserAccountDeleted,
        GameManagerInstantiated,
        PlayerResumedFromExpiredToken, PlayerSigningIn, PlayerSignedIn, PlayerAuthenticating, PlayerAuthenticated, 
        PlayerSigningOut, PlayerSignedOut, PlayerSignInFailed, PlayerSessionExpired,
        CheckForExistingSession, CheckForExistingSessionFailed, CheckForExistingSessionSucceeded,
        ScoreUpdating, ScoreUpdated, ScoreFailedToUpdate,
        LeaderboardOpening, LeaderboardOpened, LeaderboardClosing, LeaderboardClosed,
        AchievementsOpening,AchievementsOpened,AchievementsClosing,AchievementsClosed,
        RewardAdWatching, RewardAdWatched,
        UGS_InitializationStarted, UGS_InitializationCompleted, UGS_InitializationFailed
    }
}