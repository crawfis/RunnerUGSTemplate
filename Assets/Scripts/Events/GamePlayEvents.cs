namespace CrawfisSoftware.TempleRun
{
    // Todo: Resume and Pause are used for many things, including stopping player movement
    // after failure. Consider splitting into GamePause and PlayerPause or something like that.
    public enum GamePlayEvents
    {
        LeftTurnSucceeded, RightTurnSucceeded,
        PlayerFailing, PlayerFailed, PlayerDied,
        GameStarting, GameStarted, GameEnding, GameEnded, Pause, Resume, Quitting,
        CountdownStarted, CountdownTick,
        SplineSegmentCreated, CurrentSplineChanging, CurrentSplineChanged,
        ActiveTrackChanging, TrackSegmentCreated,
        TeleportStarted, TeleportEnded,
        GameDifficultyChanged,
        LeaderboardDisplayed,
        GameScenesUnloaded,
        GameScenesLoaded,
        LoadingScreenShowing, LoadingScreenShown, HidingLoadingScreen, LoadingScreenHidden,
        LeaderboardClosing,
        Quitted
    };
}