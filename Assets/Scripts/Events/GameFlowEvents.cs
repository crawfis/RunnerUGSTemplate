namespace CrawfisSoftware.Events
{
    public enum GameFlowEvents
    {
        LoadingScreenShowing, LoadingScreenShown, LoadingScreenHidding, LoadingScreenHidden,
        GameplayReady,
        MainMenuShowing, MainMenuShown, MainMenuHidden,
        GameStarting, GameStarted, GameEnding, GameEnded, 
        Pause, Resume, 
        Quitting, Quitted,
        CountdownStarting, CountdownStarted, CountdownTick,
        GameDifficultyChanged,
        GameScenesUnloading, GameScenesUnloaded,
        GameScenesLoading, GameScenesLoaded,
    }
}