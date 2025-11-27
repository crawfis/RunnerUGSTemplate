namespace CrawfisSoftware.UGS
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
        ScoreUpdating, ScoreUpdated, ScoreFailedToUpdate,
        LeaderboardOpening, LeaderboardOpened, LeaderboardClosing, LeaderboardClosed,
        AchievementsOpening,AchievementsOpened,AchievementsClosing,AchievementsClosed,
        RewardAdWatching, RewardAdWatched,
    }
}