namespace CrawfisSoftware.TempleRun
{
    public static class GameState
    {
        public static bool IsGameStarted { get; set; } = false;
        public static bool IsGameOver { get; internal set; } = false;
        public static bool IsGamePaused { get; internal set; } = false;
        public static bool IsGameConfigured { get; internal set; } = false;
        public static bool IsUserSignedIn { get; internal set; } = false;
        public static bool IsPlayerAlive { get; internal set; } = false;
        public static bool IsPlayerSignedIn { get; internal set; }
        public static bool IsUnityServicesInitialized { get; internal set; }
        public static bool IsLoading { get; internal set; }
        public static void Reset()
        {
            IsGameStarted = false;
            IsGameOver = false;
            IsGamePaused = false;
            IsGameConfigured = false;
            IsUserSignedIn = false;
            IsPlayerAlive = false;
            IsPlayerSignedIn = false;
            IsUnityServicesInitialized = false;
            IsLoading = false;

        }
    }
}