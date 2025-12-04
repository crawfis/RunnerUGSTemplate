namespace CrawfisSoftware.Events
{
    public enum GameFlowEvents
    {
        // Todo: Rearrange these in logical order, but that breaks the Inspector dropdown order.
        // Todo: Move GameDifficultyChanged and GameDifficultyChanging to TempleRun events and replace with more generic GameConfiguring, GameConfigured?
        LoadingScreenShowing, LoadingScreenShown, LoadingScreenHidding, LoadingScreenHidden,
        GameplayReady,
        MainMenuShowing, MainMenuShown, MainMenuHidden,
        GameStarting, GameStarted, GameEnding, GameEnded, 
        Pause, Resume, 
        Quitting, Quitted,
        CountdownStarting, CountdownStarted, CountdownTick, CountdownEnding, CountdownEnded,
        GameDifficultyChanged,
        GameScenesUnloading, GameScenesUnloaded,
        GameScenesLoading, GameScenesLoaded,
        GameplayNotReady, GameDifficultyChanging,
        GameDifficultyChangeFailed
    }
}