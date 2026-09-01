using CrawfisSoftware.Events;

namespace CrawfisSoftware.GameFlow.Events
{
    [EventEnum]
    public enum GameFlowEvents
    {
        // ---------- Loading Screen ----------
        LoadingScreenShowRequested = 0,
        LoadingScreenShowing = 1,
        LoadingScreenShown = 2,
        LoadingScreenHideRequested = 3,
        LoadingScreenHiding = 4,
        LoadingScreenHidden = 5,

        // ---------- Main Menu ----------
        MainMenuShowRequested = 10,
        MainMenuShowing = 11,
        MainMenuShown = 12,
        MainMenuHideRequested = 13,
        MainMenuHiding = 14,
        MainMenuHidden = 15,

        // ---------- Game Session (Menu <-> Run) ----------
        GameStartRequested = 20,
        GameStarting = 21,
        GameStarted = 22,

        GameEndRequested = 23,
        GameEnding = 24,
        GameEnded = 25,

        RestartRequested = 26,
        ReturnToMainMenuRequested = 27,

        // ---------- Scenes (Additive/Async friendly) ----------
        GameScenesUnloadRequested = 30,
        GameScenesUnloading = 31,
        GameScenesUnloaded = 32,
        GameScenesUnloadFailed = 33,

        GameScenesLoadRequested = 34,
        GameScenesLoading = 35,
        GameScenesLoaded = 36,
        GameScenesLoadFailed = 37,

        GameScenesActivating = 38,
        GameScenesActivated = 39,

        // ---------- Gameplay Lifecycle ----------
        GameplayPreparing = 50,   // pooling/spawn/warmup/bind systems
        GameplayReady = 51,       // safe to start countdown / accept start
        GameplayNotReady = 52,    // disable input / block start

        GameplayStarting = 53,    // enabling player control, etc.
        GameplayStarted = 54,
        GameplayEnding = 55,
        GameplayEnded = 56,

        // ---------- Pause Lifecycle ----------
        PauseRequested = 60,
        Pausing = 61,
        Paused = 62,

        ResumeRequested = 63,
        Resuming = 64,
        Resumed = 65,

        // ---------- Config / Difficulty ----------
        GameConfigChangeRequested = 80,
        GameConfigApplying = 81,
        GameConfigApplied = 82,
        GameConfigApplyFailed = 83,
        LevelApplied = 85,                    // data: int (selected level number; gameplay maps it to a track)

        DifficultyChangeRequested = 90,
        DifficultyChanging = 91,
        DifficultyChanged = 92,
        DifficultyChangeFailed = 93,
        /// <summary>
        /// The difficulty table the services layer supplied, bridged from
        /// <c>GameServiceEvents.DifficultySettingsAvailable</c>. Data: <c>IList&lt;DifficultyConfig&gt;</c>.
        /// </summary>
        /// <remarks>
        /// <para><b>Sticky.</b> A difficulty table is current state, not a one-time announcement:
        /// self-describing, and true whenever it is read. It has to be retained, because the only
        /// publisher is the services layer during boot and the only consumer is
        /// <c>TempleRunGameFlowBridge</c>, which lives in <c>Game_Boot_2_Play</c> and does not
        /// exist yet when that publish happens. Announced only once, it would reach nothing,
        /// every time.</para>
        /// </remarks>
        [EventDelivery(EventDelivery.Sticky)]
        DifficultySettingsApplied = 94,

        // ---------- Save / Load (optional but useful hooks) ----------
        SaveLoadRequested = 100,
        SaveLoading = 101,
        SaveLoaded = 102,
        SaveLoadFailed = 103,

        SaveRequested = 110,
        Saving = 111,
        Saved = 112,
        SaveFailed = 113,

        // ---------- Quit ----------
        QuitRequested = 120,
        Quitting = 121,
        QuitCancelled = 122,
        QuitCompleted = 123,

        // ---------- Level Selector ----------
        LevelSelectorShowRequested = 130,
        LevelSelectorShowing = 131,
        LevelSelectorShown = 132,
        LevelSelectorHideRequested = 133,
        LevelSelectorHiding = 134,
        LevelSelectorHidden = 135,
        LevelSelected = 136,              // data: LevelConfig
        LevelUnlocked = 137,              // data: LevelConfig (newly unlocked)
        LevelProgressSaved = 138,

        // ---------- Currency ----------

        /// <summary>
        /// The player's banked lifetime soft-currency balance. Data: long.
        /// </summary>
        /// <remarks>
        /// <para>Translated from the services contract by <c>UGSGameFlowBridge</c>. It is the
        /// stored total, not this run's coin count - that one stays in TempleRun and resets every
        /// run.</para>
        /// <para><b>Sticky</b>, and the first event in this enum to declare a delivery. The
        /// balance arrives once at sign-in, while the HUD that displays it lives in a gameplay
        /// scene loaded per run. Announced only once, it would be long gone before any HUD
        /// existed, and the display would stay blank until a run ended and banked.</para>
        /// </remarks>
        [EventPayload(typeof(long))]
        [EventDelivery(EventDelivery.Sticky)]
        CurrencyBalanceChanged = 140,

        /// <summary>
        /// Coins collected so far in the current run. Data: int, the running TOTAL for the run,
        /// not a delta.
        /// </summary>
        /// <remarks>
        /// <para>Translated by <c>TempleRunGameFlowBridge</c> from the <c>CoinCollected</c> event
        /// in <c>TempleRunEvents</c>, so that UI outside the gameplay domain can show a coin count
        /// without naming a TempleRun event. Written that way round on purpose: the dotted form
        /// reads as a cross-domain reference to /audit-events, which greps textually and cannot
        /// tell a doc comment from code.</para>
        /// <para>Not Sticky, unlike <see cref="CurrencyBalanceChanged"/>. This one only has
        /// meaning during a run, and the HUD that reads it is loaded with the run - there is no
        /// late subscriber to rescue. Retaining it would also outlive the run it describes and
        /// hand the next one a stale count before its first coin.</para>
        /// </remarks>
        [EventPayload(typeof(int))]
        SessionCoinsChanged = 141,
    }
}