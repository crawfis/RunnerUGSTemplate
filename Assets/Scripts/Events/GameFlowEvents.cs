namespace CrawfisSoftware.Events
{
    public enum GameFlowEvents
    {
        GameStarting, GameStarted, GameEnding, GameEnded, 
        Pause, Resume, 
        Quitting, Quitted,
        CountdownStarting, CountdownStarted, CountdownTick,
        GameDifficultyChanged,
        GameScenesUnloading, GameScenesUnloaded,
        GameScenesLoading, GameScenesLoaded,
        GameplayReady,
        LoadingScreenShowing,  LoadingScreenShown, LoadingScreenHidden
    }
}